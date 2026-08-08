using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class TechnologySourceRepository
{
    public List<TechnologySource> GetAll()
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, SourceType, DocumentName, DocumentRevision, Reference, Description FROM MasterLibrary_TechnologySources ORDER BY Name;";
        return Read(command);
    }

    public TechnologySource? GetById(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, SourceType, DocumentName, DocumentRevision, Reference, Description FROM MasterLibrary_TechnologySources WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return Read(command).SingleOrDefault();
    }

    public int Add(TechnologySource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO MasterLibrary_TechnologySources (Name, SourceType, DocumentName, DocumentRevision, Reference, Description) VALUES ($name, $sourceType, $documentName, $documentRevision, $reference, $description); SELECT last_insert_rowid();";
        AddParameters(command, source);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(TechnologySource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE MasterLibrary_TechnologySources SET Name = $name, SourceType = $sourceType, DocumentName = $documentName, DocumentRevision = $documentRevision, Reference = $reference, Description = $description WHERE Id = $id;";
        AddParameters(command, source);
        command.Parameters.AddWithValue("$id", source.Id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM MasterLibrary_TechnologySources WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static List<TechnologySource> Read(SqliteCommand command)
    {
        var result = new List<TechnologySource>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new TechnologySource
        {
            Id = reader.GetInt32(0), Name = reader.GetString(1),
            SourceType = Enum.Parse<TechnologySourceType>(reader.GetString(2), true),
            DocumentName = reader.GetString(3), DocumentRevision = reader.GetString(4),
            Reference = reader.GetString(5), Description = reader.GetString(6)
        });
        return result;
    }

    private static void AddParameters(SqliteCommand command, TechnologySource source)
    {
        command.Parameters.AddWithValue("$name", source.Name);
        command.Parameters.AddWithValue("$sourceType", source.SourceType.ToString());
        command.Parameters.AddWithValue("$documentName", source.DocumentName);
        command.Parameters.AddWithValue("$documentRevision", source.DocumentRevision);
        command.Parameters.AddWithValue("$reference", source.Reference);
        command.Parameters.AddWithValue("$description", source.Description);
    }
}