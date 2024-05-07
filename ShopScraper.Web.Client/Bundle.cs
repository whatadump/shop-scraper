namespace ShopScraper.Web.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services;

public static class Bundle
{
    public static IServiceCollection UseInteractiveApplication(this IServiceCollection services, IConfigurationRoot config)
    {
        services.AddScoped<UserManager>();
        
        return services;
    }
}