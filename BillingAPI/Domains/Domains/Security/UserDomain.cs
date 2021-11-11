using DataAccess.Interface;
using Domains.Interfaces.Security;
using Entity.Common.Helper;
using Entity.Common.Constants;
using Entity.Common.Entities;
using Entity.Common.Logs;
using Entity.Common.WebExtention;
using Entity.Entities.Security;
using Entity.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Entity.Common.Attributes;
using System.Reflection;
using Entity.Base;
using Domains.Validation.Security;
using Entity.Entities.DTO;

namespace Domains.Domains.Security
{
    public class UserDomain : IUserDomain
    {
        private readonly IBaseRepository<UserEntity> repository;
        private readonly IConfiguration config;
        private readonly IWebExtention WebWorker;

        public UserDomain( 
        IConfiguration _config, IWebExtention _WebWorker, IBaseRepository<UserEntity> _repository)
        {
            config = _config;
            WebWorker = _WebWorker;
            repository = _repository;
        }
        public async Task<ResultEntity<UserEntity>> FindOneByQuery(Expression<Func<UserEntity, bool>> expression)
        {
            return await repository.FindOneByQuery(expression);
        }
        public async Task<ResultEntity<UserEntity>> AddUser(UserEntity entity)
        {
            ResultEntity<UserEntity> result = new ResultEntity<UserEntity>();
            try
            {
                UserValidation validator = new UserValidation(repository);
                var validationResult = await validator.ValidateAsync(entity);
                if (validationResult.IsValid)
                {
                    var checkExistUserResult = await repository.FindOneByQuery(a => a.Username == entity.Username);
                    if (checkExistUserResult.Status == (int)ResponseStatus.Success)
                    {
                        result.Status = (int)ResponseStatus.ValidationError; //Error username already exist
                        result.Entity = null;
                        result.MessageEnglish =   CommonDomainMessage.ErrUsernameExist;
                    }
                    else
                    {
                        entity.Password = !string.IsNullOrWhiteSpace(entity.Password) ? SecurityHelper.HashMD5(entity.Password) : string.Empty;
                        entity.CreatedOn = DateTime.Now;
                        entity.UpdatedOn = null;
                        entity.CreatedBy = WebWorker.UserID;
                        result = await repository.Insert(entity);
 
                    }
                }
                else
                {
                    result.Status = (int)ResponseStatus.ValidationError;//Error Validation issue
                    result.MessageEnglish = validationResult.ToString(",");
                }
            }
            catch (Exception ex)
            {
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.Message);
            }

            return result;
        }

        public async Task<ResultEntity<UserEntity>> Delete(UserEntity entity)
        {
            return await repository.Delete(entity);
        }

        public async Task<ResultEntity<UserEntity>> Edit(UserEntity entity)
        {
            ResultEntity<UserEntity> result = new ResultEntity<UserEntity>();
            try
            {
                UpdateUserValidation validator = new UpdateUserValidation(repository);
                var validationResult = await validator.ValidateAsync(entity);
                if (validationResult.IsValid)
                {
                    var checkExistUserResult = await repository.FindOneByQuery(a => a.Username == entity.Username && a.ID != entity.ID);
                    if (checkExistUserResult.Status == (int)ResponseStatus.Success)
                    {
                        result.Status = (int)ResponseStatus.ValidationError;
                        result.Entity = null;
                        result.MessageEnglish =   CommonDomainMessage.ErrUsernameExist;
                    }
                    else
                    {
                        var getItemByIDResult = await repository.FindOneByQuery(a => a.ID == entity.ID);
                        if (getItemByIDResult.Status == (int)ResponseStatus.Success)
                        {
                            entity.CreatedOn = getItemByIDResult.Entity.CreatedOn;
                            entity.CreatedBy = getItemByIDResult.Entity.CreatedBy;
                            entity.Password = getItemByIDResult.Entity.Password;
                            entity.UpdatedOn = DateTime.Now;
                            entity.UpdatedBy = WebWorker.UserID;
                            result = await repository.Update(entity);
                        }
                    }
                }
                else
                {
                    result.Status = (int)ResponseStatus.ValidationError;
                    result.Entity = null;
                    result.MessageEnglish = validationResult.ToString(",");
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.Message);
            }

            return result;
        }

        public async Task<ResultList<UserEntity>> GetAllUsers(GridParameters gridParam)
        {
            ResultList<UserEntity> result = new ResultList<UserEntity>();
            try
            {
                var userData = await repository.FindAll(gridParam, Dynamicsearchfilter<UserEntity>(gridParam.SearchText, gridParam.Culture));
                List<UserEntity> userList = new List<UserEntity>();
                if (userData != null && userData.Status == (int)ResponseStatus.Success)
                {
                    result.List = userData.List;
                    result.TotalPages = userData.TotalPages;
                    result.ItemCount = userData.ItemCount;
                    result.Status = (int)ResponseStatus.Success;
                }
               
            }
            catch (Exception ex)
            {
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.Message);
            }
            return result;
        }

        public async Task<ResultEntity<UserEntity>> GetUserByID(Guid userId)
        {
            ResultEntity<UserEntity> result = new ResultEntity<UserEntity>();
            try
            {
                var user = await repository.FindOneByQuery(x => x.ID == userId);
                if (user.Status == (int)ResponseStatus.Success)
                {
                    result.Entity = user.Entity;
                }
            }
            catch (Exception ex)
            {
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.Message);
            }
            return result;
        }
        private Expression<Func<TEntity, bool>> Dynamicsearchfilter<TEntity>(string value, string culture = "en")
        {
            var p = Expression.Parameter(typeof(TEntity), "p");
            Expression<Func<TEntity, bool>> ex = null;
            Expression expression = null;
            Expression body = null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (var Property in typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute<GlobleSearch>() != null))
                {
                    try
                    {
                        var isParentPropert = Property.GetCustomAttribute<GlobleSearch>().IsParent;
                        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        var valueToFind = Expression.Constant(value);
                        ParentTableMapping tableMapping = Property.GetCustomAttributes<ParentTableMapping>().FirstOrDefault();
                        if (tableMapping != null)
                        {
                            PropertyInfo ParentPro = typeof(TEntity).GetProperties().Where(p => p.PropertyType == tableMapping.SourceType).FirstOrDefault();
                            if (ParentPro != null)
                            {
                                string name = "Name";
                                string ProName = $"{ParentPro.Name}.{name}";
                                var propertyToSearch = ProName.Split('.').Aggregate<string, Expression>(p, Expression.PropertyOrField);
                                expression = Expression.Call(propertyToSearch, containsMethod, valueToFind);
                                body = body == null ? expression : Expression.Or(body, expression);
                            }
                        }
                        else
                        {
                            var propertyToSearch = isParentPropert ?
                          GetLastPropertyExpression<TEntity>(p, Property.Name, "Name")
                           : Expression.Property(p, Property.Name);
                            if (propertyToSearch.Type == typeof(string))
                            {
                                containsMethod = Property.PropertyType.GetMethod(nameof(string.Contains), new[] { typeof(string) });
                                expression = Expression.Call(propertyToSearch, containsMethod, valueToFind);
                                body = body == null ? expression : Expression.Or(body, expression);
                            }
                            else
                            {
                                expression = GetExpression(propertyToSearch, Property, value);
                                if (expression != null)
                                {
                                    body = body == null ? expression : Expression.Or(body, expression);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                ex = Expression.Lambda<Func<TEntity, bool>>(body, p);
            }
            return ex;
        }
        private Expression GetLastPropertyExpression<TSource>(ParameterExpression pe, params string[] properties)
        {
            Expression lastMember = pe;
            for (int i = 0; i < properties.Length; i++)
            {
                MemberExpression member = Expression.Property(lastMember, properties[i]);
                lastMember = member;
            }
            return lastMember;
        }
        public Expression GetExpression(Expression propertyExpression, PropertyInfo Property, string ValuetoSearch)
        {
            ConstantExpression searchExpression = null;
            MethodInfo containsMethod = null;
            Expression body = null;
            switch (Type.GetTypeCode(Property.PropertyType))
            {
                case TypeCode.Object:
                    if (Property.PropertyType == typeof(Guid))
                    {
                        searchExpression = Expression.Constant(Guid.Parse(ValuetoSearch));
                        containsMethod = typeof(Guid).GetMethod("Equals", new[] { typeof(Guid) });
                    }
                    else if (Property.PropertyType == typeof(Guid?))
                    {
                        searchExpression = Expression.Constant(Guid.Parse(ValuetoSearch));
                        propertyExpression = Expression.Property(propertyExpression, "Value");
                        containsMethod = typeof(Guid).GetMethod("Equals", new[] { typeof(Guid) });
                    }
                    else
                    {
                        searchExpression = Expression.Constant(ValuetoSearch.ToString());
                        containsMethod = typeof(object).GetMethod("Equals", new[] { typeof(object) });
                    }

                    body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    break;
                case TypeCode.String:
                    searchExpression = Expression.Constant(ValuetoSearch.ToString());
                    containsMethod = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                    body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    break;
                case TypeCode.Byte:
                    byte _byte;
                    if (byte.TryParse(ValuetoSearch, out _byte))
                    {
                        searchExpression = Expression.Constant(_byte);
                        containsMethod = typeof(byte).GetMethod("Equals", new[] { typeof(byte) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Int16:
                    short _int16;
                    if (short.TryParse(ValuetoSearch, out _int16))
                    {
                        searchExpression = Expression.Constant(_int16);
                        containsMethod = typeof(short).GetMethod("Equals", new[] { typeof(short) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Int32:
                    int _int;
                    if (int.TryParse(ValuetoSearch, out _int))
                    {
                        searchExpression = Expression.Constant(_int);
                        containsMethod = typeof(int).GetMethod("Equals", new[] { typeof(int) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Int64:
                    long _int64;
                    if (long.TryParse(ValuetoSearch, out _int64))
                    {
                        searchExpression = Expression.Constant(_int64);
                        containsMethod = typeof(long).GetMethod("Equals", new[] { typeof(long) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Single:
                    float _Single;
                    if (float.TryParse(ValuetoSearch, out _Single))
                    {
                        searchExpression = Expression.Constant(_Single);
                        containsMethod = typeof(float).GetMethod("Equals", new[] { typeof(float) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Double:
                    double _Double;
                    if (double.TryParse(ValuetoSearch, out _Double))
                    {
                        searchExpression = Expression.Constant(_Double);
                        containsMethod = typeof(double).GetMethod("Equals", new[] { typeof(double) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                case TypeCode.Boolean:
                    bool _Boolean;
                    if (bool.TryParse(ValuetoSearch, out _Boolean))
                    {
                        searchExpression = Expression.Constant(_Boolean);
                        containsMethod = typeof(bool).GetMethod("Equals", new[] { typeof(bool) });
                        body = Expression.Call(propertyExpression, containsMethod, searchExpression);
                    }
                    break;
                default:
                    body = null;
                    break;
            }
            return body;
        }
        public async Task<ResultEntity<bool>> ResetPassword(ResetPasswordDetails PasswordDetails)
        {
            ResultEntity<bool> resultEntity = new ResultEntity<bool>();
            ResetPasswordValidator validator = new ResetPasswordValidator(repository);
            var validationResult = await validator.ValidateAsync(PasswordDetails);
            if (validationResult.IsValid)
            {
                var user = await repository.FindOneByQuery(c => c.Email == PasswordDetails.Email && c.IsActive);
                if (user != null)
                {

                    user.Entity.Password = SecurityHelper.HashMD5(PasswordDetails.NewPassword);
                    var updatedUser = (await repository.Update(user.Entity));
                    resultEntity.Entity = updatedUser.Status == (int)ResponseStatus.Success ? true : false;
                    resultEntity.Status = updatedUser.Status;
                    resultEntity.MessageEnglish = updatedUser.MessageEnglish;
                }
                else
                {
                    resultEntity.Entity = false;
                    resultEntity.Status = (int)ResponseStatus.ValidationError;
                    resultEntity.MessageEnglish = CommonDomainMessage.ErrUserNotFound;
                }
            }
            else
            {
                resultEntity.Entity = false;
                resultEntity.Status = (int)ResponseStatus.ValidationError;
                resultEntity.MessageEnglish = string.Join(",", validationResult.Errors.Select(p => p.ErrorMessage).ToArray());
            }
            return resultEntity;
        }
        public async Task<ResultEntity<bool>> ChangePassword(ChangePasswordDTO changePasswordDetails)
        {
            ResultEntity<bool> resultEntity = new ResultEntity<bool>();

            ChangePasswordValidator validator = new ChangePasswordValidator();
            var validationResult = await validator.ValidateAsync(changePasswordDetails);
            if (validationResult.IsValid)
            {
                var user = await repository.FindOneByQuery(c => c.ID == changePasswordDetails.UserID && c.IsActive);
                if (user != null && user.Status == (int)ResponseStatus.Success)
                {
                    if (user.Entity.Password != SecurityHelper.HashMD5(changePasswordDetails.OldPassword))
                    {
                        resultEntity.Entity = false;
                        resultEntity.Status = (int)ResponseStatus.ValidationError;
                        resultEntity.MessageEnglish =  CommonDomainMessage.ErrPasswordNotMatch;
                        return resultEntity;
                    }
                    UserEntity userEntity = user.Entity;
                    userEntity.Password = SecurityHelper.HashMD5(changePasswordDetails.NewPassword);
                    var updatedUser = (await repository.Update(userEntity));

                    resultEntity.Entity = updatedUser.Status == (int)ResponseStatus.Success ? true : false;
                    resultEntity.Status = updatedUser.Status;
                    resultEntity.MessageEnglish = updatedUser.MessageEnglish;
                    
                }
                else
                {
                    resultEntity.Entity = false;
                    resultEntity.Status = (int)ResponseStatus.ValidationError;
                    resultEntity.MessageEnglish =  CommonDomainMessage.ErrUserNotFound;
                }
            }
            else
            {
                resultEntity.Entity = false;
                resultEntity.Status = (int)ResponseStatus.ValidationError;
                resultEntity.MessageEnglish = string.Join(",", validationResult.Errors.Select(p => p.ErrorMessage).ToArray());
            }
            return resultEntity;
        }
    }
}
