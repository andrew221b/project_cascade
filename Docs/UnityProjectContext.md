# CASCADE — текущий контекст Unity

<!-- unity-onboarding:generated:start -->

Обновлено: 5 октября 2026, Europe/Sofia. P00–P11 COMPLETE; P12–P20 NOT_STARTED.

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
- Прямые зависимости шаблона сохранены; установленные автором Assistant и
  Inference сохранены, отдельный deprecated Generators удалён при AI repair.
  Не считать Navigation/Timeline/Visual Scripting частью архитектуры CASCADE
  только из-за их наличия в defaults. SDK рекламы/IAP/сервисов не добавлялись.
- Последний проверочный импорт запускался с `StandaloneOSX` и `-nographics`.
  Player ни для одной платформы не собирался. Мобильная сборка не проверена.

## Источники и код

Авторские `AGENTS.md`, `TASKS.md`, Docs/Data/Prompts/Tools/Reports находятся
в родительском Git root, вне Unity Assets. Не создавать конкурирующие копии.

`Assets/_Cascade/` следует C17. Domain, Solver, Application и Content — чистые
C# сборки без UnityEngine. Один Resolver обслуживает gameplay, preview и BFS.
Typed SymbolId, immutable states/definitions, все устройства, Attempt/Undo/Hint,
40 primary/117 localized/111 words и production import реализованы P01–P09.
Editor/Authoring содержит UI Toolkit Workshop: Validate/Solve/Replay/Export и
version/hash/goal proof gate. Editor/Validation пишет EditMode/PlayMode XML.
ProjectBootstrap/PackageRepair остаётся только явно вызываемым helper.

Первой в build settings добавлена `Assets/_Cascade/Scenes/CascadePlayable.unity`;
SampleScene сохранена следом. Gray Presentation использует uGUI/TMP, один
EventSystem/InputSystemUIInputModule и pure input gate. Один persistent row,
видимые этажи/полный target, drag/tap-tap, precise focus и бесплатные hint/Undo.
Home/onboarding/save/meta, окончательный визуал и event juice ещё не реализованы.

Application commit предшествует presentation. P11 gray gate не меняет core-state;
полноценный единый event/tween player относится к P13. Новые ассеты P12 ещё не
генерировались. TMP essentials импортированы штатным API, с OFL/attribution.

## Канон и визуальный источник

Один постоянный ряд проходит через этажи и должен точно совпасть с приёмником.
Нет сетки, очистки совпадений, спавна или gameplay physics. EN — основной язык,
RU/UK — дополнительные; Shapes/Words/Mixed используют единый SymbolId.
Atomic swaps сохраняют multiset; contacts проверяются по final row.

Автор подтвердил визуальный источник:
`/Users/andrew/Desktop/unity projects/unity_projects/shape_sorting-final`.
Его Assets просматривались только для определения источника; они не переносились.
Фактические модели, материалы, шрифты и текущие экраны нужно проверить в P12.
Impeccable context выполнен один раз; PRODUCT/DESIGN ещё не созданы. Gray UI
служит проверке ввода; окончательное визуальное направление относится к P12.

## Проверки и инструменты

- P00 Unity headless import/compile: exit 0, C# ошибок 0.
- .meta создаёт Unity; последняя проверка Assets не нашла missing/orphan.
- Python reference: 39 тестов OK; existing data audit — 801 assertion,
  40 конфигураций, 117 решаемых языковых вариантов, 111 слов.
- P11 EditMode 305/305 и PlayMode 12/12 PASS. Все 40 primary пройдены через UI: 92 команды. C# совпал с Python по всем 40/117
  графам, минимумам/путям/метрикам. Виртуальные Mouse/Touchscreen прошли через
  настоящий Input System/EventSystem; физический телефон и player build не проверены.
- Unity CLI `1.0.0-beta.5` доступен. Команды `projects verify` в этой версии нет;
  проверка .meta/GUID выполнена напрямую по файлам, CLI не обновлялся.
- 5 октября Unity AI конфликт исправлен: удалён deprecated отдельный Generators,
  который дублировал 1365 GUID встроенных в Assistant генераторов. Установлены
  Assistant 2.20.0-pre.1 и Inference 2.6.1; свежая Console: 0 ошибок и один
  warning облачного Account API. 16573 активных GUID проверены без дубликатов.
- Live MCP теперь подтверждён для CASCADE: root probe, Editor state, Console,
  диагностическая C# команда и повторный импорт. Мост Running; Codex Accepted.
  До изменяющих MCP-команд после переключения проекта повторить root probe.
  Автор привязал CascadeUnity к отдельному Unity Cloud project в shapeinc25;
  поле ввода встроенного Assistant активно. Облачный запрос не отправлялся.
- Все проверки и package baseline: `Reports/P00/`. Причины восстановления
  шаблона: `Docs/P00_SETUP.md`. Новый Editor, второй MCP или другие SDK не
  устанавливались.

P11 завершён после повторной проверки. Mac разблокирован, Refresh завершил
импорт; C# compiled, свежая Console без ошибок. Device Simulator закрыт,
поток его NRE после domain reload не повторяется; повторное открытие самого
Simulator не проверялось. Portrait captures C01/C38/C32 Mixed8/C37 Words
проверены через официальный MCP. Это технический gray UI, не финальный visual.

Исправлены второй палец на одном slot, отменённый touch над соседом, вторичная
кнопка мыши при удерживаемом жесте, перекрытие precision controls кнопкой hint,
устаревшая подпись использованного моста. Explicit Drop/Deliver сохраняет правила.
POC audit: Docs/POC_REVIEW.md, Tools/audit_concept.py, Reports/P11/concept_audit.json.
Автор требует будущие уникальные assets CASCADE: ShapeMania только reference
рисовки/близкой палитры; яркая мультяшная тема, компактные платформы, явный
приёмник и связная стилизация окружения/эффектов. Сначала завершить POC по плану.

Пока неизвестны: окончательный визуал, glyph coverage, mobile performance,
реальный device drag и человеческая оценка интереса. Отчёты: Reports/P00–P11.
Следующий этап — P12, он ещё не начат.

Источники: AGENTS, README, TASKS, P00, C01/C02/C17/C20, реальные Unity
ProjectSettings/Packages/Assets и журналы выполненных проверок.

<!-- unity-onboarding:generated:end -->
