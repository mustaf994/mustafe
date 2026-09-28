using Microsoft.AspNetCore.Identity;

namespace ReExamManagementSystem.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName) : base(roleName) { }

    public string? Description { get; set; }
}
