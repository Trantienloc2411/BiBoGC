using AuthorizationModule.Domain.Entities;

namespace AuthorizationModule.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(User user, out DateTime expirationAt);   
    string GenerateRefreshToken(); //randomString
}