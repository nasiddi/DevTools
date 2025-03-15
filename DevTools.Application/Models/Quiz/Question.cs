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

    public bool IsActive { get; set; } = false;
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

public class Answer
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