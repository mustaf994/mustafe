using Microsoft.AspNetCore.Identity;

namespace ReExamManagementSystem.Infrastructure.Identity;

/// <summary>
/// Extends the framework's Identity user with the profile fields the system
/// needs for every account (admin, examination officer or student). Kept in
/// Infrastructure - not Domain - because it inherits from IdentityUser, an
/// ASP.NET Core Identity type the Domain layer must not depend on. Domain
/// entities that belong to a user (e.g. Student) reference it only by string
/// Id, never by navigation property.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfilePicturePath { get; set; }
}
