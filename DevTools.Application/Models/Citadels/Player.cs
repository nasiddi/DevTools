using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace DevTools.Application.Models.Citadels;

public class Player
{
    public int Id { get; set; }
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<Turn> Turns { get; set; } = new List<Turn>();
    public ICollection<PlayerResult> PlayerResults { get; set; } = new List<PlayerResult>();

}