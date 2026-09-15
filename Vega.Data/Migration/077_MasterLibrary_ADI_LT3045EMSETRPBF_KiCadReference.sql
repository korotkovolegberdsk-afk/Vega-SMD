-- Exact LT3045EMSE#TRPBF MPN.  The MSE package is a 12-lead MSOP with an exposed pad.
-- Local KiCad geometry is retained as a declared reference, not manufacturer CAD.
INSERT OR IGNORE INTO PackageDefinition
    (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
     Length,Width,Height,BodyLength,BodyWidth,Pitch,LeadCount,PadCount,ThermalPadLength,ThermalPadWidth,
     IPCName,MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'MSOP012P065W304','MSOP-12 MSE','MSOP-12 MSE','MSOP','IC',c.Id,f.Id,
       4.90,4.039,0.95,4.039,3.00,0.65,12,13,2.845,1.651,
       'MSE','IC_LEADED','12-lead MSOP with exposed pad',
       'KiCad reference package geometry; confirm exact MPN mechanical drawing before production release.',1,datetime('now'),datetime('now')
FROM PackageCategory c JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='MSOP'
WHERE c.Code='IC';

INSERT OR IGNORE INTO ComponentDefinition
    (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,DatasheetUrl,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'LT3045EMSE#TRPBF','Analog Devices','Ultralow-noise high-PSRR linear regulator','IC',p.Id,'Production',
       'https://www.analog.com/en/products/lt3045.html',
       'Exact MPN. KiCad MSOP footprint and model are reference geometry; tape data is not added until separately verified.',
       1,datetime('now'),'KiCad reference import',datetime('now'),'KiCad reference import','New IC-category MPN.'
FROM PackageDefinition p WHERE p.PackageName='MSOP012P065W304';

INSERT INTO ComponentFootprint
    (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'MSOP-12-1EP_3x4.039mm_P0.65mm_EP1.651x2.845mm',13,1.45,0.40,0.65,'F.Paste','KiCad reference','',
       'MSOP-12 MSE','ReferenceValidated','Reference footprint: 12 perimeter terminals plus 1.651 x 2.845 mm exposed pad.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='LT3045EMSE#TRPBF'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId=c.Id);

INSERT INTO ComponentCadModel
    (ComponentDefinitionId,ModelPath,FileSha256,Length,Width,Height,SourceSystem,SourceUrl,VerificationStatus,Notes)
SELECT c.Id,'Assets\\Reference\\KiCad\\LT3045EMSETRPBF\\LT3045EMSETRPBF.step',
       'AA91D924A017AF1CF431A9AC1A666764CB189ECA8B73B8867170C8507343C645',4.90,4.039,0.95,
       'KiCad','C:\\Program Files\\KiCad\\10.0\\share\\kicad\\3dmodels\\Package_SO.3dshapes\\MSOP-12-1EP_3x4.039mm_P0.65mm_EP1.651x2.845mm.step',
       'ReferenceValidated','Reference model only; material colour is not an MPN colour confirmation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='LT3045EMSE#TRPBF'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId=c.Id);
