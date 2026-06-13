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

public class UserAccountTests
{
    [Fact]
    public void Register_creates_account_and_raises_event()
    {
        var user = UserAccount.Register(Email.Create("op@smartpark.pe"), "hash", "Elmer Riva", UserRole.Operator);

        Assert.Equal("op@smartpark.pe", user.Email.Value);
        Assert.Equal(UserRole.Operator, user.Role);
        Assert.Single(user.DomainEvents);
        Assert.IsType<UserRegistered>(user.DomainEvents.First());
    }

    [Fact]
    public void Register_with_blank_name_throws()
        => Assert.Throws<DomainException>(
            () => UserAccount.Register(Email.Create("op@smartpark.pe"), "hash", "  ", UserRole.Driver));
}

public class IncidentTests
{
    [Fact]
    public void Raise_starts_in_alert_and_raises_event()
    {
        var incident = Incident.Raise("DET-01", "zone-a", 2, SmokeReading.Of(320));

        Assert.Equal(IncidentStatus.Alert, incident.Status);
        Assert.Contains(incident.DomainEvents, e => e is SmokeAlertRaised);
    }

    [Fact]
    public void Resolve_closes_incident_and_raises_event()
    {
        var incident = Incident.Raise("DET-01", "zone-a", 2, SmokeReading.Of(320));
        incident.ClearDomainEvents();

        incident.Resolve();

        Assert.Equal(IncidentStatus.Resolved, incident.Status);
        Assert.NotNull(incident.ResolvedAt);
        Assert.Contains(incident.DomainEvents, e => e is IncidentResolved);
    }
}

public class ParkingSessionTests
{
    [Fact]
    public void Start_activates_session_and_raises_event()
    {
        var session = PSession.Start(Guid.NewGuid());

        Assert.True(session.IsActive);
        Assert.Contains(session.DomainEvents, e => e is ParkingSessionStarted);
    }

    [Fact]
    public void Start_with_empty_driver_throws()
        => Assert.Throws<DomainException>(() => PSession.Start(Guid.Empty));

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
}
