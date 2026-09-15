-- Correct K0 from the AOSMD SOT23_5L/6L tape table.
UPDATE ComponentTapeReelGeometry SET PocketDepth=1.40,
 Notes='A0=3.15, B0=3.20, K0=1.40, P1=4.00, W=8.00 мм.',
 UpdatedAt=datetime('now'),UpdatedBy='AOSMD tape data',ChangeComment='Corrected K0 to the documented nominal value.'
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber IN ('SOT25','SOT26'));
