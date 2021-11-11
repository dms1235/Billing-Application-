using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domains.Interfaces.Auth;
using Domains.Interfaces.Security;
using Entity.Common.Entities;
using Entity.Common.WebExtention;
using Entity.Entities.Auth;
using Entity.Entities.DTO;
using Entity.Entities.Security;
using Entity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BillingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IConfiguration config;
        private readonly IAuthDomain authDomain;
        private readonly IUserDomain userDomain;
        private readonly IWebExtention webExtention;
        public AccountController(IConfiguration _config,IAuthDomain _authDomain, IUserDomain _userDomain, IWebExtention _webExtention)
        {
            config = _config;
            authDomain = _authDomain;
            userDomain = _userDomain;
            webExtention = _webExtention;
        }
        [AllowAnonymous]
        [HttpGet("version")]
        public IActionResult GetAPIVersion()
        {
            ResultEntity<object> result = new ResultEntity<object>();
            string APIVersion = config["AppSettings:APIVersion"].ToString();
            if (string.IsNullOrEmpty(APIVersion))
            {
                APIVersion = "1.0";
            }
            result.Entity = APIVersion;
            return Ok(result);
        }

        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost("Login")]
        public async Task<TokenDataResult> Login([FromForm] TokenRequestEntity entity)
        {
            TokenDataResult responseData = new TokenDataResult();
            ResultEntity<UserEntity> resEntity = await authDomain.AuthenticateUser(entity);
            if (resEntity.Status != (int)ResponseStatus.Success)
            {
                responseData.Add("Status", resEntity.Status);
                responseData.Add("MessageEnglish", resEntity.MessageEnglish);
            }
            else
            {
                var token =  authDomain.CreateToken(resEntity.Entity);
                responseData = new TokenDataResult(token.Token, token.Validity);
                responseData.Add("Status", resEntity.Status);
                responseData.Add("Data", resEntity.Entity);
            }
            return responseData;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserEntity entity, [FromHeader] string Culture = "en")
        {
            IActionResult actionResult = Unauthorized();
            ResultEntity<UserEntity> resultEntity = new ResultEntity<UserEntity>();
            webExtention.SetCulture(Culture);
            var user = await userDomain.AddUser(entity);
            if (user.Status == (int)ResponseStatus.Success)
            {
                resultEntity.Entity = user.Entity;
                resultEntity.MessageEnglish = user.MessageEnglish;
                resultEntity.Status = user.Status;
            }
            else
            {
                resultEntity.Entity = null;
                resultEntity.MessageEnglish = user.MessageEnglish;
                resultEntity.Status = user.Status;
            }
            actionResult = Ok(new
            {
                Result = resultEntity
            });
            return actionResult;
        }
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost("reset/password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDetails entity, [FromHeader] String Culture = "en")
        {
            IActionResult response = Unauthorized();
            webExtention.SetCulture(Culture);
            var result = await userDomain.ResetPassword(entity);
            response = Ok(new
            {
                Result = result
            });
            return response;
        }
        [HttpPost("change/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO entity, [FromHeader] String Culture = "en")
        {
            IActionResult response = Unauthorized();
            webExtention.SetCulture(Culture);
            var result = await userDomain.ChangePassword(entity);
            response = Ok(new
            {
                Result = result
            });
            return response;
        }
    }

}
