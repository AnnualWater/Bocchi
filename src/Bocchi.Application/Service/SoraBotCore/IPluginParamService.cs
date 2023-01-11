using System;
using System.Collections.Generic;

namespace Bocchi.SoraBotCore;

public interface IPluginParamService
{
    /// <summary>
    /// 初始化参数表
    /// </summary>
    /// <param name="id">参数表ID</param>
    /// <param name="paramList">初始参数表</param>
    /// <returns></returns>
    public Guid InitParamList(Guid id, Dictionary<string, object> paramList);

    /// <summary>
    /// 判断参数表内是否存在参数（名称匹配）
    /// </summary>
    /// <param name="id">参数表ID</param>
    /// <param name="pName">参数名</param>
    /// <returns></returns>
    public bool ContainsKey(Guid id, string pName);

    /// <summary>
    /// 通过名字获取参数
    /// </summary>
    /// <param name="id">参数表ID</param>
    /// <param name="pName">参数名</param>
    /// <returns></returns>
    public object GetValue(Guid id, string pName);

    /// <summary>
    /// 通过名字获取参数
    /// </summary>
    /// <param name="id">参数表ID</param>
    /// <returns></returns>
    public Dictionary<string, object> GetAllValue(Guid id);

    /// <summary>
    /// 通过类型获取匹配的参数
    /// </summary>
    /// <param name="id">参数表ID</param>
    /// <param name="pType">参数类型</param>
    /// <returns></returns>
    public IList<KeyValuePair<string, object>> ContainsValueType(Guid id, Type pType);

    /// <summary>
    /// 设置参数
    /// </summary>
    /// <param name="id">参数表ID</param>
    /// <param name="pName">参数名</param>
    /// <param name="pValue">参数值</param>
    public void SetValue(Guid id, string pName, object pValue);

    /// <summary>
    /// 清空参数表
    /// </summary>
    /// <param name="id">参数表ID</param>
    public void CleanList(Guid id);
}