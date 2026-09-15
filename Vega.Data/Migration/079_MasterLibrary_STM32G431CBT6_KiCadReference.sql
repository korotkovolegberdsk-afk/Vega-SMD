-- Exact STM32G431CBT6 MPN.  LQFP-48 7 x 7 mm is a distinct IC package type.
-- Local KiCad geometry is retained as a declared reference, not manufacturer CAD.
INSERT OR IGNORE INTO PackageDefinition
    (PackageName,DisplayName,StandardName,PackageFamily,ComponentType,CategoryId,FamilyId,
     Length,Width,Height,BodyLength,BodyWidth,Pitch,LeadCount,PadCount,ThermalPadLength,ThermalPadWidth,
     IPCName,MirtecAoiClass,Description,Notes,IsActive,CreatedAt,UpdatedAt)
SELECT 'LQFP048P050W700','LQFP-48 7x7','LQFP-48 7x7','QFP','IC',c.Id,f.Id,
       9.00,9.00,1.50,7.00,7.00,0.50,48,48,0,0,
       'LQFP-48','IC_LEADED','48-lead low-profile quad flat package',
       'KiCad reference package geometry; confirm exact MPN mechanical drawing before production release.',1,datetime('now'),datetime('now')
FROM PackageCategory c JOIN PackageFamily f ON f.CategoryId=c.Id AND f.Code='QFP'
WHERE c.Code='IC';

INSERT OR IGNORE INTO ComponentDefinition
    (ManufacturerPartNumber,Manufacturer,Description,ComponentType,PackageId,LifecycleStatus,DatasheetUrl,Notes,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy,ChangeComment)
SELECT 'STM32G431CBT6','STMicroelectronics','Arm Cortex-M4 32-bit microcontroller','IC',p.Id,'Production',
       'https://www.st.com/en/microcontrollers-microprocessors/stm32g431cb.html',
       'Exact MPN. KiCad LQFP reference model and footprint are package geometry; tape data is not added until separately verified.',
       1,datetime('now'),'KiCad reference import',datetime('now'),'KiCad reference import','New IC-category MPN.'
FROM PackageDefinition p WHERE p.PackageName='LQFP048P050W700';

INSERT INTO ComponentFootprint
    (ComponentDefinitionId,PatternName,PadCount,PadLength,PadWidth,PadPitch,PasteLayer,SourceSystem,SourceUrl,SourceVariant,VerificationStatus,Notes)
SELECT c.Id,'LQFP-48_7x7mm_P0.5mm',48,1.475,0.30,0.50,'F.Paste','KiCad reference','',
       'LQFP-48 7x7','ReferenceValidated','Reference footprint: 48 perimeter terminals; no exposed thermal pad.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='STM32G431CBT6'
  AND NOT EXISTS (SELECT 1 FROM ComponentFootprint f WHERE f.ComponentDefinitionId=c.Id);

INSERT INTO ComponentCadModel
    (ComponentDefinitionId,ModelPath,FileSha256,Length,Width,Height,SourceSystem,SourceUrl,VerificationStatus,Notes)
SELECT c.Id,'Assets\\Reference\\KiCad\\STM32G431CBT6\\STM32G431CBT6.step',
       '9DEA352D93177C2904AFD7D7B68EA8855CE1AB691F69F8B3E13655600514B6BF',9.00,9.00,1.50,
       'KiCad','C:\\Program Files\\KiCad\\10.0\\share\\kicad\\3dmodels\\Package_QFP.3dshapes\\LQFP-48_7x7mm_P0.5mm.step',
       'ReferenceValidated','Reference model only; material colour is not an MPN colour confirmation.'
FROM ComponentDefinition c WHERE c.ManufacturerPartNumber='STM32G431CBT6'
  AND NOT EXISTS (SELECT 1 FROM ComponentCadModel m WHERE m.ComponentDefinitionId=c.Id);
