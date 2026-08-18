-- Official manufacturer drawing assets for verified package geometry.
-- Schema-only migration: no existing rows are changed.

CREATE TABLE IF NOT EXISTS PackageManufacturerDrawingAsset
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageManufacturerDrawingGeometryId INTEGER NOT NULL,
    ProjectionGeometryId INTEGER,
    ProjectionType TEXT NOT NULL CHECK (ProjectionType IN ('Top', 'Side', 'End', 'ThreeD')),

    SourceDocument TEXT NOT NULL DEFAULT '',
    SourceRevision TEXT NOT NULL DEFAULT '',
    SourcePage TEXT NOT NULL DEFAULT '',
    AssetType TEXT NOT NULL CHECK (AssetType IN ('RasterImage', 'SvgImage', 'PdfPage', 'PdfRegion')),
    FilePath TEXT NOT NULL DEFAULT '',
    DocumentReference TEXT NOT NULL DEFAULT '',
    CropReference TEXT NOT NULL DEFAULT '',
    VerificationStatus TEXT NOT NULL DEFAULT 'Unverified',

    IsCurrent INTEGER NOT NULL DEFAULT 0 CHECK (IsCurrent IN (0, 1)),
    IsActive INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),

    Sha256 TEXT NOT NULL DEFAULT '',
    MimeType TEXT NOT NULL DEFAULT '',
    Width REAL,
    Height REAL,
    Notes TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),

    FOREIGN KEY (PackageManufacturerDrawingGeometryId)
        REFERENCES PackageManufacturerDrawingGeometry(Id)
        ON DELETE CASCADE,
    FOREIGN KEY (ProjectionGeometryId)
        REFERENCES PackageProjectionGeometry(Id)
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS IX_PackageManufacturerDrawingAsset_GeometryId
    ON PackageManufacturerDrawingAsset(PackageManufacturerDrawingGeometryId);

CREATE INDEX IF NOT EXISTS IX_PackageManufacturerDrawingAsset_ProjectionId
    ON PackageManufacturerDrawingAsset(ProjectionGeometryId);

CREATE INDEX IF NOT EXISTS IX_PackageManufacturerDrawingAsset_ProjectionType
    ON PackageManufacturerDrawingAsset(ProjectionType);

CREATE INDEX IF NOT EXISTS IX_PackageManufacturerDrawingAsset_VerificationStatus
    ON PackageManufacturerDrawingAsset(VerificationStatus);

CREATE UNIQUE INDEX IF NOT EXISTS UX_PackageManufacturerDrawingAsset_CurrentVerified
    ON PackageManufacturerDrawingAsset
    (
        PackageManufacturerDrawingGeometryId,
        ProjectionType,
        AssetType
    )
    WHERE IsCurrent = 1
      AND VerificationStatus = 'Verified'
      AND IsActive = 1;
