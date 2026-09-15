-- Correct databases created before the MPN-specific packaging audit.
DELETE FROM ComponentTapeReelGeometry
WHERE ComponentDefinitionId = (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'EZJS2VB223')
  AND PackagingCode = 'Embossed / Ø180';
