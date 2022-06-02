using System.Net.Http.Json;

namespace FsTestStack.Test.CSharp;

public static class FuncHelper
{
    public static T[] OfArray<T>(this T _)
    {
        return Array.Empty<T>();
    }

    public static async Task<T> ReadFromJsonAsync<T>(this HttpContent content, T? _ = default)
    {
        return (await HttpContentJsonExtensions.ReadFromJsonAsync<T>(content))!;
    }

    public static Func<T, T> ToIdFunc<T>(Action<T>? act)
    {
        return t =>
        {
            act?.Invoke(t);
            return t;
        };
    }
}