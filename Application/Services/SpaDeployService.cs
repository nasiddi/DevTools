using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Models;
using FluentFTP;
using FluentFTP.Rules;
using Microsoft.Extensions.Configuration;

namespace Application.ClientApp.Services
{
    public class SpaDeployService : ISpaDeployService
    {
        private readonly IConfiguration _configuration;
        public static readonly string ProjectName = "SPA";
        public static readonly string RemoteTarget = "/spa_root";
        public static readonly string DeploymentFile = "deployment.json";

        public SpaDeployService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<bool> Deploy()
        {
            var localProjectRoot = GetLocalProjectRoot();
            HasChanged();
            var commit = GetLastCommit(localProjectRoot);
            await PersistCommit(commit: commit, localProjectRoot: localProjectRoot);

            await UpdateServer(localProjectRoot: localProjectRoot);
            return true;
        }
        
        private static string GetLocalProjectRoot()
        {
            var root = "/Users/nadina/code";

            var projectFolder = Path.Join(root, ProjectName);
            return projectFolder;
        }
        
        public static bool HasChanged()
        {
            var localProjectRoot = GetLocalProjectRoot();
            return RunCommand($"cd {localProjectRoot} && git pull") != "Already up to date.\n";
        }
        
        private static Commit GetLastCommit(string localProjectRoot)
        {
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
            
            proc.Start();

            var stringBuilder = new StringBuilder();

            while (!proc.StandardOutput.EndOfStream) {
                stringBuilder.Append(proc.StandardOutput.ReadLine ());
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
        
        private static async Task PersistCommit(Commit commit, string localProjectRoot)
        {
            var json = JsonSerializer.Serialize(commit);
            await File.WriteAllTextAsync(Path.Join(localProjectRoot, DeploymentFile), json);
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

        public async Task<FtpClient> GetFtpClient()
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
    }
}