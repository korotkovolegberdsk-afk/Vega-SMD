# Tape & Reel

## Scope

Применять при хранении и отображении carrier tape для конкретного компонента производителя.

## Mandatory Rules

- Хранить Tape & Reel на уровне `ComponentDefinition`/MPN.
- Связывать MPN с `ComponentTapeReelGeometry`.
- Показывать carrier tape, компонент, W, P1, Rotation, Source и Verification.
- Использовать verified Package renderer для компонента внутри кармана.
- Применять заданный Rotation без изменения геометрии корпуса.
- Считать P0 внутренним фиксированным параметром 4 mm.

## Forbidden

- Хранить Tape & Reel на уровне Package.
- Переносить параметры упаковки одного MPN на другой без официального подтверждения.
- Показывать фиктивный рисунок при отсутствии данных.
- Давать пользователю редактировать P0.
- Смешивать carrier-tape geometry с mechanical geometry корпуса.

## Workflow

1. Выбрать конкретный Manufacturer и MPN.
2. Проверить официальный Tape & Reel source, revision и page.
3. Извлечь W, P1, Rotation и подтверждённую геометрию упаковки.
4. Связать dataset с `ComponentDefinition`.
5. Отрисовать корпус через verified Package renderer и применить Rotation.

## Verification

- Dataset однозначно связан с MPN.
- Source и Verification отображаются вместе с параметрами.
- P0 равен 4 mm и не редактируется пользователем.
- При отсутствии verified data показано сообщение `Данные Tape & Reel отсутствуют`.
