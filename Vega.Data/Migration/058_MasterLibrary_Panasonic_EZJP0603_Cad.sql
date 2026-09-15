-- Panasonic EZJP size 1 / 0603 outline: 1.60 ±0.10 × 0.80 ±0.10 × 0.80 ±0.10 mm.
-- The KiCad model is a matched-size visual reference, not an exact MPN CAD export.

UPDATE PackageDefinition
SET Height = 0.80,
    Notes = 'Panasonic EZJP size 1/0603 outline: 1.60 ±0.10 × 0.80 ±0.10 × 0.80 ±0.10 mm. MPN footprint is not inferred from package dimensions.'
WHERE PackageName = 'VAR0603';

INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\KiCad\\C_0603_1608Metric\\C_0603_1608Metric.step',
       '4857B3DC717675BF1FB22EDDA622AC3B3AE03CF44D81EEFAF3FA3BA3A86D84C1',
       1.60, 0.80, 0.80,
       'KiCad reference model; Panasonic dimensions verified',
       'https://industrial.panasonic.com/cdbs/www-data/pdf/AWC0000/AWC0000C55.pdf',
       'ReferenceValidated',
       'KiCad C_0603_1608Metric STEP is a generic reference model, not an exact Panasonic CAD export. Its rendered nominal extents match Panasonic EZJP size 1/0603 dimensions: 1.60 × 0.80 × 0.80 mm.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber IN ('EZJP1V270GA', 'EZJP1V420FA')
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);

UPDATE ComponentDefinition
SET Notes = 'Exact MPN body dimensions are verified from Panasonic EZJP-series documentation. Ultra Librarian has no exact footprint or 3D model; a matched-size KiCad reference STEP is used for visualization only. No aperture, land geometry, or tape profile is inferred.'
WHERE ManufacturerPartNumber IN ('EZJP1V270GA', 'EZJP1V420FA');
