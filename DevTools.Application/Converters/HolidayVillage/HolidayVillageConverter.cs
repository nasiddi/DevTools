using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.HolidayVillage;

public static class HolidayVillageConverter
{
    public static ConvertedFile ConvertToHolidayVillage(IImmutableList<MasterListRow> records)
    {
        var bookings = MasterListParser.ParseHolidayVillage(records);
        var rows = MasterListExporter.ExportHolidayVillage(bookings);
        var content = ConvertToCsv(rows);
        return ConvertedFile.FromCsv(fileName: "masterlist.csv", content: content);
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