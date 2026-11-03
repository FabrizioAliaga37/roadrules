using RoadRules.Services;

namespace RoadRules.Tests;

public class FakeLimitSource : ILimitSource
{
    private readonly Dictionary<string, List<LimitRecord>> _data = new()
    {
        ["ON"] = new()
        {
            new LimitRecord("Single", 1, 9000),
            new LimitRecord("Tandem", 2, 17000),
            new LimitRecord("Tridem", 3, 24000)
        }
    };

    public Task<IReadOnlyList<LimitRecord>> GetLimitsAsync(string jurisdictionCode)
    {
        IReadOnlyList<LimitRecord> result =
            _data.TryGetValue(jurisdictionCode, out var list) ? list : new List<LimitRecord>();
        return Task.FromResult(result);
    }
}
