-- Official Infineon PG-SOT23-3-4 package-outline raster asset.
-- The image is display evidence only; no coordinates are inferred from it.
INSERT INTO PackageManufacturerDrawingAsset
(
    PackageManufacturerDrawingGeometryId, ProjectionGeometryId, ProjectionType,
    SourceDocument, SourceRevision, SourcePage, AssetType, FilePath,
    DocumentReference, CropReference, VerificationStatus, IsCurrent, IsActive,
    Sha256, MimeType, Width, Height, Notes
)
SELECT g.Id, NULL, 'Top',
       'PG-SOT23-3-4 Package Outline', '05_00', '1', 'RasterImage',
       'Assets\\Manufacturer\\Infineon\\PG-SOT23-3-4-Package-Outline.png',
       'https://assets.infineon.com/is/image/infineon/infineon-pg-sot23-3-4-spo-png-package-en.png',
       '', 'Verified', 1, 1, '', 'image/png', NULL, NULL,
       'Official package outline sheet; displayed without geometric reconstruction.'
FROM PackageManufacturerDrawingGeometry g
WHERE g.PackageSeries = 'PG-SOT23-3-4'
  AND g.Manufacturer = 'Infineon'
  AND g.VerificationStatus = 'Verified'
  AND g.IsCurrent = 1
  AND NOT EXISTS
  (
      SELECT 1 FROM PackageManufacturerDrawingAsset a
      WHERE a.PackageManufacturerDrawingGeometryId = g.Id
        AND a.ProjectionType = 'Top'
        AND a.AssetType = 'RasterImage'
        AND a.IsCurrent = 1
        AND a.VerificationStatus = 'Verified'
        AND a.IsActive = 1
  );
