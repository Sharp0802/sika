﻿using System.Xml.Linq;

namespace SIKA.Models.Posts;

public class PostBranch : PostTree
{
    public new DirectoryInfo Current { get; }

    public PostBranch(FileSystemInfo parent, FileSystemInfo current) : base(parent, current)
    {
        Current = (DirectoryInfo)current;
    }

    public override XElement? GetHtml()
    {
        if (Children is null)
            return null;

        return new XElement("li",
            new XElement("span",
                new XAttribute("class", "caret"),
                new XText(Current.Name)
            ),
            new XElement("ul",
                Children
                   .Order(PostTreeComparer.Default)
                   .Select(child => child.GetHtml() as XObject)
                   .Prepend(new XAttribute("class", "nested"))
            )
        );
    }
}