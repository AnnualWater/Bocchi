using System.Threading.Tasks;
using Bocchi.Puppeteer;
using PuppeteerSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.ScreenShoot;

public class ScreenShootService : IScreenShootService, ITransientDependency
{
    private readonly IPuppeteerService _puppeteerService;

    public ScreenShootService(IPuppeteerService puppeteerService)
    {
        _puppeteerService = puppeteerService;
    }

    public async Task<string> ScreenShoot2B64(string url, string xPath)
    {
        var browser = await _puppeteerService.GetBrowser();
        var page = await browser.NewPageAsync();
        try
        {
            await page.GoToAsync(url, WaitUntilNavigation.Load);
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080,
                IsMobile = false,
                DeviceScaleFactor = 1,
                HasTouch = false
            });
            var element = await page.WaitForXPathAsync(xPath);
            return await element.ScreenshotBase64Async();
        }
        finally
        {
            await page.CloseAsync();
            _puppeteerService.RemoveBrowser(browser);
        }
    }
}