namespace ShopScraper.Domain
{
    using HostedServices;
    using Infrastructure.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Playwright;
    using Redis.OM;
    using Redis.OM.Contracts;
    using Services;
    using Services.ScrapingSources;

    public static class Bundle
    {
        public static IServiceCollection UseDomainServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddHostedService<MigratorHostedService>();
            
            //Источники товаров
            services.AddSingleton<IScrapingSource, VseInstrumentySource>();
            services.AddSingleton<IScrapingSource, KuvaldaRuSource>();
            services.AddSingleton<IScrapingSource, MegastroySource>();
            
            
            services.AddSingleton<IScraper, Scraper>();
            services.AddSingleton<IBrowser>(_ =>
            {
                var playwrightTask = Playwright.CreateAsync();
                playwrightTask.Wait();
                var playwright = playwrightTask.Result;

                var browserTask = playwright.Chromium.LaunchAsync(new() { Headless = false });
                browserTask.Wait();
                return browserTask.Result;
            });

            services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>(p =>
                new RedisConnectionProvider(configuration.GetRequiredSection("Redis:ConnectionString").Value!));
            
            return services;
        } 
    }
}