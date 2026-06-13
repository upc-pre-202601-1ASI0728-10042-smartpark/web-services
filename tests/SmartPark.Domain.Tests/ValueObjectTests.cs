using SmartPark.Domain.Common;
using SmartPark.Domain.IdentityAccess;
using SmartPark.Domain.ParkingSession;
using SmartPark.Domain.SafetyIncident;
using Xunit;

namespace SmartPark.Domain.Tests;

public class ValueObjectTests
{
    [Theory]
    [InlineData("  Operador@Mall.COM ", "operador@mall.com")]
    [InlineData("driver@smartpark.pe", "driver@smartpark.pe")]
    public void Email_Create_normalizes(string raw, string expected)
        => Assert.Equal(expected, Email.Create(raw).Value);

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    public void Email_Create_invalid_throws(string raw)
        => Assert.Throws<DomainException>(() => Email.Create(raw));

    [Fact]
    public void Email_equality_is_structural()
        => Assert.Equal(Email.Create("a@b.com"), Email.Create("A@B.COM"));

    [Theory]
    [InlineData(199, false)]
    [InlineData(200, true)]
    [InlineData(450, true)]
    public void SmokeReading_alert_threshold(double ppm, bool isAlert)
        => Assert.Equal(isAlert, SmokeReading.Of(ppm).IsAlert);

    [Fact]
    public void Money_negative_throws()
        => Assert.Throws<DomainException>(() => Money.Of(-1));

    [Fact]
    public void Money_rounds_to_two_decimals()
        => Assert.Equal(12.35m, Money.Of(12.345m).Amount);
}
