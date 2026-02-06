using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Domain.Entities;
using AuthorizationModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthorizationModule.Infrastructure.Data;

public static class AuthorizationDataSeeder
{
    public static async Task SeedAsync(AuthorizationDbContext context, ILogger logger, IPasswordHasher passwordHasher)
    {
        // Idempotent: chạy nhiều lần không tạo trùng
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Authorization DB already contains users. Skipping seed.");
            return;
        }

        // TODO: Đổi password ngay sau khi login lần đầu, hoặc đọc từ config/secret
        passwordHasher.CreatePasswordHash("Admin@123", out var adminHash, out var adminSalt);
        passwordHasher.CreatePasswordHash("Seller@123", out var sellerHash, out var sellerSalt);

        context.Users.AddRange(
            new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = adminHash,
                PasswordSalt = adminSalt,
                Role = UserRole.Administrator,
                IsActive = true
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "seller",
                PasswordHash = sellerHash,
                PasswordSalt = sellerSalt,
                Role = UserRole.Seller,
                IsActive = true
            }
        );

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded default admin/seller users.");
    }
}