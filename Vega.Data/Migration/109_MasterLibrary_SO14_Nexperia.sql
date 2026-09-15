-- Canonical 14-pin SO package: Nexperia SOT108-1 / JEDEC MS-012.
UPDATE PackageDefinition
SET PackageName='SO14P127W60', DisplayName='SO14P127W60', StandardName='SO-14 / SOT108-1 / MS-012',
    PackageFamily='SOIC', ComponentType='IC', Length=8.65, Width=6.00, Height=1.75,
    BodyLength=8.65, BodyWidth=3.90, LeadLength=.72, LeadWidth=.41, Pitch=1.27,
    LeadCount=14, PadCount=14, PolarityMark='Pin 1',
    DatasheetUrl='https://assets.nexperia.com/documents/package-information/SOT108-1.pdf',
    Description='14-pin SO package, 1.27 mm pitch, 6.00 mm overall lead span',
    Notes='Nexperia SOT108-1; JEDEC MS-012; package footprint 10.65 x 7.65 mm.',
    IPCName='SOIC127P600X175-14N', UpdatedAt=datetime('now')
WHERE PackageName='SO14'
  AND NOT EXISTS(SELECT 1 FROM PackageDefinition WHERE PackageName='SO14P127W60');

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SO14P127W60','Nexperia','SO-14 / SOT108-1 package reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Nexperia package data',datetime('now'),'Nexperia package data','Added documented SO14 geometry.'
FROM PackageDefinition p WHERE p.PackageName='SO14P127W60';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SO14P127W60',14,1.750,.700,1.270,'F.Paste','Nexperia',
 'https://assets.nexperia.com/documents/package-information/SOT108-1.pdf','SOT108-1 reflow footprint','ManufacturerVerified',
 'Fourteen 1.75 x 0.70 mm lands; 1.27 mm pitch; row-center spacing 5.90 mm; 0.15 mm stencil.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO14P127W60';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT108-1_118','Nexperia SOT108-1_118','2024-01-29','IEC 60286-3 / EIA-481 embossed carrier tape',16,8,9.50,6.50,2.30,0,0,4,1.5,1.75,0,1,0,'Q1/T1: вывод 1 со стороны перфорации при подаче вправо',0,330,0,2500,1,1,3,
 'W=16.0, P1=8.0, P0=4.0, A0=6.50, B0=9.50, K0=2.30 mm; Q1/T1.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added documented SO14 tape geometry.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SO14P127W60'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SO14','Legacy','Vega-SMD migration 109' FROM PackageDefinition WHERE PackageName='SO14P127W60';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC14','Common','Vega-SMD migration 109' FROM PackageDefinition WHERE PackageName='SO14P127W60';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOIC-14','Common','Vega-SMD migration 109' FROM PackageDefinition WHERE PackageName='SO14P127W60';
INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'SOT108-1','Nexperia','Nexperia package information' FROM PackageDefinition WHERE PackageName='SO14P127W60';
