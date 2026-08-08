ALTER TABLE PackageDefinition ADD COLUMN StandardName TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN PackageFamily TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN ComponentType TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN Manufacturer TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN ManufacturerPartNumber TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN DrawingFile TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN Model3DFile TEXT NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS MasterLibrary_PackageDocuments
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageId INTEGER NOT NULL,
    DocumentType TEXT NOT NULL,
    FileName TEXT NOT NULL DEFAULT '',
    FilePath TEXT NOT NULL DEFAULT '',
    Description TEXT NOT NULL DEFAULT '',
    FOREIGN KEY(PackageId) REFERENCES PackageDefinition(Id)
);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_PackageDocuments_PackageId ON MasterLibrary_PackageDocuments(PackageId);

ALTER TABLE StencilTechnologyRule ADD COLUMN StencilThicknessMin REAL NOT NULL DEFAULT 0;
ALTER TABLE StencilTechnologyRule ADD COLUMN StencilThicknessMax REAL NOT NULL DEFAULT 0;
ALTER TABLE StencilTechnologyRule ADD COLUMN PreferredReductionX REAL NOT NULL DEFAULT 0;
ALTER TABLE StencilTechnologyRule ADD COLUMN PreferredReductionY REAL NOT NULL DEFAULT 0;
ALTER TABLE StencilTechnologyRule ADD COLUMN SourceReference TEXT NOT NULL DEFAULT '';

UPDATE StencilTechnologyRule
SET StencilThicknessMin = CASE WHEN StencilThicknessMin = 0 THEN RecommendedThickness ELSE StencilThicknessMin END,
    StencilThicknessMax = CASE WHEN StencilThicknessMax = 0 THEN RecommendedThickness ELSE StencilThicknessMax END,
    PreferredReductionX = CASE WHEN PreferredReductionX = 0 THEN ReductionX ELSE PreferredReductionX END,
    PreferredReductionY = CASE WHEN PreferredReductionY = 0 THEN ReductionY ELSE PreferredReductionY END,
    SourceReference = CASE WHEN SourceReference = '' THEN DocumentReference ELSE SourceReference END;

INSERT INTO PackageCategory (Code, Name)
SELECT 'CHIP', 'Chip Components'
WHERE NOT EXISTS (SELECT 1 FROM PackageCategory WHERE Code = 'CHIP');
INSERT INTO PackageCategory (Code, Name)
SELECT 'IC', 'Integrated Circuits'
WHERE NOT EXISTS (SELECT 1 FROM PackageCategory WHERE Code = 'IC');
INSERT INTO PackageCategory (Code, Name)
SELECT 'MELF', 'MELF Components'
WHERE NOT EXISTS (SELECT 1 FROM PackageCategory WHERE Code = 'MELF');

INSERT INTO PackageFamily (CategoryId, Code, Name)
SELECT c.Id, 'CHIP', 'Chip Components' FROM PackageCategory c
WHERE c.Code = 'CHIP' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = c.Id AND f.Code = 'CHIP');
INSERT INTO PackageFamily (CategoryId, Code, Name)
SELECT c.Id, 'SOIC', 'Small Outline IC' FROM PackageCategory c
WHERE c.Code = 'IC' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = c.Id AND f.Code = 'SOIC');
INSERT INTO PackageFamily (CategoryId, Code, Name)
SELECT c.Id, 'QFP', 'Quad Flat Package' FROM PackageCategory c
WHERE c.Code = 'IC' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = c.Id AND f.Code = 'QFP');
INSERT INTO PackageFamily (CategoryId, Code, Name)
SELECT c.Id, 'QFN', 'Quad Flat No-lead' FROM PackageCategory c
WHERE c.Code = 'IC' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = c.Id AND f.Code = 'QFN');
INSERT INTO PackageFamily (CategoryId, Code, Name)
SELECT c.Id, 'BGA', 'Ball Grid Array' FROM PackageCategory c
WHERE c.Code = 'IC' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = c.Id AND f.Code = 'BGA');
INSERT INTO PackageFamily (CategoryId, Code, Name)
SELECT c.Id, 'MELF', 'MELF' FROM PackageCategory c
WHERE c.Code = 'MELF' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = c.Id AND f.Code = 'MELF');

WITH v(PackageName, StandardName, PackageFamily, ComponentType) AS
(
    VALUES
    ('R0201','R0201','CHIP','Resistor'), ('R0402','R0402','CHIP','Resistor'), ('R0603','R0603','CHIP','Resistor'), ('R0805','R0805','CHIP','Resistor'), ('R1206','R1206','CHIP','Resistor'),
    ('C0201','C0201','CHIP','Capacitor'), ('C0402','C0402','CHIP','Capacitor'), ('C0603','C0603','CHIP','Capacitor'), ('C0805','C0805','CHIP','Capacitor'), ('C1206','C1206','CHIP','Capacitor'),
    ('L0402','L0402','CHIP','Inductor'), ('L0603','L0603','CHIP','Inductor'), ('L0805','L0805','CHIP','Inductor'),
    ('SO08','SO08','SOIC','IC'), ('SO14','SO14','SOIC','IC'), ('SO16','SO16','SOIC','IC'), ('TSSOP','TSSOP','TSSOP','IC'),
    ('QFP','QFP','QFP','IC'), ('QFN','QFN','QFN','IC'), ('BGA','BGA','BGA','IC'), ('MELF','MELF','MELF','Diode')
)
INSERT INTO PackageDefinition (PackageName, DisplayName, StandardName, PackageFamily, ComponentType, CategoryId, FamilyId, IsActive, CreatedAt, UpdatedAt)
SELECT v.PackageName, v.PackageName, v.StandardName, v.PackageFamily, v.ComponentType, c.Id, f.Id, 1, datetime('now'), datetime('now')
FROM v
JOIN PackageCategory c ON c.Code = CASE WHEN v.PackageFamily = 'CHIP' THEN 'CHIP' WHEN v.PackageFamily = 'MELF' THEN 'MELF' ELSE 'IC' END
JOIN PackageFamily f ON f.CategoryId = c.Id AND f.Code = CASE WHEN v.PackageFamily IN ('SOIC', 'TSSOP') THEN 'SOIC' ELSE v.PackageFamily END
WHERE NOT EXISTS (SELECT 1 FROM PackageDefinition p WHERE p.PackageName = v.PackageName);
INSERT INTO StencilTechnologyRule
(PackageFamily, PackageName, ComponentType, TechnologyGoal, PreferredShape, AlternativeShape, RecommendedThickness, ReductionX, ReductionY, PreferredReductionX, PreferredReductionY, StencilThicknessMin, StencilThicknessMax, MinAreaRatio, MinAspectRatio, Coverage, Source, SourceReference, DocumentReference, TechnologyReason, Priority, IsActive)
SELECT 'CHIP', 'R0603', 'Resistor', 'StandardPasteRelease', 'Rectangle', 'Snubnose', 0.12, 10, 10, 10, 10, 0.10, 0.15, 0.66, 1.5, 100, 'IPC recommendation', 'IPC-7525', 'Internal SMT Technology Rule', 'Balanced paste release', 300, 1
WHERE NOT EXISTS (SELECT 1 FROM StencilTechnologyRule WHERE PackageName = 'R0603' AND TechnologyGoal = 'StandardPasteRelease');
INSERT INTO StencilTechnologyRule
(PackageFamily, PackageName, ComponentType, TechnologyGoal, PreferredShape, AlternativeShape, RecommendedThickness, ReductionX, ReductionY, PreferredReductionX, PreferredReductionY, StencilThicknessMin, StencilThicknessMax, MinAreaRatio, MinAspectRatio, Coverage, Source, SourceReference, DocumentReference, TechnologyReason, Priority, IsActive)
SELECT 'CHIP', 'R0603', 'Resistor', 'AntiSolderBall', 'Snubnose', 'Rectangle', 0.12, 10, 10, 10, 10, 0.10, 0.15, 0.66, 1.5, 100, 'Internal production experience', 'Internal SMT Technology Rule', 'Internal SMT Technology Rule', 'Solder ball prevention', 400, 1
WHERE NOT EXISTS (SELECT 1 FROM StencilTechnologyRule WHERE PackageName = 'R0603' AND TechnologyGoal = 'AntiSolderBall');
UPDATE StencilTechnologyRule SET PreferredShape = 'WindowPane', AlternativeShape = 'Array', Coverage = 60, StencilThicknessMin = 0.08, StencilThicknessMax = 0.12, SourceReference = 'IPC-7525' WHERE PackageFamily = 'QFN' AND TechnologyGoal = 'VoidReduction';
UPDATE StencilTechnologyRule SET PreferredShape = 'HomePlate', SourceReference = 'IPC-7525' WHERE PackageFamily = 'IC' AND PackageName = 'QFP' AND TechnologyGoal = 'FinePitch';