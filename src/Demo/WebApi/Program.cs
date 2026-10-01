var builder = WebApplication.CreateBuilder(args)
    .ApplyTemplateConfiguration(config =>
        {
            config.AddJsonFile("GlobalSettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        });

var app = builder.Build();

app.MapGet("/", (IConfiguration config) => {
    return $"""
    Hello, World from WebApi!
    {"-".PadLeft(50, '-')}
    Configurations:
        Authentication: 
            Authority: {config["Authentication:Authority"]}
            ClientId: {config["Authentication:ClientId"]}
            ClientSecret: {config["Authentication:ClientSecret"]}
            Scopes: [{string.Join(", ", config.Get<string[]>("Authentication:Scopes")!)}]
    """;
    });

app.Run();

