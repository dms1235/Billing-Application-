using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Common.Constants
{
    public static class CommonDomainMessage
    {
        public const string ErrUserNotActive = "User is not active";
        public const string ErrWrongUsernameOrPassword = "Wrong username or password";
        public const string ErrUsernameExist = "User already exist";
        public const string ErrPasswordNotMatch = "Invalid old Password";
        public const string ErrUserNotFound = "User Not Found";
    }
}
