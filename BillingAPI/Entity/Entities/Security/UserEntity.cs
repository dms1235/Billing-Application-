using Entity.Base;
using Entity.Common.Attributes;
using Entity.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entity.Entities.Security
{
    [Table(TableNames.Users)]
    public class UserEntity :BaseEntity
    {
        [GlobleSearch]
        [Column("Username")]
        public string Username { set; get; }

        [Column("Password")]
        public string Password { set; get; }

        [Column("Is_Active")]
        public bool IsActive { set; get; } = true;

        [GlobleSearch]
        [Column("Name")]
        public string Name { set; get; }

        [GlobleSearch]
        [Column("Email")]
        public string Email { set; get; }

        [GlobleSearch]
        [Column("Mobile_Number")]
        public string MobileNumber { get; set; }

      
    }
}
