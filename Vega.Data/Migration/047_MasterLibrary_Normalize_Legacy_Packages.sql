-- Normalize two legacy rows so every visible package has a usable drawing basis.
UPDATE PackageDefinition
SET DisplayName='CHIP 0603', StandardName='CHIP 0603', PackageFamily='CHIP', ComponentType='Passive',
    Length=1.60, Width=0.80, Height=0.80, BodyLength=1.60, BodyWidth=0.80,
    LeadCount=0, PadCount=2, IPCName='CHIP', JEDECName='1608', MirtecAoiClass='PASSIVE',
    Description='Generic EIA 0603 / metric 1608 passive chip package',
    Notes='Nominal body size; choose an R, C or L manufacturer series for component-specific height and tape data.',
    IsActive=1, UpdatedAt=datetime('now')
WHERE PackageName='0603';

UPDATE PackageDefinition
SET DisplayName='SOP-8', StandardName='SOIC-8', PackageFamily='SOIC', ComponentType='Integrated Circuit',
    Length=4.90, Width=3.90, Height=1.75, BodyLength=4.90, BodyWidth=3.90,
    LeadCount=8, PadCount=8, Pitch=1.27, LeadLength=0.60, LeadWidth=0.40,
    IPCName='SOIC127P600X175-8N', JEDECName='MS-012', MirtecAoiClass='SOIC',
    Description='SOIC-8 / SOP-8 integrated-circuit package',
    Notes='Nominal reference dimensions; exact tolerances follow the selected manufacturer drawing.',
    IsActive=1, UpdatedAt=datetime('now')
WHERE PackageName='SOP8';

INSERT INTO MasterLibrary_PackageDocuments (PackageId, DocumentType, FileName, FilePath, Description)
SELECT p.Id, 'Datasheet', 'TI-SOIC-8-MSOI002.pdf',
       'Docs\\Manufacturer\\TexasInstruments\\SOIC-8\\TI-SOIC-8-MSOI002.pdf',
       'Texas Instruments SOIC-8 package outline, JEDEC MS-012.'
FROM PackageDefinition p
WHERE p.PackageName='SOP8'
  AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageDocuments d WHERE d.PackageId=p.Id AND d.FileName='TI-SOIC-8-MSOI002.pdf');
