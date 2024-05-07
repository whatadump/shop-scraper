namespace ShopScraper.Domain
{
    using HostedServices;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class Bundle
    {
        public static IServiceCollection UseDomainServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddHostedService<MigratorHostedService>();
            
            
            return services;
        } 
    }
}