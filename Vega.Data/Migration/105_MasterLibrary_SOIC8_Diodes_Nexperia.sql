-- SOIC-8 / SO-8 package geometry from Diodes Incorporated and
-- Q1/T1 carrier-tape profile from Nexperia SOT96-1 packing information.
UPDATE PackageDefinition SET PackageName='SOIC8',DisplayName='SOIC8',StandardName='SO-8 / MS-012',PackageFamily='SOIC',ComponentType='IC',
 Length=4.90,Width=6.00,Height=1.45,BodyLength=4.90,BodyWidth=3.90,LeadLength=.67,LeadWidth=.38,Pitch=1.27,
 LeadCount=8,PadCount=8,PolarityMark='Pin 1',DatasheetUrl='https://www.diodes.com/assets/Package-Files/SO-8.pdf',
 Description='SOIC-8, 1.27 mm pitch, documented SO-8 geometry',Notes='Nominal dimensions from Diodes SO-8 package information; MS-012 family.',UpdatedAt=datetime('now')
WHERE PackageName='SO08P127W078' AND NOT EXISTS(SELECT 1 FROM PackageDefinition WHERE PackageName='SOIC8');

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOIC8','Generic','SOIC-8 / SO-8 package-level reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes/Nexperia package data',datetime('now'),'Diodes/Nexperia package data','Added documented SOIC8 card.'
FROM PackageDefinition p WHERE p.PackageName='SOIC8';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOIC8',8,1.505,.802,1.27,'F.Paste','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SO-8.pdf','SO-8 suggested pad layout','ManufacturerVerified',
 'Eight 1.505 x 0.802 mm lands; 1.27 mm pitch; row-center spacing 4.612 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOIC8';

DELETE FROM ComponentTapeReelGeometry WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOIC8');
INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT96-1_515','Nexperia SOT96-1_515','2015-04-17','IEC 60286-3 / EIA-481 embossed carrier tape',12,8,5.4,6.4,2.05,0,0,4,1.5,1.75,0,1,0,
 'Q1/T1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,180,0,500,1,1,3,
 'W=12.0, P1=8.0, P0=4.0, A0=6.4, B0=5.4, K0=2.05 mm; Q1/T1.',
 datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added documented SOIC8 tape geometry and orientation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOIC8';
