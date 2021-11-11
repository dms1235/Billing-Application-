using DataAccess.Common;
using DataAccess.Interface;
using Entity.Common.Attributes;
using Entity.Common.Constants;
using Entity.Common.Entities;
using Entity.Common.Helper;
using Entity.Common.Logs;
using Entity.Common.WebExtention;
using Entity.Entities.DTO;
using Entity.Entities.Invoice;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        ApplicationDBContext applicationDBContext;
        private readonly IWebExtention webExtention;
        private readonly IConfiguration config;
        public InvoiceRepository(ApplicationDBContext _applicationDBContext,IWebExtention _webExtention, IConfiguration _config)
        {
            applicationDBContext = _applicationDBContext;
            webExtention = _webExtention;
            config = _config;
        }
        public async Task<ResultEntity<InvoiceDTO>> InsertInvoiceMaster(InvoiceMaster entity, List<InvoiceDetails> invoiceDetails)
        {
            ResultEntity<InvoiceDTO> result = new ResultEntity<InvoiceDTO>();
            try
            {
                var strategy = applicationDBContext.Database.CreateExecutionStrategy();
                int commandTimeout = string.IsNullOrEmpty(config["AppSettings:CommandTimeOut"]) ? 800 : Convert.ToInt32(config["AppSettings:CommandTimeOut"]);
                applicationDBContext.Database.SetCommandTimeout(800);
                await strategy.ExecuteAsync(async () =>
                {
                    //BeginTransaction
                    using (var scope = await applicationDBContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var entityInvoiceMaster = await applicationDBContext.InvoiceMasterDBSet.AddAsync(entity);
                            await applicationDBContext.SaveChangesAsync();
                            invoiceDetails.ForEach(f => { f.InvoiceMasterID = entityInvoiceMaster.Entity.ID; });
                            await this.InsertInvoiceDetails(invoiceDetails);

                            await scope.CommitAsync();
                            InvoiceDTO invoiceDTO = new InvoiceDTO();
                            invoiceDTO.InvoiceNumber = entityInvoiceMaster.Entity.InvoiceNumber;
                            invoiceDTO.InvoiceDate = entityInvoiceMaster.Entity.InvoiceDate;
                            result.MessageEnglish = CommonRepositoryMessages.InsertSuccessMessage;
                            result.Entity = invoiceDTO;
                            result.Status = (int)ResponseStatus.Success;
                        }
                        catch (Exception ex)
                        {
                            await scope.RollbackAsync();
                            result.Entity = null;
                            result.Status = (int)ResponseStatus.Error;
                            result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                            result.DetailsEnglish = ex.ToMessageString();
                            LogHelper.LogError(ex, ex.ToMessageString());
                        }

                    }
                });
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
        public async Task<ResultEntity<object>> InsertInvoiceDetails(List<InvoiceDetails> entity)
        {
            ResultEntity<object> result = new ResultEntity<object>();
            try
            {
                await applicationDBContext.InvoiceDetailDBSet.AddRangeAsync(entity);
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
        public async Task<ResultEntity<InvoiceMaster>> UpdateInvoiceMasters(InvoiceMaster entity)
        {
            ResultEntity<InvoiceMaster> result = new ResultEntity<InvoiceMaster>();
            try
            {
                var invoicemaster = await applicationDBContext.InvoiceMasterDBSet.AsNoTracking().Where(p => p.ID == entity.ID && p.IsActive).FirstOrDefaultAsync();
                invoicemaster.VendorCode = entity.VendorCode;
                invoicemaster.ModeOfTransport = entity.ModeOfTransport;
                invoicemaster.DeliveryNote = entity.DeliveryNote;
                invoicemaster.Transpoter = entity.Transpoter;
                invoicemaster.LRNo = entity.LRNo;
                invoicemaster.SupplyDateandTime = entity.SupplyDateandTime;
                invoicemaster.PlaceOfSupply = entity.PlaceOfSupply;
                invoicemaster.BillToCompanyName = entity.BillToCompanyName;
                invoicemaster.BillToAddress1 = entity.BillToAddress1;
                invoicemaster.BillToAddress2 = entity.BillToAddress2;
                invoicemaster.BillToAddress3 = entity.BillToAddress3;
                invoicemaster.BillToAddress4 = entity.BillToAddress4;
                invoicemaster.ConsigneCompanyName = entity.ConsigneCompanyName;
                invoicemaster.ConsigneAddress1 = entity.ConsigneAddress1;
                invoicemaster.ConsigneAddress2 = entity.ConsigneAddress2;
                invoicemaster.ConsigneAddress3 = entity.ConsigneAddress3;
                invoicemaster.ConsigneAddress4 = entity.ConsigneAddress4;
                invoicemaster.UpdatedOn = DateTime.Now;
                invoicemaster.UpdatedBy = webExtention.UserID;
                applicationDBContext.InvoiceMasterDBSet.Attach(invoicemaster);
                applicationDBContext.Entry(invoicemaster).Property(p => p.CreatedBy).IsModified = false;
                applicationDBContext.Entry(invoicemaster).Property(p => p.CreatedOn).IsModified = false;
                applicationDBContext.Entry(invoicemaster).Property(p => p.UpdatedBy).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.UpdatedOn).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.VendorCode).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.ModeOfTransport).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.DeliveryNote).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.Transpoter).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.LRNo).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.SupplyDateandTime).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.PlaceOfSupply).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.BillToCompanyName).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.BillToAddress1).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.BillToAddress2).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.BillToAddress3).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.BillToAddress4).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.ConsigneCompanyName).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.ConsigneAddress1).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.ConsigneAddress2).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.ConsigneAddress3).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.ConsigneAddress4).IsModified = true;
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
        public async Task<ResultEntity<object>> UpdateInvoiceDetails(List<InvoiceDetails> invoiceDetails)
        {
            ResultEntity<object> result = new ResultEntity<object>();
            try
            {
                //first remove all the item
                 applicationDBContext.InvoiceDetailDBSet.RemoveRange(invoiceDetails);
                //add all new item
                await applicationDBContext.InvoiceDetailDBSet.AddRangeAsync(invoiceDetails);
                await applicationDBContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.InsertSuccessMessage;
                result.Entity = invoiceDetails;
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

        public async Task<ResultEntity<bool>> DeleteInvoice(Guid InvoiceID)
        {
            ResultEntity<bool> result = new ResultEntity<bool>();
            try
            {
                var invoicemaster = await applicationDBContext.InvoiceMasterDBSet.AsNoTracking().Where(p => p.ID == InvoiceID && p.IsActive).FirstOrDefaultAsync();
                invoicemaster.IsActive = false;
                invoicemaster.UpdatedOn = DateTime.Now;
                invoicemaster.UpdatedBy = webExtention.UserID;
                applicationDBContext.InvoiceMasterDBSet.Attach(invoicemaster);
                applicationDBContext.Entry(invoicemaster).Property(p => p.CreatedBy).IsModified = false;
                applicationDBContext.Entry(invoicemaster).Property(p => p.CreatedOn).IsModified = false;
                applicationDBContext.Entry(invoicemaster).Property(p => p.UpdatedBy).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.UpdatedOn).IsModified = true;
                applicationDBContext.Entry(invoicemaster).Property(p => p.IsActive).IsModified = true;
                await applicationDBContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.DeleteSuccessMessage;
                result.Entity = true;
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

        public async Task<ResultEntity<InvoiceDTO>> GetInvoiceByID(Guid InvoiceID)
        {
            ResultEntity<InvoiceDTO> result = new ResultEntity<InvoiceDTO>();
            try
            {
               var resultentity =  await applicationDBContext.InvoiceMasterDBSet.AsNoTracking().Where(x => x.ID == InvoiceID && x.IsActive).FirstOrDefaultAsync();
                if(resultentity != null)
                {
                    result.MessageEnglish = CommonRepositoryMessages.AcceptedSuccessMessage;
                    InvoiceDTO invoiceDTO = new InvoiceDTO();
                    invoiceDTO.InvoiceNumber = resultentity.InvoiceNumber;
                    invoiceDTO.InvoiceDate = resultentity.InvoiceDate;
                    result.Entity = invoiceDTO;
                    result.Status = (int)ResponseStatus.Success;
                }
                else
                {
                    result.MessageEnglish = CommonRepositoryMessages.NotFoundDetails;
                    result.Entity = null;
                    result.Status = (int)ResponseStatus.EmptyResult;
                }
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
        public async Task<ResultList<InvoiceMaster>> SearchInvoice(GridParameters gridParams, Expression<Func<InvoiceMaster, bool>> expression = null)
        {
            ResultList<InvoiceMaster> result = new ResultList<InvoiceMaster>();
            try
            {
                if (expression == null)
                {
                    expression = x => true;
                }
                expression = ExtentionMethods.Dynamicsearchfilterforentity<InvoiceMaster>(gridParams.SearchText, gridParams.Culture);
                var count = await applicationDBContext.Set<InvoiceMaster>().Where(expression).CountAsync();
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

                    var data = await applicationDBContext.Set<InvoiceMaster>().Where(expression).SortBy(gridParams.SortBy, gridParams.IsAscending, gridParams.Culture)
                        .Skip(skip).Take(gridParams.PageSize).ToListAsync();
                     
                    if (data.Count > 0)
                        result.List = data;
                    else
                    {
                        result.Status = 3;
                        result.MessageEnglish = CommonRepositoryMessages.CannotFindAllMessage;
                    }
                }
                else
                {
                    result.Status = 3;
                    result.MessageEnglish = CommonRepositoryMessages.CannotFindAllMessage;
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

      
    }
}
