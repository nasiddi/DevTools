using System;
using System.Threading.Tasks;
using DevTools.BackgroundTasks;
using DevTools.Data;
using DevTools.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class HueColorsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public HueColorsController(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("lights")]
        public async Task<IActionResult> GetLights()
        {
            return Ok(await _dbContext.HueColors.ToListAsync());
        }

        [HttpPost]
        [Route("light")]
        public async Task<IActionResult> SetLight([FromBody] HueColor hueColor)
        {
            var persistedHueColor = await _dbContext.HueColors.SingleAsync(c => c.HueId == hueColor.HueId);
            persistedHueColor.Color = hueColor.Color;
            persistedHueColor.DefaultColor = hueColor.DefaultColor;
            var mutation = await _dbContext.SaveChangesAsync();

            if (mutation <= 0)
            {
                return Ok();
            }
            
            var hueIrisColorTask = _serviceProvider.GetService<HueIrisColorTask>();
            if (hueIrisColorTask != null)
            {
                hueIrisColorTask.ColorChanged = true;
            }

            return Ok();
        }
    }
}