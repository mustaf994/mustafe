using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Common;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

/// <summary>
/// Every controller in the Admin area inherits this base. The Examination
/// Officer role is granted the same access as Administrator here, since the
/// office should be able to manage everything an admin can except staff
/// accounts - UserController overrides this with an Administrator-only
/// policy to carve out that one exception.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "RequireStaff")]
public abstract class AdminControllerBase : Controller
{
}
