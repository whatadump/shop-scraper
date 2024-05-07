namespace ShopScraper.Infrastructure.Extensions;

using System;
using System.Threading.Tasks;

public static class DatabaseExtensions
{
    public static void InTransaction(this ApplicationDbContext context, Action action)
    {
        using var transaction = context.Database.BeginTransaction();
        action();
        transaction.Commit();
    }
    
    public static async Task InTransaction(this ApplicationDbContext context, Func<Task> action)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        await action();
        await transaction.CommitAsync();
    }
}