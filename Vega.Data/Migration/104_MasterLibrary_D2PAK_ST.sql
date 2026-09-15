-- STMicroelectronics D2PAK / TO-263 nominal outline, footprint and embossed tape.
UPDATE PackageDefinition SET DisplayName='D2PAK',StandardName='TO-263',PackageFamily='DPAK',ComponentType='Transistor',
 Length=10.20,Width=15.43,Height=4.50,BodyLength=10.20,BodyWidth=9.15,LeadLength=2.54,LeadWidth=.82,Pitch=2.54,
 LeadCount=3,PadCount=4,ThermalPadLength=9.75,ThermalPadWidth=12.20,PolarityMark='Pin 1',
 DatasheetUrl='https://www.st.com/resource/en/datasheet/std155n3lh6.pdf',UpdatedAt=datetime('now')
WHERE PackageName='D2PAK';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'D2PAK','Generic','D2PAK / TO-263 package-level reference','Transistor',p.Id,'Reference','Package-level reference.',1,datetime('now'),'ST package data',datetime('now'),'ST package data','Added documented D2PAK card.'
FROM PackageDefinition p WHERE p.PackageName='D2PAK';

INSERT OR IGNORE INTO PackageFootprint
 (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT Id,'D2PAK','TO-263','Three source lands and segmented thermal land',4,1.60,3.50,2.54,0,2,3,0,'Mixed','STMicroelectronics',DatasheetUrl,'','D2PAK suggested land pattern','ManufacturerVerified'
FROM PackageDefinition WHERE PackageName='D2PAK';

UPDATE PackageFootprint SET PatternName='D2PAK',StandardName='TO-263',Description='Three 1.60 x 3.50 mm source lands and one 9.75 x 12.20 mm thermal land',
 PadCount=4,PadLength=1.60,PadWidth=3.50,PadPitch=2.54,RowCount=2,ColumnCount=3,PasteReduction=0,ApertureType='Mixed',
 SourceSystem='STMicroelectronics',SourceUrl='https://www.st.com/resource/en/datasheet/std155n3lh6.pdf',SourceVariant='D2PAK suggested land pattern',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='D2PAK');

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'D2PAK',4,1.60,3.50,2.54,'F.Paste','STMicroelectronics','https://www.st.com/resource/en/datasheet/std155n3lh6.pdf','D2PAK suggested land pattern','ManufacturerVerified',
 'Three 1.60 x 3.50 mm lead lands and one 9.75 x 12.20 mm thermal land; thermal aperture divided 3 x 3.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='D2PAK';

DELETE FROM ComponentTapeReelGeometry WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='D2PAK');
INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'D2PAK-TR','STMicroelectronics D2PAK mechanical and tape drawing','','EIA-481 / IEC 60286-3 embossed carrier tape',24,12,15.80,10.60,4.90,0,0,4,1.55,1.75,0,1,0,
 'Вывод 1: нижний левый при перфорации сверху и подаче вправо',0,330,0,1000,1,1,3,
 'W=24.00, P1=12.00, P0=4.00, P2=2.00, D0=1.55, A0=10.60, B0=15.80, K0=4.90 mm.',
 datetime('now'),'ST package data',datetime('now'),'ST package data','Added documented D2PAK tape geometry and orientation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='D2PAK';
