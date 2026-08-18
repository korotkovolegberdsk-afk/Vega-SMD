using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class PackageManufacturerDrawingGeometryRepository
{
    public PackageManufacturerDrawingGeometry? GetCurrentVerified(int packageDefinitionId)
    {
        if (packageDefinitionId <= 0) return null;
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id FROM PackageManufacturerDrawingGeometry
            WHERE PackageDefinitionId = $packageDefinitionId
              AND VerificationStatus = 'Verified'
              AND IsCurrent = 1
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$packageDefinitionId", packageDefinitionId);
        var id = command.ExecuteScalar();
        return id is null or DBNull ? null : LoadAggregate(connection, Convert.ToInt32(id));
    }

    public PackageManufacturerDrawingGeometry? GetById(int id)
    {
        if (id <= 0) return null;
        using var connection = MasterLibraryConnection.Create();
        return LoadAggregate(connection, id);
    }

    public List<PackageManufacturerDrawingGeometry> GetRevisions(int packageDefinitionId)
    {
        if (packageDefinitionId <= 0) return [];
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id FROM PackageManufacturerDrawingGeometry
            WHERE PackageDefinitionId = $packageDefinitionId
            ORDER BY IsCurrent DESC, Id DESC;
            """;
        command.Parameters.AddWithValue("$packageDefinitionId", packageDefinitionId);
        using var reader = command.ExecuteReader();
        var ids = new List<int>();
        while (reader.Read()) ids.Add(reader.GetInt32(0));
        reader.Close();
        return ids.Select(id => LoadAggregate(connection, id)!).Where(x => x is not null).ToList()!;
    }

    public void Add(PackageManufacturerDrawingGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();
        geometry.Id = InsertAggregate(connection, transaction, geometry);
        transaction.Commit();
    }

    public void Update(PackageManufacturerDrawingGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        if (geometry.Id <= 0) throw new ArgumentException("Geometry Id must be specified.", nameof(geometry));
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE PackageManufacturerDrawingGeometry
                SET PackageDefinitionId=$packageDefinitionId, Manufacturer=$manufacturer,
                    PackageSeries=$packageSeries, SourceDocument=$sourceDocument,
                    SourceRevision=$sourceRevision, SourcePage=$sourcePage,
                    VerificationStatus=$verificationStatus, IsActive=$isActive,
                    IsCurrent=$isCurrent, Notes=$notes, UpdatedAt=datetime('now')
                WHERE Id=$id;
                """;
            AddParentParameters(command, geometry);
            command.Parameters.AddWithValue("$id", geometry.Id);
            if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("Manufacturer geometry was not found.");
        }

        DeleteChildren(connection, transaction, geometry.Id);
        InsertChildren(connection, transaction, geometry);
        transaction.Commit();
    }

    public void SetCurrent(int packageDefinitionId, int geometryId)
    {
        if (packageDefinitionId <= 0 || geometryId <= 0) throw new ArgumentException("Package and geometry Ids must be specified.");
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();
        using (var clear = connection.CreateCommand())
        {
            clear.Transaction = transaction;
            clear.CommandText = "UPDATE PackageManufacturerDrawingGeometry SET IsCurrent=0, UpdatedAt=datetime('now') WHERE PackageDefinitionId=$packageDefinitionId;";
            clear.Parameters.AddWithValue("$packageDefinitionId", packageDefinitionId);
            clear.ExecuteNonQuery();
        }
        using (var set = connection.CreateCommand())
        {
            set.Transaction = transaction;
            set.CommandText = """
                UPDATE PackageManufacturerDrawingGeometry
                SET IsCurrent=1, UpdatedAt=datetime('now')
                WHERE Id=$geometryId AND PackageDefinitionId=$packageDefinitionId
                  AND VerificationStatus='Verified' AND IsActive=1;
                """;
            set.Parameters.AddWithValue("$geometryId", geometryId);
            set.Parameters.AddWithValue("$packageDefinitionId", packageDefinitionId);
            if (set.ExecuteNonQuery() == 0) throw new InvalidOperationException("Only an active verified geometry can be current.");
        }
        transaction.Commit();
    }

    public void Reject(int geometryId)
    {
        if (geometryId <= 0) throw new ArgumentException("Geometry Id must be specified.", nameof(geometryId));
        using var connection = MasterLibraryConnection.Create();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE PackageManufacturerDrawingGeometry
            SET VerificationStatus='Rejected', IsCurrent=0, IsActive=0, UpdatedAt=datetime('now')
            WHERE Id=$id;
            """;
        command.Parameters.AddWithValue("$id", geometryId);
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    private static int InsertAggregate(SqliteConnection connection, SqliteTransaction transaction, PackageManufacturerDrawingGeometry geometry)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO PackageManufacturerDrawingGeometry
            (PackageDefinitionId, Manufacturer, PackageSeries, SourceDocument, SourceRevision,
             SourcePage, VerificationStatus, IsActive, IsCurrent, Notes)
            VALUES ($packageDefinitionId, $manufacturer, $packageSeries, $sourceDocument,
                    $sourceRevision, $sourcePage, $verificationStatus, $isActive, $isCurrent, $notes);
            SELECT last_insert_rowid();
            """;
        AddParentParameters(command, geometry);
        var id = Convert.ToInt32(command.ExecuteScalar());
        geometry.Id = id;
        InsertChildren(connection, transaction, geometry);
        return id;
    }

    private static void InsertChildren(SqliteConnection connection, SqliteTransaction transaction, PackageManufacturerDrawingGeometry geometry)
    {
        foreach (var projection in geometry.Projections)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO PackageProjectionGeometry
                (ManufacturerDrawingGeometryId, ProjectionType, CoordinateSystem, OriginX, OriginY,
                 BodyContour, LeadContour, AuxiliaryGeometry, IsAvailable)
                VALUES ($geometryId, $projectionType, $coordinateSystem, $originX, $originY,
                        $bodyContour, $leadContour, $auxiliaryGeometry, $isAvailable);
                SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$geometryId", geometry.Id);
            command.Parameters.AddWithValue("$projectionType", projection.ProjectionType);
            command.Parameters.AddWithValue("$coordinateSystem", projection.CoordinateSystem);
            command.Parameters.AddWithValue("$originX", projection.OriginX);
            command.Parameters.AddWithValue("$originY", projection.OriginY);
            command.Parameters.AddWithValue("$bodyContour", projection.BodyContour);
            command.Parameters.AddWithValue("$leadContour", projection.LeadContour);
            command.Parameters.AddWithValue("$auxiliaryGeometry", projection.AuxiliaryGeometry);
            command.Parameters.AddWithValue("$isAvailable", projection.IsAvailable);
            projection.Id = Convert.ToInt32(command.ExecuteScalar());
            projection.ManufacturerDrawingGeometryId = geometry.Id;
        }
        foreach (var lead in geometry.Leads)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO PackageLeadGeometry
                (ManufacturerDrawingGeometryId, LeadNumber, Side, ProjectionType, RootX, RootY,
                 ContactX, ContactY, ProfileGeometry, Width, Thickness, Pitch, IsPin1)
                VALUES ($geometryId, $leadNumber, $side, $projectionType, $rootX, $rootY,
                        $contactX, $contactY, $profileGeometry, $width, $thickness, $pitch, $isPin1);
                SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$geometryId", geometry.Id);
            command.Parameters.AddWithValue("$leadNumber", lead.LeadNumber);
            command.Parameters.AddWithValue("$side", lead.Side);
            command.Parameters.AddWithValue("$projectionType", lead.ProjectionType);
            command.Parameters.AddWithValue("$rootX", lead.RootX);
            command.Parameters.AddWithValue("$rootY", lead.RootY);
            command.Parameters.AddWithValue("$contactX", lead.ContactX);
            command.Parameters.AddWithValue("$contactY", lead.ContactY);
            command.Parameters.AddWithValue("$profileGeometry", lead.ProfileGeometry);
            command.Parameters.AddWithValue("$width", lead.Width);
            command.Parameters.AddWithValue("$thickness", lead.Thickness);
            command.Parameters.AddWithValue("$pitch", lead.Pitch);
            command.Parameters.AddWithValue("$isPin1", lead.IsPin1);
            lead.Id = Convert.ToInt32(command.ExecuteScalar());
            lead.ManufacturerDrawingGeometryId = geometry.Id;
        }
        foreach (var dimension in geometry.Dimensions)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO PackageDimensionTolerance
                (ManufacturerDrawingGeometryId, DimensionKey, ProjectionType, NominalValue,
                 MinValue, MaxValue, Symbol, Unit, SourceLabel)
                VALUES ($geometryId, $dimensionKey, $projectionType, $nominalValue,
                        $minValue, $maxValue, $symbol, $unit, $sourceLabel);
                SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$geometryId", geometry.Id);
            command.Parameters.AddWithValue("$dimensionKey", dimension.DimensionKey);
            command.Parameters.AddWithValue("$projectionType", dimension.ProjectionType);
            command.Parameters.AddWithValue("$nominalValue", (object?)dimension.NominalValue ?? DBNull.Value);
            command.Parameters.AddWithValue("$minValue", (object?)dimension.MinValue ?? DBNull.Value);
            command.Parameters.AddWithValue("$maxValue", (object?)dimension.MaxValue ?? DBNull.Value);
            command.Parameters.AddWithValue("$symbol", dimension.Symbol);
            command.Parameters.AddWithValue("$unit", dimension.Unit);
            command.Parameters.AddWithValue("$sourceLabel", dimension.SourceLabel);
            dimension.Id = Convert.ToInt32(command.ExecuteScalar());
            dimension.ManufacturerDrawingGeometryId = geometry.Id;
        }
        foreach (var marker in geometry.Pin1Markers)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO PackagePin1Geometry
                (ManufacturerDrawingGeometryId, ProjectionType, IsPresent, MarkerType,
                 PositionX, PositionY, Width, Height)
                VALUES ($geometryId, $projectionType, $isPresent, $markerType,
                        $positionX, $positionY, $width, $height);
                SELECT last_insert_rowid();
                """;
            command.Parameters.AddWithValue("$geometryId", geometry.Id);
            command.Parameters.AddWithValue("$projectionType", marker.ProjectionType);
            command.Parameters.AddWithValue("$isPresent", marker.IsPresent);
            command.Parameters.AddWithValue("$markerType", marker.MarkerType);
            command.Parameters.AddWithValue("$positionX", marker.PositionX);
            command.Parameters.AddWithValue("$positionY", marker.PositionY);
            command.Parameters.AddWithValue("$width", marker.Width);
            command.Parameters.AddWithValue("$height", marker.Height);
            marker.Id = Convert.ToInt32(command.ExecuteScalar());
            marker.ManufacturerDrawingGeometryId = geometry.Id;
        }
    }

    private static void DeleteChildren(SqliteConnection connection, SqliteTransaction transaction, int geometryId)
    {
        foreach (var table in new[] { "PackageProjectionGeometry", "PackageLeadGeometry", "PackageDimensionTolerance", "PackagePin1Geometry" })
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"DELETE FROM {table} WHERE ManufacturerDrawingGeometryId=$geometryId;";
            command.Parameters.AddWithValue("$geometryId", geometryId);
            command.ExecuteNonQuery();
        }
    }

    private static void AddParentParameters(SqliteCommand command, PackageManufacturerDrawingGeometry geometry)
    {
        command.Parameters.AddWithValue("$packageDefinitionId", geometry.PackageDefinitionId);
        command.Parameters.AddWithValue("$manufacturer", geometry.Manufacturer);
        command.Parameters.AddWithValue("$packageSeries", geometry.PackageSeries);
        command.Parameters.AddWithValue("$sourceDocument", geometry.SourceDocument);
        command.Parameters.AddWithValue("$sourceRevision", geometry.SourceRevision);
        command.Parameters.AddWithValue("$sourcePage", geometry.SourcePage);
        command.Parameters.AddWithValue("$verificationStatus", geometry.VerificationStatus);
        command.Parameters.AddWithValue("$isActive", geometry.IsActive);
        command.Parameters.AddWithValue("$isCurrent", geometry.IsCurrent);
        command.Parameters.AddWithValue("$notes", geometry.Notes);
    }

    private static PackageManufacturerDrawingGeometry? LoadAggregate(SqliteConnection connection, int id)
    {
        using var parent = connection.CreateCommand();
        parent.CommandText = "SELECT * FROM PackageManufacturerDrawingGeometry WHERE Id=$id;";
        parent.Parameters.AddWithValue("$id", id);
        using var reader = parent.ExecuteReader();
        if (!reader.Read()) return null;
        var geometry = new PackageManufacturerDrawingGeometry
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")), PackageDefinitionId = reader.GetInt32(reader.GetOrdinal("PackageDefinitionId")),
            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")), PackageSeries = reader.GetString(reader.GetOrdinal("PackageSeries")),
            SourceDocument = reader.GetString(reader.GetOrdinal("SourceDocument")), SourceRevision = reader.GetString(reader.GetOrdinal("SourceRevision")),
            SourcePage = reader.GetString(reader.GetOrdinal("SourcePage")), VerificationStatus = reader.GetString(reader.GetOrdinal("VerificationStatus")),
            IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) != 0, IsCurrent = reader.GetInt32(reader.GetOrdinal("IsCurrent")) != 0,
            Notes = reader.GetString(reader.GetOrdinal("Notes"))
        };
        reader.Close();
        geometry.Projections = ReadProjections(connection, id);
        geometry.Leads = ReadLeads(connection, id);
        geometry.Dimensions = ReadDimensions(connection, id);
        geometry.Pin1Markers = ReadPin1Markers(connection, id);
        return geometry;
    }

    private static List<ProjectionGeometry> ReadProjections(SqliteConnection c, int id) => Read(c, id, "SELECT * FROM PackageProjectionGeometry WHERE ManufacturerDrawingGeometryId=$id ORDER BY Id;", r => new ProjectionGeometry
    {
        Id = r.GetInt32(r.GetOrdinal("Id")), ManufacturerDrawingGeometryId = id, ProjectionType = r.GetString(r.GetOrdinal("ProjectionType")), CoordinateSystem = r.GetString(r.GetOrdinal("CoordinateSystem")), OriginX = r.GetDouble(r.GetOrdinal("OriginX")), OriginY = r.GetDouble(r.GetOrdinal("OriginY")), BodyContour = r.GetString(r.GetOrdinal("BodyContour")), LeadContour = r.GetString(r.GetOrdinal("LeadContour")), AuxiliaryGeometry = r.GetString(r.GetOrdinal("AuxiliaryGeometry")), IsAvailable = r.GetInt32(r.GetOrdinal("IsAvailable")) != 0
    });

    private static List<LeadGeometry> ReadLeads(SqliteConnection c, int id) => Read(c, id, "SELECT * FROM PackageLeadGeometry WHERE ManufacturerDrawingGeometryId=$id ORDER BY ProjectionType, LeadNumber, Id;", r => new LeadGeometry
    {
        Id = r.GetInt32(r.GetOrdinal("Id")), ManufacturerDrawingGeometryId = id, LeadNumber = r.GetInt32(r.GetOrdinal("LeadNumber")), Side = r.GetString(r.GetOrdinal("Side")), ProjectionType = r.GetString(r.GetOrdinal("ProjectionType")), RootX = r.GetDouble(r.GetOrdinal("RootX")), RootY = r.GetDouble(r.GetOrdinal("RootY")), ContactX = r.GetDouble(r.GetOrdinal("ContactX")), ContactY = r.GetDouble(r.GetOrdinal("ContactY")), ProfileGeometry = r.GetString(r.GetOrdinal("ProfileGeometry")), Width = r.GetDouble(r.GetOrdinal("Width")), Thickness = r.GetDouble(r.GetOrdinal("Thickness")), Pitch = r.GetDouble(r.GetOrdinal("Pitch")), IsPin1 = r.GetInt32(r.GetOrdinal("IsPin1")) != 0
    });

    private static List<DimensionTolerance> ReadDimensions(SqliteConnection c, int id) => Read(c, id, "SELECT * FROM PackageDimensionTolerance WHERE ManufacturerDrawingGeometryId=$id ORDER BY Id;", r => new DimensionTolerance
    {
        Id = r.GetInt32(r.GetOrdinal("Id")), ManufacturerDrawingGeometryId = id, DimensionKey = r.GetString(r.GetOrdinal("DimensionKey")), ProjectionType = r.GetString(r.GetOrdinal("ProjectionType")), NominalValue = NullableDouble(r, "NominalValue"), MinValue = NullableDouble(r, "MinValue"), MaxValue = NullableDouble(r, "MaxValue"), Symbol = r.GetString(r.GetOrdinal("Symbol")), Unit = r.GetString(r.GetOrdinal("Unit")), SourceLabel = r.GetString(r.GetOrdinal("SourceLabel"))
    });

    private static List<Pin1Geometry> ReadPin1Markers(SqliteConnection c, int id) => Read(c, id, "SELECT * FROM PackagePin1Geometry WHERE ManufacturerDrawingGeometryId=$id ORDER BY Id;", r => new Pin1Geometry
    {
        Id = r.GetInt32(r.GetOrdinal("Id")), ManufacturerDrawingGeometryId = id, ProjectionType = r.GetString(r.GetOrdinal("ProjectionType")), IsPresent = r.GetInt32(r.GetOrdinal("IsPresent")) != 0, MarkerType = r.GetString(r.GetOrdinal("MarkerType")), PositionX = r.GetDouble(r.GetOrdinal("PositionX")), PositionY = r.GetDouble(r.GetOrdinal("PositionY")), Width = r.GetDouble(r.GetOrdinal("Width")), Height = r.GetDouble(r.GetOrdinal("Height"))
    });

    private static List<T> Read<T>(SqliteConnection c, int id, string sql, Func<SqliteDataReader, T> map)
    {
        using var command = c.CreateCommand(); command.CommandText = sql; command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader(); var result = new List<T>(); while (reader.Read()) result.Add(map(reader)); return result;
    }

    private static double? NullableDouble(SqliteDataReader reader, string name) => reader.IsDBNull(reader.GetOrdinal(name)) ? null : reader.GetDouble(reader.GetOrdinal(name));
}
