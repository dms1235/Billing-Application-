using Entity.Common.Entities;
using Entity.Entities.DTO;
using Entity.Entities.Invoice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Interfaces.Invoice
{
    public interface IInvoiceDomain
    {
        Task<ResultEntity<InvoiceDTO>> AddInvoice(InvoiceMaster entity, List<InvoiceDetails> invoiceDetails);
        Task<ResultEntity<InvoiceMaster>> UpdateInvoiceMaster(InvoiceMaster entity);
        Task<ResultEntity<object>> UpdateInvoiceDetails(List<InvoiceDetails> entity);
        Task<ResultEntity<bool>> Delete(Guid InvoiceID);
        Task<ResultEntity<InvoiceDTO>> GetInvoiceByID(Guid ItemID);
        Task<ResultList<InvoiceMaster>> SearchItem(GridParameters gridParameters);
    }
}
