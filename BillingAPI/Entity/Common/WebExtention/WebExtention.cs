using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Linq;
using System.Threading;

namespace Entity.Common.WebExtention
{
    public  class WebExtention : IWebExtention
    {
        public IConfiguration configuration;
        public IWebHostEnvironment Environment;
        public WebExtention(IActionContextAccessor actionContext, IConfiguration _configuration, IWebHostEnvironment _env)
        {
            Environment = _env;
            configuration = _configuration;
            if (actionContext.ActionContext != null)
            {
                var httpContext = actionContext.ActionContext.HttpContext;
                if (httpContext != null && httpContext.Request.Headers.Any())
                {
                    Header = httpContext.Request.Headers.Select(header => new HttpItem { Origin = ItemOrigin.Header, Key = header.Key, Value = header.Value }).ToList();
                    if (Header.Count > 0)
                    {
                        var cultureData = Header.FirstOrDefault(key => key.Key.ToLower() == "culture");
                        if (cultureData == null)
                        {
                            cultureData = Header.FirstOrDefault(key => key.Key.ToLower() == "accept-language");
                        }
                        if (cultureData != null)
                        {
                            string CultureValue = cultureData.Value.ToString().Replace('"', ' ').Trim();
                            AcceptedLanguage = "en-US";
                        }
                    }
                }

                if (httpContext != null && httpContext.Request.Cookies.Any())
                {
                    Cookies = new HttpItemCollection(httpContext.Request.Cookies.Select(cookie => new HttpItem { Origin = ItemOrigin.Cookies, Key = cookie.Key, Value = cookie.Value }));
                }

                try
                {
                    if (httpContext != null && httpContext.Request.Form.Any())
                    {
                        FormData = new HttpItemCollection(httpContext.Request.Form.Select(form => new HttpItem { Origin = ItemOrigin.FormData, Key = form.Key, Value = form.Value }));
                    }
                }
                catch
                {
                }
                RequestId = httpContext.TraceIdentifier;
                RemoteIP = httpContext.Connection.RemoteIpAddress.ToString();
            }
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(AcceptedLanguage);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(AcceptedLanguage);
        }
        public string RequestId { get; }

        public List<HttpItem> Header { get; set; }

        public HttpItemCollection Cookies { get; set; }

        public HttpItemCollection FormData { get; }

        public string AcceptedLanguage { get; set; } = "en-US";
        public string DefaultLanguage { get; set; } = "en-US";

        public string DefaultLanguages()
        {
            return DefaultLanguage;
        }
        public string CultureCode
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            }
        }
        public string GetRequestLanguage()
        {
            return AcceptedLanguage;
        }

        public void SetCulture(string CultureCode)
        {
            AcceptedLanguage = "en-US";
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(AcceptedLanguage);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(AcceptedLanguage);
        }

        public string RemoteIP { get; set; }
        public string UserType { get; set; }
        public string TokenIP { get; set; }
        public string UserName { get; set; }
        public Guid UserID { get; set; }
        public string UserPolicy { get; set; }
        public Guid ICID { get; set; } = Guid.Empty;
        public string UnitNumber { get; set; }

        public string WebRootPath => Environment.WebRootPath;
        public string ContentRootPath => Environment.ContentRootPath;
    }
}
