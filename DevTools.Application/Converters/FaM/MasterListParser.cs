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

        var email = rows.Select(e => e.Email).Where(e => e.Length > 0).Distinct().SingleOrDefault();
        var phoneNumber = rows.Select(e => e.Phone2).Where(e => e.Length > 0).Distinct().SingleOrDefault();
        var tripStartDate = rows.Select(e => e.TripStartDate).Distinct().Single();
        var tripEndDate = rows.Select(e => e.TripEndDate).Distinct().Single();

        var babyParticipantNumbers = rows.Where(e => e.Leistungscode == "BABYACC").Select(e => e.Teilnehmernr).ToList();
        var babyRows = rows.Where(e => babyParticipantNumbers.Contains(e.Teilnehmernr)).ToList();
        var nonBabyParticipantRows = rows.Except(babyRows).ToList();
        var groupedParticipantsRows = nonBabyParticipantRows.GroupBy(e => e.Teilnehmernr);
        var isFamilyCombo = rows.Any(r => r.Leistungscode.StartsWith("FAMPAK"));

        var travelGroup = MapGroup(rows);

        var participants = groupedParticipantsRows
            .Select(p => MapParticipant(p, tripStartDate, tripEndDate, travelGroup)).ToList();

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
            Group: travelGroup,
            IsFamilyCombo: isFamilyCombo);
    }

    private static List<Participant> GetBabies(IReadOnlyList<MasterListRow> babyRows, DateTime tripStartDate,
        DateTime tripEndDate, Participant booker)
    {
        var groupedBabyRows = babyRows.GroupBy(e => e.Teilnehmernr);
        var babies = groupedBabyRows.Select(p => MapParticipant(p, tripStartDate, tripEndDate, null, booker))
            .ToList();
        return babies;
    }

    private static Participant MapParticipant(
        IGrouping<int, MasterListRow> participant,
        DateTime tripStartDate,
        DateTime tripEndDate,
        Group? travelGroup,
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


        var inboundTravelInfo = GetTravelInfo(participant, tripStartDate, booker, isInBound: true);
        var outboundTravelInfo = GetTravelInfo(participant, tripEndDate, booker, isInBound: false);

        var checkIn = room is not null && tripStartDate != room.LeistungVon
            ? room.LeistungVon!.Value
            : inboundTravelInfo.Transport switch
            {
                Transport.Ferry => tripStartDate.AddDays(1),
                Transport.Flight or Transport.Individually => tripStartDate,
                _ => throw new ArgumentOutOfRangeException()
            };

        var checkOut = room is not null && tripEndDate != room.LeistungBis
            ? room.LeistungBis!.Value
            : outboundTravelInfo.Transport switch
            {
                Transport.Ferry => tripEndDate.AddDays(-1),
                Transport.Flight or Transport.Individually => tripEndDate,
                _ => throw new ArgumentOutOfRangeException()
            };

        var internalRemarks = participant.DistinctBy(e => e.InternalRemarks)
            .FirstOrDefault(e => e.InternalRemarks != string.Empty)
            ?.InternalRemarks ?? string.Empty;

        var firstName = nameList.Length > 1 ? nameList[1] : string.Empty;


        var staff = travelGroup == Group.Staff ? StaffParser.Parse(internalRemarks, firstName) : null;

        var car = participant.FirstOrDefault(e => e.InfoFerry.Length > 0)?.InfoFerry;

        var p = new Participant(
            ParticipantNumber: participant.DistinctBy(e => e.Teilnehmernr).Single().Teilnehmernr,
            Gender: MapGender(participant.FirstOrDefault(e => e.Teilnehmeranrede.Length > 0)?.Teilnehmeranrede),
            FamilyName: nameList[0],
            FirstName: firstName,
            DateOfBirth: dateOfBirth,
            IdentificationDocumentNumber: travelDocumentNumber,
            TravelInfo: travelInfo,
            FerryInfo: car,
            HotelInfo: hotelInfo,
            CheckIn: checkIn,
            CheckOut: checkOut,
            InboundTravelInfo: inboundTravelInfo,
            OutboundTravelInfo: outboundTravelInfo,
            RoomReference: booker?.RoomReference ?? roomReference ?? 0,
            RoomType: booker?.RoomType ?? roomType,
            Repeater: repeater,
            Staff: staff);

        return p;
    }

    private static ParticipantTravelInformation GetTravelInfo(
        IGrouping<int, MasterListRow> participant,
        DateTime date,
        Participant? booker,
        bool isInBound)
    {
        var cabinType = TryGetCabinType(participant, isInbound: isInBound);

        var cabinReference =
            (isInBound ? booker?.InboundTravelInfo.CabinReference : booker?.OutboundTravelInfo.CabinReference)
            ?? participant
                .FirstOrDefault(e => cabinType.HasValue && e.Leistungscode == cabinType.Value.ToString())
                ?.Belegungsnr;

        var greekAirport = GetGreekAirport(participant, isInbound: isInBound) ?? booker?.InboundTravelInfo?.Airport;

        var transport = Transport.Individually;

        if (greekAirport is not null)
        {
            transport = Transport.Flight;
        }
        else if (cabinType is not null)
        {
            transport = Transport.Ferry;
        }

        var travelInfo = new ParticipantTravelInformation(
            Transport: transport,
            Date: date,
            Airport: greekAirport,
            CabinReference: cabinReference,
            CabinType: cabinType);

        return travelInfo;
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

    private static CabinType? TryGetCabinType(IGrouping<int, MasterListRow> participant, bool isInbound)
    {
        var groupedRows = participant.Where(e => e.Leistungsart == "T-Schiff" && !e.Leistungscode.Contains("-"))
            .GroupBy(e => e.LeistungVon)
            .ToList();

        if (!groupedRows.Any())
        {
            return null;
        }

        var relevantRows =
            isInbound ? groupedRows.MinBy(e => e.Key)?.ToList() : groupedRows.MaxBy(e => e.Key)?.ToList();

        if (relevantRows is null)
        {
            return null;
        }

        var codes = relevantRows.Select(e => e.Leistungscode).ToList();


        foreach (var code in codes)
        {
            switch (code)
            {
                case "G1I":
                    return CabinType.G1I;
                case "G2I":
                    return CabinType.G2I;
                case "G3I":
                    return CabinType.G3I;
                case "G4I":
                    return CabinType.G4I;
                case "3BEAUSS":
                    return CabinType.ThreeBEAUSS;
                case "ABEAUSS":
                    return CabinType.ABEAUSS;
                case "DKAUSS":
                    return CabinType.DKAUSSR;
                case "EKAUS":
                    return CabinType.EKAUS;
                case "GDI":
                    return CabinType.GDI;
                case "G1IR":
                    return CabinType.G1IR;
                case "G2IR":
                    return CabinType.G2IR;
                case "G3IR":
                    return CabinType.G3IR;
                case "G4IR":
                    return CabinType.G4IR;
                case "3BEAUSSR":
                    return CabinType.ThreeBEAUSS;
                case "ABEAUSSR":
                    return CabinType.ABEAUSS;
                case "DKAUSSR":
                    return CabinType.DKAUSSR;
                case "EKAUSR":
                    return CabinType.EKAUS;
                default:
                    continue;
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

        if (rows.Any(e => e.TripCode.Contains("STAFF")))
        {
            return Group.Staff;
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
}