using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class PackageManufacturerDrawingAssetRepository
{
    public PackageManufacturerDrawingAsset? GetById(int id)
    {
        if (id <= 0) return null;
        using var c = MasterLibraryConnection.Create();
        return ReadOne(c, id);
    }

    public List<PackageManufacturerDrawingAsset> GetByGeometryId(int geometryId)
    {
        if (geometryId <= 0) return [];
        using var c = MasterLibraryConnection.Create();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM PackageManufacturerDrawingAsset WHERE PackageManufacturerDrawingGeometryId=$geometryId ORDER BY ProjectionType, Id;";
        cmd.Parameters.AddWithValue("$geometryId", geometryId);
        return ReadMany(cmd);
    }

    public PackageManufacturerDrawingAsset? GetCurrentVerified(int geometryId, string projectionType) => GetCurrentVerified(geometryId, projectionType, null);

    public PackageManufacturerDrawingAsset? GetCurrentVerified(int geometryId, string projectionType, string? assetType)
    {
        if (geometryId <= 0 || string.IsNullOrWhiteSpace(projectionType)) return null;
        using var c = MasterLibraryConnection.Create();
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM PackageManufacturerDrawingAsset
            WHERE PackageManufacturerDrawingGeometryId=$geometryId
              AND ProjectionType=$projectionType
              AND VerificationStatus='Verified' AND IsCurrent=1 AND IsActive=1
              AND ($assetType IS NULL OR AssetType=$assetType)
            ORDER BY Id DESC LIMIT 1;
            """;
        cmd.Parameters.AddWithValue("$geometryId", geometryId); cmd.Parameters.AddWithValue("$projectionType", projectionType.Trim()); cmd.Parameters.AddWithValue("$assetType", (object?)assetType?.Trim() ?? DBNull.Value);
        using var reader = cmd.ExecuteReader(); return reader.Read() ? Map(reader) : null;
    }

    public void Add(PackageManufacturerDrawingAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        using var c = MasterLibraryConnection.Create(); using var tx = c.BeginTransaction();
        using var cmd = c.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO PackageManufacturerDrawingAsset
            (PackageManufacturerDrawingGeometryId, ProjectionGeometryId, ProjectionType,
             SourceDocument, SourceRevision, SourcePage, AssetType, FilePath,
             DocumentReference, CropReference, VerificationStatus, IsCurrent, IsActive,
             Sha256, MimeType, Width, Height, Notes, CreatedAt, UpdatedAt)
            VALUES ($geometryId, $projectionId, $projectionType, $sourceDocument,
                    $sourceRevision, $sourcePage, $assetType, $filePath,
                    $documentReference, $cropReference, $verificationStatus, $isCurrent,
                    $isActive, $sha256, $mimeType, $width, $height, $notes,
                    $createdAt, $updatedAt);
            SELECT last_insert_rowid();
            """;
        AddParameters(cmd, asset); asset.Id = Convert.ToInt32(cmd.ExecuteScalar()); tx.Commit();
    }

    public void Update(PackageManufacturerDrawingAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset); if (asset.Id <= 0) throw new ArgumentException("Asset Id must be specified.", nameof(asset));
        using var c = MasterLibraryConnection.Create(); using var tx = c.BeginTransaction(); using var cmd = c.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = """
            UPDATE PackageManufacturerDrawingAsset SET
              PackageManufacturerDrawingGeometryId=$geometryId, ProjectionGeometryId=$projectionId,
              ProjectionType=$projectionType, SourceDocument=$sourceDocument, SourceRevision=$sourceRevision,
              SourcePage=$sourcePage, AssetType=$assetType, FilePath=$filePath,
              DocumentReference=$documentReference, CropReference=$cropReference,
              VerificationStatus=$verificationStatus, IsCurrent=$isCurrent, IsActive=$isActive,
              Sha256=$sha256, MimeType=$mimeType, Width=$width, Height=$height,
              Notes=$notes, UpdatedAt=$updatedAt
            WHERE Id=$id;
            """;
        AddParameters(cmd, asset); cmd.Parameters.AddWithValue("$id", asset.Id); if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Drawing asset was not found."); tx.Commit();
    }

    public void SetCurrent(int geometryId, int assetId)
    {
        if (geometryId <= 0 || assetId <= 0) throw new ArgumentException("Geometry and asset Ids must be specified.");
        using var c = MasterLibraryConnection.Create(); using var tx = c.BeginTransaction();
        using (var clear = c.CreateCommand()) { clear.Transaction = tx; clear.CommandText = "UPDATE PackageManufacturerDrawingAsset SET IsCurrent=0, UpdatedAt=datetime('now') WHERE PackageManufacturerDrawingGeometryId=$geometryId;"; clear.Parameters.AddWithValue("$geometryId", geometryId); clear.ExecuteNonQuery(); }
        using (var set = c.CreateCommand()) { set.Transaction = tx; set.CommandText = "UPDATE PackageManufacturerDrawingAsset SET IsCurrent=1, UpdatedAt=datetime('now') WHERE Id=$assetId AND PackageManufacturerDrawingGeometryId=$geometryId AND VerificationStatus='Verified' AND IsActive=1;"; set.Parameters.AddWithValue("$assetId", assetId); set.Parameters.AddWithValue("$geometryId", geometryId); if (set.ExecuteNonQuery() == 0) throw new InvalidOperationException("Only an active verified asset can be current."); }
        tx.Commit();
    }

    public void Reject(int assetId)
    {
        if (assetId <= 0) throw new ArgumentException("Asset Id must be specified.", nameof(assetId));
        using var c = MasterLibraryConnection.Create(); using var tx = c.BeginTransaction(); using var cmd = c.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = "UPDATE PackageManufacturerDrawingAsset SET VerificationStatus='Rejected', IsCurrent=0, IsActive=0, UpdatedAt=datetime('now') WHERE Id=$id;"; cmd.Parameters.AddWithValue("$id", assetId); cmd.ExecuteNonQuery(); tx.Commit();
    }

    private static void AddParameters(SqliteCommand cmd, PackageManufacturerDrawingAsset a)
    {
        cmd.Parameters.AddWithValue("$geometryId", a.PackageManufacturerDrawingGeometryId); cmd.Parameters.AddWithValue("$projectionId", (object?)a.ProjectionGeometryId ?? DBNull.Value); cmd.Parameters.AddWithValue("$projectionType", a.ProjectionType); cmd.Parameters.AddWithValue("$sourceDocument", a.SourceDocument); cmd.Parameters.AddWithValue("$sourceRevision", a.SourceRevision); cmd.Parameters.AddWithValue("$sourcePage", a.SourcePage); cmd.Parameters.AddWithValue("$assetType", a.AssetType); cmd.Parameters.AddWithValue("$filePath", a.FilePath); cmd.Parameters.AddWithValue("$documentReference", a.DocumentReference); cmd.Parameters.AddWithValue("$cropReference", a.CropReference); cmd.Parameters.AddWithValue("$verificationStatus", a.VerificationStatus); cmd.Parameters.AddWithValue("$isCurrent", a.IsCurrent); cmd.Parameters.AddWithValue("$isActive", a.IsActive); cmd.Parameters.AddWithValue("$sha256", a.Sha256); cmd.Parameters.AddWithValue("$mimeType", a.MimeType); cmd.Parameters.AddWithValue("$width", (object?)a.Width ?? DBNull.Value); cmd.Parameters.AddWithValue("$height", (object?)a.Height ?? DBNull.Value); cmd.Parameters.AddWithValue("$notes", a.Notes); cmd.Parameters.AddWithValue("$createdAt", a.CreatedAt == default ? DateTime.UtcNow : a.CreatedAt); cmd.Parameters.AddWithValue("$updatedAt", a.UpdatedAt == default ? DateTime.UtcNow : a.UpdatedAt);
    }

    private static PackageManufacturerDrawingAsset? ReadOne(SqliteConnection c, int id)
    { using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT * FROM PackageManufacturerDrawingAsset WHERE Id=$id;"; cmd.Parameters.AddWithValue("$id", id); using var r = cmd.ExecuteReader(); return r.Read() ? Map(r) : null; }
    private static List<PackageManufacturerDrawingAsset> ReadMany(SqliteCommand cmd) { using var r = cmd.ExecuteReader(); var list = new List<PackageManufacturerDrawingAsset>(); while (r.Read()) list.Add(Map(r)); return list; }
    private static PackageManufacturerDrawingAsset Map(SqliteDataReader r) => new() { Id = r.GetInt32(r.GetOrdinal("Id")), PackageManufacturerDrawingGeometryId = r.GetInt32(r.GetOrdinal("PackageManufacturerDrawingGeometryId")), ProjectionGeometryId = NullableInt(r, "ProjectionGeometryId"), ProjectionType = r.GetString(r.GetOrdinal("ProjectionType")), SourceDocument = r.GetString(r.GetOrdinal("SourceDocument")), SourceRevision = r.GetString(r.GetOrdinal("SourceRevision")), SourcePage = r.GetString(r.GetOrdinal("SourcePage")), AssetType = r.GetString(r.GetOrdinal("AssetType")), FilePath = r.GetString(r.GetOrdinal("FilePath")), DocumentReference = r.GetString(r.GetOrdinal("DocumentReference")), CropReference = r.GetString(r.GetOrdinal("CropReference")), VerificationStatus = r.GetString(r.GetOrdinal("VerificationStatus")), IsCurrent = r.GetInt32(r.GetOrdinal("IsCurrent")) != 0, IsActive = r.GetInt32(r.GetOrdinal("IsActive")) != 0, Sha256 = r.GetString(r.GetOrdinal("Sha256")), MimeType = r.GetString(r.GetOrdinal("MimeType")), Width = NullableDouble(r, "Width"), Height = NullableDouble(r, "Height"), Notes = r.GetString(r.GetOrdinal("Notes")), CreatedAt = ReadDate(r, "CreatedAt"), UpdatedAt = ReadDate(r, "UpdatedAt") };
    private static int? NullableInt(SqliteDataReader r, string n) => r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetInt32(r.GetOrdinal(n));
    private static double? NullableDouble(SqliteDataReader r, string n) => r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetDouble(r.GetOrdinal(n));
    private static DateTime ReadDate(SqliteDataReader r, string n) => DateTime.TryParse(r.GetString(r.GetOrdinal(n)), out var d) ? d : default;
}
