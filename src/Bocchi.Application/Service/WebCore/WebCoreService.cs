using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bocchi.RestSharp;
using Bocchi.WebCore.Cpolar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.WebCore;

public class WebCoreService : IWebCoreService, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebCoreService> _logger;

    private readonly string _tool;
    private string _url;

    public WebCoreService(IConfiguration configuration, ILogger<WebCoreService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _tool = _configuration.GetValue("WebCore:Tool", string.Empty);
    }

    private async Task ResetUrl()
    {
        try
        {
            switch (_tool)
            {
                case "Cpolar":
                    var url = _configuration.GetValue("WebCore:Cpolar:Url", string.Empty);
                    if (string.IsNullOrEmpty(url))
                    {
                        break;
                    }

                    var client = new RestClient(url);
                    var email = _configuration.GetValue("WebCore:Cpolar:Email", string.Empty);
                    var password = _configuration.GetValue("WebCore:Cpolar:Password", string.Empty);
                    var loginRequest = new RestRequest("/api/v1/user/login");
                    loginRequest.AddBody(JsonSerializer.Serialize(new { email, password }));
                    var loginResponse = await client.PostAsync(loginRequest).TryGetValue<CpolarLoginResponse>();
                    if (loginResponse is { Code: 20000 })
                    {
                        var infoRequest = new RestRequest("/api/v1/tunnels");
                        infoRequest.AddHeader("Authorization", $"Bearer {loginResponse.Data.Token}");
                        var infoResponse = await client.GetAsync(infoRequest).TryGetValue<CpolarInfoResponse>();
                        if (infoResponse is { Code: 20000 })
                        {
                            var info = infoResponse.CpolarInfoResponseData.Items.FirstOrDefault(item =>
                                item.Name == "Bocchi");
                            if (info != null)
                            {
                                _url = info.PublicUrl;
                                return;
                            }
                        }
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,"加载自定义URL配置出错！");
        }
        
        _url = _configuration.GetValue("Urls", "未配置Urls");
    }

    public async Task<string> GetWebUrl()
    {
        await TestUrl();
        return _url;
    }

    private async Task TestUrl()
    {
        if (string.IsNullOrEmpty(_url))
        {
            await ResetUrl();
            return;
        }

        var restClient = new RestClient(_url);
        var request = new RestRequest("/api/bocchi/status");
        var response = await restClient.GetAsync(request);
        if (response.IsSuccessful && response.Content == "success")
        {
            return;
        }

        await ResetUrl();
    }
}