using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FaM;

public static class FaMConverter
{
    public static IReadOnlyList<ConvertedFile> ConvertToFaM(IImmutableList<MasterListRow> records)
    {
        var bookings = MasterListParser.ParseFam(records);
        var rows = MasterListExporter.ExportFaM(bookings);
        var content = ConvertToCsv(rows);
        var masterList = ConvertedFile.FromCsv(fileName: "masterlist.csv", content: content);

        var staff = bookings.SelectMany(e => e.Participants).Where(e => e.Staff is not null).ToList();
        var staffRows = MasterListExporter.ExportFaMStaff(staff);
        var staffContent = ConvertToCsv(staffRows);
        var masterListStaff = ConvertedFile.FromCsv(fileName: "masterlistStaff.csv", content: staffContent);


        return new List<ConvertedFile> {masterList, masterListStaff};
    }

    private static string ConvertToCsv(ImmutableList<MasterListStaffExportRow> rows)
    {
        var config = new CsvConfiguration(new CultureInfo("de-CH"));

        using var memoryStream = new MemoryStream();
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, config))
        {
            csvWriter.WriteRecords(rows);
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    private static string ConvertToCsv(ImmutableList<MasterListExportRow> rows)
    {
        var config = new CsvConfiguration(new CultureInfo("de-CH"));

        using var memoryStream = new MemoryStream();
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, config))
        {
            var options = new TypeConverterOptions {Formats = new[] {"dd.MM.yyyy"}};
            csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
            csvWriter.WriteRecords(rows);
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}