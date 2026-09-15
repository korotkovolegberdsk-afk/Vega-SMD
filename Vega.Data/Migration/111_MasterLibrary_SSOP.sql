-- Documented SSOP package set.  The old SSOP80P065W140 row was a generic
-- placeholder with conflicting dimensions; retain it for history, but hide it
-- from the active package list so it cannot duplicate the corrected SSOP-8.
UPDATE PackageDefinition SET IsActive=0, UpdatedAt=datetime('now')
WHERE PackageName='SSOP80P065W140';

-- TI DCT, 8-pin SSOP, 0.65 mm pitch.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SSOP08P065W43','SSOP08P065W43','SSOP-8 / TI DCT / MO-187 DA','SSOP','IC',c.Id,f.Id,3.00,4.25,1.30,3.00,3.00,.40,.23,.65,8,8,'Pin 1',
 'https://www.ti.com/lit/pdf/mpds049','8-pin DCT shrink small-outline package','TI DCT; representative nominal geometry from the controlled package drawing.','SSOP65P425X130-8N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SSOP';

-- TI DB, 16-pin SSOP, 0.65 mm pitch.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SSOP16P065W78','SSOP16P065W78','SSOP-16 / TI DB','SSOP','IC',c.Id,f.Id,6.20,7.80,2.00,6.20,5.30,.75,.30,.65,16,16,'Pin 1',
 'https://www.ti.com/lit/ds/symlink/sn74hc174.pdf','16-pin DB shrink small-outline package','TI DB; representative nominal geometry from the controlled package drawing.','SSOP65P780X200-16N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SSOP';

-- TI DB, 28-pin SSOP, 0.65 mm pitch.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SSOP28P065W78','SSOP28P065W78','SSOP-28 / TI DB','SSOP','IC',c.Id,f.Id,10.20,7.80,2.00,10.20,5.30,.75,.30,.65,28,28,'Pin 1',
 'https://www.ti.com/lit/ml/mpds510a/mpds510a.pdf','28-pin DB shrink small-outline package','TI DB; representative nominal geometry from the controlled package drawing.','SSOP65P780X200-28N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SSOP';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Texas Instruments',p.StandardName || ' package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'TI package data',datetime('now'),'TI package data','Added documented SSOP geometry.'
FROM PackageDefinition p WHERE p.PackageName IN ('SSOP08P065W43','SSOP16P065W78','SSOP28P065W78');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,c.ManufacturerPartNumber,8,1.10,.40,.65,'F.Paste','Texas Instruments','https://www.ti.com/lit/pdf/mpds049','DCT0008A land pattern','ManufacturerVerified','Eight 1.10 x 0.40 mm lands; 0.65 mm pitch; row-center spacing 3.80 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SSOP08P065W43';
INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,c.ManufacturerPartNumber,16,1.85,.45,.65,'F.Paste','Texas Instruments','https://www.ti.com/lit/ds/symlink/sn74hc174.pdf','DB0016A land pattern','ManufacturerVerified','Sixteen 1.85 x 0.45 mm lands; 0.65 mm pitch; row-center spacing 7.00 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SSOP16P065W78';
INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,c.ManufacturerPartNumber,28,1.85,.45,.65,'F.Paste','Texas Instruments','https://www.ti.com/lit/ml/mpds510a/mpds510a.pdf','DB0028A land pattern','ManufacturerVerified','Twenty-eight 1.85 x 0.45 mm lands; 0.65 mm pitch; row-center spacing 7.00 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SSOP28P065W78';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SN74LVC2G125DCTR','TI carrier material SN74LVC2G125DCTR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',12,4,4.40,3.45,1.45,0,0,4,1.5,1.75,0,1,0,'Q3: вывод 1 в нижнем левом квадранте при перфорации сверху и подаче вправо',0,180,0,3000,1,1,3,
 'W=12.0, P1=4.0, P0=4.0, A0=3.45, B0=4.40, K0=1.45 mm; Q3.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented SSOP-8 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SSOP08P065W43'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SN74HC174DBR','TI carrier material SN74HC174DBR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',16,12,6.60,8.35,2.40,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,2000,1,1,3,
 'W=16.0, P1=12.0, P0=4.0, A0=8.35, B0=6.60, K0=2.40 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented SSOP-16 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SSOP16P065W78'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'DSD1796DBR','TI carrier material DSD1796DBR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',16.2,12,10.55,8.45,2.50,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,2000,1,1,3,
 'W=16.2, P1=12.0, P0=4.0, A0=8.45, B0=10.55, K0=2.50 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented SSOP-28 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SSOP28P065W78'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SSOP-8','Common','TI DCT' FROM PackageDefinition WHERE PackageName='SSOP08P065W43';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'DCT0008A','TI','TI package drawing' FROM PackageDefinition WHERE PackageName='SSOP08P065W43';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SSOP-16','Common','TI DB' FROM PackageDefinition WHERE PackageName='SSOP16P065W78';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SSOP-28','Common','TI DB' FROM PackageDefinition WHERE PackageName='SSOP28P065W78';
