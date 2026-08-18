# Git Multi-PC Workflow

## Scope

Применять при работе с Vega-SMD на нескольких компьютерах и при разделении кода, документов и данных.

## Mandatory Rules

- Хранить код, migrations, tests, XAML, SQL и verified seed scripts в GitHub.
- Хранить manufacturer documents, SVG/DXF, screenshots и backups в Google Drive.
- Хранить рабочую `MasterLibrary.db` локально и создавать её резервные копии в Google Drive.
- Перед работой выполнять Fetch, Pull и Build.
- После работы выполнять Build, Tests, Commit и Push.
- Начинать работу на другом компьютере только после Push с предыдущего.

## Forbidden

- Использовать Google Drive как рабочую синхронизируемую папку Git-репозитория.
- Открывать одну рабочую SQLite-базу одновременно на нескольких компьютерах.
- Добавлять в Git `bin/`, `obj/`, `.vs/`, runtime `MasterLibrary.db`, `*.db-wal` или `*.db-shm`.
- Работать параллельно на двух компьютерах без синхронизации Git.
- Перезаписывать удалённые изменения force-push без отдельного явного решения.

## Workflow

1. На активном компьютере выполнить Fetch, Pull и Build.
2. Внести ограниченные изменения и проверить их локально.
3. Выполнить Build и Tests.
4. Создать содержательный Commit и выполнить Push.
5. На следующем компьютере выполнить Pull до начала изменений.
6. Архивировать документы и датированные backups базы в Google Drive.

## Verification

- `git status` не содержит runtime-артефактов и локальной базы.
- Локальная ветка синхронизирована с GitHub после Push.
- Последняя резервная копия базы доступна в Google Drive.
- Проект собирается после Pull на другом компьютере.
