using System.Threading.Tasks;
using FluentFTP;

namespace DevTools.Application.Services
{
    public interface ISpaDeployService
    {
        Task<bool> Deploy();
        Task<FtpClient> GetFtpClient();
    }
}