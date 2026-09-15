-- Align reference-card dimensions with the actual local KiCad STEP envelope:
-- outer lead envelope 9.00 x 9.00 x 1.50 mm; moulded body 7.00 x 7.00 mm.
UPDATE PackageDefinition
SET Length=9.00, Width=9.00, Height=1.50, BodyLength=7.00, BodyWidth=7.00, UpdatedAt=datetime('now')
WHERE Id=(SELECT PackageId FROM ComponentDefinition WHERE ManufacturerPartNumber='STM32G431CBT6');

UPDATE ComponentCadModel
SET Length=9.00, Width=9.00, Height=1.50
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='STM32G431CBT6');
