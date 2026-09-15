-- STMicroelectronics DPAK / TO-252 nominal outline, footprint and embossed tape.
UPDATE PackageDefinition SET DisplayName='DPAK',StandardName='TO-252',PackageFamily='DPAK',ComponentType='Transistor',
 Length=6.50,Width=9.73,Height=2.30,BodyLength=6.50,BodyWidth=6.10,LeadLength=1.25,LeadWidth=.77,Pitch=2.28,
 LeadCount=3,PadCount=3,ThermalPadLength=6.70,ThermalPadWidth=6.70,PolarityMark='Pin 1',
 DatasheetUrl='https://www.st.com/resource/en/datasheet/std155n3lh6.pdf',UpdatedAt=datetime('now')
WHERE PackageName='DPAK';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'DPAK','Generic','DPAK / TO-252 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'ST package data',datetime('now'),'ST package data','Added documented DPAK card.'
FROM PackageDefinition p WHERE p.PackageName='DPAK';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'DPAK','TO-252','Two source lands and segmented thermal land',3,1.60,3.00,2.28,0,2,3,0,'Mixed','STMicroelectronics',DatasheetUrl,'','DPAK suggested land pattern','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='DPAK';

UPDATE PackageFootprint SET PatternName='DPAK',StandardName='TO-252',Description='Two 1.60 x 3.00 mm source lands and one 6.70 x 6.70 mm thermal land',
 PadCount=3,PadLength=1.60,PadWidth=3.00,PadPitch=2.28,RowCount=2,ColumnCount=3,PasteReduction=0,ApertureType='Mixed',
 SourceSystem='STMicroelectronics',SourceUrl='https://www.st.com/resource/en/datasheet/std155n3lh6.pdf',SourceVariant='DPAK suggested land pattern',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='DPAK');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'DPAK',3,1.60,3.00,2.28,'F.Paste','STMicroelectronics','https://www.st.com/resource/en/datasheet/std155n3lh6.pdf','DPAK suggested land pattern','ManufacturerVerified',
 'Two 1.60 x 3.00 mm lead lands and one 6.70 x 6.70 mm thermal land; thermal aperture divided 2 x 2.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='DPAK';

DELETE FROM ComponentTapeReelGeometry WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='DPAK');
INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'DPAK-TR','STMicroelectronics DPAK mechanical and tape drawing','','EIA-481 / IEC 60286-3 embossed carrier tape',16,8,10.50,6.90,2.65,0,0,4,1.55,1.75,0,1,0,
 'Вывод 1: нижний левый при перфорации сверху и подаче вправо',0,330,0,2500,1,1,3,
 'W=16.00, P1=8.00, P0=4.00, P2=2.00, D0=1.55, A0=6.90, B0=10.50, K0=2.65 mm.',
 datetime('now'),'ST package data',datetime('now'),'ST package data','Added documented DPAK tape geometry and orientation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='DPAK';

