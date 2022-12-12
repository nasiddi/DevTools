using System.ComponentModel.DataAnnotations.Schema;

namespace DevTools.Application.Models.Citadels;

public class PlayerResult
{
    public int Id { get; set; }
    [ForeignKey("Game")]
    public int GameId { get; set; }
    [ForeignKey("Player")]
    public int PlayerId { get; set; }
    public bool HasWon { get; set; }
    public int Points { get; set; }
    
    public Game Game { get; set; }
    public Player Player { get; set; }
}