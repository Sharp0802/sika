using System.CommandLine;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using BlogMan.Models;
using YamlDotNet.Serialization;

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

        var compile = new Command("compile", "Compile the specific project");
        compile.AddOption(project);
        compile.SetHandler(Compile, project);

        var link = new Command("link", "Link the specific project");
        link.AddOption(project);
        link.SetHandler(Link, project);

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

        root.AddCommand(compile);
        root.AddCommand(link);
        root.AddCommand(@new);
        root.AddCommand(init);
        root.AddCommand(clean);
    }

    private static Project? ReadProject(FileInfo project)
    {
        return SEH.IO(project, static project =>
        {
            using var file = File.OpenRead(project.FullName);
            using var reader = XmlReader.Create(file);
            var serializer = new XmlSerializer(typeof(Project));
            if (!serializer.CanDeserialize(reader))
            {
                Logger.Log(LogLevel.FAIL, "Cannot deserialize project file", project.FullName);
                return null;
            }

            var data = (Project)serializer.Deserialize(reader)!;

            var errors = data.Validate().ToArray();
            if (errors.Length != 0)
            {
                errors.PrintErrors(project.FullName);
                return null;
            }

            return data;
        }, out var data) ? data : null;
    }

    private static void Compile(FileInfo project)
    {
        var data = ReadProject(project);
        if (data is null)
            return;
        if (Compiler.Compile(data))
            Logger.Log(LogLevel.CMPL, "Complete compiling project");
        else
            Logger.Log(LogLevel.FAIL, "Failed to compile project");
    }

    private static void Link(FileInfo project)
    {
        var data = ReadProject(project);
        if (data is null)
            return;
        if (Linker.Link(data))
            Logger.Log(LogLevel.CMPL, "Complete compiling project");
        else
            Logger.Log(LogLevel.FAIL, "Failed to compile project");
    }

    private static void Init(string name)
    {
        var file = new FileInfo($"{name}.blog.xml");
        SEH.IO(file, _ =>
        {
            if (file.Exists)
            {
                Logger.Log(LogLevel.FAIL, "File already exists", file.FullName);
                return;
            }
            
            using var writer = file.CreateText();
            
            new XmlSerializer(typeof(Project)).Serialize(writer, new Project(
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
                    Array.Empty<LinkReference>())));
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
            
            new Serializer().Serialize(writer, new PostFrontMatter(
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