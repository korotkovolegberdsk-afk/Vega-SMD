# Vega-SMD Rules

Эти правила обязательны для любых изменений Vega-SMD.

1. `PackageDefinition` описывает физический корпус.
2. `ComponentDefinition` описывает конкретный MPN производителя.
3. Tape & Reel относится к MPN и не хранится на уровне Package.
4. Verified manufacturer geometry берётся только из официальных источников с document, revision и page.
5. Generic fallback запрещён в engineering drawing.
6. Недостающую геометрию, размеры, выводы и Pin 1 нельзя придумывать или выводить по похожему корпусу.
7. Footprint и land pattern не являются mechanical geometry и хранятся отдельно.
8. Рабочую `MasterLibrary.db` нельзя удалять, очищать или пересоздавать для исправления migration.
9. Каждую migration проверять на копии существующей базы с сохранением данных и migration history.
10. GitHub использовать для кода, migrations и tests; Google Drive — для manufacturer documents и backups.
11. При display-only очистке manufacturer drawing никогда не удалять и не повреждать инженерные размеры; если annotation нельзя убрать безопасно — оставить его.

Перед изменением прочитать соответствующий файл в `Skills/`. При конфликте выбирать более строгий запрет и останавливать работу при отсутствии verified source.
