using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using UserInfoApi.Auth;
using UserInfoApi.Models;
using UserInfoApi.Services;

namespace UserInfoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenAuthController: Controller
    {
        private readonly UserInfoService _repository;
        private readonly IEventBus _eventBus;

        public TokenAuthController(UserInfoService userInfoService, IEventBus eventBus)
        {
            if (null == userInfoService)
                throw new ArgumentNullException("context");

            _repository = userInfoService;
            _eventBus = eventBus;
        }

        [HttpPost]
        [AllowAnonymous]
        public string GetAuthToken([FromBody]UserInfo user)
        {
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                user.Username = user.Username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                user.Password = user.Password.Trim();
            }

            var existUser = _repository.GetByLoginAndPassword(user.Username, user.Password);

            if (existUser != null)
            {
                var tokenAuthOption = TokenAuthOptionBuilder.BuildFromConfig();
                var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, existUser.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, existUser.Id.ToString()),
                    };
                                
                var creds = new SigningCredentials(tokenAuthOption.SecurityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(tokenAuthOption.Issuer, tokenAuthOption.Audience,
                    claims,expires: tokenAuthOption.ExpiresIn, signingCredentials: creds);

                string tokenStr = "";

                tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
                
                var eventMessage = new UserLoggedinEvent(existUser.Id.ToString(), existUser.Username, tokenStr);

                // Once basket is checkout, sends an integration event to
                // ordering.api to convert basket to order and proceeds with
                // order creation process
                _eventBus.Publish(eventMessage);

                return JsonConvert.SerializeObject(new RequestResult
                {
                    State = RequestState.Success,
                    Data = new
                    {
                        requertAt = DateTime.Now,
                        username = user.Username,
                        //expiresIn = TokenAuthOption.ExpiresSpan.TotalSeconds,
                        //tokeyType = TokenAuthOption.TokenType,
                        accessToken = tokenStr,
                        idUserIdentity = existUser.Id
                    }
                });
                
            }
            else
            {
                return JsonConvert.SerializeObject(new RequestResult
                {
                    State = RequestState.Failed,
                    Msg = "Username or password is invalid"
                });
            }
        }

        //private string GenerateToken(User user, DateTime expires)
        //{
        //    var tokenAuthOption = TokenAuthOptionBuilder.BuildFromConfig();
        //    var handler = new JwtSecurityTokenHandler();

        //    ClaimsIdentity identity = new ClaimsIdentity(
        //        new GenericIdentity(user.Username, "TokenAuth"),
        //        new[] {
        //            new Claim("ID", user.ID.ToString())
        //        }
        //    );

        //    var securityToken = handler.CreateToken(new SecurityTokenDescriptor
        //    {
        //        Issuer = tokenAuthOption.Issuer,
        //        Audience = tokenAuthOption.Audience,
        //        SigningCredentials = tokenAuthOption.SigningCredentials,
        //        Subject = identity,
        //        Expires = expires
        //    });

        //    return handler.WriteToken(securityToken);
        //}

        [HttpGet]
        [Authorize("Bearer")]
        public string GetUserInfo()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;

            return JsonConvert.SerializeObject(new RequestResult
            {
                State = RequestState.Success,
                Data = new
                {
                    UserName = claimsIdentity.Name
                }
            });
        }
    }
}