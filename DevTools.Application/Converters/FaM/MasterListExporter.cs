using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DevTools.Application.Converters.FaM;

public class MasterListExporter
{
    public static ImmutableList<MasterListExportRow> ExportFaM(IReadOnlyList<Booking> bookings)
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
                    var row = new MasterListExportRow
                    {
                        Index = index,
                        LastName = p.FamilyName,
                        FirstName = p.FirstName.Trim(),
                        RatioBookingNumber = booking.BookingNumber,
                        RatioPersonNumber = p.ParticipantNumber,
                        Sex = p.Gender,
                        DateOfBirth = p.DateOfBirth,
                        Group = booking.Group,
                        DocumentNumber = p.IdentificationDocumentNumber,
                        TripStartTransport = p.ParticipantTravelInformation?.Transport,
                        TripStartDate = p.ParticipantTravelInformation?.TripStartDate,
                        InboundAirport = p.ParticipantTravelInformation?.InboundAirport,
                        TripEndTransport = p.ParticipantTravelInformation?.Transport,
                        TripEndDate = p.ParticipantTravelInformation?.TripEndDate,
                        OutboundAirport = p.ParticipantTravelInformation?.OutboundAirport,
                        TravelInfo = p.TravelInfo,
                        HotelInfo = p.HotelInfo,
                        RoomType = MapRoomType(room.RoomType),
                        RoomReference = room.RoomReference,
                        MealPlan = booking.MealPlan,
                        PhoneNumber = booking.PhoneNumber
                    };
                    
                    rows.Add(row);
                    index++;
                }
            }
        }

        return rows.ToImmutableList();
    }

    private static string? MapRoomType(RoomType? roomType)
    {
        return roomType switch
        {
            RoomType.D44 or RoomType.DGV => "DZ Standard",
            RoomType.DMP => "DZ Sharing Pool Meersicht",
            RoomType.DSP => "DZ Sharing Pool",
            RoomType.DSV or RoomType.M44 => "DZ Meersicht",
            RoomType.E44 or RoomType.SGV => "EZ Standard",
            RoomType.EM44 or RoomType.SSV => "EZ Meersicht",
            RoomType.EPM44 => "EZ Poolfront Meersicht",
            RoomType.FAP => "Familienappartment Sharing Pool",
            RoomType.FAS => "Familienappartment Standard",
            RoomType.FPM => "Suite Sharing Pool Meersicht",
            RoomType.FSM => "Familienappartment Meersicht",
            RoomType.FSR => "Suite",
            RoomType.P44 => "DZ Poolfront",
            RoomType.PM44 => "DZ Poolfront Meersicht",
            RoomType.SSP => "EZ Sharing Pool",
            RoomType.SSPSV => "EZ Sharing Pool Meersicht",
            RoomType.TGV => "DZ mit Zustellbett Standard",
            RoomType.TMP => "DZ mit Zustellbett Sharing Pool Meersicht",
            RoomType.TSP => "DZ mit Zustellbett Sharing Pool",
            RoomType.TSV => "DZ mit Zustellbett Meersicht",
            null => null,
            RoomType.BABYACC => throw new InvalidOperationException(),
            _ => $"Unknown RoomType {roomType}"
        };
    }
}