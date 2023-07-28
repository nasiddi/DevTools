using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DevTools.Application.Models.Citadels;

public class Statistics
{
    public IReadOnlyList<StatisticsCharacter> StatisticsCharacters { get; set; } = ImmutableList<StatisticsCharacter>.Empty;
    public IReadOnlyList<StatisticsGame> StatisticsGames { get; set; } = ImmutableList<StatisticsGame>.Empty;
}

public class StatisticsCharacter
{
    public string CharacterName { get; set; } = string.Empty;
    public IReadOnlyList<CharacterPlayed> CharacterPlayed { get; set; } = ImmutableList<CharacterPlayed>.Empty;
    public IReadOnlyList<CharacterAttack> CharacterAttacks { get; set; } = ImmutableList<CharacterAttack>.Empty;
}

public class CharacterPlayed
{
    public string PlayerName { get; set; } = string.Empty;
    public int PlayedInWonGame { get; set; }
    public int PlayedInLostGame { get; set; }
}

public class CharacterAttack
{
    public string PlayerName { get; set; } = string.Empty;
    public int SuccessfulAttacks { get; set; }
    public int UnsuccessfulAttacks { get; set; }
}

public class StatisticsGame
{
    public DateTime StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public IReadOnlyList<PlayerPoints> PlayerPoints { get; set; } = ImmutableList<PlayerPoints>.Empty;
    public int NumberOfHands { get; set; }
}

public class PlayerPoints
{
    public string PlayerName { get; set; } = string.Empty;
    public int Points { get; set; }
}