using Entity.Common.Logs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Common.Helper
{
    public class LoggingMiddleware
    {
        private const string RESPONSE_HEADER_RESPONSE_TIME = "X-Response-Time-ms";
        private const string RESPONSE_HEADER_LANGUAGE = "X-Language";
        private LogModel requestLogs;
        RequestDelegate _next;
        //  private IWebWorker webWorker;

        public LoggingMiddleware(RequestDelegate next
            //  , IWebWorker _webWorker
            )
        {
            _next = next;
            //   webWorker = _webWorker;
        }

        public async Task InvokeAsync(HttpContext pHttpContext)
        {
            if (pHttpContext.Request.Path.StartsWithSegments(new PathString("/api")))
            {
                String requestBody;
                String responseBody = "";
                DateTime requestTime = DateTime.UtcNow;
                Stopwatch stopwatch;

                pHttpContext.Request.EnableBuffering();

                using (StreamReader reader = new StreamReader(pHttpContext.Request.Body,
                                                              encoding: Encoding.UTF8,
                                                              detectEncodingFromByteOrderMarks: false,
                                                              leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    pHttpContext.Request.Body.Position = 0;
                }

                Stream originalResponseStream = pHttpContext.Response.Body;
                using (MemoryStream responseStream = new MemoryStream())
                {
                    pHttpContext.Response.Body = responseStream;

                    stopwatch = Stopwatch.StartNew();
                    await _next(pHttpContext);
                    stopwatch.Stop();

                    pHttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                    responseBody = await new StreamReader(pHttpContext.Response.Body).ReadToEndAsync();
                    pHttpContext.Response.Body.Seek(0, SeekOrigin.Begin);

                    await responseStream.CopyToAsync(originalResponseStream);
                }

                if (requestLogs == null)
                {
                    requestLogs = new LogModel();
                }
                requestLogs = LogModel.FromContext();
                // requestLogs.ExecutionTime = TimeSpan.FromMilliseconds(responseTimeForCompleteRequest);
                requestLogs.Request = requestBody;
                requestLogs.Response = responseBody;
            }
            else
            {
                await _next(pHttpContext);
            }
        }
        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            HttpRequestRewindExtensions.EnableBuffering(request);
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
        }
    }
}
