using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

internal class CreateTokenCommand(OauthClient client, StorageHelper storageHelper)
    : AsyncCommand<CreateTokenCommand.CreateSettings>
{
    public class CreateSettings : CommandSettings
    {
        [Description("Environment")]
        [CommandOption("--environment|-e")]
        public string? Environment { get; set; }

        [Description("Token server")]
        [CommandOption("--token-server|-t")]
        public string? TokenServer { get; set; }

        [Description("Audience (optional)")]
        [CommandOption("--audience|-a")]
        public string? Audience { get; set; }

        [Description("Client ID")]
        [CommandOption("--client-id|-c")]
        public string? ClientId { get; set; }

        [Description("Client secret")]
        [CommandOption("--client-secret|-s")]
        public string? ClientSecret { get; set; }

        [Description("Save information for re-use")]
        [CommandOption("--save|-S")]
        [DefaultValue(false)]
        public bool Save { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CreateSettings settings)
    {
        var tokenServer = ToTokenServer(settings.TokenServer);
        var environment = ToEnvironment(settings.Environment);

        var promptAudience = tokenServer is null || environment is null;

        if (tokenServer is null)
        {
            tokenServer = AnsiConsole.Prompt<TokenServer>(new SelectionPrompt<TokenServer>()
                .AddChoices(TokenServer.SoaInt, TokenServer.SoaExt, TokenServer.Intra)
                .Title("Select token server: "));
            // tokenServer = ToTokenServer(input);
        }

        AnsiConsole.MarkupLine($"[bold]Token server:[/] {tokenServer}");

        if (environment is null)
        {
            environment = AnsiConsole.Prompt(new SelectionPrompt<Environment>()
                .AddChoices(Environment.Test, Environment.Production).Title("Select environment: "));
        }

        AnsiConsole.MarkupLine($"[bold]Environment:[/] {environment}");


        if (settings.ClientId is null)
        {
            settings.ClientId = AnsiConsole.Prompt(new TextPrompt<string>("Enter client ID:"));
        }

        if (settings.ClientSecret is null)
        {
            settings.ClientSecret = AnsiConsole.Prompt(new TextPrompt<string>("Enter client secret:"));
        }

        if (promptAudience)
        {
            settings.Audience =
                AnsiConsole.Prompt(new TextPrompt<string?>("Enter audience (optional):").AllowEmpty());
        }

        if (settings.Save)
        {
            var name = AnsiConsole.Ask<string>("Enter title for this configuration:");
            if (storageHelper.Exists(name))
            {
                AnsiConsole.MarkupLine($"[red]Configuration with name '{name}' already exists[/]");
                return 1;
            }
            else
            {
                storageHelper.Save(name, tokenServer.Value, environment.Value, settings.ClientId,
                    settings.ClientSecret,
                    settings.Audience);
            }
        }

        var token = await client.GetTokenAsync(environment.Value, tokenServer.Value, settings.Audience,
            settings.ClientId, settings.ClientSecret);
        AnsiConsole.MarkupLine($"[bold]Token:[/] {token}");

        if (!string.IsNullOrEmpty(token))
        {
            var copyToClipboard = AnsiConsole.Confirm("Copy token to clipboard?", true);
            if (copyToClipboard)
            {
                await ClipboardService.SetTextAsync(token);
                AnsiConsole.MarkupLine($"[green]Token copied to clipboard[/]");

            }
        }

        

        return 0;
    }

    private TokenServer? ToTokenServer(string? input)
    {
        return input?.ToLower() switch
        {
            "soa-int" or "soaint" or "int" => TokenServer.SoaInt,
            "soa-ext" or "soaext" or "ext" => TokenServer.SoaExt,
            "intra" => TokenServer.Intra,
            _ => null
        };
    }

    private Environment? ToEnvironment(string? input)
    {
        return input?.ToLower() switch
        {
            "test" or "t" => Environment.Test,
            "production" or "prod" or "p" => Environment.Production,
            _ => null
        };
    }
}