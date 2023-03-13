using System.Threading.Tasks;
using PuppeteerSharp;

namespace Bocchi.Puppeteer;

public interface IPuppeteerService
{
    public Task<IBrowser> GetBrowser();
    public void RemoveBrowser(IBrowser browser);
}