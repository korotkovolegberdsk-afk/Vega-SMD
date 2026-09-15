INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Infineon-D2PAK-TO263-IPB013N06NF2S.pdf', 'Docs\\Manufacturer\\Infineon\\D2PAK\\Infineon-D2PAK-TO263-IPB013N06NF2S.pdf', 'Infineon D2PAK / TO-263 package outline.'
FROM PackageDefinition p
WHERE p.PackageFamily='D2PAK'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Infineon-D2PAK-TO263-IPB013N06NF2S.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageGuide', 'TI-Analog-Logic-Packaging-Guide.pdf', 'Docs\\Manufacturer\\TexasInstruments\\PackageGuide\\TI-Analog-Logic-Packaging-Guide.pdf', 'Texas Instruments package guide covering QFN, QFP, BGA, SO, SSOP and TSSOP families.'
FROM PackageDefinition p
WHERE p.PackageFamily IN ('QFN','QFP','BGA','SOIC','SSOP','TSSOP','TSOP')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TI-Analog-Logic-Packaging-Guide.pdf');
