# CASCADE — TASKS

P00–P05: COMPLETE. Unity AI/MCP: исправлено. P06: mechanics verified (dependent gates pending). P07: COMPLETE. P08: IN_PROGRESS. P09–P20: NOT_STARTED.
Python reference проверен отдельно; это не реализация игровых систем Unity.

Автор прямо разрешил выполнять все этапы подряд, отменив остановки между
P01–P20. После каждого этапа фиксировать результаты и проверки; визуал и
взаимодействие проверять в Unity через MCP. Интересность требует отдельно
обозначенного человеческого плейтеста. Прочие ограничения канона сохраняются.

- [x] P00 — Зафиксировать проект
- [x] P01 — Формат уровня и символы
- [x] P02 — Базовый resolver
- [x] P03 — Конвейер и бомба
- [x] P04 — Ключ и ремонт
- [x] P05 — Хрупкость и связанные пары
- [ ] P06 — Мост и печать
- [x] P07 — Solver и метрики
- [ ] P08 — История и подсказки
- [ ] P09 — 40 данных и три языка
- [ ] P10 — Редактор уровней
- [ ] P11 — Серый playable и ввод
- [ ] P12 — Визуальный эталон
- [ ] P13 — Event player и juice
- [ ] P14 — Обучение и экраны
- [ ] P15 — Локализация и glyph QA
- [ ] P16 — Save и прогресс
- [ ] P17 — Мастерство и маленькая мета
- [ ] P18 — Интеграционная приёмка
- [ ] P19 — Реклама и IAP позже
- [ ] P20 — Расширять задачами

## Unity AI/MCP — исправление 5 октября 2026

- Причина 999+ ошибок: Assistant 2.20.0-pre.1 и deprecated Generators
  1.7.0-pre.1 содержали одни ассеты с 1365 совпадающими GUID.
- Через Unity Package Manager удалён только отдельный Generators.
  Assistant 2.20.0-pre.1, Inference 2.6.1 и прежние package pins сохранены.
- Повторный импорт завершён; свежая Console через MCP и визуально: 0 ошибок,
  1 warning Account API. В 16573 GUID активных packages/Assets нет дубликатов.
- Официальный MCP подтверждён именно для CascadeUnity: root probe, состояние
  Editor, Console и компиляция/исполнение диагностической команды прошли.
  Project Settings показывает Running и принятые клиенты Codex.
- Встроенное окно Assistant открывается; для облачного чата/генераторов нужна
  отдельная привязка Unity Cloud. Автор создал CascadeUnity в shapeinc25;
  привязка проверена, поле ввода Assistant активно. В UI отображается 10%
  остатка credits организации. Облачный запрос, player build и gameplay QA
  в этом исправлении не запускались.
- Свидетельства: `Reports/AI_REPAIR/`; решение: `Docs/IMPLEMENTATION_NOTES.md`.

## P00 — результат 4 октября 2026

- По выбору автора создан отдельный `CascadeUnity/` на установленной
  Unity 6000.3.8f1; используются локальный 3D URP template и локальный Git
  в корне пакета. Файлы чужих игровых проектов не редактировались.
- Подтверждены persistent-row, точный receiver, EN/RU/UK и общий SymbolId
  для Shapes/Words/Mixed. Баланс, игровые правила, схемы, ID и цены сохранены.
- Стандартный шаблон содержал несовместимый Input System 1.12.0:
  CS0117 / BuildTarget.ReservedCFE. Через Unity Package Manager закреплён
  Input System 1.18.0. Причина и процедура: `Docs/P00_SETUP.md`.
  Другие прямые зависимости шаблона не изменены; временный embedded override
  перенесён за пределы проекта и исключён из Git.
- Итоговый headless-импорт Unity: exit 0, 0 ошибок C# компиляции.
  Проверены 32 GUID .meta: нет пропусков, orphan .meta и дубликатов.
- Python: 39 unit-тестов OK; аудит — 801 assertion, 40 основных уровней,
  117 решаемых EN/RU/UK вариантов, 111 слов. Data/Tools и прежние отчёты сохранены.
- Отчёты: `Reports/P00/`; текущий контекст: `Docs/UnityProjectContext.md`.
- Автор подтвердил `shape_sorting-final` как свою ShapeMania и визуальный
  источник. Графика CASCADE и игровые сцены ещё не создавались.
- Не запускались: C# parity, игровые EditMode/PlayMode-тесты, player build,
  device/input QA, визуальная/glyph QA, профилирование, save и SDK.
  В проекте пока нет тестов и игрового кода CASCADE для этих проверок.
- Impeccable context прочитан; отсутствуют PRODUCT/DESIGN и UI-реализация.
  Визуальные решения и необходимые ассеты относятся к P12 после проверки
  реальных материалов ShapeMania.
- Следующий этап — P01. Он не начат согласно ограничению `Prompts/P00.md`:
  «Не начинать следующий этап».

## P01 — результат 5 октября 2026

- Созданы чистая C# сборка Domain, typed SymbolId и immutable definitions:
  уровни, этажи, устройства, linked pair, entry transform, target segments и слова.
- Валидатор проверяет multiset, row 3–8, floors 1–4, числовые диапазоны,
  disjoint links, bridge/key, sensor slots/symbols и полное покрытие target.
- Короткие и длинные namespaces адаптированы без изменения исходных ID;
  original PersistedId сохранён. Фигуры и глифы не равны друг другу.
- Реальные Unity EditMode: 40/40 PASS; C# компиляция без ошибок.
  Семисимвольный Mixed принят. Проверены NFC, EN/RU/UK и surrogate pair.
- Полная Unicode grapheme conformance, mobile, PlayMode и player build
  не проверены. JSON import/parity относятся к следующим этапам.
- .meta созданы Unity; целостность и результаты: `Reports/P01/`.
  Следующий этап P02 начат по прямому разрешению автора продолжать подряд.

## P02 — результат 5 октября 2026

- Pure C# resolver: adjacent Swap/Early Drop, signed durability, общий вход,
  точный receiver и immutable snapshots/events. Invalid/locked/no-op и terminal
  команды возвращают тот же snapshot без цены и событий.
- Последний Swap сохраняет весь ряд и доставляет его в receiver; автоматическое
  обрушение не добавляет вторую команду. Drop принят один раз и стоит 0 HP.
- Unity EditMode: 50/50 PASS, включая 4200 команд проверки multiset/stable HP.
  Компиляция и свежая Console без ошибок. Отчёты: `Reports/P02/`.
- Устройства добавляются P03–P06; gameplay scene, visual, PlayMode, player build
  и C#/Python content parity ещё не запускались.

## P03 — результат 5 октября 2026

- EntryTransform выполняется один раз до contacts: left/right shift и mirror,
  как в immutable enum и Python reference. Вход не снимает такт бомбы.
- BombPhase/счётчики разделены: Defused, Exploded и Abandoned. Defuse раньше
  tick; Drop обрабатывает старую бомбу, overkill не переносится на новый этаж.
- Unity EditMode: 62/62 PASS. Last-tick defuse, useful blast, вход/посадка,
  no-op/preview invariants и один FloorExited проверены; C# ошибок нет.
- Пауза проверена на уровне pure state (нет clock callbacks), но фактический
  lifecycle/pause в playable, visual, PlayMode и player build ещё не запускались.
  Отчёты: `Reports/P03/`.

## P04 — результат 5 октября 2026

- Key/Repair одноразовые на этаж; общий вход проверяет их после transform.
  Закрытый PRE-edge нельзя открыть задним числом; новый этаж сбрасывает флаги.
- Signed deltas суммируются до одной проверки collapse. Обязательный
  HP1−cost2+repair1=0 проверен через каноническую цену хрупкого ребра (см. note).
- Predict/apply используют один pure Resolve; preview цены не меняет snapshot.
- Unity EditMode: 72/72 PASS, C# ошибок 0. Включены составной defuse+key+repair,
  repair+blast, force Drop и one-shot. Отчёты: `Reports/P04/`.
- Не запускались: игровые PlayMode, visual/device QA, player build, data parity.

## P05 — результат 5 октября 2026

- Linked set проверяется полностью до движения; обе перестановки атомарны,
  contacts видят final row. No-op определяется по всему ряду.
- Цена 1+crossedFragileCount одинакова в preview/apply: обе fragile дают 3,
  включая одинаковую подпару. Один linked ход даёт один command/event/tick.
- C25/C26/C28 победные fixtures и C27 unlock-before-linked проверены.
- Unity EditMode: 82/82 PASS, C# ошибок 0. Отчёты: `Reports/P05/`.
  Визуальная читаемость/ввод, PlayMode, build и общий JSON parity ещё не проверены.

## P06 — механики проверены 5 октября 2026

- Single-use bridge расходуется только принятой полной перестановкой,
  закрывается после linked set. Drop/invalid/no-op его не расходуют.
- StampBits собираются после final row/entry transform, не меняют tokens/HP.
  PuzzleGoal оценивает stamped delivery отдельно; базовый receiver не требует stamp.
- Unity EditMode: 90/90 PASS, C# ошибок 0. Независимый test-only BFS подтвердил
  C35 minima 2/4 и недостижимую печать; immutable snapshots восстанавливают bits.
- Отчёты: `Reports/P06/`. Полная приёмка P06 зависит от следующих инструментов:
  production solver/publication reject (P07/P10) и настоящая Application Undo (P08).
  Они не объявлены выполненными. PlayMode/visual/build также не запускались.

## P07 — результат 5 октября 2026

- Pure C# Solver использует тот же Resolve: полный BFS graph/reverse distances
  для authoring и ранний BFS из current snapshot для Hint. Бюджет 200000;
  budget/cancel → Unknown без ложных dead-state claims. Инверсии не эвристика.
- Все 40 реальных base JSON configs совпали с Python: minima, выбранные paths,
  states/transitions, dead depth/count, near choices, forced ratio, terminal variants.
  Итого 4228 states, 16136 transitions. Linked aliases считаются одной choice.
- C35 production minima 2/4. Недостижимый контракт/Unknown не дают publication proof.
- Unity EditMode: актуальная сборка 138/138 PASS, C# ошибок 0. При первом
  импорте не хватало явной ссылки Newtonsoft в test adapter; исправлена,
  runner теперь запрещает запуск после failed/incomplete compilation.
- Отчёты: `Reports/P07/`. Publication UI/gate связывается в P10; настоящая Undo
  проверяется P08. Localized production import/117 variants — P09.
  Visual/PlayMode/mobile/player build пока не запускались.
