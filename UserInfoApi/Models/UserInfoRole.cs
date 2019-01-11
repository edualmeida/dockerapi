using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace UserInfoApi.Models
{
    public class UserInfoRole
    {
        public string Name { get; set; }          
    }
}