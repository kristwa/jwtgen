using Spectre.Console;
using Spectre.Console.Cli;

internal class ListTokensCommand(StorageHelper storageHelper) : Command
{
    public override int Execute(CommandContext context)
    {
        var configurations = storageHelper.ListConfigurations();
        if (configurations.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No configurations found[/]");
            return 1;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Name");
        table.AddColumn("Environment");
        table.AddColumn("Token server");
        table.AddColumn("Client ID");
        table.AddColumn("Audience");

        foreach (var configuration in configurations)
        {
            table.AddRow(configuration.Name, configuration.Environment.ToString(), configuration.TokenServer.ToString(), configuration.ClientId,
                configuration.Audience);
        }

        AnsiConsole.Render(table);
        return 0;
    }
}