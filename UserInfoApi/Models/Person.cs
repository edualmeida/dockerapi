using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace UserInfoApi.Models
{
    public class Person
    {
        [BsonElement("Name")]
        public String Name { get; set; }
        [BsonElement("Gender")]
        public Boolean Gender { get; set; }
        [BsonElement("Birthdate")]
        public DateTime Birthdate { get; set; }
        [BsonElement("MothersName")]
        public String MothersName { get; set; }
        
    }
}