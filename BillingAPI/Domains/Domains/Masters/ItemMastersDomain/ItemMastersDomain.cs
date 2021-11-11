using DataAccess.Interface;
using Domains.Interfaces.Masters;
using Entity.Base;
using Entity.Common.Attributes;
using Entity.Common.Entities;
using Entity.Common.Helper;
using Entity.Common.Logs;
using Entity.Entities.Masters;
using Entity.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Domains.Masters.ItemMastersDomain
{
    public class ItemMastersDomain : IItemMaster
    {
        private readonly IItemMastersRepository itemMasterRepository;
        public ItemMastersDomain(IItemMastersRepository _itemMasterRepository)
        {
            itemMasterRepository = _itemMasterRepository;
        }
        public async Task<ResultEntity<ItemMasters>> Add(ItemMasters entity)
        {
            return await itemMasterRepository.InsertItemMasters(entity);
        }
        public async Task<ResultEntity<ItemMasters>> Update(ItemMasters entity)
        {
            return await itemMasterRepository.UpdateItemMasters(entity);
        }
        public async Task<ResultEntity<bool>> Delete(Guid ItemID)
        {
            return await itemMasterRepository.DeleteItemMasters(ItemID);
        }
        public async Task<ResultList<ItemMasters>> GetAllItems()
        {
            return await itemMasterRepository.GetAllItemMasters();
        }
        public async Task<ResultEntity<ItemMasters>> GetItemByID(Guid ItemID)
        {
            return await itemMasterRepository.GetItemByID(ItemID);
        }
        public async Task<ResultList<ItemMasters>> SearchItem(GridParameters gridParameters)
        {
            return await itemMasterRepository.SearchItemMaster(gridParameters, Dynamicsearchfilterforentity<ItemMasters>(gridParameters.SearchText, gridParameters.Culture));
        }
        private  Expression<Func<TEntity, bool>> Dynamicsearchfilterforentity<TEntity>(string value, string culture = "en")
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
        private Expression GetExpression(Expression propertyExpression, PropertyInfo Property, string ValuetoSearch)
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
        public ResultEntity<Dictionary<string, object>> GetFormattedMasterDataByType(Type[] DataTypes, string Culture)
        {
            ResultEntity<Dictionary<string, object>> response = new ResultEntity<Dictionary<string, object>>();
            var TableData = new Dictionary<string, object>();
            try
            {
                foreach (Type type in DataTypes.Distinct())
                {
                    var table = type.GetCustomAttribute<TableAttribute>();
                    if (table != null)
                    {
                        var tableName = table.Name;
                        Type ex = typeof(IItemMastersRepository);
                        string MethodName = type.BaseType == typeof(BaseLookupEntity) ? "GetFormattedMasterData" : "GetFormattedMasterEntityData";
                        MethodInfo mi = ex.GetMethod(MethodName);
                        MethodInfo miConstructed = mi.MakeGenericMethod(type);
                        object[] args = { Culture };
                        var resObj = miConstructed.Invoke(itemMasterRepository, args);
                        if (resObj != null)
                            TableData.Add(tableName, resObj);
                    }
                }
                response.Status = (int)ResponseStatus.Success;
                response.Entity = TableData;
            }
            catch (Exception ex)
            {
                response.Status = (int)ResponseStatus.Error;
                response.MessageEnglish = ex.Message;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return response;
        }

    }
}
