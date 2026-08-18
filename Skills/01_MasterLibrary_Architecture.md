# MasterLibrary Architecture

## Scope

Применять при проектировании, чтении и изменении сущностей Master Library.

## Mandatory Rules

- Считать `PackageDefinition` описанием физического корпуса.
- Считать `ComponentDefinition` описанием конкретного MPN производителя.
- Хранить геометрию корпуса, чертёж, land pattern и package-level технологические правила на уровне Package.
- Хранить производителя, MPN, datasheet, Tape & Reel и PnP-данные на уровне Component/MPN.
- Связывать Component с Package ссылкой, не копированием геометрии.
- Использовать `MasterLibrary.db` как рабочую Master Library.

## Forbidden

- Смешивать Package и Component/MPN.
- Хранить Tape & Reel на уровне Package.
- Дублировать mechanical geometry в `ComponentDefinition`.
- Использовать footprint как mechanical geometry.
- Использовать `SMT.db` для новых данных Master Library.

## Workflow

1. Определить, относится ли факт к физическому корпусу или к конкретному MPN.
2. Поместить данные в соответствующую сущность.
3. Связать сущности явным идентификатором.
4. Проверить отсутствие дублирования и смешения уровней.

## Verification

- Каждый package-level dataset не зависит от конкретного MPN.
- Каждый MPN-level dataset однозначно связан с `ComponentDefinition`.
- Tape & Reel доступен только через Component/MPN.
- Mechanical geometry и footprint хранятся раздельно.
