using Injection.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;


var services = new ServiceCollection();
services.AddHttpClient();
services.AddTransient<OauthClient>();
services.AddSingleton<StorageHelper>();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config =>
{
    config.AddCommand<CreateTokenCommand>("new");
    config.AddCommand<GetStoredTokenCommand>("get");
    config.AddCommand<DeleteTokenCommand>("delete");
    config.AddCommand<ListTokensCommand>("list");
    config.PropagateExceptions();
});
return app.Run(args);