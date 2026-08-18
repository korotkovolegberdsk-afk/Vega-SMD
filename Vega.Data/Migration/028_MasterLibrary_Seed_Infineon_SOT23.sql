-- Verified manufacturer drawing seed for the existing SOT23 package.
-- Source: Infineon, PG-SOT23-3-4 Package Outline, revision 05_00.
-- Coordinates and End projection are intentionally omitted because they are
-- not explicitly dimensioned in the source drawing.

INSERT INTO PackageManufacturerDrawingGeometry
(
    PackageDefinitionId, Manufacturer, PackageSeries, SourceDocument,
    SourceRevision, VerificationStatus, IsActive, IsCurrent, Notes
)
SELECT
    Id,
    'Infineon',
    'PG-SOT23-3-4',
    'PG-SOT23-3-4 Package Outline',
    '05_00',
    'Verified',
    1,
    1,
    'Official Infineon Package Outline. End projection and numeric lead coordinates are not provided; no fallback geometry is seeded.'
FROM PackageDefinition
WHERE UPPER(TRIM(PackageName)) = 'SOT23'
  AND NOT EXISTS
  (
      SELECT 1
      FROM PackageManufacturerDrawingGeometry g
      WHERE g.PackageDefinitionId = PackageDefinition.Id
        AND g.VerificationStatus = 'Verified'
        AND g.IsCurrent = 1
  );

INSERT INTO PackageProjectionGeometry
(
    ManufacturerDrawingGeometryId, ProjectionType, CoordinateSystem,
    BodyContour, LeadContour, AuxiliaryGeometry, IsAvailable
)
SELECT Id, 'Top', 'mm', '', '', '', 1
FROM PackageManufacturerDrawingGeometry
WHERE PackageSeries = 'PG-SOT23-3-4'
  AND VerificationStatus = 'Verified'
  AND IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageProjectionGeometry p
      WHERE p.ManufacturerDrawingGeometryId = PackageManufacturerDrawingGeometry.Id
        AND p.ProjectionType = 'Top'
  );

INSERT INTO PackageProjectionGeometry
(
    ManufacturerDrawingGeometryId, ProjectionType, CoordinateSystem,
    BodyContour, LeadContour, AuxiliaryGeometry, IsAvailable
)
SELECT Id, 'Side', 'mm', '', '', '', 1
FROM PackageManufacturerDrawingGeometry
WHERE PackageSeries = 'PG-SOT23-3-4'
  AND VerificationStatus = 'Verified'
  AND IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageProjectionGeometry p
      WHERE p.ManufacturerDrawingGeometryId = PackageManufacturerDrawingGeometry.Id
        AND p.ProjectionType = 'Side'
  );

INSERT INTO PackageLeadGeometry
(
    ManufacturerDrawingGeometryId, LeadNumber, Side, ProjectionType,
    ProfileGeometry, IsPin1
)
SELECT g.Id, v.LeadNumber, v.Side, 'Top',
       'Manufacturer gull-wing profile from source drawing',
       CASE WHEN v.LeadNumber = 1 THEN 1 ELSE 0 END
FROM PackageManufacturerDrawingGeometry g
JOIN
(
    SELECT 1 AS LeadNumber, 'Left' AS Side
    UNION ALL SELECT 2, 'Right'
    UNION ALL SELECT 3, 'Top'
) v
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
       v.MinValue, v.MaxValue, v.Symbol, v.Unit, v.SourceLabel
FROM PackageManufacturerDrawingGeometry g
JOIN
(
    SELECT 'BodyLength' AS DimensionKey, 'Top' AS ProjectionType, 2.9 AS NominalValue, NULL AS MinValue, NULL AS MaxValue, '±0.1' AS Symbol, 'mm' AS Unit, '2.9 ±0.1' AS SourceLabel
    UNION ALL SELECT 'BodyWidth', 'Top', 1.3, NULL, NULL, '±0.1', 'mm', '1.3 ±0.1'
    UNION ALL SELECT 'LeadCount', 'Top', 3.0, NULL, NULL, '', 'count', '3'
    UNION ALL SELECT 'LeadPitch', 'Top', 0.95, NULL, NULL, '', 'mm', '0.95'
    UNION ALL SELECT 'LeadWidth', 'Top', 0.4, 0.35, 0.5, '+0.10 / -0.05', 'mm', '0.4 +0.10/-0.05'
    UNION ALL SELECT 'BodyHeight', 'Side', 1.0, NULL, NULL, '±0.1', 'mm', '1.0 ±0.1'
    UNION ALL SELECT 'LeadThicknessMin', 'Side', NULL, 0.08, NULL, '', 'mm', '0.08...0.15'
    UNION ALL SELECT 'LeadThicknessMax', 'Side', NULL, NULL, 0.15, '', 'mm', '0.08...0.15'
    UNION ALL SELECT 'StandOffMin', 'Side', NULL, 0.15, NULL, 'min.', 'mm', '0.15 Min.'
    UNION ALL SELECT 'LeadAngleMax', 'Side', NULL, NULL, 8.0, 'max.', 'deg', '0...8°'
    UNION ALL SELECT 'OverallLeadSpan', 'Top', 2.4, NULL, NULL, '±0.15', 'mm', '2.4 ±0.15'
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
