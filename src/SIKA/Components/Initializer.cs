//    Copyright (C) 2023  Yeong-won Seo
// 
//     SIKA is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     SIKA is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using System.CommandLine;
using System.Reflection;
using System.Text.Json;
using SIKA.Models;
using SourceGenerationContext = SIKA.Contexts.SourceGenerationContext;

namespace SIKA.Components;

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
        var rootUri = new Option<string>("--root", "Specify the root uri of site to generate")
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
        init.AddOption(rootUri);
        init.SetHandler(Init, name, rootUri);

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
        if (!SEH.IO<FileInfo, Project>(project, static project =>
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
            }, out var proj))
            return null;

        proj.Info.BuildDirectory  = Path.Combine(project.DirectoryName!, proj.Info.BuildDirectory);
        proj.Info.LayoutDirectory = Path.Combine(project.DirectoryName!, proj.Info.LayoutDirectory);
        proj.Info.PostDirectory   = Path.Combine(project.DirectoryName!, proj.Info.PostDirectory);
        proj.Info.SiteDirectory   = Path.Combine(project.DirectoryName!, proj.Info.SiteDirectory);

        return proj;
    }

    private static void Build(FileInfo project)
    {
        var data = ReadProject(project);
        if (data is null)
            return;

        Logger.Log(LogLevel.INFO, "Start preprocessing project");

        if (Preprocessor.Compile(data))
        {
            Logger.Log(LogLevel.CMPL, "Complete preprocessing project");
        }
        else
        {
            Logger.Log(LogLevel.FAIL, "Failed to preprocess project");
            return;
        }

        Logger.Log(LogLevel.INFO, "Start building project");


        Logger.Log(LogLevel.INFO, "start global initializing");

        if (!SEH.IO(data.Info.SiteDirectory, dir =>
            {
                var info = new DirectoryInfo(dir);
                if (info.Exists)
                    info.Delete(true);
                info.Create();
            }))
        {
            Logger.Log(LogLevel.FAIL, "failed global initializing");
            return;
        }

        Logger.Log(LogLevel.CMPL, "complete global initializing");

        using (var razor = new RazorTemplateLinker(data))
        {
            if (!razor.Run())
            {
                Logger.Log(LogLevel.FAIL, "Failed to build project");
                return;
            }
        }

        Logger.Log(LogLevel.CMPL, "Complete building project");
        
        using (var google = new GoogleSitemapLinker(data))
        {
            if (!google.Run())
            {
                Logger.Log(LogLevel.FAIL, "Failed to build sitemap");
                return;
            }
        }

        Logger.Log(LogLevel.CMPL, "Complete building sitemap");
    }

    private static void Init(string name, string root)
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
                        root,
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

            Logger.Log(LogLevel.CMPL, $"'{file.Name}' created.");
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

            Logger.Log(LogLevel.CMPL, $"'{file.Name}' created.");
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