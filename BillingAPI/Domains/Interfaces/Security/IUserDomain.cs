using Entity.Common.Entities;
using Entity.Entities.DTO;
using Entity.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Interfaces.Security
{
    public interface IUserDomain
    {
        Task<ResultEntity<UserEntity>> FindOneByQuery(Expression<Func<UserEntity, bool>> expression);
        Task<ResultEntity<UserEntity>> AddUser(UserEntity entity);
        Task<ResultEntity<UserEntity>> Delete(UserEntity entity);
        Task<ResultEntity<UserEntity>> Edit(UserEntity entity);
        Task<ResultList<UserEntity>> GetAllUsers(GridParameters gridParam);
        Task<ResultEntity<UserEntity>> GetUserByID(Guid userId);
        Task<ResultEntity<bool>> ResetPassword(ResetPasswordDetails PasswordDetails);
        Task<ResultEntity<bool>> ChangePassword(ChangePasswordDTO changePasswordDetails);
    }
}
