-- Provenance-only primitives for the Infineon PG-SOT23-3-4 outline.
-- The source drawing does not publish a numeric coordinate origin or vertex
-- coordinates. Therefore no coordinates are inferred here; GeometryData
-- records the exact source reference for a later controlled import.

-- TOP: body contour.
INSERT INTO PackageDrawingPrimitive
(
    ProjectionGeometryId, PrimitiveType, GeometryData, Layer,
    CoordinateSystem, StrokeWidth
)
SELECT p.Id, 'Polygon',
       '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"body contour","coordinatesProvided":false}',
       'Body', 'mm', NULL
FROM PackageProjectionGeometry p
JOIN PackageManufacturerDrawingGeometry g ON g.Id = p.ManufacturerDrawingGeometryId
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND p.ProjectionType = 'Top'
  AND p.IsAvailable = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageDrawingPrimitive d
      WHERE d.ProjectionGeometryId = p.Id AND d.Layer = 'Body'
  );

-- TOP: three explicitly numbered lead elements. No lead positions are
-- reconstructed from pitch or lead count.
INSERT INTO PackageDrawingPrimitive
(
    ProjectionGeometryId, PrimitiveType, GeometryData, Layer,
    CoordinateSystem, StrokeWidth
)
SELECT p.Id, 'Polygon',
       CASE v.LeadNumber
           WHEN 1 THEN '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"lead","leadNumber":1,"coordinatesProvided":false}'
           WHEN 2 THEN '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"lead","leadNumber":2,"coordinatesProvided":false}'
           ELSE '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"lead","leadNumber":3,"coordinatesProvided":false}'
       END,
       'Lead', 'mm', NULL
FROM PackageProjectionGeometry p
JOIN PackageManufacturerDrawingGeometry g ON g.Id = p.ManufacturerDrawingGeometryId
JOIN (SELECT 1 AS LeadNumber UNION ALL SELECT 2 UNION ALL SELECT 3) v ON 1 = 1
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND p.ProjectionType = 'Top'
  AND p.IsAvailable = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageDrawingPrimitive d
      WHERE d.ProjectionGeometryId = p.Id
        AND d.Layer = 'Lead'
        AND json_extract(d.GeometryData, '$.leadNumber') = v.LeadNumber
  );

-- TOP: Pin1 marker. Its numeric position is not stated in the source.
INSERT INTO PackageDrawingPrimitive
(
    ProjectionGeometryId, PrimitiveType, GeometryData, Layer,
    CoordinateSystem, StrokeWidth
)
SELECT p.Id, 'Circle',
       '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"Pin1 marker","markerType":"Circle","coordinatesProvided":false}',
       'Pin1', 'mm', NULL
FROM PackageProjectionGeometry p
JOIN PackageManufacturerDrawingGeometry g ON g.Id = p.ManufacturerDrawingGeometryId
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND p.ProjectionType = 'Top'
  AND p.IsAvailable = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageDrawingPrimitive d
      WHERE d.ProjectionGeometryId = p.Id AND d.Layer = 'Pin1'
  );

-- SIDE: body contour and the source gull-wing profile reference. No bend or
-- segment coordinates are invented when they are absent from the drawing.
INSERT INTO PackageDrawingPrimitive
(
    ProjectionGeometryId, PrimitiveType, GeometryData, Layer,
    CoordinateSystem, StrokeWidth
)
SELECT p.Id, 'Polygon',
       '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"body contour","coordinatesProvided":false}',
       'Body', 'mm', NULL
FROM PackageProjectionGeometry p
JOIN PackageManufacturerDrawingGeometry g ON g.Id = p.ManufacturerDrawingGeometryId
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND p.ProjectionType = 'Side'
  AND p.IsAvailable = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageDrawingPrimitive d
      WHERE d.ProjectionGeometryId = p.Id AND d.Layer = 'Body'
  );

INSERT INTO PackageDrawingPrimitive
(
    ProjectionGeometryId, PrimitiveType, GeometryData, Layer,
    CoordinateSystem, StrokeWidth
)
SELECT p.Id, 'Polygon',
       '{"source":"Infineon PG-SOT23-3-4 Package Outline","element":"gull-wing side profile","coordinatesProvided":false}',
       'Lead', 'mm', NULL
FROM PackageProjectionGeometry p
JOIN PackageManufacturerDrawingGeometry g ON g.Id = p.ManufacturerDrawingGeometryId
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND p.ProjectionType = 'Side'
  AND p.IsAvailable = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageDrawingPrimitive d
      WHERE d.ProjectionGeometryId = p.Id AND d.Layer = 'Lead'
  );
