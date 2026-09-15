-- Panasonic EZJP 0402 parts: the manufacturer EZJP-series drawing specifies
-- 1.00 ±0.05 × 0.50 ±0.05 × 0.50 ±0.05 mm for size code 0 / 0402.
-- No exact-MPN Ultra Librarian CAD/footprint is available, so the matching
-- KiCad 0402 model is recorded only as a reference visualisation.

UPDATE PackageDefinition
SET Height = 0.50,
    Notes = 'Panasonic EZJP size 0/0402 outline: 1.00 ±0.05 × 0.50 ±0.05 × 0.50 ±0.05 mm. MPN footprint is not inferred from package dimensions.'
WHERE PackageName = 'VAR0402';

INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\KiCad\\C_0402_1005Metric\\C_0402_1005Metric.step',
       '03E44C4B2B727B8D4B6B1FD598CBD12DD5E614F1DFEEFA96A92A07E3A3B312A2',
       1.00, 0.50, 0.50,
       'KiCad reference model; Panasonic dimensions verified',
       'https://industrial.panasonic.com/cdbs/www-data/pdf/AWC0000/AWC0000C55.pdf',
       'ReferenceValidated',
       'KiCad C_0402_1005Metric STEP is a generic reference model, not an exact Panasonic CAD export. Its rendered nominal extents match Panasonic EZJP size 0/0402 dimensions: 1.00 × 0.50 × 0.50 mm.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber IN ('EZJP0V270GA', 'EZJP0V420WA')
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);

UPDATE ComponentDefinition
SET Notes = 'Exact MPN body dimensions are verified from Panasonic EZJP-series documentation. Ultra Librarian has no exact footprint or 3D model; a matched-size KiCad reference STEP is used for visualization only. No aperture, land geometry, or tape profile is inferred.'
WHERE ManufacturerPartNumber IN ('EZJP0V270GA', 'EZJP0V420WA');
