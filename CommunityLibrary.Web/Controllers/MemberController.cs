using CommunityLibrary.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityLibrary.Web.ViewModels;


public class MembersController : Controller
{
    private readonly ApplicationDbContext _db;
    public MembersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Members.ToListAsync());

    [HttpGet]
    public IActionResult Create() => View(new Member());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Members.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        return View(member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.Members.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
