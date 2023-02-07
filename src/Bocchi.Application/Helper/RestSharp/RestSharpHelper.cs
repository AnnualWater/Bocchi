using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;

namespace Bocchi.RestSharp;

public static class RestSharpHelper
{
    public static TValue TryGetValue<TValue>(this RestResponse response) where TValue : class
    {
        if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<TValue>(response.Content);
        }

        return null;
    }

    public static async Task<TValue> TryGetValue<TValue>(this Task<RestResponse> responseTask) where TValue : class
    {
        var response = await responseTask;
        if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<TValue>(response.Content);
        }

        return null;
    }
}