-- Exact-MPN KiCad v6+ export received from Ultra Librarian on 10-Sep-2026.
-- The archive tags the footprint with EZJS2YD472.  It contains no STEP file,
-- so the existing explicitly-labelled KiCad reference visualisation remains
-- untouched and must not be promoted to an exact MPN model.

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id,
       'EZJS_0805_PAN', 2, 1.1120, 1.4478, 1.7978,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/57356004-107a-11e9-ab3a-0a3560a4cccc/Panasonic/EZJS2YD472',
       'KiCad v6+ / EZJS_0805_PAN', 'Verified',
       'Exact KiCad v6+ export. Pads 1 and 2 are 1.1120 × 1.4478 mm at ±0.8989 mm; no paste reduction is applied.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'EZJS2YD472'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);

UPDATE ComponentCadModel
SET Notes = 'KiCad C_0805_2012Metric STEP remains a generic size-matched reference visualisation. The Ultra Librarian archive for EZJS2YD472 supplied the exact KiCad footprint but no STEP file; this model is not an exact Panasonic CAD export.'
WHERE ComponentDefinitionId = (
    SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'EZJS2YD472'
);

UPDATE ComponentDefinition
SET Notes = 'Exact EZJS2YD472 KiCad footprint is verified from the Ultra Librarian KiCad v6+ export. The received archive contains no STEP model; the existing size-matched KiCad STEP remains only a reference visualisation. The embossed Tape & Reel profile remains verified from Panasonic documentation. Paste reduction is a separate process rule.',
    UpdatedAt = datetime('now'),
    UpdatedBy = 'Ultra Librarian import',
    ChangeComment = 'Added exact-MPN Ultra Librarian footprint; archive contains no STEP model.'
WHERE ManufacturerPartNumber = 'EZJS2YD472';
