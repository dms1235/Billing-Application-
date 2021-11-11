using Entity.Common.Entities;
using Entity.Entities.Auth;
using Entity.Entities.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Interfaces.Auth
{
    public interface IAuthDomain
    {
        Task<ResultEntity<UserEntity>> AuthenticateUser(TokenRequestEntity entity);
        AppTokenResultEntity CreateToken(UserEntity userInfo);
    }
}
