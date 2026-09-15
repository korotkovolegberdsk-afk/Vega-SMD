-- ST VFQFPN32, 5 x 5 mm, 0.50 mm pitch. Exact reference: DS14186 Rev. 1.
-- The manufacturer supplies the package outline, recommended footprint and
-- complete tape dimensions, including the identifying-pin side.

UPDATE PackageDefinition SET
 DisplayName='QFN032P050W500', StandardName='VFQFPN32 / 5×5 / 0,5',
 Height=.90, BodyLength=5.00, BodyWidth=5.00, Length=5.00, Width=5.00,
 LeadLength=.40, LeadWidth=.25, ThermalPadLength=3.60, ThermalPadWidth=3.60,
 PolarityMark='Pin 1', DatasheetUrl='https://www.st.com/resource/en/datasheet/st33tphf2xspi.pdf',
 Description='32-pin VFQFPN, 5 x 5 mm, 0.5 mm pitch, exposed pad',
 Notes='Documented from ST DS14186 Rev. 1; nominal dimensions and recommended footprint.',
 IPCName='VFQFPN-32-1EP-5x5-P0.5', UpdatedAt=datetime('now')
WHERE PackageName='QFN032P050W500';

INSERT OR IGNORE INTO ComponentDefinition
 (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'ST33HTPH2X32AHE4','STMicroelectronics','STSAFE TPM in VFQFPN32 5 x 5 mm','IC',p.Id,'Production',
 'Reference MPN for documented VFQFPN32 geometry and tape profile.',1,datetime('now'),'ST DS14186',datetime('now'),'ST DS14186','Added documented VFQFPN32 package reference.'
FROM PackageDefinition p WHERE p.PackageName='QFN032P050W500';

INSERT OR IGNORE INTO ComponentFootprint
 (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'VFQFPN-32-1EP_5x5mm_P0.5mm_EP3.45x3.45mm',33,.60,.30,.50,'F.Paste',
 'ST DS14186 Rev. 1','https://www.st.com/resource/en/datasheet/st33tphf2xspi.pdf','Recommended footprint','ManufacturerDocumented',
 '32 perimeter lands 0.60 x 0.30 mm, 0.50 mm pitch; exposed pad 3.45 x 3.45 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='ST33HTPH2X32AHE4';

INSERT INTO ComponentTapeReelGeometry
 (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'12V','ST DS14186, VFQFPN32 5x5','Rev. 1','IEC 60286-3 / EIA-481 embossed carrier tape',12,8,5.25,5.25,1.10,0,0,4,1.55,1.75,.30,1,0,
 'Вывод 1 со стороны перфорации при подаче вправо',0,330,0,3000,1,1,2,
 'A0/B0=5.25, K0=1.10, P1=8.0, P0=4.0, D0=1.55, E=1.75, F=5.5, W=12.0 mm; identifying pin is on sprocket-hole side.',datetime('now'),'ST DS14186',datetime('now'),'ST DS14186','Added documented VFQFPN32 tape data.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='ST33HTPH2X32AHE4'
AND NOT EXISTS(SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id);
