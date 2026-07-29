CREATE TABLE IF NOT EXISTS PackageFamily
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    CategoryId INTEGER NOT NULL,

    Code TEXT NOT NULL,

    Name TEXT NOT NULL,

    Description TEXT DEFAULT '',

    SortOrder INTEGER DEFAULT 0,

    IsActive INTEGER DEFAULT 1,


    CreatedAt TEXT,

    CreatedBy TEXT DEFAULT '',

    UpdatedAt TEXT,

    UpdatedBy TEXT DEFAULT '',

    Version INTEGER DEFAULT 1,

    ChangeComment TEXT DEFAULT '',


    FOREIGN KEY(CategoryId)
        REFERENCES PackageCategory(Id)
);



CREATE TABLE IF NOT EXISTS PackageDefinition
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,


    PackageName TEXT NOT NULL UNIQUE,

    DisplayName TEXT NOT NULL,

    Description TEXT DEFAULT '',


    CategoryId INTEGER NOT NULL,

    FamilyId INTEGER NOT NULL,


    Length REAL DEFAULT 0,

    Width REAL DEFAULT 0,

    Height REAL DEFAULT 0,


    Pitch REAL DEFAULT 0,

    LeadCount INTEGER DEFAULT 0,

    PadCount INTEGER DEFAULT 0,

    ThermalPadCount INTEGER DEFAULT 0,


    IPCName TEXT DEFAULT '',

    JEDECName TEXT DEFAULT '',

    LandPatternName TEXT DEFAULT '',


    PolarityMark TEXT DEFAULT '',


    DatasheetUrl TEXT DEFAULT '',

    Notes TEXT DEFAULT '',


    IsActive INTEGER DEFAULT 1,


    CreatedAt TEXT,

    CreatedBy TEXT DEFAULT '',

    UpdatedAt TEXT,

    UpdatedBy TEXT DEFAULT '',

    Version INTEGER DEFAULT 1,

    ChangeComment TEXT DEFAULT '',


    FOREIGN KEY(CategoryId)
        REFERENCES PackageCategory(Id),


    FOREIGN KEY(FamilyId)
        REFERENCES PackageFamily(Id)
);