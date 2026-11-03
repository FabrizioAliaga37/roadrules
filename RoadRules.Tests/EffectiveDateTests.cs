using RoadRules.Services;

namespace RoadRules.Tests;

public class EffectiveDateTests
{
    private readonly WeightCalculator _calc = new(new FakeLimitSource());

    [Theory]
    [InlineData("2026-03-01", 17000)] // before the July change
    [InlineData("2026-07-01", 17500)] // on the day it applies
    [InlineData("2026-12-31", 17500)] // after
    public async Task Picks_the_latest_limit_in_effect_on_the_date(string asOf, int expectedKg)
    {
        var result = await _calc.CalculateAsync("ON", 2, DateOnly.Parse(asOf));
        Assert.Equal(expectedKg, result.MaxKg);
    }

    [Fact]
    public async Task No_limit_in_effect_before_the_first_row_throws()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _calc.CalculateAsync("ON", 2, new DateOnly(2025, 6, 1)));
    }
}
