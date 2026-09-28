using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Infrastructure.Services;

public class UserAccountService : IUserAccountService
{
    private const string LowerChars = "abcdefghjkmnpqrstuvwxyz";
    private const string UpperChars = "ABCDEFGHJKMNPQRSTUVWXYZ";
    private const string DigitChars = "23456789";
    private const string SymbolChars = "!@#$%";

    private readonly UserManager<ApplicationUser> _userManager;

    public UserAccountService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ServiceResult<(string UserId, string TemporaryPassword)>> CreateUserAsync(
        string email, string fullName, string role, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return ServiceResult<(string, string)>.Failure("A user with this email already exists.");
        }

        var temporaryPassword = GenerateTemporaryPassword();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            return ServiceResult<(string, string)>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return ServiceResult<(string, string)>.Failure(roleResult.Errors.Select(e => e.Description).ToArray());
        }

        return ServiceResult<(string, string)>.Success((user.Id, temporaryPassword));
    }

    public async Task<ServiceResult> UpdateProfileAsync(string userId, string fullName, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Failure("User not found.");

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? ServiceResult.Success() : ServiceResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<ServiceResult> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Failure("User not found.");

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded ? ServiceResult.Success() : ServiceResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<ServiceResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Success(); // already gone

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded ? ServiceResult.Success() : ServiceResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<IReadOnlyList<string>> GetUserIdsInRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        return users.Select(u => u.Id).ToList();
    }

    private static string GenerateTemporaryPassword()
    {
        // Guarantees at least one lower/upper/digit/symbol character, satisfying
        // the Identity password policy configured in Program.cs, rather than
        // hoping a purely random draw happens to include all four categories.
        const string allChars = LowerChars + UpperChars + DigitChars + SymbolChars;
        const int length = 12;

        var chars = new char[length];
        chars[0] = PickRandom(LowerChars);
        chars[1] = PickRandom(UpperChars);
        chars[2] = PickRandom(DigitChars);
        chars[3] = PickRandom(SymbolChars);
        for (var i = 4; i < length; i++)
        {
            chars[i] = PickRandom(allChars);
        }

        // Fisher-Yates shuffle so the guaranteed categories aren't always in the first four positions.
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char PickRandom(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
