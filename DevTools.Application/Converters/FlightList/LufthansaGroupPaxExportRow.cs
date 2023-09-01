using System;

namespace DevTools.Application.Converters.FlightList;

public class LufthansaGroupPaxExportRow
{
    [LufthansaGroup("S.No.", 1)]
    public int Index { get; set; }
    
    [LufthansaGroup("Last name", 2)]
    public string LastName { get; set; }
    
    [LufthansaGroup("First name", 3)]
    public string FirstName { get; set; }
    
    [LufthansaGroup("Middle name", 4)]
    public string? MiddleName { get; set; }
    
    [LufthansaGroup("Title", 5)]
    public string Title { get; set; }
    
    [LufthansaGroup("PAX type", 6)]
    public string? PaxType { get; set; }
    
    [LufthansaGroup("Date of birth", 7)]
    public string? DateOfBirth { get; set; }
    
    [LufthansaGroup("Gender", 8)]
    public string? Gender { get; set; }
    
    [LufthansaGroup("Airline associated with frequent flyer program", 9)]
    public string? FrequentFlyerProgram { get; set; }
    
    [LufthansaGroup("Frequent flyer number", 10)]
    public string? FrequentFlyerNumber { get; set; }
    
    [LufthansaGroup("Infant last name", 11)]
    public string? InfantLastName { get; set; }
    
    [LufthansaGroup("Infant first name", 12)]
    public string? InfantFirstName { get; set; }
    
    [LufthansaGroup("Infant date of birth", 13)]
    public string? InfantDateOfBirth { get; set; }
    
    [LufthansaGroup("Associated extra seats", 14)]
    public string? AssociatedExtraSeat { get; set; }
    
    [LufthansaGroup("Travel document type", 15)]
    public string? TravelDocumentType { get; set; }
    
    [LufthansaGroup("Document issuing country", 16)]
    public string? DocumentIssuingCountry { get; set; }
    
    [LufthansaGroup("Country code", 17)]
    public string? CountryCode { get; set; }
    
    [LufthansaGroup("Travel document number ", 18)]
    public string? TravelDocumentNumber { get; set; }
    
    [LufthansaGroup("Passenger nationality", 19)]
    public string? PassengerNationality { get; set; }
    
    [LufthansaGroup("Code of nationality", 20)]
    public string? CodeOfNationality { get; set; }
    
    [LufthansaGroup("Date of expiry", 21)]
    public string? ExpirationDate { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class LufthansaGroupAttribute : Attribute
{
    public LufthansaGroupAttribute(
        string title,
        int position)
    {
        Title = title;
        Position = position;
    }

    public string Title { get; }

    public int Position { get; }
}