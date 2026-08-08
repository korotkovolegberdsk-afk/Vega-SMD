CREATE TABLE IF NOT EXISTS MasterLibrary_TechnologyRecommendations
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageId INTEGER NOT NULL,
    RuleId INTEGER NULL,
    SourceId INTEGER NOT NULL,
    TechnologyGoal TEXT NOT NULL,
    RecommendationText TEXT NOT NULL DEFAULT '',
    ParameterJson TEXT NOT NULL DEFAULT '',
    Priority INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(PackageId) REFERENCES PackageDefinition(Id),
    FOREIGN KEY(RuleId) REFERENCES StencilTechnologyRule(Id),
    FOREIGN KEY(SourceId) REFERENCES MasterLibrary_TechnologySources(Id)
);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_TechnologyRecommendations_PackageGoal ON MasterLibrary_TechnologyRecommendations(PackageId, TechnologyGoal);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_TechnologyRecommendations_SourceId ON MasterLibrary_TechnologyRecommendations(SourceId);

INSERT OR IGNORE INTO MasterLibrary_TechnologyRecommendations (PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority)
SELECT p.Id, r.Id, s.Id, 'StandardAssembly', 'Rectangle aperture with 10% reduction for CHIP 0603.', '{"shape":"Rectangle","reductionX":10,"reductionY":10}', 200
FROM PackageDefinition p, StencilTechnologyRule r, MasterLibrary_TechnologySources s
WHERE p.PackageName = 'R0603' AND r.PackageName = '0603' AND r.TechnologyGoal = 'StandardPasteRelease' AND s.Name = 'Internal SMT Experience';

INSERT OR IGNORE INTO MasterLibrary_TechnologyRecommendations (PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority)
SELECT p.Id, r.Id, s.Id, 'VoidReduction', 'Window-pane thermal-pad pattern; coverage 50-70%, web 0.20-0.30 mm.', '{"shape":"WindowPane","coverageMin":50,"coverageMax":70,"webMin":0.20,"webMax":0.30}', 300
FROM PackageDefinition p, StencilTechnologyRule r, MasterLibrary_TechnologySources s
WHERE p.PackageName = 'QFN' AND r.PackageName = 'QFN' AND r.TechnologyGoal = 'VoidReduction' AND s.Name = 'Indium';

INSERT OR IGNORE INTO MasterLibrary_TechnologyRecommendations (PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority)
SELECT p.Id, r.Id, s.Id, 'FinePitch', 'Home-plate aperture with 10-15% toe reduction.', '{"shape":"HomePlate","toeReductionMin":10,"toeReductionMax":15}', 250
FROM PackageDefinition p, StencilTechnologyRule r, MasterLibrary_TechnologySources s
WHERE p.PackageName = 'QFP' AND r.PackageName = 'QFP' AND r.TechnologyGoal = 'FinePitch' AND s.Name = 'IPC-7525';

INSERT OR IGNORE INTO MasterLibrary_TechnologyRecommendations (PackageId, RuleId, SourceId, TechnologyGoal, RecommendationText, ParameterJson, Priority)
SELECT p.Id, r.Id, s.Id, 'HighReliability', 'Reduced aperture for anti-tombstone control.', '{"shape":"RectangleReduction","strategy":"AntiTombstone"}', 220
FROM PackageDefinition p, StencilTechnologyRule r, MasterLibrary_TechnologySources s
WHERE p.PackageName IN ('R0201', 'R0402') AND r.PackageName = CASE WHEN p.PackageName = 'R0201' THEN '0201' ELSE '0402' END AND r.TechnologyGoal = 'StandardPasteRelease' AND s.Name = 'Internal SMT Experience';