using Domains.Interfaces.Auth;
using Domains.Interfaces.Security;
using Entity.Common.Constants;
using Entity.Common.Entities;
using Entity.Common.Helper;
using Entity.Entities.Auth;
using Entity.Entities.Security;
using Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Domains.Auth
{
    public class AuthDomain : IAuthDomain
    {
        private readonly IConfiguration config;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserDomain userDomain;
        public AuthDomain(IConfiguration _config, IHttpContextAccessor _httpContextAccessor, IUserDomain _userDomain)
        {
            config = _config;
            httpContextAccessor = _httpContextAccessor; 
            userDomain = _userDomain;
        }
        public async Task<ResultEntity<UserEntity>> AuthenticateUser(TokenRequestEntity entity)
        {
            string passWord = entity.password;
            ResultEntity<UserEntity> result = new ResultEntity<UserEntity>();
           try
            {
                ResultEntity<UserEntity> user = await userDomain.FindOneByQuery(c => c.Username == entity.username && c.Password == SecurityHelper.HashMD5(entity.password) && c.IsActive == true);
                if (user != null && user?.Entity != null)
                {
                    if (user.Entity.IsActive)
                    {
                        result.Status = (int)ResponseStatus.Success;
                        result.Entity = user.Entity;
                        return result;
                    }
                    else
                    {
                        result.Status = (int)ResponseStatus.Error;
                        result.Entity = null;
                        result.MessageEnglish = CommonDomainMessage.ErrUserNotActive;
                        return result;
                    }
                }
                else
                {
                    result.Status = (int)ResponseStatus.Error;
                    result.Entity = null;
                    return result;
                }
            }
           catch(Exception ex)
            {
                result.Status = (int)ResponseStatus.Error;
                result.Entity = null;
                result.MessageEnglish = CommonDomainMessage.ErrWrongUsernameOrPassword;
                result.DetailsEnglish = ex.Message;
                return result;
            }
        }

        public AppTokenResultEntity CreateToken(UserEntity userInfo)
        {
            int tokenExpiryinMinutes = Convert.ToInt32(config["Jwt:tokenExpiryinMinutes"]);
            AppTokenResultEntity result = new AppTokenResultEntity();
            var timestamp = DateTime.Now.AddSeconds(-1).ToFileTime();
            var expTimestamp = DateTime.Now.AddMinutes(tokenExpiryinMinutes);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            string ipAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userInfo.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, timestamp.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, expTimestamp.ToFileTime().ToString()));
            claims.Add(new Claim("uid", userInfo.ID.ToString()));
            claims.Add(new Claim("uip", ipAddress));
            var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(tokenExpiryinMinutes),
            signingCredentials: credentials
            );
            result.Token = new JwtSecurityTokenHandler().WriteToken(token);
            result.Validity = tokenExpiryinMinutes;
            return result;
        }

    }
}
