CREATE TABLE IF NOT EXISTS ComponentDefinition
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,


    ManufacturerPartNumber TEXT NOT NULL UNIQUE,

    Manufacturer TEXT DEFAULT '',

    Description TEXT DEFAULT '',


    ComponentType TEXT DEFAULT '',

    Value TEXT DEFAULT '',

    Tolerance TEXT DEFAULT '',

    VoltageRating TEXT DEFAULT '',

    PowerRating TEXT DEFAULT '',


    PackageId INTEGER NOT NULL,


    LifecycleStatus TEXT DEFAULT '',

    DatasheetUrl TEXT DEFAULT '',

    InternalPartNumber TEXT DEFAULT '',

    Notes TEXT DEFAULT '',


    IsActive INTEGER DEFAULT 1,


    CreatedAt TEXT,

    CreatedBy TEXT DEFAULT '',

    UpdatedAt TEXT,

    UpdatedBy TEXT DEFAULT '',

    Version INTEGER DEFAULT 1,

    ChangeComment TEXT DEFAULT '',


    FOREIGN KEY(PackageId)
        REFERENCES PackageDefinition(Id)
);



CREATE TABLE IF NOT EXISTS EquipmentAlias
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,


    PackageId INTEGER NOT NULL,


    EquipmentType TEXT DEFAULT '',

    Vendor TEXT DEFAULT '',

    Model TEXT DEFAULT '',


    Alias TEXT DEFAULT '',

    Notes TEXT DEFAULT '',


    IsActive INTEGER DEFAULT 1,


    CreatedAt TEXT,

    CreatedBy TEXT DEFAULT '',

    UpdatedAt TEXT,

    UpdatedBy TEXT DEFAULT '',

    Version INTEGER DEFAULT 1,

    ChangeComment TEXT DEFAULT '',


    FOREIGN KEY(PackageId)
        REFERENCES PackageDefinition(Id)
);



CREATE TABLE IF NOT EXISTS PackageProcessProfile
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,


    PackageId INTEGER NOT NULL,


    StencilThickness REAL DEFAULT 0,

    ApertureType TEXT DEFAULT '',

    AreaRatio REAL DEFAULT 0,

    AspectRatio REAL DEFAULT 0,


    SPIRecommendations TEXT DEFAULT '',

    AOIRecommendations TEXT DEFAULT '',

    TypicalDefects TEXT DEFAULT '',


    PlacementRecommendations TEXT DEFAULT '',

    ReflowRecommendations TEXT DEFAULT '',

    InspectionPriority TEXT DEFAULT '',


    Notes TEXT DEFAULT '',


    IsActive INTEGER DEFAULT 1,


    CreatedAt TEXT,

    CreatedBy TEXT DEFAULT '',

    UpdatedAt TEXT,

    UpdatedBy TEXT DEFAULT '',

    Version INTEGER DEFAULT 1,

    ChangeComment TEXT DEFAULT '',


    FOREIGN KEY(PackageId)
        REFERENCES PackageDefinition(Id)
);