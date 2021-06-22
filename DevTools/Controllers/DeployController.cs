using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DevTools.Models;
using FluentFTP;
using FluentFTP.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DevTools.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class DeployController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static readonly string ProjectName = "SPA";
        private static readonly string RemoteTarget = "/spa_root";
        private static readonly string DeploymentFile = "deployment.json";

        public DeployController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("commit")]
        public async Task<IActionResult> GetCommit()
        {
            var client = await GetFtpClient();
            await using var stream = new MemoryStream();
            await client.DownloadAsync(stream, Path.Join(RemoteTarget, DeploymentFile));
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
                var localProjectRoot = GetLocalProjectRoot();

                var commit = GitPull(localProjectRoot);

                await PersistCommit(commit: commit, localProjectRoot: localProjectRoot);

                await UpdateServer(localProjectRoot: localProjectRoot);
            
            return Ok();
        }

        private async Task UpdateServer(string localProjectRoot)
        {
            var client = await GetFtpClient();
            var ftpFileNameRegexRule = new FtpFileNameRegexRule(false, new List<string> {"^[.]"});
            var ftpHiddenFolderNameRegexRule = new FtpFolderRegexRule(false, new List<string> {"^[.]"});
            var ftpDevFolderNameRegexRule = new FtpFolderNameRule(false, new List<string> {"development"});
            var result = await client.UploadDirectoryAsync(
                localFolder: localProjectRoot,
                remoteFolder: RemoteTarget,
                mode: FtpFolderSyncMode.Mirror,
                existsMode: FtpRemoteExists.Overwrite,
                rules: new List<FtpRule> {ftpFileNameRegexRule, ftpHiddenFolderNameRegexRule, ftpDevFolderNameRegexRule});

            await client.DisconnectAsync();
        }

        private async Task<FtpClient> GetFtpClient()
        {
            var authorization = _configuration.GetSection("FtpServerCredentials");

            var client = new FtpClient("gvc.ch")
            {
                EncryptionMode = FtpEncryptionMode.Explicit,
                Credentials = new NetworkCredential(authorization["Username"], authorization["Password"]),
                ValidateAnyCertificate = true,
            };

            await client.ConnectAsync();
            return client;
        }

        private static async Task PersistCommit(Commit commit, string localProjectRoot)
        {
            var json = JsonSerializer.Serialize(commit);
            await System.IO.File.WriteAllTextAsync(Path.Join(localProjectRoot, DeploymentFile), json);
        }

        private static string GetLocalProjectRoot()
        {
            var root = Directory.GetCurrentDirectory();
            var currentDirectory = "";
            while (currentDirectory != "code")
            {
                root = Directory.GetParent(root)!.ToString();
                var folders = root.Split("/");
                currentDirectory = folders[^1];
            }

            var projectFolder = Path.Join(root, ProjectName);
            return projectFolder;
        }

        private static Commit GitPull(string localProjectRoot)
        {
            RunCommand($"cd {localProjectRoot} && git pull");
            var commitString = RunCommand($"cd {localProjectRoot} && git log --pretty=format:\"%h%x09%ad%x09%s\" --date=iso -1");
            var commitSplit = commitString.Split('\t');
            var commit = new Commit(commitSplit[0], commitSplit[2], DateTime.Parse(commitSplit[1]));
            return commit;
        }

        private static string RunCommand(string command)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \" " + command + " \"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            
            proc.Start ();

            var stringBuilder = new StringBuilder();

            while (!proc.StandardOutput.EndOfStream) {
                stringBuilder.Append(proc.StandardOutput.ReadLine ());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
    }
}