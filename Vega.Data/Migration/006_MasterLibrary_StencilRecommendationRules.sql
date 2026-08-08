CREATE TABLE IF NOT EXISTS StencilRecommendationRule
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageFamily TEXT NOT NULL,
    ComponentType TEXT DEFAULT '',
    RecommendedStencilThickness REAL DEFAULT 0,
    ApertureShape TEXT DEFAULT 'Rectangle',
    ReductionX REAL DEFAULT 0,
    ReductionY REAL DEFAULT 0,
    ThermalPadRule TEXT DEFAULT '',
    AreaRatioMinimum REAL DEFAULT 0.66,
    AspectRatioMinimum REAL DEFAULT 1.5,
    RuleSource TEXT DEFAULT '',
    Notes TEXT DEFAULT '',
    UNIQUE(PackageFamily, ComponentType)
);

INSERT OR IGNORE INTO StencilRecommendationRule
(
    PackageFamily, ComponentType, RecommendedStencilThickness,
    ApertureShape, ReductionX, ReductionY, ThermalPadRule,
    AreaRatioMinimum, AspectRatioMinimum, RuleSource, Notes
)
VALUES
(
    'CHIP', 'Resistor', 0.12,
    'Rectangle', 10, 10, 'Not applicable',
    0.66, 1.5, 'IPC-7525', 'Use symmetric aperture reduction.'
),
(
    'SOIC', 'IC', 0.12,
    'Rectangle', 10, 10, 'Not applicable',
    0.66, 1.5, 'IPC-7525', 'Verify paste volume on fine-pitch leads.'
),
(
    'QFN', 'IC', 0.10,
    'RoundedRectangle', 10, 10, 'Window-pane thermal pad aperture.',
    0.66, 1.5, 'IPC-7525', 'Use window-pane apertures for the exposed pad.'
);
