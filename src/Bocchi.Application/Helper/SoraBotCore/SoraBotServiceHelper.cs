using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bocchi.SoraBotCore;

public static class SoraBotServiceHelper
{

    public static IList<MethodInfo> GetAllPluginMethod<TAttribute>(this IPlugin plugin)
        where TAttribute : Attribute, IOnSoraMessageAttribute
        => plugin.GetType().GetMethods()
            .Where(methodInfo => methodInfo.HasAttribute<TAttribute>())
            // 根据特征里的优先级进行排序
            .OrderBy(info =>
            {
                var attribute =
                    (TAttribute)info.GetCustomAttributes(typeof(TAttribute), true).Single();
                return attribute.Priority;
            }).ToList();
}