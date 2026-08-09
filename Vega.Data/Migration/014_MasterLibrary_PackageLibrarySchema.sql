ALTER TABLE PackageDefinition ADD COLUMN BodyLength REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN BodyWidth REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN LeadLength REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN LeadWidth REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN ThermalPadLength REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN ThermalPadWidth REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN BallDiameter REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN BallPitch REAL NOT NULL DEFAULT 0;
ALTER TABLE PackageDefinition ADD COLUMN YamahaName TEXT NOT NULL DEFAULT '';
ALTER TABLE PackageDefinition ADD COLUMN MirtecAoiClass TEXT NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS MasterLibrary_PackageAliases
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageId INTEGER NOT NULL,
    Alias TEXT NOT NULL COLLATE NOCASE,
    AliasType TEXT NOT NULL DEFAULT '',
    Source TEXT NOT NULL DEFAULT '',
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY(PackageId) REFERENCES PackageDefinition(Id),
    UNIQUE(PackageId, Alias)
);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_PackageAliases_Alias ON MasterLibrary_PackageAliases(Alias);
CREATE INDEX IF NOT EXISTS IX_MasterLibrary_PackageAliases_PackageId ON MasterLibrary_PackageAliases(PackageId);
CREATE INDEX IF NOT EXISTS IX_PackageDefinition_Family_Name ON PackageDefinition(PackageFamily, PackageName);