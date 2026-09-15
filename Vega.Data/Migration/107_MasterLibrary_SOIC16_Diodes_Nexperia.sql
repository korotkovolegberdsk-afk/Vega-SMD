-- SOIC16 / SO-16 package and land pattern from Diodes Incorporated;
-- SOT109-1 carrier tape and Q1/T1 orientation from Nexperia.
INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'SOIC16','SOIC16','SO-16 / SOT109-1','SOIC','IC',c.Id,f.Id,10.00,6.00,1.26,10.00,3.90,.84,.41,1.27,16,16,'Pin 1','https://www.diodes.com/assets/Package-Files/SO-16.pdf','SOIC-16, 1.27 mm pitch','Documented SO-16 geometry.',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='SOIC';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOIC16','Generic','SOIC-16 / SO-16 package-level reference','IC',p.Id,'Reference','Package-level reference.',1,datetime('now'),'Diodes/Nexperia package data',datetime('now'),'Diodes/Nexperia package data','Added documented SOIC16 card.'
FROM PackageDefinition p WHERE p.PackageName='SOIC16';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'SOIC16',16,1.450,.670,1.270,'F.Paste','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SO-16.pdf','SO-16 suggested pad layout','ManufacturerVerified','Sixteen 1.450 x 0.670 mm lands; 1.27 mm pitch; row-center spacing 4.95 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOIC16';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOT109-1_118','Nexperia SOT109-1_118','2024-01-29','IEC 60286-3 / EIA-481 embossed carrier tape',16,8,10.30,6.50,2.10,0,0,4,1.5,1.75,0,1,0,'Q1/T1: вывод 1 в верхнем левом квадранте при перфорации сверху и подаче вправо',0,330,0,2500,1,1,3,'W=16.0, P1=8.0, P0=4.0, A0=6.50, B0=10.30, K0=2.10 mm; Q1/T1.',datetime('now'),'Nexperia packing data',datetime('now'),'Nexperia packing data','Added documented SOIC16 tape geometry and orientation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOIC16'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);
