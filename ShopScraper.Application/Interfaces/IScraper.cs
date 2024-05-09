namespace ShopScraper.Application.Interfaces;

using DTO;

public interface IScraper
{
    public Task<IReadOnlyCollection<ScrapingResultDTO>> Find(ScrapingRequest request);
}