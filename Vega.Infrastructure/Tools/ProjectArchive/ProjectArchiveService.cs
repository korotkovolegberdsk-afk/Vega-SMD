using System.IO.Compression;

namespace Vega.Infrastructure.Tools.ProjectArchive;

public class ProjectArchiveService
{
    public string CreateArchive(
        string projectFolder,
        string outputFolder)
    {
        if (!Directory.Exists(projectFolder))
            throw new DirectoryNotFoundException(projectFolder);


        Directory.CreateDirectory(outputFolder);


        var projectName = new DirectoryInfo(projectFolder).Name;

        var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");


        var archivePath = Path.Combine(
            outputFolder,
            $"{projectName}_{date}.zip");


        if (File.Exists(archivePath))
            File.Delete(archivePath);


        using var archive = ZipFile.Open(
            archivePath,
            ZipArchiveMode.Create);


        foreach (var file in Directory.GetFiles(
                     projectFolder,
                     "*",
                     SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(
                projectFolder,
                file);


            if (ShouldSkip(relativePath))
                continue;


            archive.CreateEntryFromFile(
                file,
                relativePath);
        }


        return archivePath;
    }



    private bool ShouldSkip(string relativePath)
    {
        var parts = relativePath.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);


        foreach (var part in parts)
        {
            if (part.Equals(".vs",
                StringComparison.OrdinalIgnoreCase))
                return true;


            if (part.Equals("bin",
                StringComparison.OrdinalIgnoreCase))
                return true;


            if (part.Equals("obj",
                StringComparison.OrdinalIgnoreCase))
                return true;


            if (part.Equals(".git",
                StringComparison.OrdinalIgnoreCase))
                return true;


            if (part.Equals("Archives",
                StringComparison.OrdinalIgnoreCase))
                return true;
        }


        if (relativePath.EndsWith(".zip",
            StringComparison.OrdinalIgnoreCase))
            return true;


        return false;
    }
}