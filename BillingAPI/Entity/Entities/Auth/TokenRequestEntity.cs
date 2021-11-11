using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entity.Entities.Auth
{
    public class TokenRequestEntity
    {
        [Required]
        public string username { set; get; }
        [Required]
        public string password { set; get; }
    }

    public class TokenDataResult : Dictionary<string, object>
    {
        public TokenDataResult()
        {
            this.Add("Token", "");
            this.Add("Validity", 0);
        }
        public TokenDataResult(string Token, int Validity)
        {
            this.Add("Token", Token);
            this.Add("Validity", Validity);
        }
    }

    public class AppTokenResultEntity
    {
        /// <summary>
        /// token to authorize api request
        /// </summary>
        /// <example>SR01012020</example>
        public string Token { get; set; }
        /// <summary>
        /// token token validity in seconds
        /// </summary>
        /// <example>15000</example>
        public int Validity { get; set; }
    }
}
