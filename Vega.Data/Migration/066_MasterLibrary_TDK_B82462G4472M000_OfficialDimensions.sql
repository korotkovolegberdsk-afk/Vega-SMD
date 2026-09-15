-- Official mechanical dimensions without a supplied STEP file.
INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id, '', '', 6.30, 6.30, 3.00,
       'TDK Product Center',
       'https://product.tdk.com/en/search/inductor/inductor/smd/info?part_no=B82462G4472M000',
       'Verified',
       'Official TDK maximum dimensions. The received Ultra Librarian archive does not contain a STEP file; no 3D substitute is used.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'B82462G4472M000'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);
