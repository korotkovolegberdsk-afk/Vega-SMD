-- Align reference-card dimensions with the actual local KiCad STEP envelope:
-- outer leads make the model 4.90 x 4.039 x 0.95 mm, while the moulded body is 4.039 x 3.00 mm.
UPDATE PackageDefinition
SET Length=4.90, Width=4.039, Height=0.95, BodyLength=4.039, BodyWidth=3.00, UpdatedAt=datetime('now')
WHERE Id=(SELECT PackageId FROM ComponentDefinition WHERE ManufacturerPartNumber='LT3045EMSE#TRPBF');

UPDATE ComponentCadModel
SET Length=4.90, Width=4.039, Height=0.95
WHERE ComponentDefinitionId=(SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber='LT3045EMSE#TRPBF');
