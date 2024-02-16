using System;
using System.Collections.Generic;

namespace DevTools.Application.Converters.FaM;

public record Booking(
    int BookingNumber,
    int? InvoiceNumber,
    string? Email,
    string? PhoneNumber,
    MealPlan MealPlan,
    Group? Group,
    bool IsFamilyCombo,
    IReadOnlyList<Participant> Participants);

public record Participant(
    int ParticipantNumber,
    Gender? Gender,
    string FamilyName,
    string FirstName,
    DateTime? DateOfBirth,
    string? IdentificationDocumentNumber,
    string? TravelInfo,
    string? HotelInfo,
    DateTime CheckIn,
    DateTime CheckOut,
    ParticipantTravelInformation? ParticipantTravelInformation,
    int RoomReference,
    int CabinReference,
    CabinType? CabinType,
    RoomType? RoomType,
    int? Repeater)
{
    public bool IsBooker { get; private set; }
    
    public void SetAsBooker()
    {
        IsBooker = RoomType == FaM.RoomType.E44;
        
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
    DGV,
    SGV,
    TGV,
    DSP,
    SSP,
    TSP,
    DMP,
    SSPSV,
    TMP,
    DSV,
    SSV,
    TSV,
    FAS,
    FAP,
    FSM,
    FSR,
    FPM,
    MEHRBE,
    D44,
    E44,
    M44,
    EM44,
    EPM44,
    P44,
    PM44
}

public enum CabinType
{
    G1A,
    G1I,
    G2A,
    G2I,
    G3A,
    G3I,
    G4A,
    G4I,
    Pullman
}

public enum Airport
{
    ZRH,
    KLX,
    GPA,
    ATH
}