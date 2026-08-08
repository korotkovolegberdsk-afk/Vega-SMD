using Vega.ProcessLearning.Models;

namespace Vega.ProductionTracking.Models;

public enum ProductionLotStatus { Planned, Running, Completed, Closed }
public enum ProductionEquipmentType { Printer, PickAndPlace, ReflowOven, AOI, SPI }
public enum ProductionOperation { Printing, Placement, Reflow, Inspection }

public class ProductionLot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string OrderNumber { get; set; } = "";
    public string Customer { get; set; } = "";
    public string BoardName { get; set; } = "";
    public string BoardRevision { get; set; } = "";
    public int? StencilRevisionId { get; set; }
    public int? ReflowProfileId { get; set; }
    public int? ProcessLearningProjectId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public string LineName { get; set; } = "";
    public string Operator { get; set; } = "";
    public ProductionLotStatus Status { get; set; } = ProductionLotStatus.Planned;
}

public class SolderPasteBatch
{
    public int Id { get; set; }
    public int LotId { get; set; }
    public string Manufacturer { get; set; } = "";
    public string PasteName { get; set; } = "";
    public string Alloy { get; set; } = "";
    public string BatchNumber { get; set; } = "";
    public DateTime? ExpirationDate { get; set; }
    public string StorageCondition { get; set; } = "";
    public DateTime? OpenedDate { get; set; }
    public string Notes { get; set; } = "";
}

public class ProductionEquipment
{
    public int Id { get; set; }
    public ProductionEquipmentType EquipmentType { get; set; }
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string Description { get; set; } = "";
}

public class ProductionLotEquipment
{
    public int LotId { get; set; }
    public int EquipmentId { get; set; }
    public ProductionOperation Operation { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

public class ProductionLotReport
{
    public ProductionLot Lot { get; init; } = new();
    public string Stencil { get; init; } = "";
    public string Paste { get; init; } = "";
    public string Reflow { get; init; } = "";
    public IReadOnlyList<ProductionEquipment> Equipment { get; init; } = Array.Empty<ProductionEquipment>();
    public IReadOnlyList<ProcessDefectRecord> Defects { get; init; } = Array.Empty<ProcessDefectRecord>();
    public double Yield { get; init; }
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}