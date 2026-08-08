CREATE TABLE IF NOT EXISTS StencilTechnologyRule
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PackageFamily TEXT NOT NULL DEFAULT '', PackageName TEXT NOT NULL DEFAULT '', ComponentType TEXT NOT NULL DEFAULT '',
    TechnologyGoal TEXT NOT NULL DEFAULT '', PreferredShape TEXT NOT NULL DEFAULT '', AlternativeShape TEXT NOT NULL DEFAULT '',
    RecommendedThickness REAL NOT NULL DEFAULT 0, ReductionX REAL NOT NULL DEFAULT 0, ReductionY REAL NOT NULL DEFAULT 0,
    MinAreaRatio REAL NOT NULL DEFAULT 0.66, MinAspectRatio REAL NOT NULL DEFAULT 1.5, Coverage REAL NOT NULL DEFAULT 100,
    Source TEXT NOT NULL DEFAULT '', Manufacturer TEXT NOT NULL DEFAULT '', DocumentReference TEXT NOT NULL DEFAULT '',
    TechnologyReason TEXT NOT NULL DEFAULT '', Notes TEXT NOT NULL DEFAULT '', Priority INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1
);

INSERT OR IGNORE INTO StencilTechnologyRule
(PackageFamily, PackageName, ComponentType, TechnologyGoal, PreferredShape, AlternativeShape, RecommendedThickness, ReductionX, ReductionY, MinAreaRatio, MinAspectRatio, Coverage, Source, Manufacturer, DocumentReference, TechnologyReason, Notes, Priority, IsActive)
VALUES
('CHIP','0201','Resistor','StandardPasteRelease','Rectangle','HomePlate',0.10,5,5,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Small-chip paste release','',100,1),
('CHIP','0402','Resistor','StandardPasteRelease','Rectangle','Snubnose',0.10,8,8,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Reduce solder-ball risk','',100,1),
('CHIP','0402','Resistor','AntiSolderBall','Snubnose','Rectangle',0.10,8,8,0.66,1.5,100,'Internal production experience','', 'Internal SMT Technology Rule','Suppress solder balls','',200,1),
('CHIP','0603','Resistor','StandardPasteRelease','Rectangle','Snubnose',0.12,10,10,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Balanced paste release','',100,1),
('CHIP','0603','Resistor','AntiSolderBall','Snubnose','Rectangle',0.12,10,10,0.66,1.5,100,'Internal production experience','', 'Internal SMT Technology Rule','Suppress solder balls','',200,1),
('CHIP','0805','Resistor','StandardPasteRelease','Rectangle','',0.12,10,10,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Standard paste release','',100,1),
('CHIP','1206','Resistor','StandardPasteRelease','Rectangle','',0.12,10,10,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Standard paste release','',100,1),
('IC','SOIC','IC','StandardPasteRelease','Rectangle','',0.12,10,10,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Standard gull-wing leads','',100,1),
('IC','TSSOP','IC','FinePitch','HomePlate','Rectangle',0.10,10,10,0.66,1.5,100,'Manufacturer guideline','', 'Internal SMT Technology Rule','Fine-pitch bridging control','',200,1),
('IC','QFP','IC','FinePitch','HomePlate','Rectangle',0.10,10,10,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Fine-pitch bridging control','',200,1),
('QFN','QFN','IC','StandardPasteRelease','Rectangle','',0.10,10,10,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','Signal-pad paste release','',100,1),
('QFN','QFN','IC','VoidReduction','WindowPane','Array',0.10,0,0,0.66,1.5,60,'Manufacturer guideline','', 'Internal SMT Technology Rule','Thermal-pad void reduction','Coverage target 50-70%',300,1),
('BGA','BGA','IC','BGARelease','Round','',0.12,0,0,0.66,1.5,100,'IPC recommendation','', 'Internal SMT Technology Rule','BGA paste release','',200,1),
('MELF','MELF','Diode','ComponentStability','MELF','Oblong',0.12,0,0,0.66,1.5,100,'Internal production experience','', 'Internal SMT Technology Rule','Component stability','',150,1);
