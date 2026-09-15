-- Exact-MPN CAD export received from Ultra Librarian on 10-Sep-2026.

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id,
       'EZJS_0805_PAN', 2, 1.1120, 1.4478, 1.7978,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/57355db9-107a-11e9-ab3a-0a3560a4cccc/Panasonic/EZJS2VB223',
       'KiCad v6+ / EZJS_0805_PAN', 'Verified',
       'Exact KiCad v6+ export. Pads 1 and 2 are 1.1120 × 1.4478 mm at ±0.8989 mm; no paste reduction is applied.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'EZJS2VB223'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);

UPDATE ComponentCadModel
SET ModelPath = 'Assets\\Manufacturer\\Panasonic\\EZJS2VB223\\EZJS_0805_PAN.step',
    FileSha256 = 'B94B8BA2F8E6895DB8EF38CD7ED241A2B95047C78361BF49DBF3A532D215BD0F',
    Length = 2.00, Width = 1.25, Height = 0.80,
    SourceSystem = 'Ultra Librarian',
    SourceUrl = 'https://app.ultralibrarian.com/details/57355db9-107a-11e9-ab3a-0a3560a4cccc/Panasonic/EZJS2VB223',
    VerificationStatus = 'Verified',
    Notes = 'Exact EZJS2VB223 STEP export from Ultra Librarian; the file hash and KiCad archive tags were verified.'
WHERE ComponentDefinitionId = (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'EZJS2VB223');

UPDATE ComponentDefinition
SET Notes = 'Exact EZJS2VB223 footprint and STEP model are verified from the Ultra Librarian KiCad v6+ and STEP export. The embossed Tape & Reel profile remains verified from Panasonic documentation.',
    UpdatedAt = datetime('now'), UpdatedBy = 'Ultra Librarian import',
    ChangeComment = 'Replaced generic reference with exact-MPN Ultra Librarian CAD data.'
WHERE ManufacturerPartNumber = 'EZJS2VB223';
