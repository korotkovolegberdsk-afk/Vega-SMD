-- SOT23 lies with its 2.9 mm body axis along feed; pin 1 is in Q3 (lower-left).
-- TapeReelPreview maps PocketWidth horizontally and PocketLength vertically.
UPDATE ComponentTapeReelGeometry
SET PocketWidth=3.15,PocketLength=2.77,PocketDepth=1.22,PocketOrientation=0,PickupRotation=0,
    Pin1Orientation='Вывод 1 в нижнем левом секторе Q3; длинная сторона корпуса вдоль подачи',
    Notes='A0=3.15 по горизонтали, B0=2.77 по вертикали, K0=1.22, P=4.0, W=8.0 мм.',UpdatedAt=datetime('now')
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOT23');
