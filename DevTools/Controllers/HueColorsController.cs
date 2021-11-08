using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        [Route("status")]
        public async Task<IActionResult> GetStatus()
        {
            var flag = await _dbContext.Flags.SingleAsync(f => f.Name == "HueColorsTaskEnabled");
            return Ok(new Dictionary<string, bool>{{"isEnabled", flag.Flag}});
        }

        [HttpPost]
        [Route("set-enabled")]
        public async Task<IActionResult> SetEnabled([FromQuery] bool isEnabled)
        {
            var flag = await _dbContext.Flags.SingleAsync(f => f.Name == "HueColorsTaskEnabled");
            flag.Flag = isEnabled;
            var mutation = await _dbContext.SaveChangesAsync();
            var hueIrisColorTask = _serviceProvider.GetService<HueIrisColorTask>();
            if (hueIrisColorTask != null)
            {
                hueIrisColorTask.IsEnabled = isEnabled;
            }
            return Ok();
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