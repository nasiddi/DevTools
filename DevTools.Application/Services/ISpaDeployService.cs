using System.Threading.Tasks;
using DevTools.Application.Models;

namespace DevTools.Application.Services;

public interface ISpaDeployService
{
    Task<Commit?> Deploy();
    Task<Commit> LoadCommitFromServer();
}