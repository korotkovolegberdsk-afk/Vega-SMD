-- Panasonic EZJS 0805 parts.  The EZJS datasheet gives a 2.00 ±0.20 ×
-- 1.25 ±0.20 × 0.80 ±0.20 mm outline.  The KiCad STEP is a geometry-matched
-- reference visualisation, never an exact Panasonic CAD export.

UPDATE PackageDefinition
SET Height = 0.80,
    Notes = 'Panasonic EZJS size 2/0805 outline: 2.00 ±0.20 × 1.25 ±0.20 × 0.80 ±0.20 mm. MPN footprint is not inferred from package dimensions.'
WHERE PackageName = 'VAR0805';

INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\KiCad\\C_0805_2012Metric\\C_0805_2012Metric.step',
       '9A669C1A2F1EA25B88B401ACEF6EFEAA80A067BD7DA9DB6993DE719A7FBE155C',
       2.00, 1.25, 0.80,
       'KiCad reference model; Panasonic dimensions verified',
       'https://industrial.panasonic.com/cdbs/www-data/pdf/AWC0000/AWC0000C21.pdf',
       'ReferenceValidated',
       'KiCad C_0805_2012Metric STEP is a generic reference model, not an exact Panasonic CAD export. Its rendered nominal extents match Panasonic EZJS size 2/0805 dimensions: 2.00 × 1.25 × 0.80 mm.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber IN ('EZJS2VB223', 'EZJS2YC822', 'EZJS2YD472')
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);

-- The official component pages identify these two exact MPNs as embossed tape.
-- The packaging sheet supplies its 0805 embossing geometry.
INSERT INTO ComponentTapeReelGeometry
    (ComponentDefinitionId, PackagingCode, SourceReference, SourceRevision, TapeStandard,
     CarrierTapeWidth, PocketPitch, PocketLength, PocketWidth, PocketDepth, PocketOffsetX, PocketOffsetY,
     SprocketHolePitch, SprocketHoleDiameter, SprocketHoleOffset,
     FeedDirection, PocketOrientation, Pin1Orientation, PickupRotation,
     ReelDiameter, QuantityPerReel, IsDefault, IsActive, VerificationStatus, Notes,
     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT c.Id,
       'Embossed / Ø180',
       'Panasonic EZJS product page and EZJZ/EZJP/EZJS Packaging',
       '1-Mar-20', 'Embossed carrier taping',
       8.00, 4.00, 2.35, 1.55, 0.55, 2.00, 3.50,
       4.00, 1.50, 1.75,
       0, 90, 'Длинная сторона компонента поперёк направления подачи', 90,
       180.00, 5000, 1, 1, 1,
       'Panasonic EZJS 0805 embossed: A=1.55±0.20 мм (вдоль подачи), B=2.35±0.20 мм (поперёк), K0≤0.55 мм; W=8.0±0.2 мм, P0=4.0±0.1 мм, P1=4.0±0.1 мм, P2=2.0±0.05 мм, ØD0=1.5 +0.1/-0.0 мм, F=3.50±0.05 мм, E=1.75±0.10 мм, 5000 шт. на Ø180 мм катушке.',
       datetime('now'), 'Panasonic document import', datetime('now'), 'Panasonic document import',
       'Exact MPN is identified as embossed tape on the Panasonic product page; geometry imported from the shared Panasonic packaging specification.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber IN ('EZJS2YC822', 'EZJS2YD472')
  AND NOT EXISTS (SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId = c.Id AND t.PackagingCode = 'Embossed / Ø180');

UPDATE ComponentDefinition
SET Notes = CASE
    WHEN ManufacturerPartNumber IN ('EZJS2YC822', 'EZJS2YD472') THEN 'Exact MPN body dimensions and embossed Tape & Reel profile are verified from Panasonic documentation. Ultra Librarian has no exact footprint or 3D model; a matched-size KiCad reference STEP is used for visualization only. No aperture or land geometry is inferred.'
    ELSE 'Exact MPN body dimensions and paper-tape type are verified from Panasonic documentation. Ultra Librarian has no exact footprint or 3D model; a matched-size KiCad reference STEP is used for visualization only. Exact paper-tape geometry, apertures, and land geometry are not inferred.'
END
WHERE ManufacturerPartNumber IN ('EZJS2VB223', 'EZJS2YC822', 'EZJS2YD472');
