using RoadRules.Services;

namespace RoadRules.Tests;

public class WeightCalculatorTests
{
    // Before the July change, so the results match the original six tests.
    private static readonly DateOnly AsOf = new(2026, 3, 1);

    private readonly WeightCalculator _calc = new(new FakeLimitSource());

    [Theory]
    [InlineData(1, "Single", 9000)]
    [InlineData(2, "Tandem", 17000)]
    [InlineData(3, "Tridem", 24000)]
    public async Task Returns_the_limit_for_the_axle_group(int axles, string group, int expectedKg)
    {
        var result = await _calc.CalculateAsync("ON", axles, AsOf);
        Assert.Equal(group, result.AxleGroup);
        Assert.Equal(expectedKg, result.MaxKg);
    }

    [Fact]
    public async Task Unknown_jurisdiction_throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _calc.CalculateAsync("ZZ", 2, AsOf));
    }

    [Fact]
    public async Task Zero_axles_throws()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _calc.CalculateAsync("ON", 0, AsOf));
    }

    [Fact]
    public async Task Axle_count_with_no_defined_limit_throws()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _calc.CalculateAsync("ON", 4, AsOf));
    }
}
