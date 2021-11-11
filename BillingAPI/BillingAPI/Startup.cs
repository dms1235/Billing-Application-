using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Common;
using DependencyInjection;
using Entity.Common.Attributes;
using Entity.Common.Helper;
using Entity.Common.WebExtention;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BillingAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddDbContext<ApplicationDBContext>(options =>
            {
                // options.EnableSensitiveDataLogging();
                int Retrycnt = string.IsNullOrEmpty(Configuration["AppSettings:Retrycount"]) ? 10 : Convert.ToInt32(Configuration["AppSettings:Retrycount"]);
                int RetryDealy = string.IsNullOrEmpty(Configuration["AppSettings:RetryDelay"]) ? 30 : Convert.ToInt32(Configuration["AppSettings:RetryDelay"]);
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: Retrycnt,
                        maxRetryDelay: TimeSpan.FromSeconds(RetryDealy),
                        errorNumbersToAdd: null
                        );
                });
            });
            services.RegisterSerilog(Configuration);
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                // options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ValidateMobileModelAttribute));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            RegisterServices(services);
            string APIVersion = Configuration["AppSettings:APIVersion"].ToString();
            if (string.IsNullOrEmpty(APIVersion))
            {
                APIVersion = "1.0";
            }
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing Application APIs", Version = APIVersion });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                //c.OperationFilter<SecurityRequirementsOperationFilter>();
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {  }
                    }
                });
            }).AddSwaggerGenNewtonsoftSupport();
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        var accessToken = context.SecurityToken as JwtSecurityToken;
                        if (accessToken != null)
                        {
                            try
                            {
                                var webExtention = context.HttpContext.RequestServices.GetRequiredService<IWebExtention>();
                                var UserTypeStr = accessToken.Claims.Where(p => p.Type == JwtRegisteredClaimNames.Typ).FirstOrDefault().Value;
                                var UserName = accessToken.Claims.Where(p => p.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault().Value;
                                var UserID = accessToken.Claims.Where(p => p.Type == "uid").FirstOrDefault().Value;
                                var IP = accessToken.Claims.Where(p => p.Type == "uip").FirstOrDefault().Value;
                                var Role = accessToken.Claims.Where(p => p.Type == "role").FirstOrDefault().Value;
                                webExtention.UserName = UserName;
                                webExtention.UserType = UserTypeStr;
                                webExtention.TokenIP = IP;
                                webExtention.UserID = Guid.Parse(UserID);
                                webExtention.UserPolicy = Role;
                            }
                            catch
                            {
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //This is for security re prevent clickjacking attack
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                await next();
            });
            app.BillingContext();
            app.UseMiddleware<LoggingMiddleware>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
              Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                //RequestPath = "/Files"
                RequestPath = new PathString("/Files")
            });
            app.UseHttpsRedirection();
            app.UseSwaggerUI(c =>
            {
                string RouteName = Configuration["AppSettings:Swagger:RouteName"].ToString();
                if (string.IsNullOrEmpty(RouteName))
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing Application APIs");
                    c.InjectStylesheet("/Files/swagger.css");
                    c.InjectJavascript("/Files/swagger.js", "text/javascript");
                }
                else
                {
                    c.SwaggerEndpoint($"/{RouteName}/swagger/v1/swagger.json", "Billing Application APIs");
                    c.InjectStylesheet($"/{RouteName}/Files/swagger.css");
                    c.InjectJavascript($"/{RouteName}/Files/swagger.js", "text/javascript");
                }
            });

            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);
            app.UseRouting();
            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true));
            app.UseAuthorization();
            DependencyContainer.MigrateDatabase(app);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        private static void RegisterServices(IServiceCollection services)
        {
            DependencyContainer.RegisterServices(services);

        }
    }
}
