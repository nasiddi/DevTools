using System.Collections.Generic;
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
}