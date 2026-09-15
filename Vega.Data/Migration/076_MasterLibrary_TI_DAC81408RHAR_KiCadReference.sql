-- Exact DAC81408RHAR MPN.  TI RHA is a 40-pin VQFN package.
-- The local KiCad Texas RHA footprint and matching generic QFN STEP are
-- reference geometry; they are not presented as manufacturer CAD.
INSERT OR IGNORE INTO PackageDefinition
    (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
     Length,Width,Height,BodyLength,BodyWidth,Pitch,LeadCount,PadCount,ThermalPadLength,ThermalPadWidth,
     IPCName,MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'VQFN040P050W600','VQFN-40 RHA','VQFN-40 RHA','QFN','IC',c.Id,f.Id,
       6.00,6.00,0.80,6.00,6.00,0.50,40,41,4.60,4.60,
       'RHA','IC_QFN','TI RHA 40-pin VQFN with exposed pad',
       'KiCad reference package geometry; confirm exact MPN mechanical drawing before production release.',1,datetime('now'),datetime('now')
FROM PackageCategory c JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='QFN'
WHERE c.Code='IC';

INSERT OR IGNORE INTO ComponentDefinition
    (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,DatasheetUrl,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'DAC81408RHAR','Texas Instruments','Eight-channel 16-bit buffered voltage-output DAC','IC',p.Id,'Production',
       'https://www.ti.com/product/DAC81408/part-details/DAC81408RHAR',
       'Exact MPN. KiCad RHA model and footprint are reference geometry; tape data is not added until separately verified.',
       1,datetime('now'),'KiCad reference import',datetime('now'),'KiCad reference import','New IC-category MPN.'
FROM PackageDefinition p WHERE p.PackageName='VQFN040P050W600';

INSERT INTO ComponentFootprint
    (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'Texas_RHA_VQFN-40-1EP_6x6mm_P0.5mm_EP4.6x4.6mm',41,0.875,0.25,0.50,'F.Paste','KiCad reference','',
       'Texas RHA VQFN-40','ReferenceValidated','Reference footprint: 40 perimeter terminals plus 4.60 x 4.60 mm exposed pad.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='DAC81408RHAR'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId=c.Id);

INSERT INTO ComponentCadModel
    (ComponentDefinitionId,ModelPath,FileSha256,Length,Width,Height,SourceSystem,SourceUrl,VerificationStatus,Notes)
SELECT c.Id,'Assets\\Reference\\KiCad\\DAC81408RHAR\\DAC81408RHAR.step',
       'E5B4286C2329085D9EF7F90D5EB5BF41A435241ECC1BAAF1383823B6D6A946AE',6.00,6.00,0.80,
       'KiCad','C:\\Program Files\\KiCad\\10.0\\share\\kicad\\3dmodels\\Package_DFN_QFN.3dshapes\\QFN-40-1EP_6x6mm_P0.5mm_EP4.6x4.6mm.step',
       'ReferenceValidated','Reference model only; material colour is not an MPN colour confirmation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='DAC81408RHAR'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId=c.Id);
