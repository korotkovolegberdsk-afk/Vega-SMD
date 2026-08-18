namespace Vega.Models.MasterLibrary;

/// <summary>
/// Packaging geometry for a specific manufacturer part number.
/// One <see cref="ComponentDefinition"/> can have multiple tape-and-reel variants.
/// </summary>
public class ComponentTapeReelGeometry
{
    public int Id { get; set; }

    public int ComponentDefinitionId { get; set; }

    public string PackagingCode { get; set; } = "";
    public string SourceReference { get; set; } = "";
    public string SourceRevision { get; set; } = "";

    public string TapeStandard { get; set; } = "";

    public double CarrierTapeWidth { get; set; }

    public double PocketPitch { get; set; }
    public double PocketLength { get; set; }
    public double PocketWidth { get; set; }
    public double PocketDepth { get; set; }
    public double PocketOffsetX { get; set; }
    public double PocketOffsetY { get; set; }

    public double SprocketHolePitch { get; set; }
    public double SprocketHoleDiameter { get; set; }
    public double SprocketHoleOffset { get; set; }

    public double CoverTapeWidth { get; set; }

    public TapeFeedDirection FeedDirection { get; set; } = TapeFeedDirection.Unknown;
    public TapePocketOrientation PocketOrientation { get; set; } = TapePocketOrientation.Deg0;
    public string Pin1Orientation { get; set; } = "";
    public double PickupRotation { get; set; }

    public double ReelDiameter { get; set; }
    public double HubDiameter { get; set; }
    public int QuantityPerReel { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public TapeVerificationStatus VerificationStatus { get; set; } = TapeVerificationStatus.Unknown;

    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public int Version { get; set; }
    public string ChangeComment { get; set; } = "";
}
