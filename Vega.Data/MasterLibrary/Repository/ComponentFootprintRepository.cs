using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class ComponentFootprintRepository
{
    public ComponentFootprint? GetByComponentId(int componentDefinitionId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch, PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes FROM ComponentFootprint WHERE ComponentDefinitionId=$id;";
        command.Parameters.AddWithValue("$id", componentDefinitionId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    private static ComponentFootprint Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), ComponentDefinitionId = r.GetInt32(1), PatternName = r.GetString(2),
        PadCount = r.GetInt32(3), PadLength = r.GetDouble(4), PadWidth = r.GetDouble(5), PadPitch = r.GetDouble(6),
        PasteLayer = r.GetString(7), SourceSystem = r.GetString(8), SourceUrl = r.GetString(9),
        SourceVariant = r.GetString(10), VerificationStatus = r.GetString(11), Notes = r.GetString(12)
    };
}
