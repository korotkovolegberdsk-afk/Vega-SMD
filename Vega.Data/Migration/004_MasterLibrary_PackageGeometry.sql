CREATE TABLE IF NOT EXISTS PackageGeometry
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageId INTEGER NOT NULL UNIQUE,

    BodyLength REAL DEFAULT 0,
    BodyWidth REAL DEFAULT 0,
    BodyHeight REAL DEFAULT 0,

    LeadLength REAL DEFAULT 0,
    LeadWidth REAL DEFAULT 0,
    LeadPitch REAL DEFAULT 0,
    LeadCount INTEGER DEFAULT 0,

    PadLength REAL DEFAULT 0,
    PadWidth REAL DEFAULT 0,
    PadPitch REAL DEFAULT 0,

    CenterX REAL DEFAULT 0,
    CenterY REAL DEFAULT 0,

    FOREIGN KEY(PackageId)
        REFERENCES PackageDefinition(Id)
);
