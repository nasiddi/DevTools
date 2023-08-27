using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace ratio_list_converter.Parser;

public static class CsvUtils
{
    public static IImmutableList<MasterListRow> LoadRows(Stream file)
    {
        var config = new CsvConfiguration(new CultureInfo("de-CH"));

        using var reader = new StreamReader(file, Encoding.GetEncoding("iso-8859-1"));
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<MasterListRowClassMap>();
        var records = csv.GetRecords<MasterListRow>().ToList();
        return records.ToImmutableList();
    }
}