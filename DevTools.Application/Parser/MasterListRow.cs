using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace ratio_list_converter.Parser;

public class MasterListRow
{
    public string TripCode { get; set; }
    public string BookingCode { get; set; }
    public string BookingDate { get; set; }
    public int BookingNumber { get; set; }
    public int InvoiceNumber { get; set; }
    public DateTime TripStartDate { get; set; }
    public DateTime TripEndDate { get; set; }
    public int? NumberOfParticipants { get; set; }
    public string FormOfAddress { get; set; }
    public string EmptyTitle { get; set; }
    public int Titel { get; set; }
    public string BookerFirstName { get; set; }
    public string BookerLastName { get; set; }
    public string EmptyBookerName { get; set; }
    public string EmptyExtra { get; set; }
    public string Street { get; set; }
    public string Street2 { get; set; }
    public string Country { get; set; }
    public string Plz { get; set; }
    public string City { get; set; }
    public string LetterFormOfAddress { get; set; }
    public string Phone1 { get; set; }
    public string Phone2 { get; set; }
    public string Pass_info { get; set; }
    public string Email { get; set; }
    public string DateOfBirth { get; set; }
    public string UslessRow { get; set; }
    public string EmptyPersonNumber { get; set; }
    public string Last_name { get; set; }
    public string First_name { get; set; }
    public string Middle_name { get; set; }
    public string Title { get; set; }
    public string Paytype { get; set; }
    public string? Date_of_birth { get; set; }
    public string Gender { get; set; }
    public string Airline_assosiated_with_frequent_flyer_programm { get; set; }
    public string Frequent_flyer_number { get; set; }
    public string Infant_last_name { get; set; }
    public string Infant_first_name { get; set; }
    public string Date_of_birth_infant { get; set; }
    public string Associated_extra_seat { get; set; }
    public string Travel_document_type { get; set; }
    public string Document_issuing_country { get; set; }
    public string Country_code { get; set; }
    public string Travel_document_number { get; set; }
    public string Passenger_nationality { get; set; }
    public string Code_of_nationality { get; set; }
    public string Date_of_expiry { get; set; }
    public string Teilnehmeranrede { get; set; }
    public string Teilnehmername { get; set; }
    public DateTime? Tl_geburtstag { get; set; }
    public string Infounterbringung { get; set; }
    public string Inforeise { get; set; }
    public string Ref1 { get; set; }
    public string Ref2 { get; set; }
    public int Teilnehmernr { get; set; }
    public string Zustieghinfahrt { get; set; }
    public string Ausstiegrückfahrt { get; set; }
    public int? Leistungstyp { get; set; }
    public string Leistungsart { get; set; }
    public string Leistungscode { get; set; }
    public string Kontingentcode { get; set; }
    public string Leistung { get; set; }
    public int? Reiseleistungsnr { get; set; }
    public int? Leistungsstammnr { get; set; }
    public int Belegungsnr { get; set; }
    public int Vorgangnr { get; set; }
}

public class MasterListRowClassMap : ClassMap<MasterListRow>
{
    public MasterListRowClassMap()
    {
        Map(m => m.TripCode).Name("Reisecode");
        Map(m => m.BookingCode).Name("Vorgang status");
        Map(m => m.BookingDate).Name("Bu.datum");
        Map(m => m.BookingNumber).Name("Bu.nr.");
        Map(m => m.InvoiceNumber).Name("Re.nr.");
        Map(m => m.TripStartDate).Name("Reisedatum von").TypeConverter<CustomDateTimeConverter>();
        Map(m => m.TripEndDate).Name("Reisedatum bis").TypeConverter<CustomDateTimeConverter>();
        Map(m => m.NumberOfParticipants).Name("Gebuchte teilnehmer");
        Map(m => m.FormOfAddress).Name("Anrede");
        Map(m => m.EmptyTitle).Name("Titel").NameIndex(0);
        Map(m => m.Titel).Name("Titel").NameIndex(1);
        Map(m => m.BookerFirstName).Name("Vorname");
        Map(m => m.BookerLastName).Name("Nachname");
        Map(m => m.EmptyBookerName).Name("Nachname2");
        Map(m => m.EmptyExtra).Name("Zusatz");
        Map(m => m.Street).Name("Strasse");
        Map(m => m.Street2).Name("Strasse2");
        Map(m => m.Country).Name("Land");
        Map(m => m.Plz).Name("Plz");
        Map(m => m.City).Name("Ort");
        Map(m => m.LetterFormOfAddress).Name("Briefanrede");
        Map(m => m.Phone1).Name("Telefon1");
        Map(m => m.Phone2).Name("Telefon2");
        Map(m => m.Pass_info).Name("Pass_info");
        Map(m => m.Email).Name("Email");
        Map(m => m.DateOfBirth).Name("Geburtstag");
        Map(m => m.UslessRow).Name("Tnbereich");
        Map(m => m.EmptyPersonNumber).Name("Pnr");
        Map(m => m.Last_name).Name("Last_name");
        Map(m => m.First_name).Name("First_name");
        Map(m => m.Middle_name).Name("Middle_name");
        Map(m => m.Title).Name("Title");
        Map(m => m.Paytype).Name("Paytype");
        Map(m => m.Date_of_birth).Name("Date_of_birth").TypeConverter<CustomDateTimeConverter>();
        Map(m => m.Gender).Name("Gender");
        Map(m => m.Airline_assosiated_with_frequent_flyer_programm).Name("Airline_assosiated_with_frequent_flyer_programm");
        Map(m => m.Frequent_flyer_number).Name("Frequent_flyer_number");
        Map(m => m.Infant_last_name).Name("Infant_last_name");
        Map(m => m.Infant_first_name).Name("Infant_first_name");
        Map(m => m.Date_of_birth_infant).Name("Date_of_birth_infant");
        Map(m => m.Associated_extra_seat).Name("Associated_extra_seat");
        Map(m => m.Travel_document_type).Name("Travel_document_type");
        Map(m => m.Document_issuing_country).Name("Document_issuing_country");
        Map(m => m.Country_code).Name("Country_code");
        Map(m => m.Travel_document_number).Name("Travel_document_number");
        Map(m => m.Passenger_nationality).Name("Passenger_nationality");
        Map(m => m.Code_of_nationality).Name("Code_of_nationality");
        Map(m => m.Date_of_expiry).Name("Date_of_expiry");
        Map(m => m.Teilnehmeranrede).Name("Teilnehmer anrede");
        Map(m => m.Teilnehmername).Name("Teilnehmer name");
        Map(m => m.Tl_geburtstag).Name("Tl-geburtstag").TypeConverter<CustomDateTimeConverter>();
        Map(m => m.Infounterbringung).Name("Info unterbringung");
        Map(m => m.Inforeise).Name("Info reise");
        Map(m => m.Ref1).Name("Ref1");
        Map(m => m.Ref2).Name("Ref2");
        Map(m => m.Teilnehmernr).Name("Teilnehmernr");
        Map(m => m.Zustieghinfahrt).Name("Zustieg hinfahrt");
        Map(m => m.Ausstiegrückfahrt).Name("Ausstieg rückfahrt");
        Map(m => m.Leistungstyp).Name("Leistungstyp");
        Map(m => m.Leistungsart).Name("Leistungsart");
        Map(m => m.Leistungscode).Name("Leistungscode");
        Map(m => m.Kontingentcode).Name("Kontingentcode");
        Map(m => m.Leistung).Name("Leistung");
        Map(m => m.Reiseleistungsnr).Name("Reiseleistungsnr");
        Map(m => m.Leistungsstammnr).Name("Leistungsstammnr");
        Map(m => m.Belegungsnr).Name("Belegungsnr");
        Map(m => m.Vorgangnr).Name("Vorgangnr");
    }
}

public class CustomDateTimeConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (text is null || text.Length == 0)
        {
            return null;
        }
        CultureInfo provider = CultureInfo.InvariantCulture;
        
        var result = DateTime.ParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture);

        return result;
    }
}