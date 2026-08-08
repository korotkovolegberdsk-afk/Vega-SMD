CREATE TABLE IF NOT EXISTS MasterLibrary_TechnologySources
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    SourceType TEXT NOT NULL,
    DocumentName TEXT NOT NULL DEFAULT '',
    DocumentRevision TEXT NOT NULL DEFAULT '',
    Reference TEXT NOT NULL DEFAULT '',
    Description TEXT NOT NULL DEFAULT ''
);

ALTER TABLE StencilTechnologyRule ADD COLUMN TechnologySourceId INTEGER;
ALTER TABLE StencilTechnologyRule ADD COLUMN ConfidenceLevel REAL NOT NULL DEFAULT 0;
ALTER TABLE StencilTechnologyRule ADD COLUMN ApplicationCondition TEXT NOT NULL DEFAULT '';
ALTER TABLE StencilTechnologyRule ADD COLUMN RecommendedBy TEXT NOT NULL DEFAULT '';
ALTER TABLE StencilTechnologyRule ADD COLUMN ProcessGoal TEXT NOT NULL DEFAULT '';

INSERT OR IGNORE INTO MasterLibrary_TechnologySources (Name, SourceType, DocumentName, DocumentRevision, Reference, Description) VALUES
('IPC-7525', 'IndustryStandard', 'IPC-7525', '', 'Stencil Design Guidelines', 'IPC stencil design recommendation.'),
('Indium', 'SolderPasteManufacturer', '', '', 'Manufacturer guideline', 'Indium solder-paste process guidance.'),
('Alpha', 'SolderPasteManufacturer', '', '', 'Manufacturer guideline', 'Alpha solder-paste process guidance.'),
('AIM', 'SolderPasteManufacturer', '', '', 'Manufacturer guideline', 'AIM solder-paste process guidance.'),
('Kester', 'SolderPasteManufacturer', '', '', 'Manufacturer guideline', 'Kester solder-paste process guidance.'),
('ASM', 'EquipmentManufacturer', '', '', 'Equipment guideline', 'ASM equipment process guidance.'),
('Yamaha', 'EquipmentManufacturer', '', '', 'Equipment guideline', 'Yamaha equipment process guidance.'),
('Mirtec', 'EquipmentManufacturer', '', '', 'Equipment guideline', 'Mirtec inspection process guidance.'),
('Internal SMT Experience', 'ProductionExperience', '', '', 'Internal SMT Technology Rule', 'Validated internal production experience.');

UPDATE StencilTechnologyRule
SET TechnologySourceId = (SELECT Id FROM MasterLibrary_TechnologySources WHERE Name = 'IPC-7525'),
    ConfidenceLevel = 0.80,
    RecommendedBy = 'IPC-7525',
    ProcessGoal = TechnologyGoal
WHERE Source LIKE '%IPC%';

UPDATE StencilTechnologyRule
SET TechnologySourceId = (SELECT Id FROM MasterLibrary_TechnologySources WHERE Name = 'Internal SMT Experience'),
    ConfidenceLevel = 0.90,
    RecommendedBy = 'Internal SMT Experience',
    ProcessGoal = TechnologyGoal
WHERE Source LIKE '%production%';

UPDATE StencilTechnologyRule
SET TechnologySourceId = (SELECT Id FROM MasterLibrary_TechnologySources WHERE Name = 'Indium'),
    ConfidenceLevel = 0.85,
    RecommendedBy = 'Indium',
    ProcessGoal = TechnologyGoal
WHERE PackageFamily = 'QFN' AND TechnologyGoal = 'VoidReduction';