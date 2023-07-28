using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DevTools.Application.Database;
using DevTools.Application.Enums.Citadels;
using DevTools.Application.Models.Citadels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.Application.Services;

public interface ICitadelsService
{
    Task<IReadOnlyList<Character>> GetActiveCharacters();
    Task SetActiveCharacters(IReadOnlyList<Character> characters);
    Task<IReadOnlyList<Player>> GetAllPlayers();
    Task StartGame(IReadOnlyList<Player> players);
    Task<Game?> GetActiveGame();
    Task<IReadOnlyList<Player>> GetActivePlayers();
    IReadOnlyList<Character> GetAllCharacters();
    Task SubmitTurns(IReadOnlyList<NewTurn> turns);
    Task EndGame(IReadOnlyList<NewPlayerResult> playerResults);
    Task<Statistics> GetStatistics();
}

public class CitadelsService : ICitadelsService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CitadelsService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<IReadOnlyList<Character>> GetActiveCharacters()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        return await SelectActiveCharacters(dbContext);
    }

    public async Task SetActiveCharacters(IReadOnlyList<Character> characters)
    {
        ValidateCharacters(characters);

        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var activeCharacters = await SelectActiveCharacters(dbContext);

        var charactersToAdd = characters.ToList();

        foreach (var character in activeCharacters)
        {
            var update = characters.SingleOrDefault(c => c.CharacterNumber == character.CharacterNumber);

            if (character.CharacterType == update?.CharacterType)
            {
                charactersToAdd.Remove(update);
                continue;
            }

            character.DeactivationDate = DateTime.UtcNow;
        }

        await dbContext.AddRangeAsync(charactersToAdd);

        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Player>> GetAllPlayers()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        return await dbContext.Players.ToListAsync();
    }

    public async Task StartGame(IReadOnlyList<Player> players)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var currentPlayers = await dbContext.Players.ToListAsync();

        foreach (var currentPlayer in currentPlayers)
        {
            var update = players.Single(p => p.Id == currentPlayer.Id);
            currentPlayer.IsActive = update.IsActive;
        }

        await dbContext.AddRangeAsync(players.Where(p => p.Id == 0));

        var game = new Game
        {
            StartTime = DateTime.UtcNow,
        };

        await dbContext.AddAsync(game);

        await dbContext.SaveChangesAsync();
    }

    public async Task<Game?> GetActiveGame()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        return await GetActiveGame(dbContext);
    }

    private async Task<Game?> GetActiveGame(DevToolsContext dbContext)
    {
        return await dbContext.Games.SingleOrDefaultAsync(e => e.FinishTime == null);
    }

    public async Task<IReadOnlyList<Player>> GetActivePlayers()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        return await dbContext.Players.Where(p => p.IsActive).ToListAsync();
    }

    public async Task SubmitTurns(IReadOnlyList<NewTurn> turns)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var game = await GetActiveGame(dbContext);

        var hand = new Hand
        {
            GameId = game!.Id,
            Turns = turns.Select(t => new Turn
            {
                PlayerId = t.PlayerId,
                CharacterNumber = t.CharacterNumber,
                TargetCharacterNumber = t.TargetCharacterNumber,
            }).ToList()
        };

        await dbContext.AddAsync(hand);
        await dbContext.SaveChangesAsync();
    }

    public async Task EndGame(IReadOnlyList<NewPlayerResult> playerResults)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var game = await GetActiveGame(dbContext);

        game!.PlayerResults = playerResults.Select(p => new PlayerResult
        {
            PlayerId = p.PlayerId,
            HasWon = p.HasWon,
            Points = p.Points,
        }).ToList();

        game.FinishTime = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
    }

    public async Task<Statistics> GetStatistics()
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider
            .GetRequiredService<DevToolsContext>();

        var statisticsCharacters = await GetStatisticsCharacters(dbContext);

        var games = await dbContext.Games
            .Include(e => e.PlayerResults)
            .ThenInclude(e => e.Player)
            .Include(e => e.Hands)
            .ToListAsync();

        var statisticsGames = games.Select(e => new StatisticsGame
            {
                StartTime = e.StartTime,
                FinishTime = e.FinishTime,
                PlayerPoints = e.PlayerResults.Select(r => new PlayerPoints
                {
                    PlayerName = r.Player.Name,
                    Points = r.Points
                }).ToImmutableList(),
                NumberOfHands = e.Hands.Count
            })
            .OrderBy(e => e.StartTime)
            .ToImmutableList();

        return new Statistics
        {
            StatisticsCharacters = statisticsCharacters,
            StatisticsGames = statisticsGames
        };
    }

    private async Task<List<StatisticsCharacter>> GetStatisticsCharacters(DevToolsContext dbContext)
    {
        var turns = await dbContext.Turns
            .Include(e => e.Hand)
            .ThenInclude(e => e.Game)
            .ThenInclude(e => e.PlayerResults)
            .Include(e => e.Player)
            .ToListAsync();

        var characters = await dbContext.Characters.ToListAsync();

        var statisticsTurns = turns.Select(turn =>
        {
            var game = turn.Hand.Game;

            var character = characters.Single(e =>
                e.CharacterNumber == turn.CharacterNumber
                && e.ActivationDate < game.StartTime
                && (e.DeactivationDate is null ||
                    e.DeactivationDate > game.FinishTime));

            return new StatisticsTurn(
                character.CharacterType,
                character.CharacterNumber,
                turn.Hand.Game.PlayerResults.Single(r => r.PlayerId == turn.PlayerId).HasWon,
                turn.Player.Name,
                turn.HandId,
                turn.TargetCharacterNumber
            );
        }).ToImmutableList();


        var statisticsCharacters = statisticsTurns.GroupBy(e => e.CharacterType)
            .OrderBy(e => e.Key)
            .Select(e => new StatisticsCharacter
            {
                CharacterName = e.Key.ToString(),
                CharacterPlayed = e.GroupBy(g => g.PlayerName).Select(MapCharacterPlayed).ToImmutableList(),
                CharacterAttacks = e.GroupBy(g => g.PlayerName).Select(g => MapCharacterAttack(g, statisticsTurns)).ToImmutableList()
            }).ToList();
        return statisticsCharacters;
    }

    private CharacterAttack MapCharacterAttack(
        IGrouping<string, StatisticsTurn> group,
        ImmutableList<StatisticsTurn> statisticsTurns)
    {
        var attacks = group.Where(e => e.TargetCharacterNumber > 0).ToImmutableList();

        if (attacks.Count == 0)
        {
            return new CharacterAttack
            {
                PlayerName = group.Key,
                SuccessfulAttacks = 0,
                UnsuccessfulAttacks = 0
            };
        }
        
        var successful = attacks.Count(IsSuccessfulAttack);

        return new CharacterAttack
        {
            PlayerName = group.Key,
            SuccessfulAttacks = successful,
            UnsuccessfulAttacks = attacks.Count - successful
        };

        bool IsSuccessfulAttack(StatisticsTurn turn)
        {
            var characterNumbersInHand = statisticsTurns.Where(t => t.HandId == turn.HandId).Select(t => t.CharacterNumber).ToImmutableList();
            return characterNumbersInHand
                .Contains(turn.TargetCharacterNumber);
        }
    }

    private CharacterPlayed MapCharacterPlayed(IGrouping<string, StatisticsTurn> group)
    {
        var won = group.Count(e => e.HasWon);

        return new CharacterPlayed
        {
            PlayerName = group.Key,
            PlayedInWonGame = won,
            PlayedInLostGame = group.Count() - won
        };
    }

    private void ValidateCharacters(IReadOnlyList<Character> characters)
    {
        var allCharacters = GetAllCharacters();

        foreach (var character in characters)
        {
            var validCharacter = allCharacters.SingleOrDefault(
                c =>
                    c.CharacterNumber == character.CharacterNumber
                    && c.CharacterType == character.CharacterType);

            if (validCharacter is null)
            {
                throw new InvalidOperationException(
                    $"Invalid character number ({character.CharacterNumber}) and type combination ({character.CharacterType.ToString()})");
            }
        }
    }

    private static async Task<IReadOnlyList<Character>> SelectActiveCharacters(DevToolsContext dbContext)
    {
        return await dbContext.Characters
            .Where(e => e.DeactivationDate == null)
            .ToListAsync();
    }

    public IReadOnlyList<Character> GetAllCharacters()
    {
        return ImmutableList.Create(
            new Character
            {
                CharacterNumber = 1,
                CharacterType = CharacterType.Assassin,
            },
            new Character
            {
                CharacterNumber = 1,
                CharacterType = CharacterType.Witch,
            },
            new Character
            {
                CharacterNumber = 2,
                CharacterType = CharacterType.Thief,
            },
            new Character
            {
                CharacterNumber = 2,
                CharacterType = CharacterType.TaxCollector,
            },
            new Character
            {
                CharacterNumber = 3,
                CharacterType = CharacterType.Magician,
            },
            new Character
            {
                CharacterNumber = 3,
                CharacterType = CharacterType.Wizard,
            },
            new Character
            {
                CharacterNumber = 4,
                CharacterType = CharacterType.King,
            },
            new Character
            {
                CharacterNumber = 4,
                CharacterType = CharacterType.Emperor,
            },
            new Character
            {
                CharacterNumber = 5,
                CharacterType = CharacterType.Bishop,
            },
            new Character
            {
                CharacterNumber = 5,
                CharacterType = CharacterType.Abbot,
            },
            new Character
            {
                CharacterNumber = 6,
                CharacterType = CharacterType.Merchant,
            },
            new Character
            {
                CharacterNumber = 6,
                CharacterType = CharacterType.Alchemist,
            },
            new Character
            {
                CharacterNumber = 7,
                CharacterType = CharacterType.Architect,
            },
            new Character
            {
                CharacterNumber = 7,
                CharacterType = CharacterType.Navigator,
            },
            new Character
            {
                CharacterNumber = 8,
                CharacterType = CharacterType.Warlord,
            },
            new Character
            {
                CharacterNumber = 8,
                CharacterType = CharacterType.Diplomat,
            },
            new Character
            {
                CharacterNumber = 9,
                CharacterType = CharacterType.Artist,
            },
            new Character
            {
                CharacterNumber = 9,
                CharacterType = CharacterType.Queen,
            });
    }

    private record StatisticsTurn(
        CharacterType CharacterType,
        int CharacterNumber,
        bool HasWon,
        string PlayerName,
        int HandId,
        int TargetCharacterNumber);
}