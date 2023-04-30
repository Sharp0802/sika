using System.CommandLine;
using System.Reflection;
using System.Text.Json;
using BlogMan.Contexts;
using BlogMan.Models;

namespace BlogMan.Components;

public static class Initializer
{
    public static void InitializeRoot(RootCommand root)
    {
        var project = new Option<FileInfo>("--project", "Specify project that target of command")
        {
            IsRequired = true
        };
        var name = new Option<string>("--name", "Specify the name of resource to create")
        {
            IsRequired = true
        };

        var build = new Command("build", "Build the specific project");
        build.AddOption(project);
        build.SetHandler(Build, project);

        var @new = new Command("new", "Create a new post");
        @new.AddOption(project);
        @new.AddOption(name);
        @new.SetHandler(New, project, name);

        var init = new Command("init", "Create a new project");
        init.AddOption(name);
        init.SetHandler(Init, name);

        var clean = new Command("clean", "Clean the build directories of specific project");
        clean.AddOption(project);
        clean.SetHandler(Clean, project);

        root.AddCommand(build);
        root.AddCommand(@new);
        root.AddCommand(init);
        root.AddCommand(clean);
    }

    private static Project? ReadProject(FileInfo project)
    {
        return SEH.IO(project, static project =>
        {
            using var file = File.OpenRead(project.FullName);

            var data = JsonSerializer.Deserialize(file, SourceGenerationContext.Default.Project);
            if (data is null)
            {
                Logger.Log(LogLevel.FAIL, "Cannot deserialize project file", project.FullName);
                return null;
            }

            var errors = data.Validate().ToArray();
            if (errors.Length != 0)
            {
                errors.PrintErrors(project.FullName);
                return null;
            }

            return data;
        }, out var data)
            ? data
            : null;
    }

    private static void Build(FileInfo project)
    {
        var data = ReadProject(project);
        if (data is null)
            return;

        if (Preprocessor.Compile(data))
        {
            Logger.Log(LogLevel.CMPL, "Complete preprocessing project");
        }
        else
        {
            Logger.Log(LogLevel.FAIL, "Failed to preprocess project");
            return;
        }

        if (Linker.Link(data))
            Logger.Log(LogLevel.CMPL, "Complete compiling project");
        else
            Logger.Log(LogLevel.FAIL, "Failed to compile project");
    }

    private static void Init(string name)
    {
        var file = new FileInfo($"{name}.blog.json");
        SEH.IO(file, _ =>
        {
            if (file.Exists)
            {
                Logger.Log(LogLevel.FAIL, "File already exists", file.FullName);
                return;
            }

            using var writer = file.Create();

            JsonSerializer.Serialize(
                writer,
                new Project(
                    new ProjectInfo(
                        name,
                        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0),
                        "post/",
                        "layout/",
                        "site/",
                        "build/"),
                    new ProfileInfo(
                        "your-name",
                        "thumbnail-here"),
                    new Contacts(
                        "your-github-link",
                        "your-email",
                        Array.Empty<LinkReference>())),
                SourceGenerationContext.Default.Project);
        });
    }

    private static void New(FileInfo project, string name)
    {
        var data = ReadProject(project);
        if (data is null)
            return;
        var file = new FileInfo($"{Path.Combine(data.Info.PostDirectory, name)}.md");
        SEH.IO(file, _ =>
        {
            if (!Directory.Exists(data.Info.PostDirectory))
                Directory.CreateDirectory(data.Info.PostDirectory);

            if (file.Exists)
            {
                Logger.Log(LogLevel.FAIL, "File already exists", file.FullName);
                return;
            }

            using var writer = file.CreateText();

            writer.Write("---\n");

            Yaml.Serialize(
                writer,
                new PostFrontMatter(
                    "en-us",
                    "default",
                    name,
                    new[] { DateTime.Now },
                    Array.Empty<string>()));

            writer.Write($"---\n\n# {name}\n");
        });
    }

    private static void Clean(FileInfo project)
    {
        var data = ReadProject(project);
        if (data is null)
            return;
        if (Directory.Exists(data.Info.BuildDirectory))
            Directory.Delete(data.Info.BuildDirectory, true);
        if (Directory.Exists(data.Info.SiteDirectory))
            Directory.Delete(data.Info.SiteDirectory, true);
    }
}