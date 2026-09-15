-- Diodes Incorporated SOT89 outline, suggested land pattern and 12 mm tape.
UPDATE PackageDefinition SET DisplayName='SOT89',StandardName='SOT89',PackageFamily='SOT',ComponentType='Transistor',
 Length=4.50,Width=4.10,Height=1.50,BodyLength=4.50,BodyWidth=2.50,LeadLength=1.05,LeadWidth=.48,Pitch=1.50,
 LeadCount=3,PadCount=3,PolarityMark='Pin 1',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOT89.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT89';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOT89','Generic','SOT89 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented SOT89 card.'
FROM PackageDefinition p WHERE p.PackageName='SOT89';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'SOT89','SOT89','Diodes suggested pad layout',3,.58,1.63,1.50,0,2,3,0,'Mixed','Diodes',DatasheetUrl,'','SOT89 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOT89';

UPDATE PackageFootprint SET PatternName='SOT89',StandardName='SOT89',Description='Diodes suggested pad layout',
 PadCount=3,PadLength=.58,PadWidth=1.63,PadPitch=1.50,RowCount=2,ColumnCount=3,PasteReduction=0,ApertureType='Mixed',
 SourceSystem='Diodes',SourceUrl='https://www.diodes.com/assets/Package-Files/SOT89.pdf',SourceVariant='SOT89 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOT89');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOT89',3,.58,1.63,1.50,'F.Paste','Diodes','https://www.diodes.com/assets/Package-Files/SOT89.pdf','SOT89 suggested pad layout','ManufacturerVerified',
 'Two 0.58 x 1.63 mm lands and one T-shaped centre land, 1.933 x 3.030 mm upper region with 0.760 mm stem.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT89';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT89-7','Diodes SOT89 package information','2026-01-16','EIA-481 / IEC 60286-3 embossed carrier tape',12,8,4.51,4.95,1.65,0,0,4,1.55,1.75,0,1,180,
 'Вывод 1: верхний правый при перфорации сверху и подаче вправо',180,178,0,1000,1,1,3,
 'W=12.00, P1=8.00, P0=4.00, P2=2.00, D0=1.55 mm. A0/B0/K0 are manufacturer-defined by movement limits; card uses the approved +10 percent fallback.',
 datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented tape orientation and calculated pocket clearance.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT89';

INSERT INTO MasterLibrary_PackageDocuments (PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackageOutline','Diodes-SOT89.pdf','Docs\Manufacturer\Diodes\SOT89\Diodes-SOT89.pdf','Diodes SOT89 package outline, suggested pad layout and tape orientation.'
FROM PackageDefinition p WHERE p.PackageName='SOT89'
AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Diodes-SOT89.pdf');
