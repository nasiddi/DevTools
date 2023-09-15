using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using DevTools.Application.Converters.FlightList;
using DevTools.Application.Converters.HolidayVillage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ratio_list_converter.Parser;
using ratio_list_converter.Parser.RoomList;

namespace ratio_list_converter.Controllers;

[ApiController]
[Route("[controller]")]
public class ListConversionController : ControllerBase
{
    [HttpPost(Name = "ConvertList")]
    public IActionResult GetMasterList([FromForm] IFormFileCollection file, [FromQuery] IReadOnlyList<string> fileTypes)
    {
        ImmutableList<ListType> listTypes;

        if (fileTypes.Count == 0)
        {
            return BadRequest(fileTypes);
        }

        if (file.Count != 1 || file[0].ContentType != "text/csv")
        {
            return BadRequest(file);
        }

        try
        {
            listTypes = fileTypes.Select(GetListTypes).ToImmutableList();
        }
        catch (ArgumentException)
        {
            return BadRequest(fileTypes);
        }

        var files = ConvertFiles(file, listTypes);

        if (files.Count == 1)
        {
            var singleFile = files.Single();

            return File(singleFile.MimeType == MimeType.Csv
                    ? Encoding.UTF8.GetBytes(files.Single().Content!)
                    : files.Single().Stream!, GetContentType(singleFile.MimeType),
                files.Single().FileName);
        }

        using var memoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var convertedFile in files)
            {
                AddFileToZip(zipArchive, convertedFile);
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return File(memoryStream.ToArray(), GetContentType(MimeType.Zip), $"{DateTime.Now:yyMMdd-hhmm}_{file[0].FileName.Split(".")[0]}.zip");
    }

    private static List<ConvertedFile> ConvertFiles(IFormFileCollection file, ImmutableList<ListType> listTypes)
    {
        var rows = CsvUtils.LoadRows(file[0].OpenReadStream());

        var files = new List<ConvertedFile>();

        if (listTypes.Contains(ListType.FerienDorfMasterList))
        {
            files.Add(HolidayVillageConverter.ConvertToHolidayVillage(rows));
        }
        
        if (listTypes.Contains(ListType.RoomList))
        {
            files.Add(RoomListConverter.ConvertToRoomList(rows));
        }

        if (listTypes.Contains(ListType.FlightList))
        {
            files.AddRange(LufthansaGroupPaxConverter.ConvertToLufthansaGroupPax(rows));
        }

        return files;
    }

    private ListType GetListTypes(string fileType)
    {
        if (Enum.TryParse(fileType, true, out ListType listType))
        {
            return listType;
        }

        throw new ArgumentException();
    }

    private static void AddFileToZip(ZipArchive zipArchive, ConvertedFile convertedFile)
    {
        var entry = zipArchive.CreateEntry(convertedFile.FileName);
        using var entryStream = entry.Open();

        if (convertedFile.MimeType == MimeType.Csv)
        {
            var streamWriter = new StreamWriter(entryStream);
            streamWriter.Write(convertedFile.Content);
            streamWriter.Flush();
            return;
        }

        var memoryStream = new MemoryStream(convertedFile.Stream!);
        memoryStream.CopyTo(entryStream);
    }

    private string GetContentType(MimeType mimeType)
    {
        switch (mimeType)
        {
            case MimeType.Zip:
                return "application/zip";
            case MimeType.Csv:
                return "text/csv";
            case MimeType.Xslx:
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            default:
                throw new ArgumentOutOfRangeException(nameof(mimeType), mimeType, null);
        }
    }
}

public enum ListType
{
    FerienDorfMasterList,
    FlightList,
    RoomList
}