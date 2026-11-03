namespace RoadRules.Services;

public interface ILimitSource
{
    Task<IReadOnlyList<LimitRecord>> GetLimitsAsync(string jurisdictionCode);
}
