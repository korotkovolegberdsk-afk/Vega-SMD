-- One standard SOD123 package only.  SOD123F and CFP3 variants are intentionally excluded.
UPDATE PackageDefinition
SET Model3DFile='Assets\\Reference\\KiCad\\SOD123\\SOD123.step',
    Length=3.70, Width=1.80, Height=1.35, BodyLength=3.70, BodyWidth=1.80,
    UpdatedAt=datetime('now')
WHERE PackageName='SOD123';

INSERT OR IGNORE INTO PackageFootprint
    (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,
     Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT p.Id,'D_SOD-123','SOD123','Standard SOD123 diode footprint',2,0.90,0.95,4.05,
       0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SOD123.pdf','','SOD123 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition p WHERE p.PackageName='SOD123';

UPDATE PackageFootprint
SET PatternName='D_SOD-123',StandardName='SOD123',PadCount=2,PadLength=0.90,PadWidth=0.95,PadPitch=4.05,
    SourceSystem='Diodes Incorporated',SourceUrl='https://www.diodes.com/assets/Package-Files/SOD123.pdf',SourceVariant='SOD123 suggested pad layout',VerificationStatus='ManufacturerVerified',Notes='Suggested pad layout: X=0.900 mm, X1=4.050 mm, Y=0.950 mm.'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOD123');

INSERT OR IGNORE INTO ComponentDefinition
    (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'SOD123','Generic','SMD diode package reference','Diode',p.Id,'Reference',
       'Generic package sample; replace with exact diode MPN when selected from the component list.',
       1,datetime('now'),'KiCad reference import',datetime('now'),'KiCad reference import','Standard diode package model.'
FROM PackageDefinition p WHERE p.PackageName='SOD123';

INSERT OR IGNORE INTO ComponentFootprint
    (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'D_SOD-123',2,0.90,0.95,4.05,'F.Paste','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SOD123.pdf',
       'SOD123 suggested pad layout','ManufacturerVerified','Suggested pad layout: X=0.900 mm, X1=4.050 mm, Y=0.950 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOD123'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId=c.Id);

INSERT OR IGNORE INTO ComponentCadModel
    (ComponentDefinitionId,ModelPath,FileSha256,Length,Width,Height,SourceSystem,SourceUrl,VerificationStatus,Notes)
SELECT c.Id,'Assets\\Reference\\KiCad\\SOD123\\SOD123.step',
       'B3DC4065573ABC72E3D5B4CCA0E908F72A24205A41B6284B1F75CCACBE99ADF8',3.70,1.80,1.35,
       'KiCad','C:\\Program Files\\KiCad\\10.0\\share\\kicad\\3dmodels\\Diode_SMD.3dshapes\\D_SOD-123.step',
       'ReferenceValidated','Generic package model; material colour is not a manufacturer confirmation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOD123'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId=c.Id);
