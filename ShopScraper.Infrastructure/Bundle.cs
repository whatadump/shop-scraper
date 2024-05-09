namespace ShopScraper.Infrastructure
{
    using Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Options;

    public static class Bundle
    {
        internal static IServiceCollection UseInfrastructureServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseLazyLoadingProxies()
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
                    options => options.MigrationsAssembly("ShopScraper.Migrations")));

            services.AddSingleton<DefaultParserOptions>(_ => new DefaultParserOptions()
            {
                DefaultSearchResultTakePerPage = 5
            });
            
            return services;
        }
    }
}