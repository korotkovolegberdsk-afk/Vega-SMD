using Vega.ProcessLearning.Data;
using Vega.ProcessLearning.Models;
using Vega.ProductionTracking;
using Vega.ProductionTracking.Data;
using Vega.ProductionTracking.Models;
using Vega.Report;
using Vega.Report.Models;
using Xunit;

namespace Vega.Tests;

public class ProductionTrackingTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "VegaProduction", Guid.NewGuid().ToString());
    private readonly ProcessLearningRepository _process;
    private readonly ProductionTrackingRepository _repository;

    public ProductionTrackingTests()
    {
        _process = new ProcessLearningRepository(Path.Combine(_directory, "ProcessLearning.db"));
        _repository = new ProductionTrackingRepository(Path.Combine(_directory, "ProductionTracking.db"), _process);
    }

    [Fact]
    public void CreateLot_SavesProductionLot()
    {
        var id = CreateLot();
        var lot = _repository.GetLot(id);

        Assert.NotNull(lot);
        Assert.Equal("ORD-2026-001", lot!.OrderNumber);
        Assert.Equal(ProductionLotStatus.Running, lot.Status);
    }

    [Fact]
    public void Lot_CanReferenceStencilRevision()
    {
        var id = _repository.CreateLot(new ProductionLot { OrderNumber = "ORD-1", StencilRevisionId = 17 });

        Assert.Equal(17, _repository.GetLot(id)!.StencilRevisionId);
    }

    [Fact]
    public void Lot_CanReferenceReflowProfile()
    {
        var id = _repository.CreateLot(new ProductionLot { OrderNumber = "ORD-2", ReflowProfileId = 9 });

        Assert.Equal(9, _repository.GetLot(id)!.ReflowProfileId);
    }

    [Fact]
    public void AddPasteBatch_AssociatesPasteWithLot()
    {
        var lotId = CreateLot();
        _repository.AddPasteBatch(new SolderPasteBatch { LotId = lotId, Manufacturer = "Indium", PasteName = "8.9HF", Alloy = "SAC305", BatchNumber = "A123456" });

        var batch = Assert.Single(_repository.GetPasteBatches(lotId));
        Assert.Equal("A123456", batch.BatchNumber);
        Assert.Equal("SAC305", batch.Alloy);
    }

    [Fact]
    public void AddEquipment_AssociatesEquipmentWithLot()
    {
        var lotId = CreateLot();
        var equipmentId = _repository.AddEquipment(new ProductionEquipment { EquipmentType = ProductionEquipmentType.Printer, Manufacturer = "ASM", Model = "DEK Neo", SerialNumber = "PR-01" });
        _repository.AddLotEquipment(new ProductionLotEquipment { LotId = lotId, EquipmentId = equipmentId, Operation = ProductionOperation.Printing });

        var history = _repository.GetLotHistory(lotId);
        var equipment = Assert.Single(history.Equipment);
        Assert.Equal(ProductionEquipmentType.Printer, equipment.EquipmentType);
        Assert.Equal("DEK Neo", equipment.Model);
    }

    [Fact]
    public void ProcessDefect_CanReferenceProductionLot()
    {
        var lotId = CreateLot();
        _process.AddDefect(new ProcessDefectRecord { PackageId = 42, ComponentRef = "R1", ProductionLotId = lotId, DefectType = ProcessDefectType.SolderBall, Severity = ProcessDefectSeverity.Medium, Quantity = 3 });

        var defect = Assert.Single(_process.GetDefectsByProductionLot(lotId));
        Assert.Equal(lotId, defect.ProductionLotId);
    }

    [Fact]
    public void LotHistory_MapsToTechnicalReport()
    {
        var lotId = CreateLot();
        _repository.AddPasteBatch(new SolderPasteBatch { LotId = lotId, Manufacturer = "Indium", PasteName = "8.9HF", Alloy = "SAC305", BatchNumber = "A123456" });
        _process.AddDefect(new ProcessDefectRecord { PackageId = 42, ComponentRef = "U5", ProductionLotId = lotId, DefectType = ProcessDefectType.Void, Severity = ProcessDefectSeverity.High, Quantity = 1 });
        var report = new StencilTechnicalReport { ProductionLot = ProductionLotReportMapper.ToReportItem(_repository.GetLotHistory(lotId)) };
        var output = Path.Combine(_directory, "lot-report.txt");

        new StencilReportGeneratorService().GenerateTXT(report, output);
        var text = File.ReadAllText(output);

        Assert.Contains("PRODUCTION LOT", text);
        Assert.Contains("ORD-2026-001", text);
        Assert.Contains("A123456", text);
        Assert.Contains("Void", text);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    private int CreateLot() => _repository.CreateLot(new ProductionLot
    {
        ProjectId = 12, OrderNumber = "ORD-2026-001", Customer = "ABC", BoardName = "Controller", BoardRevision = "Rev A",
        StencilRevisionId = 7, ReflowProfileId = 9, ProcessLearningProjectId = 12, LineName = "SMT-1", Operator = "Test", Status = ProductionLotStatus.Running
    });
}