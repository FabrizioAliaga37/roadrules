namespace RoadRules.Services;

public interface IWeightCalculator
{
    Task<IReadOnlyList<LimitRecord>> EffectiveLimitsAsync(string jurisdictionCode, DateOnly asOf);

    Task<WeightResult> CalculateAsync(string jurisdictionCode, int axleCount, DateOnly asOf);
}
