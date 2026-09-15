-- Nexperia package outlines, reflow lands and EIA-481 packing data for the SC-70/SC-88 group.
UPDATE PackageDefinition SET DisplayName='SOT323',StandardName='SOT323 / SC-70',PackageFamily='SOT',ComponentType='Transistor',
 Length=2.00,Width=2.10,Height=.95,BodyLength=2.00,BodyWidth=1.25,LeadLength=.30,LeadWidth=.35,Pitch=.65,
 LeadCount=3,PadCount=3,PolarityMark='Pin 1',DatasheetUrl='https://assets.nexperia.com/documents/package-information/SOT323.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT323';

UPDATE PackageDefinition SET DisplayName='SOT353',StandardName='SOT353 / TSSOP5 / SC-88A',PackageFamily='SOT',ComponentType='Transistor',
 Length=2.00,Width=2.10,Height=.95,BodyLength=2.00,BodyWidth=1.25,LeadLength=.30,LeadWidth=.25,Pitch=.65,
 LeadCount=5,PadCount=5,PolarityMark='Pin 1',DatasheetUrl='https://assets.nexperia.com/documents/package-information/SOT353.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT353';

UPDATE PackageDefinition SET DisplayName='SOT363',StandardName='SOT363 / TSSOP6 / SC-88',PackageFamily='SOT',ComponentType='Transistor',
 Length=2.00,Width=2.10,Height=.95,BodyLength=2.00,BodyWidth=1.25,LeadLength=.30,LeadWidth=.25,Pitch=.65,
 LeadCount=6,PadCount=6,PolarityMark='Pin 1',DatasheetUrl='https://assets.nexperia.com/documents/package-information/SOT363.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT363';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Generic',p.StandardName,'Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added documented SOT micro-package card.'
FROM PackageDefinition p WHERE p.PackageName IN ('SOT323','SOT353','SOT363');

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,PackageName,StandardName,'Nexperia reflow soldering footprint',LeadCount,.60,.60,.65,0,2,3,0,'Rectangle','Nexperia',DatasheetUrl,'',PackageName||' reflow soldering footprint','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName IN ('SOT323','SOT353','SOT363');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,f.PatternName,f.PadCount,f.PadLength,f.PadWidth,f.PadPitch,'F.Paste',f.SourceSystem,f.SourceUrl,f.SourceVariant,f.VerificationStatus,
 CASE WHEN c.ManufacturerPartNumber IN ('SOT353','SOT363') THEN 'Four 0.60 x 0.60 mm outer lands; centre land(s) 0.60 x 0.40 mm.' ELSE 'Three 0.60 x 0.60 mm lands.' END
FROM ComponentDefinition c JOIN PackageFootprint f ON f.PackageId=c.PackageId
WHERE c.ManufacturerPartNumber IN ('SOT323','SOT353','SOT363');

-- Renderer convention: PocketWidth is horizontal A0; PocketLength is vertical B0.
UPDATE ComponentTapeReelGeometry SET PocketWidth=3.15,PocketLength=3.20,UpdatedAt=datetime('now'),
 Notes='A0=3.15 по горизонтали, B0=3.20 по вертикали, K0=1.40, P1=4.00, W=8.00 мм.'
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber IN ('SOT25','SOT26'));

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT323_115','Nexperia SOT323_115 packing information','2022-05-27','EIA-481 embossed carrier tape',8,4,2.60,2.40,1.20,0,0,4,1.50,0,0,1,0,'Вывод 1: Q3, нижний левый угол',0,180,0,3000,1,1,1,'A0=2.40 по горизонтали, B0=2.60 по вертикали, K0=1.20, P1=4.00, W=8.00 мм.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added manufacturer-verified EIA-481 tape profile.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT323';

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,c.ManufacturerPartNumber||'_115','Nexperia '||c.ManufacturerPartNumber||'_115 packing information','2026','EIA-481 embossed carrier tape',8,4,2.25,2.25,1.22,0,0,4,1.50,0,0,1,2,'Вывод 1: Q2, верхний правый угол',180,180,0,3000,1,1,1,'A0=2.25, B0=2.25, K0=1.22, P1=4.00, W=8.00 мм; ориентация Q2/T3.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added manufacturer-verified EIA-481 tape profile.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber IN ('SOT353','SOT363');
