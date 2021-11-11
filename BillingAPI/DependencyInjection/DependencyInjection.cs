using DataAccess.Common;
using DataAccess.Interface;
using DataAccess.Repository;
using Domains.Domains.Auth;
using Domains.Domains.Invoice;
using Domains.Domains.Masters.ItemMastersDomain;
using Domains.Domains.Security;
using Domains.Interfaces.Auth;
using Domains.Interfaces.Invoice;
using Domains.Interfaces.Masters;
using Domains.Interfaces.Security;
using Entity.Common.Entities;
using Entity.Common.Logs;
using Entity.Common.WebExtention;
using Entity.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DependencyInjection
{
    public static class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IWebExtention, WebExtention>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
 
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IItemMastersRepository, ItemMastersRepository>();
            services.AddScoped<IInvoiceRepository,InvoiceRepository>();

            services.AddScoped<IAuthDomain, AuthDomain>();
            services.AddScoped<IUserDomain, UserDomain>();
            services.AddScoped<IItemMaster, ItemMastersDomain>();
            services.AddScoped<IInvoiceDomain, InvoiceDomain>();


        }

        public static void MigrateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDBContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
        public static void BillingContext(this IApplicationBuilder app)
        {
            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            LogHelper.ConfigureContext(httpContextAccessor);
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        LogHelper.LogError(contextFeature.Error);
                        ResultEntity<object> resultEntity = new ResultEntity<object>()
                        {
                            Status = (int)ResponseStatus.Error,
                            MessageEnglish = "An unexpected fault happened.Try again later.",
                            Entity = null
                        };
                        await context.Response.WriteAsync(resultEntity.ToJson());
                    }
                });
            });
        }
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }
        public static void RegisterSerilog(this IServiceCollection services, IConfiguration seriLogConfiguration)
        {
            LogHelper.ConfigureSeriLog(seriLogConfiguration);
             
        }
    }
}
