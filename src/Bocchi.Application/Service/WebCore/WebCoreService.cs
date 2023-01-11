using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace Bocchi.WebCore;

public class WebCoreService : IWebCoreService, ITransientDependency
{
    private readonly IConfiguration _configuration;

    public WebCoreService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GetWebUrl()
    {
        return Task.FromResult(_configuration.GetValue("Urls", "未配置Urls"));
    }
}