-- JSCJ Incorporated SOT723 package outline, suggested lands and 8 mm carrier tape.
UPDATE PackageDefinition SET DisplayName='SOT723',StandardName='SOT723',PackageFamily='SOT',ComponentType='Transistor',
 Length=1.20,Width=1.20,Height=.36,BodyLength=1.20,BodyWidth=.80,LeadLength=.20,LeadWidth=.22,Pitch=.40,
 LeadCount=3,PadCount=3,PolarityMark='Pin 1',DatasheetUrl='https://www.jscj-elec.com/uploads/pdf/20221018/CJ3134KT%20SOT-723%20V1.0.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT723';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOT723','Generic','SOT723 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'JSCJ package data',datetime('now'),'JSCJ package data','Added documented SOT723 card.'
FROM PackageDefinition p WHERE p.PackageName='SOT723';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'SOT723','SOT723','JSCJ suggested pad layout',3,.32,.30,.40,0,2,2,0,'Rectangle','JSCJ',DatasheetUrl,'','SOT723 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOT723';

UPDATE PackageFootprint SET PatternName='SOT723',StandardName='SOT723',Description='JSCJ suggested pad layout',
 PadCount=3,PadLength=.32,PadWidth=.30,PadPitch=.40,RowCount=2,ColumnCount=2,PasteReduction=0,ApertureType='Rectangle',
 SourceSystem='JSCJ',SourceUrl='https://www.jscj-elec.com/uploads/pdf/20221018/CJ3134KT%20SOT-723%20V1.0.pdf',SourceVariant='SOT723 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOT723');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOT723',3,.32,.30,.40,'F.Paste','JSCJ','https://www.jscj-elec.com/uploads/pdf/20221018/CJ3134KT%20SOT-723%20V1.0.pdf','SOT723 suggested pad layout','ManufacturerVerified',
 'Two 0.32 x 0.30 lands; one 0.42 x 0.30 land. Row spacing 1.00. Height .36 is midpoint of .32-.40 range.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT723';


-- JSCJ documented carrier tape and orientation.
INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT723-7','JSCJ SOT723 package information','1.0','EIA-481 / IEC 60286-3 embossed carrier tape',8,2,1.45,1.33,.61,0,0,4,1.50,1.75,0,1,0,
 'Вывод 1: нижний левый; один вывод к перфорации, два от перфорации',0,178,0,8000,1,1,1,
 'W=8.00, P1=2.00, P0=4.00, P2=2.00, D0=1.50, A0=1.33, B0=1.45, K0=0.61 mm; manufacturer drawing page 5.',
 datetime('now'),'JSCJ package data',datetime('now'),'JSCJ package data','Added documented tape orientation with calculated pocket clearance.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT723';

INSERT INTO MasterLibrary_PackageDocuments (PackageId,DocumentType,FileName,FilePath,Description)
SELECT p.Id,'ManufacturerPackageOutline','CJ3134KT.pdf','Docs\Manufacturer\JSCJ\SOT723\CJ3134KT.pdf','JSCJ SOT723 package outline, suggested pad layout and tape orientation.'
FROM PackageDefinition p WHERE p.PackageName='SOT723'
AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='CJ3134KT.pdf');
