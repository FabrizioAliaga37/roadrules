namespace RoadRules.Services;

public class WeightCalculator(ILimitSource source) : IWeightCalculator
{
    public async Task<WeightResult> CalculateAsync(string jurisdictionCode, int axleCount)
    {
        if (axleCount < 1)
            throw new ArgumentOutOfRangeException(nameof(axleCount), "Axle count must be at least 1.");

        var limits = await source.GetLimitsAsync(jurisdictionCode);
        if (limits.Count == 0)
            throw new KeyNotFoundException($"No limits are defined for jurisdiction '{jurisdictionCode}'.");

        var match = limits.FirstOrDefault(l => l.AxleCount == axleCount)
            ?? throw new ArgumentOutOfRangeException(nameof(axleCount),
                $"No limit is defined for {axleCount} axles in {jurisdictionCode}.");

        return new WeightResult(jurisdictionCode, match.AxleGroup, match.MaxKg);
    }
}
