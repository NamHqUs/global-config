using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130

public static class GlobalConfigIHostBuilderExtensions
{
    public static T ApplyTemplateConfiguration<T>(this T appBuilder, Action<HostBuilderContext, ConfigurationBuilder> config) where T : IHostBuilder
    {
        IConfigurationRoot templateConfig = default!;

        appBuilder.ConfigureAppConfiguration((hostingContext, builder) =>
        {
            var configurationBuilder = new ConfigurationBuilder();
            config(hostingContext, configurationBuilder);

            templateConfig = configurationBuilder.BuildTemplateConfig();
            builder.AddConfiguration(templateConfig);
        });

        appBuilder.ConfigureServices((hostingContext, services) =>
            services.AddSingleton<IConfiguration>(templateConfig)
        );

        return appBuilder;
    }

    public static T ApplyTemplateConfiguration<T>(this T appBuilder, Action<ConfigurationBuilder> config) where T : IHostBuilder
    {
        IConfigurationRoot templateConfig = default!;

        appBuilder.ConfigureAppConfiguration((hostingContext, builder) =>
        {
            var configurationBuilder = new ConfigurationBuilder();
            config(configurationBuilder);

            templateConfig = configurationBuilder.BuildTemplateConfig();
            builder.AddConfiguration(templateConfig);
        });

        appBuilder.ConfigureServices((hostingContext, services) =>
         services.AddSingleton<IConfiguration>(templateConfig)
        );

        return appBuilder;
    }
}
