using System;
using System.Collections.Generic;

namespace DevTools.Application.Models.Citadels;

public class Game
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? FinishTime { get; set; }

    public ICollection<Hand> Hands { get; set; }
    public ICollection<PlayerResult> PlayerResults { get; set; }
}