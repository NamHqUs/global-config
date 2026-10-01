var builder = WebApplication.CreateBuilder(args)
    .ApplyTemplateConfiguration(config =>
        {
            config.AddJsonFile("GlobalSettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
        });

var app = builder.Build();

app.MapGet("/", (IConfiguration config) => $"""
    Hello, World from IdProvider!
    Configurations:
    {"-".PadLeft(50, '-')}
        Console: 
            Authority: {config["ConsoleApp:Authority"]}
            ClientId: {config["ConsoleApp:ClientId"]}
            ClientSecret: {config["ConsoleApp:ClientSecret"]}
            Scopes: [{string.Join(", ", config.Get<string[]>("ConsoleApp:Scopes")!)}]
        WebApi: 
            Authority: {config["WebApi:Authority"]}
            ClientId: {config["WebApi:ClientId"]}
            ClientSecret: {config["WebApi:ClientSecret"]}
            Scopes: [{string.Join(", ", config.Get<string[]>("WebApi:Scopes")!)}]
    {"-".PadLeft(50, '-')}
""");

app.Run();
