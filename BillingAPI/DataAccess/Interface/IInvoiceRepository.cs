using Entity.Common.Entities;
using Entity.Entities.DTO;
using Entity.Entities.Invoice;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IInvoiceRepository
    {
        Task<ResultEntity<InvoiceDTO>> InsertInvoiceMaster(InvoiceMaster entity, List<InvoiceDetails> invoiceDetails);
        Task<ResultEntity<object>> InsertInvoiceDetails(List<InvoiceDetails> entity);
        Task<ResultEntity<InvoiceMaster>> UpdateInvoiceMasters(InvoiceMaster entity);
        Task<ResultEntity<object>> UpdateInvoiceDetails(List<InvoiceDetails> invoiceDetails);
        Task<ResultEntity<bool>> DeleteInvoice(Guid InvoiceID);
        Task<ResultEntity<InvoiceDTO>> GetInvoiceByID(Guid InvoiceID);
        Task<ResultList<InvoiceMaster>> SearchInvoice(GridParameters gridParams, Expression<Func<InvoiceMaster, bool>> expression = null);
    }
}
