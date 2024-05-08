﻿namespace ShopScraper.Infrastructure.Interfaces;

using DTO;

public interface IScrapingSource
{
    public string SourceName { get; }
    
    public Task<IReadOnlyCollection<ScrapingResultDTO>> ScrapeDataForRequest(ScrapingRequest request);
}