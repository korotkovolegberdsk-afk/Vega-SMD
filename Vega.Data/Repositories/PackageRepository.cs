using Microsoft.Data.Sqlite;
using Vega.Models.Packages;

namespace Vega.Data.Repositories;

public class PackageRepository
{
    private readonly string _connectionString;


    private const string PackageColumns =
        """
        Id,
        PackageName,
        DisplayName,
        Category,
        Family,
        Length,
        Width,
        Height,
        Pitch,
        LeadCount,
        IPCName,
        JEDECName,
        YamahaName,
        MirtecName,
        StencilThickness,
        AreaRatio,
        AspectRatio,
        ApertureType,
        TypicalDefects,
        AOIRecommendations,
        SPIRecommendations,
        Notes
        """;


    public PackageRepository()
    {
        _connectionString = "Data Source=SMT.db";
    }



    public List<PackageSearchResult> GetAll()
    {
        var packages = new List<PackageSearchResult>();

        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();

        command.CommandText =
        $"""
        SELECT {PackageColumns}
        FROM Packages
        ORDER BY PackageName;
        """;


        using var reader = command.ExecuteReader();


        while (reader.Read())
        {
            packages.Add(ReadPackage(reader));
        }


        return packages;
    }




    public PackageSearchResult? GetById(
        int id)
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();

        command.CommandText =
        $"""
        SELECT {PackageColumns}
        FROM Packages
        WHERE Id = $Id
        LIMIT 1;
        """;


        command.Parameters.AddWithValue(
            "$Id",
            id);


        using var reader = command.ExecuteReader();


        if (!reader.Read())
        {
            return null;
        }


        return ReadPackage(reader);
    }




    public void Add(PackageSearchResult package)
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();


        command.CommandText =
        """
        INSERT INTO Packages
        (
            PackageName,
            DisplayName,
            Category,
            Family,
            Length,
            Width,
            Height,
            Pitch,
            LeadCount,
            IPCName,
            JEDECName,
            YamahaName,
            MirtecName,
            StencilThickness,
            AreaRatio,
            AspectRatio,
            ApertureType,
            TypicalDefects,
            AOIRecommendations,
            SPIRecommendations,
            Notes
        )
        VALUES
        (
            $PackageName,
            $DisplayName,
            $Category,
            $Family,
            $Length,
            $Width,
            $Height,
            $Pitch,
            $LeadCount,
            $IPCName,
            $JEDECName,
            $YamahaName,
            $MirtecName,
            $StencilThickness,
            $AreaRatio,
            $AspectRatio,
            $ApertureType,
            $TypicalDefects,
            $AOIRecommendations,
            $SPIRecommendations,
            $Notes
        );
        """;


        AddParameters(command, package);


        command.ExecuteNonQuery();
    }




    public void Update(PackageSearchResult package)
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();


        command.CommandText =
        """
        UPDATE Packages
        SET
            PackageName = $PackageName,
            DisplayName = $DisplayName,
            Category = $Category,
            Family = $Family,

            Length = $Length,
            Width = $Width,
            Height = $Height,

            Pitch = $Pitch,
            LeadCount = $LeadCount,

            IPCName = $IPCName,
            JEDECName = $JEDECName,

            YamahaName = $YamahaName,
            MirtecName = $MirtecName,

            StencilThickness = $StencilThickness,

            AreaRatio = $AreaRatio,
            AspectRatio = $AspectRatio,

            ApertureType = $ApertureType,

            TypicalDefects = $TypicalDefects,

            AOIRecommendations = $AOIRecommendations,

            SPIRecommendations = $SPIRecommendations,

            Notes = $Notes

        WHERE Id = $Id;
        """;


        AddParameters(command, package);


        command.Parameters.AddWithValue(
            "$Id",
            package.Id);


        command.ExecuteNonQuery();
    }




    public void Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();


        var command = connection.CreateCommand();


        command.CommandText =
        """
        DELETE FROM Packages
        WHERE Id = $Id;
        """;


        command.Parameters.AddWithValue(
            "$Id",
            id);


        command.ExecuteNonQuery();
    }




    private static PackageSearchResult ReadPackage(
        SqliteDataReader reader)
    {
        return new PackageSearchResult
        {
            Id = GetInt32(reader, "Id"),
            PackageName = GetString(reader, "PackageName"),
            DisplayName = GetString(reader, "DisplayName"),

            Category = GetString(reader, "Category"),
            Family = GetString(reader, "Family"),

            Length = GetDouble(reader, "Length"),
            Width = GetDouble(reader, "Width"),
            Height = GetDouble(reader, "Height"),

            Pitch = GetDouble(reader, "Pitch"),
            LeadCount = GetInt32(reader, "LeadCount"),

            IPCName = GetString(reader, "IPCName"),
            JEDECName = GetString(reader, "JEDECName"),

            YamahaName = GetString(reader, "YamahaName"),
            MirtecName = GetString(reader, "MirtecName"),

            StencilThickness = GetDouble(reader, "StencilThickness"),

            AreaRatio = GetDouble(reader, "AreaRatio"),
            AspectRatio = GetDouble(reader, "AspectRatio"),

            ApertureType = GetString(reader, "ApertureType"),

            TypicalDefects = GetString(reader, "TypicalDefects"),

            AOIRecommendations = GetString(reader, "AOIRecommendations"),

            SPIRecommendations = GetString(reader, "SPIRecommendations"),

            Notes = GetString(reader, "Notes")
        };
    }




    private static string GetString(
        SqliteDataReader reader,
        string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? ""
            : reader.GetString(index);
    }




    private static double GetDouble(
        SqliteDataReader reader,
        string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? 0
            : reader.GetDouble(index);
    }




    private static int GetInt32(
        SqliteDataReader reader,
        string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? 0
            : reader.GetInt32(index);
    }




    private void AddParameters(
        SqliteCommand command,
        PackageSearchResult package)
    {
        command.Parameters.AddWithValue("$PackageName", package.PackageName);
        command.Parameters.AddWithValue("$DisplayName", package.DisplayName);
        command.Parameters.AddWithValue("$Category", package.Category);
        command.Parameters.AddWithValue("$Family", package.Family);

        command.Parameters.AddWithValue("$Length", package.Length);
        command.Parameters.AddWithValue("$Width", package.Width);
        command.Parameters.AddWithValue("$Height", package.Height);

        command.Parameters.AddWithValue("$Pitch", package.Pitch);
        command.Parameters.AddWithValue("$LeadCount", package.LeadCount);

        command.Parameters.AddWithValue("$IPCName", package.IPCName);
        command.Parameters.AddWithValue("$JEDECName", package.JEDECName);

        command.Parameters.AddWithValue("$YamahaName", package.YamahaName);
        command.Parameters.AddWithValue("$MirtecName", package.MirtecName);

        command.Parameters.AddWithValue("$StencilThickness", package.StencilThickness);

        command.Parameters.AddWithValue("$AreaRatio", package.AreaRatio);
        command.Parameters.AddWithValue("$AspectRatio", package.AspectRatio);

        command.Parameters.AddWithValue("$ApertureType", package.ApertureType);

        command.Parameters.AddWithValue("$TypicalDefects", package.TypicalDefects);

        command.Parameters.AddWithValue("$AOIRecommendations", package.AOIRecommendations);

        command.Parameters.AddWithValue("$SPIRecommendations", package.SPIRecommendations);

        command.Parameters.AddWithValue("$Notes", package.Notes);
    }
}
