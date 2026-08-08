using Microsoft.Data.Sqlite;
using Vega.StencilCAM.Models;

namespace Vega.StencilCAM.Data;

public class StencilFrameRepository
{
    private static readonly string DatabasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StencilFrameLibrary.db");

    public List<StencilFrame> GetAll()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + Columns + " FROM StencilFrame ORDER BY SortOrder, Name;";
        return ReadFrames(command);
    }

    public List<StencilFrame> GetActiveFrames()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + Columns + " FROM StencilFrame WHERE IsActive = 1 ORDER BY SortOrder, Name;";
        return ReadFrames(command);
    }

    public StencilFrame? GetDefaultFrame()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + Columns + " FROM StencilFrame WHERE IsDefault = 1 AND IsActive = 1 LIMIT 1;";
        return ReadFrames(command).SingleOrDefault();
    }

    public StencilFrame? GetById(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT " + Columns + " FROM StencilFrame WHERE Id = $id LIMIT 1;";
        command.Parameters.AddWithValue("$id", id);
        return ReadFrames(command).SingleOrDefault();
    }

    public int Add(StencilFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        if (frame.IsDefault) ClearDefault(connection, transaction);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        INSERT INTO StencilFrame
        (Name, PrinterModel, FrameWidth, FrameHeight, StencilWidth, StencilHeight, OriginX, OriginY,
         GerberTemplateFile, IsDefault, IsActive, SortOrder, Notes)
        VALUES ($name, $printerModel, $frameWidth, $frameHeight, $stencilWidth, $stencilHeight, $originX, $originY,
                $gerberTemplateFile, $isDefault, $isActive, $sortOrder, $notes);
        SELECT last_insert_rowid();
        """;
        AddParameters(command, frame);
        var id = Convert.ToInt32(command.ExecuteScalar());
        transaction.Commit();
        return id;
    }

    public void Update(StencilFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        if (frame.IsDefault) ClearDefault(connection, transaction, frame.Id);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
        """
        UPDATE StencilFrame SET Name=$name, PrinterModel=$printerModel, FrameWidth=$frameWidth, FrameHeight=$frameHeight,
            StencilWidth=$stencilWidth, StencilHeight=$stencilHeight, OriginX=$originX, OriginY=$originY,
            GerberTemplateFile=$gerberTemplateFile, IsDefault=$isDefault, IsActive=$isActive,
            SortOrder=$sortOrder, Notes=$notes WHERE Id=$id;
        """;
        AddParameters(command, frame);
        command.Parameters.AddWithValue("$id", frame.Id);
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public void SetDefault(int id)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        ClearDefault(connection, transaction);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE StencilFrame SET IsDefault = 1 WHERE Id = $id AND IsActive = 1;";
        command.Parameters.AddWithValue("$id", id);
        if (command.ExecuteNonQuery() != 1) throw new ArgumentException("Active stencil frame was not found.", nameof(id));
        transaction.Commit();
    }

    public void SaveProjectFrame(StencilProjectFrame projectFrame)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO StencilProjectFrame (ProjectId, FrameId, FrameName, AssignedDate)
        VALUES ($projectId, $frameId, $frameName, $assignedDate)
        ON CONFLICT(ProjectId) DO UPDATE SET FrameId=excluded.FrameId, FrameName=excluded.FrameName, AssignedDate=excluded.AssignedDate;
        """;
        command.Parameters.AddWithValue("$projectId", projectFrame.ProjectId); command.Parameters.AddWithValue("$frameId", projectFrame.FrameId);
        command.Parameters.AddWithValue("$frameName", projectFrame.FrameName); command.Parameters.AddWithValue("$assignedDate", projectFrame.AssignedDate.ToString("O"));
        command.ExecuteNonQuery();
    }

    public StencilProjectFrame? GetProjectFrame(int projectId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProjectId, FrameId, FrameName, AssignedDate FROM StencilProjectFrame WHERE ProjectId=$projectId;";
        command.Parameters.AddWithValue("$projectId", projectId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? new StencilProjectFrame
        {
            ProjectId = reader.GetInt32(0), FrameId = reader.GetInt32(1), FrameName = reader.GetString(2), AssignedDate = DateTime.Parse(reader.GetString(3))
        } : null;
    }

    private static SqliteConnection Open()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "008_StencilFrameLibrary.sql"));
        command.ExecuteNonQuery();
        return connection;
    }

    private static void ClearDefault(SqliteConnection connection, SqliteTransaction transaction, int excludedId = 0)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "UPDATE StencilFrame SET IsDefault = 0 WHERE IsDefault = 1 AND Id <> $id;";
        command.Parameters.AddWithValue("$id", excludedId);
        command.ExecuteNonQuery();
    }

    private static List<StencilFrame> ReadFrames(SqliteCommand command)
    {
        var frames = new List<StencilFrame>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) frames.Add(new StencilFrame
        {
            Id=reader.GetInt32(0), Name=reader.GetString(1), PrinterModel=reader.GetString(2), FrameWidth=reader.GetDouble(3), FrameHeight=reader.GetDouble(4),
            StencilWidth=reader.GetDouble(5), StencilHeight=reader.GetDouble(6), OriginX=reader.GetDouble(7), OriginY=reader.GetDouble(8),
            GerberTemplateFile=reader.GetString(9), IsDefault=reader.GetInt32(10) != 0, IsActive=reader.GetInt32(11) != 0, SortOrder=reader.GetInt32(12), Notes=reader.GetString(13)
        });
        return frames;
    }

    private static void AddParameters(SqliteCommand command, StencilFrame frame)
    {
        command.Parameters.AddWithValue("$name", frame.Name); command.Parameters.AddWithValue("$printerModel", frame.PrinterModel);
        command.Parameters.AddWithValue("$frameWidth", frame.FrameWidth); command.Parameters.AddWithValue("$frameHeight", frame.FrameHeight);
        command.Parameters.AddWithValue("$stencilWidth", frame.StencilWidth); command.Parameters.AddWithValue("$stencilHeight", frame.StencilHeight);
        command.Parameters.AddWithValue("$originX", frame.OriginX); command.Parameters.AddWithValue("$originY", frame.OriginY);
        command.Parameters.AddWithValue("$gerberTemplateFile", frame.GerberTemplateFile); command.Parameters.AddWithValue("$isDefault", frame.IsDefault ? 1 : 0);
        command.Parameters.AddWithValue("$isActive", frame.IsActive ? 1 : 0); command.Parameters.AddWithValue("$sortOrder", frame.SortOrder); command.Parameters.AddWithValue("$notes", frame.Notes);
    }

    private const string Columns = "Id, Name, PrinterModel, FrameWidth, FrameHeight, StencilWidth, StencilHeight, OriginX, OriginY, GerberTemplateFile, IsDefault, IsActive, SortOrder, Notes";
}
