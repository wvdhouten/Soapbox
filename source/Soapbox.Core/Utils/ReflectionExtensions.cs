namespace Soapbox.Application.Utils;

using System;
using System.Reflection;

public static class ReflectionExtensions
{
    public static object? TryInvokeMethod<T>(this T obj, string methodName, params object[] args)
    {
        var type = typeof(T);
        var method = type.GetTypeInfo().GetDeclaredMethod(methodName)
            ?? throw new InvalidOperationException($"Method '{methodName}' not found on type '{type.FullName}'.");

        return method.Invoke(obj, args);
    }

    public static object? TryInvokeMethod<T>(this T obj, Type type, string methodName, params object[] args)
    {
        var method = type.GetTypeInfo().GetDeclaredMethod(methodName)
            ?? throw new InvalidOperationException($"Method '{methodName}' not found on type '{type.FullName}'.");

        return method.Invoke(obj, args);
    }
}
