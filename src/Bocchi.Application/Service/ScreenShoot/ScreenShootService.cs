using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.ScreenShoot;

public class ScreenShootService : IScreenShootService, ISingletonDependency, IDisposable
{
    private readonly LaunchOptions _launchOptions = new LaunchOptions { Headless = false };
    private IBrowser _browser;
    private readonly ILogger<ScreenShootService> _logger;

    public ScreenShootService(ILogger<ScreenShootService> logger)
    {
        _logger = logger;
    }


    private async Task<IPage> GetPage()
    {
        try
        {
            _browser ??= await Puppeteer.LaunchAsync(_launchOptions);
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation("Chrome驱动不存在！开始下载。。。。。。");
            var browserFetcher = new BrowserFetcher(Product.Chrome);
            await browserFetcher.DownloadAsync();
            _browser ??= await Puppeteer.LaunchAsync(_launchOptions);
        }

        return await _browser.NewPageAsync();
    }

    public async Task<string> ScreenShoot2B64(string url, string xPath)
    {
        var page = await GetPage();
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
        }
    }


    public void Dispose()
    {
        _browser?.CloseAsync().GetAwaiter().GetResult();
        _browser?.Dispose();
    }
}