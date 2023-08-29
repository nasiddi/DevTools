using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FlightList;

public static class LufthansaGroupPaxConverter
{
    public static ImmutableList<ConvertedFile> ConvertToLufthansaGroupPax(IImmutableList<MasterListRow> records)
    {
        var bookings = MasterListParser.Parse(records);
        var sortedBookings = bookings.OrderBy(e => e.Passangers.Min(p => p.FamilyName)).ToImmutableList();

        var distinctFlights = sortedBookings.SelectMany(e => e.Passangers.SelectMany(p => p.Flights)).Distinct()
            .OrderBy(e => e.DepartureDate).ToImmutableList();

        var flightsDictionary = distinctFlights.ToDictionary(k => k, _ => new List<LufthansaGroupPax>());

        foreach (var (_, passengers) in sortedBookings)
        {
            var babies = passengers.Where(e =>
                e.DateOfBirth.HasValue &&
                e.DateOfBirth.Value.AddYears(2) > e.Flights.Select(f => f.DepartureDate).Min()).ToList();
            
            var adults = passengers.Except(babies).OrderBy(e => e.DateOfBirth).ToImmutableList();
            
            foreach (var adult in adults)
            {
                foreach (var flight in adult.Flights)
                {
                    var LufthansaGroupPaxList = flightsDictionary[flight];

                    Person? infant = null;
                    var baby = babies.FirstOrDefault(e => e.Flights.Contains(flight));

                    if (baby is not null)
                    {
                        infant = new Person(
                            LastName: baby.FamilyName,
                            FirstNAme: baby.FirstName,
                            MiddleName: baby.MiddleName,
                            DateOfBirth: baby.DateOfBirth,
                            Gender: baby.Gender);

                        babies.Remove(baby);
                    }
                    
                    LufthansaGroupPaxList.Add(new LufthansaGroupPax(
                        adult.FrequentFlyerProgram,
                        adult.FrequentFlyerNumber,
                        new Person(
                            LastName: adult.FamilyName,
                            FirstNAme: adult.FirstName,
                            MiddleName: adult.MiddleName,
                            DateOfBirth: adult.DateOfBirth,
                            Gender: adult.Gender),
                        infant));
                }
            }
        }

        var flights = flightsDictionary.Select(LufthansaGroupPaxExporter.ExportLufthansaGroupPaxExportRows).ToImmutableList();

        return flights;
    }
}