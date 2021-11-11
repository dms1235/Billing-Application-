using DataAccess.Common;
using DataAccess.Interface;
using Entity.Base;
using Entity.Common.Constants;
using Entity.Common.Entities;
using Entity.Common.Helper;
using Entity.Common.Logs;
using Entity.Common.WebExtention;
using Entity.Entities.DTO;
using Entity.Entities.Masters;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ItemMastersRepository: IItemMastersRepository
    {
        ApplicationDBContext applicationDBContext;
        private readonly IWebExtention webExtention;
        public ItemMastersRepository(ApplicationDBContext _dbContext, IWebExtention _webExtention)
        {
            applicationDBContext = _dbContext;
            webExtention = _webExtention;
        }

        public async Task<ResultEntity<ItemMasters>> InsertItemMasters(ItemMasters entity)
        {
            ResultEntity<ItemMasters> result = new ResultEntity<ItemMasters>();
            try
            {
                await applicationDBContext.ItemMasterDBSet.AddAsync(entity);
                await applicationDBContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.InsertSuccessMessage;
                result.Entity = entity;
                result.Status = (int)ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public async Task<ResultEntity<ItemMasters>> UpdateItemMasters(ItemMasters entity)
        {
            ResultEntity<ItemMasters> result = new ResultEntity<ItemMasters>();
            try
            {
                var itemMaster = await applicationDBContext.ItemMasterDBSet.AsNoTracking().Where(p => p.ID == entity.ID && p.IsActive).FirstOrDefaultAsync();
                itemMaster.ItemName = entity.ItemName;
                itemMaster.ItemCode = entity.ItemCode;
                itemMaster.GSTRate = entity.GSTRate;
                itemMaster.UOM = entity.UOM;
                itemMaster.ItemPrice = entity.ItemPrice;
                itemMaster.HSNCode = entity.HSNCode;
                itemMaster.UpdatedOn = DateTime.Now;
                itemMaster.UpdatedBy = webExtention.UserID;
                applicationDBContext.ItemMasterDBSet.Attach(itemMaster);
                applicationDBContext.Entry(itemMaster).Property(p => p.CreatedBy).IsModified = false;
                applicationDBContext.Entry(itemMaster).Property(p => p.CreatedOn).IsModified = false;
                applicationDBContext.Entry(itemMaster).Property(p => p.UpdatedBy).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.UpdatedOn).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.ItemName).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.ItemCode).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.GSTRate).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.UOM).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.ItemPrice).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.HSNCode).IsModified = true;
                await applicationDBContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.UpdateSuccessMessage;
                result.Entity = entity;
                result.Status = (int)ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public async Task<ResultEntity<bool>> DeleteItemMasters(Guid ItemID)
        {
            ResultEntity<bool> result = new ResultEntity<bool>();
            try
            {
                var itemMaster = await applicationDBContext.ItemMasterDBSet.AsNoTracking().Where(p => p.ID == ItemID && p.IsActive).FirstOrDefaultAsync();
                itemMaster.IsActive = false;
                itemMaster.UpdatedOn = DateTime.Now;
                itemMaster.UpdatedBy = webExtention.UserID;
                applicationDBContext.ItemMasterDBSet.Attach(itemMaster);
                applicationDBContext.Entry(itemMaster).Property(p => p.CreatedBy).IsModified = false;
                applicationDBContext.Entry(itemMaster).Property(p => p.CreatedOn).IsModified = false;
                applicationDBContext.Entry(itemMaster).Property(p => p.UpdatedBy).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.UpdatedOn).IsModified = true;
                applicationDBContext.Entry(itemMaster).Property(p => p.IsActive).IsModified = true;
                applicationDBContext.ItemMasterDBSet.Attach(itemMaster);
                await applicationDBContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.InsertSuccessMessage;
                result.Entity = true;
                result.Status = (int)ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                result.Entity = false;
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public async Task<ResultList<ItemMasters>> GetAllItemMasters()
        {
            ResultList<ItemMasters> result = new ResultList<ItemMasters>();
            try
            {
               var AllItemList = await applicationDBContext.ItemMasterDBSet.AsNoTracking().ToListAsync();
                result.List = AllItemList;
                result.Status = (int)ResponseStatus.Success;
                result.ItemCount = AllItemList.Count;
            }
            catch (Exception ex)
            {
                result.List = null;
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public async Task<ResultEntity<ItemMasters>> GetItemByID(Guid ItemID)
        {
            ResultEntity<ItemMasters> result = new ResultEntity<ItemMasters>();
            try
            {
                var AllItemList = await applicationDBContext.ItemMasterDBSet.AsNoTracking().Where(x => x.ID == ItemID).FirstOrDefaultAsync();
                result.Entity = AllItemList;
                result.Status = (int)ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                result.Entity = null; 
                result.Status = (int)ResponseStatus.Error;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public async Task<ResultList<ItemMasters>> SearchItemMaster(GridParameters gridParams, Expression<Func<ItemMasters, bool>> expression = null)
        {
            ResultList<ItemMasters> result = new ResultList<ItemMasters>();
            try
            {
                if (expression == null)
                {
                    expression = x => true;
                }

                var count = await applicationDBContext.Set<ItemMasters>().Where(expression).CountAsync();
                if (count > 0)
                {
                    if (gridParams.PageNumber == 0)
                    {
                        gridParams.PageNumber = 1;
                    }

                    if (gridParams.PageSize == 0)
                    {
                        gridParams.PageSize = count;
                        gridParams.PageNumber = 1;
                    }
                    gridParams.SortBy = string.IsNullOrWhiteSpace(gridParams.SortBy) ? "CreatedOn" : gridParams.SortBy;
                    result.ItemCount = count;
                    result.TotalPages = (int)Math.Ceiling((double)count / gridParams.PageSize);
                    var skip = (gridParams.PageNumber - 1) * gridParams.PageSize;

                    var data = await applicationDBContext.Set<ItemMasters>().Where(expression).SortBy(gridParams.SortBy, gridParams.IsAscending, gridParams.Culture)
                        .Skip(skip).Take(gridParams.PageSize).ToListAsync();

                    if (data.Count > 0)
                        result.List = data;
                    else
                    {
                        result.Status = 3;
                        result.MessageEnglish =  CommonRepositoryMessages.CannotFindAllMessage;
                    }
                }
                else
                {
                    result.Status = 3;
                    result.MessageEnglish =  CommonRepositoryMessages.CannotFindAllMessage;
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public List<MasterDTODictionary> GetFormattedMasterData<T>(string Culture) where T : BaseLookupEntity
        {
            try
            {
                var data = applicationDBContext.Set<T>().AsNoTracking().OrderBy(a => a.CreatedOn).Select(p => MasterDTODictionary.FromLookup<T>(p, Culture)).ToList();
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, ex.ToMessageString());
                return null;
            }
        }

        public List<MasterDTODictionary> GetFormattedMasterEntityData<T>(string Culture) where T : BaseEntity
        {
            try
            {
                var data = applicationDBContext.Set<T>().AsNoTracking().OrderBy(a => a.CreatedOn).Select(p => MasterDTODictionary.FromEntity<T>(p, Culture)).ToList();
                return data;
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex, ex.ToMessageString());
                return null;
            }
        }
    }
}
