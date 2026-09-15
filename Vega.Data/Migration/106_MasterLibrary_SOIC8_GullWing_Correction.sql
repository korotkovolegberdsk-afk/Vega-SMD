-- SOIC8 nominal gull-wing lead geometry from the supplied package drawing.
UPDATE PackageDefinition
SET Height=1.45, LeadLength=.67, LeadWidth=.38, UpdatedAt=datetime('now')
WHERE PackageName='SOIC8';
