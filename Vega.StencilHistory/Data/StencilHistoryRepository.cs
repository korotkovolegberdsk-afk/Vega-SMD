using Microsoft.Data.Sqlite;
using System.Text.Json;
using Vega.StencilHistory.Models;
using Vega.StencilWorkflow.Models;

namespace Vega.StencilHistory.Data;

public class StencilHistoryRepository
{
    private readonly string _databasePath;

    public StencilHistoryRepository(string? databasePath = null)
    {
        _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "StencilHistory.db");
        EnsureSchema();
    }

    public int CreateProject(StencilProjectRecord project)
    {
        ArgumentNullException.ThrowIfNull(project);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO StencilProjects (ProjectName, CustomerName, BoardName, CreatedDate, ModifiedDate, Status, InputSource, SourceFiles, FrameName, PasteSide, Operator, Notes) " +
            "VALUES ($projectName, $customerName, $boardName, $createdDate, $modifiedDate, $status, $inputSource, $sourceFiles, $frameName, $pasteSide, $operator, $notes); SELECT last_insert_rowid();";
        AddProjectParameters(command, project);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<StencilProjectRecord> GetProjects()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + ProjectColumns + " FROM StencilProjects ORDER BY ModifiedDate DESC, Id DESC;";
        return ReadProjects(command);
    }

    public StencilProjectRecord? GetProject(int projectId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + ProjectColumns + " FROM StencilProjects WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", projectId);
        return ReadProjects(command).SingleOrDefault();
    }

    public int CreateRevision(StencilRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "INSERT INTO StencilRevisions (ProjectId, Revision, CreatedDate, Description, OriginalPasteFile, CorrectedPasteFile, MarkingFile, ReportFile, ChangesCount, WarningsCount, FrameName) " +
            "VALUES ($projectId, $revision, $createdDate, $description, $originalPasteFile, $correctedPasteFile, $markingFile, $reportFile, $changesCount, $warningsCount, $frameName); SELECT last_insert_rowid();";
        AddRevisionParameters(command, revision);
        var id = Convert.ToInt32(command.ExecuteScalar());
        using var update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = "UPDATE StencilProjects SET ModifiedDate = $modifiedDate, Status = $status WHERE Id = $projectId;";
        update.Parameters.AddWithValue("$modifiedDate", DateTime.UtcNow.ToString("O"));
        update.Parameters.AddWithValue("$status", (int)StencilWorkflowStatus.Generated);
        update.Parameters.AddWithValue("$projectId", revision.ProjectId);
        update.ExecuteNonQuery();
        transaction.Commit();
        return id;
    }

    public List<StencilRevision> GetRevisions(int projectId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + RevisionColumns + " FROM StencilRevisions WHERE ProjectId = $projectId ORDER BY CreatedDate, Id;";
        command.Parameters.AddWithValue("$projectId", projectId);
        return ReadRevisions(command);
    }

    public int AddChange(StencilChangeRecord change)
    {
        ArgumentNullException.ThrowIfNull(change);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO StencilChanges (RevisionId, RefDes, ChangeType, BeforeValue, AfterValue, Reason) " +
            "VALUES ($revisionId, $refDes, $changeType, $beforeValue, $afterValue, $reason); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$revisionId", change.RevisionId); command.Parameters.AddWithValue("$refDes", change.RefDes);
        command.Parameters.AddWithValue("$changeType", (int)change.ChangeType); command.Parameters.AddWithValue("$beforeValue", change.Before);
        command.Parameters.AddWithValue("$afterValue", change.After); command.Parameters.AddWithValue("$reason", change.Reason);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<StencilChangeRecord> GetChanges(int revisionId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, RevisionId, RefDes, ChangeType, BeforeValue, AfterValue, Reason FROM StencilChanges WHERE RevisionId = $revisionId ORDER BY Id;";
        command.Parameters.AddWithValue("$revisionId", revisionId);
        using var reader = command.ExecuteReader();
        var result = new List<StencilChangeRecord>();
        while (reader.Read()) result.Add(new StencilChangeRecord
        {
            Id = reader.GetInt32(0), RevisionId = reader.GetInt32(1), RefDes = reader.GetString(2), ChangeType = (StencilChangeType)reader.GetInt32(3),
            Before = reader.GetString(4), After = reader.GetString(5), Reason = reader.GetString(6)
        });
        return result;
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
        command.CommandText = Schema;
        command.ExecuteNonQuery();
    }

    private static List<StencilProjectRecord> ReadProjects(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        var records = new List<StencilProjectRecord>();
        while (reader.Read()) records.Add(new StencilProjectRecord
        {
            Id = reader.GetInt32(0), ProjectName = reader.GetString(1), CustomerName = reader.GetString(2), BoardName = reader.GetString(3),
            CreatedDate = DateTime.Parse(reader.GetString(4)), ModifiedDate = DateTime.Parse(reader.GetString(5)), Status = (StencilWorkflowStatus)reader.GetInt32(6),
            InputSource = reader.GetString(7), SourceFiles = JsonSerializer.Deserialize<List<string>>(reader.GetString(8)) ?? [], FrameName = reader.GetString(9),
            PasteSide = reader.GetString(10), Operator = reader.GetString(11), Notes = reader.GetString(12)
        });
        return records;
    }

    private static List<StencilRevision> ReadRevisions(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        var records = new List<StencilRevision>();
        while (reader.Read()) records.Add(new StencilRevision
        {
            Id = reader.GetInt32(0), ProjectId = reader.GetInt32(1), Revision = reader.GetString(2), CreatedDate = DateTime.Parse(reader.GetString(3)),
            Description = reader.GetString(4), OriginalPasteFile = reader.GetString(5), CorrectedPasteFile = reader.GetString(6),
            MarkingFile = reader.GetString(7), ReportFile = reader.GetString(8), ChangesCount = reader.GetInt32(9), WarningsCount = reader.GetInt32(10), FrameName = reader.GetString(11)
        });
        return records;
    }

    private static void AddProjectParameters(SqliteCommand command, StencilProjectRecord project)
    {
        command.Parameters.AddWithValue("$projectName", project.ProjectName); command.Parameters.AddWithValue("$customerName", project.CustomerName);
        command.Parameters.AddWithValue("$boardName", project.BoardName); command.Parameters.AddWithValue("$createdDate", project.CreatedDate.ToString("O"));
        command.Parameters.AddWithValue("$modifiedDate", project.ModifiedDate.ToString("O")); command.Parameters.AddWithValue("$status", (int)project.Status);
        command.Parameters.AddWithValue("$inputSource", project.InputSource); command.Parameters.AddWithValue("$sourceFiles", JsonSerializer.Serialize(project.SourceFiles));
        command.Parameters.AddWithValue("$frameName", project.FrameName); command.Parameters.AddWithValue("$pasteSide", project.PasteSide);
        command.Parameters.AddWithValue("$operator", project.Operator); command.Parameters.AddWithValue("$notes", project.Notes);
    }

    private static void AddRevisionParameters(SqliteCommand command, StencilRevision revision)
    {
        command.Parameters.AddWithValue("$projectId", revision.ProjectId); command.Parameters.AddWithValue("$revision", revision.Revision);
        command.Parameters.AddWithValue("$createdDate", revision.CreatedDate.ToString("O")); command.Parameters.AddWithValue("$description", revision.Description);
        command.Parameters.AddWithValue("$originalPasteFile", revision.OriginalPasteFile); command.Parameters.AddWithValue("$correctedPasteFile", revision.CorrectedPasteFile);
        command.Parameters.AddWithValue("$markingFile", revision.MarkingFile); command.Parameters.AddWithValue("$reportFile", revision.ReportFile);
        command.Parameters.AddWithValue("$changesCount", revision.ChangesCount); command.Parameters.AddWithValue("$warningsCount", revision.WarningsCount); command.Parameters.AddWithValue("$frameName", revision.FrameName);
    }

    private const string ProjectColumns = "Id, ProjectName, CustomerName, BoardName, CreatedDate, ModifiedDate, Status, InputSource, SourceFiles, FrameName, PasteSide, Operator, Notes";
    private const string RevisionColumns = "Id, ProjectId, Revision, CreatedDate, Description, OriginalPasteFile, CorrectedPasteFile, MarkingFile, ReportFile, ChangesCount, WarningsCount, FrameName";
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS StencilProjects (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, ProjectName TEXT NOT NULL, CustomerName TEXT NOT NULL, BoardName TEXT NOT NULL,
            CreatedDate TEXT NOT NULL, ModifiedDate TEXT NOT NULL, Status INTEGER NOT NULL, InputSource TEXT NOT NULL,
            SourceFiles TEXT NOT NULL, FrameName TEXT NOT NULL, PasteSide TEXT NOT NULL, Operator TEXT NOT NULL, Notes TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS StencilRevisions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, ProjectId INTEGER NOT NULL, Revision TEXT NOT NULL, CreatedDate TEXT NOT NULL,
            Description TEXT NOT NULL, OriginalPasteFile TEXT NOT NULL, CorrectedPasteFile TEXT NOT NULL, MarkingFile TEXT NOT NULL,
            ReportFile TEXT NOT NULL, ChangesCount INTEGER NOT NULL, WarningsCount INTEGER NOT NULL, FrameName TEXT NOT NULL,
            FOREIGN KEY(ProjectId) REFERENCES StencilProjects(Id));
        CREATE TABLE IF NOT EXISTS StencilChanges (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, RevisionId INTEGER NOT NULL, RefDes TEXT NOT NULL, ChangeType INTEGER NOT NULL,
            BeforeValue TEXT NOT NULL, AfterValue TEXT NOT NULL, Reason TEXT NOT NULL,
            FOREIGN KEY(RevisionId) REFERENCES StencilRevisions(Id));
        CREATE INDEX IF NOT EXISTS IX_StencilProjects_ProjectName ON StencilProjects(ProjectName);
        CREATE INDEX IF NOT EXISTS IX_StencilRevisions_Revision ON StencilRevisions(Revision);
        CREATE INDEX IF NOT EXISTS IX_StencilRevisions_CreatedDate ON StencilRevisions(CreatedDate);
        """;
}