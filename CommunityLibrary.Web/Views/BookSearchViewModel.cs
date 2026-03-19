using CommunityLibrary.Web.Models;

namespace CommunityLibrary.Web.ViewModels;


public class BookSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Availability { get; set; }  // null / Available / OnLoan

    public List<Book> Books { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}
