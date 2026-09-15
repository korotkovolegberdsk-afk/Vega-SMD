INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerPackageGuide', 'Nexperia-Selection-Guide-2025.pdf', 'Docs\\Manufacturer\\Nexperia\\PackageGuide\\Nexperia-Selection-Guide-2025.pdf', 'Nexperia package cross-reference and package-family guide for SOT and SOD packages.'
FROM PackageDefinition p
WHERE p.PackageFamily IN ('SOT','SOD')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Nexperia-Selection-Guide-2025.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerFootprintGuide', 'Vishay-Chip-Recommended-Pads.pdf', 'Docs\\Manufacturer\\Vishay\\Chip-Technology\\Vishay-Chip-Recommended-Pads.pdf', 'Vishay recommended solder-pad dimensions for chip sizes.'
FROM PackageDefinition p
WHERE p.PackageFamily='CHIP'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='Vishay-Chip-Recommended-Pads.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerReference', 'KEMET-Aluminum-Electrolytic-Resources.html', 'Docs\\Manufacturer\\KEMET\\AluminumCap\\KEMET-Aluminum-Electrolytic-Resources.html', 'KEMET official aluminum electrolytic capacitor technical resources.'
FROM PackageDefinition p
WHERE p.PackageFamily='ALUMINUM_CAP'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='KEMET-Aluminum-Electrolytic-Resources.html');
