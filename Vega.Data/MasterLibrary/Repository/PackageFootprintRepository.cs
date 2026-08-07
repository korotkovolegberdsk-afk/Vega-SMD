using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class PackageFootprintRepository
{
    public PackageFootprint? GetByPackageId(int packageId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT Id, PackageId, PatternName, StandardName, Description,
               PadCount, PadLength, PadWidth, PadPitch, Pin1Offset,
               RowCount, ColumnCount, PasteReduction, ApertureType
        FROM PackageFootprint
        WHERE PackageId = $packageId;
        """;
        command.Parameters.AddWithValue("$packageId", packageId);

        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public void Add(PackageFootprint footprint)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO PackageFootprint
        (
            PackageId, PatternName, StandardName, Description,
            PadCount, PadLength, PadWidth, PadPitch, Pin1Offset,
            RowCount, ColumnCount, PasteReduction, ApertureType
        )
        VALUES
        (
            $packageId, $patternName, $standardName, $description,
            $padCount, $padLength, $padWidth, $padPitch, $pin1Offset,
            $rowCount, $columnCount, $pasteReduction, $apertureType
        );
        """;
        AddParameters(command, footprint);
        command.ExecuteNonQuery();
    }

    public void Update(PackageFootprint footprint)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        UPDATE PackageFootprint
        SET
            PatternName = $patternName,
            StandardName = $standardName,
            Description = $description,
            PadCount = $padCount,
            PadLength = $padLength,
            PadWidth = $padWidth,
            PadPitch = $padPitch,
            Pin1Offset = $pin1Offset,
            RowCount = $rowCount,
            ColumnCount = $columnCount,
            PasteReduction = $pasteReduction,
            ApertureType = $apertureType
        WHERE Id = $id;
        """;
        AddParameters(command, footprint);
        command.Parameters.AddWithValue("$id", footprint.Id);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, PackageFootprint footprint)
    {
        command.Parameters.AddWithValue("$packageId", footprint.PackageId);
        command.Parameters.AddWithValue("$patternName", footprint.PatternName);
        command.Parameters.AddWithValue("$standardName", footprint.StandardName);
        command.Parameters.AddWithValue("$description", footprint.Description);
        command.Parameters.AddWithValue("$padCount", footprint.PadCount);
        command.Parameters.AddWithValue("$padLength", footprint.PadLength);
        command.Parameters.AddWithValue("$padWidth", footprint.PadWidth);
        command.Parameters.AddWithValue("$padPitch", footprint.PadPitch);
        command.Parameters.AddWithValue("$pin1Offset", footprint.Pin1Offset);
        command.Parameters.AddWithValue("$rowCount", footprint.RowCount);
        command.Parameters.AddWithValue("$columnCount", footprint.ColumnCount);
        command.Parameters.AddWithValue("$pasteReduction", footprint.PasteReduction);
        command.Parameters.AddWithValue("$apertureType", footprint.ApertureType);
    }

    private static PackageFootprint Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        PackageId = reader.GetInt32(reader.GetOrdinal("PackageId")),
        PatternName = reader.GetString(reader.GetOrdinal("PatternName")),
        StandardName = reader.GetString(reader.GetOrdinal("StandardName")),
        Description = reader.GetString(reader.GetOrdinal("Description")),
        PadCount = reader.GetInt32(reader.GetOrdinal("PadCount")),
        PadLength = reader.GetDouble(reader.GetOrdinal("PadLength")),
        PadWidth = reader.GetDouble(reader.GetOrdinal("PadWidth")),
        PadPitch = reader.GetDouble(reader.GetOrdinal("PadPitch")),
        Pin1Offset = reader.GetDouble(reader.GetOrdinal("Pin1Offset")),
        RowCount = reader.GetInt32(reader.GetOrdinal("RowCount")),
        ColumnCount = reader.GetInt32(reader.GetOrdinal("ColumnCount")),
        PasteReduction = reader.GetDouble(reader.GetOrdinal("PasteReduction")),
        ApertureType = reader.GetString(reader.GetOrdinal("ApertureType"))
    };
}
