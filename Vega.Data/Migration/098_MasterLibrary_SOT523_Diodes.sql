-- Diodes Incorporated SOT523 package outline, suggested lands and 8 mm carrier tape.
UPDATE PackageDefinition SET DisplayName='SOT523',StandardName='SOT523',PackageFamily='SOT',ComponentType='Transistor',
 Length=1.60,Width=1.60,Height=.75,BodyLength=1.60,BodyWidth=.80,LeadLength=.33,LeadWidth=.22,Pitch=.50,
 LeadCount=3,PadCount=3,PolarityMark='Pin 1',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOT523.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT523';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOT523','Generic','SOT523 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented SOT523 card.'
FROM PackageDefinition p WHERE p.PackageName='SOT523';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'SOT523','SOT523','Diodes suggested pad layout',3,.40,.51,.50,0,2,2,0,'Rectangle','Diodes',DatasheetUrl,'','SOT523 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOT523';

UPDATE PackageFootprint SET PatternName='SOT523',StandardName='SOT523',Description='Diodes suggested pad layout',
 PadCount=3,PadLength=.40,PadWidth=.51,PadPitch=.50,RowCount=2,ColumnCount=2,PasteReduction=0,ApertureType='Rectangle',
 SourceSystem='Diodes',SourceUrl='https://www.diodes.com/assets/Package-Files/SOT523.pdf',SourceVariant='SOT523 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOT523');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOT523',3,.40,.51,.50,'F.Paste','Diodes','https://www.diodes.com/assets/Package-Files/SOT523.pdf','SOT523 suggested pad layout','ManufacturerVerified',
 'Three 0.40 x 0.51 mm lands; row centre spacing C=1.29 mm; overall pattern height Y1=1.80 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT523';

DELETE FROM ComponentTapeReelGeometry
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOT523');

-- The manufacturer specifies W/P/P0/P2/D0/E/F and device orientation, but
-- defines A0/B0/K0 by the component movement requirement rather than numbers.
-- Vega fallback: +10 percent for this small package.
INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT523-7','Diodes SOT523 package information','2018-04-09','EIA-481 / IEC 60286-3 embossed carrier tape',8,4,1.76,1.76,.83,0,0,4,1.50,1.75,0,1,0,
 'Вывод 1: нижний левый; один вывод к перфорации, два от перфорации',0,178,0,3000,1,1,3,
 'W=8.00, P1=4.00, P0=4.00, P2=2.00, D0=1.50 мм. A0=B0=1.76 и K0=0.83 мм рассчитаны по правилу +10%; ориентация документирована.',
 datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented tape orientation with calculated pocket clearance.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT523';

INSERT INTO MasterLibrary_PackageDocuments (PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackageOutline','Diodes-SOT523.pdf','Docs\Manufacturer\Diodes\SOT523\Diodes-SOT523.pdf','Diodes SOT523 package outline, suggested pad layout and tape orientation.'
FROM PackageDefinition p WHERE p.PackageName='SOT523'
AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Diodes-SOT523.pdf');
