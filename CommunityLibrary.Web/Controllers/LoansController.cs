using CommunityLibrary.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CommunityLibrary.Web.ViewModels;

public class LoansController : Controller
{
    private readonly ApplicationDbContext _db;

    public LoansController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var loans = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        return View(loans);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Books = await _db.Books.Where(b => b.IsAvailable)
                              .Select(b => new SelectListItem(b.Title, b.Id.ToString()))
                              .ToListAsync();
        ViewBag.Members = await _db.Members
                              .Select(m => new SelectListItem(m.FullName, m.Id.ToString()))
                              .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Loan model)
    {
        // Re-check business rule inside a transaction to reduce race conditions
        using var tx = await _db.Database.BeginTransactionAsync();
        bool alreadyOnLoan = await _db.Loans.AnyAsync(
            l => l.BookId == model.BookId && l.ReturnedDate == null);

        if (alreadyOnLoan)
        {
            ModelState.AddModelError("", "This book is already on an active loan.");
            // Re-populate dropdowns before returning view
            ViewBag.Books = await _db.Books.Where(b => b.IsAvailable)
                                  .Select(b => new SelectListItem(b.Title, b.Id.ToString()))
                                  .ToListAsync();
            ViewBag.Members = await _db.Members
                                  .Select(m => new SelectListItem(m.FullName, m.Id.ToString()))
                                  .ToListAsync();
            return View(model);
        }

        model.LoanDate = DateTime.UtcNow;
        model.DueDate = DateTime.UtcNow.AddDays(14);
        _db.Loans.Add(model);

        // Mark book unavailable
        var book = await _db.Books.FindAsync(model.BookId);
        if (book == null) return NotFound();
        book.IsAvailable = false;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkReturned(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null) return NotFound();
        if (loan.ReturnedDate != null) return RedirectToAction("Index"); // already returned

        loan.ReturnedDate = DateTime.UtcNow;
        loan.Book.IsAvailable = true;

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }


}
