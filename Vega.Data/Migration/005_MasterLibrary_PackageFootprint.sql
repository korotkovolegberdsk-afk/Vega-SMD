CREATE TABLE IF NOT EXISTS PackageFootprint
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageId INTEGER NOT NULL UNIQUE,

    PatternName TEXT DEFAULT '',
    StandardName TEXT DEFAULT '',
    Description TEXT DEFAULT '',

    PadCount INTEGER DEFAULT 0,
    PadLength REAL DEFAULT 0,
    PadWidth REAL DEFAULT 0,
    PadPitch REAL DEFAULT 0,

    Pin1Offset REAL DEFAULT 0,
    RowCount INTEGER DEFAULT 0,
    ColumnCount INTEGER DEFAULT 0,

    PasteReduction REAL DEFAULT 0,
    ApertureType TEXT DEFAULT '',

    FOREIGN KEY(PackageId)
        REFERENCES PackageDefinition(Id)
);
