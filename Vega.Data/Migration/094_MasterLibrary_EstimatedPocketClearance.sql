-- Correct only tape pockets whose A0/B0 were not documented by a package
-- or packing drawing. The approved Vega-SMD fallback adds 10 percent to
-- each component envelope dimension, capped at 1.00 mm total clearance.
UPDATE ComponentTapeReelGeometry
SET PocketLength=CASE (SELECT ManufacturerPartNumber FROM ComponentDefinition WHERE Id=ComponentDefinitionId)
      WHEN 'SOD523' THEN 1.76
      WHEN 'SOD723' THEN 1.54
      WHEN 'SOD923' THEN 1.10
      WHEN 'SMA' THEN 5.715
      WHEN 'SMB' THEN 5.825
      WHEN 'SMC' THEN 8.734
    END,
    PocketWidth=CASE (SELECT ManufacturerPartNumber FROM ComponentDefinition WHERE Id=ComponentDefinitionId)
      WHEN 'SOD523' THEN .88
      WHEN 'SOD723' THEN .66
      WHEN 'SOD923' THEN .66
      WHEN 'SMA' THEN 2.866
      WHEN 'SMB' THEN 3.982
      WHEN 'SMC' THEN 6.496
    END,
    VerificationStatus=3,
    Notes='A0/B0 не указаны в доступной документации: карман рассчитан по правилу Vega-SMD — габарит корпуса + 10%, но не более 1,00 мм суммарного зазора.',
    UpdatedAt=datetime('now'),UpdatedBy='Vega-SMD pocket clearance rule'
WHERE ComponentDefinitionId IN (
  SELECT Id FROM ComponentDefinition
  WHERE ManufacturerPartNumber IN ('SOD523','SOD723','SOD923','SMA','SMB','SMC')
);
