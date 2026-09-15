-- Standard multilayer chip-inductor body sizes.
-- Nominal dimensions are package references; the manufacturer series drawing remains authoritative.
CREATE TEMP TABLE IF NOT EXISTS InductorSeed(
    PackageName TEXT, Length REAL, Width REAL, Height REAL, Metric TEXT);
DELETE FROM InductorSeed;
INSERT INTO InductorSeed VALUES
('IND01005',0.40,0.20,0.15,'0402'),
('IND0201',0.60,0.30,0.30,'0603'),
('IND0402',1.00,0.50,0.55,'1005'),
('IND0603',1.60,0.80,0.80,'1608'),
('IND0805',2.00,1.25,1.00,'2012'),
('IND1008',2.50,2.00,1.00,'2520'),
('IND1206',3.20,1.60,1.00,'3216'),
('IND1210',3.20,2.50,1.00,'3225'),
('IND1812',4.50,3.20,1.20,'4532'),
('IND2010',5.00,2.50,1.20,'5025'),
('IND2220',5.70,5.00,1.50,'5750'),
('IND2512',6.30,3.20,1.50,'6332');

INSERT OR IGNORE INTO PackageDefinition(
    PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
    Length,Width,Height,BodyLength,BodyWidth,PadCount,LeadCount,IPCName,JEDECName,
    MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT s.PackageName,s.PackageName,s.PackageName,'INDUCTOR','Inductor',c.Id,f.Id,
       s.Length,s.Width,s.Height,s.Length,s.Width,2,0,'INDUCTOR',s.Metric,
       'INDUCTOR','Standard chip inductor package',
       'Nominal EIA/JIS body dimensions; confirm height and land pattern against the selected manufacturer series.',
       1,datetime('now'),datetime('now')
FROM InductorSeed s
JOIN PackageCategory c ON c.Code='PASSIVE'
JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='INDUCTOR';

INSERT INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT p.Id,'L'||substr(p.PackageName,4),'Footprint','INDUCTOR seed 041'
FROM PackageDefinition p
WHERE p.PackageName IN (SELECT PackageName FROM InductorSeed)
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageAliases a WHERE a.PackageId=p.Id AND lower(a.Alias)=lower('L'||substr(p.PackageName,4)));

INSERT INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT p.Id,'IND-'||substr(p.PackageName,4),'Standard','INDUCTOR seed 041'
FROM PackageDefinition p
WHERE p.PackageName IN (SELECT PackageName FROM InductorSeed)
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageAliases a WHERE a.PackageId=p.Id AND lower(a.Alias)=lower('IND-'||substr(p.PackageName,4)));

DROP TABLE InductorSeed;
