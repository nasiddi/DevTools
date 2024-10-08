using System;
using System.Collections.Generic;

namespace DevTools.Application.Converters.FaM;

public record Booking(
    int BookingNumber,
    int? InvoiceNumber,
    string? Email,
    string? PhoneNumber,
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
    string? FerryInfo,
    string? HotelInfo,
    DateTime CheckIn,
    DateTime CheckOut,
    MealPlan MealPlan,
    ParticipantTravelInformation InboundTravelInfo,
    ParticipantTravelInformation OutboundTravelInfo,
    int RoomReference,
    RoomType? RoomType,
    int? Repeater,
    Staff? Staff)
{
    public bool IsBooker { get; private set; }

    public void SetAsBooker()
    {
        IsBooker = true;
    }
}

public record ParticipantTravelInformation(
    Transport Transport,
    DateTime Date,
    Airport? Airport,
    int? CabinReference,
    CabinType? CabinType);

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
    EKAUS,
    G1I,
    DKAUSS,
    G2I,
    ThreeBEAUSS,
    G3I,
    ABEAUSS,
    G4I,
    GDI,
    G1IR,
    G2IR,
    G3IR,
    G4IR,
    ThreeBEAUSSR,
    ABEAUSSR,
    DKAUSSR,
    EKAUSSR
}

public static class CabinTypeExtensions
{
    public static string? ToCustomString(this CabinType cabinType)
    {
        return cabinType switch
        {
            CabinType.EKAUS => "EKAUS",
            CabinType.G1I => "G1I",
            CabinType.DKAUSS => "DKAUSS",
            CabinType.G2I => "G2I",
            CabinType.ThreeBEAUSS => "3BEAUSS",
            CabinType.G3I => "G3I",
            CabinType.ABEAUSS => "ABEAUSS",
            CabinType.G4I => "G4I",
            CabinType.GDI => "GDI",
            CabinType.G1IR => "G1IR",
            CabinType.G2IR => "G2IR",
            CabinType.G3IR => "G3IR",
            CabinType.G4IR => "G4IR",
            CabinType.ThreeBEAUSSR => "3BEAUSSR",
            CabinType.ABEAUSSR => "ABEAUSSR",
            CabinType.DKAUSSR => "DKAUSSR",
            CabinType.EKAUSSR => "EKAUSSR",
            _ => null
        };
    }
}

public enum Airport
{
    ZRH,
    KLX,
    GPA,
    ATH
}

public record Staff(
    string RemarksBooking,
    string RemarksParticipants,
    Team Team,
    string Profession,
    string Skills,
    string SmallGroupLeader,
    string GvCService,
    string DriversLicence,
    string LicenceDate,
    bool WouldDrive,
    bool HaveCar,
    string CarDetails,
    string ServiceHelp,
    bool Band,
    bool Theater,
    bool Technic,
    bool Singing,
    bool WorshipDance,
    bool ChildCare,
    string Instruments
);

public enum Team
{
    Adults,
    Youth,
    Kids
}