using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class TechnologyRecommendationRepository
{
    public List<TechnologyRecommendation> GetByPackage(int packageId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority FROM MasterLibrary_TechnologyRecommendations WHERE PackageId = $packageId ORDER BY Priority DESC, Id;";
        command.Parameters.AddWithValue("$packageId", packageId);
        return Read(command);
    }

    public List<TechnologyRecommendation> GetByGoal(string technologyGoal)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority FROM MasterLibrary_TechnologyRecommendations WHERE TechnologyGoal = $technologyGoal COLLATE NOCASE ORDER BY Priority DESC, Id;";
        command.Parameters.AddWithValue("$technologyGoal", technologyGoal ?? "");
        return Read(command);
    }

    public List<TechnologyRecommendation> GetBySource(int sourceId)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority FROM MasterLibrary_TechnologyRecommendations WHERE SourceId = $sourceId ORDER BY Priority DESC, Id;";
        command.Parameters.AddWithValue("$sourceId", sourceId);
        return Read(command);
    }

    public int Add(TechnologyRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO MasterLibrary_TechnologyRecommendations (PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority) VALUES ($packageId, $ruleId, $sourceId, $technologyGoal, $recommendationText, $parameterJson, $priority); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$packageId", recommendation.PackageId);
        command.Parameters.AddWithValue("$ruleId", recommendation.RuleId is null ? DBNull.Value : recommendation.RuleId.Value);
        command.Parameters.AddWithValue("$sourceId", recommendation.SourceId);
        command.Parameters.AddWithValue("$technologyGoal", recommendation.TechnologyGoal);
        command.Parameters.AddWithValue("$recommendationText", recommendation.RecommendationText);
        command.Parameters.AddWithValue("$parameterJson", recommendation.ParameterJson);
        command.Parameters.AddWithValue("$priority", recommendation.Priority);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static List<TechnologyRecommendation> Read(SqliteCommand command)
    {
        var result = new List<TechnologyRecommendation>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(new TechnologyRecommendation
        {
            Id = reader.GetInt32(0), PackageId = reader.GetInt32(1),
            RuleId = reader.IsDBNull(2) ? null : reader.GetInt32(2), SourceId = reader.GetInt32(3),
            TechnologyGoal = reader.GetString(4), RecommendationText = reader.GetString(5),
            ParameterJson = reader.GetString(6), Priority = reader.GetInt32(7)
        });
        return result;
    }
}