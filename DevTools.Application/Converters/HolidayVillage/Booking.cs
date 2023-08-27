using System;
using System.Collections.Generic;

namespace DevTools.Application.Converters.HolidayVillage;

public record Booking(
        int BookingNumber,
        string? Email,
        string? PhoneNumber,
        CommunicationType? CommunicationType,
        int? NumberOfParticipants,
        bool IsFullBoard,
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
        PickupDropOffLocation? PickUpLocation,
        PickupDropOffLocation? DropOffLocation,
        string? TravelRemarks,
        string? HotelRemarks,
        string? BikeTransport,
        IReadOnlyList<PreBookedExcursion> Excursions);

public record PreBookedExcursion(string Name, DateTime Date);

public enum Gender
{
        Male,
        Female
}

public enum Transport
{
        Bus,
        Flight,
        Individually
}

public enum PickupDropOffLocation
{
        ZRH,
        Wiesendangen,
        Bern,
        Kölliken,
        Sommeri,
        Weinfelden,
}

public enum RoomType
{
        SUPAPMEER,
        SUPAP,
        OSKMEER,
        OSMEER,
        OS,
        BABYACC,
        
}

public enum CommunicationType
{
        Print,
        Digital
}