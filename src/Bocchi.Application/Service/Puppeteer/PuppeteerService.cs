using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.Puppeteer;

public class PuppeteerService : IPuppeteerService, ISingletonDependency, IDisposable
{
    private readonly LaunchOptions _launchOptions = new LaunchOptions { Headless = false };
    private readonly List<IBrowser> _browserList;
    private readonly ILogger<PuppeteerService> _logger;

    public PuppeteerService(ILogger<PuppeteerService> logger)
    {
        _browserList = new List<IBrowser>();
        _logger = logger;
    }

    public async Task<IBrowser> GetBrowser()
    {
        IBrowser browser = null;
        try
        {
            browser = await PuppeteerSharp.Puppeteer.LaunchAsync(_launchOptions);
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation("Chrome驱动不存在！开始下载。。。。。。");
            var browserFetcher = new BrowserFetcher(Product.Chrome);
            await browserFetcher.DownloadAsync();
            browser ??= await PuppeteerSharp.Puppeteer.LaunchAsync(_launchOptions);
        }

        _browserList.Add(browser);
        return browser;
    }

    public void RemoveBrowser(IBrowser browser)
    {
        _browserList.Remove(browser);
        browser.CloseAsync().GetAwaiter().GetResult();
        browser.Dispose();
    }

    public void Dispose()
    {
        foreach (var browser in _browserList)
        {
            browser.CloseAsync().GetAwaiter().GetResult();
            browser.Dispose();
        }
    }
}