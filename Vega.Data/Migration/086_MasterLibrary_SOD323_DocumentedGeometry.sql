-- Diodes SOD323 package sheet, 2017-03-16. Preserve the original KiCad asset metadata.
UPDATE PackageDefinition
SET Length=2.50, Width=1.30, Height=1.10, BodyLength=1.70, BodyWidth=1.30,
    LeadLength=0.30, LeadWidth=0.30, LeadCount=2,
    DatasheetUrl='https://www.diodes.com/assets/Package-Files/SOD323.pdf',
    UpdatedAt=datetime('now')
WHERE PackageName='SOD323';

-- X1=2.700 is the outside span, not the pad centre spacing: 2.700-X=2.110.
UPDATE PackageFootprint SET PadPitch=2.110
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOD323');
UPDATE ComponentFootprint
SET PadPitch=2.110,
    Notes='Documented pads X=0.590, Y=0.450 mm; outer span X1=2.700 mm; centre spacing=2.110 mm.'
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOD323');

-- The sheet does not specify A0/B0/K0. Cavity dimensions below are illustration
-- allowances, not verified manufacturer packaging dimensions. B0 clears He max 2.70.
UPDATE ComponentTapeReelGeometry
SET PocketLength=2.90, PocketWidth=1.90, PocketDepth=1.40, VerificationStatus=3,
    Notes='Документированы W=8, P=4, P0=4, P2=2, D=1,5 мм. Размеры кармана 2,90 x 1,90 x 1,40 мм — расчётные зазоры для иллюстрации, не размеры изготовителя; A0/B0/K0 в документе не заданы.',
    UpdatedAt=datetime('now'), ChangeComment='Corrected SOD323 cavity clearance and provenance.'
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOD323');
