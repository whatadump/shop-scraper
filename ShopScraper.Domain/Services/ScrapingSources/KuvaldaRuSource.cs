namespace ShopScraper.Domain.Services.ScrapingSources;

using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Options;
using Infrastructure.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

public partial class KuvaldaRuSource : IScrapingSource
{
    public KuvaldaRuSource(IBrowser browser, ILogger<KuvaldaRuSource> logger, DefaultParserOptions options)
    {
        _browser = browser;
        _logger = logger;
        _options = options;
    }

    public string SourceName => "Кувалда.ру";

    private const string OriginalHost = "https://www.kuvalda.ru";

    private const string SearchTemplate = "https://www.kuvalda.ru/?digiSearch=true&term={0}&params=%7Csort%3DDEFAULT";

    private readonly IBrowser _browser;
    private readonly ILogger<KuvaldaRuSource> _logger;
    private readonly DefaultParserOptions _options;
    
    public async Task<IReadOnlyCollection<ScrapingResultDTO>> ScrapeDataForRequest(ScrapingRequest? request)
    {
        if (string.IsNullOrEmpty(request?.SearchQuery) || string.IsNullOrWhiteSpace(request?.SearchQuery))
        {
            return [];
        }

        var page = await _browser.NewPageAsync();
        await page.GotoAsync(string.Format(SearchTemplate, request.SearchQuery));
        await page.WaitForSelectorAsync(".digi-products-grid");
        var content = await page.ContentAsync();
        await page.CloseAsync();
        
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));
        var result = document.QuerySelectorAll(".digi-product")
            .Select(elem =>
            {
                var priceStringRaw = elem.QuerySelector(".digi-product-price-variant")?.Text() ?? string.Empty;
                var priceString = NumbersRegex().Replace(priceStringRaw, "");
                if (priceString == string.Empty || !decimal.TryParse(priceString, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out var price))
                {
                    _logger.LogError("Ошибка в парсере - стоимость не совпала с ожидаемым форматом");
                    return null;
                }

                var title = elem.QuerySelector(".digi-product__label")?.Text();
                var articleRaw = elem.QuerySelector(".digi-product__vendor-code")?.Text() ?? string.Empty;
                var article = NumbersRegex().Replace(articleRaw, string.Empty);
                var imageSrc = elem.QuerySelector(".digi-product__image")?.GetAttribute("src");
                var originalSrc = elem.QuerySelector(".digi-product__label")?.GetAttribute("href");

                if (new[] { title, article, imageSrc, originalSrc }.IsAnyNullOrEmptyLike())
                {
                    _logger.LogError("Ошибка в парсере, не удалось распарсить элемент");
                    return null;
                }

                return new ScrapingResultDTO(title!, imageSrc!, price, OriginalHost + originalSrc!, article,
                    SourceName);
            })
            .Where(x => x is not null)
            .Take(_options.DefaultSearchResultTakePerPage)
            .ToArray();
        
        _logger.LogInformation($"Получено {result.Length} записей из {SourceName}");
        
        return result!;
    }
    
    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex NumbersRegex();
}