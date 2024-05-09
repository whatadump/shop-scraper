namespace ShopScraper.Infrastructure.DTO;

public record ScrapingResultDTO(
    string Title,
    string ImageUrl,
    decimal Price,
    string OriginalUrl,
    string Article,
    string SourceName)
{
    public int GroupId { get; set; }
}