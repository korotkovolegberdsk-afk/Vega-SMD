-- Remaining standard TI PW TSSOP outlines.  Geometry and carrier data are
-- retained separately so each drawing uses the documented pocket profile.

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'TSSOP20P065W64','TSSOP20P065W64','TSSOP-20 / TI PW / MO-153','TSSOP','IC',c.Id,f.Id,6.50,6.40,1.20,6.50,4.40,.60,.25,.65,20,20,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/txb0108.pdf','20-pin PW thin shrink small-outline package','TI PW; nominal geometry from the controlled package drawing.','TSSOP65P640X120-20N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='TSSOP';

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'TSSOP24P065W64','TSSOP24P065W64','TSSOP-24 / TI PW / MO-153','TSSOP','IC',c.Id,f.Id,7.80,6.40,1.20,7.80,4.40,.60,.25,.65,24,24,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/tca9555.pdf','24-pin PW thin shrink small-outline package','TI PW; nominal geometry from the controlled package drawing.','TSSOP65P640X120-24N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='TSSOP';

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'TSSOP28P065W64','TSSOP28P065W64','TSSOP-28 / TI PW / MO-153','TSSOP','IC',c.Id,f.Id,9.70,6.40,1.20,9.70,4.40,.60,.25,.65,28,28,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/tps23861.pdf','28-pin PW thin shrink small-outline package','TI PW; nominal geometry from the controlled package drawing.','TSSOP65P640X120-28N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='TSSOP';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Texas Instruments',p.StandardName || ' package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'TI package data',datetime('now'),'TI package data','Added documented extended TSSOP geometry.'
FROM PackageDefinition p WHERE p.PackageName IN ('TSSOP20P065W64','TSSOP24P065W64','TSSOP28P065W64');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,c.ManufacturerPartNumber,p.PadCount,1.50,.45,.65,'F.Paste','Texas Instruments',p.DatasheetUrl,'TI PW land pattern','ManufacturerVerified',
 '1.50 x 0.45 mm lands; 0.65 mm pitch; row-center spacing 5.80 mm.'
FROM ComponentDefinition c JOIN PackageDefinition p ON p.Id=c.PackageId
WHERE c.ManufacturerPartNumber IN ('TSSOP20P065W64','TSSOP24P065W64','TSSOP28P065W64');

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'TXB0108PWR','TI carrier material TXB0108PWR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',16,8,7.00,6.95,1.40,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2000,1,1,3,
 'W=16.0, P1=8.0, P0=4.0, A0=6.95, B0=7.00, K0=1.40 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented TSSOP-20 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='TSSOP20P065W64'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'TCA9555PWR','TI carrier material TCA9555PWR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',16,8,8.30,6.95,1.60,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2000,1,1,3,
 'W=16.0, P1=8.0, P0=4.0, A0=6.95, B0=8.30, K0=1.60 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented TSSOP-24 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='TSSOP24P065W64'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'TPS23861PWR','TI carrier material TPS23861PWR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',16,12,10.10,6.75,1.80,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2000,1,1,3,
 'W=16.0, P1=12.0, P0=4.0, A0=6.75, B0=10.10, K0=1.80 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented TSSOP-28 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='TSSOP28P065W64'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'TSSOP-20','Common','TI PW' FROM PackageDefinition WHERE PackageName='TSSOP20P065W64';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'TSSOP-24','Common','TI PW' FROM PackageDefinition WHERE PackageName='TSSOP24P065W64';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'TSSOP-28','Common','TI PW' FROM PackageDefinition WHERE PackageName='TSSOP28P065W64';
