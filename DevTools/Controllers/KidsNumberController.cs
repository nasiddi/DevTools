using System.Threading.Tasks;
using DevTools.Data;
using DevTools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevTools.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KidsNumberController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public KidsNumberController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetNumber()
        {
            return Ok(await GetNumberEntity());
        }
        
        [HttpPost]
        public async Task<IActionResult> SetNumber([FromQuery] int number)
        {
            var numberEntity = await GetNumberEntity();
            numberEntity.Number = number;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
        
        private async Task<KidsNumber> GetNumberEntity()
        {
            return await _dbContext.KidsNumber.SingleAsync();
        }
    }
}