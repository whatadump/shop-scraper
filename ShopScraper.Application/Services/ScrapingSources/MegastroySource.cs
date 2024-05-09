namespace ShopScraper.Application.Services.ScrapingSources;

using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using DTO;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Options;
using Utils;

public partial class MegastroySource : IScrapingSource
{
    private const string SearchTemplate = "https://megastroy.com/catalog/search?q={0}";

    private const string OriginHostname = "https://megastroy.com";

    public MegastroySource(IBrowser browser, ILogger<MegastroySource> logger, DefaultParserOptions options)
    {
        _browser = browser;
        _logger = logger;
        _options = options;
    }

    public string SourceName => "Мегастрой";

    private readonly IBrowser _browser;
    private readonly ILogger<MegastroySource> _logger;
    private readonly DefaultParserOptions _options;

    public async Task<IReadOnlyCollection<ScrapingResultDTO>> ScrapeDataForRequest(ScrapingRequest request)
    {
        if (string.IsNullOrEmpty(request?.SearchQuery) || string.IsNullOrWhiteSpace(request?.SearchQuery))
        {
            return [];
        }

        var page = await _browser.NewPageAsync();
        await page.GotoAsync(string.Format(SearchTemplate, request.SearchQuery));
        await page.WaitForSelectorAsync(".js-widget-name-search_productslist");
        var content = await page.ContentAsync();
        await page.CloseAsync();
        
        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(content));
        return document.QuerySelectorAll(".js-widget-name-search_productslist .product-prev")
            .Select(elem =>
            {
                var article = NumbersRegex().Replace(elem.QuerySelector(".code")?.Text() ?? string.Empty, string.Empty);
                var imageSrc = elem.QuerySelector("img")?.GetAttribute("src");
                var title = elem.QuerySelector(".title")?.Text();
                var originSrc = elem.QuerySelector(".title")?.GetAttribute("href");
                var priceRaw = elem.QuerySelector(".price")?.Text();
                var priceFiltered = NumbersRegex().Replace(priceRaw ?? string.Empty, string.Empty);
                if (!decimal.TryParse(priceFiltered, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    _logger.LogError("Не удалось распарсить цену");
                    return null;
                }

                if (new[] { article, imageSrc, title, originSrc }.IsAnyNullOrEmptyLike())
                {
                    _logger.LogError("Не удалось распарсить запись: один из элементов был null");
                    return null;
                }

                return new ScrapingResultDTO(title!, imageSrc!, price, originSrc!, article, SourceName);
            })
            .Where(x => x is not null)
            .Take(_options.DefaultSearchResultTakePerPage)
            .ToArray()!;
    }
    
    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex NumbersRegex();
}