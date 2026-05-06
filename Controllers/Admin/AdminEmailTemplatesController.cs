using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Data;
using ServicesApp.Models.Entities;

namespace ServicesApp.Controllers.Admin;

[Authorize(Policy = "Settings.Email")]
public class AdminEmailTemplatesController : Controller
{
    private readonly AppDbContext _db;

    public AdminEmailTemplatesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var templates = await _db.EmailTemplates.OrderBy(t => t.Name).ToListAsync();
        return View(templates);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var template = await _db.EmailTemplates.FindAsync(id);
        if (template == null) return NotFound();
        return View(template);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmailTemplate model)
    {
        if (ModelState.IsValid)
        {
            var template = await _db.EmailTemplates.FindAsync(model.Id);
            if (template == null) return NotFound();

            template.Subject = model.Subject;
            template.Body = model.Body;
            template.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Template '{template.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }
}
