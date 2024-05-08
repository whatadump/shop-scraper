namespace ShopScraper.Domain
{
    using HostedServices;
    using Infrastructure.Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Playwright;
    using Services;
    using Services.ScrapingSources;

    public static class Bundle
    {
        public static IServiceCollection UseDomainServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddHostedService<MigratorHostedService>();
            services.AddSingleton<IScrapingSource, VseInstrumentySource>();
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
            
            services.AddSingleton<Infrastructure>()
            
            return services;
        } 
    }
}