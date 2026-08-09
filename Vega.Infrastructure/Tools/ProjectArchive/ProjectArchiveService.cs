using System.IO.Compression;

namespace Vega.Infrastructure.Tools.ProjectArchive;

public sealed record ProjectArchiveResult(string ArchivePath, int FileCount, long SizeBytes);

public class ProjectArchiveService
{
    public string CreateArchive(string projectFolder, string outputFolder)
    {
        if (!Directory.Exists(projectFolder))
            throw new DirectoryNotFoundException(projectFolder);

        Directory.CreateDirectory(outputFolder);
        var projectName = new DirectoryInfo(projectFolder).Name;
        var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        var archivePath = Path.Combine(outputFolder, $"{projectName}_{date}.zip");

        return CreateArchiveToFile(projectFolder, archivePath, includeSourceOnlyExclusions: false).ArchivePath;
    }

    public ProjectArchiveResult CreateSourceArchive(string sourceRoot, string archivePath)
    {
        if (!Directory.Exists(sourceRoot))
            throw new DirectoryNotFoundException(sourceRoot);
        if (string.IsNullOrWhiteSpace(archivePath))
            throw new ArgumentException("Archive path is required.", nameof(archivePath));

        return CreateArchiveToFile(sourceRoot, archivePath, includeSourceOnlyExclusions: true);
    }

    private ProjectArchiveResult CreateArchiveToFile(string sourceRoot, string archivePath, bool includeSourceOnlyExclusions)
    {
        var fullArchivePath = Path.GetFullPath(archivePath);
        var outputDirectory = Path.GetDirectoryName(fullArchivePath)
            ?? throw new InvalidOperationException("Archive output directory is invalid.");
        Directory.CreateDirectory(outputDirectory);

        var temporaryArchivePath = fullArchivePath + ".partial";
        if (File.Exists(temporaryArchivePath))
            File.Delete(temporaryArchivePath);

        var fileCount = 0;
        try
        {
            using (var archive = ZipFile.Open(temporaryArchivePath, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(sourceRoot, file);
                    if (ShouldSkip(relativePath, includeSourceOnlyExclusions))
                        continue;

                    try
                    {
                        archive.CreateEntryFromFile(file, relativePath);
                        fileCount++;
                    }
                    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                    {
                        throw new IOException($"Unable to add source file '{relativePath}' to the archive.", exception);
                    }
                }
            }

            if (File.Exists(fullArchivePath))
                File.Delete(fullArchivePath);
            File.Move(temporaryArchivePath, fullArchivePath);
            return new ProjectArchiveResult(fullArchivePath, fileCount, new FileInfo(fullArchivePath).Length);
        }
        catch
        {
            if (File.Exists(temporaryArchivePath))
                File.Delete(temporaryArchivePath);
            throw;
        }
    }

    private static bool ShouldSkip(string relativePath, bool includeSourceOnlyExclusions)
    {
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var part in parts)
        {
            if (part.Equals(".vs", StringComparison.OrdinalIgnoreCase)
                || part.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || part.Equals("obj", StringComparison.OrdinalIgnoreCase)
                || part.Equals(".git", StringComparison.OrdinalIgnoreCase)
                || part.Equals("Archives", StringComparison.OrdinalIgnoreCase))
                return true;

            if (includeSourceOnlyExclusions && (part.Equals("TestResults", StringComparison.OrdinalIgnoreCase)
                || part.Equals("_PackTemp", StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        if (relativePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return true;

        return includeSourceOnlyExclusions && (relativePath.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)
            || relativePath.EndsWith(".cache", StringComparison.OrdinalIgnoreCase)
            || relativePath.EndsWith(".suo", StringComparison.OrdinalIgnoreCase)
            || relativePath.EndsWith(".user", StringComparison.OrdinalIgnoreCase));
    }
}