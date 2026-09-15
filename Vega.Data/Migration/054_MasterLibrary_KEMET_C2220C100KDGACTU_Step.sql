-- Exact-MPN STEP imported from the Ultra Librarian 3D CAD Model export.
INSERT INTO ComponentCadModel
    (ComponentDefinitionId, ModelPath, FileSha256, Length, Width, Height,
     SourceSystem, SourceUrl, VerificationStatus, Notes)
SELECT c.Id,
       'Assets\\Manufacturer\\KEMET\\C2220C100KDGACTU\\CCS_CCSTDNOS_2220_L6.1W5.4T1.8B0.95.step',
       '9069D627C685BCFA58DF9252061A8084EB3C0146B06B24D6FA07D09F2261C60E',
       6.10, 5.40, 1.80,
       'Ultra Librarian',
       'https://app.ultralibrarian.com/details/236988d6-6e06-11ea-8c00-0ad2c9526b44/Kemet/C2220C100KDGACTU',
       'Verified',
       'Exact MPN STEP export. File name and parsed STEP bounding extents confirm 6.10 × 5.40 × 1.80 mm.'
FROM ComponentDefinition c
WHERE c.ManufacturerPartNumber = 'C2220C100KDGACTU'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId = c.Id);

UPDATE ComponentDefinition
SET Notes = 'Exact MPN footprint and STEP model are verified from separate Ultra Librarian exports. Tape & Reel data are not imported without a source file.'
WHERE ManufacturerPartNumber = 'C2220C100KDGACTU';
