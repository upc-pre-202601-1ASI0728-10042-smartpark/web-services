using System;
using SmartPark.Domain.Common;
using SmartPark.Domain.Notifications;
using Xunit;

namespace SmartPark.Domain.Tests;

/// <summary>Pruebas de la entidad del bounded context Notifications.</summary>
public class DeviceTokenTests
{
    [Fact]
    public void Register_creates_token_with_expected_fields()
    {
        var userId = Guid.NewGuid();
        var token = DeviceToken.Register(userId, "fcm-token-123", "iOS");

        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal(userId, token.UserId);
        Assert.Equal("fcm-token-123", token.Token);
        Assert.Equal("iOS", token.Platform);
    }

    [Fact]
    public void Register_with_empty_user_throws()
        => Assert.Throws<DomainException>(() => DeviceToken.Register(Guid.Empty, "token", "Android"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_with_blank_token_throws(string token)
        => Assert.Throws<DomainException>(() => DeviceToken.Register(Guid.NewGuid(), token, "Android"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_defaults_platform_to_android_when_blank(string platform)
        => Assert.Equal("Android", DeviceToken.Register(Guid.NewGuid(), "token", platform).Platform);
}
