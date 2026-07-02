using System;
using SmartPark.Domain.Common;
using SmartPark.Domain.ParkingSession;
using Xunit;
using PSession = SmartPark.Domain.ParkingSession.ParkingSession;

namespace SmartPark.Domain.Tests;

/// <summary>Pruebas del servicio de dominio de cálculo de costo de estacionamiento.</summary>
public class ParkingCostCalculatorTests
{
    private static readonly ParkingCostCalculator Calc = new(baseFee: 5.00m, hourlyRate: 3.50m);

    [Fact]
    public void Calculate_charges_minimum_of_one_hour()
    {
        var start = new DateTimeOffset(2026, 6, 25, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddMinutes(20); // menos de una hora

        var cost = Calc.Calculate(start, end);

        Assert.Equal(8.50m, cost.Amount); // 5.00 base + 3.50 * 1
        Assert.Equal("PEN", cost.Currency);
    }

    [Fact]
    public void Calculate_rounds_partial_hours_up()
    {
        var start = new DateTimeOffset(2026, 6, 25, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(2).AddMinutes(1); // 2h 1m -> 3 horas

        var cost = Calc.Calculate(start, end);

        Assert.Equal(15.50m, cost.Amount); // 5.00 + 3.50 * 3
    }

    [Fact]
    public void Calculate_exact_hours_are_not_rounded_up()
    {
        var start = new DateTimeOffset(2026, 6, 25, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(3);

        var cost = Calc.Calculate(start, end);

        Assert.Equal(15.50m, cost.Amount); // 5.00 + 3.50 * 3
    }

    [Fact]
    public void Calculate_with_exit_before_entry_throws()
    {
        var start = new DateTimeOffset(2026, 6, 25, 10, 0, 0, TimeSpan.Zero);
        Assert.Throws<DomainException>(() => Calc.Calculate(start, start.AddHours(-1)));
    }

    [Fact]
    public void Calculate_with_negative_rate_throws()
        => Assert.Throws<DomainException>(() => new ParkingCostCalculator(hourlyRate: -1m));

    [Fact]
    public void Finalize_with_calculated_cost_closes_session()
    {
        var session = PSession.Start(Guid.NewGuid());
        session.RegisterVehicleLocation(VehicleLocation.Of("SPACE-L1-A01"));
        var cost = Calc.Calculate(session.StartedAt, session.StartedAt.AddHours(1));

        session.Finalize(cost);

        Assert.False(session.IsActive);
        Assert.Equal(8.50m, session.AccumulatedCost.Amount);
        Assert.NotNull(session.EndedAt);
    }
}
