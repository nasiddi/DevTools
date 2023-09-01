using System;
using System.Collections.Immutable;

namespace DevTools.Application.Converters.RoomList;

public record Room(ImmutableList<Guest> Guests)
{
    public int NumberOfGuests => Guests.Count;
}

public record Guest(
    string? FamilyName,
    string? FirstName,
    Gender? Gender,
    DateTime? DateOfBirth,
    string? PassportNumber,
    string? Nationality,
    DateTime? ExpirationDate,
    string? Comments,
    DateTime? TripStartDate,
    DateTime? TripEndDate,
    int? Age);
    
public enum Gender
{
    Male,
    Female
}