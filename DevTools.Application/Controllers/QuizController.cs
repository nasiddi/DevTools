using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevTools.Application.Database;
using DevTools.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public QuizController(IFileService fileService, IServiceScopeFactory serviceScopeFactory)
    {
        _fileService = fileService;
        _serviceScopeFactory = serviceScopeFactory;
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
    
    [HttpGet]
    [Route("questions")]
    public async Task<IActionResult> GetQuestions()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var questions = await dbContext.Questions.Include(e => e.Answers).ToListAsync();

        return Ok(questions);
    }
    
    [HttpGet]
    [Route("questions/current")]
    public async Task<IActionResult> GetCurrentQuestionIndex()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var quizShow = await dbContext.QuizShows.SingleAsync();

        return Ok(quizShow);
    }
    
    [HttpPost]
    [Route("questions/current")]
    public async Task<IActionResult> SetCurrentQuestionIndex([FromQuery]int questionIndex)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var quizShow = await dbContext.QuizShows.SingleAsync();
        
        quizShow.QuestionIndex = questionIndex;
        await dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("answers/{id}/current")]
    public async Task<IActionResult> SetCurrentAnswer([FromRoute] int id)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var questions = await dbContext.Questions
            .Include(e => e.Answers)
            .ToListAsync();

        foreach (var question in questions)
        {
            foreach (var answer in question.Answers)
            {
                answer.IsSelectedByContestant = answer.Id == id;
            }
        }

        await dbContext.SaveChangesAsync();
        return Ok();
    }
}
