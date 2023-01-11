using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace Bocchi.SoraBotCore;

public class PluginParamService : IPluginParamService, ISingletonDependency
{
    private readonly Dictionary<Guid, Dictionary<string, object>> _paramList;

    private readonly ILogger<PluginParamService> _logger;

    public PluginParamService(ILogger<PluginParamService> logger)
    {
        _paramList = new();
        _logger = logger;
    }

    public Guid InitParamList(Guid id, Dictionary<string, object> paramList)
    {
        if (!_paramList.ContainsKey(id))
        {
            _paramList[id] = new Dictionary<string, object>();
            _paramList[id] = paramList;
            _paramList[id]["sessionId"] = id;
            return id;
        }
        else
        {
            foreach (var (pName, pValue) in paramList)
            {
                _paramList[id][pName] = pValue;
            }

            return id;
        }
    }

    public bool ContainsKey(Guid id, string pName)
    {
        if (!_paramList.ContainsKey(id))
        {
            return false;
        }

        return _paramList[id].ContainsKey(pName);
    }

    public object GetValue(Guid id, string pName)
    {
        if (!_paramList.ContainsKey(id) || !_paramList[id].ContainsKey(pName))
        {
            return null;
        }

        return _paramList[id][pName];
    }

    public Dictionary<string, object> GetAllValue(Guid id)
    {
        if (!_paramList.ContainsKey(id))
        {
            return new Dictionary<string, object>();
        }

        return _paramList[id];
    }

    public IList<KeyValuePair<string, object>> ContainsValueType(Guid id, Type pType)
    {
        if (!_paramList.ContainsKey(id))
        {
            return null;
        }

        return _paramList[id].Where(p => p.Value.GetType() == pType).ToList();
    }

    public void SetValue(Guid id, string pName, object pValue)
    {
        if (!_paramList.ContainsKey(id))
        {
            _paramList[id] = new Dictionary<string, object>();
        }

        _paramList[id][pName] = pValue;
    }

    public void CleanList(Guid id)
    {
        if (!_paramList.ContainsKey(id))
        {
            return;
        }


        _paramList.Remove(id);
    }
}