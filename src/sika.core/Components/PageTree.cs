// Copyright (C)  2024  Yeong-won Seo
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

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using sika.core.Components.Abstract;
using sika.core.Model;

namespace sika.core.Components;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public class PageTree
{
    public PageTree()
    {
        Content = new PageTreeData("");
        Parent  = null;
    }

    private PageTree(PageTreeData data, PageTree? parent)
    {
        Content = data;
        Parent  = parent;
    }

    public PageTreeData   Content  { get; }
    public PageTree?      Parent   { get; }
    public List<PageTree> Children { get; } = [];

    private IEnumerable<PageTree> Traverse()
    {
        var queue = new Queue<PageTree>();

        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in current.Children)
                queue.Enqueue(child);
        }
    }

    internal string GetFullPath()
    {
        var stack = new Stack<string>();
        var tree  = this;
        do
        {
            stack.Push(tree.Content.Name);
            tree = tree.Parent;
        } while (tree?.Parent != null);

        return string.Join('/', stack);
    }

    internal PageTree GetRoot()
    {
        var current = this;
        while (current.Parent is not null)
            current = current.Parent;
        return current;
    }

    internal Dictionary<string, string> GetManglingMap(string root)
    {
        return Traverse()
            .Where(tree => tree.Content is PageLeafData)
            .ToDictionary(tree => tree.GetFullPath(), tree => root + tree.GetFullPath());
    }

    internal XElement GetHtmlInternal()
    {
        if (Content is PageLeafData leaf)
        {
            return new XElement("li",
                new XElement("a",
                    new XAttribute("href", $"/{GetFullPath()}.html"),
                    new XText(leaf.Metadata.Title)));
        }

        return new XElement("li",
            new XElement("span",
                new XAttribute("class", "caret"),
                new XText(Content.Name)),
            new XElement("ul",
                Children
                    .OrderBy(tree => tree.Content.Name)
                    .Select(child => child.GetHtmlInternal() as XObject)
                    .Prepend(new XAttribute("class", "nested"))));
    }

    public string GetHtml()
    {
        return GetHtmlInternal().ToString(SaveOptions.OmitDuplicateNamespaces);
    }

    public async Task<bool> PreprocessAsync(DirectoryInfo source, IPreprocessor preprocessor)
    {
        var directories = source.GetDirectories();
        var files       = source.GetFiles();

        var tasks = ArrayPool<Task>.Shared.Rent(directories.Length + files.Length);

        for (var i = 0; i < directories.Length; i++)
        {
            var node = new PageTree(new PageTreeData(directories[i].Name), this);
            tasks[i] = node.PreprocessAsync(directories[i], preprocessor).ContinueWith(ret =>
            {
                if (ret.Result)
                    Children.Add(node);
            });
        }

        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];

            Console.WriteLine($"  [{i + 1}/{files.Length}] Preprocess '{file.FullName}'");
            var task = preprocessor.PreprocessAsync(file);
            _ = task.ContinueWith(
                t => Children.Add(new PageTree(t.Result, this)),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            _ = task.ContinueWith(
                t => Console.WriteLine($"Preprocess failed: '{file.FullName}'\n{t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);
            tasks[directories.Length + i] = task;
        }

        // array-pool may return array bigger than requested
        await Task.WhenAll(tasks.Take(directories.Length + files.Length));

        ArrayPool<Task>.Shared.Return(tasks);

        return true;
    }

    public async Task LinkAsync(ILinker linker)
    {
        var array = Traverse().Where(tree => tree.Content is PageLeafData).ToArray();
        await Parallel.ForAsync(0, array.Length, async (i, _) =>
        {
            Console.WriteLine($"  [{i + 1}/{array.Length}] Link '{array[i].GetFullPath()}'");
            await linker.CompileAsync(array[i]);
        });
    }

    public async Task WriteAsync(IWriter writer)
    {
        foreach (var node in Traverse())
            await writer.WriteAsync(node);
    }
}

public record PageTreeData(string Name);

public record PageLeafData(
    string       Name,
    PostMetadata Metadata,
    string       Content)
    : PageTreeData(Name)
{
    public string Content { get; set; } = Content;
}