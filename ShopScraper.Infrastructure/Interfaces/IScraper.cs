namespace ShopScraper.Infrastructure.Interfaces;

using DTO;

public interface IScraper
{
    public Task<IReadOnlyCollection<ScrapingResultDTO>> Find(ScrapingRequest request);
}