CREATE TABLE IF NOT EXISTS ComponentHeightVariant
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ComponentDefinitionId INTEGER NOT NULL,
    VariantName TEXT NOT NULL DEFAULT '',
    ConditionText TEXT NOT NULL DEFAULT '',
    Height REAL NOT NULL,
    SourceDocument TEXT NOT NULL DEFAULT '',
    SourceReference TEXT NOT NULL DEFAULT '',
    VerificationStatus TEXT NOT NULL DEFAULT 'Unverified',
    IsCurrent INTEGER NOT NULL DEFAULT 1,
    Notes TEXT NOT NULL DEFAULT '',
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY(ComponentDefinitionId) REFERENCES ComponentDefinition(Id)
);
CREATE INDEX IF NOT EXISTS IX_ComponentHeightVariant_Component ON ComponentHeightVariant(ComponentDefinitionId);
