CREATE TABLE IF NOT EXISTS DefectDefinitions
(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL UNIQUE,
    EnglishName TEXT NOT NULL,
    RussianName TEXT NOT NULL,
    Category TEXT NOT NULL,
    DescriptionEN TEXT NOT NULL DEFAULT '',
    DescriptionRU TEXT NOT NULL DEFAULT '',
    Severity TEXT NOT NULL,
    TypicalCause TEXT NOT NULL DEFAULT '',
    TypicalSolution TEXT NOT NULL DEFAULT ''
);
CREATE INDEX IF NOT EXISTS IX_DefectDefinitions_Category ON DefectDefinitions(Category);

INSERT OR IGNORE INTO DefectDefinitions (Code, EnglishName, RussianName, Category, DescriptionEN, DescriptionRU, Severity, TypicalCause, TypicalSolution) VALUES
('SolderBall', 'Solder Ball', 'Шарики припоя', 'SolderPrinting', 'Small isolated solder spheres around pads.', 'Небольшие изолированные шарики припоя вокруг площадок.', 'Medium', 'Excess paste volume.', 'Reduce aperture or use Snubnose.'),
('SolderBridge', 'Solder Bridge', 'Перемычка припоя', 'Reflow', 'Unwanted solder connection between conductors.', 'Нежелательное соединение припоем между проводниками.', 'High', 'Excess paste or insufficient spacing.', 'Use HomePlate aperture and verify stencil alignment.'),
('InsufficientSolder', 'Insufficient Solder', 'Недостаточный объём припоя', 'SolderPrinting', 'Solder volume is below the process requirement.', 'Объём припоя ниже технологического требования.', 'High', 'Aperture reduction is too large.', 'Increase aperture coverage.'),
('ExcessSolder', 'Excess Solder', 'Избыточный объём припоя', 'SolderPrinting', 'Solder volume is above the process requirement.', 'Объём припоя выше технологического требования.', 'Medium', 'Oversized aperture.', 'Reduce aperture coverage.'),
('OpenJoint', 'Open Joint', 'Отсутствие пайки / непропай', 'Reflow', 'No reliable solder joint was formed.', 'Надёжное паяное соединение не сформировано.', 'Critical', 'Insufficient solder or wetting issue.', 'Review paste volume and reflow profile.'),
('Tombstone', 'Tombstone', 'Эффект "надгробного камня"', 'Reflow', 'Chip component lifted on one end.', 'Чип-компонент поднят с одной стороны.', 'High', 'Uneven wetting or paste volume.', 'Apply anti-tombstone aperture balance.'),
('ComponentShift', 'Component Shift', 'Смещение компонента', 'ComponentPlacement', 'Component is displaced from its intended position.', 'Компонент смещён относительно расчётного положения.', 'Medium', 'Placement or paste imbalance.', 'Check placement program and paste symmetry.'),
('WrongComponent', 'Wrong Component', 'Неверный компонент', 'AOI', 'Installed component does not match BOM.', 'Установленный компонент не соответствует BOM.', 'Critical', 'Feeder or setup error.', 'Verify feeder setup and BOM verification.'),
('WrongPolarity', 'Wrong Polarity', 'Неверная полярность', 'AOI', 'Polarized component orientation is incorrect.', 'Неверная ориентация полярного компонента.', 'Critical', 'Placement program error.', 'Verify polarity marks and PnP program.'),
('Void', 'Void', 'Пустоты в паяном соединении', 'Inspection', 'Gas voids reduce solder-joint area.', 'Газовые пустоты уменьшают площадь паяного соединения.', 'High', 'Thermal pad paste coverage or reflow profile.', 'Use WindowPane coverage 50-70% and review profile.'),
('HeadInPillow', 'Head-in-Pillow', 'Эффект "голова на подушке"', 'Reflow', 'BGA ball and paste did not coalesce.', 'Шар BGA и паяльная паста не соединились.', 'Critical', 'Warpage or poor wetting.', 'Review profile, coplanarity and paste.'),
('LiftedLead', 'Lifted Lead', 'Поднятый вывод', 'Mechanical', 'Lead is lifted from solder pad.', 'Вывод поднят над паяльной площадкой.', 'High', 'Mechanical stress or poor wetting.', 'Inspect placement force and profile.'),
('BentLead', 'Bent Lead', 'Изогнутый вывод', 'Mechanical', 'Lead is visibly deformed.', 'Вывод имеет видимую деформацию.', 'Medium', 'Handling or placement force.', 'Inspect feeder and placement tooling.'),
('MissingComponent', 'Missing Component', 'Отсутствует компонент', 'AOI', 'Required component was not placed.', 'Требуемый компонент не установлен.', 'Critical', 'Feeder empty or placement failure.', 'Verify feeder and placement alarms.');