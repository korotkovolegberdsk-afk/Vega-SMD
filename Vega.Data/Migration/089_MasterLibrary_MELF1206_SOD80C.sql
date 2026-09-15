-- Canonical MiniMELF / SOD80C diode package.
UPDATE PackageDefinition SET DisplayName='MELF1206',StandardName='MiniMELF / SOD80C',PackageFamily='MELF',ComponentType='Diode',
 Length=3.50,Width=1.50,Height=1.50,BodyLength=3.50,BodyWidth=1.50,LeadLength=.30,LeadWidth=1.50,
 LeadCount=2,PadCount=2,PolarityMark='Cathode Band',DatasheetUrl='https://assets.nexperia.com/documents/package-information/SOD80C.pdf',UpdatedAt=datetime('now')
WHERE PackageName='MELF1206';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'MELF1206','Generic','MiniMELF / SOD80C diode package','Diode',p.Id,'Reference',
 'Package-level reference; electrical characteristics depend on exact MPN.',1,datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added unique cylindrical diode package card.'
FROM PackageDefinition p WHERE p.PackageName='MELF1206';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_MiniMELF','MiniMELF / SOD80C','Nexperia reflow soldering footprint',2,.90,1.60,3.40,0,1,2,0,'Rectangle','Nexperia','https://assets.nexperia.com/documents/package-information/SOD80C.pdf','','SOD80C reflow footprint','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='MELF1206';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,f.PatternName,f.PadCount,f.PadLength,f.PadWidth,f.PadPitch,'F.Paste',f.SourceSystem,f.SourceUrl,f.SourceVariant,f.VerificationStatus,
 'Documented reflow land geometry; paste reduction is calculated separately.'
FROM ComponentDefinition c JOIN PackageFootprint f ON f.PackageId=c.PackageId WHERE c.ManufacturerPartNumber='MELF1206';

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOD80C_115','Nexperia SOD80C_115','2026-08-31','IEC 60286-3 embossed carrier tape',
 8,4,3.95,1.70,1.75,0,0,4,1.5,0,0,1,0,'Катод слева по направлению подачи',0,180,0,2500,1,1,1,
 'Nexperia: A0=1.70, B0=3.95, K0=1.75, P1=4.0, W=8.0 мм; катодная ориентация сохранена.',
 datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added manufacturer-verified SOD80C tape profile.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='MELF1206';
