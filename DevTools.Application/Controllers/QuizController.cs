using System;
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
            .Include(e => e.Teams)
            .ThenInclude(e => e.Answers)
            .Include(e => e.Teams)
            .ThenInclude(e => e.Jokers)
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
            .Include(e => e.Teams)
            .ThenInclude(e => e.Answers)
            .Include(e => e.Teams)
            .ThenInclude(e => e.Jokers)
            .Include(e => e.Jokers)
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .SingleAsync();

        quizShow.QuestionIndex = 0;
        quizShow.QuestionStartTime = null;
        
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
        
        _dbContext.RemoveRange(quizShow.Teams);

        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("registration/toggle")]
    public async Task<IActionResult> ToggleQuizRegistration()
    {
        var quizShow = await GetActiveQuizShow().SingleAsync();
        quizShow.RegistrationIsOpen = !quizShow.RegistrationIsOpen;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("questions/current")]
    public async Task<IActionResult> SetCurrentQuestionIndex([FromQuery]int questionIndex)
    {
        
        var quizShow = await GetActiveQuizShow().SingleAsync();
        
        quizShow.QuestionIndex = questionIndex;
        quizShow.QuestionStartTime = DateTime.Now;
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
    
    [HttpPost]
    [Route("teams/register")]
    public async Task<IActionResult> RegisterTeam([FromQuery] string name, [FromQuery] Guid teamId)
    {
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Jokers)
            .SingleAsync();
        
        quizShow.AddTeam(name, teamId);
        
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("teams/{teamId}/answer/{answerId}")]
    public async Task<IActionResult> SetTeamAnswer(int teamId, int questionId, int answerId)
    {
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .Include(e => e.Teams)
            .ThenInclude(e => e.Answers)
            .SingleAsync();
        
        quizShow.AddTeamAnswer(teamId, answerId);
        
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost]
    [Route("teams/{teamId}/jokers/{jokerId}/use")]
    public async Task<IActionResult> UseTeamJoker(int teamId, int jokerId)
    {
        var quizShow = await GetActiveQuizShow()
            .Include(e => e.Teams)
            .ThenInclude(e => e.Jokers)
            .SingleAsync();
        
        quizShow.UseTeamJoker(teamId, jokerId);
        
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    private IQueryable<QuizShow> GetActiveQuizShow()
    {
        return _dbContext.QuizShows
            .Where(e => e.IsActive);
    }
}
