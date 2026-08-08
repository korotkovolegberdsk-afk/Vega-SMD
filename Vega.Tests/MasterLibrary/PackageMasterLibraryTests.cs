using Vega.Data.MasterLibrary.Repository;
using Vega.Gerber.Models;
using Vega.Models.MasterLibrary;
using Vega.Services.MasterLibrary;
using Xunit;

namespace Vega.Tests.MasterLibrary;

public class PackageMasterLibraryTests : IDisposable
{
    private readonly PackageDefinitionMasterLibraryTestDatabase _database = new();
    private readonly PackageDefinitionRepository _packages = new();
    private readonly PackageDocumentRepository _documents = new();
    private readonly StencilTechnologyRuleService _rules = new();

    [Fact]
    public void R0603_PackageDocumentAndStencilRule_AreAvailable()
    {
        var package = _database.CreatePackage("R0603");
        package.StandardName = "IEC 0603";
        package.PackageFamily = "CHIP";
        package.ComponentType = "Resistor";
        package.Manufacturer = "Vega";
        package.ManufacturerPartNumber = "R0603-TEST";
        package.DrawingFile = "Packages/CHIP/R0603/Drawing/R0603.pdf";
        package.Model3DFile = "Packages/CHIP/R0603/3D/R0603.step";
        _packages.Add(package);
        var stored = _packages.GetAll().Single(item => item.PackageName == package.PackageName);
        var documentId = _documents.Add(new PackageDocument
        {
            PackageId = stored.Id, DocumentType = PackageDocumentType.Drawing, FileName = "R0603.pdf",
            FilePath = stored.DrawingFile, Description = "Mechanical drawing"
        });

        var documents = _documents.GetByPackageId(stored.Id);
        var rule = _rules.GetRule(new PackageDefinition { PackageName = "R0603" }, ApertureStrategy.StandardPasteRelease);

        Assert.Equal("IEC 0603", stored.StandardName);
        Assert.Equal("CHIP", stored.PackageFamily);
        Assert.Equal("R0603.pdf", Assert.Single(documents).FileName);
        Assert.True(documentId > 0);
        Assert.NotNull(rule);
        Assert.Equal("Rectangle", rule!.PreferredShape);
        Assert.Equal(10, rule.PreferredReductionX, 6);
        Assert.Equal(0.10, rule.StencilThicknessMin, 6);
    }

    [Fact]
    public void Qfn_ThermalRule_UsesWindowPane()
    {
        var rule = _rules.GetRule(new PackageDefinition { PackageName = "QFN" }, ApertureStrategy.VoidReduction);

        Assert.NotNull(rule);
        Assert.Equal("WindowPane", rule!.PreferredShape);
        Assert.InRange(rule.Coverage, 50, 70);
    }

    [Fact]
    public void Qfp_FinePitchRule_UsesHomePlate()
    {
        var rule = _rules.GetRule(new PackageDefinition { PackageName = "QFP" }, ApertureStrategy.FinePitch);

        Assert.NotNull(rule);
        Assert.Equal("HomePlate", rule!.PreferredShape);
    }

    public void Dispose() => _database.Dispose();
}