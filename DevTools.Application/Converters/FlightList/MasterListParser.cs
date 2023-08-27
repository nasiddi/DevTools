using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FlightList;

public class MasterListParser
{
    public static ImmutableList<Booking> Parse(IImmutableList<MasterListRow> records)
    {
        var bookings = records
            .Where(e => e.BookingCode == "Festbuchung")
            .GroupBy(e => e.BookingNumber)
            .Select(MapBooking)
            .Where(e => e.Passangers.Count > 0)
            .ToList();

        return bookings.ToImmutableList();
    }

    private static Booking MapBooking(IGrouping<int, MasterListRow> arg)
    {
        var rows = arg.ToList();
        
        var groupedParticipants = rows.GroupBy(e => e.Teilnehmernr);

        var passengers = groupedParticipants.Select(p =>
        {
            var name = p.DistinctBy(e => e.Teilnehmername).FirstOrDefault()?.Teilnehmername ?? string.Empty;
            var nameList = name.Split("  ", 2);
            var names = nameList.Length > 1 ? nameList[1].Split(" ") : new[] {string.Empty};

            var dateOfBirth = p.DistinctBy(e => e.Tl_geburtstag).FirstOrDefault()?.Tl_geburtstag;

            var frequentFlyerProgram = p
                .FirstOrDefault(e => e.Airline_assosiated_with_frequent_flyer_programm.Length > 0)
                ?.Airline_assosiated_with_frequent_flyer_programm;

            var frequentFlyerNumber = p
                .FirstOrDefault(e => e.Frequent_flyer_number.Length > 0)
                ?.Frequent_flyer_number;

            var tripStartFlightRow = p.FirstOrDefault(e => e.Leistungsart == "T-Flug" && DepartsFromSwissAirport(e));
            var tripStartFlight = tripStartFlightRow is null ? null : new Flight(tripStartFlightRow.TripStartDate, tripStartFlightRow.Leistungscode);

            var tripEndFlightRow = p.FirstOrDefault(e => e.Leistungsart == "T-Flug" && !DepartsFromSwissAirport(e));
            var tripEndFlight = tripEndFlightRow is null ? null : new Flight(tripEndFlightRow.TripEndDate, tripEndFlightRow.Leistungscode);

            var flights = new List<Flight?> {tripStartFlight, tripEndFlight}.Where(e => e is not null).Select(e => e!).ToImmutableList();

            return new Passenger(
                Gender: MapGender(p.FirstOrDefault(e => e.Teilnehmeranrede.Length > 0)?.Teilnehmeranrede),
                FamilyName: nameList[0],
                FirstName: names[0],
                MiddleName: names.Length > 1 ? names[1] : string.Empty,
                DateOfBirth: dateOfBirth,
                FrequentFlyerProgram: frequentFlyerProgram,
                FrequentFlyerNumber: frequentFlyerNumber,
                Flights: flights
            );
        }).ToImmutableList();

        return new Booking(
            BookingNumber: arg.Key,
            Passangers: passengers.Where(e => e.Flights.Count > 0).ToImmutableList());
    }

    private static bool DepartsFromSwissAirport(MasterListRow row)
    {
        if (Enum.TryParse(row.Leistungscode[..3], out SwissAirports airport))
        {
            _ = airport;
            return true;
        }

        return false;
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