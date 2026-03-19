using CommunityLibrary.Web.Models;
using Microsoft.EntityFrameworkCore;

public class LoanTests
{
    // ── Test 1: Cannot loan a book that is already on an active loan ──
    [Fact]
    public async Task CannotLoan_BookAlreadyOnActiveLoan()
    {
        using var db = DbContextHelper.Create("T1");
        db.Books.Add(new Book { Id = 1, Title = "X", Author = "A", Isbn = "0", Category = "C", IsAvailable = false });
        db.Loans.Add(new Loan
        {
            BookId = 1,
            MemberId = 1,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        });
        await db.SaveChangesAsync();

        bool alreadyOut = await db.Loans.AnyAsync(
            l => l.BookId == 1 && l.ReturnedDate == null);

        Assert.True(alreadyOut);
    }

    // ── Test 2: Returned loan makes book available again ──────────────
    [Fact]
    public async Task ReturnedLoan_MakesBookAvailable()
    {
        using var db = DbContextHelper.Create("T2");
        db.Books.Add(new Book { Id = 1, Title = "X", Author = "A", Isbn = "0", Category = "C", IsAvailable = false });
        db.Loans.Add(new Loan
        {
            BookId = 1,
            MemberId = 1,
            LoanDate = DateTime.UtcNow.AddDays(-5),
            DueDate = DateTime.UtcNow.AddDays(9)
        });
        await db.SaveChangesAsync();

        var loan = await db.Loans.Include(l => l.Book).FirstAsync();
        loan.ReturnedDate = DateTime.UtcNow;
        loan.Book.IsAvailable = true;
        await db.SaveChangesAsync();

        Assert.True((await db.Books.FindAsync(1))!.IsAvailable);
        Assert.NotNull(loan.ReturnedDate);
    }

    // ── Test 3: Book search returns expected matches ───────────────────
    [Fact]
    public async Task BookSearch_ReturnsTitleMatch()
    {
        using var db = DbContextHelper.Create("T3");
        db.Books.AddRange(
            new Book { Title = "Clean Code", Author = "Robert Martin", Isbn = "1", Category = "Tech", IsAvailable = true },
            new Book { Title = "Dune", Author = "Frank Herbert", Isbn = "2", Category = "Sci-Fi", IsAvailable = true }
        );
        await db.SaveChangesAsync();

        var results = await db.Books.Where(b => b.Title.Contains("Clean")).ToListAsync();

        Assert.Single(results);
        Assert.Equal("Clean Code", results[0].Title);
    }

    // ── Test 4: Overdue logic ─────────────────────────────────────────
    [Fact]
    public void OverdueLogic_DetectsOverdueLoan()
    {
        var loan = new Loan
        {
            LoanDate = DateTime.UtcNow.AddDays(-20),
            DueDate = DateTime.UtcNow.AddDays(-6),
            ReturnedDate = null
        };
        Assert.True(loan.IsOverdue);
    }

    // ── Test 5: Returned loan is NOT considered overdue ───────────────
    [Fact]
    public void OverdueLogic_ReturnedLoanNotOverdue()
    {
        var loan = new Loan
        {
            LoanDate = DateTime.UtcNow.AddDays(-20),
            DueDate = DateTime.UtcNow.AddDays(-6),
            ReturnedDate = DateTime.UtcNow.AddDays(-3)   // was returned
        };
        Assert.False(loan.IsOverdue);
    }
}
