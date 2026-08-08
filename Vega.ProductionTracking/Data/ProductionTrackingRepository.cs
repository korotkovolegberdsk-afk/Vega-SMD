using Microsoft.Data.Sqlite;
using Vega.ProcessLearning.Data;
using Vega.ProductionTracking.Models;

namespace Vega.ProductionTracking.Data;

public class ProductionTrackingRepository
{
    private readonly string _databasePath;
    private readonly ProcessLearningRepository? _processLearning;

    public ProductionTrackingRepository(string? databasePath = null, ProcessLearningRepository? processLearning = null)
    {
        _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "ProductionTracking.db");
        _processLearning = processLearning;
        EnsureSchema();
    }

    public int CreateLot(ProductionLot lot)
    {
        ArgumentNullException.ThrowIfNull(lot);
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO ProductionLots (ProjectId, OrderNumber, Customer, BoardName, BoardRevision, StencilRevisionId, ReflowProfileId, ProcessLearningProjectId, StartDate, EndDate, LineName, Operator, Status) VALUES ($projectId, $orderNumber, $customer, $boardName, $boardRevision, $stencilRevisionId, $reflowProfileId, $processLearningProjectId, $startDate, $endDate, $lineName, $operator, $status); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$projectId", lot.ProjectId); command.Parameters.AddWithValue("$orderNumber", lot.OrderNumber); command.Parameters.AddWithValue("$customer", lot.Customer); command.Parameters.AddWithValue("$boardName", lot.BoardName); command.Parameters.AddWithValue("$boardRevision", lot.BoardRevision);
        command.Parameters.AddWithValue("$stencilRevisionId", Value(lot.StencilRevisionId)); command.Parameters.AddWithValue("$reflowProfileId", Value(lot.ReflowProfileId)); command.Parameters.AddWithValue("$processLearningProjectId", Value(lot.ProcessLearningProjectId)); command.Parameters.AddWithValue("$startDate", lot.StartDate.ToString("O")); command.Parameters.AddWithValue("$endDate", lot.EndDate is null ? DBNull.Value : lot.EndDate.Value.ToString("O")); command.Parameters.AddWithValue("$lineName", lot.LineName); command.Parameters.AddWithValue("$operator", lot.Operator); command.Parameters.AddWithValue("$status", lot.Status.ToString());
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public ProductionLot? GetLot(int id)
    {
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProjectId, OrderNumber, Customer, BoardName, BoardRevision, StencilRevisionId, ReflowProfileId, ProcessLearningProjectId, StartDate, EndDate, LineName, Operator, Status FROM ProductionLots WHERE Id = $id;"; command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader(); return reader.Read() ? ReadLot(reader) : null;
    }

    public int AddPasteBatch(SolderPasteBatch batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        using var connection = Open(); using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SolderPasteBatches (LotId, Manufacturer, PasteName, Alloy, BatchNumber, ExpirationDate, StorageCondition, OpenedDate, Notes) VALUES ($lotId, $manufacturer, $pasteName, $alloy, $batchNumber, $expirationDate, $storageCondition, $openedDate, $notes); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$lotId", batch.LotId); command.Parameters.AddWithValue("$manufacturer", batch.Manufacturer); command.Parameters.AddWithValue("$pasteName", batch.PasteName); command.Parameters.AddWithValue("$alloy", batch.Alloy); command.Parameters.AddWithValue("$batchNumber", batch.BatchNumber); command.Parameters.AddWithValue("$expirationDate", batch.ExpirationDate is null ? DBNull.Value : batch.ExpirationDate.Value.ToString("O")); command.Parameters.AddWithValue("$storageCondition", batch.StorageCondition); command.Parameters.AddWithValue("$openedDate", batch.OpenedDate is null ? DBNull.Value : batch.OpenedDate.Value.ToString("O")); command.Parameters.AddWithValue("$notes", batch.Notes);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<SolderPasteBatch> GetPasteBatches(int lotId)
    {
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "SELECT Id, LotId, Manufacturer, PasteName, Alloy, BatchNumber, ExpirationDate, StorageCondition, OpenedDate, Notes FROM SolderPasteBatches WHERE LotId = $lotId ORDER BY Id;"; command.Parameters.AddWithValue("$lotId", lotId);
        var result = new List<SolderPasteBatch>(); using var reader = command.ExecuteReader(); while (reader.Read()) result.Add(new SolderPasteBatch { Id = reader.GetInt32(0), LotId = reader.GetInt32(1), Manufacturer = reader.GetString(2), PasteName = reader.GetString(3), Alloy = reader.GetString(4), BatchNumber = reader.GetString(5), ExpirationDate = ReadDate(reader, 6), StorageCondition = reader.GetString(7), OpenedDate = ReadDate(reader, 8), Notes = reader.GetString(9) }); return result;
    }

    public int AddEquipment(ProductionEquipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO ProductionEquipment (EquipmentType, Manufacturer, Model, SerialNumber, Description) VALUES ($equipmentType, $manufacturer, $model, $serialNumber, $description); SELECT last_insert_rowid();"; command.Parameters.AddWithValue("$equipmentType", equipment.EquipmentType.ToString()); command.Parameters.AddWithValue("$manufacturer", equipment.Manufacturer); command.Parameters.AddWithValue("$model", equipment.Model); command.Parameters.AddWithValue("$serialNumber", equipment.SerialNumber); command.Parameters.AddWithValue("$description", equipment.Description); return Convert.ToInt32(command.ExecuteScalar());
    }

    public void AddLotEquipment(ProductionLotEquipment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment); using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "INSERT INTO ProductionLotEquipment (LotId, EquipmentId, Operation, Date) VALUES ($lotId, $equipmentId, $operation, $date);"; command.Parameters.AddWithValue("$lotId", assignment.LotId); command.Parameters.AddWithValue("$equipmentId", assignment.EquipmentId); command.Parameters.AddWithValue("$operation", assignment.Operation.ToString()); command.Parameters.AddWithValue("$date", assignment.Date.ToString("O")); command.ExecuteNonQuery();
    }

    public ProductionLotReport GetLotHistory(int lotId)
    {
        var lot = GetLot(lotId) ?? throw new InvalidOperationException($"Production lot {lotId} was not found.");
        var batches = GetPasteBatches(lotId); var equipment = GetEquipment(lotId);
        var defects = _processLearning?.GetDefectsByProductionLot(lotId) ?? new List<Vega.ProcessLearning.Models.ProcessDefectRecord>();
        var totalDefects = defects.Sum(defect => defect.Quantity);
        return new ProductionLotReport { Lot = lot, Stencil = lot.StencilRevisionId is null ? "Not assigned" : $"Revision #{lot.StencilRevisionId}", Reflow = lot.ReflowProfileId is null ? "Not assigned" : $"Profile #{lot.ReflowProfileId}", Paste = batches.Count == 0 ? "Not assigned" : string.Join(", ", batches.Select(batch => $"{batch.Manufacturer} {batch.PasteName} {batch.Alloy} ({batch.BatchNumber})")), Equipment = equipment, Defects = defects, Yield = Math.Max(0, 100 - totalDefects), Recommendations = totalDefects == 0 ? Array.Empty<string>() : new[] { "Review defects and process parameters before the next lot." } };
    }

    private List<ProductionEquipment> GetEquipment(int lotId)
    {
        using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "SELECT e.Id, e.EquipmentType, e.Manufacturer, e.Model, e.SerialNumber, e.Description FROM ProductionEquipment e INNER JOIN ProductionLotEquipment le ON le.EquipmentId = e.Id WHERE le.LotId = $lotId ORDER BY le.Date, e.Id;"; command.Parameters.AddWithValue("$lotId", lotId); var result = new List<ProductionEquipment>(); using var reader = command.ExecuteReader(); while (reader.Read()) result.Add(new ProductionEquipment { Id = reader.GetInt32(0), EquipmentType = Enum.Parse<ProductionEquipmentType>(reader.GetString(1)), Manufacturer = reader.GetString(2), Model = reader.GetString(3), SerialNumber = reader.GetString(4), Description = reader.GetString(5) }); return result;
    }

    private SqliteConnection Open() { var directory = Path.GetDirectoryName(Path.GetFullPath(_databasePath)); Directory.CreateDirectory(directory!); var connection = new SqliteConnection($"Data Source={_databasePath};Pooling=False"); connection.Open(); return connection; }
    private void EnsureSchema() { using var connection = Open(); using var command = connection.CreateCommand(); command.CommandText = "CREATE TABLE IF NOT EXISTS ProductionLots (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProjectId INTEGER NOT NULL, OrderNumber TEXT NOT NULL, Customer TEXT NOT NULL, BoardName TEXT NOT NULL, BoardRevision TEXT NOT NULL, StencilRevisionId INTEGER NULL, ReflowProfileId INTEGER NULL, ProcessLearningProjectId INTEGER NULL, StartDate TEXT NOT NULL, EndDate TEXT NULL, LineName TEXT NOT NULL, Operator TEXT NOT NULL, Status TEXT NOT NULL); CREATE TABLE IF NOT EXISTS SolderPasteBatches (Id INTEGER PRIMARY KEY AUTOINCREMENT, LotId INTEGER NOT NULL, Manufacturer TEXT NOT NULL, PasteName TEXT NOT NULL, Alloy TEXT NOT NULL, BatchNumber TEXT NOT NULL, ExpirationDate TEXT NULL, StorageCondition TEXT NOT NULL, OpenedDate TEXT NULL, Notes TEXT NOT NULL); CREATE TABLE IF NOT EXISTS ProductionEquipment (Id INTEGER PRIMARY KEY AUTOINCREMENT, EquipmentType TEXT NOT NULL, Manufacturer TEXT NOT NULL, Model TEXT NOT NULL, SerialNumber TEXT NOT NULL, Description TEXT NOT NULL); CREATE TABLE IF NOT EXISTS ProductionLotEquipment (LotId INTEGER NOT NULL, EquipmentId INTEGER NOT NULL, Operation TEXT NOT NULL, Date TEXT NOT NULL, PRIMARY KEY (LotId, EquipmentId, Operation, Date)); CREATE INDEX IF NOT EXISTS IX_ProductionLots_OrderNumber ON ProductionLots(OrderNumber); CREATE INDEX IF NOT EXISTS IX_SolderPasteBatches_LotId ON SolderPasteBatches(LotId); CREATE INDEX IF NOT EXISTS IX_ProductionLotEquipment_LotId ON ProductionLotEquipment(LotId);"; command.ExecuteNonQuery(); }
    private static object Value(int? value) => value is null ? DBNull.Value : value.Value;
    private static DateTime? ReadDate(SqliteDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? null : DateTime.Parse(reader.GetString(ordinal));
    private static ProductionLot ReadLot(SqliteDataReader reader) => new() { Id = reader.GetInt32(0), ProjectId = reader.GetInt32(1), OrderNumber = reader.GetString(2), Customer = reader.GetString(3), BoardName = reader.GetString(4), BoardRevision = reader.GetString(5), StencilRevisionId = reader.IsDBNull(6) ? null : reader.GetInt32(6), ReflowProfileId = reader.IsDBNull(7) ? null : reader.GetInt32(7), ProcessLearningProjectId = reader.IsDBNull(8) ? null : reader.GetInt32(8), StartDate = DateTime.Parse(reader.GetString(9)), EndDate = ReadDate(reader, 10), LineName = reader.GetString(11), Operator = reader.GetString(12), Status = Enum.Parse<ProductionLotStatus>(reader.GetString(13)) };
}