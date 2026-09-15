-- Exact MPN import from the Ultra Librarian archive downloaded 2026-09-11.
-- The archive tags the selected KiCad footprint and symbol with MLK1005S56NJTD25.
INSERT INTO ComponentDefinition
    (ManufacturerPartNumber, Manufacturer, Description, ComponentType, Value, Tolerance,
     PackageId, LifecycleStatus, DatasheetUrl, Notes, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT
    'MLK1005S56NJTD25', 'TDK',
    'General-purpose 56 nH ±5 % ceramic-core SMD inductor, 0402 / 1005 metric',
    'Inductor', '56 nH', '±5 %',
    p.Id, 'Production',
    'https://app.ultralibrarian.com/details/77f412ce-1073-11e9-ab3a-0a3560a4cccc/TDK/MLK1005S56NJTD25',
    'Exact Ultra Librarian export. No tape profile is recorded until a source for this MPN is verified.',
    1, datetime('now'), 'Ultra Librarian import', datetime('now'), 'Ultra Librarian import',
    'Exact MPN CAD import: STEP and KiCad v6+ in metric units.'
FROM PackageDefinition p
WHERE p.PackageName = 'IND0402'
  AND NOT EXISTS (SELECT 1 FROM ComponentDefinition WHERE ManufacturerPartNumber = 'MLK1005S56NJTD25');

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id, 'IND_1005_TDK', 2, 0.57, 0.60, 1.13,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/77f412ce-1073-11e9-ab3a-0a3560a4cccc/TDK/MLK1005S56NJTD25',
       'IND_1005_TDK', 'Verified',
       'Exact KiCad v6 export tag: MLK1005S56NJTD25. Pad geometry is the source footprint; paste reduction is not applied.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'MLK1005S56NJTD25'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);

INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\TDK\\MLK1005S56NJTD25\\IND_1005_TDK.step',
       'B700C53B2DDA889D67E9F56007A071595AEE2B1EB79E7F7CC60B66C770E12654',
       1.00, 0.50, 0.50,
       'Ultra Librarian',
       'https://app.ultralibrarian.com/details/77f412ce-1073-11e9-ab3a-0a3560a4cccc/TDK/MLK1005S56NJTD25',
       'Verified',
       'Exact STEP file from the MLK1005S56NJTD25 archive; projection dimensions are verified by Vega-SMD.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'MLK1005S56NJTD25'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);
