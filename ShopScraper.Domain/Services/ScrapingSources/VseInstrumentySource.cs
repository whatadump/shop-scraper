namespace ShopScraper.Domain.Services.ScrapingSources;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

public partial class VseInstrumentySource : IScrapingSource
{
    private const string PrimaryUrl = "https://www.vseinstrumenti.ru/search/?what=";
    
    public string SourceName => "ВсеИнструменты";
    
    private readonly ILogger<VseInstrumentySource> _logger;
    private readonly IBrowser _browser;
    private readonly DefaultParserOptions _options;

    public VseInstrumentySource(ILogger<VseInstrumentySource> logger, IBrowser browser, DefaultParserOptions options)
    {
        _logger = logger;
        _browser = browser;
        _options = options;
    }

    private async Task<IReadOnlyCollection<ScrapingResultDTO>> GetFromGrid(IEnumerable<IElement> elements)
    {
        return elements.Select(element =>
            {
                var priceString = element.QuerySelector("[data-qa='product-price-current']")?.Text().Replace(',', '.');
                var title = element.QuerySelector("[data-qa='product-name']")?.Text();
                var article = element.QuerySelector("[data-qa='product-code-text']")?.Text();
                var imageUrl = element.QuerySelector("img")?.GetAttribute("src");
                var originalUrl = element.QuerySelector("[data-qa='product-name']")?.GetAttribute("href");

                if (priceString is null || title is null || article is null || imageUrl is null ||
                    originalUrl is null)
                {
                    _logger.LogError("Один из распарсенных элементов оказался null");
                    return null;
                }
                
                var filteredPrice = NumbersRegex().Replace(priceString, "");
                if (filteredPrice.EndsWith('.'))
                {
                    filteredPrice += '0';
                }
                
                if (!decimal.TryParse(filteredPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    _logger.LogError("Неверный формат цены");
                    return null;
                }

                return new ScrapingResultDTO(title, imageUrl, price, originalUrl, NumbersRegex().Replace(article, ""), SourceName);
            })
            .Where(x => x is not null)
            .Take(_options.DefaultSearchResultTakePerPage)
            .ToArray()!;
    }
    
    private async Task<IReadOnlyCollection<ScrapingResultDTO>> GetFromTable(IEnumerable<IElement> elements)
    {
        return elements.Select(element =>
            {
                var priceString = element.QuerySelector("[data-qa='product-price-current']")?.Text().Replace(',', '.');
                var title = element.QuerySelector("[data-qa='product-name']")?.Text();
                var article = element.QuerySelector("[data-qa='product-code-text']")?.Text();
                var imageUrl = element.QuerySelector("img")?.GetAttribute("src");
                var originalUrl = element.QuerySelector("[data-qa='product-name']")?.GetAttribute("href");

                if (priceString is null || title is null || article is null || imageUrl is null ||
                    originalUrl is null)
                {
                    _logger.LogError("Один из распарсенных элементов оказался null");
                    return null;
                }

                var filteredPrice = NumbersRegex().Replace(priceString, "");
                if (filteredPrice.EndsWith('.'))
                {
                    filteredPrice += '0';
                }

                if (!decimal.TryParse(filteredPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    _logger.LogError("Неверный формат цены");
                    return null;
                }

                return new ScrapingResultDTO(title, imageUrl, price, originalUrl, NumbersRegex().Replace(article, ""), SourceName);
            })
            .Where(x => x is not null)
            .Take(_options.DefaultSearchResultTakePerPage)
            .ToArray()!;
    }

    public async Task<IReadOnlyCollection<ScrapingResultDTO>> ScrapeDataForRequest(ScrapingRequest request)
    {
        if (string.IsNullOrEmpty(request?.SearchQuery))
        {
            return [];
        }

        try
        {
            var page = await _browser.NewPageAsync();
            await page.GotoAsync($"{PrimaryUrl}{HttpUtility.UrlEncode(request.SearchQuery)}");
            await page.WaitForSelectorAsync("[data-qa='catalog-icon']");
            await Task.WhenAll(
                page.EvaluateAsync("""
                                   var i = 0; setInterval(() => {
                                       if (i < 20){
                                           window.scrollTo(0, i++ * document.body.scrollHeight / 100)
                                       }
                                   }, 5); 
                                   """),
                Task.Delay(TimeSpan.FromSeconds(2)));
            
            var htmlContent = await page.ContentAsync();
            await page.CloseAsync();

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(htmlContent));

            var grid = document.QuerySelectorAll("#product-listing-top [data-qa='products-tile']");
            if (grid.Length > 0)
            {
                _logger.LogInformation($"По запросу найдено {grid.Length} товаров");
                return await GetFromGrid(grid);
            }

            var table = document.QuerySelectorAll("#product-listing-top [data-qa='products-tile-horizontal']");
            if (table.Length > 0)
            {
                _logger.LogInformation($"По запросу найдено {grid.Length} товаров");
                return await GetFromTable(table);
            }

            return [];
        }
        catch (TimeoutException)
        {
            _logger.LogError("Не удалось открыть страницу");
            return [];
        }
    }

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex NumbersRegex();
}