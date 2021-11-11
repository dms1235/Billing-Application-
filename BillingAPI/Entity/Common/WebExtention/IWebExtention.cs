using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Common.WebExtention
{
   public interface IWebExtention
    {
        string RequestId { get; }
        string RemoteIP { get; }
        string UserName { get; set; }
        string UserType { get; set; }
        string TokenIP { get; set; }
        Guid UserID { get; set; }
        string UserPolicy { get; set; }
        string WebRootPath { get; }
        string ContentRootPath { get; }
        Guid ICID { get; set; }

        string UnitNumber { get; set; }

        List<HttpItem> Header { get; }
        HttpItemCollection FormData { get; }
        HttpItemCollection Cookies { get; }
        void SetCulture(string CultureCode);
    }
}
