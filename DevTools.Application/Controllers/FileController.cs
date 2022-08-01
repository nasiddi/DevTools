using System.Linq;
using System.Threading.Tasks;
using DevTools.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevTools.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }
    
    [HttpGet]
    [Route("{guid}")]
    public IActionResult GetFile([FromRoute] string guid)
    {
        var fileDownloadResult = _fileService.GetFile(guid);

        if (fileDownloadResult.Message != null)
        {
            return Ok(fileDownloadResult.Message);
        }

        var stream = fileDownloadResult.Content!;

        stream.Position = 0;
        return File(stream, "application/octec-stream", fileDownloadResult.Filename);
    }
    
    [HttpGet]
    [Route("{guid}/meta-info")]
    public IActionResult GetFileMetaInfo([FromRoute] string guid)
    {
        return Ok(_fileService.GetFileMetaInfo(guid));
    }

    [HttpPost]
    [ApiKey(true)]
    public async Task<IActionResult> ImportFile()
    {
        var files = Request.Form.Files.ToList();
        var results = await _fileService.UploadFiles(files);
        return Ok(results);
    }

    [HttpGet]
    [Route("meta-info")]
    public async Task<IActionResult> GetAllMetaInfos()
    {
        var fileMetaInfos = await _fileService.GetFileMetaInfos();
        return Ok(fileMetaInfos);
    }

    [HttpPost]
    [Route("{guid}/remove")]

    public async Task<IActionResult> RemoveFile(string guid)
    {
        await _fileService.RemoveFile(guid);
        return Ok();
    }
}