-- Infineon PG-SOT23-3-4 verified manufacturer drawing seed.
-- The existing SOT23 PackageDefinition is referenced; no package is created.
-- End, 3D, footprint, stencil and tape geometry are intentionally omitted.

INSERT INTO PackageManufacturerDrawingGeometry
(
    PackageDefinitionId, Manufacturer, PackageSeries, SourceDocument,
    SourceRevision, VerificationStatus, IsActive, IsCurrent, Notes
)
SELECT
    p.Id,
    'Infineon',
    'PG-SOT23-3-4',
    'PG-SOT23-3-4 Package Outline',
    '05_00',
    'Verified',
    1,
    1,
    'Official Infineon package outline. End view and numeric lead coordinates are not present in the source drawing.'
FROM PackageDefinition p
WHERE UPPER(TRIM(p.PackageName)) = 'SOT23'
  AND NOT EXISTS
  (
      SELECT 1
      FROM PackageManufacturerDrawingGeometry g
      WHERE g.PackageDefinitionId = p.Id
        AND g.VerificationStatus = 'Verified'
        AND g.IsCurrent = 1
  );

-- Resolve the current verified Infineon geometry for the existing SOT23.
INSERT INTO PackageProjectionGeometry
(
    ManufacturerDrawingGeometryId, ProjectionType, CoordinateSystem,
    BodyContour, LeadContour, AuxiliaryGeometry, IsAvailable
)
SELECT g.Id, 'Top', 'mm', '',
       'Manufacturer gull-wing lead from Infineon package outline', '', 1
FROM PackageManufacturerDrawingGeometry g
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageProjectionGeometry p
      WHERE p.ManufacturerDrawingGeometryId = g.Id
        AND p.ProjectionType = 'Top'
  );

INSERT INTO PackageProjectionGeometry
(
    ManufacturerDrawingGeometryId, ProjectionType, CoordinateSystem,
    BodyContour, LeadContour, AuxiliaryGeometry, IsAvailable
)
SELECT g.Id, 'Side', 'mm', '',
       'Manufacturer gull-wing side profile from source drawing', '', 1
FROM PackageManufacturerDrawingGeometry g
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageProjectionGeometry p
      WHERE p.ManufacturerDrawingGeometryId = g.Id
        AND p.ProjectionType = 'Side'
  );

-- Lead coordinates are deliberately left at the schema defaults because the
-- source drawing does not provide numeric Root/Contact coordinates.
INSERT INTO PackageLeadGeometry
(
    ManufacturerDrawingGeometryId, LeadNumber, Side, ProjectionType,
    ProfileGeometry, Width, Thickness, PitchAlongRow, IsPin1
)
SELECT g.Id, v.LeadNumber, v.Side, 'Top',
       'Manufacturer gull-wing lead from Infineon package outline',
       0.38, 0.09, 1.9,
       CASE WHEN v.LeadNumber = 1 THEN 1 ELSE 0 END
FROM PackageManufacturerDrawingGeometry g
JOIN
(
    SELECT 1 AS LeadNumber, 'Left' AS Side
    UNION ALL SELECT 2, 'Right'
    UNION ALL SELECT 3, 'Top'
) v ON 1 = 1
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageLeadGeometry l
      WHERE l.ManufacturerDrawingGeometryId = g.Id
        AND l.ProjectionType = 'Top'
        AND l.LeadNumber = v.LeadNumber
  );

-- Complete an earlier compatible seed, if present, without changing lead
-- coordinates or creating duplicate lead rows.
UPDATE PackageLeadGeometry
SET Width = 0.38,
    Thickness = 0.09,
    PitchAlongRow = 1.9,
    ProfileGeometry = 'Manufacturer gull-wing lead from Infineon package outline'
WHERE ManufacturerDrawingGeometryId IN
(
    SELECT Id FROM PackageManufacturerDrawingGeometry
    WHERE PackageSeries = 'PG-SOT23-3-4'
      AND VerificationStatus = 'Verified'
      AND IsCurrent = 1
)
  AND ProjectionType = 'Top'
  AND LeadNumber IN (1, 2, 3);

INSERT INTO PackagePin1Geometry
(
    ManufacturerDrawingGeometryId, ProjectionType, IsPresent, MarkerType
)
SELECT g.Id, 'Top', 1, 'Circle'
FROM PackageManufacturerDrawingGeometry g
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackagePin1Geometry p
      WHERE p.ManufacturerDrawingGeometryId = g.Id
        AND p.ProjectionType = 'Top'
  );

INSERT INTO PackageDimensionTolerance
(
    ManufacturerDrawingGeometryId, DimensionKey, ProjectionType,
    NominalValue, MinValue, MaxValue, Symbol, Unit, SourceLabel
)
SELECT g.Id, v.DimensionKey, v.ProjectionType, v.NominalValue,
       v.MinValue, v.MaxValue, v.Symbol, 'mm', v.SourceLabel
FROM PackageManufacturerDrawingGeometry g
JOIN
(
    SELECT 'BodyLength' AS DimensionKey, 'Top' AS ProjectionType, 2.9 AS NominalValue, NULL AS MinValue, NULL AS MaxValue, '±0.1' AS Symbol, '2.9 ±0.1' AS SourceLabel
    UNION ALL SELECT 'BodyWidth', 'Top', 1.3, NULL, NULL, '±0.1', '1.3 ±0.1'
    UNION ALL SELECT 'LeadPitchAlongRow', 'Top', 1.9, NULL, NULL, '', '1.9'
    UNION ALL SELECT 'LeadWidth', 'Top', 0.38, NULL, 0.48, '+0.10 / -0.05', '0.38 +0.10/-0.05'
    UNION ALL SELECT 'BodyHeight', 'Side', 1.0, NULL, NULL, '±0.1', '1.0 ±0.1'
    UNION ALL SELECT 'LeadThicknessMin', 'Side', NULL, 0.09, NULL, 'min.', '0.09'
    UNION ALL SELECT 'LeadThicknessMax', 'Side', NULL, NULL, 0.15, 'max.', '0.15'
    UNION ALL SELECT 'StandOffMin', 'Side', NULL, 0.15, NULL, 'min.', '0.15'
    UNION ALL SELECT 'LeadAngleMax', 'Side', NULL, NULL, 8.0, 'max.', '0...8°'
) v ON 1 = 1
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageDimensionTolerance d
      WHERE d.ManufacturerDrawingGeometryId = g.Id
        AND d.DimensionKey = v.DimensionKey
  );
