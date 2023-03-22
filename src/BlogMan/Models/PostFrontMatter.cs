using YamlDotNet.Serialization;

namespace BlogMan.Models;

[Serializable, YamlSerializable]
public class PostFrontMatter
{
    public string? Layout { get; set; }
    public string? Title { get; set; }
    public DateTime[]? Date { get; set; }
    public string[]? Topic { get; set; }

    public PostFrontMatter(string layout, string title, DateTime[] date, string[] topic)
    {
        Layout = layout;
        Title = title;
        Date = date;
        Topic = topic;
    }
}