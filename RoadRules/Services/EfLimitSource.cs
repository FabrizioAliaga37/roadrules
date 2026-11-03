using Microsoft.EntityFrameworkCore;
using RoadRules.Data;

namespace RoadRules.Services;

public class EfLimitSource(IDbContextFactory<AppDbContext> factory) : ILimitSource
{
    public async Task<IReadOnlyList<LimitRecord>> GetLimitsAsync(string jurisdictionCode)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.AxleLimits
            .Where(l => l.Jurisdiction!.Code == jurisdictionCode)
            // ThenBy makes the order of rows with the same AxleCount fixed. Without it,
            // SQL Server returned Ontario's July 17,500 tandem row before the January
            // 17,000 one (SQLite happened to return insert order). Step 11 adds the same line.
            .OrderBy(l => l.AxleCount)
            .ThenBy(l => l.EffectiveFrom)
            .Select(l => new LimitRecord(l.AxleGroup, l.AxleCount, l.MaxKg))
            .ToListAsync();
    }
}
