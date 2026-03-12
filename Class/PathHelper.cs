using System;
using System.IO;
using System.Linq;

public static class PathHelper
{
    private static string _imagesDirectory;

    public static string ImagesDirectory
    {
        get
        {
            if (_imagesDirectory != null) return _imagesDirectory;

            string[] paths = {
                Path.Combine(GetProjectRoot(), "image-citizen"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DiplomApp"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DiplomApp", "Images"),
                Path.Combine(Path.GetTempPath(), "DiplomApp", "Images")
            };

            foreach (string path in paths)
            {
                try
                {
                    Directory.CreateDirectory(path);
                    _imagesDirectory = path;
                    return path;
                }
                catch { }
            }

            throw new Exception("Не удалось создать папку для изображений");
        }
    }

    private static string GetProjectRoot()
    { 
        var directory = AppContext.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(directory, "../../../"));

        while (!Directory.GetFiles(projectRoot, "*.csproj").Any() &&
               projectRoot != Path.GetPathRoot(projectRoot))
        {
            projectRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
        }
        return projectRoot;
    }
}
