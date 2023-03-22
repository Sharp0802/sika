using System.Xml.Serialization;

namespace BlogMan.Models;

[Serializable, XmlRoot]
public class Project
{
    public string? Name { get; set; }
    public Version? ApiVersion { get; set; }
    public string? PostDirectory { get; set; }
    public string? LayoutDirectory { get; set; }
    public string? SiteDirectory { get; set; }
    public string? BuildDirectory { get; set; }

    public Project(string name, Version api, string post, string layout, string site, string build)
    {
        Name = name;
        ApiVersion = api;
        PostDirectory = post;
        LayoutDirectory = layout;
        SiteDirectory = site;
        BuildDirectory = build;
    }
}