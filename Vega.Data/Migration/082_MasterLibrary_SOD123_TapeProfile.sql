-- Standard 8 mm embossed-tape profile for the single SOD123 reference package.
-- Diode body is vertical in the pocket; sprocket holes use the standard 4 mm pitch.
INSERT OR IGNORE INTO ComponentTapeReelGeometry
    (ComponentDefinitionId,PackagingCode,SourceReference,SourceRevision,TapeStandard,
     CarrierTapeWidth,PocketPitch,PocketLength,PocketWidth,PocketDepth,PocketOffsetX,PocketOffsetY,
     SprocketHolePitch,SprocketHoleDiameter,SprocketHoleOffset,CoverTapeWidth,
     FeedDirection,PocketOrientation,Pin1Orientation,PickupRotation,
     ReelDiameter,HubDiameter,QuantityPerReel,IsDefault,IsActive,VerificationStatus,Notes,
     CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT c.Id,'SOD123 / 8 mm tape','Diodes Incorporated SOD123 package information','2017-03-16', 'Embossed Carrier Tape',
       8.00,4.00,4.50,2.40,2.40,0,0,
       4.00,1.50,0,0,
       1,0,'Диод расположен вертикально; катодная полоса ориентирована по схеме производителя',0,
       0,0,3000,1,1,1,'Официальный профиль SOD123: W=8±0,30 мм, P/P0=4±0,10 мм, D=1,5+0,10/-0 мм, P2=2±0,05 мм.',
       datetime('now'),'Vega-SMD reference',datetime('now'),'Vega-SMD reference','Added standard SOD123 tape profile.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOD123'
  AND NOT EXISTS (SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id AND t.IsDefault=1);
