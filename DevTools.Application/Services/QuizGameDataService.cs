using System;
using System.Linq;
using System.Threading.Tasks;
using DevTools.Application.Database;
using DevTools.Application.Models.Quiz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.Application.Services;

public class QuizGameDataService
{
    private readonly IServiceProvider _serviceProvider;

    
    public QuizGameDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public QuizShow QuizShow => _quizShow;

    private QuizShow _quizShow { get; set; } = null!;
    
    private static readonly object Lock = new object();

    public async Task LoadQuizShow()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        _quizShow = await GetActiveQuizShow(dbContext);
    }

    public async Task ResetQuiz()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);

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
        
        dbContext.RemoveRange(quizShow.Teams);
        await UpdateQuizShow(quizShow, dbContext);
    }

    public async Task ToggleQuizRegistration()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        quizShow.RegistrationIsOpen = !quizShow.RegistrationIsOpen;
        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task SetCurrentQuestionIndex(int questionIndex)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        quizShow.QuestionIndex = questionIndex;
        quizShow.QuestionStartTime = DateTime.UtcNow;
        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task LockInAnswer()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        
        foreach (var question in quizShow.Questions)
        {
            question.IsLockedIn = question.Index == quizShow.QuestionIndex;
        }

        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task SetCurrentAnswer(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);

        foreach (var question in quizShow.Questions)
        {
            foreach (var answer in question.Answers)
            {
                answer.IsSelectedByContestant = answer.Id == id;
            }
        }

        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task UseJoker(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        var joker = quizShow.Jokers.Single(e => e.Id == id);
        joker.QuestionIndex = quizShow.QuestionIndex;
        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task RegisterTeam(string name, Guid teamId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        quizShow.AddTeam(name, teamId);
        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task SetTeamAnswer(int teamId, int answerId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        quizShow.AddTeamAnswer(teamId, answerId);
        await UpdateQuizShow(quizShow, dbContext);
    }
    
    public async Task UseTeamJoker(int teamId, int jokerId)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevToolsContext>();
        var quizShow = await GetActiveQuizShow(dbContext);
        quizShow.UseTeamJoker(teamId, jokerId);
        await UpdateQuizShow(quizShow, dbContext);
    }
    
    private async Task<QuizShow> GetActiveQuizShow(DevToolsContext dbContext)
    {
        return await dbContext.QuizShows
            .Where(e => e.IsActive)
            .Include(e => e.Teams)
            .ThenInclude(e => e.Answers)
            .Include(e => e.Teams)
            .ThenInclude(e => e.Jokers)
            .Include(e => e.Jokers)
            .Include(e => e.Questions)
            .ThenInclude(e => e.Answers)
            .SingleAsync();
    }
    
    private async Task UpdateQuizShow(QuizShow quizShow, DevToolsContext dbContext)
    {
        await dbContext.SaveChangesAsync();
        
        foreach (var question in quizShow.Questions)
        {
            question.ShuffleAnswers();
        }

        lock (Lock)
        {
            _quizShow = quizShow;
        }
    }
}