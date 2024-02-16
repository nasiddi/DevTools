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
            .Select(MapBooking)
            .ToList();

        return bookings.ToImmutableList();
    }

    private static Booking MapBooking(IGrouping<int, MasterListRow> arg)
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
        var isFamilyCombo = rows.Any(r => r.Leistungscode.StartsWith("FAMPAK"));

        var participants = groupedParticipantsRows
            .Select(p => MapParticipant(p, tripStartDate, tripEndDate)).ToList();

        var booker = participants.FirstOrDefault(e => e.FirstName == rows.First().BookerFirstName)
                     ?? participants.FirstOrDefault(e => rows.First().BookerFirstName.Contains(e.FirstName))
                     ?? participants.FirstOrDefault(e => e.FirstName.Contains(rows.First().BookerFirstName))
                     ?? participants.OrderBy(e => e.DateOfBirth).First();

        booker.SetAsBooker();
        var babies = GetBabies(babyRows, tripStartDate, tripEndDate, booker);
        participants.AddRange(babies);

        return new Booking(
            BookingNumber: arg.Key,
            rows.First().InvoiceNumber,
            Email: email,
            PhoneNumber: phoneNumber,
            Participants: participants.OrderBy(e => e.RoomReference).ThenBy(e => e.DateOfBirth).ToList(),
            MealPlan: MapMealPlan(rows),
            Group: MapGroup(rows),
            IsFamilyCombo: isFamilyCombo);
    }

    private static List<Participant> GetBabies(IReadOnlyList<MasterListRow> babyRows, DateTime tripStartDate,
        DateTime tripEndDate, Participant booker)
    {
        var groupedBabyRows = babyRows.GroupBy(e => e.Teilnehmernr);
        var babies = groupedBabyRows.Select(p => MapParticipant(p, tripStartDate, tripEndDate, booker))
            .ToList();
        return babies;
    }

    private static Participant MapParticipant(
        IGrouping<int, MasterListRow> participant,
        DateTime tripStartDate,
        DateTime tripEndDate,
        Participant? booker = default)
    {
        var name = participant.DistinctBy(e => e.Teilnehmername).Single().Teilnehmername;
        var nameList = name.Split("  ", 2);

        var dateOfBirth = participant.DistinctBy(e => e.Tl_geburtstag).SingleOrDefault()?.Tl_geburtstag;

        var room = participant.FirstOrDefault(e => e.Leistungsart == "U-Zimmer");

        var roomReference = room?.Belegungsnr;
        var roomType = MapRoomType(room);

        var travelDocumentNumber = participant.DistinctBy(e => e.Travel_document_number)
            .SingleOrDefault(e => e.Travel_document_number != string.Empty)
            ?.Travel_document_number;

        var travelInfo = participant.DistinctBy(e => e.Inforeise).FirstOrDefault(e => e.Inforeise != string.Empty)
            ?.Inforeise;

        var hotelInfo = participant.DistinctBy(e => e.Infounterbringung)
            .FirstOrDefault(e => e.Infounterbringung != string.Empty)
            ?.Infounterbringung;

        var repeater = GetRepeater(participant);


        CabinType? cabinType = null;

        var transport = Transport.Individually;

        if (participant.Any(e => e.Leistungsart == "T-Flug"))
        {
            transport = Transport.Flight;
        }
        else if (participant.Any(e => e.Leistungsart == "T-Schiff"))
        {
            transport = Transport.Ferry;
            cabinType = TryGetCabinType(participant);
        }

        var cabinReference = participant
            .FirstOrDefault(e => cabinType.HasValue && e.Leistungscode == cabinType.Value.ToString())
            ?.Belegungsnr;

        var participantTravelInformation = new ParticipantTravelInformation(
            Transport: booker?.ParticipantTravelInformation?.Transport ?? transport,
            TripStartDate: tripStartDate,
            TripEndDate: tripEndDate,
            InboundAirport: GetGreekAirport(participant, isInbound: true) ??
                            booker?.ParticipantTravelInformation?.InboundAirport,
            OutboundAirport: GetGreekAirport(participant, isInbound: false) ??
                             booker?.ParticipantTravelInformation?.OutboundAirport);

        var checkIn = participantTravelInformation.Transport switch
        {
            Transport.Ferry => tripStartDate.AddDays(1),
            Transport.Flight or Transport.Individually => tripStartDate,
            _ => throw new ArgumentOutOfRangeException()
        };

        var checkOut = participantTravelInformation.Transport switch
        {
            Transport.Ferry => tripEndDate.AddDays(-1),
            Transport.Flight or Transport.Individually => tripEndDate,
            _ => throw new ArgumentOutOfRangeException()
        };

        var p = new Participant(
            ParticipantNumber: participant.DistinctBy(e => e.Teilnehmernr).Single().Teilnehmernr,
            Gender: MapGender(participant.FirstOrDefault(e => e.Teilnehmeranrede.Length > 0)?.Teilnehmeranrede),
            FamilyName: nameList[0],
            FirstName: nameList.Length > 1 ? nameList[1] : string.Empty,
            DateOfBirth: dateOfBirth,
            IdentificationDocumentNumber: travelDocumentNumber,
            TravelInfo: travelInfo,
            HotelInfo: hotelInfo,
            CheckIn: checkIn,
            CheckOut: checkOut,
            ParticipantTravelInformation: participantTravelInformation,
            RoomReference: booker?.RoomReference ?? roomReference ?? 0,
            CabinReference: booker?.CabinReference ?? cabinReference ?? 0,
            CabinType: booker?.CabinType ?? cabinType,
            RoomType: booker?.RoomType ?? roomType,
            Repeater: repeater);

        return p;
    }

    private static int? GetRepeater(IGrouping<int, MasterListRow> participant)
    {
        var repeaterString = participant.DistinctBy(e => e.Ref2).FirstOrDefault(e => int.TryParse(e.Ref2, out _))?.Ref2;

        if (repeaterString is null)
        {
            return null;
        }

        int.TryParse(repeaterString, out var repeater);
        return repeater;
    }

    private static CabinType? TryGetCabinType(IGrouping<int, MasterListRow> participant)
    {
        var codes = participant.Where(e => e.Leistungsart == "T-Schiff").Select(e => e.Leistungscode).ToList();
        var isExterior = participant.Any(e => e.Leistungscode == "FAEHRAUSSEN");

        foreach (var code in codes)
        {
            var cabinCode = isExterior ? code.Replace('I', 'A') : code;

            if (Enum.TryParse(typeof(CabinType), cabinCode, out var cabinType))
            {
                return (CabinType?) cabinType;
            }
        }

        return null;
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

    private static RoomType? MapRoomType(MasterListRow? row)
    {
        if (row is null)
        {
            return null;
        }

        Enum.TryParse(typeof(RoomType), row.Leistungscode, out var roomType);
        return (RoomType?) roomType;
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

    private static int? GetNumberOfBeds(RoomType? roomType)
    {
        return roomType switch
        {
            RoomType.DGV or RoomType.DSP or RoomType.DMP or RoomType.DSV or RoomType.D44 or RoomType.M44 or RoomType.P44
                or RoomType.PM44 => 2,
            RoomType.SGV or RoomType.SSP or RoomType.SSPSV or RoomType.SSV or RoomType.E44 or RoomType.EM44
                or RoomType.EPM44 => 1,
            RoomType.TGV or RoomType.TSP or RoomType.TMP or RoomType.TSV => 3,
            RoomType.FAS or RoomType.FAP or RoomType.FSM or RoomType.FSR or RoomType.FPM => 4,
            RoomType.MEHRBE => 50,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
        };
    }
}