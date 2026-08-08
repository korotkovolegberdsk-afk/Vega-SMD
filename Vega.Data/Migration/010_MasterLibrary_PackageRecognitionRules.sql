CREATE TABLE IF NOT EXISTS MasterLibrary_PackageRecognitionRules
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Pattern TEXT NOT NULL,
    PackageId INTEGER NOT NULL,
    Priority INTEGER NOT NULL DEFAULT 0,
    MatchType TEXT NOT NULL,
    FOREIGN KEY(PackageId) REFERENCES PackageDefinition(Id)
);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_PackageRecognitionRules_Pattern ON MasterLibrary_PackageRecognitionRules(Pattern);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_PackageRecognitionRules_PackageId ON MasterLibrary_PackageRecognitionRules(PackageId);

INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'R0603', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='R0603' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='R0603');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'C0603', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='C0603' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='C0603');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'L0603', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='L0603' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='L0603');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'SOIC-8', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='SO08' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='SOIC-8');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'TSSOP-16', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='TSSOP' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='TSSOP-16');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'QFN-32', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='QFN' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='QFN-32');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'QFP-64', Id, 300, 'Exact' FROM PackageDefinition WHERE PackageName='QFP' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='QFP-64');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'SO08P127W078', Id, 250, 'Exact' FROM PackageDefinition WHERE PackageName='SO08' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='SO08P127W078');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'QFP032P065W092', Id, 250, 'Exact' FROM PackageDefinition WHERE PackageName='QFP' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='QFP032P065W092');
INSERT INTO MasterLibrary_PackageRecognitionRules (Pattern, PackageId, Priority, MatchType)
SELECT 'SSOP80P065W140', Id, 250, 'Exact' FROM PackageDefinition WHERE PackageName='TSSOP' AND NOT EXISTS (SELECT 1 FROM MasterLibrary_PackageRecognitionRules WHERE Pattern='SSOP80P065W140');