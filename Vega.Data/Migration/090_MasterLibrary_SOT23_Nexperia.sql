-- Canonical SOT23 / TO-236AB package and tape geometry from Nexperia.
UPDATE PackageDefinition SET DisplayName='SOT23',StandardName='SOT23 / TO-236AB',PackageFamily='SOT',ComponentType='Transistor',
 Length=2.90,Width=2.50,Height=1.00,BodyLength=2.90,BodyWidth=1.30,LeadLength=.60,LeadWidth=.38,Pitch=.95,
 LeadCount=3,PadCount=3,PolarityMark='Pin 1',DatasheetUrl='https://assets.nexperia.com/documents/package-information/SOT23.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT23';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOT23','Generic','SOT23 / TO-236AB transistor package','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added canonical SOT23 card.'
FROM PackageDefinition p WHERE p.PackageName='SOT23';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'SOT-23','SOT23 / TO-236AB','Nexperia reflow soldering footprint',3,.60,.70,1.90,0,2,2,0,'Rectangle','Nexperia','https://assets.nexperia.com/documents/package-information/SOT23.pdf','','SOT23 reflow footprint','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOT23';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,f.PatternName,f.PadCount,f.PadLength,f.PadWidth,f.PadPitch,'F.Paste',f.SourceSystem,f.SourceUrl,f.SourceVariant,f.VerificationStatus,'Documented reflow land geometry; paste reduction is calculated separately.'
FROM ComponentDefinition c JOIN PackageFootprint f ON f.PackageId=c.PackageId WHERE c.ManufacturerPartNumber='SOT23';

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT23_215','Nexperia SOT23_215','2024-08-20','IEC 60286-3 embossed carrier tape',8,4,3.10,2.70,1.20,0,0,4,1.5,0,0,1,0,'Вывод 1: Q3/T4',0,180,0,3000,1,1,1,'A0=3.1, B0=2.7, K0=1.20, P1=4.0, W=8.0 мм.',datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added manufacturer-verified SOT23 tape profile.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOT23';
