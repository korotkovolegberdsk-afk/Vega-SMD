-- SOT23 embossed carrier tape nominal dimensions verified against the package tape drawing.
UPDATE ComponentTapeReelGeometry
SET PocketLength=3.15,PocketWidth=2.77,PocketDepth=1.22,SourceReference='SOT-23 embossed carrier tape drawing',
    Notes='A0=3.15, B0=2.77, K0=1.22, P1=4.0, W=8.0 мм; вывод 1 в квадранте Q3.',UpdatedAt=datetime('now')
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOT23');
