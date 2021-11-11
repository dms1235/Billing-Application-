using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Entity.Common.Helper
{
    public static class SecurityHelper
    {
        public static string HashMD5(string text)
        {
            string result = string.Empty;
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(text));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                result = sBuilder.ToString();
            }

            return result;

        }
    }
}
