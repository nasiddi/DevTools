using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FlightList;

public record LufthansaGroupPax(
    string? FrequentFlyerProgram,
    string? FrequentFlyerNumber,
    Person Person,
    Person? Infant);

public record Person(
    string LastName,
    string FirstNAme,
    string MiddleName,
    Gender? Gender,
    DateTime? DateOfBirth);

public class LufthansaGroupPaxExporter
{
    public static ConvertedFile ExportLufthansaGroupPaxExportRows(KeyValuePair<Flight, List<LufthansaGroupPax>> paxList)

    {
        var passengers = paxList.Value.Select((e, index) =>
        {
            var isChild = e.Person.DateOfBirth?.AddYears(12) > paxList.Key.DepartureDate;

            return new LufthansaGroupPaxExportRow
            {
                Index = index + 1,
                LastName = e.Person.LastName,
                FirstName = e.Person.FirstNAme,
                MiddleName = e.Person.MiddleName,
                Title = MapTitle(e.Person.Gender),
                PaxType = isChild ? "CHD" : null,
                DateOfBirth = isChild ? e.Person.DateOfBirth?.ToString("dd-MMM-yyyy").ToUpper() : null,
                Gender = null,
                FrequentFlyerProgram = e.FrequentFlyerProgram,
                FrequentFlyerNumber = e.FrequentFlyerNumber,
                InfantLastName = e.Infant?.LastName,
                InfantFirstName = e.Infant?.FirstNAme,
                InfantDateOfBirth = e.Infant?.DateOfBirth?.ToString("dd-MMM-yyyy").ToUpper(),
                AssociatedExtraSeat = null,
                TravelDocumentType = null,
                DocumentIssuingCountry = null,
                CountryCode = null,
                TravelDocumentNumber = null,
                PassengerNationality = null,
                CodeOfNationality = null,
                ExpirationDate = null
            };
        }).ToImmutableList();
        
        using var memoryStream = new MemoryStream();
        ConvertToXlsx(passengers, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return ConvertedFile.FromXlsx($"Pax_{paxList.Key.DepartureDate:yyyy-MM-dd}_{paxList.Key.FlightCode}.xlsx", memoryStream.ToArray());
    }
    
    private static void ConvertToXlsx(ImmutableList<LufthansaGroupPaxExportRow> rows, MemoryStream memoryStream)
    {
        var fileInfo = new FileInfo($"{AppContext.BaseDirectory}/Converters/FlightList/flightlist_lhg.xlsx");
        using var excelPackage = new ExcelPackage(fileInfo);
        var sheet = excelPackage.Workbook.Worksheets[1];

        var columns = typeof(LufthansaGroupPaxExportRow)
            .GetProperties()
            .Select(i => (ColumnData: i, ColumnMeta: i.GetCustomAttribute<LufthansaGroupAttribute>()))
            .Where(i => i.ColumnMeta != null)
            .Select(i => (ColumnData: i.ColumnData, ColumnMeta: i.ColumnMeta!))
            .ToImmutableList();

        var rowIndex = 2;
        
        foreach (var row in rows)
        {
            foreach (var column in columns)
            {
                var columnIndex = column.ColumnMeta.Position;
                var content = column.ColumnData.GetValue(row);
                sheet.Cells[rowIndex, columnIndex].Value = content;
            }
        
            rowIndex++;
        }

        var table = sheet.Tables[0];
        var oldAddress = table.Address;
        var newAddress = new ExcelAddressBase(oldAddress.Start.Row, oldAddress.Start.Column, rowIndex - 1, oldAddress.End.Column);
        table.TableXml.InnerXml = table.TableXml.InnerXml.Replace(oldAddress.ToString(), newAddress.ToString());

        excelPackage.SaveAs(memoryStream);
    }

    private static string MapTitle(Gender? gender)
    {
        if (gender is null)
        {
            return string.Empty;
        }
        
        switch (gender)
        {
            case Gender.Male:
                return "MR";
            case Gender.Female:
                return "MRS";
            default:
                throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
        }
    }
}