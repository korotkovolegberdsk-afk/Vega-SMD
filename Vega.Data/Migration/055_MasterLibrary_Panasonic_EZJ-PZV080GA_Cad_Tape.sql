-- EZJ-PZV080GA: manufacturer dimensions and pressed-carrier taping are taken
-- from Panasonic EZJP-series documents (revision 1-Aug-25).
-- Ultra Librarian did not supply a footprint or a 3D model for this exact MPN.
-- A KiCad 0201 reference STEP is used only after matching its nominal extents
-- to the Panasonic outline; no MPN-specific land or aperture is created.

UPDATE PackageDefinition
SET Height = 0.30,
    Notes = 'Panasonic EZJP Z/0201 outline: 0.60 ±0.03 × 0.30 ±0.03 × 0.30 ±0.03 mm. MPN footprint is not inferred from package dimensions.'
WHERE PackageName = 'VAR0201';

INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\KiCad\\C_0201_0603Metric\\C_0201_0603Metric.step',
       '453AFBA1C6E8AD718C79C3B370606CDF345221D24AC8202778236E8B3BFB19C7',
       0.60, 0.30, 0.30,
       'KiCad reference model; Panasonic dimensions verified',
       'https://industrial.panasonic.com/cdbs/www-data/pdf/AWC0000/AWC0000C55.pdf',
       'ReferenceValidated',
       'KiCad C_0201_0603Metric STEP is a generic reference model, not an exact Panasonic CAD export. Its rendered nominal extents match Panasonic EZJP Z/0201 dimensions: 0.60 × 0.30 × 0.30 mm.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'EZJ-PZV080GA'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);

INSERT INTO ComponentTapeReelGeometry
    (ComponentDefinitionId, PackagingCode, SourceReference, SourceRevision, TapeStandard,
     CarrierTapeWidth, PocketPitch, PocketLength, PocketWidth, PocketDepth, PocketOffsetX, PocketOffsetY,
     SprocketHolePitch, SprocketHoleDiameter, SprocketHoleOffset,
     FeedDirection, PocketOrientation, Pin1Orientation, PickupRotation,
     ReelDiameter, QuantityPerReel, IsDefault, IsActive, VerificationStatus, Notes,
     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT c.Id,
       'GA',
       'Panasonic EZJP series — Packaging (Pressed carrier taping)',
       '1-Aug-25', 'Pressed carrier taping',
       8.00, 2.00, 1.12, 0.62, 0.60, 2.00, 3.50,
       4.00, 1.50, 1.75,
       0, 90, 'Длинная сторона компонента поперёк направления подачи', 90,
       180.00, 10000, 1, 1, 1,
       'Panasonic EZJP Z/0201: A=0.62±0.05 мм (вдоль подачи), B=1.12±0.05 мм (поперёк), K0≤0.60 мм; W=8.0±0.2 мм, P0=4.0±0.1 мм, P1=2.0±0.05 мм, P2=2.0±0.05 мм, ØD0=1.5 +0.1/-0.0 мм, F=3.50±0.05 мм, E=1.75±0.10 мм.',
       datetime('now'), 'Panasonic document import', datetime('now'), 'Panasonic document import',
       'Exact GA packaging profile imported from Panasonic EZJP-series packaging document.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'EZJ-PZV080GA'
  AND NOT EXISTS (SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId = c.Id AND t.PackagingCode = 'GA');

UPDATE ComponentDefinition
SET Notes = 'Exact MPN body dimensions and GA tape profile are verified from Panasonic EZJP-series documentation. Ultra Librarian has no exact footprint or 3D model; a matched-size KiCad reference STEP is used for visualization only. No aperture or land geometry is inferred.'
WHERE ManufacturerPartNumber = 'EZJ-PZV080GA';
