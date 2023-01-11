using System;
using System.Collections.Generic;

namespace Bocchi.SoraBotCore;

public abstract class PluginWithSession : IPlugin
{

    /// <summary>
    /// 参数服务
    /// </summary>
    private readonly IPluginParamService _pluginParamService;

    /// <summary>
    /// 会话ID
    /// </summary>
    private Guid _sessionId = Guid.Empty;

    private uint _finish;

    public bool SetSessionId(Guid id)
    {
        if (_sessionId != Guid.Empty)
        {
            return false;
        }

        _finish = 0;
        _sessionId = id;
        return true;
    }

    public Guid GetSessionId()
        => _sessionId;

    public void FinishMethod()
    {
        _finish++;
    }

    public uint GetFinished() => _finish;


    protected PluginWithSession(IPluginParamService pluginParamService)
    {
        _pluginParamService = pluginParamService;
    }

    public abstract uint Priority { get; }

    protected void SetValue(string pName, object pValue)
        => _pluginParamService.SetValue(_sessionId, pName, pValue);

    protected object GetValue(string pName)
        => _pluginParamService.GetValue(_sessionId, pName);

    protected Dictionary<string, object> GetAllValue()
        => _pluginParamService.GetAllValue(_sessionId);

    protected TValue GetValue<TValue>(string pName)
        => (TValue)GetValue(pName);

    protected void NextMethodWaitNewEvent()
    {
        FinishMethod();
        throw new MethodWaitNewEventException();
    }

    protected void WaitNewEvent()
    {
        throw new MethodWaitNewEventException();
    }

    protected void FinishPlugin()
    {
        throw new PluginFinishException();
    }


}