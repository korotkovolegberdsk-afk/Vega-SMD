-- Official TDK 3D outline for the B82462G4 series; downloaded from the exact MPN product page.
UPDATE ComponentCadModel
SET ModelPath = 'Assets\\Manufacturer\\TDK\\B82462G4472M000\\b82462_g4.step',
    FileSha256 = '1258055029A84B05DBD5EC20155DCEDC0D45A5A1572CF06FC67779E21EFAEBA7',
    Length = 7.68,
    Width = 7.20,
    Height = 2.92,
    SourceSystem = 'TDK Product Center',
    SourceUrl = 'https://product.tdk.com/system/files/dam/doc/product/inductor/inductor/smd/3doutline_step/b82462_g4.step',
    VerificationStatus = 'Verified',
    Notes = 'Official TDK B82462G4 STEP outline, obtained from the exact B82462G4472M000 product page. STEP envelope: 7.68 x 7.20 x 2.92 mm; TDK catalog body maximum remains 6.30 x 6.30 x 3.00 mm.'
WHERE ComponentDefinitionId = (
    SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'B82462G4472M000'
);
