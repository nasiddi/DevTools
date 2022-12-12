using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DevTools.Application.Models.Citadels;

public class Hand
{
    public int Id { get; set; }
    [ForeignKey("Game")]
    public int GameId { get; set; }
    
    public Game Game { get; set; }
    public ICollection<Turn> Turns { get; set; }

}