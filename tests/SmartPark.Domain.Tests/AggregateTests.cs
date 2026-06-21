using System;
using System.Linq;
using SmartPark.Domain.Common;
using SmartPark.Domain.IdentityAccess;
using SmartPark.Domain.IdentityAccess.Events;
using SmartPark.Domain.ParkingSession;
using SmartPark.Domain.ParkingSession.Events;
using SmartPark.Domain.SafetyIncident;
using SmartPark.Domain.SafetyIncident.Events;
using Xunit;
using PSession = SmartPark.Domain.ParkingSession.ParkingSession;

namespace SmartPark.Domain.Tests;

/// <summary>Pruebas del agregado raíz del bounded context Identity &amp; Access.</summary>
public class UserAccountTests
{
    [Fact]
    public void Register_creates_account_and_raises_event()
    {
        var user = UserAccount.Register(Email.Create("op@smartpark.pe"), "hash", "Elmer Riva", UserRole.Operator);

        Assert.Equal("op@smartpark.pe", user.Email.Value);
        Assert.Equal(UserRole.Operator, user.Role);
        Assert.Equal("Elmer Riva", user.FullName);
        Assert.Single(user.DomainEvents);
        var ev = Assert.IsType<UserRegistered>(user.DomainEvents.First());
        Assert.Equal(user.Id, ev.UserId);
        Assert.Equal("op@smartpark.pe", ev.Email);
        Assert.Equal(UserRole.Operator, ev.Role);
    }

    [Fact]
    public void Register_driver_sets_driver_role()
        => Assert.Equal(UserRole.Driver,
            UserAccount.Register(Email.Create("d@smartpark.pe"), "hash", "Conductor", UserRole.Driver).Role);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_with_blank_name_throws(string name)
        => Assert.Throws<DomainException>(
            () => UserAccount.Register(Email.Create("op@smartpark.pe"), "hash", name, UserRole.Driver));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_with_blank_password_hash_throws(string hash)
        => Assert.Throws<DomainException>(
            () => UserAccount.Register(Email.Create("op@smartpark.pe"), hash, "Nombre", UserRole.Driver));
}

/// <summary>Pruebas del agregado raíz del bounded context Safety &amp; Incident.</summary>
public class IncidentTests
{
    private static Incident AnAlert() => Incident.Raise("DET-01", "zone-a", 2, SmokeReading.Of(320));

    [Fact]
    public void Raise_starts_in_alert_and_raises_event_with_ppm()
    {
        var incident = AnAlert();

        Assert.Equal(IncidentStatus.Alert, incident.Status);
        Assert.Equal("DET-01", incident.DetectorId);
        Assert.Equal("zone-a", incident.ZoneId);
        Assert.Equal(2, incident.LevelNumber);
        Assert.Equal(320, incident.Reading.Ppm);
        var ev = Assert.IsType<SmokeAlertRaised>(Assert.Single(incident.DomainEvents));
        Assert.Equal(320, ev.SmokeLevel);
        Assert.Equal("zone-a", ev.ZoneId);
    }

    [Theory]
    [InlineData("", "zone-a")]
    [InlineData("   ", "zone-a")]
    [InlineData("DET-01", "")]
    [InlineData("DET-01", "   ")]
    public void Raise_with_blank_detector_or_zone_throws(string detector, string zone)
        => Assert.Throws<DomainException>(() => Incident.Raise(detector, zone, 1, SmokeReading.Of(300)));

    [Fact]
    public void Confirm_sets_status_to_confirmed()
    {
        var incident = AnAlert();
        incident.Confirm();
        Assert.Equal(IncidentStatus.Confirmed, incident.Status);
    }

    [Fact]
    public void Confirm_after_resolved_throws()
    {
        var incident = AnAlert();
        incident.Resolve();
        Assert.Throws<DomainException>(() => incident.Confirm());
    }

    [Fact]
    public void Resolve_closes_incident_and_raises_event()
    {
        var incident = AnAlert();
        incident.ClearDomainEvents();

        incident.Resolve();

        Assert.Equal(IncidentStatus.Resolved, incident.Status);
        Assert.NotNull(incident.ResolvedAt);
        Assert.Contains(incident.DomainEvents, e => e is IncidentResolved);
    }

    [Fact]
    public void Resolve_is_idempotent_and_does_not_raise_twice()
    {
        var incident = AnAlert();
        incident.Resolve();
        var resolvedAt = incident.ResolvedAt;
        incident.ClearDomainEvents();

        incident.Resolve(); // segunda llamada: no-op

        Assert.Equal(IncidentStatus.Resolved, incident.Status);
        Assert.Equal(resolvedAt, incident.ResolvedAt);
        Assert.DoesNotContain(incident.DomainEvents, e => e is IncidentResolved);
    }
}

/// <summary>Pruebas del agregado raíz del bounded context Parking Session.</summary>
public class ParkingSessionTests
{
    [Fact]
    public void Start_activates_session_and_raises_event()
    {
        var driver = Guid.NewGuid();
        var session = PSession.Start(driver);

        Assert.True(session.IsActive);
        Assert.Equal(driver, session.DriverId);
        Assert.Equal(Money.Zero(), session.AccumulatedCost);
        Assert.Null(session.EndedAt);
        var ev = Assert.IsType<ParkingSessionStarted>(Assert.Single(session.DomainEvents));
        Assert.Equal(session.Id, ev.SessionId);
    }

    [Fact]
    public void Start_with_empty_driver_throws()
        => Assert.Throws<DomainException>(() => PSession.Start(Guid.Empty));

    [Fact]
    public void RegisterVehicleLocation_sets_location_and_raises_event()
    {
        var session = PSession.Start(Guid.NewGuid());
        session.ClearDomainEvents();

        session.RegisterVehicleLocation(VehicleLocation.Of("A-01"));

        Assert.Equal("A-01", session.VehicleLocation!.SpaceId);
        var ev = Assert.IsType<VehicleLocationRegistered>(Assert.Single(session.DomainEvents));
        Assert.Equal("A-01", ev.SpaceId);
    }

    [Fact]
    public void RegisterVehicleLocation_on_finalized_session_throws()
    {
        var session = PSession.Start(Guid.NewGuid());
        session.Finalize(Money.Of(10));
        Assert.Throws<DomainException>(() => session.RegisterVehicleLocation(VehicleLocation.Of("A-02")));
    }

    [Fact]
    public void Finalize_closes_session_and_sets_cost()
    {
        var session = PSession.Start(Guid.NewGuid());
        session.RegisterVehicleLocation(VehicleLocation.Of("A-01"));

        session.Finalize(Money.Of(15.50m));

        Assert.False(session.IsActive);
        Assert.NotNull(session.EndedAt);
        Assert.Equal(15.50m, session.AccumulatedCost.Amount);
    }

    [Fact]
    public void Finalize_is_idempotent_and_keeps_first_cost()
    {
        var session = PSession.Start(Guid.NewGuid());
        session.Finalize(Money.Of(15.50m));
        var endedAt = session.EndedAt;

        session.Finalize(Money.Of(99.99m)); // segunda llamada: no-op

        Assert.Equal(15.50m, session.AccumulatedCost.Amount);
        Assert.Equal(endedAt, session.EndedAt);
    }
}
