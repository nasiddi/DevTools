using System;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.RoomList;

public class RoomListExporter
{
    private const int LastColumnIndex = 11;

    public static ConvertedFile ExportRoomListExportRows(ImmutableList<Room> rooms)

    {
        var groupedRooms = rooms.GroupBy(e => e.NumberOfGuests)
            .OrderBy(e => e.Key)
            .ToImmutableList();

        using var memoryStream = new MemoryStream();
        ConvertToXlsx(groupedRooms, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return ConvertedFile.FromXlsx("RoomList.xlsx", memoryStream.ToArray());
    }

    private static void ConvertToXlsx(
        ImmutableList<IGrouping<int, Room>> groupedRooms,
        MemoryStream memoryStream)
    {
        var fileInfo = new FileInfo($"{AppContext.BaseDirectory}/Converters/RoomList/roomlist.xlsx");
        using var excelPackage = new ExcelPackage(fileInfo);
        var sheet = excelPackage.Workbook.Worksheets[1];


        var tripStartDate = groupedRooms.SelectMany(e => e.SelectMany(r => r.Guests.Select(g => g.TripStartDate)))
            .Min();
        var tripEndDate = groupedRooms.SelectMany(e => e.SelectMany(r => r.Guests.Select(g => g.TripEndDate))).Max();

        sheet.Cells[4, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        sheet.Cells[4, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        sheet.Cells[4, 10].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FF0000"));
        sheet.Cells[4, 9, 4, 10].Style.Font.Bold = true;

        sheet.Cells[4, 9].Value = "total guests:";
        sheet.Cells[4, 10].Value = groupedRooms.Sum(e => e.Sum(d => d.NumberOfGuests));
        
        sheet.Cells[4, 13].Value = tripStartDate;
        sheet.Cells[5, 13].Value = tripEndDate;
        sheet.Cells[4, 13, 5, 13].Style.Numberformat.Format = "dd.mm.yyyy";

        var rowIndex = 5;
        
        foreach (var groupedRoom in groupedRooms)
        {
            sheet.Cells[rowIndex, 5].Value = GetRoomName(groupedRoom.Key);
            sheet.Cells[rowIndex, 6].Value = "Anzahl";
            sheet.Cells[rowIndex, 7].Value = groupedRoom.Count();
            sheet.Cells[rowIndex, 8].Formula = $"=IFERROR(F{rowIndex}-G{rowIndex},\"\")";
            rowIndex++;
        }

        rowIndex = 15;

        var columns = typeof(RoomListExportRow)
            .GetProperties()
            .Select(i => (ColumnData: i, ColumnMeta: i.GetCustomAttribute<RoomListAttribute>()))
            .Where(i => i.ColumnMeta != null)
            .Select(i => (ColumnData: i.ColumnData, ColumnMeta: i.ColumnMeta!))
            .ToImmutableList();

        foreach (var roomGroup in groupedRooms)
        {
            rowIndex += 2;
            SetTableHeader(
                sheet: sheet,
                columns: columns,
                rowIndex: rowIndex,
                name: GetRoomName(roomGroup.Key),
                roomCount: roomGroup.Count(),
                guestCount: roomGroup.Sum(e => e.NumberOfGuests));
            rowIndex += 2;
            var index = 1;

            foreach (var room in roomGroup)
            {
                var rows = room.Guests.Select(e => new RoomListExportRow
                {
                    Index = index,
                    LastName = e.FamilyName,
                    FirstName = e.FirstName,
                    Title = MapTitle(e.Gender),
                    DateOfBirth = e.DateOfBirth,
                    PassportNumber = e.PassportNumber,
                    Nationality = e.Nationality,
                    ExpirationDate = e.ExpirationDate,
                    Comments = e.Comments,
                    DepartureDate = e.TripEndDate,
                    Age = e.Age
                }).ToImmutableList();

                index++;

                foreach (var row in rows)
                {
                    foreach (var column in columns)
                    {
                        var columnIndex = column.ColumnMeta.Position;
                        var content = column.ColumnData.GetValue(row);
                        sheet.Cells[rowIndex, columnIndex].Value = content;

                        if (column.ColumnMeta.IsDate)
                        {
                            sheet.Cells[rowIndex, columnIndex].Style.Numberformat.Format = "dd.mm.yy";
                        }
                    }

                    rowIndex++;
                }

                var indexCells = sheet.Cells[rowIndex - room.NumberOfGuests, 1, rowIndex - 1, 1];
                indexCells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                indexCells.Merge = true;

                var roomCells = sheet.Cells[rowIndex - room.NumberOfGuests, 1, rowIndex - 1, LastColumnIndex];
                roomCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                roomCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                sheet.Cells[rowIndex - 1, 1, rowIndex - 1, LastColumnIndex].Style.Border.Bottom.Style =
                    ExcelBorderStyle.Thin;
            }
        }

        excelPackage.SaveAs(memoryStream);
    }

    private static void SetTableHeader(
        ExcelWorksheet sheet,
        ImmutableList<(PropertyInfo ColumnData, RoomListAttribute ColumnMeta)> columns,
        int rowIndex,
        string name,
        int roomCount,
        int guestCount)
    {
        sheet.Cells[rowIndex, 1, rowIndex + 1, LastColumnIndex].Style.Fill.BackgroundColor
            .SetColor(ColorTranslator.FromHtml("#D9D9D9"));
        sheet.Cells[rowIndex, 1, rowIndex, LastColumnIndex].Style.Border.Top.Style = ExcelBorderStyle.Medium;
        sheet.Cells[rowIndex, 1, rowIndex + 1, 1].Style.Border.Left.Style = ExcelBorderStyle.Medium;
        sheet.Cells[rowIndex, LastColumnIndex, rowIndex + 1, LastColumnIndex].Style.Border.Right.Style =
            ExcelBorderStyle.Medium;
        sheet.Cells[rowIndex + 1, 1, rowIndex + 1, LastColumnIndex].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;

        sheet.Cells[rowIndex, 2, rowIndex, 5].Style.Font.Bold = true;

        sheet.Cells[rowIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        sheet.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        sheet.Cells[rowIndex, 3].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FF0000"));

        sheet.Cells[rowIndex, 2].Value = $"{name}:";
        sheet.Cells[rowIndex, 3].Value = roomCount;

        sheet.Cells[rowIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        sheet.Cells[rowIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        sheet.Cells[rowIndex, 5].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FF0000"));

        sheet.Cells[rowIndex, 4].Value = "guests:";
        sheet.Cells[rowIndex, 5].Value = guestCount;

        foreach (var (_, columnMeta) in columns)
        {
            sheet.Cells[rowIndex + 1, columnMeta.Position].Value = columnMeta.Title;
        }
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

    private static string GetRoomName(int count)
    {
        var name = Enum.IsDefined(typeof(RoomSize), count) ? ((RoomSize) count).ToString().ToLower() : $"{count}-bed";
        return $"{name} rooms";
    }

    private enum RoomSize
    {
        Single = 1,
        Double = 2,
        Triple = 3
    }
}