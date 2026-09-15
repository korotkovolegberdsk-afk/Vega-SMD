-- SOT23-5L / SOT23-6L nominal package, land and tape data from AOSMD.
UPDATE PackageDefinition SET DisplayName='SOT25',StandardName='SOT23-5L / MO-178C',PackageFamily='SOT',ComponentType='Transistor',
 Length=2.90,Width=2.80,Height=.95,BodyLength=2.90,BodyWidth=1.60,LeadLength=.60,LeadWidth=.40,Pitch=.95,
 LeadCount=5,PadCount=5,PolarityMark='Pin 1',DatasheetUrl='https://www.aosmd.com/sites/default/files/res/packaging_information/SOT23_5.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT25';

UPDATE PackageDefinition SET DisplayName='SOT26',StandardName='SOT23-6L / MO-178C',PackageFamily='SOT',ComponentType='Transistor',
 Length=2.90,Width=2.80,Height=1.10,BodyLength=2.90,BodyWidth=1.60,LeadLength=.60,LeadWidth=.40,Pitch=.95,
 LeadCount=6,PadCount=6,PolarityMark='Pin 1',DatasheetUrl='https://www.aosmd.com/sites/default/files/res/packaging_information/SOT23_6.pdf',UpdatedAt=datetime('now')
WHERE PackageName='SOT26';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Generic',p.StandardName,'Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'AOSMD package data',datetime('now'),'AOSMD package data','Added documented SOT23 multi-lead card.'
FROM PackageDefinition p WHERE p.PackageName IN ('SOT25','SOT26');

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,PackageName,StandardName,'AOSMD recommended land pattern',LeadCount,.80,.63,.95,0,2,3,0,'Rectangle','AOSMD',DatasheetUrl,'',PackageName||' recommended land pattern','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName IN ('SOT25','SOT26');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,f.PatternName,f.PadCount,f.PadLength,f.PadWidth,f.PadPitch,'F.Paste',f.SourceSystem,f.SourceUrl,f.SourceVariant,f.VerificationStatus,'Documented recommended land geometry; aperture reduction is calculated separately.'
FROM ComponentDefinition c JOIN PackageFootprint f ON f.PackageId=c.PackageId
WHERE c.ManufacturerPartNumber IN ('SOT25','SOT26');

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT23_5L/6L','AOSMD SOT23_5L/6L Tape and Reel Data','TR-00018','EIA-481 embossed carrier tape',8,4,3.15,3.20,1.40,0,0,4,1.50,0,0,1,0,'Вывод 1: нижний левый угол',0,178,54,3000,1,1,1,'A0=3.15, B0=3.20, K0=1.40, P1=4.00, W=8.00 мм.',datetime('now'),'AOSMD tape data',datetime('now'),'AOSMD tape data','Added manufacturer-verified tape profile.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber IN ('SOT25','SOT26');
