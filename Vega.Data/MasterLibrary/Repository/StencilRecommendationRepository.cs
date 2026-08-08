using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models.MasterLibrary;

namespace Vega.Data.MasterLibrary.Repository;

public class StencilRecommendationRepository
{
    public List<StencilRecommendationRule> GetRulesByPackageFamily(string packageFamily)
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT Id, PackageFamily, ComponentType, RecommendedStencilThickness,
               ApertureShape, ReductionX, ReductionY, ThermalPadRule,
               AreaRatioMinimum, AspectRatioMinimum, RuleSource, Notes
        FROM StencilRecommendationRule
        WHERE PackageFamily = $packageFamily COLLATE NOCASE
        ORDER BY ComponentType;
        """;
        command.Parameters.AddWithValue("$packageFamily", packageFamily);

        return ReadRules(command);
    }

    public StencilRecommendationRule? GetRuleForPackage(
        int packageId,
        string componentType = "")
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT r.Id, r.PackageFamily, r.ComponentType,
               r.RecommendedStencilThickness, r.ApertureShape,
               r.ReductionX, r.ReductionY, r.ThermalPadRule,
               r.AreaRatioMinimum, r.AspectRatioMinimum,
               r.RuleSource, r.Notes
        FROM PackageDefinition p
        INNER JOIN PackageFamily f
            ON f.Id = p.FamilyId
        INNER JOIN StencilRecommendationRule r
            ON r.PackageFamily = f.Code COLLATE NOCASE
            OR r.PackageFamily = f.Name COLLATE NOCASE
        WHERE p.Id = $packageId
          AND (r.ComponentType = $componentType COLLATE NOCASE
               OR r.ComponentType = '')
        ORDER BY CASE
            WHEN r.ComponentType = $componentType COLLATE NOCASE THEN 0
            ELSE 1
        END
        LIMIT 1;
        """;
        command.Parameters.AddWithValue("$packageId", packageId);
        command.Parameters.AddWithValue("$componentType", componentType);

        return ReadRules(command).SingleOrDefault();
    }

    private static List<StencilRecommendationRule> ReadRules(SqliteCommand command)
    {
        var rules = new List<StencilRecommendationRule>();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            rules.Add(new StencilRecommendationRule
            {
                Id = reader.GetInt32(0),
                PackageFamily = reader.GetString(1),
                ComponentType = reader.GetString(2),
                RecommendedStencilThickness = reader.GetDouble(3),
                ApertureShape = Enum.Parse<ApertureShape>(reader.GetString(4)),
                ReductionX = reader.GetDouble(5),
                ReductionY = reader.GetDouble(6),
                ThermalPadRule = reader.GetString(7),
                AreaRatioMinimum = reader.GetDouble(8),
                AspectRatioMinimum = reader.GetDouble(9),
                RuleSource = reader.GetString(10),
                Notes = reader.GetString(11)
            });
        }

        return rules;
    }
}
