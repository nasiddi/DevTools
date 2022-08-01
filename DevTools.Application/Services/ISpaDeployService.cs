using System.Threading.Tasks;
using DevTools.Application.Models;
using FluentFTP;

namespace DevTools.Application.Services;

public interface ISpaDeployService
{
    Task<Commit?> Deploy();
    Task<Commit> LoadCommitFromServer();
}