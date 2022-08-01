using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DevTools.Application.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;
using File = DevTools.Application.Models.File;

namespace DevTools.Application.Services;

public interface IFileService
{ 
    Task<IReadOnlyList<FileUploadResult>> UploadFiles(IReadOnlyList<IFormFile> files);
    FileMetaInfo GetFileMetaInfo(string guid);
    FileDownloadResult GetFile(string guid);
    Task<IReadOnlyList<FileMetaInfo>> GetFileMetaInfos();
    Task RemoveFile(string guid);
}

public record FileMetaInfo(string Filename, int Size, string Guid);

public record FileUploadResult(
    string FileName, 
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    FileUploadResultType ResultType, 
    string? Guid = default,
    string? Error = default);

public record FileDownloadResult(
    string? Filename = default,
    string? Message = default,
    MemoryStream? Content = default);

public enum FileUploadResultType
{
    Success,
    Failed,
    NameExists
    
}
public class FileService : IFileService
{
    private const string RemotePath = "Downloads/FileUpload";

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FileService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public async Task<IReadOnlyList<FileUploadResult>> UploadFiles(IReadOnlyList<IFormFile> files)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();
        
        var persistedFileInfos = await PersistFileInfos(files, dbContext);
        
        var section = _configuration.GetSection("Rocinante");
        using var client = new SftpClient(section["Host"], section["Username"], section["Password"]);
        client.Connect();
        var results = files.Select(f => UploadFile(f, client, persistedFileInfos)).ToList();
        client.Disconnect();
        
        foreach (var result in results.Where(r => r.ResultType != FileUploadResultType.Success))
        {
            var file = persistedFileInfos.Single(i => i.FileName == result.FileName);
            dbContext.Remove(file);
        }

        await dbContext.SaveChangesAsync();
        return results;
    }

    private static async Task<IReadOnlyList<File>> PersistFileInfos(
        IReadOnlyList<IFormFile> files,
        DevToolsContext dbContext)
    {
        var filesToPersist = files.Select(f => new File
        {
            Guid = Guid.NewGuid().ToString("N"),
            FileName = f.FileName,
            Bytes = (int) f.Length
        }).ToList();

        await dbContext.AddRangeAsync(filesToPersist);
        await dbContext.SaveChangesAsync();
        return filesToPersist;
    }

    public FileMetaInfo GetFileMetaInfo(string guid)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var file = dbContext.Files.SingleOrDefault(f => f.Guid == guid);

        return file == null 
            ? new FileMetaInfo("Unknown File Id", 0, string.Empty)
            : new FileMetaInfo(file.FileName, file.Bytes, file.Guid);
    }
    
    public async Task<IReadOnlyList<FileMetaInfo>> GetFileMetaInfos()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var files = await dbContext.Files.ToListAsync();
        return files.Select(f => new FileMetaInfo(f.FileName, f.Bytes, f.Guid)).ToList();
    }

    public async Task RemoveFile(string guid)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var file = dbContext.Files.SingleOrDefault(f => f.Guid == guid);
        dbContext.Remove(file);

        if (file != null)
        {
            var section = _configuration.GetSection("Rocinante");
            using var client = new SftpClient(section["Host"], section["Username"], section["Password"]);
            client.Connect();
        
            client.DeleteFile(Path.Join(RemotePath, $"{guid}{Path.GetExtension(file.FileName)}"));
            client.Disconnect();
        }

        await dbContext.SaveChangesAsync();
    }

    public FileDownloadResult GetFile(string guid)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var file = dbContext.Files.SingleOrDefault(f => f.Guid == guid);

        if (file == null)
        {
            return new FileDownloadResult("Unknown File Id");
        }
        
        var section = _configuration.GetSection("Rocinante");
        using var client = new SftpClient(section["Host"], section["Username"], section["Password"]);
        client.Connect();
        var stream = new MemoryStream();
        client.DownloadFile(Path.Join(RemotePath, $"{guid}{Path.GetExtension(file.FileName)}"), stream);
        client.Disconnect();
        return new FileDownloadResult(file.FileName, Content: stream);
    }

    private static FileUploadResult UploadFile(
        IFormFile file,
        SftpClient client,
        IReadOnlyList<File> persistedFileInfos)
    {
        var fileInfo = persistedFileInfos.Single(i => i.FileName == file.FileName);
        var result = new FileUploadResult(file.FileName, FileUploadResultType.Success, fileInfo.Guid);
        var extension = Path.GetExtension(file.FileName);

        try
        {
            client.UploadFile(
                file.OpenReadStream(),
                Path.Join(RemotePath, $"{fileInfo.Guid}{extension}"));
        }
        catch (Exception e)
        {
            return result with {ResultType = FileUploadResultType.Failed, Error = e.Message};
        }

        return result;
    }
}