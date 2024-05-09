namespace ShopScraper.Infrastructure.Utils;

public static class CollectionUtils
{
    public static bool IsAnyNullOrEmptyLike(this IEnumerable<string?> strings) 
        => strings.Any(x => string.IsNullOrEmpty(x) || string.IsNullOrWhiteSpace(x));

}