using System.Threading.Tasks;
using Bocchi.HtmlAgilityPack;
using HtmlAgilityPack;
using RestSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.ComicSubscription;

public class SearchService : ITransientDependency
{
    public const string Url = "https://www.yhpdm.com/";
    private readonly RestClient _client = new(Url);

    public async Task<SearchComicInfo> SearchComicById(string comicId)
    {
        var request = new RestRequest($"/showp/{comicId}.html");
        var response = await _client.ExecuteAsync(request);
        if (!response.IsSuccessful || response.Content == null)
        {
            return null;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(response.Content);

        var route = doc.DocumentNode
            .SelectSingleNode("//div[@class='tabs']/div/div[last()]/ul/li/a/@href")
            .GetAttributeValue("href", string.Empty);
        
        return new SearchComicInfo
        {
            Title = doc.DocumentNode.SelectSingleNode("//div[@class='rate r']/h1/text()").TryGetText(),
            NowEpisode = doc.DocumentNode.SelectSingleNode("//div[@class='sinfo']/p[2]/text()").TryGetText(),
            UpdateInfo = doc.DocumentNode.SelectSingleNode("//div[@class='sinfo']/p[2]/font/text()").TryGetText(),
            LastUrl = string.IsNullOrEmpty(route) ? $"{Url}/showp/{{comicId}}.html" : $"{Url}/{route}"
        };
    }
}