using System.Net;
using System.Text.RegularExpressions;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EmployeeManagementSystem.Tests;

public class EmployeeTests : IClassFixture<AppFactory>
{
    private readonly AppFactory _factory;

    public EmployeeTests(AppFactory factory) => _factory = factory;

    private async Task<HttpClient> Admin()
    {
        var client = _factory.Browser();
        await client.SignIn("admin", AppFactory.AdminPassword);
        return client;
    }

    private async Task<HttpClient> Viewer()
    {
        var client = _factory.Browser();
        await client.SignIn("viewer", AppFactory.ViewerPassword);
        return client;
    }

    private async Task<int> IdOf(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.Employees.SingleAsync(e => e.Email == email)).Id;
    }

    [Fact]
    public async Task Home_shows_real_totals_but_no_records_to_visitors()
    {
        var html = await _factory.Browser().GetStringAsync("/");
        Assert.DoesNotContain("Secure Database", html);
        Assert.DoesNotContain("Department Management", html);
        Assert.DoesNotContain("john.carter", html);
        Assert.DoesNotContain("Rachel Lee", html);
        Assert.Contains("Sign in to see recent employee records", html);

        using var scope = _factory.Services.CreateScope();
        var count = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Employees.CountAsync();
        Assert.Contains($"<strong>{count}</strong>", html);
    }

    [Theory]
    [InlineData("/Employees")]
    [InlineData("/Employees/Details/1")]
    [InlineData("/Employees/Create")]
    public async Task Pages_require_sign_in(string path)
    {
        var response = await _factory.Browser().GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        var response = await _factory.Browser().Submit("/Account/Login", "/Account/Login", ("username", "admin"), ("password", "wrong-password"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Username or password is incorrect", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task External_return_urls_are_ignored()
    {
        var response = await _factory.Browser().Submit("/Account/Login", "/Account/Login", ("username", "admin"), ("password", AppFactory.AdminPassword), ("returnUrl", "https://evil.example/"));
        Assert.Equal("/Employees", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Viewer_cannot_see_salaries_or_change_records()
    {
        var viewer = await Viewer();
        var html = await viewer.GetStringAsync("/Employees?sort=salary");
        Assert.DoesNotContain("salary-cell", html);
        Assert.DoesNotContain("Salary", html);
        Assert.DoesNotContain("/Employees/Create", html);

        foreach (var path in new[] { "/Employees/Create", "/Employees/Edit/1", "/Employees/Delete/1" })
        {
            var response = await viewer.GetAsync(path);
            Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
        }

        var post = await viewer.PostAsync("/Employees/Delete/1", new FormUrlEncodedContent([new("id", "1")]));
        Assert.Contains("/Account/AccessDenied", post.Headers.Location!.ToString());
        Assert.DoesNotContain("Salary", await viewer.GetStringAsync("/Employees/Details/1"));
    }

    [Fact]
    public async Task Admin_can_create_edit_and_delete()
    {
        var admin = await Admin();
        var created = await admin.Submit("/Employees/Create", "/Employees/Create", Helpers.Employee("  Lifecycle@Example.com "));
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        var id = await IdOf("lifecycle@example.com");

        var fields = Helpers.Employee("lifecycle2@example.com", id: id.ToString());
        var edited = await admin.Submit($"/Employees/Edit/{id}", $"/Employees/Edit/{id}", fields);
        Assert.Equal(HttpStatusCode.Redirect, edited.StatusCode);
        Assert.Contains("lifecycle2@example.com", await admin.GetStringAsync("/Employees?searchString=lifecycle2"));

        var deleted = await admin.Submit($"/Employees/Delete/{id}", $"/Employees/Delete/{id}", ("id", id.ToString()));
        Assert.Equal(HttpStatusCode.Redirect, deleted.StatusCode);
        Assert.DoesNotContain("lifecycle2@example.com", await admin.GetStringAsync("/Employees?searchString=lifecycle2"));
    }

    [Fact]
    public async Task Duplicate_email_is_rejected_ignoring_case()
    {
        var admin = await Admin();
        var response = await admin.Submit("/Employees/Create", "/Employees/Create", Helpers.Employee("JOHN.CARTER@example.com"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("already uses this email", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Saving_an_employee_with_their_own_email_is_allowed()
    {
        var admin = await Admin();
        var id = await IdOf("david.clark@example.com");
        var response = await admin.Submit($"/Employees/Edit/{id}", $"/Employees/Edit/{id}", Helpers.Employee("david.clark@example.com", id: id.ToString()));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Future_hire_date_is_rejected()
    {
        var admin = await Admin();
        var future = DateTime.UtcNow.AddDays(10).ToString("yyyy-MM-dd");
        var response = await admin.Submit("/Employees/Create", "/Employees/Create", Helpers.Employee("future@example.com", future));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("cannot be in the future", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Overlong_name_is_rejected()
    {
        var admin = await Admin();
        var fields = Helpers.Employee("long@example.com");
        fields[0] = ("FirstName", new string('a', 101));
        var response = await admin.Submit("/Employees/Create", "/Employees/Create", fields);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Search_ignores_case_and_blank_search_shows_no_clear_button()
    {
        var admin = await Admin();
        var html = await admin.GetStringAsync("/Employees?searchString=FINANCE");
        Assert.Contains("emily.davis@example.com", html);
        Assert.DoesNotContain("john.carter@example.com", html);

        Assert.DoesNotContain(">\n                    Clear\n", (await admin.GetStringAsync("/Employees?searchString=%20")).ReplaceLineEndings("\n"));
    }

    [Fact]
    public async Task Directory_pages_and_sorts()
    {
        var admin = await Admin();
        var first = await admin.GetStringAsync("/Employees?sort=salary");
        Assert.Contains("Page 1 of", first);
        Assert.Contains(">Next</a>", first);
        Assert.Equal(10, Regex.Matches(first, "class=\"salary-cell\"").Count);
        Assert.Contains("$58,000.00", first);
        Assert.True(first.IndexOf("daniel.wilson@example.com") < first.IndexOf("rachel.lee@example.com"), "Lowest salary should be first.");

        var descending = await admin.GetStringAsync("/Employees?sort=salary&desc=true");
        Assert.True(descending.IndexOf("alex.morgan@example.com") < descending.IndexOf("olivia.brown@example.com"), "Highest salary should be first.");

        var second = await admin.GetStringAsync("/Employees?sort=salary&page=2");
        Assert.Contains("Page 2 of", second);
        Assert.Contains(">Previous</a>", second);
        Assert.Contains("Page 2 of", await admin.GetStringAsync("/Employees?sort=salary&page=999"));
    }

    [Fact]
    public async Task Demo_is_off_by_default()
    {
        var response = await _factory.Browser().Submit("/Account/Login", "/Account/Demo");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Try the demo", await _factory.Browser().GetStringAsync("/"));
    }
}

public class DemoTests : IClassFixture<DemoAppFactory>
{
    private readonly DemoAppFactory _factory;

    public DemoTests(DemoAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Guest_can_try_the_app_and_reset_restores_the_samples()
    {
        var guest = _factory.Browser();
        var response = await guest.Submit("/Account/Login", "/Account/Demo");
        Assert.Equal("/Employees", response.Headers.Location!.ToString());
        Assert.Contains("Shared demo", await guest.GetStringAsync("/Employees"));

        var created = await guest.Submit("/Employees/Create", "/Employees/Create", Helpers.Employee("guest@example.com"));
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Reset(db);
        Assert.Equal(12, await db.Employees.CountAsync());
        Assert.False(await db.Employees.AnyAsync(e => e.Email == "guest@example.com"));
    }
}
