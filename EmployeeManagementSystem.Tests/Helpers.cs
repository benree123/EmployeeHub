using System.Net;
using System.Text.RegularExpressions;

namespace EmployeeManagementSystem.Tests;

public static partial class Helpers
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();

    public static async Task<HttpResponseMessage> Submit(this HttpClient client, string formPage, string action, params (string Key, string Value)[] fields)
    {
        var page = await client.GetStringAsync(formPage);
        var values = fields.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList();
        values.Add(new("__RequestVerificationToken", TokenPattern().Match(page).Groups[1].Value));
        return await client.PostAsync(action, new FormUrlEncodedContent(values));
    }

    public static async Task SignIn(this HttpClient client, string username, string password)
    {
        var response = await client.Submit("/Account/Login", "/Account/Login", ("username", username), ("password", password));
        Xunit.Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    public static (string, string)[] Employee(string email, string hireDate = "2025-01-15", string? id = null)
    {
        var fields = new List<(string, string)>
        {
            ("FirstName", "Test"), ("LastName", "Person"), ("Email", email), ("Department", "Quality"),
            ("Position", "Tester"), ("Salary", "50000"), ("HireDate", hireDate)
        };
        if (id is not null) fields.Add(("Id", id));
        return fields.ToArray();
    }
}
