using RoadRules.Services;

namespace RoadRules.Tests;

// Captured from the Vue page before conversion. Any change to these numbers
// is a behaviour change, not a refactor.
public class ParityTests
{
    private readonly WeightCalculator _calc = new(new FakeLimitSource());
    private static readonly DateOnly AsOf = new(2026, 3, 1);

    [Theory]
    [InlineData("ON", 1, 9000)]
    [InlineData("ON", 2, 17000)]
    [InlineData("ON", 3, 24000)]
    public async Task Vue_and_Blazor_agree(string code, int axles, int kg)
    {
        var r = await _calc.CalculateAsync(code, axles, AsOf);
        Assert.Equal(kg, r.MaxKg);
    }

    [Fact]
    public async Task Unknown_jurisdiction_is_an_error_message_not_a_crash()
    {
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _calc.CalculateAsync("ZZ", 2, AsOf));
        Assert.Contains("ZZ", ex.Message);
    }
}
