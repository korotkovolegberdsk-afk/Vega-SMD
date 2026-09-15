-- Exact-MPN CAD export received from Ultra Librarian on 10-Sep-2026.
-- The archive itself identifies EZJS2YC822 in the KiCad tags and provides
-- the EZJS_0805_PAN STEP model.  These dimensions are source geometry;
-- paste reduction is intentionally not applied here.

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id,
       'EZJS_0805_PAN', 2, 1.1176, 1.4478, 1.8034,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/891e7cd6-109b-11e9-ab3a-0a3560a4cccc/Panasonic/EZJS2YC822',
       'KiCad v6+ / EZJS_0805_PAN', 'Verified',
       'Exact KiCad v6+ export. Pads 1 and 2 are 1.1176 × 1.4478 mm at ±0.9017 mm; no paste reduction is applied.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'EZJS2YC822'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);

UPDATE ComponentCadModel
SET ModelPath = 'Assets\\Manufacturer\\Panasonic\\EZJS2YC822\\EZJS_0805_PAN.step',
    FileSha256 = '577D5EB71454069034F88A633ECB71F80B74EFBB9F2CBB7CB43857A05EFF7FE1',
    Length = 2.00,
    Width = 1.25,
    Height = 0.80,
    SourceSystem = 'Ultra Librarian',
    SourceUrl = 'https://app.ultralibrarian.com/details/891e7cd6-109b-11e9-ab3a-0a3560a4cccc/Panasonic/EZJS2YC822',
    VerificationStatus = 'Verified',
    Notes = 'Exact EZJS2YC822 STEP export from Ultra Librarian. The file SHA-256 and KiCad archive tags were verified; rendered nominal dimensions are 2.00 × 1.25 × 0.80 mm.'
WHERE ComponentDefinitionId = (
    SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'EZJS2YC822'
);

UPDATE ComponentDefinition
SET Notes = 'Exact EZJS2YC822 footprint and STEP model are verified from the Ultra Librarian KiCad v6+ and STEP export. The embossed Tape & Reel profile remains verified from Panasonic documentation. Paste reduction is a separate process rule.',
    UpdatedAt = datetime('now'),
    UpdatedBy = 'Ultra Librarian import',
    ChangeComment = 'Replaced the generic KiCad visual reference with exact-MPN Ultra Librarian CAD data.'
WHERE ManufacturerPartNumber = 'EZJS2YC822';
