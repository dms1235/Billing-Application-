using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Entity.Enums
{
    public enum ResponseStatus : short
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        Error = 1,
        [EnumMember]
        ValidationError = 2,
        [EnumMember]
        EmptyResult = 3,
        [EnumMember]
        InvalidInput = 4,
        [EnumMember]
        UnAuthorized = 5
    }
    public struct TableNames
    {
        public const string Users = "Users";
        public const string ItemMasters = "Item_Masters";
        public const string InvoiceMaster = "Invoice_Masters";
        public const string InvoiceDetails = "Invoice_Details";
    }
    public enum SortType
    {
        ASC = 1,
        DESC = 2
    }
}
