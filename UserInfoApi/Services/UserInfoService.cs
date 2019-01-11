using System.Collections.Generic;
using System.Linq;
using UserInfoApi.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace UserInfoApi.Services
{
    public class UserInfoService
    {
         private readonly IMongoCollection<UserInfo> _userinfos;

        public UserInfoService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("UserInfoStoreDb"));
            var database = client.GetDatabase("UserInfoStoreDb");

            _userinfos = database.GetCollection<UserInfo>("UserInfos");
        }

        public List<UserInfo> Get()
        {            
            return _userinfos.Find(UserInfo => true).ToList();
        }

        public UserInfo Get(string id)
        {
            var docId = new ObjectId(id);

            return _userinfos.Find<UserInfo>(userinfo => userinfo.Id == docId).FirstOrDefault();
        }

        public UserInfo Create(UserInfo userinfo)
        {
            _userinfos.InsertOne(userinfo);

            return userinfo;
        }

        public void Update(string id, UserInfo userinfoIn)
        {
            var docId = new ObjectId(id);

            _userinfos.ReplaceOne(userinfo => userinfo.Id == docId, userinfoIn);
        }

        public void Remove(UserInfo userinfoIn)
        {
            _userinfos.DeleteOne(userinfo => userinfo.Id == userinfoIn.Id);
        }

        public void Remove(ObjectId id)
        {
            _userinfos.DeleteOne(userinfo => userinfo.Id == id);
        }

        public UserInfo GetByLoginAndPassword(string username, string password)
        {
            return _userinfos.Find<UserInfo>(userinfo => 
                userinfo.Username == username &&
                userinfo.Password == password
            ).FirstOrDefault();
        }
    }
}