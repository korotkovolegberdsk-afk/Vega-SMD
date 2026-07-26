using Microsoft.Data.Sqlite;
using Vega.Models.Packages;

namespace Vega.Data.Repositories;

public class PackageRepository
{
    private readonly string _connectionString;


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
        """
        SELECT *
        FROM Packages;
        """;


        using var reader = command.ExecuteReader();


        while (reader.Read())
        {
            packages.Add(new PackageSearchResult
            {
                Id = reader.GetInt32(0),
                PackageName = reader.GetString(1),
                DisplayName = reader.GetString(2),

                Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Family = reader.IsDBNull(4) ? "" : reader.GetString(4),

                Length = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                Width = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                Height = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),

                Pitch = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                LeadCount = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),

                IPCName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                JEDECName = reader.IsDBNull(11) ? "" : reader.GetString(11),

                YamahaName = reader.IsDBNull(12) ? "" : reader.GetString(12),
                MirtecName = reader.IsDBNull(13) ? "" : reader.GetString(13),

                StencilThickness = reader.IsDBNull(14) ? 0 : reader.GetDouble(14),

                AreaRatio = reader.IsDBNull(15) ? 0 : reader.GetDouble(15),
                AspectRatio = reader.IsDBNull(16) ? 0 : reader.GetDouble(16),

                ApertureType = reader.IsDBNull(17) ? "" : reader.GetString(17),

                TypicalDefects = reader.IsDBNull(18) ? "" : reader.GetString(18),

                AOIRecommendations = reader.IsDBNull(19) ? "" : reader.GetString(19),

                SPIRecommendations = reader.IsDBNull(20) ? "" : reader.GetString(20),

                Notes = reader.IsDBNull(21) ? "" : reader.GetString(21)
            });
        }


        return packages;
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