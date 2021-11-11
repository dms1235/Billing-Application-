using Entity.Entities.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Entities.DTO
{
    public class InvoiceDTO
    {
        public Int64 InvoiceNumber { get; set; }    
        public DateTime InvoiceDate { get; set; }

    }

    public class InvoiceRequestDTO
    {
        public InvoiceMaster invoiceMaster { get; set; }
        public List<InvoiceDetails> invoiceDetails { get; set; }
    }

}
