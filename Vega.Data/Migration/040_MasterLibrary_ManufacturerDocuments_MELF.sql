INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackagingGuide', 'Vishay-MELF-Packaging.pdf', 'Docs\\Manufacturer\\Vishay\\MELF\\Vishay-MELF-Packaging.pdf', 'Vishay SMD chip and MELF resistor packaging dimensions.'
FROM PackageDefinition p
WHERE p.PackageFamily='MELF'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Vishay-MELF-Packaging.pdf');
