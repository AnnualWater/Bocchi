using System.Threading.Tasks;
using Hangfire.Dashboard;

namespace Bocchi.AuthorizeFilter;

public class CustomHangfireAuthorizeFilter : IDashboardAsyncAuthorizationFilter
{
    public Task<bool> AuthorizeAsync(DashboardContext context)
    {
        return Task.FromResult(context.GetHttpContext().Request.Host.Host is "127.0.0.1" or "localhost");
    }
}