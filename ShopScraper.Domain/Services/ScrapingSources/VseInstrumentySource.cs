namespace ShopScraper.Domain.Services.ScrapingSources;

using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

public partial class VseInstrumentySource : IScrapingSource
{
    private const string PrimaryUrl = "https://www.vseinstrumenti.ru/search/?what=";
    
    public string SourceName => "ВсеИнструменты";
    
    private readonly HttpClient _client;
    private readonly ILogger<VseInstrumentySource> _logger;
    private readonly IBrowser _browser;

    public VseInstrumentySource(ILogger<VseInstrumentySource> logger, IBrowser browser)
    {
        _logger = logger;
        _browser = browser;
        _client = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true
        })
        {
            DefaultRequestHeaders =
            {
                { "Accept-Language", "en-GB,en;q=0.9,ru;q=0.8,en-US;q=0.7,tt;q=0.6" },
                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36" }
            }
        };
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
            var htmlContent = await page.ContentAsync();
            await page.CloseAsync();

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(htmlContent));

            var grid = document.QuerySelectorAll("[data-qa='products-tile']")
                .Take(5)
                .ToArray();
            
            var table = document.QuerySelectorAll("[data-qa='products-tile-horizontal']")
                .Take(5)
                .ToArray();

            if (grid.Length > 0)
            {
                _logger.LogInformation($"По запросу найдено {grid.Length} товаров");
                return await GetFromGrid(grid);
            }

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