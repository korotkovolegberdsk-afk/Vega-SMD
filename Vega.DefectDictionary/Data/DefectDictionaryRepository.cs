using Microsoft.Data.Sqlite;
using Vega.DefectDictionary.Models;

namespace Vega.DefectDictionary.Data;

public class DefectDictionaryRepository
{
    private readonly string _databasePath;

    public DefectDictionaryRepository(string? databasePath = null)
    {
        _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "DefectDictionary.db");
        EnsureSchema();
    }

    public List<DefectDefinition> GetAll()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, EnglishName, RussianName, Category, DescriptionEN, DescriptionRU, Severity, TypicalCause, TypicalSolution FROM DefectDefinitions ORDER BY Category, EnglishName;";
        return Read(command);
    }

    public DefectDefinition? GetByCode(string code)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, EnglishName, RussianName, Category, DescriptionEN, DescriptionRU, Severity, TypicalCause, TypicalSolution FROM DefectDefinitions WHERE Code = $code COLLATE NOCASE;";
        command.Parameters.AddWithValue("$code", code ?? "");
        return Read(command).SingleOrDefault();
    }

    public List<DefectDefinition> GetByCategory(DefectCategory category)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, EnglishName, RussianName, Category, DescriptionEN, DescriptionRU, Severity, TypicalCause, TypicalSolution FROM DefectDefinitions WHERE Category = $category ORDER BY EnglishName;";
        command.Parameters.AddWithValue("$category", category.ToString());
        return Read(command);
    }

    public List<DefectDefinition> Search(string query)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code, EnglishName, RussianName, Category, DescriptionEN, DescriptionRU, Severity, TypicalCause, TypicalSolution FROM DefectDefinitions WHERE Code LIKE $query OR EnglishName LIKE $query OR RussianName LIKE $query OR DescriptionEN LIKE $query OR DescriptionRU LIKE $query ORDER BY EnglishName;";
        command.Parameters.AddWithValue("$query", "%" + (query ?? "") + "%");
        return Read(command);
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
        var migration = Path.Combine(AppContext.BaseDirectory, "Migration", "013_DefectDictionary.sql");
        command.CommandText = File.Exists(migration) ? File.ReadAllText(migration) : Schema;
        command.ExecuteNonQuery();
    }

    private static List<DefectDefinition> Read(SqliteCommand command)
    {
        var result = new List<DefectDefinition>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new DefectDefinition
        {
            Id = reader.GetInt32(0), Code = reader.GetString(1), EnglishName = reader.GetString(2), RussianName = reader.GetString(3),
            Category = Enum.Parse<DefectCategory>(reader.GetString(4)), DescriptionEN = reader.GetString(5), DescriptionRU = reader.GetString(6),
            Severity = Enum.Parse<DefectSeverity>(reader.GetString(7)), TypicalCause = reader.GetString(8), TypicalSolution = reader.GetString(9)
        });
        return result;
    }

    private const string Schema = """
        CREATE TABLE IF NOT EXISTS DefectDefinitions (Id INTEGER PRIMARY KEY AUTOINCREMENT, Code TEXT NOT NULL UNIQUE, EnglishName TEXT NOT NULL, RussianName TEXT NOT NULL, Category TEXT NOT NULL, DescriptionEN TEXT NOT NULL DEFAULT '', DescriptionRU TEXT NOT NULL DEFAULT '', Severity TEXT NOT NULL, TypicalCause TEXT NOT NULL DEFAULT '', TypicalSolution TEXT NOT NULL DEFAULT '');
        """;
}