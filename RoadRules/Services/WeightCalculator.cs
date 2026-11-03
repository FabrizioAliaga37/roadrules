namespace RoadRules.Services;

public class WeightCalculator(ILimitSource source) : IWeightCalculator
{
    public async Task<IReadOnlyList<LimitRecord>> EffectiveLimitsAsync(string jurisdictionCode, DateOnly asOf)
    {
        var all = await source.GetAllLimitsAsync(jurisdictionCode);
        if (all.Count == 0)
            throw new KeyNotFoundException($"No limits are defined for jurisdiction '{jurisdictionCode}'.");

        // The rule: per axle group, the latest row effective on or before asOf.
        return all
            .Where(l => l.EffectiveFrom <= asOf)
            .GroupBy(l => l.AxleCount)
            .Select(g => g.OrderByDescending(l => l.EffectiveFrom).First())
            .OrderBy(l => l.AxleCount)
            .ToList();
    }

    public async Task<WeightResult> CalculateAsync(string jurisdictionCode, int axleCount, DateOnly asOf)
    {
        if (axleCount < 1)
            throw new ArgumentOutOfRangeException(nameof(axleCount), "Axle count must be at least 1.");

        var effective = await EffectiveLimitsAsync(jurisdictionCode, asOf);
        var match = effective.FirstOrDefault(l => l.AxleCount == axleCount)
            ?? throw new ArgumentOutOfRangeException(nameof(axleCount),
                $"No limit is in effect for {axleCount} axles in {jurisdictionCode} on {asOf:yyyy-MM-dd}.");

        return new WeightResult(jurisdictionCode, match.AxleGroup, match.MaxKg, match.EffectiveFrom);
    }
}
