using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.Hosting;
#pragma warning restore IDE0130

public static class GlobalConfigExtensions
{
    public static T ApplyTemplateConfiguration<T>(this T appBuilder, Action<ConfigurationBuilder> config) where T : IHostApplicationBuilder
    {
        var configurationBuilder = new ConfigurationBuilder();
        config(configurationBuilder);

        var templateConfig = configurationBuilder.BuildTemplateConfig();

        appBuilder.Configuration.AddConfiguration(templateConfig);
        appBuilder.Services.AddSingleton<IConfiguration>(templateConfig);

        return appBuilder;
    }
}
