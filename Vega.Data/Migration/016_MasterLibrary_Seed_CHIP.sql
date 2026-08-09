WITH seed(PackageName, ComponentType, Length, Width, Height, IPCName) AS (VALUES
('R0201','Resistor',0.60,0.30,0.23,'RESC0603X23'),('R0402','Resistor',1.00,0.50,0.35,'RESC1005X35'),('R0603','Resistor',1.60,0.80,0.55,'RESC1608X55'),('R0805','Resistor',2.00,1.25,0.55,'RESC2012X55'),('R1206','Resistor',3.20,1.60,0.55,'RESC3216X55'),
('C0201','Capacitor',0.60,0.30,0.23,'CAPC0603X23'),('C0402','Capacitor',1.00,0.50,0.55,'CAPC1005X55'),('C0603','Capacitor',1.60,0.80,0.80,'CAPC1608X80'),('C0805','Capacitor',2.00,1.25,0.85,'CAPC2012X85'),('C1206','Capacitor',3.20,1.60,0.85,'CAPC3216X85'),
('L0402','Inductor',1.00,0.50,0.55,'INDC1005X55'),('L0603','Inductor',1.60,0.80,0.80,'INDC1608X80'),('L0805','Inductor',2.00,1.25,1.00,'INDC2012X100'))
INSERT OR IGNORE INTO PackageDefinition(PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,IPCName,Description,PolarityMark,MirtecAoiClass,IsActive,CreatedAt,UpdatedAt)
SELECT s.PackageName,s.PackageName,s.PackageName,'CHIP',s.ComponentType,c.Id,f.Id,s.Length,s.Width,s.Height,s.IPCName,'Standard chip package','Pin 1 / polarity per component marking','CHIP',1,datetime('now'),datetime('now') FROM seed s JOIN PackageCategory c ON c.Code='PASSIVE' JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='CHIP';

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source) SELECT Id,'0603','Industry','Vega seed' FROM PackageDefinition WHERE PackageName='R0603';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source) SELECT Id,'1608','Industry','Vega seed' FROM PackageDefinition WHERE PackageName='R0603';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source) SELECT Id,'RES_0603','Industry','Vega seed' FROM PackageDefinition WHERE PackageName='R0603';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source) SELECT Id,'RESC1608','Industry','Vega seed' FROM PackageDefinition WHERE PackageName='R0603';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source) SELECT Id,'Chip0603','Industry','Vega seed' FROM PackageDefinition WHERE PackageName='R0603';