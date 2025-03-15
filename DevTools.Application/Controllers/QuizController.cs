using System.Linq;
using System.Threading.Tasks;
using DevTools.Application.Database;
using DevTools.Application.Models.Quiz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly DevToolsContext _dbContext;

    public QuizController(DevToolsContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]
    [Route("questions")]
    public async Task<IActionResult> GetQuestions()
    {

        var quizShow = await GetActiveQuizShow()
            .AsNoTracking()
            .Where(e => e.IsActive)
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .SingleAsync();
        
        foreach (var question in quizShow.Questions)
        {
            question.ShuffleAnswers();
        }

        return Ok(quizShow.Questions);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetQuizShow()
    {
        var quizShow = await GetActiveQuizShow()
            .AsNoTracking()
            .Include(e => e.Jokers)
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .SingleAsync();
        
        foreach (var question in quizShow.Questions)
        {
            question.ShuffleAnswers();
        }

        return Ok(quizShow);
    }
    
    [HttpPost]
    [Route("reset")]
    public async Task<IActionResult> ResetQuizShow()
    {
       
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Jokers)
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .SingleAsync();

        quizShow.QuestionIndex = 1;
        
        foreach (var question in quizShow.Questions)
        {
            question.IsLockedIn = false;
            foreach (var answer in question.Answers)
            {
                answer.IsSelectedByContestant = false;
            }
        }
        
        foreach (var joker in quizShow.Jokers)
        {
            joker.QuestionIndex = null;
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("questions/current")]
    public async Task<IActionResult> SetCurrentQuestionIndex([FromQuery]int questionIndex)
    {
        
        var quizShow = await GetActiveQuizShow().SingleAsync();
        
        quizShow.QuestionIndex = questionIndex;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("questions/current/locked-in")]
    public async Task<IActionResult> LockInAnswer()
    {
       
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Questions)
            .SingleAsync();

        
        foreach (var question in quizShow.Questions)
        {
            question.IsLockedIn = question.Index == quizShow.QuestionIndex;
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("answers/{id}/current")]
    public async Task<IActionResult> SetCurrentAnswer([FromRoute] int id)
    {
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .SingleAsync();

        foreach (var question in quizShow.Questions)
        {
            foreach (var answer in question.Answers)
            {
                answer.IsSelectedByContestant = answer.Id == id;
            }
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("jokers/{id}/use")]
    public async Task<IActionResult> UseJoker([FromRoute] int id)
    {
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Jokers)
            .SingleAsync();

        var joker = quizShow.Jokers.Single(e => e.Id == id);
        joker.QuestionIndex = quizShow.QuestionIndex;

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    private IQueryable<QuizShow> GetActiveQuizShow()
    {
        return _dbContext.QuizShows
            .Where(e => e.IsActive);
    }
}
