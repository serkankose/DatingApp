using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private DataContext context;

        public AuthRepository(DataContext context)
        {
            this.context = context;            
        }
        public async Task<User> Login(string username, string password)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if(user==null) return null;

            if(!VerifyPasswordHash(password, user.PassswordSalt, user.PassswordHash)) return null;

        }

        private bool VerifyPasswordHash(string password, byte[] passswordSalt, byte[] passswordHash)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512()){
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if(computedHash[i]!=passswordHash[i]) return false;
                }
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            user.PassswordHash = passwordHash;
            user.PassswordSalt = passwordSalt;

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string userName)
        {
            if(await context.Users.AnyAsync(x => x.Username == userName)) return true;
            return false;
        }
    }
}