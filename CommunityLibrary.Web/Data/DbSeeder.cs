using Bogus;
using CommunityLibrary.Web.Models;
using Microsoft.AspNetCore.Identity;

public static class DbSeeder
{
    private static readonly string[] Categories =
        { "Fiction","Non-Fiction","Science","History","Biography","Technology","Mystery","Fantasy" };

    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        const string adminEmail = "admin@library.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser
            { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin@1234!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (db.Books.Any()) return;

        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3).TrimEnd('.'))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
            .RuleFor(b => b.Category, f => f.PickRandom(Categories))
            .RuleFor(b => b.IsAvailable, _ => true);

        var books = bookFaker.Generate(20);
        db.Books.AddRange(books);
        await db.SaveChangesAsync();

        var memberFaker = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

        var members = memberFaker.Generate(10);
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var loans = new List<Loan>();

        for (int i = 0; i < 5; i++)
        {
            var start = now.AddDays(-Random.Shared.Next(30, 90));
            loans.Add(new Loan
            {
                BookId = books[i].Id,
                MemberId = members[i % 10].Id,
                LoanDate = start,
                DueDate = start.AddDays(14),
                ReturnedDate = start.AddDays(Random.Shared.Next(3, 13))
            });
        }
        for (int i = 5; i < 10; i++)
        {
            var start = now.AddDays(-Random.Shared.Next(1, 6));
            loans.Add(new Loan
            {
                BookId = books[i].Id,
                MemberId = members[i % 10].Id,
                LoanDate = start,
                DueDate = start.AddDays(14)
            });
            books[i].IsAvailable = false;
        }
        for (int i = 10; i < 15; i++)
        {
            var start = now.AddDays(-Random.Shared.Next(20, 40));
            loans.Add(new Loan
            {
                BookId = books[i].Id,
                MemberId = members[i % 10].Id,
                LoanDate = start,
                DueDate = start.AddDays(14) 
            });
            books[i].IsAvailable = false;
        }

        db.Loans.AddRange(loans);
        db.Books.UpdateRange(books);
        await db.SaveChangesAsync();
    }
}
