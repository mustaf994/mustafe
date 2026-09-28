using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReExamManagementSystem.Web.Areas.Student.Controllers;

/// <summary>
/// Every controller in the Student area inherits this base, so the
/// Student-only restriction is enforced once, at compile time. Named
/// "StudentAreaControllerBase" (not "StudentControllerBase") to avoid
/// colliding with the Domain.Entities.Student entity by name alone.
/// </summary>
[Area("Student")]
[Authorize(Policy = "RequireStudent")]
public abstract class StudentAreaControllerBase : Controller
{
}
