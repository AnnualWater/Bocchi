using System;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace Bocchi.SoraBotPlugin.OrderMusic;

public class SearchCacheService : ISearchCacheService, ISingletonDependency
{
    private readonly Dictionary<Guid, List<MusicDataItem>> _cache;
    private readonly IGuidGenerator _guidGenerator;

    public SearchCacheService(IGuidGenerator guidGenerator)
    {
        _guidGenerator = guidGenerator;
        _cache = new Dictionary<Guid, List<MusicDataItem>>();
    }

    public Guid AddItem(List<MusicDataItem> data)
    {
        var id = _guidGenerator.Create();
        _cache.Add(id, data);
        return id;
    }

    public List<MusicDataItem> GetItem(Guid id)
    {
        if (!_cache.ContainsKey(id))
        {
            return new List<MusicDataItem>();
        }

        var data = _cache[id];
        _cache.Remove(id);
        return data;

    }
}

public interface ISearchCacheService
{
    public Guid AddItem(List<MusicDataItem> data);
    public List<MusicDataItem> GetItem(Guid id);
}