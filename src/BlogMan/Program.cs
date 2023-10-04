using System.CommandLine;
using BlogMan.Components;

namespace BlogMan;

public static class Program
{
    public static async Task<int> Main(params string[] args)
    {
        var root = new RootCommand();
        Initializer.InitializeRoot(root);
        return await root.InvokeAsync(args);
    }
}
