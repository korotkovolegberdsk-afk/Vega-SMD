-- Stores manufacturer-supplied drawing primitives for verified projections.
-- This migration creates schema only; no existing geometry or data is changed.

CREATE TABLE IF NOT EXISTS PackageDrawingPrimitive
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectionGeometryId INTEGER NOT NULL,
    PrimitiveType TEXT NOT NULL CHECK (PrimitiveType IN ('Line', 'Arc', 'Circle', 'Polygon')),

    StartX REAL,
    StartY REAL,
    EndX REAL,
    EndY REAL,

    CenterX REAL,
    CenterY REAL,
    Radius REAL,
    StartAngle REAL,
    EndAngle REAL,

    GeometryData TEXT NOT NULL DEFAULT '',
    StrokeWidth REAL,
    Layer TEXT NOT NULL CHECK (Layer IN ('Body', 'Lead', 'Pin1', 'Dimension', 'Auxiliary')),
    CoordinateSystem TEXT NOT NULL DEFAULT 'mm',

    FOREIGN KEY (ProjectionGeometryId)
        REFERENCES PackageProjectionGeometry(Id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_PackageDrawingPrimitive_ProjectionGeometryId
    ON PackageDrawingPrimitive(ProjectionGeometryId);

CREATE INDEX IF NOT EXISTS IX_PackageDrawingPrimitive_Layer
    ON PackageDrawingPrimitive(Layer);

CREATE INDEX IF NOT EXISTS IX_PackageDrawingPrimitive_PrimitiveType
    ON PackageDrawingPrimitive(PrimitiveType);
