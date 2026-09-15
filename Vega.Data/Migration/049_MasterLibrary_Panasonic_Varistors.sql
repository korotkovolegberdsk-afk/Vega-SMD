-- Panasonic multilayer SMD varistors from the official EZJ-P product pages.
-- Mechanical height and drawings remain unset until the corresponding Panasonic
-- engineering drawing is imported; no guessed dimensions are written here.

INSERT INTO PackageFamily(CategoryId,Code,Name,Description,SortOrder,IsActive,CreatedAt,UpdatedAt)
SELECT c.Id,'VARISTOR','VARISTOR','Panasonic multilayer SMD varistor packages',65,1,datetime('now'),datetime('now')
FROM PackageCategory c
WHERE c.Code='PASSIVE'
  AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.Code='VARISTOR');

CREATE TEMP TABLE IF NOT EXISTS PanasonicVaristorPackages(
    PackageName TEXT, Length REAL, Width REAL, Metric TEXT);
DELETE FROM PanasonicVaristorPackages;
INSERT INTO PanasonicVaristorPackages VALUES
('VAR0201',0.60,0.30,'0201'),
('VAR0402',1.00,0.50,'0402'),
('VAR0603',1.60,0.80,'0603'),
('VAR0805',2.00,1.25,'0805');

INSERT OR IGNORE INTO PackageDefinition(
    PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
    Length,Width,Height,BodyLength,BodyWidth,PadCount,LeadCount,IPCName,JEDECName,
    MirtecAoiClass,Description,DatasheetUrl,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT s.PackageName,s.PackageName,s.Metric,'VARISTOR','Varistor',c.Id,f.Id,
       s.Length,s.Width,0,s.Length,s.Width,2,0,'CHIP',s.Metric,
       'CHIP', 'Panasonic multilayer SMD varistor package reference',
       'https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors',
       'Body dimensions from Panasonic product family; import the selected part engineering drawing before marking verified.',
       1,datetime('now'),datetime('now')
FROM PanasonicVaristorPackages s
JOIN PackageCategory c ON c.Code='PASSIVE'
JOIN PackageFamily f ON f.Code='VARISTOR';

INSERT INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT p.Id,s.Metric,'EIA/JIS','Panasonic EZJ-P'
FROM PackageDefinition p JOIN PanasonicVaristorPackages s ON s.PackageName=p.PackageName
WHERE NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageAliases a WHERE a.PackageId=p.Id AND lower(a.Alias)=lower(s.Metric));

CREATE TEMP TABLE IF NOT EXISTS PanasonicVaristorComponents(
    PartNumber TEXT, PackageName TEXT, VaristorVoltage TEXT, MaxVoltage TEXT, Url TEXT);
DELETE FROM PanasonicVaristorComponents;
INSERT INTO PanasonicVaristorComponents VALUES
('EZJ-PZV080GA','VAR0201','8.0 V','5.6 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468/model/72063'),
('EZJ-PZV120GA','VAR0201','12 V','7.5 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468/model/72066'),
('EZJP0V270GA','VAR0402','27 V','16 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468'),
('EZJP0V420WA','VAR0402','42 V','30 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468'),
('EZJP1V270GA','VAR0603','27 V','16 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468'),
('EZJP1V420FA','VAR0603','42 V','30 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468'),
('EZJS2VB223','VAR0805','8 to 15 V','6 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70482'),
('EZJS2YD472','VAR0805','40 to 60 V','30 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70482'),
('EZJS2YC822','VAR0805','24 to 35 V','18 V','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70482');

INSERT OR IGNORE INTO ComponentDefinition(
    ManufacturerPartNumber,Manufacturer,Description,ComponentType,Value,VoltageRating,
    PackageId,DatasheetUrl,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT v.PartNumber,'Panasonic','Multilayer SMD varistor - '||v.PackageName,'Varistor',v.VaristorVoltage,
       v.MaxVoltage,p.Id,v.Url,
       'Official Panasonic product data. Packaging and electrical values are from the Panasonic series page; verify the selected engineering drawing before production use.',
       1,datetime('now'),datetime('now')
FROM PanasonicVaristorComponents v
JOIN PackageDefinition p ON p.PackageName=v.PackageName;

INSERT INTO MasterLibrary_PackageDocuments(PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackageOutline','Panasonic-EZJ-P-Engineering-Drawings.url','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468',
       'Panasonic EZJ-P series product page with engineering-drawing links.'
FROM PackageDefinition p
WHERE p.PackageFamily='VARISTOR'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Panasonic-EZJ-P-Engineering-Drawings.url');

INSERT INTO MasterLibrary_PackageDocuments(PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackaging','Panasonic-EZJ-P-Taping.url','https://na.industrial.panasonic.com/products/circuit-protection/non-linear-resistors/lineup/multilayer-smd-varistors/series/70468',
       'Panasonic series packaging data: paper taping, 2 mm or 4 mm pitch according to part number.'
FROM PackageDefinition p
WHERE p.PackageFamily='VARISTOR'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Panasonic-EZJ-P-Taping.url');

DROP TABLE PanasonicVaristorPackages;
DROP TABLE PanasonicVaristorComponents;
