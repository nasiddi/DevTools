using System;

namespace DevTools.Application.Converters.FaM;

public class MasterListExportRow
{
    public int Index { get; set; }
    public int RatioPersonNumber { get; set; }

    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public int RatioBookingNumber { get; set; }
    public Gender? Sex { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Group? Group { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int? Repeater { get; set; }
    public Transport? TripStartTransport { get; set; }
    public DateTime? TripStartDate { get; set; }
    public Airport? InboundAirport { get; set; }
    public Transport? TripEndTransport { get; set; }
    public DateTime? TripEndDate { get; set; }
    public Airport? OutboundAirport { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? RoomType { get; set; }
    public int RoomReference { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? CabinType { get; set; }
    public int CabinReference { get; set; }
    public MealPlan MealPlan { get; set; }
    public string? TravelInfo { get; set; }
    public string? HotelInfo { get; set; }
}