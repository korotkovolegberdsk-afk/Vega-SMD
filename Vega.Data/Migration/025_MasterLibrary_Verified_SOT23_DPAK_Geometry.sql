-- Values below are nominal dimensions transcribed from the verified package
-- outlines supplied for this library. This migration intentionally updates
-- only the two referenced physical packages.

-- SOT-23 / JEDEC TO-236AB: body 2.90 x 1.30 x 1.10 mm;
-- the two collinear lead positions are 1.90 mm apart; lead width 0.40 mm.
UPDATE PackageDefinition
SET Length = 2.90,
    Width = 1.30,
    Height = 1.10,
    BodyLength = 2.90,
    BodyWidth = 1.30,
    Pitch = 1.90,
    LeadWidth = 0.40
WHERE UPPER(TRIM(PackageName)) = 'SOT23';

-- DPAK / TO-252: D = 6.60 mm, E = 6.10 mm, A = 2.30 mm,
-- e = 2.29 mm, L = 1.14 mm, b = 0.76 mm. The 10 mm value is the
-- maximum overall span including leads, while the body stays 6.60 x 6.10.
UPDATE PackageDefinition
SET Length = 10.00,
    Width = 6.60,
    Height = 2.30,
    BodyLength = 6.60,
    BodyWidth = 6.10,
    Pitch = 2.29,
    LeadLength = 1.14,
    LeadWidth = 0.76
WHERE UPPER(TRIM(PackageName)) = 'DPAK';
