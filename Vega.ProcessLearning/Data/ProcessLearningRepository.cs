using Microsoft.Data.Sqlite;
using Vega.ProcessLearning.Models;

namespace Vega.ProcessLearning.Data;

public class ProcessLearningRepository
{
    private readonly string _databasePath;

    public ProcessLearningRepository(string? databasePath = null)
    {
        _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "ProcessLearning.db");
        EnsureSchema();
    }

    public int AddDefect(ProcessDefectRecord defect)
    {
        ArgumentNullException.ThrowIfNull(defect);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO ProcessDefects (ProjectId, RevisionId, Date, ComponentRef, PackageId, DefectDefinitionId, ReflowProfileId, ProductionLotId, DefectType, Severity, Quantity, Description) VALUES ($projectId, $revisionId, $date, $componentRef, $packageId, $defectDefinitionId, $reflowProfileId, $productionLotId, $defectType, $severity, $quantity, $description); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$projectId", defect.ProjectId); command.Parameters.AddWithValue("$revisionId", defect.RevisionId); command.Parameters.AddWithValue("$date", defect.Date.ToString("O"));
        command.Parameters.AddWithValue("$componentRef", defect.ComponentRef); command.Parameters.AddWithValue("$packageId", defect.PackageId); command.Parameters.AddWithValue("$defectDefinitionId", defect.DefectDefinitionId is null ? DBNull.Value : defect.DefectDefinitionId.Value); command.Parameters.AddWithValue("$reflowProfileId", defect.ReflowProfileId is null ? DBNull.Value : defect.ReflowProfileId.Value); command.Parameters.AddWithValue("$productionLotId", defect.ProductionLotId is null ? DBNull.Value : defect.ProductionLotId.Value); command.Parameters.AddWithValue("$defectType", defect.DefectType.ToString());
        command.Parameters.AddWithValue("$severity", defect.Severity.ToString()); command.Parameters.AddWithValue("$quantity", defect.Quantity); command.Parameters.AddWithValue("$description", defect.Description);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<ProcessDefectRecord> GetDefectsByPackage(int packageId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProjectId, RevisionId, Date, ComponentRef, PackageId, DefectDefinitionId, ReflowProfileId, ProductionLotId, DefectType, Severity, Quantity, Description FROM ProcessDefects WHERE PackageId = $packageId ORDER BY Date DESC, Id DESC;";
        command.Parameters.AddWithValue("$packageId", packageId);
        var result = new List<ProcessDefectRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new ProcessDefectRecord
        {
            Id = reader.GetInt32(0), ProjectId = reader.GetInt32(1), RevisionId = reader.GetInt32(2), Date = DateTime.Parse(reader.GetString(3)), ComponentRef = reader.GetString(4),
            PackageId = reader.GetInt32(5), DefectDefinitionId = reader.IsDBNull(6) ? null : reader.GetInt32(6), ReflowProfileId = reader.IsDBNull(7) ? null : reader.GetInt32(7), ProductionLotId = reader.IsDBNull(8) ? null : reader.GetInt32(8), DefectType = Enum.Parse<ProcessDefectType>(reader.GetString(9)), Severity = Enum.Parse<ProcessDefectSeverity>(reader.GetString(10)), Quantity = reader.GetInt32(11), Description = reader.GetString(12)
        });
        return result;
    }

    public List<ProcessDefectRecord> GetDefectsByProductionLot(int productionLotId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProjectId, RevisionId, Date, ComponentRef, PackageId, DefectDefinitionId, ReflowProfileId, ProductionLotId, DefectType, Severity, Quantity, Description FROM ProcessDefects WHERE ProductionLotId = $productionLotId ORDER BY Date DESC, Id DESC;";
        command.Parameters.AddWithValue("$productionLotId", productionLotId);
        var result = new List<ProcessDefectRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new ProcessDefectRecord
        {
            Id = reader.GetInt32(0), ProjectId = reader.GetInt32(1), RevisionId = reader.GetInt32(2), Date = DateTime.Parse(reader.GetString(3)), ComponentRef = reader.GetString(4),
            PackageId = reader.GetInt32(5), DefectDefinitionId = reader.IsDBNull(6) ? null : reader.GetInt32(6), ReflowProfileId = reader.IsDBNull(7) ? null : reader.GetInt32(7), ProductionLotId = reader.IsDBNull(8) ? null : reader.GetInt32(8), DefectType = Enum.Parse<ProcessDefectType>(reader.GetString(9)), Severity = Enum.Parse<ProcessDefectSeverity>(reader.GetString(10)), Quantity = reader.GetInt32(11), Description = reader.GetString(12)
        });
        return result;
    }
    public int AddExperience(ProcessExperienceRecord experience)
    {
        ArgumentNullException.ThrowIfNull(experience);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO ProcessExperience (PackageId, DefectType, PreviousStrategy, NewStrategy, BeforeParameters, AfterParameters, Result, Confidence, CreatedDate) VALUES ($packageId, $defectType, $previousStrategy, $newStrategy, $beforeParameters, $afterParameters, $result, $confidence, $createdDate); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$packageId", experience.PackageId); command.Parameters.AddWithValue("$defectType", experience.DefectType.ToString()); command.Parameters.AddWithValue("$previousStrategy", experience.PreviousStrategy);
        command.Parameters.AddWithValue("$newStrategy", experience.NewStrategy); command.Parameters.AddWithValue("$beforeParameters", experience.BeforeParameters); command.Parameters.AddWithValue("$afterParameters", experience.AfterParameters);
        command.Parameters.AddWithValue("$result", experience.Result.ToString()); command.Parameters.AddWithValue("$confidence", experience.Confidence); command.Parameters.AddWithValue("$createdDate", experience.CreatedDate.ToString("O"));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<ProcessExperienceRecord> GetExperience(int packageId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, DefectType, PreviousStrategy, NewStrategy, BeforeParameters, AfterParameters, Result, Confidence, CreatedDate FROM ProcessExperience WHERE PackageId = $packageId ORDER BY CreatedDate DESC, Id DESC;";
        command.Parameters.AddWithValue("$packageId", packageId);
        return ReadExperience(command);
    }

    public ProcessExperienceRecord? GetBestExperience(int packageId, ProcessDefectType defectType)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, DefectType, PreviousStrategy, NewStrategy, BeforeParameters, AfterParameters, Result, Confidence, CreatedDate FROM ProcessExperience WHERE PackageId = $packageId AND DefectType = $defectType AND Result = 'Improved' ORDER BY Confidence DESC, CreatedDate DESC, Id DESC LIMIT 1;";
        command.Parameters.AddWithValue("$packageId", packageId); command.Parameters.AddWithValue("$defectType", defectType.ToString());
        return ReadExperience(command).SingleOrDefault();
    }

    private SqliteConnection Open()
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_databasePath));
        Directory.CreateDirectory(directory!);
        var connection = new SqliteConnection($"Data Source={_databasePath};Pooling=False");
        connection.Open();
        return connection;
    }

    private void EnsureSchema()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ProcessDefects (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, ProjectId INTEGER NOT NULL, RevisionId INTEGER NOT NULL, Date TEXT NOT NULL,
                ComponentRef TEXT NOT NULL, PackageId INTEGER NOT NULL, DefectDefinitionId INTEGER NULL, ReflowProfileId INTEGER NULL, ProductionLotId INTEGER NULL, DefectType TEXT NOT NULL, Severity TEXT NOT NULL, Quantity INTEGER NOT NULL, Description TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS ProcessExperience (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, PackageId INTEGER NOT NULL, DefectType TEXT NOT NULL, PreviousStrategy TEXT NOT NULL,
                NewStrategy TEXT NOT NULL, BeforeParameters TEXT NOT NULL, AfterParameters TEXT NOT NULL, Result TEXT NOT NULL, Confidence REAL NOT NULL, CreatedDate TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_ProcessDefects_PackageId ON ProcessDefects(PackageId);
            CREATE INDEX IF NOT EXISTS IX_ProcessExperience_PackageDefect ON ProcessExperience(PackageId, DefectType);
            """;
        command.ExecuteNonQuery();
        using var columns = connection.CreateCommand();
        columns.CommandText = "PRAGMA table_info(ProcessDefects);";
        using var reader = columns.ExecuteReader();
        var hasDefinitionId = false; var hasReflowProfileId = false; var hasProductionLotId = false;
        while (reader.Read())
        {
            hasDefinitionId |= reader.GetString(1).Equals("DefectDefinitionId", StringComparison.OrdinalIgnoreCase);
            hasReflowProfileId |= reader.GetString(1).Equals("ReflowProfileId", StringComparison.OrdinalIgnoreCase);
            hasProductionLotId |= reader.GetString(1).Equals("ProductionLotId", StringComparison.OrdinalIgnoreCase);
        }
        if (!hasDefinitionId)
        {
            using var alter = connection.CreateCommand(); alter.CommandText = "ALTER TABLE ProcessDefects ADD COLUMN DefectDefinitionId INTEGER NULL;"; alter.ExecuteNonQuery();
        }
        if (!hasReflowProfileId)
        {
            using var alter = connection.CreateCommand(); alter.CommandText = "ALTER TABLE ProcessDefects ADD COLUMN ReflowProfileId INTEGER NULL;"; alter.ExecuteNonQuery();
        }
    }

    private static List<ProcessExperienceRecord> ReadExperience(SqliteCommand command)
    {
        var result = new List<ProcessExperienceRecord>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new ProcessExperienceRecord
        {
            Id = reader.GetInt32(0), PackageId = reader.GetInt32(1), DefectType = Enum.Parse<ProcessDefectType>(reader.GetString(2)), PreviousStrategy = reader.GetString(3),
            NewStrategy = reader.GetString(4), BeforeParameters = reader.GetString(5), AfterParameters = reader.GetString(6), Result = Enum.Parse<ProcessExperienceResult>(reader.GetString(7)),
            Confidence = reader.GetDouble(8), CreatedDate = DateTime.Parse(reader.GetString(9))
        });
        return result;
    }
}