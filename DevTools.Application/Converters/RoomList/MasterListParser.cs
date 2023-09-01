using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.RoomList;

public class MasterListParser
{
    public static ImmutableList<Room> Parse(IImmutableList<MasterListRow> records)
    {
        var bookings = records
            .Where(e => e.BookingCode == "Festbuchung" && e.Leistungsart == "U-Zimmer")
            .GroupBy(e => e.Teilnehmernr)
            .GroupBy(e => e.FirstOrDefault(p => p.Belegungsnr.HasValue)?.Belegungsnr)
            .Select(e => MapRoom(e))
            .ToList();

        return bookings.ToImmutableList();
    }

    private static Room MapRoom(IGrouping<int?, IGrouping<int, MasterListRow>> guestsByRoomKey)
    {
        var rows = guestsByRoomKey.ToList();

        var guests = rows.Select(p =>
        {
            var name = p.DistinctBy(e => e.Teilnehmername).FirstOrDefault()?.Teilnehmername ?? string.Empty;
            var nameList = name.Split("  ", 2);

            var dateOfBirth = p.DistinctBy(e => e.Tl_geburtstag).FirstOrDefault()?.Tl_geburtstag;
            var tripStartDate = p.DistinctBy(e => e.TripStartDate).FirstOrDefault()?.TripStartDate;
            var tripEndDate = p.DistinctBy(e => e.TripEndDate).FirstOrDefault()?.TripEndDate;
            var passportNumber = p.DistinctBy(e => e.Travel_document_number).FirstOrDefault()?.Travel_document_number;
            var expirationDate = p.DistinctBy(e => e.Date_of_expiry).FirstOrDefault()?.Date_of_expiry;
            var countryCode = p.DistinctBy(e => e.Country_code).FirstOrDefault()?.Country_code;

            var age = tripStartDate?.Year - dateOfBirth?.Year;
            if (age.HasValue && dateOfBirth!.Value.Date > tripStartDate!.Value.AddYears(-age.Value)) age--;            
            string? nationality = null;

            if (countryCode is not null)
            {
                try
                {
                    var regionInfo = new RegionInfo(countryCode);
                    nationality = regionInfo.EnglishName;
                }
                catch
                {
                }
            }

            return new Guest(
                FamilyName: nameList[0],
                FirstName: nameList.Length > 1 ? nameList[1] : null,
                Gender: MapGender(p.FirstOrDefault(e => e.Teilnehmeranrede.Length > 0)?.Teilnehmeranrede),
                DateOfBirth: dateOfBirth,
                PassportNumber: passportNumber,
                Nationality: nationality,
                ExpirationDate: expirationDate,
                Comments: !guestsByRoomKey.Key.HasValue ? "No RoomReference found." : null,
                TripStartDate: tripStartDate,
                TripEndDate: tripEndDate,
                Age: age
            );
        }).ToImmutableList();

        return new Room(Guests: guests.ToImmutableList());
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