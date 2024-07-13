using System.Collections.Generic;
using System.Collections.Immutable;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FaM.Ferry;

public class FaMFerryConverter
{
    public static IReadOnlyList<ConvertedFile> ConvertToFaM(IImmutableList<MasterListRow> records)
    {
        var bookings = MasterListParser.ParseFam(records);
        


        return new List<ConvertedFile> {FaMFerryExporter.ExportFamFerryRows(bookings)};
    }
}