using System.CommandLine;
using SIKA.Components;

namespace SIKA;

public static class Program
{
    public static async Task<int> Main(params string[] args)
    {
        var root = new RootCommand();
        Initializer.InitializeRoot(root);
        return await root.InvokeAsync(args);
    }
}
