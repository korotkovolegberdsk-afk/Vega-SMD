-- Official SOD323 land pattern and tape data.
UPDATE PackageDefinition
SET PolarityMark='Cathode Band', UpdatedAt=datetime('now')
WHERE PackageName='SOD323';

INSERT OR IGNORE INTO PackageFootprint
    (PackageId,PatternName,StandardName,Description,PadCount,PadLength,PadWidth,PadPitch,
     Pin1Offset,RowCount,ColumnCount,PasteReduction,ApertureType,SourceSystem,SourceUrl,SourceMpn,SourceVariant,VerificationStatus)
SELECT p.Id,'D_SOD-323','SOD323','Official suggested pad layout',2,0.590,0.450,2.700,
       0,1,2,0,'Rectangle','Diodes Incorporated','https://www.diodes.com/assets/Package-Files/SOD323.pdf','',
       'SOD323 suggested pad layout','ManufacturerVerified'
FROM PackageDefinition p WHERE p.PackageName='SOD323'
  AND NOT EXISTS (SELECT 1 FROM PackageFootprint f WHERE f.PackageId=p.Id);

UPDATE PackageFootprint
SET PatternName='D_SOD-323',StandardName='SOD323',PadCount=2,PadLength=0.590,PadWidth=0.450,PadPitch=2.700,
    SourceSystem='Diodes Incorporated',SourceUrl='https://www.diodes.com/assets/Package-Files/SOD323.pdf',
    SourceVariant='SOD323 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOD323');

INSERT OR IGNORE INTO ComponentFootprint
    (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'D_SOD-323',2,0.590,0.450,2.700,'F.Paste','Diodes Incorporated',
       'https://www.diodes.com/assets/Package-Files/SOD323.pdf','SOD323 suggested pad layout','ManufacturerVerified',
       'Suggested pad layout: X=0.590 mm, X1=2.700 mm, Y=0.450 mm.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='SOD323'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId=c.Id);

UPDATE ComponentTapeReelGeometry
SET SourceRevision='2017-03-16', QuantityPerReel=3000,
    Notes='Официально: лента 8 мм, P=4±0,10 мм, P0=4±0,10 мм, P2=2±0,05 мм, D=1,5+0,10/-0 мм, F=3,5±0,05 мм, W=8±0,30 мм; A0/B0/K0 определяются размером компонента.'
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOD323');
