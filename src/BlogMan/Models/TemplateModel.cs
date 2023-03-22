namespace BlogMan.Models;

public class TemplateModel
{
    public PostFrontMatter Metadata { get; }
    public string Html { get; }

    public TemplateModel(PostFrontMatter metadata, string html)
    {
        Metadata = metadata;
        Html = html;
    }
}