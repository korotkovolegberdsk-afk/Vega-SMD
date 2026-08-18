-- Crop references select the clearly legible Top and Side regions of the
-- official sheet. They do not create or infer mechanical coordinates.
UPDATE PackageManufacturerDrawingAsset
SET CropReference = '{"page":1,"x":0.02,"y":0.03,"width":0.50,"height":0.78,"coordinateSystem":"normalized-page"}',
    UpdatedAt = datetime('now')
WHERE ProjectionType = 'Top'
  AND AssetType = 'RasterImage'
  AND SourceDocument = 'PG-SOT23-3-4 Package Outline'
  AND VerificationStatus = 'Verified'
  AND IsCurrent = 1
  AND IsActive = 1;

INSERT INTO PackageManufacturerDrawingAsset
(
    PackageManufacturerDrawingGeometryId, ProjectionGeometryId, ProjectionType,
    SourceDocument, SourceRevision, SourcePage, AssetType, FilePath,
    DocumentReference, CropReference, VerificationStatus, IsCurrent, IsActive,
    Sha256, MimeType, Width, Height, Notes
)
SELECT a.PackageManufacturerDrawingGeometryId, NULL, 'Side',
       a.SourceDocument, a.SourceRevision, a.SourcePage, a.AssetType, a.FilePath,
       a.DocumentReference,
       '{"page":1,"x":0.57,"y":0.01,"width":0.41,"height":0.75,"coordinateSystem":"normalized-page"}',
       'Verified', 1, 1, a.Sha256, a.MimeType, a.Width, a.Height,
       'Official Side projection region from the same Infineon package-outline sheet.'
FROM PackageManufacturerDrawingAsset a
WHERE a.ProjectionType = 'Top'
  AND a.AssetType = 'RasterImage'
  AND a.SourceDocument = 'PG-SOT23-3-4 Package Outline'
  AND a.VerificationStatus = 'Verified'
  AND a.IsCurrent = 1
  AND a.IsActive = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageManufacturerDrawingAsset x
      WHERE x.PackageManufacturerDrawingGeometryId = a.PackageManufacturerDrawingGeometryId
        AND x.ProjectionType = 'Side' AND x.AssetType = 'RasterImage'
        AND x.IsCurrent = 1 AND x.VerificationStatus = 'Verified' AND x.IsActive = 1
  );
