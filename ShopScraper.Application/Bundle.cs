namespace ShopScraper.Application
{
    using Interfaces;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Playwright;
    using Options;
    using Services;
    using Services.ScrapingSources;

    public static class Bundle
    {
        public static IServiceCollection UseDomainServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            
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
            
            services.AddSingleton<DefaultParserOptions>(_ => new DefaultParserOptions()
            {
                DefaultSearchResultTakePerPage = 5
            });
            
            return services;
        } 
    }
}