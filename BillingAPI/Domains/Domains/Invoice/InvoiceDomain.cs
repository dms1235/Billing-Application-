using DataAccess.Interface;
using Domains.Interfaces.Invoice;
using Entity.Common.Entities;
using Entity.Entities.DTO;
using Entity.Entities.Invoice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Domains.Invoice
{
    public class InvoiceDomain : IInvoiceDomain
    {
        private readonly IInvoiceRepository invoiceRepository;
        public InvoiceDomain(IInvoiceRepository _invoiceRepository)
        {
            invoiceRepository = _invoiceRepository;
        }

        public async Task<ResultEntity<InvoiceDTO>> AddInvoice(InvoiceMaster entity,List<InvoiceDetails> invoiceDetails)
        {
            return await invoiceRepository.InsertInvoiceMaster(entity, invoiceDetails);
        }

        public async Task<ResultEntity<InvoiceMaster>> UpdateInvoiceMaster(InvoiceMaster entity)
        {
            return await invoiceRepository.UpdateInvoiceMasters(entity);
        }
        public async Task<ResultEntity<object>> UpdateInvoiceDetails(List<InvoiceDetails> entity)
        {
            return await invoiceRepository.UpdateInvoiceDetails(entity);
        }
        public async Task<ResultEntity<bool>> Delete(Guid InvoiceID)
        {
            return await invoiceRepository.DeleteInvoice(InvoiceID);
        }
        public async Task<ResultEntity<InvoiceDTO>> GetInvoiceByID(Guid ItemID)
        {
            return await invoiceRepository.GetInvoiceByID(ItemID);
        }
        public async Task<ResultList<InvoiceMaster>> SearchItem(GridParameters gridParameters)
        {
            return await invoiceRepository.SearchInvoice(gridParameters);
        }
    }
}
