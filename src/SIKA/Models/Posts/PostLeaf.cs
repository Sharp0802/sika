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

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using SIKA.Components;
using SIKA.Models.Utilities;

namespace SIKA.Models.Posts;

public enum PostKind
{
    None,
    Welcome,
    Error
}

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class PostLeaf : PostTree
{
    public DirectoryInfo    Directory { get; }
    public FileInfo         Info      { get; }
    public PostFrontMatter? Metadata  { get; }
    public PostKind         Kind      { get; }

    public string Title => Metadata?.Title ?? Path.GetFileNameWithoutExtension(Info.Name);

    public PostLeaf(FileSystemInfo parent, FileSystemInfo current) : base(parent, current)
    {
        Directory = (DirectoryInfo)parent;
        Info      = (FileInfo)current;
        Metadata  = GetFrontMatter();

        Kind = current.Name.ToLowerInvariant() switch
        {
            "welcome.md" => PostKind.Welcome,
            "error.md"   => PostKind.Error,
            _            => PostKind.None
        };
    }

    private PostFrontMatter? GetFrontMatter()
    {
        var name = Path.GetFileNameWithoutExtension(Info.Name);

        var q =
            from fsi in Directory.EnumerateFiles()
            let ext = fsi.Extension
            let cmp = Path.GetFileNameWithoutExtension(fsi.Name)
            where ext.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
                  ext.Equals(".yml",  StringComparison.OrdinalIgnoreCase)
            where name.Equals(cmp, StringComparison.Ordinal)
            select fsi;

        var file = q.FirstOrDefault();
        if (file is null)
        {
            Logger.Log(LogLevel.WARN, "yaml metadata not found; linkage will be failed", Info.FullName);
            return null;
        }

        using var stream = file.OpenText();
        return Yaml.Deserialize<PostFrontMatter>(stream);
    }

    public string GetIdentifier()
    {
        if (Current.Name.Equals("index.md",   StringComparison.OrdinalIgnoreCase) ||
            Current.Name.Equals("welcome.md", StringComparison.OrdinalIgnoreCase))
            return "index.html";
        return UriUtils.Normalize(Path.ChangeExtension(Current.Name, "html"));
    }

    public string GetHRef()
    {
        return $"/{GetIdentifier()}";
    }

    public override XElement? GetHtml()
    {
        if (Parent is null && Kind == PostKind.Error)
            return null;

        return new XElement("li",
            new XElement("a",
                new XAttribute("href", GetHRef()),
                new XText(Title)
            )
        );
    }
}