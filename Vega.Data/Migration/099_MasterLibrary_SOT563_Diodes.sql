-- Diodes Incorporated SOT563 package outline, suggested lands and 8 mm carrier tape.
UPDATE PackageDefinition SET DisplayName='SOT563',StandardName='SOT563',PackageFamily='SOT',ComponentType='Transistor',
 Length=1.60,Width=1.60,Height=.575,BodyLength=1.60,BodyWidth=1.20,LeadLength=.20,LeadWidth=.20,Pitch=.50,
 LeadCount=6,PadCount=6,PolarityMark='Pin 1',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOT563.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT563';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOT563','Generic','SOT563 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented SOT563 card.'
FROM PackageDefinition p WHERE p.PackageName='SOT563';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'SOT563','SOT563','Diodes suggested pad layout',6,.30,.67,.50,0,2,2,0,'Rectangle','Diodes',DatasheetUrl,'','SOT563 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOT563';

UPDATE PackageFootprint SET PatternName='SOT563',StandardName='SOT563',Description='Diodes suggested pad layout',
 PadCount=6,PadLength=.30,PadWidth=.67,PadPitch=.50,RowCount=2,ColumnCount=2,PasteReduction=0,ApertureType='Rectangle',
 SourceSystem='Diodes',SourceUrl='https://www.diodes.com/assets/Package-Files/SOT563.pdf',SourceVariant='SOT563 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOT563');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOT563',6,.30,.67,.50,'F.Paste','Diodes','https://www.diodes.com/assets/Package-Files/SOT563.pdf','SOT563 suggested pad layout','ManufacturerVerified',
 'Six 0.30 x 0.67 mm lands; row centre spacing 1.27 mm. Height 0.575 mm is the midpoint of the documented 0.55-0.60 mm range.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT563';


-- The manufacturer specifies W/P/P0/P2/D0/E/F and device orientation, but
-- defines A0/B0/K0 by the component movement requirement rather than numbers.
-- Vega fallback: +10 percent for this small package.
INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT563-7','Diodes SOT563 package information','2021-01-06','EIA-481 / IEC 60286-3 embossed carrier tape',8,4,1.76,1.76,.64,0,0,4,1.55,1.75,0,1,0,
 'Вывод 1: нижний левый; по три вывода с каждой стороны',0,178,0,3000,1,1,3,
 'W=8.00, P1=4.00, P0=4.00, P2=2.00, D0=1.55 мм. A0=B0=1.76 и K0=0.64 мм рассчитаны по правилу +10%; ориентация документирована.',
 datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented tape orientation with calculated pocket clearance.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT563';

INSERT INTO MasterLibrary_PackageDocuments (PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackageOutline','Diodes-SOT563.pdf','Docs\Manufacturer\Diodes\SOT563\Diodes-SOT563.pdf','Diodes SOT563 package outline, suggested pad layout and tape orientation.'
FROM PackageDefinition p WHERE p.PackageName='SOT563'
AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Diodes-SOT563.pdf');
