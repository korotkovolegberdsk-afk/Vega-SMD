using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class PackageGeometryRepository
{
    public PackageGeometry? GetByPackageId(int packageId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT Id, PackageId, BodyLength, BodyWidth, BodyHeight,
               LeadLength, LeadWidth, LeadPitch, LeadCount,
               PadLength, PadWidth, PadPitch, CenterX, CenterY
        FROM PackageGeometry
        WHERE PackageId = $packageId;
        """;
        command.Parameters.AddWithValue("$packageId", packageId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public void Add(PackageGeometry geometry)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO PackageGeometry
        (
            PackageId, BodyLength, BodyWidth, BodyHeight,
            LeadLength, LeadWidth, LeadPitch, LeadCount,
            PadLength, PadWidth, PadPitch, CenterX, CenterY
        )
        VALUES
        (
            $packageId, $bodyLength, $bodyWidth, $bodyHeight,
            $leadLength, $leadWidth, $leadPitch, $leadCount,
            $padLength, $padWidth, $padPitch, $centerX, $centerY
        );
        """;
        AddParameters(command, geometry);
        command.ExecuteNonQuery();
    }

    public void Update(PackageGeometry geometry)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        UPDATE PackageGeometry
        SET
            BodyLength = $bodyLength,
            BodyWidth = $bodyWidth,
            BodyHeight = $bodyHeight,
            LeadLength = $leadLength,
            LeadWidth = $leadWidth,
            LeadPitch = $leadPitch,
            LeadCount = $leadCount,
            PadLength = $padLength,
            PadWidth = $padWidth,
            PadPitch = $padPitch,
            CenterX = $centerX,
            CenterY = $centerY
        WHERE Id = $id;
        """;
        AddParameters(command, geometry);
        command.Parameters.AddWithValue("$id", geometry.Id);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, PackageGeometry geometry)
    {
        command.Parameters.AddWithValue("$packageId", geometry.PackageId);
        command.Parameters.AddWithValue("$bodyLength", geometry.BodyLength);
        command.Parameters.AddWithValue("$bodyWidth", geometry.BodyWidth);
        command.Parameters.AddWithValue("$bodyHeight", geometry.BodyHeight);
        command.Parameters.AddWithValue("$leadLength", geometry.LeadLength);
        command.Parameters.AddWithValue("$leadWidth", geometry.LeadWidth);
        command.Parameters.AddWithValue("$leadPitch", geometry.LeadPitch);
        command.Parameters.AddWithValue("$leadCount", geometry.LeadCount);
        command.Parameters.AddWithValue("$padLength", geometry.PadLength);
        command.Parameters.AddWithValue("$padWidth", geometry.PadWidth);
        command.Parameters.AddWithValue("$padPitch", geometry.PadPitch);
        command.Parameters.AddWithValue("$centerX", geometry.CenterX);
        command.Parameters.AddWithValue("$centerY", geometry.CenterY);
    }

    private static PackageGeometry Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        PackageId = reader.GetInt32(reader.GetOrdinal("PackageId")),
        BodyLength = reader.GetDouble(reader.GetOrdinal("BodyLength")),
        BodyWidth = reader.GetDouble(reader.GetOrdinal("BodyWidth")),
        BodyHeight = reader.GetDouble(reader.GetOrdinal("BodyHeight")),
        LeadLength = reader.GetDouble(reader.GetOrdinal("LeadLength")),
        LeadWidth = reader.GetDouble(reader.GetOrdinal("LeadWidth")),
        LeadPitch = reader.GetDouble(reader.GetOrdinal("LeadPitch")),
        LeadCount = reader.GetInt32(reader.GetOrdinal("LeadCount")),
        PadLength = reader.GetDouble(reader.GetOrdinal("PadLength")),
        PadWidth = reader.GetDouble(reader.GetOrdinal("PadWidth")),
        PadPitch = reader.GetDouble(reader.GetOrdinal("PadPitch")),
        CenterX = reader.GetDouble(reader.GetOrdinal("CenterX")),
        CenterY = reader.GetDouble(reader.GetOrdinal("CenterY"))
    };
}
