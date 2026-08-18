CREATE TABLE IF NOT EXISTS ComponentTapeReelGeometry
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    ComponentDefinitionId INTEGER NOT NULL,

    PackagingCode TEXT DEFAULT '',
    SourceReference TEXT DEFAULT '',
    SourceRevision TEXT DEFAULT '',

    TapeStandard TEXT DEFAULT '',

    CarrierTapeWidth REAL NOT NULL DEFAULT 0,

    PocketPitch REAL NOT NULL DEFAULT 0,
    PocketLength REAL NOT NULL DEFAULT 0,
    PocketWidth REAL NOT NULL DEFAULT 0,
    PocketDepth REAL NOT NULL DEFAULT 0,
    PocketOffsetX REAL NOT NULL DEFAULT 0,
    PocketOffsetY REAL NOT NULL DEFAULT 0,

    SprocketHolePitch REAL NOT NULL DEFAULT 0,
    SprocketHoleDiameter REAL NOT NULL DEFAULT 0,
    SprocketHoleOffset REAL NOT NULL DEFAULT 0,

    CoverTapeWidth REAL NOT NULL DEFAULT 0,

    FeedDirection INTEGER NOT NULL DEFAULT 0,
    PocketOrientation INTEGER NOT NULL DEFAULT 0,

    Pin1Orientation TEXT DEFAULT '',
    PickupRotation REAL NOT NULL DEFAULT 0,

    ReelDiameter REAL NOT NULL DEFAULT 0,
    HubDiameter REAL NOT NULL DEFAULT 0,
    QuantityPerReel INTEGER NOT NULL DEFAULT 0,

    IsDefault INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,

    VerificationStatus INTEGER NOT NULL DEFAULT 0,

    Notes TEXT DEFAULT '',

    CreatedAt TEXT,
    CreatedBy TEXT DEFAULT '',
    UpdatedAt TEXT,
    UpdatedBy TEXT DEFAULT '',

    Version INTEGER NOT NULL DEFAULT 1,
    ChangeComment TEXT DEFAULT '',

    FOREIGN KEY(ComponentDefinitionId)
        REFERENCES ComponentDefinition(Id)
);

CREATE INDEX IF NOT EXISTS IX_ComponentTapeReelGeometry_ComponentDefinitionId
    ON ComponentTapeReelGeometry(ComponentDefinitionId);

CREATE INDEX IF NOT EXISTS IX_ComponentTapeReelGeometry_PackagingCode
    ON ComponentTapeReelGeometry(PackagingCode);

CREATE UNIQUE INDEX IF NOT EXISTS IX_ComponentTapeReelGeometry_DefaultPerComponent
    ON ComponentTapeReelGeometry(ComponentDefinitionId)
    WHERE IsDefault = 1;
