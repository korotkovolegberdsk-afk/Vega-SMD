using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;
using Vega.ReflowProfile;
using Vega.ReflowProfile.Data;
using Vega.ReflowProfile.Models;
using ReflowProfileModel = Vega.ReflowProfile.Models.ReflowProfile;
using Vega.Report;
using Vega.Report.Models;
using Vega.StencilHistory.Data;
using Vega.StencilHistory.Models;
using Xunit;

namespace Vega.Tests;

public class ReflowProfileTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "VegaReflow", Guid.NewGuid().ToString());
    private readonly string _databasePath;
    private readonly ReflowProfileRepository _repository;

    public ReflowProfileTests()
    {
        _databasePath = Path.Combine(_directory, "ReflowProfile.db");
        _repository = new ReflowProfileRepository(_databasePath);
    }

    [Fact]
    public void CreateProfile_SavesMetadataAndPoints()
    {
        var id = CreateProfile();
        _repository.AddPoint(new ReflowProfilePoint { ProfileId = id, TimeSeconds = 0, TemperatureC = 25, SensorChannel = "TC1" });

        var profile = _repository.GetProfile(id);
        var point = Assert.Single(_repository.GetPoints(id));

        Assert.NotNull(profile);
        Assert.Equal("Lead-free production", profile!.Name);
        Assert.Equal(ReflowProfileType.LeadFree, profile.ProfileType);
        Assert.Equal("TC1", point.SensorChannel);
    }

    [Fact]
    public void ImportCsv_ReadsTimeTemperaturePoints()
    {
        var file = Path.Combine(_directory, "profile.csv");
        Directory.CreateDirectory(_directory);
        File.WriteAllText(file, "Time,Temperature\n0,25\n10,50\n20,80\n");

        var points = new ReflowProfileImportService().Import(file, 5);

        Assert.Equal(3, points.Count);
        Assert.Equal(80, points[^1].TemperatureC);
        Assert.Equal(5, points[0].ProfileId);
    }

    [Fact]
    public void Analyze_CalculatesPeakTalAndRampRate()
    {
        var id = CreateProfile();
        var points = SamplePoints(id);
        var analyzer = new ReflowProfileAnalyzerService();
        var analysis = analyzer.Analyze(id, points);

        Assert.Equal(245, analysis.PeakTemperature);
        Assert.InRange(analysis.TimeAboveLiquidus, 65, 75);
        Assert.Equal(2, analysis.RampRate, 3);
        _repository.SaveAnalysis(analysis);
        Assert.NotNull(_repository.GetAnalysis(id));
    }

    [Fact]
    public void CalculateTal_UsesLiquidusCrossings()
    {
        var tal = new ReflowProfileAnalyzerService().CalculateTAL(SamplePoints(1), 217);

        Assert.InRange(tal, 65, 75);
    }

    [Fact]
    public void CalculateRampRate_ReturnsDegreesPerSecond()
    {
        var ramp = new ReflowProfileAnalyzerService().CalculateRampRate(SamplePoints(1));

        Assert.Equal(2, ramp, 3);
    }

    [Fact]
    public void StencilProject_CanPersistReflowProfileLink()
    {
        var profileId = CreateProfile();
        var history = new StencilHistoryRepository(Path.Combine(_directory, "StencilHistory.db"));
        var projectId = history.CreateProject(new StencilProjectRecord { ProjectName = "Controller", ReflowProfileId = profileId });

        var project = history.GetProject(projectId);

        Assert.NotNull(project);
        Assert.Equal(profileId, project!.ReflowProfileId);
    }
    [Fact]
    public void ProcessDefect_CanReferenceReflowProfile()
    {
        var profileId = CreateProfile();
        var processPath = Path.Combine(_directory, "ProcessLearning.db");
        var process = new ProcessLearningRepository(processPath);
        process.AddDefect(new ProcessDefectRecord
        {
            PackageId = 42, ReflowProfileId = profileId, DefectType = ProcessDefectType.Void,
            Severity = ProcessDefectSeverity.High, Quantity = 2
        });

        var defect = Assert.Single(process.GetDefectsByPackage(42));
        Assert.Equal(profileId, defect.ReflowProfileId);
    }

    [Fact]
    public void TechnicalReport_WritesReflowSection()
    {
        var profileId = CreateProfile();
        var profile = _repository.GetProfile(profileId)!;
        var points = SamplePoints(profileId);
        var analysis = new ReflowProfileAnalyzerService().Analyze(profileId, points);
        var report = new StencilTechnicalReport { ReflowProfile = ReflowProfileReportMapper.ToReportItem(profile, analysis, points) };
        var output = Path.Combine(_directory, "report.txt");

        new StencilReportGeneratorService().GenerateTXT(report, output);
        var text = File.ReadAllText(output);

        Assert.Contains("REFLOW PROFILE", text);
        Assert.Contains("Peak: 245", text);
        Assert.Contains("Lead-free production", text);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private int CreateProfile() => _repository.CreateProfile(new ReflowProfileModel
    {
        Name = "Lead-free production", EquipmentName = "Heller", OvenModel = "1809", SolderPaste = "Indium 8.9HF",
        PasteAlloy = "SAC305", ProfileType = ReflowProfileType.LeadFree, Operator = "Test"
    });

    private static List<ReflowProfilePoint> SamplePoints(int profileId) =>
    [
        new() { ProfileId = profileId, TimeSeconds = 0, TemperatureC = 25, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 30, TemperatureC = 85, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 60, TemperatureC = 145, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 90, TemperatureC = 170, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 120, TemperatureC = 220, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 150, TemperatureC = 245, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 180, TemperatureC = 230, SensorChannel = "TC1" },
        new() { ProfileId = profileId, TimeSeconds = 210, TemperatureC = 180, SensorChannel = "TC1" }
    ];
}