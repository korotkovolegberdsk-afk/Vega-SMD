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
        command.CommandText +=
        """
         WHERE IsActive = 1 AND TechnologyGoal = $technologyGoal COLLATE NOCASE
         ORDER BY Priority DESC;
        """;
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
         RecommendedThickness, ReductionX, ReductionY, MinAreaRatio, MinAspectRatio, Coverage,
         Source, Manufacturer, DocumentReference, TechnologyReason, Notes, Priority, IsActive)
        VALUES
        ($packageFamily, $packageName, $componentType, $technologyGoal, $preferredShape, $alternativeShape,
         $recommendedThickness, $reductionX, $reductionY, $minAreaRatio, $minAspectRatio, $coverage,
         $source, $manufacturer, $documentReference, $technologyReason, $notes, $priority, $isActive);
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
            PackageFamily = $packageFamily, PackageName = $packageName, ComponentType = $componentType,
            TechnologyGoal = $technologyGoal, PreferredShape = $preferredShape, AlternativeShape = $alternativeShape,
            RecommendedThickness = $recommendedThickness, ReductionX = $reductionX, ReductionY = $reductionY,
            MinAreaRatio = $minAreaRatio, MinAspectRatio = $minAspectRatio, Coverage = $coverage,
            Source = $source, Manufacturer = $manufacturer, DocumentReference = $documentReference,
            TechnologyReason = $technologyReason, Notes = $notes, Priority = $priority, IsActive = $isActive
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
        SELECT Id, PackageFamily, PackageName, ComponentType, TechnologyGoal,
               PreferredShape, AlternativeShape, RecommendedThickness, ReductionX, ReductionY,
               MinAreaRatio, MinAspectRatio, Coverage, Source, Manufacturer, DocumentReference,
               TechnologyReason, Notes, Priority, IsActive
        FROM StencilTechnologyRule
        """;
        return command;
    }

    private static List<StencilTechnologyRule> ReadRules(SqliteCommand command)
    {
        var rules = new List<StencilTechnologyRule>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rules.Add(new StencilTechnologyRule
            {
                Id = reader.GetInt32(0), PackageFamily = reader.GetString(1), PackageName = reader.GetString(2),
                ComponentType = reader.GetString(3), TechnologyGoal = reader.GetString(4),
                PreferredShape = reader.GetString(5), AlternativeShape = reader.GetString(6),
                RecommendedThickness = reader.GetDouble(7), ReductionX = reader.GetDouble(8), ReductionY = reader.GetDouble(9),
                MinAreaRatio = reader.GetDouble(10), MinAspectRatio = reader.GetDouble(11), Coverage = reader.GetDouble(12),
                Source = reader.GetString(13), Manufacturer = reader.GetString(14), DocumentReference = reader.GetString(15),
                TechnologyReason = reader.GetString(16), Notes = reader.GetString(17), Priority = reader.GetInt32(18),
                IsActive = reader.GetInt32(19) != 0
            });
        }
        return rules;
    }

    private static void AddParameters(SqliteCommand command, StencilTechnologyRule rule)
    {
        command.Parameters.AddWithValue("$packageFamily", rule.PackageFamily); command.Parameters.AddWithValue("$packageName", rule.PackageName);
        command.Parameters.AddWithValue("$componentType", rule.ComponentType); command.Parameters.AddWithValue("$technologyGoal", rule.TechnologyGoal);
        command.Parameters.AddWithValue("$preferredShape", rule.PreferredShape); command.Parameters.AddWithValue("$alternativeShape", rule.AlternativeShape);
        command.Parameters.AddWithValue("$recommendedThickness", rule.RecommendedThickness); command.Parameters.AddWithValue("$reductionX", rule.ReductionX);
        command.Parameters.AddWithValue("$reductionY", rule.ReductionY); command.Parameters.AddWithValue("$minAreaRatio", rule.MinAreaRatio);
        command.Parameters.AddWithValue("$minAspectRatio", rule.MinAspectRatio); command.Parameters.AddWithValue("$coverage", rule.Coverage);
        command.Parameters.AddWithValue("$source", rule.Source); command.Parameters.AddWithValue("$manufacturer", rule.Manufacturer);
        command.Parameters.AddWithValue("$documentReference", rule.DocumentReference); command.Parameters.AddWithValue("$technologyReason", rule.TechnologyReason);
        command.Parameters.AddWithValue("$notes", rule.Notes); command.Parameters.AddWithValue("$priority", rule.Priority);
        command.Parameters.AddWithValue("$isActive", rule.IsActive ? 1 : 0);
    }
}
