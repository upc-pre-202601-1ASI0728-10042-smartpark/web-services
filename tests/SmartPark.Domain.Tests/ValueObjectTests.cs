using SmartPark.Domain.Common;
using SmartPark.Domain.IdentityAccess;
using SmartPark.Domain.ParkingSession;
using SmartPark.Domain.SafetyIncident;
using Xunit;

namespace SmartPark.Domain.Tests;

/// <summary>Pruebas de los objetos de valor del bounded context Identity &amp; Access.</summary>
public class EmailTests
{
    [Theory]
    [InlineData("  Operador@Mall.COM ", "operador@mall.com")]
    [InlineData("driver@smartpark.pe", "driver@smartpark.pe")]
    [InlineData("Mixed.Case@Example.IO", "mixed.case@example.io")]
    public void Create_normalizes_trim_and_lowercase(string raw, string expected)
        => Assert.Equal(expected, Email.Create(raw).Value);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    [InlineData("@nodomain.com")]
    [InlineData("spaces in@mail.com")]
    public void Create_with_invalid_format_throws(string raw)
        => Assert.Throws<DomainException>(() => Email.Create(raw));

    [Fact]
    public void Equality_is_structural_and_case_insensitive()
        => Assert.Equal(Email.Create("a@b.com"), Email.Create("A@B.COM"));

    [Fact]
    public void Different_emails_are_not_equal()
        => Assert.NotEqual(Email.Create("a@b.com"), Email.Create("c@d.com"));

    [Fact]
    public void ToString_returns_normalized_value()
        => Assert.Equal("user@smartpark.pe", Email.Create(" User@SmartPark.pe ").ToString());
}

/// <summary>Pruebas del objeto de valor monetario.</summary>
public class MoneyTests
{
    [Fact]
    public void Zero_is_zero_pen()
    {
        var zero = Money.Zero();
        Assert.Equal(0m, zero.Amount);
        Assert.Equal("PEN", zero.Currency);
    }

    [Fact]
    public void Of_defaults_to_pen()
        => Assert.Equal("PEN", Money.Of(10).Currency);

    [Fact]
    public void Of_accepts_custom_currency()
        => Assert.Equal("USD", Money.Of(10, "USD").Currency);

    [Theory]
    [InlineData(12.345, 12.35)] // away-from-zero
    [InlineData(12.344, 12.34)]
    [InlineData(12.355, 12.36)]
    [InlineData(10, 10.00)]
    public void Of_rounds_to_two_decimals_away_from_zero(decimal raw, decimal expected)
        => Assert.Equal(expected, Money.Of(raw).Amount);

    [Fact]
    public void Of_negative_throws()
        => Assert.Throws<DomainException>(() => Money.Of(-0.01m));

    [Fact]
    public void Equality_considers_amount_and_currency()
    {
        Assert.Equal(Money.Of(10), Money.Of(10));
        Assert.NotEqual(Money.Of(10, "USD"), Money.Of(10, "PEN"));
        Assert.NotEqual(Money.Of(10), Money.Of(11));
    }

    [Fact]
    public void ToString_uses_two_decimals_and_currency()
        => Assert.Equal("15.50 PEN", Money.Of(15.5m).ToString());
}

/// <summary>Pruebas del objeto de valor de lectura de humo.</summary>
public class SmokeReadingTests
{
    [Fact]
    public void Alert_threshold_is_200_ppm()
        => Assert.Equal(200, SmokeReading.AlertThresholdPpm);

    [Theory]
    [InlineData(0, false)]
    [InlineData(199, false)]
    [InlineData(199.99, false)]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(1000, true)]
    public void IsAlert_reflects_threshold(double ppm, bool isAlert)
        => Assert.Equal(isAlert, SmokeReading.Of(ppm).IsAlert);

    [Fact]
    public void Of_stores_the_ppm_value()
        => Assert.Equal(320, SmokeReading.Of(320).Ppm);

    [Fact]
    public void Of_negative_throws()
        => Assert.Throws<DomainException>(() => SmokeReading.Of(-1));

    [Fact]
    public void Equality_is_by_ppm()
    {
        Assert.Equal(SmokeReading.Of(250), SmokeReading.Of(250));
        Assert.NotEqual(SmokeReading.Of(250), SmokeReading.Of(260));
    }
}

/// <summary>Pruebas del objeto de valor de ubicación del vehículo.</summary>
public class VehicleLocationTests
{
    [Theory]
    [InlineData("  space-l1-a03 ", "SPACE-L1-A03")]
    [InlineData("a-01", "A-01")]
    public void Of_normalizes_trim_and_uppercase(string raw, string expected)
        => Assert.Equal(expected, VehicleLocation.Of(raw).SpaceId);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Of_with_blank_throws(string raw)
        => Assert.Throws<DomainException>(() => VehicleLocation.Of(raw));

    [Fact]
    public void Equality_is_structural_after_normalization()
        => Assert.Equal(VehicleLocation.Of("space-l1-a03"), VehicleLocation.Of("  SPACE-L1-A03 "));

    [Fact]
    public void ToString_returns_space_id()
        => Assert.Equal("SPACE-L2-B07", VehicleLocation.Of("space-l2-b07").ToString());
}
