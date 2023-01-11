using System.Threading.Tasks;

namespace Bocchi.WebCore;

public interface IWebCoreService
{
    public Task<string> GetWebUrl();
}