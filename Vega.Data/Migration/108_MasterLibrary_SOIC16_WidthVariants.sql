-- Canonical SO package names encode pin count, pitch and total lead span.
-- Legacy SOIC8/SOIC16 names remain aliases for search compatibility.
UPDATE PackageDefinition
SET PackageName='SO08P127W60',DisplayName='SO08P127W60',IPCName='SOIC127P600X175-8N',UpdatedAt=datetime('now')
WHERE PackageName='SOIC8'
  AND NOT EXISTS(SELECT 1 FROM PackageDefinition WHERE PackageName='SO08P127W60');

UPDATE ComponentDefinition SET ManufacturerPartNumber='SO08P127W60',UpdatedAt=datetime('now'),
 ChangeComment='Canonical SO package designation applied.'
WHERE ManufacturerPartNumber='SOIC8'
  AND NOT EXISTS(SELECT 1 FROM ComponentDefinition WHERE ManufacturerPartNumber='SO08P127W60');
UPDATE ComponentFootprint SET PatternName='SO08P127W60'
WHERE PatternName='SOIC8' AND ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SO08P127W60');

UPDATE PackageDefinition
SET PackageName='SO16P127W60',DisplayName='SO16P127W60',IPCName='SOIC127P600X175-16N',UpdatedAt=datetime('now')
WHERE PackageName='SOIC16'
  AND NOT EXISTS(SELECT 1 FROM PackageDefinition WHERE PackageName='SO16P127W60');

UPDATE ComponentDefinition SET ManufacturerPartNumber='SO16P127W60',UpdatedAt=datetime('now'),
 ChangeComment='Canonical SO package designation applied.'
WHERE ManufacturerPartNumber='SOIC16'
  AND NOT EXISTS(SELECT 1 FROM ComponentDefinition WHERE ManufacturerPartNumber='SO16P127W60');
UPDATE ComponentFootprint SET PatternName='SO16P127W60'
WHERE PatternName='SOIC16' AND ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SO16P127W60');

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC8','Legacy','Vega-SMD migration 108' FROM PackageDefinition WHERE PackageName='SO08P127W60';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-8','JEDEC/common','Vega-SMD migration 108' FROM PackageDefinition WHERE PackageName='SO08P127W60';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC16','Legacy','Vega-SMD migration 108' FROM PackageDefinition WHERE PackageName='SO16P127W60';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-16','JEDEC/common','Vega-SMD migration 108' FROM PackageDefinition WHERE PackageName='SO16P127W60';

-- 16-pin, 1.27 mm pitch, 7.60 mm overall span; 5.30 x 10.20 mm body.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SO16P127W76','SO16P127W76','SO-16L / NS0016A','SOIC','IC',c.Id,f.Id,10.20,7.60,2.00,10.20,5.30,.75,.43,1.27,16,16,'Pin 1',
 'https://www.st.com/content/ccc/resource/technical/document/datasheet/64/c8/d7/39/5c/a3/44/3d/CD00000318.pdf/files/CD00000318.pdf/jcr:content/translations/en.CD00000318.pdf',
 '16-pin SO/SOP, 1.27 mm pitch, 7.60 mm overall span','5.30 mm body; separate geometry from 6.00 and 10.30 mm variants.','SOIC127P760X200-16N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SOIC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SO16P127W76','Generic','SO-16, 7.60 mm overall span package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'ST/KiCad package data',datetime('now'),'ST/KiCad package data','Added distinct 7.60 mm SO16 geometry.'
FROM PackageDefinition p WHERE p.PackageName='SO16P127W76';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SO16P127W76',16,2.400,.600,1.270,'F.Paste','KiCad 10 official footprint library','http://www.ti.com/lit/ml/msop002a/msop002a.pdf','SOIC-16W_5.3x10.2mm_P1.27mm','ReferenceValidated','Sixteen 2.40 x 0.60 mm lands; 1.27 mm pitch; row-center spacing 6.50 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO16P127W76';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SO16P127W76-GENERIC','IEC 60286-3 calculated fallback','2022','IEC 60286-3 / EIA-481 embossed carrier tape',16,12,11.20,8.36,2.20,0,0,4,1.5,1.75,0,1,0,'Pin 1 toward sprocket-hole side; verify for selected MPN',0,330,0,0,1,1,1,'Pocket fallback: component size plus 10%, capped at 1.00 mm total per axis. Replace with selected manufacturer profile.',datetime('now'),'Vega-SMD calculated fallback',datetime('now'),'Vega-SMD calculated fallback','Added provisional SO16 7.60 mm tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO16P127W76'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

-- 16-pin wide SOIC (300 mil), 1.27 mm pitch, 10.30 mm overall span.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'SO16P127W103','SO16P127W103','SO-16W / SOT162-1 / MS-013','SOIC','IC',c.Id,f.Id,10.30,10.30,2.50,10.30,7.50,.58,.41,1.27,16,16,'Pin 1',
 'https://www.diodes.com/assets/Package-Files/Pd-1005.pdf','16-pin wide SOIC, 1.27 mm pitch, 10.30 mm overall span','7.50 mm body; JEDEC MS-013 AA / Nexperia SOT162-1 family.','SOIC127P1030X265-16N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SOIC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SO16P127W103','Generic','SO-16W, 10.30 mm overall span package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes/Nexperia/KiCad package data',datetime('now'),'Diodes/Nexperia/KiCad package data','Added distinct 10.30 mm SO16 geometry.'
FROM PackageDefinition p WHERE p.PackageName='SO16P127W103';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SO16P127W103',16,2.050,.600,1.270,'F.Paste','KiCad 10 official footprint library','https://www.analog.com/media/en/package-pcb-resources/package/pkg_pdf/soic_wide-rw/rw_16.pdf','SOIC-16W_7.5x10.3mm_P1.27mm','ReferenceValidated','Sixteen 2.05 x 0.60 mm lands; 1.27 mm pitch; row-center spacing 9.30 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO16P127W103';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT162-1_118','Nexperia SOT162-1_118','2013-04-12','IEC 60286-3 / EIA-481 embossed carrier tape',16,12,10.78,10.90,3.00,0,0,4,1.5,1.75,0,1,0,'Q1/T1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,1000,1,1,3,'W=16.0, P1=12.0, P0=4.0, A0=10.90, B0=10.78, K0=3.00 mm; Q1/T1.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added documented SO16 wide tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO16P127W103'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-16W_5.3x10.2mm_P1.27mm','KiCad','KiCad 10 official footprint library' FROM PackageDefinition WHERE PackageName='SO16P127W76';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-16W_7.5x10.3mm_P1.27mm','KiCad','KiCad 10 official footprint library' FROM PackageDefinition WHERE PackageName='SO16P127W103';
