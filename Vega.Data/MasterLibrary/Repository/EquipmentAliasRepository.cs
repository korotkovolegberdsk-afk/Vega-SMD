using System.Globalization;
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class EquipmentAliasRepository
{
    public List<EquipmentAlias> GetByPackageId(int packageId)
    {
        var aliases = new List<EquipmentAlias>();

        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT Id, PackageId, EquipmentType, Vendor, Model, Alias, Notes,
               IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
               Version, ChangeComment
        FROM EquipmentAlias
        WHERE PackageId = $packageId
          AND IsActive = 1
        ORDER BY Vendor, Alias;
        """;
        command.Parameters.AddWithValue("$packageId", packageId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            aliases.Add(Map(reader));
        }

        return aliases;
    }

    public void Add(EquipmentAlias alias)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        var now = DateTime.Now;

        command.CommandText =
        """
        INSERT INTO EquipmentAlias
        (
            PackageId, EquipmentType, Vendor, Model, Alias, Notes, IsActive,
            CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version, ChangeComment
        )
        VALUES
        (
            $packageId, $equipmentType, $vendor, $model, $alias, $notes, $isActive,
            $createdAt, $createdBy, $updatedAt, $updatedBy, $version, $changeComment
        );
        """;

        AddParameters(command, alias);
        command.Parameters.AddWithValue(
            "$createdAt", alias.CreatedAt == default ? now : alias.CreatedAt);
        command.Parameters.AddWithValue(
            "$updatedAt", alias.UpdatedAt == default ? now : alias.UpdatedAt);
        command.Parameters.AddWithValue("$version", alias.Version <= 0 ? 1 : alias.Version);
        command.ExecuteNonQuery();
    }

    public void Update(EquipmentAlias alias)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        UPDATE EquipmentAlias
        SET
            EquipmentType = $equipmentType,
            Vendor = $vendor,
            Model = $model,
            Alias = $alias,
            Notes = $notes,
            IsActive = $isActive,
            UpdatedAt = $updatedAt,
            UpdatedBy = $updatedBy,
            Version = Version + 1,
            ChangeComment = $changeComment
        WHERE Id = $id;
        """;

        AddParameters(command, alias);
        command.Parameters.AddWithValue("$id", alias.Id);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM EquipmentAlias WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, EquipmentAlias alias)
    {
        command.Parameters.AddWithValue("$packageId", alias.PackageId);
        command.Parameters.AddWithValue("$equipmentType", alias.EquipmentType);
        command.Parameters.AddWithValue("$vendor", alias.Vendor);
        command.Parameters.AddWithValue("$model", alias.Model);
        command.Parameters.AddWithValue("$alias", alias.Alias);
        command.Parameters.AddWithValue("$notes", alias.Notes);
        command.Parameters.AddWithValue("$isActive", alias.IsActive);
        command.Parameters.AddWithValue("$createdBy", alias.CreatedBy);
        command.Parameters.AddWithValue("$updatedBy", alias.UpdatedBy);
        command.Parameters.AddWithValue("$changeComment", alias.ChangeComment);
    }

    private static EquipmentAlias Map(SqliteDataReader reader) => new()
    {
        Id = ReadInt32(reader, "Id"),
        PackageId = ReadInt32(reader, "PackageId"),
        EquipmentType = ReadString(reader, "EquipmentType"),
        Vendor = ReadString(reader, "Vendor"),
        Model = ReadString(reader, "Model"),
        Alias = ReadString(reader, "Alias"),
        Notes = ReadString(reader, "Notes"),
        IsActive = ReadInt32(reader, "IsActive") != 0,
        CreatedAt = ReadDateTime(reader, "CreatedAt"),
        CreatedBy = ReadString(reader, "CreatedBy"),
        UpdatedAt = ReadDateTime(reader, "UpdatedAt"),
        UpdatedBy = ReadString(reader, "UpdatedBy"),
        Version = ReadInt32(reader, "Version"),
        ChangeComment = ReadString(reader, "ChangeComment")
    };

    private static string ReadString(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);
        return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
    }

    private static int ReadInt32(SqliteDataReader reader, string name)
    {
        var index = reader.GetOrdinal(name);
        return reader.IsDBNull(index) ? 0 : reader.GetInt32(index);
    }

    private static DateTime ReadDateTime(SqliteDataReader reader, string name)
    {
        return DateTime.TryParse(
            ReadString(reader, name),
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var value)
            ? value
            : default;
    }
}
