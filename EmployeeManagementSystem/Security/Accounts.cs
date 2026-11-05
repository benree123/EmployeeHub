using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeManagementSystem.Security
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";
        public const string DemoGuest = "DemoGuest";
        public const string CanEditPolicy = "CanEdit";
    }

    public static class UserExtensions
    {
        // Viewers can search the directory, but cannot change records or see salaries.
        public static bool CanEdit(this ClaimsPrincipal user) => user.IsInRole(Roles.Admin) || user.IsInRole(Roles.DemoGuest);
    }

    public sealed class Accounts
    {
        private readonly Dictionary<string, (string Role, byte[] Hash)> _users;

        private Accounts(string adminPassword, string viewerPassword)
        {
            _users = new(StringComparer.Ordinal)
            {
                ["admin"] = (Roles.Admin, Hash(adminPassword)),
                ["viewer"] = (Roles.Viewer, Hash(viewerPassword))
            };
        }

        public static Accounts Load(IConfiguration configuration, string dataDirectory)
        {
            var admin = configuration["Auth:AdminPassword"];
            var viewer = configuration["Auth:ViewerPassword"];
            if (string.IsNullOrEmpty(admin) || string.IsNullOrEmpty(viewer))
            {
                // Local convenience: generated once and excluded from Git.
                var file = Path.Combine(dataDirectory, "local-credentials.txt");
                if (!File.Exists(file))
                    File.WriteAllLines(file, [$"admin={Generate()}", $"viewer={Generate()}"]);
                var values = File.ReadAllLines(file).Select(x => x.Split('=', 2)).ToDictionary(x => x[0], x => x[1]);
                admin = string.IsNullOrEmpty(admin) ? values["admin"] : admin;
                viewer = string.IsNullOrEmpty(viewer) ? values["viewer"] : viewer;
            }
            if (admin.Length < 12 || viewer.Length < 12)
                throw new InvalidOperationException("Account passwords must have at least 12 characters.");
            return new Accounts(admin, viewer);
        }

        public string? Validate(string? username, string? password)
        {
            var supplied = Hash(password ?? "");
            var known = _users.TryGetValue(username ?? "", out var user);
            var expected = known ? user.Hash : new byte[supplied.Length];
            return CryptographicOperations.FixedTimeEquals(supplied, expected) && known ? user.Role : null;
        }

        private static string Generate() => Convert.ToHexString(RandomNumberGenerator.GetBytes(20));
        private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
}
