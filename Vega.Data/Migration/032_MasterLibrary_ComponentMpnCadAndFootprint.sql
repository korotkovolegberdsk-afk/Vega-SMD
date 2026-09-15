-- Exact-MPN CAD and pad data. PackageFootprint remains a package-level fallback.
CREATE TABLE IF NOT EXISTS ComponentFootprint
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ComponentDefinitionId INTEGER NOT NULL UNIQUE,
    PatternName TEXT NOT NULL DEFAULT '',
    PadCount INTEGER NOT NULL DEFAULT 0,
    PadLength REAL NOT NULL DEFAULT 0,
    PadWidth REAL NOT NULL DEFAULT 0,
    PadPitch REAL NOT NULL DEFAULT 0,
    PasteLayer TEXT NOT NULL DEFAULT '',
    SourceSystem TEXT NOT NULL DEFAULT '',
    SourceUrl TEXT NOT NULL DEFAULT '',
    SourceVariant TEXT NOT NULL DEFAULT '',
    VerificationStatus TEXT NOT NULL DEFAULT '',
    Notes TEXT NOT NULL DEFAULT '',
    FOREIGN KEY(ComponentDefinitionId) REFERENCES ComponentDefinition(Id)
);

CREATE TABLE IF NOT EXISTS ComponentCadModel
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ComponentDefinitionId INTEGER NOT NULL UNIQUE,
    ModelPath TEXT NOT NULL DEFAULT '',
    FileSha256 TEXT NOT NULL DEFAULT '',
    Length REAL NOT NULL DEFAULT 0,
    Width REAL NOT NULL DEFAULT 0,
    Height REAL NOT NULL DEFAULT 0,
    SourceSystem TEXT NOT NULL DEFAULT '',
    SourceUrl TEXT NOT NULL DEFAULT '',
    VerificationStatus TEXT NOT NULL DEFAULT '',
    Notes TEXT NOT NULL DEFAULT '',
    FOREIGN KEY(ComponentDefinitionId) REFERENCES ComponentDefinition(Id)
);

-- MPN-specific, verified data from the received Ultra Librarian archive and TDK.
INSERT INTO ComponentDefinition
    (ManufacturerPartNumber, Manufacturer, Description, ComponentType, Value, Tolerance,
     PackageId, LifecycleStatus, DatasheetUrl, Notes, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT
    'MLG1005S56NJTD25', 'TDK',
    '56 nH ±5 %, automotive multilayer ceramic inductor, 0402 / 1005 metric',
    'Inductor', '56 nH', '±5 %',
    p.Id, 'Production',
    'https://product.tdk.com/en/search/inductor/inductor/smd/info?part_no=MLG1005S56NJTD25',
    'Exact MPN record. Tape pocket geometry remains unverified; do not substitute a generic 0402 tape profile.',
    1, datetime('now'), 'Ultra Librarian import', datetime('now'), 'Ultra Librarian import',
    'Exact MPN received from Ultra Librarian; manufacturer data checked against TDK.'
FROM PackageDefinition p
WHERE p.PackageName = 'IND0402'
  AND NOT EXISTS (SELECT 1 FROM ComponentDefinition WHERE ManufacturerPartNumber = 'MLG1005S56NJTD25');

INSERT INTO ComponentFootprint
    (ComponentDefinitionId, PatternName, PadCount, PadLength, PadWidth, PadPitch,
     PasteLayer, SourceSystem, SourceUrl, SourceVariant, VerificationStatus, Notes)
SELECT c.Id, 'IND_1005_TDK', 2, 0.57, 0.60, 1.13,
       'F.Paste', 'Ultra Librarian',
       'https://app.ultralibrarian.com/details/6c45e039-1073-11e9-ab3a-0a3560a4cccc/TDK/MLG1005S56NJTD25',
       'IND_1005_TDK', 'Verified',
       'Exact export tags contain MLG1005S56NJTD25. Values are source pad geometry; paste reduction is a separate process rule.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'MLG1005S56NJTD25'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId = c.Id);

INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\TDK\\MLG1005S56NJTD25\\IND_1005_TDK.step',
       '409AF3EF16EE2A22255259A869E08106F9C3B3826EC29737688120EA15E2E555',
       1.00, 0.50, 0.50,
       'Ultra Librarian / TDK',
       'https://product.tdk.com/en/search/inductor/inductor/smd/info?part_no=MLG1005S56NJTD25',
       'Verified',
       'STEP projection validated by Vega-SMD; TDK body dimensions are 1.00 × 0.50 × 0.50 mm.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'MLG1005S56NJTD25'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);
