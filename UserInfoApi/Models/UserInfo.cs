using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace UserInfoApi.Models
{
    public class UserInfo
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        //[Required]
        //[Display(Name="Username")]
         [BsonElement("Username")]
        public String Username { get; set; }

        [BsonElement("Password")]
        public String Password { get; set; }

        [BsonElement("Created")]
        public DateTime Created { get; set; }

        [BsonElement("Roles")]
        public List<UserInfoRole> Roles { get; set; }

        [BsonElement("Person")]
        public Person Person { get; set; }

    }
}