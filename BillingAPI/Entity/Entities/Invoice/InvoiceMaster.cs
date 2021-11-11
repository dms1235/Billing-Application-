using Entity.Base;
using Entity.Common.Attributes;
using Entity.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entity.Entities.Invoice
{
    [Table(TableNames.InvoiceMaster)]
    public class InvoiceMaster : BaseEntity
    {
        [Column("Invoice_Number")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [GlobleSearch]
        public Int64 InvoiceNumber { get; set; }
        [Column("Invoice_Date")]
        public DateTime InvoiceDate { get; set; }
        [Column("PONumber")]
        public Int64? PONumber { get; set; }
        [Column("PODate")]
        public DateTime? PODate { get; set; }
        [Column("VendorCode")]
        [GlobleSearch]
        public string VendorCode { get; set; }
        [Column("ModeOfTransport")]
        [GlobleSearch]
        public string ModeOfTransport { get; set; }
        [Column("DeliveryNote")]
        [GlobleSearch]
        public string DeliveryNote { get; set; }
        [Column("Transpoter")]
        [GlobleSearch]
        public string Transpoter { get; set; }
        [Column("LRNo")]
        [GlobleSearch]
        public string LRNo { get; set; }
        [Column("SupplyDateandTime")]
        public DateTime? SupplyDateandTime { get; set; }
        [Column("PlaceOfSupply")]
        public string PlaceOfSupply { get; set; }
        [Column("BillToCompanyName")]
        [GlobleSearch]
        public string BillToCompanyName { get; set; }

        [Column("BillToAddress1")]
        public string BillToAddress1 { get; set; }

        [Column("BillToAddress2")]
        public string BillToAddress2 { get; set; }
        [Column("BillToAddress3")]
        public string BillToAddress3 { get; set; }
        [Column("BillToAddress4")]
        public string BillToAddress4 { get; set; }
        [Column("BillToGSTNo")]
        [GlobleSearch]
        public string BillToGSTNo { get; set; }
        [Column("ConsigneCompanyName")]
        [GlobleSearch]
        public string ConsigneCompanyName { get; set; }
        [Column("ConsigneAddress1")]
        public string ConsigneAddress1 { get; set; }
        [Column("ConsigneAddress2")]
        public string ConsigneAddress2 { get; set; }
        [Column("ConsigneAddress3")]
        public string ConsigneAddress3 { get; set; }
        [Column("ConsigneAddress4")]
        public string ConsigneAddress4 { get; set; }
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;
         
    }
    [Table(TableNames.InvoiceDetails)]
    public class InvoiceDetails : BaseEntity
    {
        [Column("ItemName")]
        public string ItemName { get; set; }
        [Column("ItemCode")]
        public string ItemCode { get; set; }
        [Column("ItemID")]
        public Guid ItemID { get; set; }
        [Column("ItemQty")]
        public int ItemQty { get; set; }
        [Column("ItemRate")]
        public decimal ItemRate { get; set; }
        [Column("ItemAmount")]
        public decimal ItemAmount { get; set; }
        [Column("DiscAmt")]
        public decimal DiscAmt { get; set; }
        [Column("ItemWiseAmout")]
        public decimal ItemWiseAmout { get; set; }
        [Column("NetAmount")]
        public decimal NetAmount { get; set; }
        [Column("InvoiceMasterID")]
        [ForeignKey("InvoiceMaster")]
        public Guid InvoiceMasterID { get; set; }
        public InvoiceMaster InvoiceMaster { get; set; }
        [Column("GrossAmount")]
        public decimal GrossAmount { get; set; }
    }
}
