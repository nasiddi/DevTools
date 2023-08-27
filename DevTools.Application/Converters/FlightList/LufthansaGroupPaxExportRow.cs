using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DevTools.Application.Converters.FlightList;

public class LufthansaGroupPaxExportRow
{
    [StarAlliance("S.No.", 1)]
    public int Index { get; set; }
    
    [StarAlliance("Last name", 2)]
    public string LastName { get; set; }
    
    [StarAlliance("First name", 3)]
    public string FirstName { get; set; }
    
    [StarAlliance("Middle name", 4)]
    public string? MiddleName { get; set; }
    
    [StarAlliance("Title", 5)]
    public string Title { get; set; }
    
    [StarAlliance("PAX type", 6)]
    public string? PaxType { get; set; }
    
    [StarAlliance("Date of birth", 7)]
    public string? DateOfBirth { get; set; }
    
    [StarAlliance("Gender", 8)]
    public string? Gender { get; set; }
    
    [StarAlliance("Airline associated with frequent flyer program", 9)]
    public string? FrequentFlyerProgram { get; set; }
    
    [StarAlliance("Frequent flyer number", 10)]
    public string? FrequentFlyerNumber { get; set; }
    
    [StarAlliance("Infant last name", 11)]
    public string? InfantLastName { get; set; }
    
    [StarAlliance("Infant first name", 12)]
    public string? InfantFirstName { get; set; }
    
    [StarAlliance("Infant date of birth", 13)]
    public string? InfantDateOfBirth { get; set; }
    
    [StarAlliance("Associated extra seats", 14)]
    public string? AssociatedExtraSeat { get; set; }
    
    [StarAlliance("Travel document type", 15)]
    public string? TravelDocumentType { get; set; }
    
    [StarAlliance("Document issuing country", 16)]
    public string? DocumentIssuingCountry { get; set; }
    
    [StarAlliance("Country code", 17)]
    public string? CountryCode { get; set; }
    
    [StarAlliance("Travel document number ", 18)]
    public string? TravelDocumentNumber { get; set; }
    
    [StarAlliance("Passenger nationality", 19)]
    public string? PassengerNationality { get; set; }
    
    [StarAlliance("Code of nationality", 20)]
    public string? CodeOfNationality { get; set; }
    
    [StarAlliance("Date of expiry", 21)]
    public string? ExpirationDate { get; set; }
}


public class FlightListDateTimeConverter : DefaultTypeConverter
{
    public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is null)
        {
            return null;
        }

        var date = value as DateTime?;

        var result = date!.Value.ToString("dd-MMM-yyyy");

        return result;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class StarAllianceAttribute : Attribute
{
    public StarAllianceAttribute(
        string title,
        int position)
    {
        Title = title;
        Position = position;
    }

    public string Title { get; }

    public int Position { get; }
}