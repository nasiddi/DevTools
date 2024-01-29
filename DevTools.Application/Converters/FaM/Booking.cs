using System;
using System.Collections.Generic;

namespace DevTools.Application.Converters.FaM;

public record Booking(
    int BookingNumber,
    string? Email,
    string? PhoneNumber,
    int? NumberOfParticipants,
    MealPlan MealPlan,
    Group? Group,
    IReadOnlyList<Room> Rooms);

public record Room(RoomType? RoomType, int RoomReference)
{
    public readonly List<Participant> Participants = new();

    public void AddParticipant(Participant participant)
    {
        Participants.Add(participant);
    }
}

public record Participant(
    int ParticipantNumber,
    Gender? Gender,
    string FamilyName,
    string FirstName,
    DateTime? DateOfBirth,
    string? IdentificationDocumentNumber,
    string? TravelInfo,
    string? HotelInfo,
    ParticipantTravelInformation? ParticipantTravelInformation,
    int RoomReference)
{
    public bool IsBooker { get; private set; }

    public void SetAsBooker()
    {
        IsBooker = true;
    }
}

public record ParticipantTravelInformation(
    Transport Transport,
    DateTime TripStartDate,
    DateTime TripEndDate,
    Airport? InboundAirport,
    Airport? OutboundAirport);

public enum Gender
{
    Male,
    Female
}

public enum Transport
{
    Ferry,
    Flight,
    Individually
}

public enum Group
{
    EmpoweringWeek,
    FiftyFivePlus,
    Staff
}

public enum MealPlan
{
    HalfBoard,
    FullBoard,
    AllInclusive
}

public enum RoomType
{
    BABYACC,
    D44,
    DGV,
    DMP,
    DSP,
    DSV,
    E44,
    EM44,
    EPM44,
    FAP,
    FAS,
    FPM,
    FSM,
    FSR,
    M44,
    P44,
    PM44,
    SGV,
    SSP,
    SSPSV,
    SSV,
    TGV,
    TMP,
    TSP,
    TSV
}

public enum Airport
{
    ZRH,
    KLX,
    GPA,
    ATH
}