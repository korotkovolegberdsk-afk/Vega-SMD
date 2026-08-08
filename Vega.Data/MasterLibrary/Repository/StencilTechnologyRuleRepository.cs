using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class StencilTechnologyRuleRepository
{
    public List<StencilTechnologyRule> GetAll()
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection);
        command.CommandText += " ORDER BY Priority DESC, PackageFamily, PackageName;";
        return ReadRules(command);
    }

    public StencilTechnologyRule? GetById(int id)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection);
        command.CommandText += " WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return ReadRules(command).SingleOrDefault();
    }

    public List<StencilTechnologyRule> GetByPackage(string packageName)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection);
        command.CommandText +=
        """
         WHERE IsActive = 1
           AND (PackageName = '' OR $packageName LIKE '%' || PackageName || '%'
                OR $packageName LIKE '%' || PackageFamily || '%')
         ORDER BY CASE WHEN PackageName <> '' AND $packageName LIKE '%' || PackageName || '%' THEN 0 ELSE 1 END,
                  Priority DESC;
        """;
        command.Parameters.AddWithValue("$packageName", packageName ?? "");
        return ReadRules(command);
    }

    public List<StencilTechnologyRule> GetByTechnologyGoal(string technologyGoal)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = CreateSelectCommand(connection);
        command.CommandText += " WHERE IsActive = 1 AND TechnologyGoal = $technologyGoal COLLATE NOCASE ORDER BY Priority DESC;";
        command.Parameters.AddWithValue("$technologyGoal", technologyGoal ?? "");
        return ReadRules(command);
    }

    public int Add(StencilTechnologyRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        INSERT INTO StencilTechnologyRule
        (PackageFamily, PackageName, ComponentType, TechnologyGoal, PreferredShape, AlternativeShape,
         RecommendedThickness, StencilThicknessMin, StencilThicknessMax, ReductionX, ReductionY, PreferredReductionX, PreferredReductionY, MinAreaRatio, MinAspectRatio, Coverage,
         Source, Manufacturer, DocumentReference, SourceReference, TechnologySourceId, ConfidenceLevel, ApplicationCondition, RecommendedBy, ProcessGoal, TechnologyReason, Notes, Priority, IsActive)
        VALUES
        ($packageFamily, $packageName, $componentType, $technologyGoal, $preferredShape, $alternativeShape,
         $recommendedThickness, $stencilThicknessMin, $stencilThicknessMax, $reductionX, $reductionY, $preferredReductionX, $preferredReductionY, $minAreaRatio, $minAspectRatio, $coverage,
         $source, $manufacturer, $documentReference, $sourceReference, $technologySourceId, $confidenceLevel, $applicationCondition, $recommendedBy, $processGoal, $technologyReason, $notes, $priority, $isActive);
        SELECT last_insert_rowid();
        """;
        AddParameters(command, rule);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(StencilTechnologyRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        UPDATE StencilTechnologyRule SET
            PackageFamily = $packageFamily, PackageName = $packageName, ComponentType = $componentType, TechnologyGoal = $technologyGoal,
            PreferredShape = $preferredShape, AlternativeShape = $alternativeShape, RecommendedThickness = $recommendedThickness,
            StencilThicknessMin = $stencilThicknessMin, StencilThicknessMax = $stencilThicknessMax, ReductionX = $reductionX, ReductionY = $reductionY,
            PreferredReductionX = $preferredReductionX, PreferredReductionY = $preferredReductionY, MinAreaRatio = $minAreaRatio, MinAspectRatio = $minAspectRatio, Coverage = $coverage,
            Source = $source, Manufacturer = $manufacturer, DocumentReference = $documentReference, SourceReference = $sourceReference,
            TechnologySourceId = $technologySourceId, ConfidenceLevel = $confidenceLevel, ApplicationCondition = $applicationCondition,
            RecommendedBy = $recommendedBy, ProcessGoal = $processGoal, TechnologyReason = $technologyReason, Notes = $notes,
            Priority = $priority, IsActive = $isActive
        WHERE Id = $id;
        """;
        AddParameters(command, rule);
        command.Parameters.AddWithValue("$id", rule.Id);
        command.ExecuteNonQuery();
    }

    public void SetActive(int id, bool active)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE StencilTechnologyRule SET IsActive = $active WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$active", active ? 1 : 0);
        command.ExecuteNonQuery();
    }

    private static SqliteCommand CreateSelectCommand(SqliteConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT Id, PackageFamily, PackageName, ComponentType, TechnologyGoal, PreferredShape, AlternativeShape,
               RecommendedThickness, StencilThicknessMin, StencilThicknessMax, ReductionX, ReductionY, PreferredReductionX, PreferredReductionY,
               MinAreaRatio, MinAspectRatio, Coverage, Source, Manufacturer, DocumentReference, SourceReference,
               TechnologySourceId, ConfidenceLevel, ApplicationCondition, RecommendedBy, ProcessGoal, TechnologyReason, Notes, Priority, IsActive
        FROM StencilTechnologyRule
        """;
        return command;
    }

    private static List<StencilTechnologyRule> ReadRules(SqliteCommand command)
    {
        var rules = new List<StencilTechnologyRule>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) rules.Add(new StencilTechnologyRule
        {
            Id = reader.GetInt32(0), PackageFamily = reader.GetString(1), PackageName = reader.GetString(2), ComponentType = reader.GetString(3),
            TechnologyGoal = reader.GetString(4), PreferredShape = reader.GetString(5), AlternativeShape = reader.GetString(6),
            RecommendedThickness = reader.GetDouble(7), StencilThicknessMin = reader.GetDouble(8), StencilThicknessMax = reader.GetDouble(9),
            ReductionX = reader.GetDouble(10), ReductionY = reader.GetDouble(11), PreferredReductionX = reader.GetDouble(12), PreferredReductionY = reader.GetDouble(13),
            MinAreaRatio = reader.GetDouble(14), MinAspectRatio = reader.GetDouble(15), Coverage = reader.GetDouble(16), Source = reader.GetString(17),
            Manufacturer = reader.GetString(18), DocumentReference = reader.GetString(19), SourceReference = reader.GetString(20),
            TechnologySourceId = reader.IsDBNull(21) ? null : reader.GetInt32(21), ConfidenceLevel = reader.GetDouble(22),
            ApplicationCondition = reader.GetString(23), RecommendedBy = reader.GetString(24), ProcessGoal = reader.GetString(25),
            TechnologyReason = reader.GetString(26), Notes = reader.GetString(27), Priority = reader.GetInt32(28), IsActive = reader.GetInt32(29) != 0
        });
        return rules;
    }

    private static void AddParameters(SqliteCommand command, StencilTechnologyRule rule)
    {
        command.Parameters.AddWithValue("$packageFamily", rule.PackageFamily); command.Parameters.AddWithValue("$packageName", rule.PackageName);
        command.Parameters.AddWithValue("$componentType", rule.ComponentType); command.Parameters.AddWithValue("$technologyGoal", rule.TechnologyGoal);
        command.Parameters.AddWithValue("$preferredShape", rule.PreferredShape); command.Parameters.AddWithValue("$alternativeShape", rule.AlternativeShape);
        command.Parameters.AddWithValue("$recommendedThickness", rule.RecommendedThickness); command.Parameters.AddWithValue("$stencilThicknessMin", rule.StencilThicknessMin); command.Parameters.AddWithValue("$stencilThicknessMax", rule.StencilThicknessMax);
        command.Parameters.AddWithValue("$reductionX", rule.ReductionX); command.Parameters.AddWithValue("$reductionY", rule.ReductionY); command.Parameters.AddWithValue("$preferredReductionX", rule.PreferredReductionX); command.Parameters.AddWithValue("$preferredReductionY", rule.PreferredReductionY);
        command.Parameters.AddWithValue("$minAreaRatio", rule.MinAreaRatio); command.Parameters.AddWithValue("$minAspectRatio", rule.MinAspectRatio); command.Parameters.AddWithValue("$coverage", rule.Coverage);
        command.Parameters.AddWithValue("$source", rule.Source); command.Parameters.AddWithValue("$manufacturer", rule.Manufacturer); command.Parameters.AddWithValue("$documentReference", rule.DocumentReference); command.Parameters.AddWithValue("$sourceReference", rule.SourceReference);
        command.Parameters.AddWithValue("$technologySourceId", rule.TechnologySourceId is null ? DBNull.Value : rule.TechnologySourceId.Value); command.Parameters.AddWithValue("$confidenceLevel", rule.ConfidenceLevel);
        command.Parameters.AddWithValue("$applicationCondition", rule.ApplicationCondition); command.Parameters.AddWithValue("$recommendedBy", rule.RecommendedBy); command.Parameters.AddWithValue("$processGoal", rule.ProcessGoal);
        command.Parameters.AddWithValue("$technologyReason", rule.TechnologyReason); command.Parameters.AddWithValue("$notes", rule.Notes); command.Parameters.AddWithValue("$priority", rule.Priority); command.Parameters.AddWithValue("$isActive", rule.IsActive ? 1 : 0);
    }
}