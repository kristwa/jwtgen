using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

internal class DeleteTokenCommand(StorageHelper storageHelper) : Command<DeleteTokenCommand.DeleteSettings>
{
    public class DeleteSettings : CommandSettings
    {
        [Description("Name of stored configuration")]
        [CommandArgument(0, "[name]")]
        public string? Name { get; set; }
    }

    public override int Execute(CommandContext context, DeleteSettings settings)
    {
        if (settings.Name is null)
        {
            var configurations = storageHelper.ListConfigurations();
            if (configurations.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No configurations found[/]");
                return 1;
            }

            var prompt = new SelectionPrompt<TokenConfiguration>().Title("Select configuration to delete: ").PageSize(10)
                .MoreChoicesText("More").AddChoices(configurations);

            prompt.Converter = cfg => $"{cfg.Name} ({cfg.Environment}, {cfg.TokenServer})";

            var selection = AnsiConsole.Prompt(prompt);
            settings.Name = selection.Name;
        }

        if (storageHelper.Delete(settings.Name))
        {
            AnsiConsole.MarkupLine($"[bold]Configuration '{settings.Name}' deleted[/]");
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]Configuration '{settings.Name}' not found[/]");
            return 1;
        }
    }
}