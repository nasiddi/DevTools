using System.Collections.Immutable;
using DevTools.Application.Converters.RoomList;

namespace ratio_list_converter.Parser.RoomList;

public class RoomListConverter
{
    public static ConvertedFile ConvertToRoomList(IImmutableList<MasterListRow> records)
    {
        var rooms = MasterListParser.Parse(records);
        return RoomListExporter.ExportRoomListExportRows(rooms);
    }

}