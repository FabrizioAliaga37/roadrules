using RoadRules.Shared;

namespace RoadRules.Services;

// Returns every row for a jurisdiction, history included; it does no selection.
// Picking the row in effect on a date is the calculator's job, so it can be unit-tested.
public interface ILimitSource
{
    Task<IReadOnlyList<LimitRecord>> GetAllLimitsAsync(string jurisdictionCode);
}
