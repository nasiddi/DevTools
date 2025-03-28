using System;
using System.Threading.Tasks;
using DevTools.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevTools.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly QuizGameDataService _quizGameDataService;

    public QuizController(QuizGameDataService quizGameDataService)
    {
        _quizGameDataService = quizGameDataService;
    }

    [HttpGet]
    public IActionResult GetQuizShow()
    {
        return Ok(_quizGameDataService.QuizShow);
    }

    [HttpPost]
    [Route("reset")]
    public async Task<IActionResult> ResetQuizShow()
    {
        await _quizGameDataService.ResetQuiz();
        return Ok();
    }
    
    [HttpPost]
    [Route("jokers/reset")]
    public async Task<IActionResult> ResetJokers()
    {
        await _quizGameDataService.ResetJokers();
        return Ok();
    }
    
    [HttpPost]
    [Route("reload")]
    public async Task<IActionResult> ReloadQuizShow()
    {
        await _quizGameDataService.LoadQuizShow();
        return Ok();
    }

    [HttpPost]
    [Route("registration/toggle")]
    public async Task<IActionResult> ToggleQuizRegistration()
    {
        await _quizGameDataService.ToggleQuizRegistration();
        return Ok();
    }

    [HttpPost]
    [Route("questions/current")]
    public async Task<IActionResult> SetCurrentQuestionIndex([FromQuery] int questionIndex)
    {
        await _quizGameDataService.SetCurrentQuestionIndex(questionIndex);
        return Ok();
    }

    [HttpPost]
    [Route("questions/current/locked-in")]
    public async Task<IActionResult> LockInAnswer()
    {
        await _quizGameDataService.LockInAnswer();
        return Ok();
    }

    [HttpPost]
    [Route("answers/{id}/current")]
    public async Task<IActionResult> SetCurrentAnswer([FromRoute] int id)
    {
        await _quizGameDataService.SetCurrentAnswer(id);
        return Ok();
    }

    [HttpPost]
    [Route("jokers/{id}/use")]
    public async Task<IActionResult> UseJoker([FromRoute] int id)
    {
        await _quizGameDataService.UseJoker(id);
        return Ok();
    }

    [HttpPost]
    [Route("teams/register")]
    public async Task<IActionResult> RegisterTeam([FromQuery] string name, [FromQuery] Guid teamId)
    {
        await _quizGameDataService.RegisterTeam(name, teamId);
        return Ok();
    }

    [HttpPost]
    [Route("teams/{teamId}/answer/{answerId}")]
    public async Task<IActionResult> SetTeamAnswer(int teamId, int answerId)
    {
        await _quizGameDataService.SetTeamAnswer(teamId, answerId);
        return Ok();
    }

    [HttpPost]
    [Route("teams/{teamId}/jokers/{jokerId}/use")]
    public async Task<IActionResult> UseTeamJoker(int teamId, int jokerId)
    {
        await _quizGameDataService.UseTeamJoker(teamId, jokerId);
        return Ok();
    }
}