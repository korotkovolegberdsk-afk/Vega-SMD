CREATE TABLE IF NOT EXISTS StencilFrame
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    PrinterModel TEXT NOT NULL DEFAULT '',
    FrameWidth REAL NOT NULL DEFAULT 0,
    FrameHeight REAL NOT NULL DEFAULT 0,
    StencilWidth REAL NOT NULL DEFAULT 0,
    StencilHeight REAL NOT NULL DEFAULT 0,
    OriginX REAL NOT NULL DEFAULT 0,
    OriginY REAL NOT NULL DEFAULT 0,
    GerberTemplateFile TEXT NOT NULL DEFAULT '',
    IsDefault INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    SortOrder INTEGER NOT NULL DEFAULT 0,
    Notes TEXT NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS IX_StencilFrame_IsActive ON StencilFrame(IsActive);
CREATE INDEX IF NOT EXISTS IX_StencilFrame_IsDefault ON StencilFrame(IsDefault);
CREATE UNIQUE INDEX IF NOT EXISTS UX_StencilFrame_ActiveDefault
    ON StencilFrame(IsDefault) WHERE IsDefault = 1 AND IsActive = 1;

CREATE TABLE IF NOT EXISTS StencilProjectFrame
(
    ProjectId INTEGER PRIMARY KEY,
    FrameId INTEGER NOT NULL,
    FrameName TEXT NOT NULL,
    AssignedDate TEXT NOT NULL,
    FOREIGN KEY(FrameId) REFERENCES StencilFrame(Id)
);

INSERT OR IGNORE INTO StencilFrame
(Name, PrinterModel, FrameWidth, FrameHeight, StencilWidth, StencilHeight, OriginX, OriginY, GerberTemplateFile, IsDefault, IsActive, SortOrder, Notes)
VALUES
('LPKF_DEFAULT_FRAME', 'LPKF', 400, 500, 400, 500, 0, 0, '', 1, 1, 0, 'Default LPKF stencil frame');
