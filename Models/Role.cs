 using System.Collections.Generic;
 using Microsoft.AspNetCore.Connections;
 using Microsoft.AspNetCore.Identity;

 namespace ZawagProject.Models
 {
     public class Role:IdentityRole<int>
     {
         public ICollection<UserRole> userRole { get; set; }


     }
 }