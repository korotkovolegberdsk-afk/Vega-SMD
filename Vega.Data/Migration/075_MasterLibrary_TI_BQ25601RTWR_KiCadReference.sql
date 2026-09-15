-- Exact BQ25601RTWR MPN. TI identifies RTW as a 24-pin WQFN package;
-- the local KiCad Texas RTW 4 x 4 mm model is a declared reference model.
INSERT OR IGNORE INTO PackageDefinition
    (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
     Length,Width,Height,BodyLength,BodyWidth,Pitch,LeadCount,PadCount,ThermalPadLength,ThermalPadWidth,
     IPCName,MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'WQFN024P050W400','WQFN-24 RTW','WQFN-24 RTW','QFN','IC',c.Id,f.Id,
       4.00,4.00,0.80,4.00,4.00,0.50,24,25,2.70,2.70,
       'RTW','IC_QFN','TI RTW 24-pin WQFN with exposed pad',
       'KiCad reference package geometry; confirm exact MPN mechanical drawing before production release.',1,datetime('now'),datetime('now')
FROM PackageCategory c JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='QFN'
WHERE c.Code='IC';

INSERT OR IGNORE INTO ComponentDefinition
    (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,DatasheetUrl,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'BQ25601RTWR','Texas Instruments','I2C single-cell 3 A buck battery charger with power path','IC',p.Id,'Production',
       'https://www.ti.com/product/BQ25601/part-details/BQ25601RTWR',
       'Exact MPN. KiCad RTW model and footprint are reference geometry; tape data is not added until separately verified.',
       1,datetime('now'),'KiCad reference import',datetime('now'),'KiCad reference import','New IC-category MPN.'
FROM PackageDefinition p WHERE p.PackageName='WQFN024P050W400';

INSERT INTO ComponentFootprint
    (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'Texas_RTW_WQFN-24-1EP_4x4mm_P0.5mm_EP2.7x2.7',25,0.825,0.25,0.50,'F.Paste','KiCad reference','',
       'Texas RTW WQFN-24','ReferenceValidated','Reference footprint: 24 perimeter terminals plus 2.70 x 2.70 mm exposed pad.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='BQ25601RTWR'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId=c.Id);

INSERT INTO ComponentCadModel
    (ComponentDefinitionId,ModelPath,FileSha256,Length,Width,Height,SourceSystem,SourceUrl,VerificationStatus,Notes)
SELECT c.Id,'Assets\\Reference\\KiCad\\BQ25601RTWR\\BQ25601RTWR.step',
       'FC1F36078CBC019C939B868BC4011DB4ECBE000EEAFC462D2469D0B1AC55FE00',4.00,4.00,0.80,
       'KiCad','C:\\Program Files\\KiCad\\10.0\\share\\kicad\\3dmodels\\Package_DFN_QFN.3dshapes\\Texas_RTW_WQFN-24-1EP_4x4mm_P0.5mm_EP2.7x2.7mm.step',
       'ReferenceValidated','Reference model only; material colour is not an MPN colour confirmation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='BQ25601RTWR'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId=c.Id);
