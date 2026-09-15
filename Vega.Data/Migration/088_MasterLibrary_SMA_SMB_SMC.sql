-- Canonical DO-214 diode packages from Diodes Incorporated package sheets.
UPDATE PackageDefinition SET ComponentType='Diode',Length=5.195,Width=2.605,Height=2.18,
 BodyLength=4.30,BodyWidth=2.605,LeadLength=1.14,LeadWidth=1.45,LeadCount=2,PadCount=2,
 PolarityMark='Cathode Band',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SMA.pdf',UpdatedAt=datetime('now') WHERE PackageName='SMA';
UPDATE PackageDefinition SET ComponentType='Diode',Length=5.295,Width=3.62,Height=2.25,
 BodyLength=4.315,BodyWidth=3.62,LeadLength=1.14,LeadWidth=2.085,LeadCount=2,PadCount=2,
 PolarityMark='Cathode Band',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SMB.pdf',UpdatedAt=datetime('now') WHERE PackageName='SMB';
UPDATE PackageDefinition SET ComponentType='Diode',Length=7.94,Width=5.905,Height=2.25,
 BodyLength=6.855,BodyWidth=5.905,LeadLength=1.14,LeadWidth=2.965,LeadCount=2,PadCount=2,
 PolarityMark='Cathode Band',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SMC.pdf',UpdatedAt=datetime('now') WHERE PackageName='SMC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Generic','Canonical DO-214 SMD diode package','Diode',p.Id,'Reference',
 'Package-level reference; electrical characteristics depend on exact MPN.',1,datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added unique DO-214 package card.'
FROM PackageDefinition p WHERE p.PackageName IN ('SMA','SMB','SMC');

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_SMA','SMA','Suggested pad layout',2,2.50,1.70,4.00,0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SMA.pdf','','SMA suggested pad layout','ManufacturerVerified' FROM PackageDefinition WHERE PackageName='SMA';
INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_SMB','SMB','Suggested pad layout',2,2.50,2.30,4.30,0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SMB.pdf','','SMB suggested pad layout','ManufacturerVerified' FROM PackageDefinition WHERE PackageName='SMB';
INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_SMC','SMC','Suggested pad layout',2,2.50,3.30,6.90,0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SMC.pdf','','SMC suggested pad layout','ManufacturerVerified' FROM PackageDefinition WHERE PackageName='SMC';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,f.PatternName,f.PadCount,f.PadLength,f.PadWidth,f.PadPitch,'F.Paste',f.SourceSystem,f.SourceUrl,f.SourceVariant,f.VerificationStatus,
 'Documented package land pattern; paste reduction is calculated separately.'
FROM ComponentDefinition c JOIN PackageFootprint f ON f.PackageId=c.PackageId
WHERE c.ManufacturerPartNumber IN ('SMA','SMB','SMC');

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,c.ManufacturerPartNumber||' package tape','Diodes Incorporated package information',
 CASE c.ManufacturerPartNumber WHEN 'SMB' THEN '2019-08-30' ELSE '2017-03-14' END,'Embossed Carrier Tape',
 CASE c.ManufacturerPartNumber WHEN 'SMC' THEN 16 ELSE 12 END,
 CASE c.ManufacturerPartNumber WHEN 'SMA' THEN 4 ELSE 8 END,
 CASE c.ManufacturerPartNumber WHEN 'SMA' THEN 5.9 WHEN 'SMB' THEN 5.9 ELSE 8.5 END,
 CASE c.ManufacturerPartNumber WHEN 'SMA' THEN 3.2 WHEN 'SMB' THEN 4.3 ELSE 6.6 END,
 CASE c.ManufacturerPartNumber WHEN 'SMA' THEN 2.6 WHEN 'SMB' THEN 2.7 ELSE 3.5 END,
 0,0,4,1.5,0,0,1,0,'Диод расположен вертикально; катодная полоса сохраняет одну ориентацию',0,330,100,
 CASE c.ManufacturerPartNumber WHEN 'SMA' THEN 5000 ELSE 3000 END,1,1,3,
 'W, P, P0 и D документированы; A0/B0/K0 не заданы и для изображения рассчитаны с запасом.',
 datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added one tape profile per unique DO-214 outline.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber IN ('SMA','SMB','SMC');
