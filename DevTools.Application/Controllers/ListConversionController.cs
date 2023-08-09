using System;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ratio_list_converter.Exporter;
using ratio_list_converter.Parser;

namespace ratio_list_converter.Controllers;

[ApiController]
[Route("[controller]")]
public class ListConversionController : ControllerBase
{
    [HttpPost(Name = "ConvertList")]
    public IActionResult GetMasterList([FromForm] IFormFileCollection file)
    {
        var bookings = MasterListParser.ParseMasterList(file[0].OpenReadStream());
        var rows = MasterListExporter.ExportMasterList(bookings);
        byte[] data;
        
        var config = new CsvConfiguration(new CultureInfo("de-CH"));
        
        using (var memoryStream = new MemoryStream())
        {
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, config))
            {
                var options = new TypeConverterOptions { Formats = new[] { "dd.MM.yyyy" } };
                csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                csvWriter.WriteRecords(rows);
            } 

            data = memoryStream.ToArray();
        }
        
        return File(data, "text/csv", "masterlist.csv");
    }
}