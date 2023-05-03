namespace BlogMan.Components;

public static class DirectoryInfoExtension
{
    public static void CopyTo(this DirectoryInfo src, DirectoryInfo dst)
    {
        if (!dst.Exists)
            dst.Create();
        
        foreach (var file in src.EnumerateFiles())
            File.Copy(file.FullName, Path.Combine(dst.FullName, file.Name), true);

        foreach (var sub in src.EnumerateDirectories())
            CopyTo(sub, new DirectoryInfo(Path.Combine(dst.FullName, sub.Name)));
    }
}