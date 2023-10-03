using System.Diagnostics.Contracts;

namespace BlogMan.Models.Utilities;

public static class UriUtils
{
    private const string Usable = "ABCDEFGHIJKLMNOPQRSTUVUXYZabcdefghijklmnopqrstuvwxyz0123456789-_.~";
    
    [Pure]
    public static string Normalize(string uri, char replace = '_')
    {
        return new string(uri.Normalize().Select(ch => Usable.Contains(ch) ? ch : replace).ToArray());
    }
}