using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FaM;

public class MasterListParser
{
    public static ImmutableList<Booking> ParseFam(IImmutableList<MasterListRow> records)
    {
        var bookings = records
            .Where(e => e.BookingCode == "Festbuchung")
            .GroupBy(e => e.BookingNumber)
            .Select(e => MapBooking(e, true))
            .ToList();

        return bookings.ToImmutableList();
    }

    private static Booking MapBooking(IGrouping<int, MasterListRow> arg, bool parseExcursions)
    {
        var rows = arg.ToList();

        var numberOfParticipants =
            rows.SingleOrDefault(e => e.NumberOfParticipants.HasValue)?.NumberOfParticipants!.Value;
        var email = rows.Select(e => e.Email).Where(e => e.Length > 0).Distinct().SingleOrDefault();
        var phoneNumber = rows.Select(e => e.Phone2).Where(e => e.Length > 0).Distinct().SingleOrDefault();
        var tripStartDate = rows.Select(e => e.TripStartDate).Distinct().Single();
        var tripEndDate = rows.Select(e => e.TripEndDate).Distinct().Single();

        var babyParticipantNumbers = rows.Where(e => e.Leistungscode == "BABYACC").Select(e => e.Teilnehmernr).ToList();
        var babyRows = rows.Where(e => babyParticipantNumbers.Contains(e.Teilnehmernr)).ToList();
        var nonBabyParticipantRows = rows.Except(babyRows).ToList();
        var groupedParticipantsRows = nonBabyParticipantRows.GroupBy(e => e.Teilnehmernr);

        var participants = groupedParticipantsRows.Select(p => MapParticipant(p, tripStartDate, tripEndDate)).ToList();

        var booker = participants.FirstOrDefault(e => e.FirstName == rows.First().BookerFirstName)
                     ?? participants.FirstOrDefault(e => rows.First().BookerFirstName.Contains(e.FirstName))
                     ?? participants.FirstOrDefault(e => e.FirstName.Contains(rows.First().BookerFirstName))
                     ?? participants.OrderBy(e => e.DateOfBirth).First();

        booker.SetAsBooker();

        var groupedBabyRows = babyRows.GroupBy(e => e.Teilnehmernr);
        var babies = groupedBabyRows.Select(p => MapParticipant(p, tripStartDate, tripEndDate, booker)).ToList();
        participants.AddRange(babies);

        var rooms = nonBabyParticipantRows.Where(e => e is {Leistungsart: "U-Zimmer", Belegungsnr: not null})
            .DistinctBy(e => e.Belegungsnr)
            .Select(MapRoom)
            .OrderBy(e => e.RoomType)
            .ToList();

        var occupiedRooms = ClusterParticipants(
            orderedParticipants: participants.OrderByDescending(e => e.IsBooker).ThenBy(e => e.DateOfBirth).ToList(),
            rooms: rooms.ToList(),
            babyParticipantNumbers: babyParticipantNumbers);

        return new Booking(
            BookingNumber: arg.Key,
            Email: email,
            PhoneNumber: phoneNumber,
            NumberOfParticipants: numberOfParticipants,
            MealPlan: MapMealPlan(rows),
            Group: MapGroup(rows),
            Rooms: occupiedRooms);
    }

    private static Participant MapParticipant(IGrouping<int, MasterListRow> participant, DateTime tripStartDate,
        DateTime tripEndDate, Participant? booker = default)
    {
        var name = participant.DistinctBy(e => e.Teilnehmername).Single().Teilnehmername;
        var nameList = name.Split("  ", 2);

        var dateOfBirth = participant.DistinctBy(e => e.Tl_geburtstag).SingleOrDefault()?.Tl_geburtstag;
        var roomReference = participant
            .FirstOrDefault(e => e.Leistungsart == "U-Zimmer" && e.Leistungscode != "BABYACC")
            ?.Belegungsnr;
        var travelDocumentNumber = participant.DistinctBy(e => e.Travel_document_number)
            .SingleOrDefault(e => e.Travel_document_number != string.Empty)
            ?.Travel_document_number;

        var travelInfo = participant.DistinctBy(e => e.Inforeise).FirstOrDefault(e => e.Inforeise != string.Empty)
            ?.Inforeise;

        var hotelInfo = participant.DistinctBy(e => e.Infounterbringung)
            .FirstOrDefault(e => e.Infounterbringung != string.Empty)
            ?.Infounterbringung;

        var transport = Transport.Individually;

        if (participant.Any(e => e.Leistungsart == "T-Flug"))
        {
            transport = Transport.Flight;
        }
        else if (participant.Any(e => e.Leistungsart == "T-Schiff"))
        {
            transport = Transport.Ferry;
        }

        if (booker?.ParticipantTravelInformation is not null)
        {
            transport = booker.ParticipantTravelInformation.Transport;
        }

        var participantTravelInformation = new ParticipantTravelInformation(
            Transport: transport,
            TripStartDate: tripStartDate,
            TripEndDate: tripEndDate,
            InboundAirport: GetGreekAirport(participant, isInbound: true) ?? booker?.ParticipantTravelInformation?.InboundAirport,
            OutboundAirport: GetGreekAirport(participant, isInbound: false)  ?? booker?.ParticipantTravelInformation?.OutboundAirport);

        return new Participant(
            ParticipantNumber: participant.DistinctBy(e => e.Teilnehmernr).Single().Teilnehmernr,
            Gender: MapGender(participant.FirstOrDefault(e => e.Teilnehmeranrede.Length > 0)?.Teilnehmeranrede),
            FamilyName: nameList[0],
            FirstName: nameList.Length > 1 ? nameList[1] : string.Empty,
            DateOfBirth: dateOfBirth,
            IdentificationDocumentNumber: travelDocumentNumber,
            TravelInfo: travelInfo,
            HotelInfo: hotelInfo,
            ParticipantTravelInformation: participantTravelInformation,
            RoomReference: roomReference ?? 0);
    }

    private static MealPlan MapMealPlan(List<MasterListRow> rows)
    {
        if (rows.Any(e => e.Leistungsart == "S-Pauschalreise" && e.Leistungscode.StartsWith("ALLINC")))
        {
            return MealPlan.AllInclusive;
        }

        return rows.Any(e => e.Leistungsart == "S-Pauschalreise" && e.Leistungscode.StartsWith("VP"))
            ? MealPlan.FullBoard
            : MealPlan.HalfBoard;
    }

    private static Airport? GetGreekAirport(IGrouping<int, MasterListRow> p, bool isInbound)
    {
        if (isInbound)
        {
            var tripStartFlightRow = p.FirstOrDefault(e => e.Leistungsart == "T-Flug" && e.Leistungscode[..3] == "ZRH");

            if (tripStartFlightRow is null)
            {
                return null;
            }

            Enum.TryParse(tripStartFlightRow?.Leistungscode[4..], out Airport inboundAirport);
            return inboundAirport;
        }

        var tripEndFlightRow = p.FirstOrDefault(e => e.Leistungsart == "T-Flug" && e.Leistungscode[4..] == "ZRH");

        if (tripEndFlightRow is null)
        {
            return null;
        }

        Enum.TryParse(tripEndFlightRow?.Leistungscode[..3], out Airport outboundAirport);
        return outboundAirport;
    }

    private static Group? MapGroup(IReadOnlyList<MasterListRow> rows)
    {
        if (rows.Any(e => e.TripCode.Contains("FAM55")))
        {
            return Group.FiftyFivePlus;
        }

        if (rows.Any(e => e.TripCode.Contains("FAMEW")))
        {
            return Group.EmpoweringWeek;
        }

        return null;
    }

    private static IReadOnlyList<Room> ClusterParticipants(
        IReadOnlyList<Participant> orderedParticipants,
        IReadOnlyList<Room> rooms,
        IReadOnlyList<int> babyParticipantNumbers)
    {
        var participants = orderedParticipants.Where(e => !babyParticipantNumbers.Contains(e.ParticipantNumber))
            .ToList();

        var babies = orderedParticipants.Except(participants).ToList();

        foreach (var participant in participants)
        {
            var room = rooms.SingleOrDefault(e => e.RoomReference == participant.RoomReference);
            room?.AddParticipant(participant);
        }

        foreach (var baby in babies)
        {
            var roomReference = participants.First().RoomReference;
            var room = rooms.SingleOrDefault(e => e.RoomReference == roomReference);
            room?.AddParticipant(baby);
        }

        return rooms;
    }

    private static Room MapRoom(MasterListRow row)
    {
        Enum.TryParse(typeof(RoomType), row.Leistungscode, out var roomType);
        return new Room((RoomType?) roomType, row.Belegungsnr!.Value);
    }

    private static Gender? MapGender(string? genderCode)
    {
        return genderCode switch
        {
            "F" => Gender.Female,
            "H" => Gender.Male,
            _ => null
        };
    }
}