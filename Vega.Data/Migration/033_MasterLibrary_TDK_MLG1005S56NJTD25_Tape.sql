-- TDK MLG1005S packaging geometry from the current automotive MLG1005S catalogue.
-- The values are a series-level packaging specification matching the exact TD25 packaging style.
INSERT INTO ComponentTapeReelGeometry
    (ComponentDefinitionId, PackagingCode, SourceReference, SourceRevision, TapeStandard,
     CarrierTapeWidth, PocketPitch, PocketLength, PocketWidth, PocketDepth, PocketOffsetX, PocketOffsetY,
     SprocketHolePitch, SprocketHoleDiameter, SprocketHoleOffset, CoverTapeWidth,
     FeedDirection, PocketOrientation, Pin1Orientation, PickupRotation,
     ReelDiameter, QuantityPerReel, IsDefault, IsActive, VerificationStatus, Notes,
     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, ChangeComment)
SELECT c.Id,
       'TD25',
       'TDK MLG1005S automotive high-frequency catalogue, packaging style',
       '20260706', 'Punched (Paper) Taping',
       8.00, 2.00, 1.12, 0.62, 0.80, 2.00, 3.50,
       4.00, 1.50, 1.75, 0,
       0, 90, 'Длинная сторона компонента поперёк направления подачи', 90,
       180.00, 10000, 1, 1, 1,
       'TDK values: A=0.62±0.1 mm (вдоль подачи), B=1.12±0.1 mm (поперёк), K≤0.8 mm; P0=4.0±0.1 mm, P1=2.0±0.05 mm, hole Ø1.5 +0.1/-0.0 mm, W=8.0±0.3 mm.',
       datetime('now'), 'TDK import', datetime('now'), 'TDK import',
       'Exact TD25 packaging profile imported from manufacturer MLG1005S catalogue.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber='MLG1005S56NJTD25'
  AND NOT EXISTS (SELECT 1 FROM ComponentTapeReelGeometry t WHERE t.ComponentDefinitionId=c.Id AND t.PackagingCode='TD25');
