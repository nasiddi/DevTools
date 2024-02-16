using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DevTools.Application.Converters.FaM;

public class MasterListExporter
{
    public static ImmutableList<MasterListExportRow> ExportFaM(IReadOnlyList<Booking> bookings)
    {
        var sortedBookings = bookings
            .OrderBy(r => r.Participants.Min(p => p.ParticipantTravelInformation?.TripStartDate))
            .ThenBy(r => r.Participants.Min(p => p.FamilyName));

        var rows = new List<MasterListExportRow>();

        var index = 1;

        foreach (var booking in sortedBookings)
        {
            var roomCount = booking.Participants.Select(e => e.RoomReference).ToHashSet().Count;
            
            foreach (var p in booking.Participants.OrderBy(e => e.FamilyName).ThenBy(e => e.FirstName))
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
                    CheckIn = p.CheckIn,
                    CheckOut = p.CheckOut,
                    TravelInfo = p.TravelInfo,
                    HotelInfo = p.HotelInfo ?? (roomCount > 1 ? "Family Combo" : null),
                    RoomType = MapRoomType(p.RoomType),
                    RoomReference = p.RoomReference,
                    InvoiceNumber = booking.InvoiceNumber,
                    CabinType = MapCabinType(p.CabinType),
                    CabinReference = p.CabinReference,
                    MealPlan = booking.MealPlan,
                    PhoneNumber = booking.PhoneNumber,
                    Email = booking.Email,
                    Repeater = p.Repeater
                };

                rows.Add(row);
                index++;
            }
        }

        return rows.ToImmutableList();
    }

    private static string? MapRoomType(RoomType? roomType)
    {
        return roomType switch
        {
            RoomType.D44 or RoomType.DGV => "DBL GV / Double room, Garden view",
            RoomType.DMP or RoomType.PM44 => "DBL SP SF / Double room, Sharing pool, Sea front",
            RoomType.DSP or RoomType.P44 => "DBL SP / Double room, Sharing pool",
            RoomType.DSV or RoomType.M44 => "DBL SV / Double room, Sea view",
            RoomType.E44 or RoomType.SGV => "DBL GV S / Double room, Garden view, Single",
            RoomType.EM44 or RoomType.SSV or RoomType.EPM44 => "DBL SV S / Double room, Sea view, Single",
            RoomType.FAP => "FAM SP / Family room, Sharing Pool",
            RoomType.FAS => "FAM GV / Family room, Garden view",
            RoomType.FPM => "SUI SF / Suite, Sharing Pool, Sea front",
            RoomType.FSM => "FAM SV / Family room, Sea view",
            RoomType.FSR => "SUI ROH / Suite, Run of the house",
            RoomType.SSP => "DBL SP S / Double room, Sharing pool, Single",
            RoomType.SSPSV => "DBL SP SF S / Double room, Sharing pool, Sea front, Single",
            RoomType.TGV => "DBL GV+1 / Double room, Garden view (plus 1 bed)",
            RoomType.TMP => "DBL SP SF+1 / Double room, Sharing pool, Sea front (plus 1 bed)",
            RoomType.TSP => "DBL SP+1 / Double room, Sharing pool (plus 1 bed)",
            RoomType.TSV => "DBL SV+1 / Double room, Sea view (plus 1 bed)",
            RoomType.MEHRBE => "Staff",
            null => null,
            RoomType.BABYACC => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
        };
    }

    private static string? MapCabinType(CabinType? cabinType)
    {
        return cabinType switch
        {
            CabinType.G1A => "Einzelkabine Aussen",
            CabinType.G1I => "Einzelkabine Innen",
            CabinType.G2A => "Doppelkabine Aussen",
            CabinType.G2I => "Doppelkabine Innen",
            CabinType.G3A => "Dreierkabine Aussen",
            CabinType.G3I => "Dreierkabine Innen",
            CabinType.G4A => "Viererkabine Aussen",
            CabinType.G4I => "Viererkabine Innen",
            CabinType.Pullman => "Sitzplatz",
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(cabinType), cabinType, null)
        };
    }
}