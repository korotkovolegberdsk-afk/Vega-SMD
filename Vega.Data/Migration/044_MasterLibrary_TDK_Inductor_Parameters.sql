-- Parameters transcribed from the official TDK SMD inductor catalogue.
CREATE TEMP TABLE IF NOT EXISTS TdkInductorParameters(
    Mpn TEXT, ValueRange TEXT, CurrentRange TEXT, HeightRange TEXT);
DELETE FROM TdkInductorParameters;
INSERT INTO TdkInductorParameters VALUES
('MHQ0402PSA','0.2–22 nH','0.11–0.60 A','0.22 mm'),
('MHQ0603P','0.6–39 nH','0.16–1.00 A','0.40 mm'),
('MHQ1005P','0.7–560 nH','0.07–1.20 A','0.60 mm'),
('MLG0402Q','0.2–33 nH','0.12–0.35 A','0.22 mm'),
('MLG0603P','0.6–120 nH','0.08–1.00 A','0.33 mm'),
('MLG1005S','0.3–390 nH','0.05–1.00 A','0.55 mm'),
('MLJ1005H','0.048–0.20 μH','0.48–1.10 A','0.65 mm'),
('MLJ1608W','0.10–0.56 μH','0.40–0.80 A','0.95 mm'),
('MLF1005','0.10–2.20 μH','0.03–0.18 A','0.55 mm'),
('MLF1608','0.047–33 μH','0.002–0.20 A','0.95 mm'),
('MLF2012','0.047–100 μH','0.002–0.30 A','1.05–1.45 mm'),
('B82422A','0.0082–100 μH','0.065–0.80 A','2.00 mm'),
('B82432C','1–1000 μH','0.055–0.60 A','3.20 mm');

UPDATE ComponentDefinition
SET Value=(SELECT ValueRange FROM TdkInductorParameters x WHERE x.Mpn=ComponentDefinition.ManufacturerPartNumber),
    Notes='TDK catalogue parameters: inductance ' || (SELECT ValueRange FROM TdkInductorParameters x WHERE x.Mpn=ComponentDefinition.ManufacturerPartNumber)
          || '; rated current ' || (SELECT CurrentRange FROM TdkInductorParameters x WHERE x.Mpn=ComponentDefinition.ManufacturerPartNumber)
          || '; catalogue height ' || (SELECT HeightRange FROM TdkInductorParameters x WHERE x.Mpn=ComponentDefinition.ManufacturerPartNumber) || '.',
    UpdatedAt=datetime('now')
WHERE Manufacturer IN ('TDK','TDK Electronics')
  AND ManufacturerPartNumber IN (SELECT Mpn FROM TdkInductorParameters);

DROP TABLE TdkInductorParameters;
