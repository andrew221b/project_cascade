# CASCADE — контекст Unity после P00

<!-- unity-onboarding:generated:start -->

Проверено: 4 октября 2026, Europe/Sofia. Этап: P00 COMPLETE.

## Проект и окружение

- Git root и авторская спецификация:
  `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2`.
- Unity root: `CascadeUnity/`; `Assets`, `Packages/manifest.json` и
  `ProjectSettings/ProjectVersion.txt` подтверждены на диске.
- Unity `6000.3.8f1`, revision `1c7db571dde0`, Apple Silicon.
- Локальный шаблон `com.unity.template.urp-blank@17.0.14`.
- Render pipeline: URP; pipeline и PC/Mobile renderer assets оставлены из
  шаблона. Источники: GraphicsSettings, Assets/Settings и package lock.
- Input: Input System, `activeInputHandler: 1`; Input System `1.18.0`.
- URP фактически `17.3.0` builtin; Test Framework `1.6.0` builtin;
  uGUI `2.0.0` builtin. Исходные запросы шаблона для первых двух —
  `17.0.1` и `1.4.2`; фактические версии фиксируются Editor и lock.
- Остальные прямые зависимости — неизменённые defaults локального шаблона.
  Не считать Navigation/Timeline/Visual Scripting частью архитектуры CASCADE
  только из-за их наличия в defaults. SDK рекламы/IAP/сервисов не добавлялись.
- Последний проверочный импорт запускался с `StandaloneOSX` и `-nographics`.
  Player ни для одной платформы не собирался. Мобильная сборка не проверена.

## Источники и код

Авторские `AGENTS.md`, `TASKS.md`, Docs/Data/Prompts/Tools/Reports находятся
в родительском Git root, вне Unity Assets. Не создавать конкурирующие копии.

`Assets/_Cascade/` подготовлен по C17: Domain, Solver, Application,
Presentation, Content, Editor, Tests/EditMode и Tests/PlayMode. Игрового C#
кода и собственных asmdef пока нет. Единственный script — Editor-only
`ProjectBootstrap/PackageRepair.cs`, явно вызываемый helper восстановления
первоначального Input System. Автоматических package-изменений он не делает.

В build settings включена только штатная `Assets/Scenes/SampleScene.unity`.
Это scene шаблона; boot/gameplay flow CASCADE ещё не существует.

Плановая архитектура по C17: чистые C# Domain и Solver, один общий resolver;
Application управляет попытками/историей, Presentation показывает final-state.
uGUI/TMP, input adapter и один tween adapter. Эта архитектура пока не реализована.

## Канон и визуальный источник

Один постоянный ряд проходит через этажи и должен точно совпасть с приёмником.
Нет сетки, очистки совпадений, спавна или gameplay physics. EN — основной язык,
RU/UK — дополнительные; Shapes/Words/Mixed используют единый SymbolId.
Atomic swaps сохраняют multiset; contacts проверяются по final row.

Автор подтвердил визуальный источник:
`/Users/andrew/Desktop/unity projects/unity_projects/shape_sorting-final`.
Его Assets просматривались только для определения источника; они не переносились.
Фактические модели, материалы, шрифты и текущие экраны нужно проверить в P12.
Impeccable context выполнен один раз; PRODUCT/DESIGN и собственный UI отсутствуют.

## Проверки и инструменты

- Итоговый Unity headless import/compile: exit 0, C# ошибок 0.
- Asset integrity: 32 .meta GUID, нет missing/orphan .meta и duplicate GUID.
- Python reference: 39 тестов OK; existing data audit — 801 assertion,
  40 конфигураций, 117 решаемых языковых вариантов, 111 слов.
- EditMode/PlayMode tests, C# parity, player builds и device/visual/glyph QA
  не выполнялись: игровой реализации CASCADE и её tests пока нет.
- Unity CLI `1.0.0-beta.5` доступен. Команды `projects verify` в этой версии нет;
  проверка .meta/GUID выполнена напрямую по файлам, CLI не обновлялся.
- В начале Unity MCP отвечал для ShapeMania, а не CASCADE. Позднее live-запрос
  перестал отвечать. Для CASCADE live MCP не подтверждён. До любых будущих
  изменяющих MCP-команд обязательно повторно проверить project root.
- Все проверки и package baseline: `Reports/P00/`. Причины восстановления
  шаблона: `Docs/P00_SETUP.md`. Новый Editor, второй MCP или другие SDK не
  устанавливались.

Пока неизвестны: конечная пригодность визуала, производительность на телефоне,
поведение drag, glyph coverage и C#/Python parity. Headless-импорт не является
визуальной или игровой приёмкой. Следующий этап по TASKS — P01; он не начат.

Источники: AGENTS, README, TASKS, P00, C01/C02/C17/C20, реальные Unity
ProjectSettings/Packages/Assets и журналы выполненных проверок.

<!-- unity-onboarding:generated:end -->
