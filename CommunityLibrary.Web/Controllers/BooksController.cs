using CommunityLibrary.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunityLibrary.Web.ViewModels;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;

    public BooksController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
    string? searchTerm, string? category, string? availability)
    {
        // Start with IQueryable — NO .ToList() here
        IQueryable<Book> query = _db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(b =>
                b.Title.Contains(searchTerm) ||
                b.Author.Contains(searchTerm));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(b => b.Category == category);

        if (availability == "Available")
            query = query.Where(b => b.IsAvailable);
        else if (availability == "OnLoan")
            query = query.Where(b => !b.IsAvailable);

        // Only materialise here at the end
        var vm = new BookSearchViewModel
        {
            SearchTerm = searchTerm,
            Category = category,
            Availability = availability,
            Books = await query.OrderBy(b => b.Title).ToListAsync(),
            Categories = await _db.Books
                               .Select(b => b.Category)
                               .Distinct()
                               .OrderBy(c => c)
                               .ToListAsync()
        };
        return View(vm);
    }

}