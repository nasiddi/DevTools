using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DevTools.Application.Converters.FlightList;

namespace DevTools.Application.Converters.HolidayVillage;

public class MasterListExporter
{
    public static ImmutableList<LufthansaGroupPaxExportRow> ExportLufthansaGroupPaxExportRows()
    {
        return ImmutableList<LufthansaGroupPaxExportRow>.Empty;
    }
    public static ImmutableList<MasterListExportRow> ExportHolidayVillage(IReadOnlyList<Booking> bookings)
    {
        var sortedBookings = bookings.OrderBy(e =>
                e.Rooms.Min(r => r.Participants.Min(p => p.ParticipantTravelInformation?.TripStartDate)))
            .ThenBy(e => e.Rooms.Min(r => r.Participants.Min(p => p.FamilyName)));
        
        var rows = new List<MasterListExportRow>();

        var index = 1;
        
        foreach (var booking in sortedBookings)
        {
            foreach (var room in booking.Rooms.OrderBy(e => e.RoomReference))
            {
                foreach (var p in room.Participants.OrderBy(e => e.FamilyName).ThenBy(e => e.FirstName))
                {
                    var excursions = p.ParticipantTravelInformation?.Excursions;
                    var count = excursions?.Count ?? 0;
                    
                    var row = new MasterListExportRow
                    {
                        Index = index,
                        LastName = p.FamilyName,
                        FirstName = p.FirstName.Trim(),
                        RatioBookingNumber = booking.BookingNumber,
                        RatioPersonNumber = p.ParticipantNumber,
                        Sex = p.Gender,
                        DateOfBirth = p.DateOfBirth,
                        DocumentNumber = p.IdentificationDocumentNumber,
                        Transport = p.ParticipantTravelInformation?.Transport,
                        BikeTransport = p.ParticipantTravelInformation?.BikeTransport,
                        TripStartDate = p.ParticipantTravelInformation?.TripStartDate,
                        PickupLocation = p.ParticipantTravelInformation?.PickUpLocation,
                        TripEndDate = p.ParticipantTravelInformation?.TripEndDate,
                        DropOffLocation = p.ParticipantTravelInformation?.DropOffLocation,
                        TravelInfo = p.TravelInfo,
                        HotelInfo = p.HotelInfo,
                        RoomType = room.RoomType,
                        RoomReference = room.RoomReference,
                        Meals = booking.IsFullBoard ? "Full Board" : "Half Board",
                        PhoneNumber = booking.PhoneNumber,
                        CommunicationType = booking.CommunicationType,
                        Excursion1 = count > 0 ? excursions![0].Name : null,
                        Excursion1Date = count > 0 ? excursions![0].Date : null,
                        Excursion2 = count > 1 ? excursions![1].Name : null,
                        Excursion2Date = count > 1 ? excursions![1].Date : null,
                        Excursion3 = count > 2 ? excursions![1].Name : null,
                        Excursion3Date = count > 2 ? excursions![2].Date : null,
                        Excursion4 = count > 3 ? excursions![1].Name : null,
                        Excursion4Date = count > 3 ? excursions![3].Date : null,
                        Excursion5 = count > 4 ? excursions![1].Name : null,
                        Excursion5Date = count > 4 ? excursions![4].Date : null,
                        Excursion6 = count > 5 ? excursions![1].Name : null,
                        Excursion6Date = count > 5 ? excursions![5].Date : null
                    };
                    
                    rows.Add(row);
                    index++;
                }
            }
        }

        return rows.ToImmutableList();
    }
}