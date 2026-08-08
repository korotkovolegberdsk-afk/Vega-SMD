using Microsoft.Data.Sqlite;
using Vega.ReflowProfile.Models;
using ReflowProfileModel = Vega.ReflowProfile.Models.ReflowProfile;

namespace Vega.ReflowProfile.Data;

public class ReflowProfileRepository
{
    private readonly string _databasePath;

    public ReflowProfileRepository(string? databasePath = null)
    {
        _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "ReflowProfile.db");
        EnsureSchema();
    }

    public int CreateProfile(ReflowProfileModel profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO ReflowProfiles (Name, EquipmentName, OvenModel, SolderPaste, PasteAlloy, ProfileType, CreatedDate, Operator, Notes) VALUES ($name, $equipmentName, $ovenModel, $solderPaste, $pasteAlloy, $profileType, $createdDate, $operator, $notes); SELECT last_insert_rowid();";
        AddProfileParameters(command, profile);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int AddPoint(ReflowProfilePoint point)
    {
        ArgumentNullException.ThrowIfNull(point);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO ReflowProfilePoints (ProfileId, TimeSeconds, TemperatureC, SensorChannel) VALUES ($profileId, $timeSeconds, $temperatureC, $sensorChannel); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$profileId", point.ProfileId); command.Parameters.AddWithValue("$timeSeconds", point.TimeSeconds);
        command.Parameters.AddWithValue("$temperatureC", point.TemperatureC); command.Parameters.AddWithValue("$sensorChannel", point.SensorChannel);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public ReflowProfileModel? GetProfile(int profileId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, EquipmentName, OvenModel, SolderPaste, PasteAlloy, ProfileType, CreatedDate, Operator, Notes FROM ReflowProfiles WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", profileId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadProfile(reader) : null;
    }

    public List<ReflowProfilePoint> GetPoints(int profileId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProfileId, TimeSeconds, TemperatureC, SensorChannel FROM ReflowProfilePoints WHERE ProfileId = $profileId ORDER BY TimeSeconds, Id;";
        command.Parameters.AddWithValue("$profileId", profileId);
        var result = new List<ReflowProfilePoint>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new ReflowProfilePoint { Id = reader.GetInt32(0), ProfileId = reader.GetInt32(1), TimeSeconds = reader.GetDouble(2), TemperatureC = reader.GetDouble(3), SensorChannel = reader.GetString(4) });
        return result;
    }

    public void SaveAnalysis(ReflowProfileAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(analysis);
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO ReflowProfileAnalysis (ProfileId, RampRate, SoakStart, SoakEnd, SoakTime, LiquidusTemperature, TimeAboveLiquidus, PeakTemperature, CoolingRate, Status) VALUES ($profileId, $rampRate, $soakStart, $soakEnd, $soakTime, $liquidusTemperature, $timeAboveLiquidus, $peakTemperature, $coolingRate, $status) ON CONFLICT(ProfileId) DO UPDATE SET RampRate = excluded.RampRate, SoakStart = excluded.SoakStart, SoakEnd = excluded.SoakEnd, SoakTime = excluded.SoakTime, LiquidusTemperature = excluded.LiquidusTemperature, TimeAboveLiquidus = excluded.TimeAboveLiquidus, PeakTemperature = excluded.PeakTemperature, CoolingRate = excluded.CoolingRate, Status = excluded.Status;";
        command.Parameters.AddWithValue("$profileId", analysis.ProfileId); command.Parameters.AddWithValue("$rampRate", analysis.RampRate); command.Parameters.AddWithValue("$soakStart", analysis.SoakStart); command.Parameters.AddWithValue("$soakEnd", analysis.SoakEnd); command.Parameters.AddWithValue("$soakTime", analysis.SoakTime); command.Parameters.AddWithValue("$liquidusTemperature", analysis.LiquidusTemperature); command.Parameters.AddWithValue("$timeAboveLiquidus", analysis.TimeAboveLiquidus); command.Parameters.AddWithValue("$peakTemperature", analysis.PeakTemperature); command.Parameters.AddWithValue("$coolingRate", analysis.CoolingRate); command.Parameters.AddWithValue("$status", analysis.Status.ToString());
        command.ExecuteNonQuery();
    }

    public ReflowProfileAnalysis? GetAnalysis(int profileId)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProfileId, RampRate, SoakStart, SoakEnd, SoakTime, LiquidusTemperature, TimeAboveLiquidus, PeakTemperature, CoolingRate, Status FROM ReflowProfileAnalysis WHERE ProfileId = $profileId;";
        command.Parameters.AddWithValue("$profileId", profileId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? new ReflowProfileAnalysis { ProfileId = reader.GetInt32(0), RampRate = reader.GetDouble(1), SoakStart = reader.GetDouble(2), SoakEnd = reader.GetDouble(3), SoakTime = reader.GetDouble(4), LiquidusTemperature = reader.GetDouble(5), TimeAboveLiquidus = reader.GetDouble(6), PeakTemperature = reader.GetDouble(7), CoolingRate = reader.GetDouble(8), Status = Enum.Parse<ReflowProfileStatus>(reader.GetString(9)) } : null;
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
            CREATE TABLE IF NOT EXISTS ReflowProfiles (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, EquipmentName TEXT NOT NULL, OvenModel TEXT NOT NULL, SolderPaste TEXT NOT NULL, PasteAlloy TEXT NOT NULL, ProfileType TEXT NOT NULL, CreatedDate TEXT NOT NULL, Operator TEXT NOT NULL, Notes TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS ReflowProfilePoints (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProfileId INTEGER NOT NULL, TimeSeconds REAL NOT NULL, TemperatureC REAL NOT NULL, SensorChannel TEXT NOT NULL, FOREIGN KEY(ProfileId) REFERENCES ReflowProfiles(Id));
            CREATE TABLE IF NOT EXISTS ReflowProfileAnalysis (ProfileId INTEGER PRIMARY KEY, RampRate REAL NOT NULL, SoakStart REAL NOT NULL, SoakEnd REAL NOT NULL, SoakTime REAL NOT NULL, LiquidusTemperature REAL NOT NULL, TimeAboveLiquidus REAL NOT NULL, PeakTemperature REAL NOT NULL, CoolingRate REAL NOT NULL, Status TEXT NOT NULL, FOREIGN KEY(ProfileId) REFERENCES ReflowProfiles(Id));
            CREATE INDEX IF NOT EXISTS IX_ReflowProfilePoints_ProfileId ON ReflowProfilePoints(ProfileId);
            CREATE INDEX IF NOT EXISTS IX_ReflowProfiles_CreatedDate ON ReflowProfiles(CreatedDate);
            """;
        command.ExecuteNonQuery();
    }

    private static void AddProfileParameters(SqliteCommand command, ReflowProfileModel profile)
    {
        command.Parameters.AddWithValue("$name", profile.Name); command.Parameters.AddWithValue("$equipmentName", profile.EquipmentName); command.Parameters.AddWithValue("$ovenModel", profile.OvenModel); command.Parameters.AddWithValue("$solderPaste", profile.SolderPaste); command.Parameters.AddWithValue("$pasteAlloy", profile.PasteAlloy); command.Parameters.AddWithValue("$profileType", profile.ProfileType.ToString()); command.Parameters.AddWithValue("$createdDate", profile.CreatedDate.ToString("O")); command.Parameters.AddWithValue("$operator", profile.Operator); command.Parameters.AddWithValue("$notes", profile.Notes);
    }

    private static ReflowProfileModel ReadProfile(SqliteDataReader reader) => new() { Id = reader.GetInt32(0), Name = reader.GetString(1), EquipmentName = reader.GetString(2), OvenModel = reader.GetString(3), SolderPaste = reader.GetString(4), PasteAlloy = reader.GetString(5), ProfileType = Enum.Parse<ReflowProfileType>(reader.GetString(6)), CreatedDate = DateTime.Parse(reader.GetString(7)), Operator = reader.GetString(8), Notes = reader.GetString(9) };
}