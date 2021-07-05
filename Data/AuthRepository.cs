using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZawagProject.Models;

namespace ZawagProject.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User> Login(string userName, string password)
        { 
           
            var user = await _context.Users.Include(p=>p.Photos).FirstOrDefaultAsync(x => x.UserName ==userName);
            if (user == null)
                return null;
            // if (!VerfiyPasswordHash(password, user.PasswordSalt, user.PasswordHash))
            // {
            //     return null;
            // }
            return user;
            
        }

        private bool VerfiyPasswordHash(string password, byte[] passwordSalt, byte[] passwordHash)
        {

            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
              
               var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i = 0; i < computeHash.Length; i++)
                {
                    if (computeHash[i] != passwordHash[i]) return false;
                    
                }
                return true;
            }
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            // user.PasswordSalt = passwordSalt;
            // user.PasswordHash = passwordHash;
            await _context.Users.AddAsync(user);
            
            await _context.SaveChangesAsync();
            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string userName)
        {
            var user = await _context.Users.AnyAsync(x => x.UserName == userName);
            if (user)
            {
                return true;
            }
            else { return false; }
        }
    }
}
