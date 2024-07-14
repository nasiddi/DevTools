using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ratio_list_converter.Parser;
using ExcelHorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment;
using ExcelVerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment;

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

        var ferryBookings = bookings.Select(SelectBookingNumberWithCabinReferences).OrderBy(e => e.BookingNumber)
            .Where(e => e.FerryInfos.Count > 0)
            .ToList();


        using var memoryStream = new MemoryStream();
        ConvertToXlsx(ferryBookings, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return ConvertedFile.FromXlsx("FerryVouchers.xlsx", memoryStream.ToArray());
    }

    private static FerryBooking SelectBookingNumberWithCabinReferences(Booking booking)
    {
        var inboundFerryInfos = booking.Participants.Where(e => e.InboundTravelInfo.Transport == Transport.Ferry)
            .Select(e => new FerryInfo(
                e.InboundTravelInfo.Date,
                e.InboundTravelInfo.CabinReference,
                e.InboundTravelInfo.CabinType!.Value,
                e.FamilyName,
                e.FirstName,
                e.DateOfBirth,
                e.FerryInfo,
                true))
            .ToList();

        var outboundFerryInfos = booking.Participants.Where(e => e.OutboundTravelInfo.Transport == Transport.Ferry)
            .Select(e => new FerryInfo(
                e.OutboundTravelInfo.Date,
                e.OutboundTravelInfo.CabinReference,
                e.OutboundTravelInfo.CabinType!.Value,
                e.FamilyName,
                e.FirstName,
                e.DateOfBirth,
                e.FerryInfo,
                false))
            .ToList();

        return new FerryBooking(
            booking.BookingNumber,
            inboundFerryInfos.Union(outboundFerryInfos).ToList());
    }

    private static void ConvertToXlsx(
        List<FerryBooking> ferryBookings,
        MemoryStream memoryStream)

    {
        var fileInfo = new FileInfo($"{AppContext.BaseDirectory}/Converters/FaM/Ferry/voucher.xlsx");
        using var excelPackage = new ExcelPackage(fileInfo);

        MapFerryInfo(excelPackage, ferryBookings);

        var template = excelPackage.Workbook.Worksheets[2];

        foreach (var ferryBooking in ferryBookings)
        {
            var cabinReferences = ferryBooking.FerryInfos.Select(e => e.CabinReference);
            var additionalPassengers = ferryBookings.Where(e => e.BookingNumber != ferryBooking.BookingNumber).SelectMany(e => e.FerryInfos)
                .Where(e => cabinReferences.Contains(e.CabinReference)).ToList();

            MapBookings(excelPackage, ferryBooking, additionalPassengers, template);
        }

        excelPackage.Workbook.Worksheets.Single(e => e.Name == "Template").Hidden = eWorkSheetHidden.VeryHidden;
        excelPackage.SaveAs(memoryStream);
    }

    private static void MapBookings(ExcelPackage excelPackage, FerryBooking ferryBooking, List<FerryInfo> additionalPassengers, ExcelWorksheet template)
    {
        var passengers = ferryBooking.FerryInfos.Union(additionalPassengers).OrderByDescending(e => e.IsInbound)
            .ThenBy(e => e.CabinReference).ToList();
        
        var sheet = excelPackage.Workbook.Worksheets.Copy("Template", $"{ferryBooking.BookingNumber}");
        var cabins = passengers.GroupBy(e => (e.IsInbound, e.CabinReference, e.Date)).ToList();

        var rowIndex = 1;
       
        foreach (var cabin in cabins)
        {
            FormatPage(sheet, template, rowIndex);

            var isInbound = cabin.Key.IsInbound;
            var departurePort = isInbound ? "Ancona" : "Patras";
            var arrivalPort = !isInbound ? "Ancona" : "Patras";
            
            AddTitle(sheet, rowIndex, isInbound);
            AddIntro(sheet, rowIndex, isInbound);
            AddSummary(sheet, rowIndex, departurePort, cabin.Key.Date, template, arrivalPort, cabin);

            AddTableHeader(sheet, rowIndex, ColumnStartIndex, "Cabin");
            AddTableHeader(sheet, rowIndex, 3, "Title");
            AddTableHeader(sheet, rowIndex, 4, "Name");
            AddTableHeader(sheet, rowIndex, 5, "First Name");
            AddTableHeader(sheet, rowIndex, 6, "DoB");

            var tableContextIndex = rowIndex + TableStartRowIndex + 1;

            foreach (var participant in cabin)
            {
                AddTableRow(sheet, tableContextIndex, participant);
                tableContextIndex++;
            }

            sheet.Cells[rowIndex + TableStartRowIndex, ColumnStartIndex, rowIndex + TableStartRowIndex, 6].Style
                .Fill
                .PatternType = ExcelFillStyle.Solid;
            sheet.Cells[rowIndex + TableStartRowIndex, ColumnStartIndex, rowIndex + TableStartRowIndex, 6].Style
                .Fill
                .BackgroundColor.SetColor(Color.FromArgb(50, 50, 50));
            sheet.Cells[rowIndex + TableStartRowIndex, ColumnStartIndex, rowIndex + TableStartRowIndex, 6].Style
                .Font
                .Color
                .SetColor(Color.White);
            sheet.Cells[rowIndex + TableStartRowIndex + 1, ColumnStartIndex, tableContextIndex - 1, 6].Style.Border
                    .Bottom.Style =
                ExcelBorderStyle.Thin;
            sheet.Cells[rowIndex + TableStartRowIndex + 1, ColumnStartIndex, tableContextIndex - 1, 6].Style.Border
                .Bottom.Color
                .SetColor(Color.Black);

            sheet.Row(tableContextIndex + cabin.Count()).PageBreak = true;
            rowIndex = tableContextIndex + cabin.Count() + 1;
            
        }

        sheet.Column(7).Hidden = true;
    }

    public static void FormatPage(ExcelWorksheet datedSheet, ExcelWorksheet template, int rowIndex)
    {
        for (int i = 0; i <= 30; i++)
        {
            datedSheet.Row(rowIndex + i).Height = template.Row(i + 1).Height;
        }

        MergeCells(datedSheet, rowIndex);
    }

    private static void AddTableRow(ExcelWorksheet datedSheet, int tableContextIndex, FerryInfo ferryInfo)
    {
        datedSheet.Cells[tableContextIndex, ColumnStartIndex].Value = ferryInfo.CabinType.ToCustomString() ?? string.Empty;

        var title = "";

        if (ferryInfo.DateOfBirth.HasValue)
        {
            var age = ferryInfo.Date.Year - ferryInfo.DateOfBirth.Value.Year;
            if (ferryInfo.DateOfBirth.Value > ferryInfo.Date.AddYears(-age)) age--;


            title = age switch
            {
                < 4 => "INF",
                < 17 => "CHLD",
                _ => "ADT"
            };
        }

        datedSheet.Cells[tableContextIndex, 3].Value = title;
        datedSheet.Cells[tableContextIndex, 4].Value = ferryInfo.FamilyName;
        datedSheet.Cells[tableContextIndex, 5].Value = ferryInfo.FirstName;
        datedSheet.Cells[tableContextIndex, 6].Value = ferryInfo.DateOfBirth ?? new DateTime();
        datedSheet.Cells[tableContextIndex, 6].Style.Numberformat.Format = "dd.MM.yyyy";
    }

    private static void AddTableHeader(ExcelWorksheet datedSheet, int rowIndex, int colIndex, string text)
    {
        datedSheet.Cells[rowIndex + TableStartRowIndex, colIndex].Value = text;
    }

    private static void AddSummary(
        ExcelWorksheet datedSheet,
        int rowIndex,
        string departurePort,
        DateTime date,
        ExcelWorksheet template,
        string arrivalPort,
        IGrouping<(bool IsInbound, int? CabinReference, DateTime Date), FerryInfo> participants)
    {
        AddHeader(datedSheet, rowIndex + 3, "Check in");
        AddLongValue(datedSheet, rowIndex + 3, "Empfohlene Einfindungszeit 2\u00bd Std. vor Abfahrt");

        AddHeader(datedSheet, rowIndex + 4, "Gepäck auf\nder Fähre");
        AddLongValue(datedSheet, rowIndex + 4,
            "Bitte packen Sie für die Nacht auf der Fähre eine separate Tasche bzw. einen separaten Koffer. Sobald die Fähre den Hafen verlassen hat, kommen Sie nicht mehr in die Tiefgarage zu Ihrem Auto.");

        AddHeader(datedSheet, rowIndex + 5, $"Abfahrt {departurePort}");
        AddValue(sheet: datedSheet, rowIndex: rowIndex + 5, colIndex: 4, content: $"{date:dd.MM.yyyy}",
            size: DefaultBigSize, bold: true);
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 5,
            5,
            "DEP Time",
            $"{date:dd.MM.yyyy}{departurePort}",
            DefaultBigSize,
            true,
            template.Cells[6, 5]);

        AddHeader(datedSheet, rowIndex + 6, $"Ankunft {arrivalPort}");
        AddValue(sheet: datedSheet, rowIndex: rowIndex + 6, colIndex: 4,
            content: $"{date.AddDays(1):dd.MM.yyyy}", size: DefaultBigSize);
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 6,
            5,
            "ARR Time",
            $"{date:dd.MM.yyyy}{departurePort}",
            DefaultBigSize,
            templateCell: template.Cells[7, 5]);

        AddHeader(datedSheet, rowIndex + 7, "Ihre Fähre");
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 7,
            4,
            "Ferry",
            $"{date:dd.MM.yyyy}{departurePort}",
            DefaultBigSize);

        AddHeader(datedSheet, rowIndex + 8, "Ref.NR. Kabine");
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 8,
            4,
            "RefNo_Cab",
            $"{date:dd.MM.yyyy}{departurePort}",
            DefaultBigSize);

        AddHeader(datedSheet, rowIndex + 9, "Ref.Nr. Fahrzeug");
        AddValueFromFerryInfo(
            datedSheet,
            rowIndex + 9,
            4,
            "RefNo_Vehicule",
            $"{date:dd.MM.yyyy}{departurePort}",
            DefaultBigSize);

        var carInfo = participants.Select(e => e.CarInfo).FirstOrDefault(e => e?.Length > 0);

        AddHeader(datedSheet, rowIndex + 10, "Info Fahrzeug");
        AddLongValue(datedSheet, rowIndex + 10, carInfo, DefaultBigSize);
    }

    private static void AddHeader(ExcelWorksheet sheet, int rowIndex, string content)
    {
        var cell = sheet.Cells[rowIndex, ColumnStartIndex, rowIndex, 3];
        cell.Value = content;
        cell.Style.Font.Name = FontName;
        cell.Style.Font.Size = DefaultSmallSize;
        cell.Style.Font.Bold = true;
        cell.Style.WrapText = true;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
    }

    private static void MergeCells(ExcelWorksheet sheet, int rowIndex)
    {
        // LongValues
        MergeCell(sheet, rowIndex + 3, 4, 6);
        MergeCell(sheet, rowIndex + 4, 4, 6);
        MergeCell(sheet, rowIndex + 10, 4, 6);

        // Intro
        MergeCell(sheet, rowIndex + 2, ColumnStartIndex, 6);

        // Headers
        for (int i = 3; i <= 10; i++)
        {
            MergeCell(sheet, rowIndex + i, ColumnStartIndex, 3);
        }
    }

    private static void MergeCell(ExcelWorksheet sheet, int rowIndex, int startCol, int endCol)
    {
        sheet.Cells[rowIndex, startCol, rowIndex, endCol].Merge = true;
    }

    private static void AddLongValue(ExcelWorksheet sheet, int rowIndex, string? content,
        float size = DefaultSmallSize)
    {
        var cell = sheet.Cells[rowIndex, 4, rowIndex, 6];
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
        introCell.Style.WrapText = true;
        introCell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        introCell.RichText.Clear();

        var introText = introCell.RichText;


        var first = introText.Add(isInbound
            ? "Sie halten das elektronische Ticket für Ihre Fährüberfahrt von ANCONA nach PATRAS in den Händen.\n"
            : "Sie halten das elektronische Ticket für Ihre Fährüberfahrt von PATRAS nach ANCONA in den Händen.\n");

        first.Size = DefaultSmallSize;
        first.FontName = FontName;
        first.Bold = false;


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

    private static void MapFerryInfo(
        ExcelPackage excelPackage,
        List<FerryBooking> ferryBookings)
    {
        var ferryInfoSheet = excelPackage.Workbook.Worksheets[1];
        var table = ferryInfoSheet.Tables.First();
        var emptyRow = table.Address.End.Row;
        var rowIndex = table.Address.End.Row + 1;
        var dates = ferryBookings.SelectMany(e => e.FerryInfos.Select(i => (i.Date, i.IsInbound))).Distinct().OrderBy(e => e.Date);

        foreach (var (date, isInbound) in dates)
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

    private record FerryBooking(int BookingNumber, IList<FerryInfo> FerryInfos);

    private record FerryInfo(DateTime Date, int? CabinReference, CabinType CabinType, string FamilyName,
        string FirstName, DateTime? DateOfBirth, string? CarInfo, bool IsInbound);
}