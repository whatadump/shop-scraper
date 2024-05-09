namespace ShopScraper.Application.Services;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DTO;
using Interfaces;
using Microsoft.Extensions.Logging;
using Utils;

public partial class Scraper : IScraper
{
    private readonly IScrapingSource[] _sources;
    private readonly ILogger<Scraper> _logger;

    public Scraper(IEnumerable<IScrapingSource> sources, ILogger<Scraper> logger)
    {
        _sources = sources.ToArray();
        _logger = logger;

        var count = _sources.TryGetNonEnumeratedCount(out var neCount) ? neCount : _sources.Length;
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
        double GroupSimilarityThreshold = .7d;
        
        if (string.IsNullOrEmpty(request?.SearchQuery) || string.IsNullOrWhiteSpace(request?.SearchQuery))
        {
            _logger.LogInformation("Пришел пустой запрос на поиск");
            return [];
        }

        _logger.LogInformation($"Делаем поиск по тексту \"{request.SearchQuery}\"");
        
        var results = await Task.WhenAll(_sources.Select(x => x.ScrapeDataForRequest(request)));
        var resultsArray = results.SelectMany(x => x).OrderByDescending(x => x.Price).ToArray();

        var currentGroupId = 0;
        var groupDef = new ConcurrentDictionary<ScrapingResultDTO, int>(Environment.ProcessorCount, resultsArray.Length);
        var regex = Alphanumeric();
        var groupMergeLock = new object();
        
        Parallel.ForEach(resultsArray, iter1 =>
        {
            foreach (var iter2 in resultsArray)
            {
                //Если та-же запись
                if (iter1 == iter2 && groupDef.TryAdd(iter1, currentGroupId))
                {
                    Interlocked.Increment(ref currentGroupId);
                    continue;
                }

                var normalized1 = regex.Replace(iter1.Title.ToLower(), string.Empty);
                var normalized2 = regex.Replace(iter2.Title.ToLower(), string.Empty);
                
                //Если строки разные
                if (normalized1.StrikeAMatchCompare(normalized2) < GroupSimilarityThreshold) continue;
                
                //Если строки схожи
                var in1 = groupDef.TryGetValue(iter1, out var group1);
                var in2 = groupDef.TryGetValue(iter1, out var group2);
                if (in1 && in2)
                {
                    lock (groupMergeLock)
                    {
                        MergeGroups(group1, group2);
                    }
                    
                    continue;
                }

                if (in1 ^ in2)
                {
                    var groupId = in1 ? group1 : group2;
                    groupDef[iter1] = groupId;
                    groupDef[iter2] = groupId;
                    continue;
                }

                groupDef[iter1] = currentGroupId;
                groupDef[iter2] = currentGroupId;
                Interlocked.Increment(ref currentGroupId);
            }
        });


        _logger.LogInformation("Записали результаты в Redis");
        foreach (var pair in groupDef)
        {
            pair.Key.GroupId = pair.Value;
        }

        return resultsArray
            .OrderBy(x => x.GroupId)
            .ToArray();

        void MergeGroups(int id1, int id2)
        {
            var keys = groupDef
                .Where(x => x.Value == id1 || x.Value == id2)
                .Select(x => x.Key);
            foreach (var key in keys)
            {
                groupDef[key] = id1;
            }
        }
    }

    [GeneratedRegex("[^a-zA-Zа-яА-ЯёЁ0-9]")]
    private static partial Regex Alphanumeric();
}