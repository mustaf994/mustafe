using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

/// <summary>
/// Every controller in the Officer area inherits this base, so the
/// Examination-Officer-only restriction is enforced once, at compile time.
/// </summary>
[Area("Officer")]
[Authorize(Policy = "RequireExaminationOfficer")]
public abstract class OfficerControllerBase : Controller
{
}
