using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UserInfoApi.Models;
using UserInfoApi.Services;

namespace UserInfoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly UserInfoService _userInfoService;

        public UserInfoController(UserInfoService userInfoService)
        {
            _userInfoService = userInfoService;
        }

        // GET api/values
        [HttpGet]
        [Authorize(JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<IEnumerable<UserInfo>> Get()
        {
            return _userInfoService.Get();
        }

        [HttpGet("{id:length(24)}", Name = "GetUserInfo")]
        public ActionResult<UserInfo> Get(string id)
        {
            var userInfo = _userInfoService.Get(id);

            if (userInfo == null)
            {
                return NotFound();
            }

            return userInfo;
        }

        [HttpPost]
        public ActionResult<UserInfo> Create([FromBody] UserInfo userInfo)
        {
            _userInfoService.Create(userInfo);

            return CreatedAtRoute("GetUserInfo", new { id = userInfo.Id.ToString() }, userInfo);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, UserInfo userInfoIn)
        {
            var userInfo = _userInfoService.Get(id);

            if (userInfo == null)
            {
                return NotFound();
            }

            _userInfoService.Update(id, userInfoIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var userInfo = _userInfoService.Get(id);

            if (userInfo == null)
            {
                return NotFound();
            }

            _userInfoService.Remove(userInfo.Id);

            return NoContent();
        }
    }
}
