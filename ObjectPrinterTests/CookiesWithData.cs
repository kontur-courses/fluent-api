using ObjectPrinterTests.Tests;

namespace ObjectPrinterTests;

public class CookiesWithData
{
    public Dictionary<string, string> cookie { get; set; } = new();

    public Dictionary<string, Person> data { get; set; } = new();


    public static CookiesWithData GetFakeCookiesWithData()
    {
        var cookies = new Dictionary<string, string>();
        cookies.Add("token", "token:key");
        var data = new Dictionary<string, Person> { { "adr", MockPerson.GetCoolProgramer } };

        var cookiesWithData = new CookiesWithData
        {
            cookie = cookies,
            data = data
        };

        return cookiesWithData;
    }
}