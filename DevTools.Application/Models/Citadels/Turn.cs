using System.ComponentModel.DataAnnotations.Schema;

namespace DevTools.Application.Models.Citadels;

public class Turn
{
    public int Id { get; set; }
    [ForeignKey("Hand")]
    public int HandId { get; set; }
    [ForeignKey("Player")]
    public int PlayerId { get; set; }
    public int CharacterNumber { get; set; }
    public int TargetCharacterNumber { get; set; }

    public Hand Hand { get; set; }
    public Player Player { get; set; }
}