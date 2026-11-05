using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace EmployeeManagementSystem.Tests;

public class AppFactory : WebApplicationFactory<Program>
{
    public const string AdminPassword = "Disposable-admin-password-2026";
    public const string ViewerPassword = "Disposable-viewer-password-2026";
    private readonly string _databaseFile = Path.Combine(Path.GetTempPath(), $"employeehub-{Guid.NewGuid():N}.db");

    public AppFactory() : this(demo: false) { }

    protected AppFactory(bool demo)
    {
        // The app reads these while it is being built, so they must exist before it starts.
        var sqlServer = Environment.GetEnvironmentVariable("TEST_SQLSERVER");
        Environment.SetEnvironmentVariable("DatabaseProvider", sqlServer is null ? "Sqlite" : "SqlServer");
        Environment.SetEnvironmentVariable("ConnectionStrings__Sqlite", $"Data Source={_databaseFile}");
        if (sqlServer is not null)
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"{sqlServer};Database=eh_{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable("Auth__AdminPassword", AdminPassword);
        Environment.SetEnvironmentVariable("Auth__ViewerPassword", ViewerPassword);
        Environment.SetEnvironmentVariable("Demo__Enabled", demo ? "true" : "false");
        Environment.SetEnvironmentVariable("RateLimits__Login", "1000");
        Environment.SetEnvironmentVariable("RateLimits__Writes", "1000");
    }

    public HttpClient Browser() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        File.Delete(_databaseFile);
    }
}

public class DemoAppFactory : AppFactory
{
    public DemoAppFactory() : base(demo: true) { }
}
