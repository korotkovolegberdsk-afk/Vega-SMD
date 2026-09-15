-- LQFP-32 7 x 7 mm: nominal body outline and packing are tied to ST's
-- STM32L071KBT6TR.  A0/B0/K0 are intentionally calculated from the approved
-- 10 percent pocket-clearance rule because TN1206 does not publish them.

INSERT OR IGNORE INTO PackageDefinition
 (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,Length,Width,Height,BodyLength,BodyWidth,LeadLength,LeadWidth,Pitch,LeadCount,PadCount,PolarityMark,DatasheetUrl,Description,Notes,IPCName,IsActive,CreatedAt,UpdatedAt)
SELECT 'LQFP032P080W091','LQFP032P080W091','LQFP-32 / 7×7 / JEDEC MS-026','QFP','IC',c.Id,f.Id,9.10,9.10,1.40,7.00,7.00,.60,.375,.80,32,32,'Pin 1',
 'https://www.st.com/resource/en/datasheet/stm32l071kb.pdf','32-pin low-profile quad flat package, 7 x 7 mm','Nominal outline: JEDEC MS-026 ABA; ST LQFP32 reference.', 'QFP80P910X140-32N',1,datetime('now'),datetime('now')
FROM PackageFamily f JOIN PackageCategory c ON c.Id=f.CategoryId WHERE f.Code='QFP';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'STM32L071KBT6TR','STMicroelectronics','STM32L071 MCU in LQFP-32, 7 x 7 x 1.4 mm','IC',p.Id,'Production','Reference MPN for documented LQFP32 geometry and tape orientation.',1,datetime('now'),'ST package data',datetime('now'),'ST package data','Added documented LQFP-32 package reference.'
FROM PackageDefinition p WHERE p.PackageName='LQFP032P080W091';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'LQFP-32_7x7mm_P0.8mm',32,1.20,.45,.80,'F.Paste','ГОСТ IEC 61188-5-2','https://www.st.com/resource/en/datasheet/stm32l071kb.pdf','Nominal land calculation','CalculatedFromStandard',
 'Calculated from the documented terminal envelope for standard-density assembly; 1.20 x 0.45 mm lands, 0.80 mm pitch.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='STM32L071KBT6TR';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'5V','ST TN1206, LQFP32 7x7','Rev 3','IEC 60286-3 / EIA-481 embossed carrier tape',16,12,10.01,10.01,1.54,0,0,4,1.5,1.75,0,1,0,'Q2: вывод 1 в верхнем правом квадранте при перфорации сверху и подаче вправо',90,330,0,2000,1,1,2,
 'W=16.0, P1=12.0, P0=4.0, D0=1.5 mm from ST TN1206. A0/B0=10.01 and K0=1.54 mm calculated as envelope plus 10%; Pin 1 Q2 from ST TN1206.',datetime('now'),'ST TN1206 / Vega-SMD rule',datetime('now'),'ST TN1206 / Vega-SMD rule','Added LQFP32 tape data with calculated pocket clearance.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='STM32L071KBT6TR'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO MasterLibrary_PackageAliases(PackageId,Alias,AliasType,Source)
SELECT Id,'LQFP-32','Common','ST / JEDEC' FROM PackageDefinition WHERE PackageName='LQFP032P080W091';
