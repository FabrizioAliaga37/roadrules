using RoadRules.Services;
using RoadRules.Shared;

namespace RoadRules.Tests;

public class FakeLimitSource : ILimitSource
{
    private readonly Dictionary<string, List<LimitRecord>> _data = new()
    {
        ["ON"] = new()
        {
            new LimitRecord("Single", 1, 9000, new DateOnly(2026, 1, 1)),
            new LimitRecord("Tandem", 2, 17000, new DateOnly(2026, 1, 1)),
            new LimitRecord("Tandem", 2, 17500, new DateOnly(2026, 7, 1)),
            new LimitRecord("Tridem", 3, 24000, new DateOnly(2026, 1, 1))
        }
    };

    public Task<IReadOnlyList<LimitRecord>> GetAllLimitsAsync(string jurisdictionCode)
    {
        IReadOnlyList<LimitRecord> result =
            _data.TryGetValue(jurisdictionCode, out var list) ? list : new List<LimitRecord>();
        return Task.FromResult(result);
    }
}
