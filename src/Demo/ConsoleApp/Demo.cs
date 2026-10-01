using Microsoft.Extensions.Configuration;

class Demo(IConfiguration config)
{
    public void Run()
    {
        try
        {

            Console.WriteLine($"""
                {"-".PadLeft(100, '-')}
                Authentication: 
                    Authority: {config["Authentication:Authority"]}
                    ClientId: {config["Authentication:ClientId"]}
                    ClientSecret: {config["Authentication:ClientSecret"]}
                    Scopes: [{string.Join(", ", config.Get<string[]>("Authentication:Scopes")!)}]

                WebApi: 
                    Url: {config["WebApi:Url"]}
                {"-".PadLeft(100, '-')}
                """);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}