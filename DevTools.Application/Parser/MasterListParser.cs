using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace ratio_list_converter.Parser;

public class MasterListParser
{
    
    public static ImmutableList<Booking> ParseMasterList(Stream file)
    {
        var config = new CsvConfiguration(new CultureInfo("de-CH"));

        using var reader = new StreamReader(file, Encoding.GetEncoding( "iso-8859-1" ));
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<MasterListRowClassMap>();
        var records = csv.GetRecords<MasterListRow>().ToList();

        var bookings = records
            .Where(e => e.BookingCode == "Festbuchung")
            .GroupBy(e => e.BookingNumber)
            .Select(MapToBooking)
            .ToList();

        return bookings.ToImmutableList();
    }

    private static Booking MapToBooking(IGrouping<int, MasterListRow> arg)
    {
        var rows = arg.ToList();

        var numberOfParticipants = rows.SingleOrDefault(e => e.NumberOfParticipants.HasValue)?.NumberOfParticipants!.Value;
        var email = rows.Select(e => e.Email).Where(e => e.Length > 0).Distinct().SingleOrDefault();
        var phoneNumber = rows.Select(e => e.Phone2).Where(e => e.Length > 0).Distinct().SingleOrDefault();
        var mediaCode = rows.Select(e => e.Kontingentcode).Where(e => e.StartsWith("UNT")).Distinct().SingleOrDefault();
        var tripStartDate = rows.Select(e => e.TripStartDate).Distinct().Single();
        var tripEndDate = rows.Select(e => e.TripEndDate).Distinct().Single();
        
        var groupedParticipants = rows.GroupBy(e => e.Teilnehmernr);

        var bikes = rows.Where(e => e.Kontingentcode is "VELO" or "EBIKE").Select(e => e.Kontingentcode).ToList();

        var participants = groupedParticipants.Select(p =>
            {
                var name = p.DistinctBy(e => e.Teilnehmername).Single().Teilnehmername;
                var nameList = name.Split("  ", 2);

                var dateOfBirth = p.DistinctBy(e => e.Tl_geburtstag).SingleOrDefault()?.Tl_geburtstag;
                var roomReference = p.SingleOrDefault(e => e.Leistungsart == "U-Zimmer" && e.Leistungscode != "BABYACC")?.Belegungsnr;
                var travelDocumentNumber = p.DistinctBy(e => e.Travel_document_number).SingleOrDefault(e => e.Travel_document_number != string.Empty)
                    ?.Travel_document_number;
                
                var travelInfo = p.DistinctBy(e => e.Inforeise).SingleOrDefault(e => e.Inforeise != string.Empty)
                    ?.Inforeise;
                
                var hotelInfo = p.DistinctBy(e => e.Infounterbringung).SingleOrDefault(e => e.Infounterbringung != string.Empty)
                    ?.Infounterbringung;

                var transport = Transport.Individually;
                
                if (p.Any(e => e.Leistungsart == "T-Flug"))
                {
                    transport = Transport.Flight;
                    
                }
                else if (p.Any(e => e.Leistungsart == "T-Bufahrt"))
                {
                    transport = Transport.Bus;
                }

                var excursionRows = p.Where(e => e.Leistungsart == "S-Pauschalreise" && e.Leistungscode.StartsWith("AUS")).ToHashSet();

                var excursions = MapExcursions(excursionRows);

                var bike = "";

                if (bikes.Count > 0)
                {
                    bike = bikes.First();
                    bikes.Remove(bike);
                    bike = bike.Substring(0, 1) + bike.Substring(1).ToLower();
                }
                

                var participantTravelInformation = new ParticipantTravelInformation(
                    Transport: transport,
                    TripStartDate: tripStartDate,
                    TripEndDate: tripEndDate,
                    PickUpLocation: MapPickUpDropOffLocation(p.FirstOrDefault(e => e.Zustieghinfahrt.Length > 0)),
                    DropOffLocation: MapPickUpDropOffLocation(p.FirstOrDefault(e => e.Ausstiegrückfahrt.Length > 0)),
                    TravelRemarks: p.FirstOrDefault(e => e.Inforeise.Length > 0)?.Inforeise,
                    HotelRemarks: p.FirstOrDefault(e => e.Infounterbringung.Length > 0)?.Infounterbringung,
                    BikeTransport: bike,
                    Excursions: excursions);

                return new Participant(
                    ParticipantNumber: p.DistinctBy(e => e.Teilnehmernr).Single().Teilnehmernr,
                    Gender: MapGender(p.FirstOrDefault(e => e.Teilnehmeranrede.Length > 0)?.Teilnehmeranrede),
                    FamilyName: nameList[0],
                    FirstName: nameList.Length > 1 ? nameList[1] : string.Empty,
                    DateOfBirth: dateOfBirth,
                    IdentificationDocumentNumber: travelDocumentNumber,
                    TravelInfo: travelInfo,
                    HotelInfo: hotelInfo,
                    ParticipantTravelInformation: participantTravelInformation,
                    RoomReference: roomReference ?? 0);
            }).ToList();

        var booker = participants.FirstOrDefault(e => e.FirstName == rows.First().BookerFirstName)
                     ?? participants.FirstOrDefault(e => rows.First().BookerFirstName.Contains(e.FirstName))
                     ?? participants.FirstOrDefault(e => e.FirstName.Contains(rows.First().BookerFirstName));

        booker?.SetAsBooker();

        var rooms = rows.Where(e => e.Leistungsart == "U-Zimmer")
            .DistinctBy(e => e.Belegungsnr)
            .Select(MapRoom)
            .OrderBy(e => e.RoomType)
            .ToList();

        if (rows.Any(e => e.Leistungscode == "ZSK") && rooms.Count > 0)
        {
            rooms[0] = rooms[0] with {RoomType = RoomType.OSKMEER};
        }

        var babyParticipantNumbers = rows.Where(e => e.Leistungscode == "BABYACC")
            .Select(e => e.Teilnehmernr)
            .ToList();

        var occupiedRooms = ClusterParticipants(
            participants.OrderByDescending(e => e.IsBooker).ThenBy(e => e.DateOfBirth).ToList(),
            rooms.Where(e => e.RoomType != RoomType.BABYACC).ToList(),
            babyParticipantNumbers);

        return new Booking(
            BookingNumber: arg.Key,
            Email: email,
            PhoneNumber: phoneNumber,
            CommunicationType: MapCommunicationType(mediaCode),
            NumberOfParticipants: numberOfParticipants,
            IsFullBoard: rows.Any(e => e.Leistungscode == "VP"),
            Rooms: occupiedRooms);
    }

    private static IReadOnlyList<PreBookedExcursion> MapExcursions(IReadOnlySet<MasterListRow> excursionRows)
    {
        return excursionRows.Select(e => MapExcursion(e.Leistung, e.TripStartDate))
            .OrderBy(e => e.Date)
            .ToList();
    }

    private static PreBookedExcursion MapExcursion(string field, DateTime tripStartDate)
    {
        var withoutPrefix = field.Replace("Ausflug ", "");
        var dateString = withoutPrefix.Substring(withoutPrefix.LastIndexOf(" ", StringComparison.InvariantCulture)).Trim();
        var name = withoutPrefix.Replace(dateString, "").Trim();
        var dateSpilt = dateString.Split(".");
        var date = new DateTime(tripStartDate.Year, int.Parse(dateSpilt[1]), int.Parse(dateSpilt[0]));

        return new PreBookedExcursion(name, date);
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
        RoomType? roomType = row.Leistungscode switch
        {
            "SUPAPMEER" => RoomType.SUPAPMEER,
            "SUPAP" => RoomType.SUPAP,
            "OSKMEER" => RoomType.OSKMEER,
            "OSMEER" => RoomType.OSMEER,
            "OS" => RoomType.OS,
            "BABYACC" => RoomType.BABYACC,
            _ => null
        };

        return new Room(roomType, row.Belegungsnr);
    }
    
    private static PickupDropOffLocation? MapPickUpDropOffLocation(MasterListRow? row)
    {
        return row?.Zustieghinfahrt switch
        {
            "Bern Car-Terminal Neufeld" => PickupDropOffLocation.Bern,
            "Zürich Flughafen  Bus Station  Kante R" => PickupDropOffLocation.ZRH,
            "Kölliken Autobahnraststätte" => PickupDropOffLocation.Kölliken,
            "Sommeri  Surprise Reisen" => PickupDropOffLocation.Sommeri,
            "Weinfelden  Bahnhof SBB" => PickupDropOffLocation.Weinfelden,
            "Bahnhof Wiesendangen" => PickupDropOffLocation.Wiesendangen,
            _ => null
        };
    }

    private static CommunicationType? MapCommunicationType(string? mediaCode)
    {
        return mediaCode switch
        {
            "UNTPRINT" => CommunicationType.Print,
            "UNTDIGIT" => CommunicationType.Digital,
            _ => null
        };
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