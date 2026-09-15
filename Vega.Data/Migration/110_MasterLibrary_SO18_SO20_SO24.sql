-- Distinct 1.27 mm pitch SO packages. Geometry, footprints and tape profiles
-- remain separate because pin count, body length, land pattern and pockets differ.

-- SO18, TI DW (R-PDSO-G18), JEDEC MS-013 AB.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SO18P127W103','SO18P127W103','SOIC-18 / TI DW / MS-013 AB','SOIC','IC',c.Id,f.Id,11.55,10.30,2.65,11.55,7.50,.84,.41,1.27,18,18,'Pin 1',
 'https://www.ti.com/lit/pdf/MPDS172A','18-pin wide SOIC, 1.27 mm pitch, 10.30 mm nominal overall span','TI DW (R-PDSO-G18); JEDEC MS-013 variation AB.','SOIC127P1030X265-18N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SOIC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SO18P127W103','Texas Instruments','SOIC-18 / DW package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'TI package data',datetime('now'),'TI package data','Added documented SO18 geometry.'
FROM PackageDefinition p WHERE p.PackageName='SO18P127W103';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SO18P127W103',18,2.000,.600,1.270,'F.Paste','ГОСТ IEC 61188-5-2 / IPC-7351',
 'https://www.ti.com/lit/pdf/MPDS172A','Calculated nominal SOIC-18 land pattern','StandardCalculated','Eighteen 2.00 x 0.60 mm lands; 1.27 mm pitch; row-center spacing 9.30 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO18P127W103';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'UC3526ADWTR','TI Carrier Material Report UC3526ADWTR','2026','IEC 60286-3 / EIA-481 embossed carrier tape',24,12,12.00,10.90,2.70,0,0,4,1.5,1.75,0,1,0,'Q1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,2000,1,1,3,
 'W=24.0, P1=12.0, P0=4.0, A0=10.90, B0=12.00, K0=2.70 mm; Q1.',datetime('now'),'Texas Instruments carrier data',datetime('now'),'Texas Instruments carrier data','Added documented SO18 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO18P127W103'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

-- SO20, Nexperia SOT163-1.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SO20P127W103','SO20P127W103','SO20 / SOT163-1 / MS-013','SOIC','IC',c.Id,f.Id,12.80,10.33,2.65,12.80,7.50,.84,.41,1.27,20,20,'Pin 1',
 'https://assets.nexperia.com/documents/package-information/SOT163-1.pdf','20-pin SO package, 1.27 mm pitch, 10.33 mm overall span','Nexperia SOT163-1; JEDEC MS-013.','SOIC127P1033X265-20N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SOIC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SO20P127W103','Nexperia','SO20 / SOT163-1 package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added documented SO20 geometry.'
FROM PackageDefinition p WHERE p.PackageName='SO20P127W103';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SO20P127W103',20,1.500,.600,1.270,'F.Paste','Nexperia',
 'https://assets.nexperia.com/documents/package-information/SOT163-1.pdf','SOT163-1 reflow footprint','ManufacturerVerified','Twenty 1.50 x 0.60 mm lands; 1.27 mm pitch; row-center spacing 9.50 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO20P127W103';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT163-1_118','Nexperia SOT163-1_118','2022-07-25','IEC 60286-3 / EIA-481 embossed carrier tape',24,12,13.40,10.90,2.70,0,0,4,1.5,1.75,0,1,0,'Q1/T1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,2000,1,1,3,
 'W=24.0, P1=12.0, P0=4.0, A0=10.90, B0=13.40, K0=2.70 mm; Q1/T1.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added documented SO20 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO20P127W103'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

-- SO24, Nexperia SOT137-1.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SO24P127W103','SO24P127W103','SO24 / SOT137-1 / MS-013','SOIC','IC',c.Id,f.Id,15.40,10.33,2.65,15.40,7.50,.65,.43,1.27,24,24,'Pin 1',
 'https://assets.nexperia.com/documents/package-information/SOT137-1.pdf','24-pin SO package, 1.27 mm pitch, 10.33 mm overall span','Nexperia SOT137-1; IEC 075E05; JEDEC MS-013.','SOIC127P1033X265-24N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SOIC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SO24P127W103','Nexperia','SO24 / SOT137-1 package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added documented SO24 geometry.'
FROM PackageDefinition p WHERE p.PackageName='SO24P127W103';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SO24P127W103',24,2.400,.800,1.270,'F.Paste','Nexperia',
 'https://assets.nexperia.com/documents/package-information/SOT137-1.pdf','SOT137-1 reflow footprint','ManufacturerVerified','Twenty-four 2.40 x 0.80 mm lands; 2.20 x 0.60 mm paste deposit; 1.27 mm pitch; row-center spacing 8.80 mm; 0.15 mm stencil.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO24P127W103';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT137-1_118','Nexperia SOT137-1_118','2020-01-21','IEC 60286-3 / EIA-481 embossed carrier tape',24,12,15.90,10.90,3.05,0,0,4,1.5,1.75,0,1,0,'Q1/T1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,1000,1,1,3,
 'W=24.0, P1=12.0, P0=4.0, A0=10.90, B0=15.90, K0=3.05 mm; Q1/T1.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added documented SO24 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO24P127W103'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-18','Common','Texas Instruments DW' FROM PackageDefinition WHERE PackageName='SO18P127W103';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'DW18','TI','Texas Instruments DW' FROM PackageDefinition WHERE PackageName='SO18P127W103';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-20','Common','Nexperia SOT163-1' FROM PackageDefinition WHERE PackageName='SO20P127W103';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOT163-1','Nexperia','Nexperia package information' FROM PackageDefinition WHERE PackageName='SO20P127W103';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-24','Common','Nexperia SOT137-1' FROM PackageDefinition WHERE PackageName='SO24P127W103';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOT137-1','Nexperia','Nexperia package information' FROM PackageDefinition WHERE PackageName='SO24P127W103';
