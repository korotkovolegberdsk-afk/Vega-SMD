-- Official manufacturer references for the package families currently seeded in the library.
INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Vishay-Chip-Resistors-0402-1206.pdf', 'Docs\\Manufacturer\\Vishay\\Chip-0402-1206\\Vishay-Chip-Resistors-0402-1206.pdf', 'Vishay chip component dimensions for 0402, 0603, 0805 and 1206 case sizes.'
FROM PackageDefinition p
WHERE p.PackageName IN ('R0402','R0603','R0805','R1206','C0402','C0603','C0805','C1206','L0402','L0603','L0805')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Vishay-Chip-Resistors-0402-1206.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'TI-SOIC-8-MSOI002.pdf', 'Docs\\Manufacturer\\TexasInstruments\\SOIC-8\\TI-SOIC-8-MSOI002.pdf', 'Texas Instruments SOIC-8 package outline, JEDEC MS-012.'
FROM PackageDefinition p
WHERE p.PackageName IN ('SO08','SO08P127W078')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TI-SOIC-8-MSOI002.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'TI-QFN32-LMH1297.pdf', 'Docs\\Manufacturer\\TexasInstruments\\QFN32\\TI-QFN32-LMH1297.pdf', 'Texas Instruments QFN-32 package reference with 5.00 mm x 5.00 mm body.'
FROM PackageDefinition p
WHERE p.PackageName IN ('QFN32P050W500','QFN032P050W500')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TI-QFN32-LMH1297.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOT223.pdf', 'Docs\\Manufacturer\\Nexperia\\SOT223\\Nexperia-SOT223.pdf', 'Nexperia SOT223 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOT223'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOT223.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOD123.pdf', 'Docs\\Manufacturer\\Nexperia\\SOD123\\Nexperia-SOD123.pdf', 'Nexperia SOD123 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOD123'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOD123.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOT323.pdf', 'Docs\\Manufacturer\\Nexperia\\SOT323\\Nexperia-SOT323.pdf', 'Nexperia SOT323 / SC-70 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOT323'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOT323.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Nexperia-SOT89.pdf', 'Docs\\Manufacturer\\Nexperia\\SOT89\\Nexperia-SOT89.pdf', 'Nexperia SOT89 package outline.'
FROM PackageDefinition p
WHERE p.PackageName='SOT89'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-SOT89.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'Infineon-DPAK-TO252-IRLR7843.pdf', 'Docs\\Manufacturer\\Infineon\\DPAK\\Infineon-DPAK-TO252-IRLR7843.pdf', 'Infineon D-Pak / TO-252 package outline.'
FROM PackageDefinition p
WHERE p.PackageFamily='DPAK'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Infineon-DPAK-TO252-IRLR7843.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'TI-TSSOP-Package-Outline.pdf', 'Docs\\Manufacturer\\TexasInstruments\\TSSOP\\TI-TSSOP-Package-Outline.pdf', 'Texas Instruments TSSOP package outline reference, JEDEC MO-153.'
FROM PackageDefinition p
WHERE p.PackageFamily='TSSOP'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TI-TSSOP-Package-Outline.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageOutline', 'TI-PLCC-FN0084A.pdf', 'Docs\\Manufacturer\\TexasInstruments\\PLCC\\TI-PLCC-FN0084A.pdf', 'Texas Instruments PLCC package outline reference.'
FROM PackageDefinition p
WHERE p.PackageFamily='PLCC'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TI-PLCC-FN0084A.pdf');
