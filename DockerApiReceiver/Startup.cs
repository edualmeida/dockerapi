using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DockerApiReceiver
{
    public class TokenAuthOption
    {        
        public TokenAuthOption(string audience, string issuer, SecurityKey securityKey, DateTime expiresSpan)
        {
            if (string.IsNullOrWhiteSpace(audience))
                throw new ArgumentNullException("audience");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentNullException("issuer");

            this.Audience = audience;
            this.Issuer = issuer;
            this.SecurityKey = securityKey;
            this.ExpiresIn = expiresSpan;
            this.TokenType = JwtBearerDefaults.AuthenticationScheme;
        }

        public string Audience { get; private set; } // = Configuration["Tokens:Issuer"]; //"http://localhost:4200/";
        public string Issuer { get; private set; } // = "http://localhost:56451/";
        //public static RsaSecurityKey Key { get; } = new RsaSecurityKey(RSAKeyHelper.GenerateKey());
        //public static SigningCredentials SigningCredentials { get; } = new SigningCredentials(Key, SecurityAlgorithms.RsaSha256Signature);
        public SecurityKey SecurityKey { get; private set; }
            //new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TK123ASSAHUDHSADUHASDHUASDHUSDHUHASDUHASDUHASHD"));

        public DateTime ExpiresIn { get; private set; }
        public string TokenType { get; private set; }
    }

    public class TokenAuthOptionBuilder
    {
        public static TokenAuthOption BuildFromConfig()
        {
            IConfiguration Configuration = null;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            return new TokenAuthOption(
                Configuration["Tokens:Audience"], 
                Configuration["Tokens:Issuer"],
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:SecretKey"])),
                expiresSpan: DateTime.Now.AddDays(30));            
        }
    }

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

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<RabbitListener>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseRabbitListener();
        }
    }
}
