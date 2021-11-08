using System.Threading.Tasks;
using FluentFTP;

namespace Application.ClientApp.Services
{
    public interface ISpaDeployService
    {
        Task<bool> Deploy();
        Task<FtpClient> GetFtpClient();
    }
}