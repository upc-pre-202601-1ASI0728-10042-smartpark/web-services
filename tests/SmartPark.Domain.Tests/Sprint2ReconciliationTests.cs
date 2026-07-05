using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartPark.Application.Abstractions;
using SmartPark.Application.Notifications;
using SmartPark.Application.ParkingOperations;
using SmartPark.Domain.Common;
using SmartPark.Domain.Notifications;
using SmartPark.Domain.ParkingSession;
using Xunit;
using PSession = SmartPark.Domain.ParkingSession.ParkingSession;

namespace SmartPark.Domain.Tests;

/// <summary>Preferencias de notificación (Notifications, Sprint 2 reconciliation).</summary>
public class NotificationPreferencesTests
{
    [Fact]
    public void CreateDefault_enables_all_preferences()
    {
        var prefs = Domain.Notifications.NotificationPreferences.CreateDefault(Guid.NewGuid());

        Assert.True(prefs.SmokeAlerts);
        Assert.True(prefs.AvailabilityAlerts);
        Assert.True(prefs.SessionReminders);
        Assert.True(prefs.Promotions);
    }

    [Fact]
    public void CreateDefault_with_empty_user_throws()
        => Assert.Throws<DomainException>(() => Domain.Notifications.NotificationPreferences.CreateDefault(Guid.Empty));

    [Fact]
    public void Update_overwrites_all_preferences()
    {
        var prefs = Domain.Notifications.NotificationPreferences.CreateDefault(Guid.NewGuid());

        prefs.Update(smokeAlerts: true, availabilityAlerts: false, sessionReminders: true, promotions: false);

        Assert.True(prefs.SmokeAlerts);
        Assert.False(prefs.AvailabilityAlerts);
        Assert.True(prefs.SessionReminders);
        Assert.False(prefs.Promotions);
    }
}

/// <summary>Casos de uso de sesiones nuevos (resumen y sesión activa).</summary>
public class SessionQueriesTests
{
    [Fact]
    public async Task Summary_counts_active_entries_and_exits()
    {
        var repo = new FakeSessionRepository();
        repo.Seed(PSession.Start(Guid.NewGuid()));                 // activa
        repo.Seed(PSession.Start(Guid.NewGuid()));                 // activa
        var finalized = PSession.Start(Guid.NewGuid());
        finalized.Finalize(Money.Of(10));                          // finalizada
        repo.Seed(finalized);

        var summary = await new GetSessionSummaryHandler(repo).HandleAsync(new GetSessionSummaryQuery());

        Assert.Equal("LOT-SP-01", summary.LotId);
        Assert.Equal(2, summary.ActiveSessions);
        Assert.Equal(3, summary.EntriesLastHour);
        Assert.Equal(1, summary.ExitsLastHour);
        Assert.True(summary.AverageDurationMinutes >= 0);
    }

    [Fact]
    public async Task Active_returns_mapped_session_for_driver()
    {
        var driver = Guid.NewGuid();
        var session = PSession.Start(driver);
        session.RegisterVehicleLocation(VehicleLocation.Of("SPACE-L1-A03"));
        var repo = new FakeSessionRepository();
        repo.Seed(session);

        var dto = await new GetActiveSessionHandler(repo).HandleAsync(new GetActiveSessionQuery(driver));

        Assert.NotNull(dto);
        Assert.Equal("Active", dto!.Status);
        Assert.Equal("SPACE-L1-A03", dto.SpaceCode);
        Assert.Equal("LOT-SP-01", dto.LotId);
        Assert.Null(dto.EndedAt);
    }

    [Fact]
    public async Task Active_returns_null_when_driver_has_no_active_session()
    {
        var dto = await new GetActiveSessionHandler(new FakeSessionRepository())
            .HandleAsync(new GetActiveSessionQuery(Guid.NewGuid()));

        Assert.Null(dto);
    }

    private sealed class FakeSessionRepository : IParkingSessionRepository
    {
        private readonly List<PSession> _sessions = new();

        public void Seed(PSession session) => _sessions.Add(session);

        public Task<PSession?> GetActiveByDriverAsync(Guid driverId, CancellationToken ct = default)
            => Task.FromResult(_sessions.FirstOrDefault(s => s.DriverId == driverId && s.EndedAt is null));

        public Task<PSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_sessions.FirstOrDefault(s => s.Id == id));

        public Task<IReadOnlyList<PSession>> GetByDriverAsync(Guid driverId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PSession>>(_sessions.Where(s => s.DriverId == driverId).ToList());

        public Task<IReadOnlyList<PSession>> GetActiveByLocationsAsync(IEnumerable<string> spaceIds, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PSession>>(new List<PSession>());

        public Task<IReadOnlyList<PSession>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<PSession>>(_sessions.ToList());

        public void Add(PSession session) => _sessions.Add(session);
    }
}
