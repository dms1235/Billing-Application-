using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Entities.DTO
{
    public class ChangePasswordDTO
    {
        public Guid UserID { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class ResetPasswordDetails
    {
        public Guid RequestID { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string IsResetPasswordFromWeb { get; set; }
    }
}
