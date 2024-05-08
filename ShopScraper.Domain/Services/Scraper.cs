namespace ShopScraper.Domain.Services;

using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

public class Scraper : IScraper
{
    private readonly IScrapingSource[] _sources;
    private readonly ILogger<Scraper> _logger;

    public Scraper(IEnumerable<IScrapingSource> sources, ILogger<Scraper> logger)
    {
        _sources = sources.ToArray();
        _logger = logger;
        
        var count = _sources.TryGetNonEnumeratedCount(out var neCount) ? neCount : _sources.Count();
        if (count == 0)
        {
            _logger.LogError("Не найдено ни одного источника данных о товарах");
        }
        else
        {
            _logger.LogInformation($"Найдены источники данных о товарах: {string.Join(", ", _sources.Select(x => x.SourceName))}");
        }
    }

    public async Task<IReadOnlyCollection<ScrapingResultDTO>> Find(ScrapingRequest? request)
    {
        if (string.IsNullOrEmpty(request?.SearchQuery) || string.IsNullOrWhiteSpace(request?.SearchQuery))
        {
            _logger.LogInformation("Пришел пустой запрос на поиск");
            return [];
        }
        
        _logger.LogInformation($"Делаем поиск по тексту \"{request.SearchQuery}\"");
        
        var results = await Task.WhenAll(_sources.Select(x => x.ScrapeDataForRequest(request)));
        return results.SelectMany(x => x).ToArray();
    }
}