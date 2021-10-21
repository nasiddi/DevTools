﻿using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using DevTools.BackgroundTasks;
using DevTools.Models;
using DevTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevTools.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DeployController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISpaDeployService _spaDeployService;
        private readonly IServiceProvider _serviceProvider;
        
        public DeployController(IConfiguration configuration, ISpaDeployService spaDeployService, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _spaDeployService = spaDeployService;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        [Route("commit")]
        public async Task<IActionResult> GetCommit()
        {
            var client = await _spaDeployService.GetFtpClient();
            await using var stream = new MemoryStream();
            await client.DownloadAsync(stream, Path.Join(SpaDeployService.RemoteTarget, SpaDeployService.DeploymentFile));
            stream.Position = 0;
            var reader = new StreamReader(stream);
            string text = await reader.ReadToEndAsync();
            var commit = JsonSerializer.Deserialize<Commit>(text);
            return Ok(commit);
        }

        [HttpPost]
        [Route("spa")]
        public async Task<IActionResult> DeploySpa()
        {
            return Ok(await _spaDeployService.Deploy());
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
        [Route("automatic-deploy-enabled")]
        public IActionResult IsAutoDeploy()
        {
            var spaDeployTask = _serviceProvider.GetService<SpaDeployTask>();
            return Ok(spaDeployTask!.IsEnabled);
        }
    }
}