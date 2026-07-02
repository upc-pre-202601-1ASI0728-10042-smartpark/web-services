using SmartPark.Domain.EnergyEfficiency;
using Xunit;

namespace SmartPark.Domain.Tests;

/// <summary>Pruebas de la lógica de recomendación de iluminación (Energy Efficiency).</summary>
public class LightingRecommendationTests
{
    [Fact]
    public void Empty_zone_is_reduced_to_standby()
    {
        var rec = LightingRecommendation.FromOccupancy(0.0);

        Assert.Equal("ReduceToStandby", rec.Status);
        Assert.Equal(20, rec.RecommendedLevel);
        Assert.Equal(80, rec.SavingsPercent);
    }

    [Fact]
    public void Low_occupancy_zone_is_dimmed()
    {
        var rec = LightingRecommendation.FromOccupancy(0.20); // por debajo del 30 %

        Assert.Equal("Dim", rec.Status);
        Assert.Equal(50, rec.RecommendedLevel);
        Assert.Equal(50, rec.SavingsPercent);
    }

    [Theory]
    [InlineData(0.30)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void Busy_zone_keeps_full_lighting(double rate)
    {
        var rec = LightingRecommendation.FromOccupancy(rate);

        Assert.Equal("Optimal", rec.Status);
        Assert.Equal(100, rec.RecommendedLevel);
        Assert.Equal(0, rec.SavingsPercent);
    }

    [Fact]
    public void Negative_rate_is_treated_as_empty()
        => Assert.Equal("ReduceToStandby", LightingRecommendation.FromOccupancy(-0.5).Status);
}
