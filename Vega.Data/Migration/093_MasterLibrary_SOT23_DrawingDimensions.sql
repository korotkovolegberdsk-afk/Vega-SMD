-- SOT23 nominal display geometry and suggested pad layout from Hottech BAV70 package drawing.
UPDATE PackageDefinition SET Length=2.90,Width=2.40,Height=1.03,BodyLength=2.90,BodyWidth=1.30,
 LeadLength=.55,LeadWidth=.40,Pitch=.95,UpdatedAt=datetime('now') WHERE PackageName='SOT23';

UPDATE PackageFootprint SET PadLength=.60,PadWidth=.80,PadPitch=1.90,
 SourceSystem='Hottech',SourceUrl='https://static.chipdip.ru/lib/333/DOC030333222.pdf',SourceVariant='SOT23 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId IN (SELECT Id FROM PackageDefinition WHERE PackageName='SOT23');

UPDATE ComponentFootprint SET PadLength=.60,PadWidth=.80,PadPitch=1.90,
 SourceSystem='Hottech',SourceUrl='https://static.chipdip.ru/lib/333/DOC030333222.pdf',SourceVariant='SOT23 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOT23');
