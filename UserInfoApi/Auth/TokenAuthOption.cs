using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace UserInfoApi.Auth
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
}

