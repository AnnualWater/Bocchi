using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bocchi.HtmlAgilityPack;
using Bocchi.SoraBotPlugin.ComicSubscription;
using HtmlAgilityPack;
using RestSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.Blazor.Pages.Sora.ComicSubscription;

public class SearchService : ITransientDependency
{
    readonly RestClient _client = new("https://www.yhdmp.net/");
    private readonly SoraBotPlugin.ComicSubscription.SearchService _searchService;

    public SearchService(SoraBotPlugin.ComicSubscription.SearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<List<SearchComicItem>> SearchComic(string search)
    {
        var request = new RestRequest("/s_all");
        request.AddParameter("ex", "1");
        request.AddParameter("kw", search);
        var response = await _client.ExecuteAsync(request);
        if (!response.IsSuccessful || response.Content == null)
        {
            return new List<SearchComicItem>();
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(response.Content);
        var searchNodes = doc.DocumentNode.SelectNodes("//div[@class='lpic']/ul/li");
        if (searchNodes == null || searchNodes.Count == 0)
        {
            return new List<SearchComicItem>();
        }

        var resp = new List<SearchComicItem>();
        foreach (var searchNode in searchNodes)
        {
            var url = searchNode.SelectSingleNode("./h2/a").GetAttributeValue("href", string.Empty);
            var id = Regex.Match(url, "[0-9]+").Value;
            resp.Add(new SearchComicItem
            {
                ComicId = id,
                Title = searchNode.SelectSingleNode("./h2/a/@title").GetAttributeValue("title", string.Empty),
                ImgUrl = searchNode.SelectSingleNode("./a/img").GetAttributeValue("src", string.Empty),
                UpdateInfo = searchNode.SelectSingleNode("./span[1]/font/text()").TryGetText()
            });
        }

        return resp;
    }

    public async Task<SearchComicInfo> SearchComicById(string comicId)
        => await _searchService.SearchComicById(comicId);
}