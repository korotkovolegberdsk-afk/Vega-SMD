CREATE TABLE IF NOT EXISTS PackageCategory
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    Code TEXT NOT NULL UNIQUE,

    Name TEXT NOT NULL,

    Description TEXT DEFAULT '',

    SortOrder INTEGER DEFAULT 0,

    IsActive INTEGER DEFAULT 1,


    CreatedAt TEXT,

    CreatedBy TEXT DEFAULT '',

    UpdatedAt TEXT,

    UpdatedBy TEXT DEFAULT '',

    Version INTEGER DEFAULT 1,

    ChangeComment TEXT DEFAULT ''
);