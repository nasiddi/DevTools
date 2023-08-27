using System;
using System.Collections.Immutable;

namespace DevTools.Application.Converters.FlightList;

public record Booking(
    int BookingNumber,
    ImmutableList<Passenger> Passangers
);

public record Passenger(
    Gender? Gender,
    string FamilyName,
    string FirstName,
    string MiddleName,
    DateTime? DateOfBirth,
    string? FrequentFlyerProgram,
    string? FrequentFlyerNumber,
    ImmutableList<Flight> Flights);

public record Flight(
    DateTime DepartureDate,
    string FlightCode);

public enum Gender
{
    Male,
    Female
}

public enum SwissAirports
{
    ZRH,
    BSL,
    GVA
}