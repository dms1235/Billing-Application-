using Entity.Base;
using Entity.Common.Attributes;
using Entity.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entity.Entities.Masters
{
    [Table(TableNames.ItemMasters)]
    public class ItemMasters : BaseEntity
    {
        [GlobleSearch]
        [Column("Item_Code")]
        public string ItemCode { set; get; }
        [GlobleSearch]
        [Column("Item_Name")]
        public string ItemName { set; get; }
        [GlobleSearch]
        [Column("UOM")]
        public string UOM { set; get; }
        [GlobleSearch]
        [Column("HSN_Code")]
        public string HSNCode { set; get; }

        [Column("GST_Rate")]
        public decimal GSTRate { set; get; } = 0;

        [Column("Item_Price")]
        public decimal ItemPrice { set; get; } = 0;

        [Column("Is_Active")]
        public bool IsActive { set; get; } = true;
    }
}
