using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevTools.Application.Models.Quiz;

public class QuizShow
{
    [Key]
    public int Id { get; set; }
    public int QuestionIndex { get; set; }
}

public class Question
{
    public int Id { get; set; }
    public int Index { get; set; }
    [StringLength(150)]
    public string QuestionText { get; set; } = null!;
    [StringLength(30)]
    public string Amount { get; set; } = null!;

    public ICollection<Answer> Answers { get; set; } = new HashSet<Answer>();
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
}