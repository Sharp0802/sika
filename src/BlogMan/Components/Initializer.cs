using System.CommandLine;
using System.Globalization;
using System.Reflection;
using System.Text;
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
        var type = new Option<string>("--type", "Specify the type of resource to create")
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

        var @new = new Command("new", "Create a new resource");
        @new.AddOption(type);
        @new.AddOption(name);
        @new.SetHandler(New, type, name);

        root.AddCommand(compile);
        root.AddCommand(link);
        root.AddCommand(@new);
    }

    private static void Compile(FileInfo project)
    {
        using var file = File.OpenRead(project.FullName);
        using var reader = XmlReader.Create(file);
        var serializer = new XmlSerializer(typeof(Project));
        if (!serializer.CanDeserialize(reader))
        {
            Logger.Log(LogLevel.FAIL, "Cannot deserialize project file", project.FullName);
            return;
        }
        var data = (Project)serializer.Deserialize(reader)!;

        var errors = data.Validate().ToArray();
        if (errors.Any())
        {
            errors.PrintErrors(project.FullName);
            return;
        }

        if (Compiler.Compile(data))
            Logger.Log(LogLevel.CMPL, "Complete compiling project");
        else
            Logger.Log(LogLevel.FAIL, "Failed to compile project");
    }

    private static void Link(FileInfo project)
    {
        using var file = File.OpenRead(project.FullName);
        using var reader = XmlReader.Create(file);
        var serializer = new XmlSerializer(typeof(Project));
        if (!serializer.CanDeserialize(reader))
        {
            Logger.Log(LogLevel.FAIL, "Cannot deserialize project file", project.FullName);
            return;
        }
        var data = (Project)serializer.Deserialize(reader)!;

        var errors = data.Validate().ToArray();
        if (errors.Any())
        {
            errors.PrintErrors(project.FullName);
            return;
        }

        if (Linker.Link(data))
            Logger.Log(LogLevel.CMPL, "Complete compiling project");
        else
            Logger.Log(LogLevel.FAIL, "Failed to compile project");
    }

    private static void New(string type, string name)
    {
        try
        {
            switch (type)
            {
                case "project":
                    using (var file = File.OpenWrite($"{name}.xml"))
                    {
                        new XmlSerializer(typeof(Project)).Serialize(file, new Project(
                                new ProjectInfo(
                                    name,
                                    Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0),
                                    "post/",
                                    "layout/",
                                    "site/",
                                    "build/"
                                ),
                                new ProfileInfo(
                                    "<your-name>",
                                    "<profile-image>"
                                ),
                                new Contacts(
                                    "<your-github-accout-here>",
                                    "<your-email-here>",
                                    Array.Empty<LinkReference>()
                                )
                            )
                        );
                    }

                    break;
                case "post":
                    using (var file = File.OpenWrite($"{DateTime.Now:yyyyMMdd}_{name}.md"))
                    using (var writer = new StreamWriter(file, Encoding.UTF8))
                    {
                        new Serializer().Serialize(
                            writer,
                            new PostFrontMatter(
                                CultureInfo.CurrentCulture.Name,
                                "default",
                                name,
                                new[] { DateTime.Now },
                                Array.Empty<string>()));
                    }

                    break;
                case "layout":
                    File.Create($"{name}.razor");
                    break;
                default:
                    throw new InvalidOperationException($"Invalid resource type: '{type}'");
            }
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.CRIT, e);
        }
    }
}