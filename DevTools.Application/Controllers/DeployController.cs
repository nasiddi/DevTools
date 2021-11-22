using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DevTools.Application.BackgroundTasks;
using DevTools.Application.Models;
using DevTools.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.Application.Controllers
{
    [ApiController]
    [ApiKey(false)]
    [Route("[controller]")]
    public class DeployController : ControllerBase
    {
        private readonly ISpaDeployService _spaDeployService;
        private readonly IServiceProvider _serviceProvider;
        
        public DeployController(ISpaDeployService spaDeployService, IServiceProvider serviceProvider)
        {
            _spaDeployService = spaDeployService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("commit")]
        public async Task<IActionResult> GetCommit()
        {
            try
            {
                var client = await _spaDeployService.GetFtpClient();
                await using var stream = new MemoryStream();
                await client.DownloadAsync(stream,
                    Path.Join(SpaDeployService.RemoteTarget, SpaDeployService.DeploymentFile));
                stream.Position = 0;
                var reader = new StreamReader(stream);
                string text = await reader.ReadToEndAsync();
                var commit = JsonSerializer.Deserialize<Commit>(text);
                return Ok(commit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Ok(new Commit("Error fetching last commit", e.Message, new DateTime()));
            }
            
        }

        [HttpPost]
        [Route("spa")]
        public  IActionResult DeploySpa()
        {
            var task = _spaDeployService.Deploy();
            return Ok();
            
        }

        [HttpPost]
        [Route("on-change")]
        public IActionResult DeployOnChange([FromQuery] bool deployOnChange)
        {
            var spaDeployTask = _serviceProvider.GetService<SpaDeployTask>();
            spaDeployTask!.IsEnabled = deployOnChange;
            return Ok();
        }

        [HttpGet]
        [Route("background-task")]
        public IActionResult GetBackgroundTaskStatus()
        {
            var spaDeployTask = _serviceProvider.GetService<SpaDeployTask>();
            return Ok(new BackgroundTaskStatus
            {
                IsEnabled = spaDeployTask!.IsEnabled,
                IsRunning = spaDeployTask.IsRunning
            });
        }
    }

    public class BackgroundTaskStatus
    {
        public bool IsEnabled { get; set; }
        public bool IsRunning { get; set; }
    }
}