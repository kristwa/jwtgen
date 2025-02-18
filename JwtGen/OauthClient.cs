using System.Net.Http.Json;
using Spectre.Console;

internal class OauthClient(HttpClient client)
{
    private Dictionary<(Environment, TokenServer), string> _tokenServers = new()
    {
        [(Environment.Test, TokenServer.SoaInt)] =
            "https://testsoa-int.mistraltest.mistralnett.test/mga/sps/oauth/oauth20/token",
        [(Environment.Test, TokenServer.SoaExt)] =
            "https://testsoa-ext.mistraltest.mistralnett.test/mga/sps/oauth/oauth20/token",
        [(Environment.Test, TokenServer.Intra)] = "https://intra.mistraltest.mistralnett.test/mga/sps/oauth/oauth20/token",
        [(Environment.Production, TokenServer.SoaInt)] =
            "https://prodsoa-int.mistral.mistralnett.com/mga/sps/oauth/oauth20/token",
        [(Environment.Production, TokenServer.SoaExt)] =
            "https://prodsoa-ext.mistral.mistralnett.com/mga/sps/oauth/oauth20/token",
        [(Environment.Production, TokenServer.Intra)] =
            "https://intra.mistral.mistralnett.com/mga/sps/oauth/oauth20/token"
    };

    public async Task<string?> GetTokenAsync(Environment environment, TokenServer tokenServer, string? audience,
        string clientId, string clientSecret, bool silentMode = false)
    {
        var url = _tokenServers[(environment, tokenServer)];

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
        };

        if (audience is not null)
        {
            form.Add("scope", audience);
        }
        
        if (!silentMode)
            AnsiConsole.MarkupLine("Requesting token from: [cyan]{0}[/] (client id: [magenta]{1}[/], scope: [magenta]{2}[/])", url, clientId, audience ?? "<none>");
        
        var response = await client.PostAsync(url, new FormUrlEncodedContent(form));

        try
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine("Error: [red]{0}[/]", content);
                return null;
            }
            
            var token = await response.Content.ReadFromJsonAsync<TokenResult>();

            if (token is not null)
                return token.AccessToken;
        }
        catch (HttpRequestException e)
        {
            AnsiConsole.MarkupLine("Error: [red]{0}[/]", e.Message);
        }

        return null;
    }
}