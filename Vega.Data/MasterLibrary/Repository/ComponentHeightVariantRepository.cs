using Microsoft.Data.Sqlite;
using Vega.Models.MasterLibrary;
using Vega.Data.MasterLibrary.Database;

namespace Vega.Data.MasterLibrary.Repository;

public sealed class ComponentHeightVariantRepository
{
    public IReadOnlyList<ComponentHeightVariant> GetCurrent(int componentDefinitionId)
    {
        using var c = MasterLibraryConnection.Create();
        using var q = c.CreateCommand();
        q.CommandText = "SELECT Id,ComponentDefinitionId,VariantName,ConditionText,Height,SourceDocument,SourceReference,VerificationStatus,IsCurrent,Notes,CreatedAt,UpdatedAt FROM ComponentHeightVariant WHERE ComponentDefinitionId=$id AND IsCurrent=1 ORDER BY Height,Id";
        q.Parameters.AddWithValue("$id", componentDefinitionId);
        using var r = q.ExecuteReader();
        var result = new List<ComponentHeightVariant>();
        while (r.Read()) result.Add(Map(r));
        return result;
    }

    public ComponentHeightVariant Add(ComponentHeightVariant value)
    {
        if (value.Height <= 0) throw new ArgumentOutOfRangeException(nameof(value.Height));
        if (string.IsNullOrWhiteSpace(value.SourceDocument) || string.IsNullOrWhiteSpace(value.SourceReference))
            throw new ArgumentException("Источник высоты должен быть указан.", nameof(value));
        if (string.Equals(value.VerificationStatus, "Unverified", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Неподтвержденную высоту нельзя сохранять как вариант компонента.", nameof(value));
        using var c = MasterLibraryConnection.Create();
        using var q = c.CreateCommand();
        q.CommandText = "INSERT INTO ComponentHeightVariant(ComponentDefinitionId,VariantName,ConditionText,Height,SourceDocument,SourceReference,VerificationStatus,IsCurrent,Notes,CreatedAt,UpdatedAt) VALUES($component,$name,$condition,$height,$document,$reference,$status,$current,$notes,datetime('now'),datetime('now')); SELECT last_insert_rowid();";
        q.Parameters.AddWithValue("$component", value.ComponentDefinitionId);
        q.Parameters.AddWithValue("$name", value.VariantName);
        q.Parameters.AddWithValue("$condition", value.ConditionText);
        q.Parameters.AddWithValue("$height", value.Height);
        q.Parameters.AddWithValue("$document", value.SourceDocument);
        q.Parameters.AddWithValue("$reference", value.SourceReference);
        q.Parameters.AddWithValue("$status", value.VerificationStatus);
        q.Parameters.AddWithValue("$current", value.IsCurrent);
        q.Parameters.AddWithValue("$notes", value.Notes);
        value.Id = Convert.ToInt32(q.ExecuteScalar());
        return value;
    }

    private static ComponentHeightVariant Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), ComponentDefinitionId = r.GetInt32(1), VariantName = r.GetString(2), ConditionText = r.GetString(3),
        Height = r.GetDouble(4), SourceDocument = r.GetString(5), SourceReference = r.GetString(6), VerificationStatus = r.GetString(7),
        IsCurrent = r.GetBoolean(8), Notes = r.GetString(9), CreatedAt = DateTime.Parse(r.GetString(10)), UpdatedAt = DateTime.Parse(r.GetString(11))
    };
}
