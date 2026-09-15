-- Remove the candidate created before STEP-envelope validation detected a mismatch.
DELETE FROM ComponentFootprint
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'ESP32-S3-WROOM-1-N8');
DELETE FROM ComponentCadModel
WHERE ComponentDefinitionId IN (SELECT Id FROM ComponentDefinition WHERE ManufacturerPartNumber = 'ESP32-S3-WROOM-1-N8');
DELETE FROM ComponentDefinition WHERE ManufacturerPartNumber = 'ESP32-S3-WROOM-1-N8';
DELETE FROM PackageDefinition WHERE PackageName = 'MOD-ESP32S3-WROOM1';
DELETE FROM PackageFamily WHERE Code = 'WIRELESS_MODULE' AND NOT EXISTS (SELECT 1 FROM PackageDefinition p WHERE p.FamilyId = PackageFamily.Id);
DELETE FROM PackageCategory WHERE Code = 'MODULE' AND NOT EXISTS (SELECT 1 FROM PackageFamily f WHERE f.CategoryId = PackageCategory.Id);
