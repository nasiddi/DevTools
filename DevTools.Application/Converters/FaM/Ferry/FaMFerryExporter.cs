using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ratio_list_converter.Parser;

namespace DevTools.Application.Converters.FaM.Ferry;

public static class FaMFerryExporter
{
    private const string FontName = "Calibri";
    private const float DefaultSmallSize = 11;
    private const float DefaultBigSize = 14;
    private const int TableStartRowIndex = 12;
    private const int ColumnStartIndex = 2;


    public static ConvertedFile ExportFamFerryRows(IList<Booking> bookings)
    {
        var inboundTravelers = bookings
            .SelectMany(e => e.Participants.Select(p => new {Participant = p, e.BookingNumber}))
            .Where(e => e.Participant.InboundTravelInfo.Transport == Transport.Ferry)
            .GroupBy(e => e.Participant.InboundTravelInfo.Date)
            .ToDictionary(g => g.Key,
                g => g.Select(e => (Participant: e.Participant, BookingNumber: e.BookingNumber)).ToList());

        var outboundTravelers = bookings
            .SelectMany(e => e.Participants.Select(p => new {Participant = p, e.BookingNumber}))
            .Where(e => e.Participant.OutboundTravelInfo.Transport == Transport.Ferry)
            .GroupBy(e => e.Participant.OutboundTravelInfo.Date)
            .ToDictionary(g => g.Key,
                g => g.Select(e => (Participant: e.Participant, BookingNumber: e.BookingNumber)).ToList());


        using var memoryStream = new MemoryStream();
        ConvertToXlsx(inboundTravelers, outboundTravelers, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return ConvertedFile.FromXlsx("FerryVouchers.xlsx", memoryStream.ToArray());
    }

    private static void ConvertToXlsx(
        Dictionary<DateTime, List<(Participant Participant, int BookingNumber)>> inboundTravelers,
        Dictionary<DateTime, List<(Participant Participant, int BookingNumber)>> outboundTravelers,
        MemoryStream memoryStream)

    {
        var fileInfo = new FileInfo($"{AppContext.BaseDirectory}/Converters/FaM/Ferry/voucher.xlsx");
        using var excelPackage = new ExcelPackage(fileInfo);

        var dates = inboundTravelers.Select(e =>
                new KeyValuePair<DateTime, (bool IsInbound, IList<(Participant Participant, int BookingNumber)>
                    Participants)>(e.Key,
                    (true, e.Value.ToList())))
            .Union(outboundTravelers.Select(e =>
                new KeyValuePair<DateTime, (bool IsInbound, IList<(Participant Participant, int BookingNumber)>
                    Participants)>(e.Key,
                    (true, e.Value.ToList()))))
            .ToList();

        MapFerryInfo(excelPackage, dates);

        var template = excelPackage.Workbook.Worksheets[2];


        foreach (var date in dates)
        {
            MapDate(date, excelPackage, template);
        }

        excelPackage.Workbook.Worksheets.Delete("Template");
        excelPackage.SaveAs(memoryStream);
    }

    private static void MapDate(
        KeyValuePair<DateTime, (bool IsInbound, IList<(Participant Participant, int BookingNumber)> Participants)> date,
        ExcelPackage excelPackage, ExcelWorksheet template)
    {
        var isInbound = date.Value.IsInbound;
        var departurePort = isInbound ? "Ancona" : "Patras";
        var arrivalPort = !isInbound ? "Ancona" : "Patras";

        var datedSheet =
            excelPackage.Workbook.Worksheets.Copy("Template", $"{date.Key:dd.MM.yyyy} {departurePort} - {arrivalPort}");

        for (int i = 1; i <= 6; i++)
        {
            datedSheet.Column(i).Width = template.Column(i).Width;
        }

        var cabins = date.Value.Participants.ToList().GroupBy(e =>
            isInbound
                ? e.Participant.InboundTravelInfo.CabinReference
                : e.Participant.OutboundTravelInfo.CabinReference).ToList();

        var rowIndex = 1;

        foreach (var participants in cabins)
        {
            for (int i = 0; i <= 30; i++)
            {
                datedSheet.Row(rowIndex + i).Height = template.Row(i + 1).Height;
            }

            AddTitle(datedSheet, rowIndex, isInbound);
            AddIntro(datedSheet, rowIndex, isInbound);
            AddSummary(datedSheet, rowIndex, departurePort, date, template, arrivalPort, participants);

            AddTableHeader(datedSheet, rowIndex, ColumnStartIndex, "Cabin");
            AddTableHeader(datedSheet, rowIndex, 3, "Title");
            AddTableHeader(datedSheet, rowIndex, 4, "Name");
            AddTableHeader(datedSheet, rowIndex, 5, "First Name");
            AddTableHeader(datedSheet, rowIndex, 6, "DoB");

            var tableContextIndex = rowIndex + TableStartRowIndex + 1;

            foreach (var participant in participants)
            {
                AddTableRow(datedSheet, tableContextIndex, participant, isInbound);
                tableContextIndex++;
            }

            datedSheet.Cells[rowIndex + TableStartRowIndex, ColumnStartIndex, rowIndex + TableStartRowIndex, 6].Style
                .Fill
                .PatternType = ExcelFillStyle.Solid;
            datedSheet.Cells[rowIndex + TableStartRowIndex, ColumnStartIndex, rowIndex + TableStartRowIndex, 6].Style
                .Fill
                .BackgroundColor.SetColor(Color.FromArgb(50, 50, 50));
            datedSheet.Cells[rowIndex + TableStartRowIndex, ColumnStartIndex, rowIndex + TableStartRowIndex, 6].Style
                .Font
                .Color
                .SetColor(Color.White);
            datedSheet.Cells[rowIndex + TableStartRowIndex + 1, ColumnStartIndex, tableContextIndex - 1, 6].Style.Border
                    .Bottom.Style =
                ExcelBorderStyle.Thin;
            datedSheet.Cells[rowIndex + TableStartRowIndex + 1, ColumnStartIndex, tableContextIndex - 1, 6].Style.Border
                .Bottom.Color
                .SetColor(Color.Black);


            rowIndex += 38;
        }

        datedSheet.Column(7).Hidden = true;
    }

    private static void AddTableRow(ExcelWorksheet datedSheet, int tableContextIndex,
        (Participant Participant, int BookingNumber) participantAndBookingNumber,
        bool isInbound)
    {
        var participant = participantAndBookingNumber.Participant;

        datedSheet.Cells[tableContextIndex, ColumnStartIndex].Value =
            isInbound
                ? participant.InboundTravelInfo.CabinType?.ToCustomString() ?? string.Empty
                : participant.OutboundTravelInfo.CabinType?.ToCustomString() ?? string.Empty;

        var title = "";

        if (participant.DateOfBirth.HasValue)
        {
            int age = participant.InboundTravelInfo.Date.Year - participant.DateOfBirth.Value.Year;
            if (participant.DateOfBirth.Value > participant.InboundTravelInfo.Date.AddYears(-age)) age--;


            title = age switch
            {
                < 4 => "INF",
                < 17 => "CHLD",
                _ => "ADT"
            };
        }

        datedSheet.Cells[tableContextIndex, 3].Value = title;
        datedSheet.Cells[tableContextIndex, 4].Value = participant.FamilyName;
        datedSheet.Cells[tableContextIndex, 5].Value = participant.FirstName;
        datedSheet.Cells[tableContextIndex, 6].Value = participant.DateOfBirth ?? new DateTime();
        datedSheet.Cells[tableContextIndex, 6].Style.Numberformat.Format = "dd.MM.yyyy";
        datedSheet.Cells[tableContextIndex, 7].Value = participantAndBookingNumber.BookingNumber;
    }

    private static void AddTableHeader(ExcelWorksheet datedSheet, int rowIndex, int colIndex, string text)
    {
        datedSheet.Cells[rowIndex + TableStartRowIndex, colIndex].Value = text;
    }

    private static void AddSummary(
        ExcelWorksheet datedSheet,
        int rowIndex,
        string departurePort,
        KeyValuePair<DateTime, (bool IsInbound, IList<(Participant Participant, int BookingNumber)> Participants)> date,
        ExcelWorksheet template,
        string arrivalPort,
        IGrouping<int?, (Participant Participant, int BookingNumber)> participants)
    {
        AddHeader(datedSheet, rowIndex + 3, "Check in");
        AddLongValue(datedSheet, rowIndex + 3, "Empfohlene Einfindungszeit 2\u00bd Std. vor Abfahrt");

        AddHeader(datedSheet, rowIndex + 4, "Gepäck auf\nder Fähre");
        AddLongValue(datedSheet, rowIndex + 4,
            "Bitte packen Sie für die Nacht auf der Fähre eine separate Tasche bzw. einen separaten Koffer. Sobald die Fähre den Hafen verlassen hat, kommen Sie nicht mehr in die Tiefgarage zu Ihrem Auto.");

        AddHeader(datedSheet, rowIndex + 5, $"Abfahrt {departurePort}");
        AddValue(sheet: datedSheet, rowIndex: rowIndex + 5, colIndex: 4, content: $"{date.Key:dd.MM.yyyy}",
            size: DefaultBigSize, bold: true);
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 5,
            5,
            "DEP Time",
            $"{date.Key:dd.MM.yyyy}{departurePort}",
            DefaultBigSize,
            true,
            template.Cells[6, 5]);

        AddHeader(datedSheet, rowIndex + 6, $"Ankunft {arrivalPort}");
        AddValue(sheet: datedSheet, rowIndex: rowIndex + 6, colIndex: 4,
            content: $"{date.Key.AddDays(1):dd.MM.yyyy}", size: DefaultBigSize);
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 6,
            5,
            "ARR Time",
            $"{date.Key:dd.MM.yyyy}{departurePort}",
            DefaultBigSize,
            templateCell: template.Cells[7, 5]);

        AddHeader(datedSheet, rowIndex + 7, "Ihre Fähre");
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 7,
            4,
            "Ferry",
            $"{date.Key:dd.MM.yyyy}{departurePort}",
            DefaultBigSize);

        AddHeader(datedSheet, rowIndex + 8, "Ref.NR. Kabine");
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 8,
            4,
            "RefNo_Cab",
            $"{date.Key:dd.MM.yyyy}{departurePort}",
            DefaultBigSize);

        AddHeader(datedSheet, rowIndex + 9, "Ref.Nr. Fahrzeug");
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 9,
            4,
            "RefNo_Vehicule",
            $"{date.Key:dd.MM.yyyy}{departurePort}",
            DefaultBigSize);

        var carInfo = participants.Select(e => e.Participant).FirstOrDefault(e => e.FerryInfo?.Length > 0)?.FerryInfo;

        AddHeader(datedSheet, rowIndex + 10, "Info Fahrzeug");
        AddLongValue(datedSheet, rowIndex + 10, carInfo, DefaultBigSize);
    }

    private static void AddHeader(ExcelWorksheet sheet, int rowIndex, string content)
    {
        var cell = sheet.Cells[rowIndex, ColumnStartIndex, rowIndex, 3];
        cell.Merge = true;
        cell.Value = content;

        cell.Style.Font.Name = FontName;
        cell.Style.Font.Size = DefaultSmallSize;
        cell.Style.Font.Bold = true;
        cell.Style.WrapText = true;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
    }

    private static void AddLongValue(ExcelWorksheet sheet, int rowIndex, string? content, float size = DefaultSmallSize)
    {
        var cell = sheet.Cells[rowIndex, 4, rowIndex, 6];
        cell.Merge = true;
        cell.Value = content ?? string.Empty;
        cell.Style.Font.Name = FontName;
        cell.Style.Font.Size = size;
        cell.Style.WrapText = true;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
    }

    private static void AddValue(
        ExcelWorksheet sheet,
        int rowIndex,
        int colIndex,
        string content,
        float size = DefaultSmallSize,
        bool bold = false)
    {
        var cell = sheet.Cells[rowIndex, colIndex];
        cell.Value = content;
        cell.Style.Font.Name = FontName;
        cell.Style.Font.Size = size;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        cell.Style.Font.Bold = bold;
    }

    private static void AddValueFromFerryInfo(
        ExcelWorksheet sheet,
        int rowIndex,
        int colIndex,
        string header,
        string key,
        float size = DefaultSmallSize,
        bool bold = false,
        ExcelRange? templateCell = default)
    {
        var cell = sheet.Cells[rowIndex, colIndex];
        cell.Formula = $"INDEX(FerryInfo[{header}], MATCH(\"{key}\", FerryInfo[Key], 0))";
        cell.Style.Font.Name = FontName;
        cell.Style.Font.Size = size;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        cell.Style.Font.Bold = bold;

        if (templateCell is not null)
        {
            cell.Style.Numberformat.Format = templateCell.Style.Numberformat.Format;
        }
    }

    private static void AddIntro(ExcelWorksheet datedSheet, int rowIndex, bool isInbound)
    {
        var introCell = datedSheet.Cells[rowIndex + 2, ColumnStartIndex, rowIndex + 2, 6];
        introCell.Merge = true;
        introCell.Style.WrapText = true;
        introCell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;


        var introText = introCell.RichText;


        var first = introText.Add(isInbound
            ? "Sie halten das elektronische Ticket für Ihre Fährüberfahrt von ANCONA nach PATRAS in den Händen.\n"
            : "Sie halten das elektronische Ticket für Ihre Rückreise mit der Fähre von PATRAS nach ANCONA in den Händen.\n");

        first.Size = DefaultSmallSize;
        first.FontName = FontName;


        var second = introText.Add(isInbound
            ? "Bitte melden Sie sich mit diesem Dokument beim Ticketoffice in Ancona am Schalter von ANEK-Lines.\n"
            : "Bitte melden Sie sich mit diesem Dokument beim Ticketoffice in Patras am Schalter von ANEK-Lines.\n");

        second.Size = DefaultSmallSize;
        second.FontName = FontName;
        second.Bold = true;

        var third = introText.Add(
            "Lage und Anfahrt zum Ticketoffice sind in Ihren Reiseunterlagen detailliert beschrieben. Dort erhalten Sie nach dem Check-in die Boarding-Karten für die Fähre.");

        third.Size = DefaultSmallSize;
        third.FontName = FontName;
        third.Bold = false;
    }

    private static void AddTitle(ExcelWorksheet datedSheet, int rowIndex, bool isInbound)
    {
        var title = datedSheet.Cells[rowIndex + 1, ColumnStartIndex].RichText;
        datedSheet.Cells[rowIndex + 1, ColumnStartIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

        var lead = title.Add("Voucher für Fährticket | Anek-Superfast | ");
        lead.Size = 16.5f;
        lead.FontName = FontName;

        var ports = title.Add(isInbound ? "Hinfahrt Ancona - Patras" : "Rückfahrt Patras - Ancona");
        ports.Size = 16.5f;
        ports.FontName = FontName;
        ports.Bold = true;
    }

    private static void MapFerryInfo(ExcelPackage excelPackage,
        List<KeyValuePair<DateTime, (bool IsInbound, IList<(Participant Participant, int BookingNumber)> Participants)>>
            dates)
    {
        var ferryInfoSheet = excelPackage.Workbook.Worksheets[1];

        var table = ferryInfoSheet.Tables.First();

        var emptyRow = table.Address.End.Row;

        int rowIndex = table.Address.End.Row + 1;

        foreach (var (date, (isInbound, _)) in dates)
        {
            table.Address = new ExcelAddressBase(table.Address.Start.Row,
                table.Address.Start.Column,
                rowIndex,
                table.Address.End.Column);

            ferryInfoSheet.Cells[rowIndex, 3].Value = date;
            ferryInfoSheet.Cells[rowIndex, 5].Value = isInbound ? "Ancona" : "Patras";
            ferryInfoSheet.Cells[rowIndex, 6].Value = date.AddDays(1);
            ferryInfoSheet.Cells[rowIndex, 8].Value = !isInbound ? "Ancona" : "Patras";
            ferryInfoSheet.Cells[rowIndex, 11].Value = $"{date:dd.MM.yyyy}{(isInbound ? "Ancona" : "Patras")}";

            rowIndex++;
        }

        ferryInfoSheet.DeleteRow(emptyRow);
    }
}