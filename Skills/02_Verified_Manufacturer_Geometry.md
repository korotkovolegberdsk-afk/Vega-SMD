# Verified Manufacturer Geometry

## Scope

Применять при создании, импорте, хранении и отображении инженерной геометрии корпуса.

## Mandatory Rules

- Использовать только официальный manufacturer package outline, mechanical drawing, package drawing, datasheet или package specification.
- Фиксировать Manufacturer, PackageSeries, SourceDocument, SourceRevision, SourcePage, VerificationStatus и IsCurrent.
- Сохранять только явно указанные размеры, допуски, проекции, координаты, профили выводов и признаки Pin 1.
- Показывать отсутствие данных явно, если официальный источник недостаточен.

## Forbidden

- Придумывать или интерполировать недостающие размеры.
- Вычислять геометрию по LeadCount, Pitch или похожему корпусу.
- Использовать generic fallback в engineering drawing.
- Смешивать геометрию разных производителей или ревизий.
- Восстанавливать точные координаты по растровой картинке без достаточных исходных данных.
- Подменять mechanical geometry footprint-геометрией.

## Workflow

1. Найти официальный источник производителя.
2. Проверить документ, ревизию, страницу, единицы и читаемость.
3. Извлечь только подтверждённые данные без догадок.
4. Привязать dataset к источнику и отметить статус проверки.
5. Сделать текущей только подтверждённую актуальную запись.

## Verification

- Каждое числовое значение прослеживается до официального источника.
- SourceDocument, SourceRevision и SourcePage заполнены.
- Для Package нет более одного dataset с `VerificationStatus = Verified` и `IsCurrent = 1`.
- При недостатке данных renderer сообщает об этом и не строит фиктивный корпус.
