-- First unique diode package after SOD123: standard SOD323 only.
-- Package outline and tape family are from the Diodes Incorporated package sheet.
UPDATE PackageDefinition
SET Model3DFile='Assets\\Reference\\KiCad\\SOD323\\SOD323.step',
    Length=1.70, Width=1.25, Height=0.95, BodyLength=1.70, BodyWidth=1.25,
    UpdatedAt=datetime('now')
WHERE PackageName='SOD323';

INSERT OR IGNORE INTO ComponentDefinition
    (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOD323','Generic','SMD diode package reference','Diode',p.Id,'Reference',
       'Generic package sample; replace with exact diode MPN when selected from the component list.',
       1,datetime('now'),'KiCad reference import',datetime('now'),'KiCad reference import','Standard diode package model.'
FROM PackageDefinition p WHERE p.PackageName='SOD323';

INSERT OR IGNORE INTO ComponentCadModel
    (ComponentDefinitionId,ModelPath,FileSha256,Length,Width,Height,SourceSystem,SourceUrl,VerificationStatus,Notes)
SELECT c.Id,'Assets\\Reference\\KiCad\\SOD323\\SOD323.step','CD8619B2119CD95683A2C5E660AB4F2D4DB674F696CD6F9E9756F221171A214D',1.70,1.25,0.95,
       'KiCad','C:\\Program Files\\KiCad\\10.0\\share\\kicad\\3dmodels\\Diode_SMD.3dshapes\\D_SOD-323.step',
       'ReferenceValidated','Generic SOD323 reference model; material colour is not a manufacturer confirmation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOD323'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO ComponentTapeReelGeometry
    (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,
     CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,
     SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,
     FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,
     ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,
     CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOD323 / 8 mm tape','Diodes Incorporated SOD323 package information','2016-06-01','Embossed Carrier Tape',
       8.00,4.00,2.40,1.90,1.30,0,0,
       4.00,1.50,0,0,1,0,
       'Диод расположен вертикально; катодная полоса ориентирована по схеме производителя',0,
       0,0,3000,1,1,1,
       'Официальная схема упаковки SOD323: лента 8 мм, шаг 4 мм, отверстия 4 мм.',
       datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added standard SOD323 diode package.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOD323'
  AND NOT EXISTS (SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id AND t.IsDefault=1);
