using System.Globalization;
using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class ComponentTapeReelGeometryRepository
{
    public ComponentTapeReelGeometry? GetById(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection, "WHERE Id = $id");
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public List<ComponentTapeReelGeometry> GetByComponentId(int componentDefinitionId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection, "WHERE ComponentDefinitionId = $componentDefinitionId ORDER BY IsDefault DESC, Id");
        command.Parameters.AddWithValue("$componentDefinitionId", componentDefinitionId);
        using var reader = command.ExecuteReader();

        var result = new List<ComponentTapeReelGeometry>();
        while (reader.Read()) result.Add(Map(reader));
        return result;
    }

    public ComponentTapeReelGeometry? GetDefaultByComponentId(int componentDefinitionId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection, "WHERE ComponentDefinitionId = $componentDefinitionId AND IsDefault = 1");
        command.Parameters.AddWithValue("$componentDefinitionId", componentDefinitionId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public ComponentTapeReelGeometry Create(ComponentTapeReelGeometry entity)
    {
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();

        if (entity.IsDefault)
            ClearDefault(connection, transaction, entity.ComponentDefinitionId);

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        INSERT INTO ComponentTapeReelGeometry
        (
            ComponentDefinitionId, PackagingCode, SourceReference, SourceRevision, TapeStandard,
            CarrierTapeWidth, PocketPitch, PocketLength, PocketWidth, PocketDepth, PocketOffsetX, PocketOffsetY,
            SprocketHolePitch, SprocketHoleDiameter, SprocketHoleOffset, CoverTapeWidth,
            FeedDirection, PocketOrientation, Pin1Orientation, PickupRotation,
            ReelDiameter, HubDiameter, QuantityPerReel, IsDefault, IsActive, VerificationStatus,
            Notes, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version, ChangeComment
        )
        VALUES
        (
            $componentDefinitionId, $packagingCode, $sourceReference, $sourceRevision, $tapeStandard,
            $carrierTapeWidth, $pocketPitch, $pocketLength, $pocketWidth, $pocketDepth, $pocketOffsetX, $pocketOffsetY,
            $sprocketHolePitch, $sprocketHoleDiameter, $sprocketHoleOffset, $coverTapeWidth,
            $feedDirection, $pocketOrientation, $pin1Orientation, $pickupRotation,
            $reelDiameter, $hubDiameter, $quantityPerReel, $isDefault, $isActive, $verificationStatus,
            $notes, $createdAt, $createdBy, $updatedAt, $updatedBy, $version, $changeComment
        );
        SELECT last_insert_rowid();
        """;

        var now = DateTime.Now;
        AddParameters(command, entity);
        command.Parameters.AddWithValue("$createdAt", entity.CreatedAt == default ? now : entity.CreatedAt);
        command.Parameters.AddWithValue("$updatedAt", entity.UpdatedAt == default ? now : entity.UpdatedAt);
        command.Parameters.AddWithValue("$version", entity.Version <= 0 ? 1 : entity.Version);

        entity.Id = Convert.ToInt32(command.ExecuteScalar());
        transaction.Commit();
        return entity;
    }

    public void Update(ComponentTapeReelGeometry entity)
    {
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();

        EnsureExists(connection, transaction, entity.Id);
        if (entity.IsDefault)
            ClearDefault(connection, transaction, entity.ComponentDefinitionId, entity.Id);

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        UPDATE ComponentTapeReelGeometry
        SET
            ComponentDefinitionId = $componentDefinitionId,
            PackagingCode = $packagingCode,
            SourceReference = $sourceReference,
            SourceRevision = $sourceRevision,
            TapeStandard = $tapeStandard,
            CarrierTapeWidth = $carrierTapeWidth,
            PocketPitch = $pocketPitch,
            PocketLength = $pocketLength,
            PocketWidth = $pocketWidth,
            PocketDepth = $pocketDepth,
            PocketOffsetX = $pocketOffsetX,
            PocketOffsetY = $pocketOffsetY,
            SprocketHolePitch = $sprocketHolePitch,
            SprocketHoleDiameter = $sprocketHoleDiameter,
            SprocketHoleOffset = $sprocketHoleOffset,
            CoverTapeWidth = $coverTapeWidth,
            FeedDirection = $feedDirection,
            PocketOrientation = $pocketOrientation,
            Pin1Orientation = $pin1Orientation,
            PickupRotation = $pickupRotation,
            ReelDiameter = $reelDiameter,
            HubDiameter = $hubDiameter,
            QuantityPerReel = $quantityPerReel,
            IsDefault = $isDefault,
            IsActive = $isActive,
            VerificationStatus = $verificationStatus,
            Notes = $notes,
            UpdatedAt = $updatedAt,
            UpdatedBy = $updatedBy,
            Version = Version + 1,
            ChangeComment = $changeComment
        WHERE Id = $id;
        """;

        AddParameters(command, entity);
        command.Parameters.AddWithValue("$id", entity.Id);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now);
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public void Delete(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();

        var entity = GetById(connection, transaction, id)
            ?? throw new InvalidOperationException("Tape-and-reel geometry was not found.");
        if (entity.IsDefault)
            throw new InvalidOperationException("The default tape-and-reel geometry cannot be deleted without selecting a replacement default profile.");

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "DELETE FROM ComponentTapeReelGeometry WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public void SetDefault(int componentDefinitionId, int geometryId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();

        using (var check = connection.CreateCommand())
        {
            check.Transaction = transaction;
            check.CommandText = "SELECT COUNT(*) FROM ComponentTapeReelGeometry WHERE Id = $geometryId AND ComponentDefinitionId = $componentDefinitionId;";
            check.Parameters.AddWithValue("$geometryId", geometryId);
            check.Parameters.AddWithValue("$componentDefinitionId", componentDefinitionId);
            if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                throw new InvalidOperationException("The tape-and-reel geometry does not belong to the specified component.");
        }

        ClearDefault(connection, transaction, componentDefinitionId);

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "UPDATE ComponentTapeReelGeometry SET IsDefault = 1, UpdatedAt = $updatedAt, Version = Version + 1 WHERE Id = $geometryId AND ComponentDefinitionId = $componentDefinitionId;";
        command.Parameters.AddWithValue("$geometryId", geometryId);
        command.Parameters.AddWithValue("$componentDefinitionId", componentDefinitionId);
        command.Parameters.AddWithValue("$updatedAt", DateTime.Now);
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    private static SqliteCommand CreateSelectCommand(SqliteConnection connection, string whereClause)
    {
        var command = connection.CreateCommand();
        command.CommandText =
        $"""
        SELECT Id, ComponentDefinitionId, PackagingCode, SourceReference, SourceRevision, TapeStandard,
               CarrierTapeWidth, PocketPitch, PocketLength, PocketWidth, PocketDepth, PocketOffsetX, PocketOffsetY,
               SprocketHolePitch, SprocketHoleDiameter, SprocketHoleOffset, CoverTapeWidth,
               FeedDirection, PocketOrientation, Pin1Orientation, PickupRotation,
               ReelDiameter, HubDiameter, QuantityPerReel, IsDefault, IsActive, VerificationStatus,
               Notes, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Version, ChangeComment
        FROM ComponentTapeReelGeometry
        {whereClause};
        """;
        return command;
    }

    private static ComponentTapeReelGeometry? GetById(SqliteConnection connection, SqliteTransaction transaction, int id)
    {
        using var command = CreateSelectCommand(connection, "WHERE Id = $id");
        command.Transaction = transaction;
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private static void EnsureExists(SqliteConnection connection, SqliteTransaction transaction, int id)
    {
        if (GetById(connection, transaction, id) is null)
            throw new InvalidOperationException("Tape-and-reel geometry was not found.");
    }

    private static void ClearDefault(SqliteConnection connection, SqliteTransaction transaction, int componentDefinitionId, int exceptId = 0)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "UPDATE ComponentTapeReelGeometry SET IsDefault = 0 WHERE ComponentDefinitionId = $componentDefinitionId AND Id <> $exceptId;";
        command.Parameters.AddWithValue("$componentDefinitionId", componentDefinitionId);
        command.Parameters.AddWithValue("$exceptId", exceptId);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(SqliteCommand command, ComponentTapeReelGeometry entity)
    {
        command.Parameters.AddWithValue("$componentDefinitionId", entity.ComponentDefinitionId);
        command.Parameters.AddWithValue("$packagingCode", entity.PackagingCode ?? "");
        command.Parameters.AddWithValue("$sourceReference", entity.SourceReference ?? "");
        command.Parameters.AddWithValue("$sourceRevision", entity.SourceRevision ?? "");
        command.Parameters.AddWithValue("$tapeStandard", entity.TapeStandard ?? "");
        command.Parameters.AddWithValue("$carrierTapeWidth", entity.CarrierTapeWidth);
        command.Parameters.AddWithValue("$pocketPitch", entity.PocketPitch);
        command.Parameters.AddWithValue("$pocketLength", entity.PocketLength);
        command.Parameters.AddWithValue("$pocketWidth", entity.PocketWidth);
        command.Parameters.AddWithValue("$pocketDepth", entity.PocketDepth);
        command.Parameters.AddWithValue("$pocketOffsetX", entity.PocketOffsetX);
        command.Parameters.AddWithValue("$pocketOffsetY", entity.PocketOffsetY);
        command.Parameters.AddWithValue("$sprocketHolePitch", entity.SprocketHolePitch);
        command.Parameters.AddWithValue("$sprocketHoleDiameter", entity.SprocketHoleDiameter);
        command.Parameters.AddWithValue("$sprocketHoleOffset", entity.SprocketHoleOffset);
        command.Parameters.AddWithValue("$coverTapeWidth", entity.CoverTapeWidth);
        command.Parameters.AddWithValue("$feedDirection", (int)entity.FeedDirection);
        command.Parameters.AddWithValue("$pocketOrientation", (int)entity.PocketOrientation);
        command.Parameters.AddWithValue("$pin1Orientation", entity.Pin1Orientation ?? "");
        command.Parameters.AddWithValue("$pickupRotation", entity.PickupRotation);
        command.Parameters.AddWithValue("$reelDiameter", entity.ReelDiameter);
        command.Parameters.AddWithValue("$hubDiameter", entity.HubDiameter);
        command.Parameters.AddWithValue("$quantityPerReel", entity.QuantityPerReel);
        command.Parameters.AddWithValue("$isDefault", entity.IsDefault);
        command.Parameters.AddWithValue("$isActive", entity.IsActive);
        command.Parameters.AddWithValue("$verificationStatus", (int)entity.VerificationStatus);
        command.Parameters.AddWithValue("$notes", entity.Notes ?? "");
        command.Parameters.AddWithValue("$createdBy", entity.CreatedBy ?? "");
        command.Parameters.AddWithValue("$updatedBy", entity.UpdatedBy ?? "");
        command.Parameters.AddWithValue("$changeComment", entity.ChangeComment ?? "");
    }

    private static ComponentTapeReelGeometry Map(SqliteDataReader reader) => new()
    {
        Id = ReadInt32(reader, "Id"),
        ComponentDefinitionId = ReadInt32(reader, "ComponentDefinitionId"),
        PackagingCode = ReadString(reader, "PackagingCode"),
        SourceReference = ReadString(reader, "SourceReference"),
        SourceRevision = ReadString(reader, "SourceRevision"),
        TapeStandard = ReadString(reader, "TapeStandard"),
        CarrierTapeWidth = ReadDouble(reader, "CarrierTapeWidth"),
        PocketPitch = ReadDouble(reader, "PocketPitch"),
        PocketLength = ReadDouble(reader, "PocketLength"),
        PocketWidth = ReadDouble(reader, "PocketWidth"),
        PocketDepth = ReadDouble(reader, "PocketDepth"),
        PocketOffsetX = ReadDouble(reader, "PocketOffsetX"),
        PocketOffsetY = ReadDouble(reader, "PocketOffsetY"),
        SprocketHolePitch = ReadDouble(reader, "SprocketHolePitch"),
        SprocketHoleDiameter = ReadDouble(reader, "SprocketHoleDiameter"),
        SprocketHoleOffset = ReadDouble(reader, "SprocketHoleOffset"),
        CoverTapeWidth = ReadDouble(reader, "CoverTapeWidth"),
        FeedDirection = (TapeFeedDirection)ReadInt32(reader, "FeedDirection"),
        PocketOrientation = (TapePocketOrientation)ReadInt32(reader, "PocketOrientation"),
        Pin1Orientation = ReadString(reader, "Pin1Orientation"),
        PickupRotation = ReadDouble(reader, "PickupRotation"),
        ReelDiameter = ReadDouble(reader, "ReelDiameter"),
        HubDiameter = ReadDouble(reader, "HubDiameter"),
        QuantityPerReel = ReadInt32(reader, "QuantityPerReel"),
        IsDefault = ReadBoolean(reader, "IsDefault"),
        IsActive = ReadBoolean(reader, "IsActive"),
        VerificationStatus = (TapeVerificationStatus)ReadInt32(reader, "VerificationStatus"),
        Notes = ReadString(reader, "Notes"),
        CreatedAt = ReadDateTime(reader, "CreatedAt"),
        CreatedBy = ReadString(reader, "CreatedBy"),
        UpdatedAt = ReadDateTime(reader, "UpdatedAt"),
        UpdatedBy = ReadString(reader, "UpdatedBy"),
        Version = ReadInt32(reader, "Version"),
        ChangeComment = ReadString(reader, "ChangeComment")
    };

    private static string ReadString(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int ReadInt32(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    private static double ReadDouble(SqliteDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetDouble(ordinal);
    }

    private static bool ReadBoolean(SqliteDataReader reader, string name) => ReadInt32(reader, name) != 0;

    private static DateTime ReadDateTime(SqliteDataReader reader, string name) =>
        DateTime.TryParse(ReadString(reader, name), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : default;
}
