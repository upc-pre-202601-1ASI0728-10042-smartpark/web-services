using SmartPark.Domain.EnergyEfficiency;
using Xunit;

namespace SmartPark.Domain.Tests;

/// <summary>Pruebas de la lógica de recomendación de iluminación (Energy Efficiency).</summary>
public class LightingRecommendationTests
{
    [Fact]
    public void Empty_zone_is_turned_off_to_safety_minimum()
    {
        var rec = LightingRecommendation.FromOccupancy(0.0);

        Assert.Equal("Off", rec.Action);
        Assert.Equal(100, rec.CurrentLightingLevel);
        Assert.Equal(10, rec.RecommendedLightingLevel);
        Assert.Equal(2.25, rec.EstimatedSavingsKwh); // (100-10)/100 * 2.5
    }

    [Fact]
    public void Low_occupancy_zone_is_dimmed()
    {
        var rec = LightingRecommendation.FromOccupancy(0.20); // por debajo del 35 %

        Assert.Equal("Dim", rec.Action);
        Assert.Equal(50, rec.RecommendedLightingLevel);
        Assert.Equal(1.25, rec.EstimatedSavingsKwh); // (100-50)/100 * 2.5
    }

    [Theory]
    [InlineData(0.35)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void Busy_zone_keeps_full_lighting(double rate)
    {
        var rec = LightingRecommendation.FromOccupancy(rate);

        Assert.Equal("Maintain", rec.Action);
        Assert.Equal(100, rec.RecommendedLightingLevel);
        Assert.Equal(0, rec.EstimatedSavingsKwh);
    }

    [Fact]
    public void Negative_rate_is_treated_as_empty()
        => Assert.Equal("Off", LightingRecommendation.FromOccupancy(-0.5).Action);
}
