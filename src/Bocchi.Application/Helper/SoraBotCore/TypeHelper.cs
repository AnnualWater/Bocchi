using System;
using System.Linq;
using System.Reflection;

namespace Bocchi.SoraBotCore;

public static class TypeHelper
{
    public static bool HasAttribute(this MethodInfo methodInfo, Type attributeType)
        => methodInfo
            .CustomAttributes
            .Where(methodData => methodData.AttributeType == attributeType)
            .ToList().Count != 0;

    public static bool HasAttribute<TAttribute>(this MethodInfo methodInfo) where TAttribute : Attribute
        => methodInfo.HasAttribute(typeof(TAttribute));
}