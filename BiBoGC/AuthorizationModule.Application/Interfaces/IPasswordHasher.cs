namespace AuthorizationModule.Application.Interfaces;

public interface IPasswordHasher
{
    void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt);
    bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt);
}