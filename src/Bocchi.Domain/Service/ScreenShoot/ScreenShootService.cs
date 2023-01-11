using System.Threading.Tasks;

namespace Bocchi.ScreenShoot;

public interface IScreenShootService
{
    public Task<string> ScreenShoot2B64(string url, string xPath);
}