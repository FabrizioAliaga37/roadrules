using Microsoft.EntityFrameworkCore;
using RoadRules.Data;
using RoadRules.Shared;

namespace RoadRules.Services;

public class EfLimitSource(IDbContextFactory<AppDbContext> factory) : ILimitSource
{
    public async Task<IReadOnlyList<LimitRecord>> GetAllLimitsAsync(string jurisdictionCode)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.AxleLimits
            .Where(l => l.Jurisdiction!.Code == jurisdictionCode)
            .OrderBy(l => l.AxleCount).ThenBy(l => l.EffectiveFrom)
            .Select(l => new LimitRecord(l.AxleGroup, l.AxleCount, l.MaxKg, l.EffectiveFrom))
            .ToListAsync();
    }
}
