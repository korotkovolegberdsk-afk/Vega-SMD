using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class ComponentCadModelRepository
{
    public ComponentCadModel? GetByComponentId(int componentDefinitionId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height, SourceSystem, SourceUrl, VerificationStatus, Notes FROM ComponentCadModel WHERE ComponentDefinitionId=$id;";
        command.Parameters.AddWithValue("$id", componentDefinitionId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private static ComponentCadModel Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), ComponentDefinitionId = r.GetInt32(1), ModelPath = r.GetString(2), FileSha256 = r.GetString(3),
        Length = r.GetDouble(4), Width = r.GetDouble(5), Height = r.GetDouble(6), SourceSystem = r.GetString(7),
        SourceUrl = r.GetString(8), VerificationStatus = r.GetString(9), Notes = r.GetString(10)
    };
}
