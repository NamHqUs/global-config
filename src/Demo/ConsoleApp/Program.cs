using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .ApplyTemplateConfiguration(config =>
        {
            config.AddJsonFile("GlobalSettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
    .ConfigureServices((context, services) =>
        {
            services.AddSingleton<Demo>();
        });

IHost app = builder.Build();

var demo = app.Services.GetRequiredService<Demo>();
demo.Run();

await app.RunAsync();
