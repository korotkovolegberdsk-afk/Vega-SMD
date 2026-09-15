-- One canonical package record per distinct SOD outline. Values are nominal
-- or midpoints of manufacturer limit dimensions; limits remain in provenance docs.
UPDATE PackageDefinition SET ComponentType='Diode', Length=1.60, Width=.80, Height=.60,
 BodyLength=1.20, BodyWidth=.80, LeadLength=.20, LeadWidth=.30, LeadCount=2, PadCount=2,
 PolarityMark='Cathode Band', DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOD523.pdf', UpdatedAt=datetime('now')
WHERE PackageName='SOD523';
UPDATE PackageDefinition SET ComponentType='Diode', Length=1.40, Width=.60, Height=.52,
 BodyLength=1.00, BodyWidth=.60, LeadLength=.20, LeadWidth=.285, LeadCount=2, PadCount=2,
 PolarityMark='Cathode Band', DatasheetUrl='https://www.nxp.com/docs/en/package-information/SOD723.pdf', UpdatedAt=datetime('now')
WHERE PackageName='SOD723';
UPDATE PackageDefinition SET ComponentType='Diode', Length=1.00, Width=.60, Height=.37,
 BodyLength=.80, BodyWidth=.60, LeadLength=.10, LeadWidth=.20, LeadCount=2, PadCount=2,
 PolarityMark='Cathode Band', DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOD923-0.2mm-Lead-Width.pdf', UpdatedAt=datetime('now')
WHERE PackageName='SOD923';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT p.PackageName,'Generic','Canonical SMD diode package','Diode',p.Id,'Reference',
 'Package-level reference; electrical characteristics depend on exact MPN.',1,datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added unique SOD package card.'
FROM PackageDefinition p WHERE p.PackageName IN ('SOD523','SOD723','SOD923');

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_SOD-523','SOD523','Suggested pad layout',2,.60,.70,1.40,0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SOD523.pdf','','SOD523 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOD523';
INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_SOD-723','SOD723','Reflow soldering footprint',2,.50,.42,1.00,0,1,2,0,'Rectangle','NXP Semiconductors','https://www.nxp.com/docs/en/package-information/SOD723.pdf','','SOD723 reflow footprint','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOD723';
INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D_SOD-923','SOD923','Suggested pad layout',2,.30,.40,.90,0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SOD923-0.2mm-Lead-Width.pdf','','SOD923 0.2 mm lead suggested layout','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='SOD923';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,f.PatternName,f.PadCount,f.PadLength,f.PadWidth,f.PadPitch,'F.Paste',f.SourceSystem,f.SourceUrl,f.SourceVariant,f.VerificationStatus,
 'Package-level documented land pattern; paste reduction is calculated separately.'
FROM ComponentDefinition c JOIN PackageFootprint f ON f.PackageId=c.PackageId
WHERE c.ManufacturerPartNumber IN ('SOD523','SOD723','SOD923');

INSERT OR IGNORE INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,c.ManufacturerPartNumber||' / 8 mm tape','Package-level tape illustration','', 'Embossed Carrier Tape',
 8,CASE WHEN c.ManufacturerPartNumber IN ('SOD723','SOD923') THEN 2 ELSE 4 END,
 CASE c.ManufacturerPartNumber WHEN 'SOD523' THEN 1.94 WHEN 'SOD723' THEN 1.70 ELSE 1.30 END,
 CASE c.ManufacturerPartNumber WHEN 'SOD523' THEN .90 WHEN 'SOD723' THEN .70 ELSE .80 END,
 CASE c.ManufacturerPartNumber WHEN 'SOD523' THEN .73 WHEN 'SOD723' THEN .68 ELSE .55 END,0,0,4,1.5,0,0,1,0,
 'Диод расположен вертикально; катодная полоса сохраняет одну ориентацию',0,178,50,
 CASE c.ManufacturerPartNumber WHEN 'SOD523' THEN 3000 WHEN 'SOD723' THEN 3000 ELSE 8000 END,1,1,3,
 'Ширина, шаг и карман используются для справочного изображения; точную упаковку проверять по MPN.',
 datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added one tape profile per unique SOD outline.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber IN ('SOD523','SOD723','SOD923');
