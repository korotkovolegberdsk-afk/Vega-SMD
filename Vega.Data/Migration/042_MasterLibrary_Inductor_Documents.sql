INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerReference', 'Vishay-Chip-Resistors-0402-1206.pdf',
       'Docs\\Manufacturer\\Vishay\\Chip-0402-1206\\Vishay-Chip-Resistors-0402-1206.pdf',
       'Vishay chip-component package dimension reference used for the common 0402–1206 body-size family.'
FROM PackageDefinition p
WHERE p.PackageFamily='INDUCTOR'
  AND p.PackageName IN ('IND0402','IND0603','IND0805','IND1206')
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d
                  WHERE d.PackageId=p.Id AND d.FileName='Vishay-Chip-Resistors-0402-1206.pdf');

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'ManufacturerFootprintGuide', 'Vishay-Chip-Recommended-Pads.pdf',
       'Docs\\Manufacturer\\Vishay\\Chip-Technology\\Vishay-Chip-Recommended-Pads.pdf',
       'Vishay recommended solder-pad dimensions for chip-style passive footprints.'
FROM PackageDefinition p
WHERE p.PackageFamily='INDUCTOR'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d
                  WHERE d.PackageId=p.Id AND d.FileName='Vishay-Chip-Recommended-Pads.pdf');
