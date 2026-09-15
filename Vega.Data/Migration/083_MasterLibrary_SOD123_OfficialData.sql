-- Official Diodes Incorporated SOD123 package and tape data.
UPDATE PackageFootprint
SET PadLength=0.90,PadWidth=0.95,PadPitch=4.05,
    SourceSystem='Diodes Incorporated',SourceUrl='https://www.diodes.com/assets/Package-Files/SOD123.pdf',
    SourceVariant='SOD123 suggested pad layout',VerificationStatus='ManufacturerVerified'
WHERE PackageId=(SELECT Id FROM PackageDefinition WHERE PackageName='SOD123');

UPDATE ComponentFootprint
SET PadLength=0.90,PadWidth=0.95,PadPitch=4.05,
    SourceSystem='Diodes Incorporated',SourceUrl='https://www.diodes.com/assets/Package-Files/SOD123.pdf',
    SourceVariant='SOD123 suggested pad layout',VerificationStatus='ManufacturerVerified',
    Notes='Suggested pad layout: X=0.900 mm, X1=4.050 mm, Y=0.950 mm.'
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOD123');

UPDATE ComponentTapeReelGeometry
SET PackagingCode='SOD123 / 8 mm tape',SourceReference='Diodes Incorporated SOD123 Package Information',
    SourceRevision='2017-03-16',TapeStandard='Embossed Carrier Tape',CarrierTapeWidth=8.00,
    PocketPitch=4.00,PocketLength=4.50,PocketWidth=2.40,PocketDepth=2.40,
    SprocketHolePitch=4.00,SprocketHoleDiameter=1.50,PocketOrientation=0,
    Pin1Orientation='Диод расположен вертикально; катодная полоса ориентирована по схеме производителя',
    QuantityPerReel=3000,VerificationStatus=1,
    Notes='Официальный профиль SOD123: W=8±0,30 мм, P/P0=4±0,10 мм, D=1,5+0,10/-0 мм, P2=2±0,05 мм.',
    UpdatedAt=datetime('now'),UpdatedBy='Diodes Incorporated PDF'
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='SOD123')
  AND IsDefault=1;
