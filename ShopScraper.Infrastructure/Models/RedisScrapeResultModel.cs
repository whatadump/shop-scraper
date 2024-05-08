namespace ShopScraper.Infrastructure.Models;

using DTO;
using Redis.OM.Modeling;

[Document(Prefixes = [nameof(RedisScrapeResultModel)])]
public partial class RedisScrapeResultModel
{
    [RedisIdField]
    public string Id { get; set; }
    
    [Searchable]
    public string SearchQuery { get; set; }
    
    public ScrapingResultDTO[] Results { get; set; }
}