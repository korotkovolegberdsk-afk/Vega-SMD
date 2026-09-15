-- Three common TI PW TSSOP outlines. Geometry follows the controlled PW
-- package drawing; carrier profiles are tied to the listed orderable MPNs.

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'TSSOP08P065W64','TSSOP08P065W64','TSSOP-8 / TI PW / MO-153','TSSOP','IC',c.Id,f.Id,3.00,6.40,1.20,3.00,4.40,.60,.25,.65,8,8,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/lm358.pdf','8-pin PW thin shrink small-outline package','TI PW; nominal geometry from the controlled package drawing.','TSSOP65P640X120-8N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='TSSOP';

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'TSSOP14P065W64','TSSOP14P065W64','TSSOP-14 / TI PW / MO-153','TSSOP','IC',c.Id,f.Id,5.00,6.40,1.20,5.00,4.40,.60,.25,.65,14,14,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/sn74hc14.pdf','14-pin PW thin shrink small-outline package','TI PW; nominal geometry from the controlled package drawing.','TSSOP65P640X120-14N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='TSSOP';

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'TSSOP16P065W64','TSSOP16P065W64','TSSOP-16 / TI PW / MO-153','TSSOP','IC',c.Id,f.Id,5.00,6.40,1.20,5.00,4.40,.60,.25,.65,16,16,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/sn74hc174.pdf','16-pin PW thin shrink small-outline package','TI PW; nominal geometry from the controlled package drawing.','TSSOP65P640X120-16N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='TSSOP';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Texas Instruments',p.StandardName || ' package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'TI package data',datetime('now'),'TI package data','Added documented TSSOP geometry.'
FROM PackageDefinition p WHERE p.PackageName IN ('TSSOP08P065W64','TSSOP14P065W64','TSSOP16P065W64');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,c.ManufacturerPartNumber,p.PadCount,1.50,.45,.65,'F.Paste','Texas Instruments',p.DatasheetUrl,'TI PW land pattern','ManufacturerVerified',
 '1.50 x 0.45 mm lands; 0.65 mm pitch; row-center spacing 5.80 mm.'
FROM ComponentDefinition c JOIN PackageDefinition p ON p.Id=c.PackageId
WHERE c.ManufacturerPartNumber IN ('TSSOP08P065W64','TSSOP14P065W64','TSSOP16P065W64');

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'LM358PWR','TI carrier material LM358PWR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',12,8,3.60,7.00,1.60,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2000,1,1,3,
 'W=12.0, P1=8.0, P0=4.0, A0=7.00, B0=3.60, K0=1.60 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented TSSOP-8 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='TSSOP08P065W64'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SN74HC14PWR','TI carrier material SN74HC14PWR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',12,8,5.60,6.90,1.60,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2000,1,1,3,
 'W=12.0, P1=8.0, P0=4.0, A0=6.90, B0=5.60, K0=1.60 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented TSSOP-14 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='TSSOP14P065W64'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SN74HC174PWR','TI carrier material SN74HC174PWR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',12,8,5.60,6.90,1.60,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2000,1,1,3,
 'W=12.0, P1=8.0, P0=4.0, A0=6.90, B0=5.60, K0=1.60 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented TSSOP-16 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='TSSOP16P065W64'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'TSSOP-8','Common','TI PW' FROM PackageDefinition WHERE PackageName='TSSOP08P065W64';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'TSSOP-14','Common','TI PW' FROM PackageDefinition WHERE PackageName='TSSOP14P065W64';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'TSSOP-16','Common','TI PW' FROM PackageDefinition WHERE PackageName='TSSOP16P065W64';
