using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Data.MasterLibrary.Repository;
using Vega.Models.MasterLibrary;

namespace Vega.PackageRecognition.Data;

public class PackageRecognitionRuleRepository
{
    private readonly PackageDefinitionRepository _packageRepository = new();

    public List<PackageRecognitionRule> GetAll()
    {
        using var connection = MasterLibraryConnection.Create();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Pattern, PackageId, Priority, MatchType FROM MasterLibrary_PackageRecognitionRules ORDER BY Priority DESC, Id;";
        using var reader = command.ExecuteReader();
        var rules = new List<PackageRecognitionRule>();
        while (reader.Read()) rules.Add(new PackageRecognitionRule
        {
            Id = reader.GetInt32(0), Pattern = reader.GetString(1), PackageId = reader.GetInt32(2), Priority = reader.GetInt32(3),
            MatchType = Enum.Parse<PackageRecognitionMatchType>(reader.GetString(4), true)
        });
        return rules;
    }

    public PackageDefinition? GetPackage(int packageId) => _packageRepository.GetById(packageId);
}