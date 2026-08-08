using Vega.DefectDictionary;
using Vega.DefectDictionary.Data;
using Vega.DefectDictionary.Models;
using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;
using Vega.Report;
using Vega.Report.Models;
using Xunit;

namespace Vega.Tests;

public class DefectDictionaryTests : IDisposable
{
    private readonly string _dictionaryPath = Path.Combine(Path.GetTempPath(), "VegaDefectDictionary", Guid.NewGuid() + ".db");
    private readonly string _processPath = Path.Combine(Path.GetTempPath(), "VegaDefectDictionary", Guid.NewGuid() + ".db");
    private readonly string _reportPath = Path.Combine(Path.GetTempPath(), "VegaDefectDictionary", Guid.NewGuid() + ".txt");
    private readonly DefectDictionaryRepository _dictionary;

    public DefectDictionaryTests() => _dictionary = new DefectDictionaryRepository(_dictionaryPath);

    [Fact]
    public void Search_FindsSolderBall()
    {
        var definitions = _dictionary.Search("solder ball");

        var defect = Assert.Single(definitions);
        Assert.Equal("SolderBall", defect.Code);
        Assert.Equal("Solder Ball", defect.EnglishName);
    }

    [Fact]
    public void GetByCode_ReturnsRussianTranslation()
    {
        var defect = _dictionary.GetByCode("Tombstone");

        Assert.NotNull(defect);
        Assert.Equal("Эффект \"надгробного камня\"", defect!.RussianName);
        Assert.Equal(DefectCategory.Reflow, defect.Category);
    }

    [Fact]
    public void ProcessLearningRecord_PreservesDictionaryLinkAndLegacyType()
    {
        var definition = _dictionary.GetByCode("SolderBall")!;
        var repository = new ProcessLearningRepository(_processPath);
        repository.AddDefect(new ProcessDefectRecord
        {
            PackageId = 42, DefectDefinitionId = definition.Id, DefectType = ProcessDefectType.SolderBall,
            Severity = ProcessDefectSeverity.Medium, Quantity = 1
        });

        var defect = Assert.Single(repository.GetDefectsByPackage(42));
        Assert.Equal(definition.Id, defect.DefectDefinitionId);
        Assert.Equal(ProcessDefectType.SolderBall, defect.DefectType);
    }

    [Fact]
    public void TechnicalReport_WritesEnglishAndRussianDefectNames()
    {
        var definition = _dictionary.GetByCode("SolderBall")!;
        var report = new StencilTechnicalReport { Defects = [DefectDictionaryReportMapper.ToReportItem(definition)] };

        new StencilReportGeneratorService().GenerateTXT(report, _reportPath);
        var text = File.ReadAllText(_reportPath);

        Assert.Contains("Solder Ball", text);
        Assert.Contains("Шарики припоя", text);
        Assert.Contains("Excess paste volume", text);
    }

    public void Dispose()
    {
        foreach (var path in new[] { _dictionaryPath, _processPath, _reportPath })
            if (File.Exists(path)) File.Delete(path);
    }
}