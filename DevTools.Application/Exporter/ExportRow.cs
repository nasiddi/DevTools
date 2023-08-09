using System;
using ratio_list_converter.Parser;

namespace ratio_list_converter.Exporter;

public class ExportRow
{
    public int Index { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public int RatioBookingNumber { get; set; }
    public int RatioPersonNumber { get; set; }
    public Gender? Sex { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? DocumentNumber { get; set; }
    public Transport? Transport { get; set; }
    public string? BikeTransport { get; set; }
    public DateTime? TripStartDate { get; set; }
    public PickupDropOffLocation? PickupLocation { get; set; }
    public DateTime? TripEndDate { get; set; }
    public string? TravelInfo { get; set; }
    public PickupDropOffLocation? DropOffLocation { get; set; }
    public RoomType? RoomType { get; set; }
    public int RoomReference { get; set; }
    public string Meals { get; set; }
    public string? HotelInfo { get; set; }
    public string? PhoneNumber { get; set; }
    public CommunicationType? CommunicationType { get; set; }
    public string? Excursion1 { get; set; }
    public DateTime? Excursion1Date { get; set; }
    public string? Excursion2 { get; set; }
    public DateTime? Excursion2Date { get; set; }
    public string? Excursion3 { get; set; }
    public DateTime? Excursion3Date { get; set; }
    public string? Excursion4 { get; set; }
    public DateTime? Excursion4Date { get; set; }
    public string? Excursion5 { get; set; }
    public DateTime? Excursion5Date { get; set; }
    public string? Excursion6 { get; set; }
    public DateTime? Excursion6Date { get; set; }
    
}