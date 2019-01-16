using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

using RabbitMQ.Client;
using RabbitLib;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using LoggerApi.IntegrationEvents.Events;
using LoggerApi.IntegrationEvents.EventHandling;
using LoggerApi.Auth;

namespace LoggerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var tokenAuthOption = TokenAuthOptionBuilder.BuildFromConfig();
                        
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
              .AddJwtBearer(options => {

                  options.RequireHttpsMetadata = false;
                  options.SaveToken = true;
                  options.TokenValidationParameters =
                       new TokenValidationParameters
                       {
                           //ValidateIssuer = true,
                           //ValidateAudience = true,
                           //ValidateLifetime = true,
                           //ValidateIssuerSigningKey = true,

                           ValidIssuer = tokenAuthOption.Issuer,
                           ValidAudience = tokenAuthOption.Audience,                           
                           IssuerSigningKey = tokenAuthOption.SecurityKey
                       };

                  options.Events = new JwtBearerEvents()
                  {
                      OnAuthenticationFailed = c =>
                      {
                          c.NoResult();

                          c.Response.StatusCode = 401;
                          c.Response.ContentType = "text/plain";

                          return c.Response.WriteAsync(c.Exception.ToString());
                      }

                  };
              }               
              );

            // Enable the use of an [Authorize("Bearer")] attribute on methods and classes to protect.
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            services.AddCors();

            //services.AddScoped<UserInfoService>();
            services.AddCustomIntegrations(Configuration);
            services.AddEventBus(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);          

            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
  
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));            
            //loggerFactory.AddFile(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            #region Handle Exception
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseExceptionHandler(appBuilder => {
                appBuilder.Use(async (context, next) => {
                    var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;

                    //when authorization has failed, should retrun a json message to client
                    if (error != null && error.Error is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(
                            new { authenticated = false, tokenExpired = true }
                        ));
                    }
                    //when orther error, retrun a error message json to client
                    else if (error != null && error.Error != null)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(
                            new { success = false, error = error.Error.Message }
                        ));
                    }
                    //when no error, do next.
                    else await next();
                });
            });
            #endregion

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()                
                );

            app.UseAuthentication();
            //app.UseHttpsRedirection();
            app.UseMvc();

            ConfigureEventBus(app);
        }

        protected virtual void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<UserLoggedinEvent, UserLoggedinEventHandler>();            
        }
    }
}
