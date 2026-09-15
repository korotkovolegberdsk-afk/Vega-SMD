-- Exact-MPN footprint imported from the Ultra Librarian KiCAD v6+ archive.
-- No STEP model or Tape & Reel source was present in that archive.
INSERT INTO ComponentDefinition
    (ManufacturerPartNumber, Manufacturer, Description, ComponentType, Value, Tolerance,
     PackageId, LifecycleStatus, DatasheetUrl, Notes, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT
    'C2220C100KDGACTU', 'KEMET',
    '10 pF ±10 %, 1000 VDC C0G commercial-grade MLCC, 2220',
    'Capacitor', '10 pF', '±10 %',
    p.Id, 'Production',
    'https://www.kemet.com/en/us/capacitors/ceramic-capacitors/surface-mount-ceramic-capacitors/c0g-mlccs.html',
    'Exact MPN footprint is verified from the received Ultra Librarian KiCAD v6+ archive. STEP and tape data are intentionally left unverified until separate source files are received.',
    1, datetime('now'), 'Ultra Librarian import', datetime('now'), 'Ultra Librarian import',
    'Imported exact KiCAD footprint CAPC610540_180N_KEM from Ultra Librarian.'
FROM PackageDefinition p
WHERE p.PackageName = 'C2220'
  AND NOT EXISTS (SELECT 1 FROM ComponentDefinition WHERE ManufacturerPartNumber = 'C2220C100KDGACTU');

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id,
       'CAPC610540_180N_KEM', 2, 1.699997, 5.399990, 5.099990,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/236988d6-6e06-11ea-8c00-0ad2c9526b44/Kemet/C2220C100KDGACTU',
       'KiCAD v6+ / CAPC610540_180N_KEM', 'Verified',
       'Exact export tags contain C2220C100KDGACTU. Values are source pad geometry; no paste reduction has been applied.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'C2220C100KDGACTU'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);
