﻿// Copyright (C)  2024  Yeong-won Seo
// 
// SIKA is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SIKA is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using sika.core.Components;
using sika.core.Model;
using sika.core.Text;

typeof(Program).Assembly.GetManifestResourceStream("sika.Resources.build-help.txt");
typeof(Program).Assembly.GetManifestResourceStream("sika.Resources.new-help.txt");

if (args.Length < 1)
{
    await PrintHelp("sika.Resources.help.txt");
    return -1;
}

switch (args[0])
{
    case "new":
        return await CreateProject(args[1..]);
    case "build":
        return await BuildProject(args[1..]);
    case "help":
        await PrintHelp("sika.Resources.help.txt");
        return 0;
    default:
        await PrintHelp("sika.Resources.help.txt");
        return -1;
}

async Task<int> CreateProject(string[] args)
{
    if (args.Length < 1 || args[0] == "--help")
    {
        await PrintHelp("sika.Resources.new-help.txt");
        return -1;
    }

    var project = new Project(
        new ProjectInfo("https://example.com", "post", "layout", "site"),
        new ProfileInfo("<username>", "<profile-image>"),
        new Contacts("<github-link>", "<your-email>", []));
    var data = new Json().Serialize(project);

    var file      = new FileInfo("sika.json");
    var siteDir   = new DirectoryInfo("site");
    var layoutDir = new DirectoryInfo("layout");
    var postDir   = new DirectoryInfo("post");
    if (file.Exists)
    {
        Console.Error.WriteLine("Couldn't create project: There is already project file (sika.json)");
        return -1;
    }

    if (siteDir.Exists)
    {
        Console.Error.WriteLine("Couldn't create folder: There is already site folder (site/)");
        return -1;
    }

    if (layoutDir.Exists)
    {
        Console.Error.WriteLine("Couldn't create folder: There is already layout folder (layout/)");
        return -1;
    }

    if (postDir.Exists)
    {
        Console.Error.WriteLine("Couldn't create folder: There is already post folder (post/)");
        return -1;
    }

    await using var fs = file.OpenWrite();
    await using var sw = new StreamWriter(fs, Encoding.UTF8);
    sw.Write(data);

    siteDir.Create();


    layoutDir.Create();
    await File.WriteAllTextAsync("layout/default.cshtml", await ReadResourceAsync("sika.Resources.default.cshtml"));

    postDir.Create();
    await File.WriteAllTextAsync("post/index.md", await ReadResourceAsync("sika.Resources.welcome.txt"));

    return 0;
}

async Task<int> BuildProject(string[] args)
{
    if (args.Length > 0 && args[0] == "--help")
    {
        await PrintHelp("sika.Resources.build-help.txt");
        return -1;
    }

    var file = new FileInfo("./sika.json");
    if (!file.Exists)
    {
        Console.Error.WriteLine("Couldn't find project file (sika.json)");
        return -1;
    }

    var project = new Json().Deserialize<Project>(File.ReadAllText(file.FullName));
    if (project is null)
    {
        Console.Error.WriteLine("Couldn't load project file (sika.json). Is project file in correct format?");
        return -1;
    }

    project.InitializeDirectories(Path.GetDirectoryName(file.FullName)!);

    var tree = new PageTree();
    Console.WriteLine("[1/4] Preprocess");
    await tree.PreprocessAsync(new DirectoryInfo(project.Info.PostDirectory), new YamlPreprocessor());
    Console.WriteLine("[2/4] Link : RazorLinker");
    await tree.LinkAsync(new RazorLinker(tree, project));
    Console.WriteLine("[3/4] Link : GoogleSitemapLinker");
    await tree.LinkAsync(new GoogleSitemapLinker(project));
    Console.WriteLine("[4/4] Write");
    await tree.WriteAsync(new FileSystemWriter(project));

    return 0;
}

async Task PrintHelp(string name)
{
    Console.Error.WriteLine(await ReadResourceAsync(name));
}

async Task<string> ReadResourceAsync(string name)
{
    await using var res = typeof(Program).Assembly.GetManifestResourceStream(name);
    using var       sr  = new StreamReader(res!);
    return await sr.ReadToEndAsync();
}