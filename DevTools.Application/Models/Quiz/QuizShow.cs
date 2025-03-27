using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace DevTools.Application.Models.Quiz;

public class QuizShow
{
    [Key]
    public int Id { get; set; }
    public int QuestionIndex { get; set; }
    
    public ICollection<Question> Questions { get; set; } = new HashSet<Question>();
    public ICollection<Joker> Jokers { get; set; } = new HashSet<Joker>();
    public ICollection<Team> Teams { get; set; } = new HashSet<Team>();

    public bool IsActive { get; set; } = false;
    public bool RegistrationIsOpen { get; set; } = false;
    
    
    public DateTime? QuestionStartTime { get; set; }

    public void AddTeam(string name, Guid teamId)
    {
        Teams.Add(new Team { TeamId = teamId, Name = name, Jokers = new List<TeamJoker>
        {
            new TeamJoker
            {
                JokerType = JokerType.Half,
                QuestionIndex = null
            },
            new TeamJoker
            {
                JokerType = JokerType.Poll,
                QuestionIndex = null
            },

        }});
    }

    public void AddTeamAnswer(int teamId, int answerId)
    {
        var question = Questions.Single(e => e.Index == QuestionIndex);
        var answerTime = DateTime.Now - QuestionStartTime!;
        
        var team = Teams.Single(e => e.Id == teamId);
        team.AddAnswer(QuestionIndex, answerId, question.Answers.Single(a => a.Id == answerId).IsCorrect, answerTime.Value.Milliseconds);
    }

    public void UseTeamJoker(int teamId, int jokerId)
    {
        var team = Teams.Single(e => e.Id == teamId);
        team.UseJoker(jokerId, QuestionIndex);
    }
}

public class Joker
{
    public int Id { get; set; }
    public JokerType JokerType { get; set; }
    [ForeignKey("QuizShow")] 
    public int QuizShowId { get; set; } = 1;
    public int? QuestionIndex { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JokerType
{
    Half,
    Poll,
    Phone
}public class Answer
 {
     public int Id { get; set; }
     [ForeignKey("Question")]
     public int QuestionId { get; set; }
     [StringLength(150)]
     public string AnswerText { get; set; } = null!;
     public bool IsCorrect { get; set; }
     public bool IsSelectedByContestant { get; set; }
 
     public bool IsInHalfJoker { get; set; } = false;
 }

public class Question
{
    public int Id { get; set; }
    [ForeignKey("QuizShow")] 
    public int QuizShowId { get; set; } = 1;
    public int Index { get; set; }
    [StringLength(150)]
    public string QuestionText { get; set; } = null!;
    [StringLength(30)]
    public string Amount { get; set; } = null!;
    public bool IsLockedIn { get; set; } = false;

    public ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
    
    public void ShuffleAnswers()
    {
        var random = new Random(Index);
        Answers = Answers.OrderBy(a => random.Next()).ToList();
    }
}



public class Team
{
    public int Id { get; set; }
    [ForeignKey("QuizShow")] 
    public int QuizShowId { get; set; } = 1;
    [MaxLength(20)]
    public string Name { get; init; } = null!;
    
    public Guid TeamId { get; init; }
    
    public ICollection<TeamAnswer> Answers { get; set; } = new List<TeamAnswer>();
    public ICollection<TeamJoker> Jokers { get; set; } = new List<TeamJoker>();

    public void AddAnswer(int questionIndex, int answerId, bool isCorrect, int answerTime)
    {
        Answers.Add(new TeamAnswer
        {
            QuestionIndex = questionIndex,
            IsCorrect = isCorrect,
            AnswerId = answerId,
            AnswerTimeMilliseconds = answerTime
        });
    }

    public void UseJoker(int jokerId, int questionIndex)
    {
        Jokers.Single(e => e.Id == jokerId).QuestionIndex = questionIndex;
    }
}

public class TeamAnswer
{
    public int Id { get; set; }
    [ForeignKey("Team")]
    public int TeamId { get; set; }
    public int QuestionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int AnswerId { get; set; }
    public int AnswerTimeMilliseconds { get; set; }
}

public class TeamJoker
{
    public int Id { get; init; }
    public JokerType JokerType { get; init; }
    [ForeignKey("Team")] 
    public int TeamId { get; init; }
    public int? QuestionIndex { get; set; }
    
}