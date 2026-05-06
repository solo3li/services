using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Models;
using System.Security.Claims;

namespace ServicesApp.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminRolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminRolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return View(roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
            TempData["SuccessMessage"] = $"Role '{roleName}' created successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Permissions(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) return NotFound();

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var model = new RolePermissionsViewModel
        {
            RoleId = roleId,
            RoleName = role.Name ?? "",
            Permissions = AppPermissions.All.Select(p => new PermissionSelection
            {
                Name = p,
                IsSelected = existingClaims.Any(c => c.Type == "Permission" && c.Value == p)
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermissions(RolePermissionsViewModel model)
    {
        var role = await _roleManager.FindByIdAsync(model.RoleId);
        if (role == null) return NotFound();

        var claims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in claims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        foreach (var permission in model.Permissions.Where(p => p.IsSelected))
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission.Name));
        }

        TempData["SuccessMessage"] = "Permissions updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}

public class RolePermissionsViewModel
{
    public string RoleId { get; set; } = "";
    public string RoleName { get; set; } = "";
    public List<PermissionSelection> Permissions { get; set; } = new();
}

public class PermissionSelection
{
    public string Name { get; set; } = "";
    public bool IsSelected { get; set; }
}
