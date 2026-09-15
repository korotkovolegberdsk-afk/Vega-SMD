-- Keep databases that received the preceding STEP import in sync with the verified mesh envelope.
UPDATE ComponentCadModel
SET Length = 7.68,
    Width = 7.20,
    Height = 2.92,
    Notes = 'Official TDK B82462G4 STEP outline, obtained from the exact B82462G4472M000 product page. STEP envelope: 7.68 x 7.20 x 2.92 mm; TDK catalog body maximum remains 6.30 x 6.30 x 3.00 mm.'
WHERE ComponentDefinitionId = (
    SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'B82462G4472M000'
);
