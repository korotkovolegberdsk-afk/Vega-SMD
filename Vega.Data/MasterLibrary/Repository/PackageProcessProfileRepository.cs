using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class PackageProcessProfileRepository
{
    public PackageProcessProfile? GetByPackageId(int packageId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            PackageId,
            StencilThickness,
            ApertureType,
            AreaRatio,
            AspectRatio,
            SPIRecommendations,
            AOIRecommendations,
            TypicalDefects,
            PlacementRecommendations,
            ReflowRecommendations,
            InspectionPriority,
            Notes,
            IsActive,
            CreatedAt,
            CreatedBy,
            UpdatedAt,
            UpdatedBy,
            Version,
            ChangeComment
        FROM PackageProcessProfile
        WHERE PackageId = $packageId
        ORDER BY Id
        LIMIT 1;
        """;

        command.Parameters.AddWithValue("$packageId", packageId);

        using var reader = command.ExecuteReader();

        return reader.Read()
            ? Map(reader)
            : null;
    }

    public void Upsert(PackageProcessProfile profile)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT Id
        FROM PackageProcessProfile
        WHERE PackageId = $packageId
        ORDER BY Id
        LIMIT 1;
        """;

        command.Parameters.AddWithValue("$packageId", profile.PackageId);

        var existingId = command.ExecuteScalar();

        command.Parameters.Clear();

        if (existingId is null)
        {
            Insert(command, profile);
        }
        else
        {
            Update(command, profile, Convert.ToInt32(existingId));
        }
    }

    private static void Insert(
        SqliteCommand command,
        PackageProcessProfile profile)
    {
        var now = DateTime.Now;

        command.CommandText =
        """
        INSERT INTO PackageProcessProfile
        (
            PackageId,
            StencilThickness,
            ApertureType,
            AreaRatio,
            AspectRatio,
            SPIRecommendations,
            AOIRecommendations,
            TypicalDefects,
            PlacementRecommendations,
            ReflowRecommendations,
            InspectionPriority,
            Notes,
            IsActive,
            CreatedAt,
            CreatedBy,
            UpdatedAt,
            UpdatedBy,
            Version,
            ChangeComment
        )
        VALUES
        (
            $packageId,
            $stencilThickness,
            $apertureType,
            $areaRatio,
            $aspectRatio,
            $spiRecommendations,
            $aoiRecommendations,
            $typicalDefects,
            $placementRecommendations,
            $reflowRecommendations,
            $inspectionPriority,
            $notes,
            $isActive,
            $createdAt,
            $createdBy,
            $updatedAt,
            $updatedBy,
            $version,
            $changeComment
        );
        """;

        AddParameters(command, profile);
        command.Parameters.AddWithValue(
            "$createdAt",
            profile.CreatedAt == default ? now : profile.CreatedAt);
        command.Parameters.AddWithValue(
            "$updatedAt",
            profile.UpdatedAt == default ? now : profile.UpdatedAt);
        command.Parameters.AddWithValue(
            "$version",
            profile.Version <= 0 ? 1 : profile.Version);

        command.ExecuteNonQuery();
    }

    private static void Update(
        SqliteCommand command,
        PackageProcessProfile profile,
        int id)
    {
        command.CommandText =
        """
        UPDATE PackageProcessProfile
        SET
            StencilThickness = $stencilThickness,
            ApertureType = $apertureType,
            AreaRatio = $areaRatio,
            AspectRatio = $aspectRatio,
            SPIRecommendations = $spiRecommendations,
            AOIRecommendations = $aoiRecommendations,
            TypicalDefects = $typicalDefects,
            PlacementRecommendations = $placementRecommendations,
            ReflowRecommendations = $reflowRecommendations,
            InspectionPriority = $inspectionPriority,
            Notes = $notes,
            IsActive = $isActive,
            UpdatedAt = $updatedAt,
            UpdatedBy = $updatedBy,
            Version = Version + 1,
            ChangeComment = $changeComment
        WHERE Id = $id;
        """;

        AddParameters(command, profile);
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now);

        command.ExecuteNonQuery();
    }

    private static void AddParameters(
        SqliteCommand command,
        PackageProcessProfile profile)
    {
        command.Parameters.AddWithValue("$packageId", profile.PackageId);
        command.Parameters.AddWithValue("$stencilThickness", profile.StencilThickness);
        command.Parameters.AddWithValue("$apertureType", profile.ApertureType);
        command.Parameters.AddWithValue("$areaRatio", profile.AreaRatio);
        command.Parameters.AddWithValue("$aspectRatio", profile.AspectRatio);
        command.Parameters.AddWithValue("$spiRecommendations", profile.SPIRecommendations);
        command.Parameters.AddWithValue("$aoiRecommendations", profile.AOIRecommendations ?? "");
        command.Parameters.AddWithValue("$typicalDefects", profile.TypicalDefects);
        command.Parameters.AddWithValue("$placementRecommendations", profile.PlacementRecommendations);
        command.Parameters.AddWithValue("$reflowRecommendations", profile.ReflowRecommendations);
        command.Parameters.AddWithValue("$inspectionPriority", profile.InspectionPriority);
        command.Parameters.AddWithValue("$notes", profile.Notes);
        command.Parameters.AddWithValue("$isActive", profile.IsActive);
        command.Parameters.AddWithValue("$createdBy", profile.CreatedBy);
        command.Parameters.AddWithValue("$updatedBy", profile.UpdatedBy);
        command.Parameters.AddWithValue("$changeComment", profile.ChangeComment);
    }

    private static PackageProcessProfile Map(SqliteDataReader reader)
    {
        return new PackageProcessProfile
        {
            Id = ReadInt32(reader, "Id"),
            PackageId = ReadInt32(reader, "PackageId"),
            StencilThickness = ReadDouble(reader, "StencilThickness"),
            ApertureType = ReadString(reader, "ApertureType"),
            AreaRatio = ReadDouble(reader, "AreaRatio"),
            AspectRatio = ReadDouble(reader, "AspectRatio"),
            SPIRecommendations = ReadString(reader, "SPIRecommendations"),
            AOIRecommendations = ReadString(reader, "AOIRecommendations"),
            TypicalDefects = ReadString(reader, "TypicalDefects"),
            PlacementRecommendations = ReadString(reader, "PlacementRecommendations"),
            ReflowRecommendations = ReadString(reader, "ReflowRecommendations"),
            InspectionPriority = ReadString(reader, "InspectionPriority"),
            Notes = ReadString(reader, "Notes"),
            IsActive = ReadInt32(reader, "IsActive") != 0,
            CreatedBy = ReadString(reader, "CreatedBy"),
            UpdatedBy = ReadString(reader, "UpdatedBy"),
            Version = ReadInt32(reader, "Version"),
            ChangeComment = ReadString(reader, "ChangeComment")
        };
    }

    private static string ReadString(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? string.Empty
            : reader.GetString(index);
    }

    private static int ReadInt32(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? 0
            : reader.GetInt32(index);
    }

    private static double ReadDouble(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);

        return reader.IsDBNull(index)
            ? 0
            : reader.GetDouble(index);
    }
}
