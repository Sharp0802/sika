using System.CommandLine;
using BlogMan.Components;

var root = new RootCommand();
Initializer.InitializeRoot(root);
return await root.InvokeAsync(args);