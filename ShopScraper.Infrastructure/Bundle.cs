namespace ShopScraper.Infrastructure
{
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class Bundle
    {
        internal static IServiceCollection UseInfrastructureServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseLazyLoadingProxies()
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
                    options => options.MigrationsAssembly("ShopScraper.Migrations")));
            
            return services;
        }
    }
}