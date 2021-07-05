using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using ZawagProject.Models;

namespace ZawagProject.Data
{
    public class TrialData
    {
        private readonly UserManager<User> _usermanager;
        private readonly RoleManager<Role> _roleManager;

        public TrialData(UserManager<User> usermanager, RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
            _usermanager = usermanager;
        }
        // read data from json files
        public void TrialUsers()
        {
            if (!_usermanager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserTrial.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);
                var roles=new List<Role>{
                    new Role{Name="Admin"},
                    new Role{Name="Moderator"},
                    new Role{Name="Member"},
                    new Role{Name="VIP"}
                };
                foreach(var role in roles){
                    _roleManager.CreateAsync(role).Wait();
                }
                foreach (var user in users)
                {
                    // user.Photos.ToList().ForEach(p=>p.IsApproved=true);
                    _usermanager.CreateAsync(user, "password").Wait();
                    _usermanager.AddToRoleAsync(user,"Member").Wait();
                    // byte[]passwordSalt,passwordHash;
                    // CreatePasswordHash("password", out  passwordHash, out  passwordSalt);
                    // // user.PasswordHash=passwordHash;
                    // // user.PasswordSalt=passwordSalt;
                    // user.UserName=user.UserName.ToLower();
                    // _context.Add(user);
                }
                var adminUser=new User{
                    UserName="Admin"
                };
                IdentityResult result=_usermanager.CreateAsync(adminUser,"ahmedmandoo").Result;
                var admin=_usermanager.FindByNameAsync("Admin").Result;
                _usermanager.AddToRolesAsync(admin,new[]{"Admin","Moderator"}).Wait();
            }
            // _context.SaveChanges();
        }
        //  private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        // {
        //     using (var hmac = new System.Security.Cryptography.HMACSHA512())
        //     {
        //         passwordSalt = hmac.Key;
        //         passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //     }
        // }

    }
}