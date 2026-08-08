using Microsoft.Data.Sqlite;
using Vega.Data.MasterLibrary.Database;
using Vega.Models;
using Vega.Models.MasterLibrary;
using Vega.Services;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class StencilProjectAnalyzerServiceTests : IDisposable
{
    private static readonly string[] PackageNames = ["R0603", "QFN-32", "QFP-0.5"];
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionService _packageService = new();

    public StencilProjectAnalyzerServiceTests()
    {
        AddPackage("R0603", 0.8, 0, 0.9, 0.45);
        AddPackage("QFN-32", 0.5, 1, 2.0, 2.0);
        AddPackage("QFP-0.5", 0.5, 0, 0.8, 0.3);
    }

    [Fact]
    public void Analyze_Project_ReturnsEngineeringResultsAndExpectedStatistics()
    {
        var project = new StencilProject
        {
            ProjectName = "Test Project",
            TopPasteFile = TestData("test-project.gtp"),
            PnpFile = TestData("test-project.csv")
        };

        var result = new StencilProjectAnalyzerService().Analyze(project);

        Assert.Equal(3, result.TotalComponents);
        Assert.Equal(3, result.AnalyzedComponents);
        Assert.Equal(2, result.OkCount);
        Assert.Equal(1, result.WarningCount);
        Assert.Equal(0, result.CriticalCount);

        var r0603 = Assert.Single(result.ComponentResults, item => item.RefDes == "R1");
        Assert.Equal(StencilAnalysisStatus.OK, r0603.Status);

        var qfn = Assert.Single(result.ComponentResults, item => item.RefDes == "U1");
        Assert.Equal(StencilAnalysisStatus.Warning, qfn.Status);
        Assert.Equal("WindowPane", qfn.TechnologyRule!.PreferredShape);
        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.Array, qfn.RecommendedShape);

        var qfp = Assert.Single(result.ComponentResults, item => item.RefDes == "U2");
        Assert.Equal(StencilAnalysisStatus.OK, qfp.Status);
        Assert.Equal(Vega.Gerber.Models.ApertureShapeType.HomePlate, qfp.RecommendedShape);
    }

    public void Dispose()
    {
        using var connection = MasterLibraryConnection.Create();
        using (var command = connection.CreateCommand())
        {
            command.CommandText =
            """
            DELETE FROM PackageFootprint
            WHERE PatternName IN ('R0603', 'QFN-32', 'QFP-0.5');
            DELETE FROM ComponentDefinition
            WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5'));
            DELETE FROM EquipmentAlias
            WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5'));
            DELETE FROM PackageProcessProfile
            WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5'));
            DELETE FROM PackageGeometry
            WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5'));
            DELETE FROM MasterLibrary_PackageDocuments
            WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5'));
            DELETE FROM MasterLibrary_PackageRecognitionRules
            WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5'));
            DELETE FROM PackageDefinition WHERE PackageName IN ('QFN-32', 'QFP-0.5');
            """;
            command.ExecuteNonQuery();
        }
        _database.Dispose();
    }

    private void AddPackage(string packageName, double pitch, int thermalPads, double padLength, double padWidth)
    {
        var saved = _packageService.GetAll().SingleOrDefault(item => item.PackageName == packageName);
        if (saved is null)
        {
            var package = _database.CreatePackage("PROJECT");
            package.PackageName = packageName;
            package.DisplayName = packageName;
            package.Pitch = pitch;
            package.PadCount = 1;
            package.ThermalPadCount = thermalPads;
            _packageService.Add(package);
            saved = _packageService.GetAll().Single(item => item.PackageName == packageName);
        }

        _packageService.AddFootprint(new PackageFootprint
        {
            PackageId = saved.Id,
            PatternName = packageName,
            PadCount = 1,
            PadLength = padLength,
            PadWidth = padWidth,
            PadPitch = pitch
        });
    }

    private static string TestData(string fileName) => Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
}

