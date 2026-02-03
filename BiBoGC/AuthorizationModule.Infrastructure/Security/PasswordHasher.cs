using System.Security.Cryptography;
using System.Text;
using AuthorizationModule.Application.Interfaces;

namespace AuthorizationModule.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        passwordSalt = Convert.ToBase64String(hmac.Key);
        passwordHash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }

    public bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
    {
        var key = Convert.FromBase64String(passwordSalt);
        using var hmac = new HMACSHA512(key);
        
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(computedHash) == passwordHash;
    }
}