using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

internal class GetStoredTokenCommand(OauthClient client, StorageHelper storageHelper)
    : AsyncCommand<GetStoredTokenCommand.StoredSettings>
{
    public class StoredSettings : CommandSettings
    {
        [Description("Name of stored configuration")]
        [CommandArgument(0, "[name]")]
        public string? Name { get; set; }
        
        [Description("Custom audience")]
        [CommandOption("--audience|-a")]
        public string? Audience { get; set; }
        
        [Description("Prompt for custom audience for each regeneration")]
        [CommandOption("--prompt-audience|-P")]
        [DefaultValue(false)]
        public bool PromptAudience { get; set; }
        
        [Description("Silent mode (print only token)")]
        [CommandOption("--silent|-s")]
        [DefaultValue(false)]
        public bool Silent { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, StoredSettings settings)
    {
        TokenConfiguration? configuration = null;

        if (settings.Name is null)
        {
            var configurations = storageHelper.ListConfigurations();
            if (configurations.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No configurations found[/]");
                return 1;
            }

            var prompt = new SelectionPrompt<TokenConfiguration>().Title("Select configuration: ").PageSize(10)
                .MoreChoicesText("More").AddChoices(configurations);

            prompt.Converter = cfg => $"{cfg.Name} ({cfg.Environment}, {cfg.TokenServer})";

            var selection = AnsiConsole.Prompt(prompt);
            configuration = configurations.First(c =>
                c.Name.Equals(selection.Name, StringComparison.InvariantCultureIgnoreCase));
        }
        else
        {
            configuration = storageHelper.GetConfiguration(settings.Name);
        }

        if (configuration is null)
        {
            AnsiConsole.MarkupLine("[red]Configuration not found[/]");
            return 1;
        }

        string? customAudience = null;
        string? audience = null;

        if (!string.IsNullOrEmpty(configuration.Audience) && !configuration.Audience.Equals(".", StringComparison.InvariantCultureIgnoreCase))
        {
            audience = configuration.Audience;
        }
        
        if (settings.PromptAudience)
        {
            customAudience = AnsiConsole.Ask<string?>("Enter audience (optional):");
        }
        else if (settings.Audience is not null)
        {
            customAudience = settings.Audience;
        }
        
        
        var token = await client.GetTokenAsync(configuration.Environment, configuration.TokenServer,
            string.IsNullOrEmpty(customAudience) ? audience : customAudience,
            configuration.ClientId, configuration.ClientSecret, settings.Silent);
        
        if (token is not null)
            AnsiConsole.MarkupLine(settings.Silent ? token : $"[bold]Token:[/] {token}");

        if (!string.IsNullOrEmpty(token) && !settings.Silent)
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
}