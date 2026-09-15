INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOD323.pdf', 'Docs\\Manufacturer\\Nexperia\\SOD323\\Nexperia-SOD323.pdf', 'Nexperia SOD323 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOD323'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOD323.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOD523.pdf', 'Docs\\Manufacturer\\Nexperia\\SOD523\\Nexperia-SOD523.pdf', 'Nexperia SOD523 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOD523'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOD523.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOT353.pdf', 'Docs\\Manufacturer\\Nexperia\\SOT353\\Nexperia-SOT353.pdf', 'Nexperia SOT353 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOT353'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOT353.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOT363.pdf', 'Docs\\Manufacturer\\Nexperia\\SOT363\\Nexperia-SOT363.pdf', 'Nexperia SOT363 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOT363'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOT363.pdf');
