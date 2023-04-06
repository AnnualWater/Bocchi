using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Volo.Abp;

namespace Bocchi.OpenAi;

public class ChatWebService : BocchiAppService, IChatWebService
{
    private readonly RestClient _client;
    private readonly string _module;

    public ChatWebService(IConfiguration configuration)
    {
        // 检查API-Key
        var apiKey = configuration.GetValue("OpenAi:ApiKey", string.Empty);
        if (apiKey == string.Empty)
        {
            throw new BusinessException("Bocchi:OpenAI-apiKey", "未配置apiKey");
        }

        // 检查Module配置
        _module = configuration.GetValue("OpenAi:Module", string.Empty);
        if (_module == string.Empty)
        {
            throw new BusinessException("Bocchi:OpenAI-Module", "未配置Module");
        }

        // 检查代理
        var options = new RestClientOptions("https://api.openai.com/v1/");
        var proxyUrl = configuration.GetValue("OpenAi:Proxy", string.Empty);
        if (proxyUrl != string.Empty)
        {
            options.Proxy = new WebProxy(proxyUrl);
        }

        // 创建client
        _client = new RestClient(options);
        _client.AddDefaultHeaders(new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Authorization", $"Bearer {apiKey}" }
        });
    }

    public async Task<string> Chat(List<ChatMessageDto> messages)
    {
        var body = new ChatRequestBody
        {
            Model = _module,
            Messages = messages.Select(i => ObjectMapper.Map<ChatMessageDto, ChatMessage>(i)).ToList()
        };
        var request = new RestRequest("/chat/completions", Method.Post);
        request.AddStringBody(JsonSerializer.Serialize(body), ContentType.Json);
        var response = await _client.ExecuteAsync(request);
        if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
        {
            var jObject = JsonSerializer.Deserialize<ChatResponse>(response.Content);
            var responseMessage = jObject.Choices[0].Message.Content;
            return responseMessage;
        }
        else
        {
            throw new UserFriendlyException($"调用API出错：{response.ErrorMessage}");
        }
    }
}