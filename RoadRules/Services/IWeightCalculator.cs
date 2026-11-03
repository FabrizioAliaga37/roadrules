namespace RoadRules.Services;

public interface IWeightCalculator
{
    Task<WeightResult> CalculateAsync(string jurisdictionCode, int axleCount);
}
