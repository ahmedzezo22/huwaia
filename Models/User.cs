﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ZawagProject.DTO;

namespace ZawagProject.Models
{
    public class User:IdentityUser<int>
    {
       // public int Id { get; set; }
        //public string UserName { get; set; }
        //public byte[] PasswordHash { get; set; }
        //public byte[] PasswordSalt { get; set; } 
        public string Gender { get; set; }
        public DateTime DateofBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Hobbies { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        
        public ICollection<Photo> Photos { get; set; }
        public ICollection<Like> Likees { get; set; }
         public ICollection<Like> Likers { get; set; }
         public ICollection<Message> MessagesSent { get; set; }

         public ICollection<Message> MessagesRecieved { get; set; }
         
          public ICollection<UserRole> userRole { get; set; }
         
         
         
         
        
        }
}
