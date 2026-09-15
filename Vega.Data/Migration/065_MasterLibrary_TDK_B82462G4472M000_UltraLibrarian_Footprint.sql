-- Exact-MPN footprint received from the Ultra Librarian KiCad v6+ export on 2026-09-11.
-- Body dimensions are from TDK B82462G4 official datasheet/product page.
INSERT OR IGNORE INTO PackageDefinition
    (PackageName, DisplayName, StandardName, PackageFamily, ComponentType, CategoryId, FamilyId,
     Length, Width, Height, BodyLength, BodyWidth, PadCount, LeadCount, IPCName, JEDECName,
     MirtecAoiClass, Description, Notes, IsActive, CreatedAt, UpdatedAt)
SELECT 'IND2424', 'IND2424', 'IND2424', 'INDUCTOR', 'Inductor', c.Id, f.Id,
       6.30, 6.30, 3.00, 6.30, 6.30, 2, 0, 'INDUCTOR', '2424',
       'INDUCTOR', 'TDK B82462G4 shielded wire-wound power inductor',
       'Exact TDK maximum body size: 6.30 x 6.30 x 3.00 mm.',
       1, datetime('now'), datetime('now')
FROM PackageCategory c
JOIN PackageFamily f ON f.CategoryId = c.Id AND f.Code = 'INDUCTOR'
WHERE c.Code = 'PASSIVE';

INSERT INTO MasterLibrary_PackageAliases(PackageId, Alias, AliasType, Source)
SELECT p.Id, 'IND-2424', 'Standard', 'TDK B82462G4'
FROM PackageDefinition p
WHERE p.PackageName = 'IND2424'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageAliases a WHERE a.PackageId = p.Id AND a.Alias = 'IND-2424');

INSERT INTO ComponentDefinition
    (ManufacturerPartNumber, Manufacturer, Description, ComponentType, Value, Tolerance,
     PackageId, LifecycleStatus, DatasheetUrl, Notes, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT 'B82462G4472M000', 'TDK',
       '4.7 uH shielded wire-wound automotive power inductor, 6.3 x 6.3 x 3.0 mm',
       'Inductor', '4.7 uH', '±20 %',
       p.Id, 'Production',
       'https://product.tdk.com/en/search/inductor/inductor/smd/info?part_no=B82462G4472M000',
       'Exact KiCad footprint and official TDK body dimensions. No STEP file or tape geometry was delivered; do not substitute either.',
       1, datetime('now'), 'Ultra Librarian import', datetime('now'), 'Ultra Librarian import',
       'Exact MPN import: source footprint plus official TDK dimensions.'
FROM PackageDefinition p
WHERE p.PackageName = 'IND2424'
  AND NOT EXISTS (SELECT 1 FROM ComponentDefinition WHERE ManufacturerPartNumber = 'B82462G4472M000');

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id, 'IND_EPCOS_B82462G4_EPC', 2, 1.50, 2.40, 5.50,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/90bd52a6-1067-11e9-ab3a-0a3560a4cccc/TDK/B82462G4472M000',
       'KiCad v6+ / IND_EPCOS_B82462G4_EPC', 'Verified',
       'Exact KiCad export tagged B82462G4472M000. Pads are at +/-2.75 mm; paste reduction is not applied.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'B82462G4472M000'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);
