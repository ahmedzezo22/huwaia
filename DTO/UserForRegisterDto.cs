using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ZawagProject.DTO
{
    public class UserForRegisterDto
    {
       public UserForRegisterDto()
    {
      CreatedAt=DateTime.Now;
      LastActive=DateTime.Now;
        
    }
        [Required]
        [StringLength(20,MinimumLength =4,ErrorMessage ="اسم المستخدم يجب الا تقل عن 4 احرف ولاتزيد عن عشره")]
        public string UserName { get; set; }
          [Required]
        [StringLength(10,MinimumLength =8,ErrorMessage ="كلمة السر يجب الا تقل عن 8 احرف ولاتزيد عن عشره")]
        public string Password { get; set; }
         [Required]
       public string Gender { get; set; }
        [Required]
       public string KnownAs { get; set; }
        [Required]
       public string city { get; set; }
        [Required]
       public string Country { get; set; }
        [Required]
       public DateTime DateOfBirth { get; set; }

       public DateTime CreatedAt { get; set; }

       public DateTime LastActive { get; set; }
    }
   
}
