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
    
    [LufthansaGroup("Academic Title", 5)]
    public string AcademicTitle { get; set; }
    
    [LufthansaGroup("Salutation Title", 6)]
    public string SalutationTitle { get; set; }
    
    [LufthansaGroup("PAX type", 7)]
    public string? PaxType { get; set; }
    
    [LufthansaGroup("Date of birth", 8)]
    public string? DateOfBirth { get; set; }
    
    [LufthansaGroup("Gender", 9)]
    public string? Gender { get; set; }
    
    [LufthansaGroup("Airline associated with frequent flyer program", 10)]
    public string? FrequentFlyerProgram { get; set; }
    
    [LufthansaGroup("Frequent flyer number", 11)]
    public string? FrequentFlyerNumber { get; set; }
    
    [LufthansaGroup("Infant last name", 12)]
    public string? InfantLastName { get; set; }
    
    [LufthansaGroup("Infant first name", 13)]
    public string? InfantFirstName { get; set; }
    
    [LufthansaGroup("Infant gender", 14)]
    public string? InfantGender { get; set; }
    
    [LufthansaGroup("Infant date of birth", 15)]
    public string? InfantDateOfBirth { get; set; }
    
    [LufthansaGroup("Associated extra seats", 16)]
    public string? AssociatedExtraSeat { get; set; }
    
    [LufthansaGroup("Travel document type", 17)]
    public string? TravelDocumentType { get; set; }
    
    [LufthansaGroup("Document issuing country", 18)]
    public string? DocumentIssuingCountry { get; set; }
    
    [LufthansaGroup("Country code", 19)]
    public string? CountryCode { get; set; }
    
    [LufthansaGroup("Travel document number ", 20)]
    public string? TravelDocumentNumber { get; set; }
    
    [LufthansaGroup("Passenger nationality", 21)]
    public string? PassengerNationality { get; set; }
    
    [LufthansaGroup("Code of nationality", 22)]
    public string? CodeOfNationality { get; set; }
    
    [LufthansaGroup("Date of expiry", 23)]
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