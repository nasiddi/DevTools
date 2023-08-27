using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DevTools.Application.Models.Citadels;
using DevTools.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevTools.Application.Controllers;

[ApiController]
[ApiKey(true)]
[Route("[controller]")]
public class CitadelsController : ControllerBase
{
    private readonly ICitadelsService _citadelsService;

    public CitadelsController(ICitadelsService citadelsService)
    {
        _citadelsService = citadelsService;
    }

    [HttpPost]
    [Route("characters")]
    public async Task<OkResult> SetActiveCharacters([FromBody] IReadOnlyList<Character> characters)
    {
        await _citadelsService.SetActiveCharacters(characters);
        return Ok();
    }

    [HttpGet]
    [Route("characters/active")]
    public async Task<IReadOnlyList<Character>> GetActiveCharacters()
    {
        return await _citadelsService.GetActiveCharacters();
    }

    [HttpGet]
    [Route("characters/all")]
    public IReadOnlyList<Character> GetAllCharacters()
    {
        return _citadelsService.GetAllCharacters();
    }

    [HttpGet]
    [Route("players/all")]
    public async Task<IReadOnlyList<Player>> GetAllPlayers()
    {
        return await _citadelsService.GetAllPlayers();
    }

    [HttpGet]
    [Route("players/active")]
    public async Task<IReadOnlyList<Player>> GetActivePlayers()
    {
        return await _citadelsService.GetActivePlayers();
    }

    [HttpPost]
    [Route("game/start")]
    public async Task StartGame(IReadOnlyList<Player> players)
    {
        await _citadelsService.StartGame(players);
    }

    [HttpGet]
    [Route("game/active")]
    public async Task<Game?> GetActiveGame()
    {
        return await _citadelsService.GetActiveGame();
    }

    [HttpPost]
    [Route("turns/submit")]
    public async Task SubmitTurns([FromBody] IReadOnlyList<NewTurn> turns)
    {
        await _citadelsService.SubmitTurns(turns);
    }

    [HttpPost]
    [Route("game/end")]
    public async Task EndGame([FromBody] IReadOnlyList<NewPlayerResult> playerResults)
    {
        await _citadelsService.EndGame(playerResults);
    }

    [HttpGet]
    [Route("statistic-data")]
    public async Task<Statistics> GetStatistics()
    {

        await Task.CompletedTask;

        var statistics = new Statistics
        {
            StatisticsCharacters = ImmutableList.Create(
                new StatisticsCharacter
                {
                    CharacterName = "Assassin",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInWonGame = 1
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInWonGame = 2
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "Thief",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 4
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 3
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "Magician",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 5,
                            PlayedInWonGame = 2
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 6,
                            PlayedInWonGame = 3
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "King",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 8,
                            PlayedInWonGame = 2
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 9,
                            PlayedInWonGame = 3
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "Bishop",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 11,
                            PlayedInWonGame = 2
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 10,
                            PlayedInWonGame = 3
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "Merchant",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 12,
                            PlayedInWonGame = 2
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 13,
                            PlayedInWonGame = 3
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "Architect",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 13,
                            PlayedInWonGame = 2
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 14,
                            PlayedInWonGame = 3
                        })
                },
                new StatisticsCharacter
                {
                    CharacterName = "Warlord",
                    CharacterPlayed = ImmutableList.Create(
                        new CharacterPlayed
                        {
                            PlayerName = "Pascal",
                            PlayedInLostGame = 11,
                            PlayedInWonGame = 2
                        },
                        new CharacterPlayed
                        {
                            PlayerName = "Nadina",
                            PlayedInLostGame = 11,
                            PlayedInWonGame = 2
                        })
                })
        };
        // return statistics;
        return await _citadelsService.GetStatistics();

    }
}