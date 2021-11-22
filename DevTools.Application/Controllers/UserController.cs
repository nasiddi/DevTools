using System.Threading.Tasks;
using DevTools.Application.Database;
using DevTools.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DevTools.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase    {
        private readonly DevToolsContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserController(DevToolsContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("verify")]
        public async Task<IActionResult> VerifyUser([FromBody] User user)
        {
            var persistedUser = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Username == user.Username && u.PasswordHash == user.PasswordHash);

            if (persistedUser == null)
            {
                return Unauthorized();
            }
            
            var apiKey = _configuration.GetValue<string>(persistedUser.IsAdmin ? "AdminApiKey" : "ApiKey");
            return Ok(apiKey);
        }
    }
}