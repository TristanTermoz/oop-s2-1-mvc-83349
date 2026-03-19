using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLibrary.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
        => _roleManager = roleManager;

    // GET /Admin/Roles
    public IActionResult Index()
        => View(_roleManager.Roles.ToList());

    // POST /Admin/Roles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName))
            await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        return RedirectToAction("Index");
    }

    // POST /Admin/Roles/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role != null) await _roleManager.DeleteAsync(role);
        return RedirectToAction("Index");
    }
}

