-- Diodes Incorporated SOT223 outline, suggested land pattern and 12 mm tape.
UPDATE PackageDefinition SET DisplayName='SOT223',StandardName='SOT223',PackageFamily='SOT',ComponentType='Transistor',
 Length=6.50,Width=7.00,Height=1.60,BodyLength=6.50,BodyWidth=3.50,LeadLength=.95,LeadWidth=.70,Pitch=2.30,
 LeadCount=4,PadCount=4,PolarityMark='Pin 1',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOT223.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT223';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOT223','Generic','SOT223 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented SOT223 card.'
FROM PackageDefinition p WHERE p.PackageName='SOT223';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'SOT223','SOT223','Diodes suggested pad layout',4,1.20,1.60,2.30,0,2,3,0,'Mixed','Diodes',DatasheetUrl,'','SOT223 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOT223';

UPDATE PackageFootprint SET PatternName='SOT223',StandardName='SOT223',Description='Diodes suggested pad layout',
 PadCount=4,PadLength=1.20,PadWidth=1.60,PadPitch=2.30,RowCount=2,ColumnCount=3,PasteReduction=0,ApertureType='Mixed',
 SourceSystem='Diodes',SourceUrl='https://www.diodes.com/assets/Package-Files/SOT223.pdf',SourceVariant='SOT223 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOT223');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOT223',4,1.20,1.60,2.30,'F.Paste','Diodes','https://www.diodes.com/assets/Package-Files/SOT223.pdf','SOT223 suggested pad layout','ManufacturerVerified',
 'Three 1.20 x 1.60 mm lead lands and one 3.30 x 1.60 mm heat-tab land; row centre spacing 6.40 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT223';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT223-TA','Diodes SOT223 package information','2017-03-06','EIA-481 / IEC 60286-3 embossed carrier tape',12,8,7.70,7.15,1.76,0,0,4,1.50,1.75,0,1,0,
 'Вывод 1: нижний левый при перфорации сверху и подаче вправо',0,178,0,1000,1,1,3,
 'W=12.00, P1=8.00, P0=4.00, P2=2.00, D0=1.50 mm. A0/B0/K0 are manufacturer-defined by movement limits; card uses the approved +10 percent fallback.',
 datetime('now'),'Diodes package data',datetime('now'),'Diodes package data','Added documented tape orientation and calculated pocket clearance.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT223';

INSERT INTO MasterLibrary_PackageDocuments (PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackageOutline','Diodes-SOT223.pdf','Docs\Manufacturer\Diodes\SOT223\Diodes-SOT223.pdf','Diodes SOT223 package outline, suggested pad layout and tape orientation.'
FROM PackageDefinition p WHERE p.PackageName='SOT223'
AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Diodes-SOT223.pdf');
