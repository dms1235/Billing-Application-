using Entity.Common.Helper;
using Entity.Common.WebExtention;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Common.Logs
{
    public static class LogHelper
    {
        public static bool IsSerilogConfigured { get; private set; } = false;

        private static IHttpContextAccessor HttpContextAccessor;
        public static void ConfigureContext(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        private static HttpContext GetHttpContextInternal()
        {
            if (HttpContextAccessor == null)
            {
                return null;
            }
            return HttpContextAccessor.HttpContext;
        }

        public static T GetApplicationService<T>()
        {
            try
            {
                var context = GetHttpContext();
                if (context != null)
                {
                    return context.RequestServices.GetRequiredService<T>();
                }

                return default(T);
            }
            catch (Exception ex)
            {
                LogHelper.LogDebug($"Class:SerilogHelper, Method:GetApplicationService, Message: {ex.ToMessageString()}");
                return default(T);
            }
        }

        public static HttpContext GetHttpContext() { return GetHttpContextInternal(); }

        public static void ConfigureSeriLog(IConfiguration seriLogConfiguration)
        {
            // Set true to disable log
            bool isLogEnable = seriLogConfiguration.GetValue<bool>("Serilog:IsLogEnable");
            Log.Logger = new LoggerConfiguration()
                           .Filter.ByExcluding(_ => !isLogEnable)
                           .Enrich.WithHttpRequestId()
                           .ReadFrom.Configuration(seriLogConfiguration)
                           .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                           .CreateLogger();
            IsSerilogConfigured = true;
            // Need to make path dynamic and append data everytime 
            // Also need to set message template with custom result
            bool selfLogEnable = seriLogConfiguration.GetValue<bool>("Serilog:SelfLogEnable");
            if (selfLogEnable)
            {
                string selfLogFilePath = seriLogConfiguration.GetValue<string>("Serilog:SelfLogFilePath");
                if (!Directory.Exists(selfLogFilePath))
                {
                    Directory.CreateDirectory(selfLogFilePath);
                }
                string applicationName = seriLogConfiguration.GetSection("Serilog:Properties").GetValue<string>("ApplicationName");
                var filename = selfLogFilePath + string.Format("{0}_{1}.txt", applicationName, DateTime.Now.ToString("yyyy-MM-dd"));
                var file = File.AppendText(filename);
                Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(file));
            }
        }

        /// <summary>
        /// To Log Debug Messages 
        /// </summary>
        public static void LogDebug(string message, string userName = "")
        {
            GetDefaultLogger(null, userName).Debug(message);
        }

        /// <summary>
        /// To Log Debug Messages with custom properties
        /// </summary>
        public static void LogDebug(LogModel logModel)
        {
            GetDefaultLogger(logModel).Debug(logModel.Message);
        }

        /// <summary>
        /// To Log Information Messages 
        /// </summary>
        public static void LogInformation(string message, string userName = "")
        {
            GetDefaultLogger(null, userName).Information(message);
        }

        /// <summary>
        /// To Log Information Messages with custom properties
        /// </summary>
        public static void LogInformation(LogModel logModel)
        {
            GetDefaultLogger(logModel).Information(logModel.Message);
        }

        /// <summary>
        /// To Log Exception details with custom properties
        /// </summary>
        public static void LogError(Exception ex, string message = "", string userName = "")
        {
            if (message.Trim().Length == 0)
            {
                message = ex.ToMessageString();
            }
            HttpContext httpContext = GetHttpContext();
            if (httpContext != null)
            {

                string Request = GetRequestBody(httpContext.Request).ConfigureAwait(false).GetAwaiter().GetResult();
                GetDefaultLogger(null, userName).ForContext("Request", Request).ForContext("Message", message).Error(ex, string.Empty);
            }
            else
            {

                GetDefaultLogger(null, userName).ForContext("Message", message).Error(ex, string.Empty);
            }
        }

        public static void LogError(string message, string userName = "")
        {
            if (message.Trim().Length > 0)
            {
                GetDefaultLogger(null, userName).ForContext("Message", message);
            }
        }
        public static string GetAbsoluteUrl(HttpRequest httpRequest)
        {
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = httpRequest.Scheme;
            uriBuilder.Host = httpRequest.Host.Host;
            uriBuilder.Path = httpRequest.Path.ToString();
            uriBuilder.Query = httpRequest.QueryString.ToString();
            return uriBuilder.Uri.AbsoluteUri;
        }

        private static IBrowser GetBrowserDetails()
        {
            try
            {
                HttpContext httpContext = GetHttpContext();
                var userAgentStringSpan = httpContext.Request.Headers["User-Agent"][0].AsSpan();
                return Detector.GetBrowser(userAgentStringSpan);
            }
            catch
            {
                return null;
            }
        }

        private static ILogger GetDefaultLogger(LogModel logModel = null, string userName = "")
        {
            string ipAddress = string.Empty;
            string action = string.Empty;
            string requestURL = string.Empty;
            HttpContext httpContext = GetHttpContext();
            string Request = "";
            if (httpContext != null)
            {
                ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
                action = httpContext.Request.Path;
                requestURL = GetAbsoluteUrl(httpContext.Request);
                IBrowser browser = GetBrowserDetails();
                Request = GetRequestBody(httpContext.Request).ConfigureAwait(false).GetAwaiter().GetResult();
                if (logModel == null)
                {
                    if (string.IsNullOrEmpty(userName))
                    {
                        IWebExtention webWorker = GetApplicationService<IWebExtention>();
                        if (webWorker != null)
                        {
                            userName = webWorker.UserName;
                        }
                    }
                    return Log.Logger.ForContext("BrowserName", browser?.Name).ForContext("BrowserVersion", browser?.Version)
                      .ForContext("OperatingSystem", browser?.OS).ForContext("IPAddress", ipAddress).ForContext("Action", action).
                       ForContext("RequestURL", requestURL).ForContext("UserName", userName).ForContext("Request", Request);
                }
                else
                {
                    if (string.IsNullOrEmpty(logModel.Request))
                    {
                        logModel.Request = Request;
                    }
                    return Log.Logger.ForContext("IPAddress", logModel.IPAddress).ForContext("UserName", logModel.UserName)
                       .ForContext("Request", logModel.Request).ForContext("Response", logModel.Response).ForContext("Action", logModel.Action).
                       ForContext("RequestURL", logModel.RequestURL).ForContext("ExecutionTime", logModel.ExecutionTime).ForContext("BrowserName", browser?.Name)
                      .ForContext("BrowserVersion", browser?.Version).ForContext("OperatingSystem", browser?.OS);
                }
            }
            else
            {
                if (logModel == null)
                {
                    if (string.IsNullOrEmpty(userName))
                    {
                        IWebExtention webWorker = GetApplicationService<IWebExtention>();
                        if (webWorker != null)
                        {
                            userName = webWorker.UserName;
                        }
                    }
                    return Log.Logger.ForContext("IPAddress", ipAddress).ForContext("Action", action).
                       ForContext("RequestURL", requestURL).ForContext("UserName", userName).ForContext("Request", Request);
                }
                else
                {
                    if (string.IsNullOrEmpty(logModel.Request))
                    {
                        logModel.Request = Request;
                    }
                    return Log.Logger.ForContext("IPAddress", logModel.IPAddress).ForContext("UserName", logModel.UserName)
                       .ForContext("Request", logModel.Request).ForContext("Response", logModel.Response).ForContext("Action", logModel.Action).
                       ForContext("RequestURL", logModel.RequestURL).ForContext("ExecutionTime", logModel.ExecutionTime);
                }
            }
        }
        private static async Task<string> GetRequestBody(HttpRequest request)
        {
            var body = new StreamReader(request.Body);
            //The modelbinder has already read the stream and need to reset the stream index
            body.BaseStream.Seek(0, SeekOrigin.Begin);
            var requestBody = body.ReadToEnd();

            string Headers =  request.Headers.GetFormattedString();
            return $"Request:{requestBody.Replace("\0", "").Trim()} | Headers:{Headers.Trim()}";
        }

    }
    /// <summary>
    /// Enrich log events with a HttpRequestId GUID.
    /// </summary>
    public class HttpRequestIdEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpRequestIdPropertyName = "RequestId";

        static readonly string RequestIdItemName = typeof(HttpRequestIdEnricher).Name + "+RequestId";

        /// <summary>
        /// Enrich the log event with an id assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");
            Guid requestId;
            if (!TryGetCurrentHttpRequestId(out requestId))
                return;
            var requestIdProperty = new LogEventProperty(HttpRequestIdPropertyName, new ScalarValue(requestId));
            logEvent.AddPropertyIfAbsent(requestIdProperty);
        }

        /// <summary>
        /// Retrieve the id assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="requestId">The request id.</param>
        /// <returns><c>true</c> if there is a request in progress; <c>false</c> otherwise.</returns>
        public static bool TryGetCurrentHttpRequestId(out Guid requestId)
        {
            HttpContext httpContext = LogHelper.GetHttpContext();
            if (httpContext != null && httpContext.Items == null)
            {
                requestId = default(Guid);
                return false;
            }
            else if (httpContext == null)
            {
                requestId = default(Guid);
                return false;
            }

            var requestIdItem = httpContext.Items[RequestIdItemName];
            if (requestIdItem == null)
                httpContext.Items[RequestIdItemName] = requestId = Guid.NewGuid();
            else
                requestId = (Guid)requestIdItem;
            return true;
        }
    }

    public class LogModel
    {
        public static LogModel FromContext()
        {
            LogModel logModel = new LogModel();
            HttpContext httpContext = LogHelper.GetHttpContext();
            string ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
            string action = httpContext.Request.Path;
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = httpContext.Request.Scheme;
            uriBuilder.Host = httpContext.Request.Host.Host;
            uriBuilder.Path = httpContext.Request.Path.ToString();
            uriBuilder.Query = httpContext.Request.QueryString.ToString();
            string requestURL = uriBuilder.Uri.AbsoluteUri;
            logModel.Action = action;
            logModel.IPAddress = ipAddress;
            logModel.RequestURL = requestURL;
            try
            {
                logModel.Message = string.Format("Request {0} {1} {2}", httpContext.Request.Method, httpContext.Request.Path, httpContext.Response.StatusCode.ToString());
            }
            catch
            {
                logModel.Message = "";
            }
            return logModel;
        }

        public string Action { get; set; }
        public string RequestURL { get; set; }
        public string IPAddress { get; set; }

        public string UserName { get; set; }
        public string Message { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }

        public TimeSpan ExecutionTime { get; set; }
        public string SessionID { get; set; }
    }
}
