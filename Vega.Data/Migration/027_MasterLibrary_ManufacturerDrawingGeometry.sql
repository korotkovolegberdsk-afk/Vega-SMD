CREATE TABLE IF NOT EXISTS PackageManufacturerDrawingGeometry
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageDefinitionId INTEGER NOT NULL,
    Manufacturer TEXT NOT NULL DEFAULT '',
    PackageSeries TEXT NOT NULL DEFAULT '',
    SourceDocument TEXT NOT NULL DEFAULT '',
    SourceRevision TEXT NOT NULL DEFAULT '',
    SourcePage TEXT NOT NULL DEFAULT '',
    VerificationStatus TEXT NOT NULL DEFAULT 'Unverified',
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
    IsCurrent INTEGER NOT NULL DEFAULT 0 CHECK (IsCurrent IN (0, 1)),
    Notes TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (PackageDefinitionId) REFERENCES PackageDefinition(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PackageProjectionGeometry
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ManufacturerDrawingGeometryId INTEGER NOT NULL,
    ProjectionType TEXT NOT NULL,
    CoordinateSystem TEXT NOT NULL DEFAULT 'mm',
    OriginX REAL NOT NULL DEFAULT 0,
    OriginY REAL NOT NULL DEFAULT 0,
    BodyContour TEXT NOT NULL DEFAULT '',
    LeadContour TEXT NOT NULL DEFAULT '',
    AuxiliaryGeometry TEXT NOT NULL DEFAULT '',
    IsAvailable INTEGER NOT NULL DEFAULT 1 CHECK (IsAvailable IN (0, 1)),
    FOREIGN KEY (ManufacturerDrawingGeometryId) REFERENCES PackageManufacturerDrawingGeometry(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PackageLeadGeometry
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ManufacturerDrawingGeometryId INTEGER NOT NULL,
    LeadNumber INTEGER NOT NULL,
    Side TEXT NOT NULL DEFAULT '',
    ProjectionType TEXT NOT NULL DEFAULT '',
    RootX REAL NOT NULL DEFAULT 0,
    RootY REAL NOT NULL DEFAULT 0,
    ContactX REAL NOT NULL DEFAULT 0,
    ContactY REAL NOT NULL DEFAULT 0,
    ProfileGeometry TEXT NOT NULL DEFAULT '',
    Width REAL NOT NULL DEFAULT 0,
    Thickness REAL NOT NULL DEFAULT 0,
    Pitch REAL NOT NULL DEFAULT 0,
    IsPin1 INTEGER NOT NULL DEFAULT 0 CHECK (IsPin1 IN (0, 1)),
    FOREIGN KEY (ManufacturerDrawingGeometryId) REFERENCES PackageManufacturerDrawingGeometry(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PackageDimensionTolerance
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ManufacturerDrawingGeometryId INTEGER NOT NULL,
    DimensionKey TEXT NOT NULL,
    ProjectionType TEXT NOT NULL DEFAULT '',
    NominalValue REAL,
    MinValue REAL,
    MaxValue REAL,
    Symbol TEXT NOT NULL DEFAULT '',
    Unit TEXT NOT NULL DEFAULT 'mm',
    SourceLabel TEXT NOT NULL DEFAULT '',
    FOREIGN KEY (ManufacturerDrawingGeometryId) REFERENCES PackageManufacturerDrawingGeometry(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS PackagePin1Geometry
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ManufacturerDrawingGeometryId INTEGER NOT NULL,
    ProjectionType TEXT NOT NULL,
    IsPresent INTEGER NOT NULL DEFAULT 0 CHECK (IsPresent IN (0, 1)),
    MarkerType TEXT NOT NULL DEFAULT '',
    PositionX REAL NOT NULL DEFAULT 0,
    PositionY REAL NOT NULL DEFAULT 0,
    Width REAL NOT NULL DEFAULT 0,
    Height REAL NOT NULL DEFAULT 0,
    FOREIGN KEY (ManufacturerDrawingGeometryId) REFERENCES PackageManufacturerDrawingGeometry(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_PackageManufacturerDrawingGeometry_PackageDefinitionId
    ON PackageManufacturerDrawingGeometry(PackageDefinitionId);

CREATE INDEX IF NOT EXISTS IX_PackageProjectionGeometry_GeometryId
    ON PackageProjectionGeometry(ManufacturerDrawingGeometryId, ProjectionType);

CREATE INDEX IF NOT EXISTS IX_PackageLeadGeometry_GeometryId
    ON PackageLeadGeometry(ManufacturerDrawingGeometryId, ProjectionType, LeadNumber);

CREATE INDEX IF NOT EXISTS IX_PackageDimensionTolerance_GeometryId
    ON PackageDimensionTolerance(ManufacturerDrawingGeometryId, DimensionKey);

CREATE INDEX IF NOT EXISTS IX_PackagePin1Geometry_GeometryId
    ON PackagePin1Geometry(ManufacturerDrawingGeometryId, ProjectionType);

-- A package may have only one current verified manufacturer drawing.
CREATE UNIQUE INDEX IF NOT EXISTS UX_PackageManufacturerDrawingGeometry_VerifiedCurrent
    ON PackageManufacturerDrawingGeometry(PackageDefinitionId)
    WHERE VerificationStatus = 'Verified' AND IsCurrent = 1;
