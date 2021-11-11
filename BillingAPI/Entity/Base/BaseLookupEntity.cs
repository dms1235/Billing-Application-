using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Entity.Common.Attributes;
namespace Entity.Base
{
    public abstract class BaseLookupEntity :BaseEntity
    {
        [GlobleSearch]
        [Column("Code", Order = 1)]
        public string Code { set; get; }
        [GlobleSearch]
        [Column("Name", Order = 3)]
        public string Name { set; get; }
        [Column("Is_Enabled", Order = 4)]
        public bool IsEnabled { set; get; } = true;
    }
}
