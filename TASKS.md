# CASCADE — TASKS

P00–P01: COMPLETE. Unity AI/MCP: исправлено. P02: IN_PROGRESS. P03–P20: NOT_STARTED.
Python reference проверен отдельно; это не реализация игровых систем Unity.

Автор прямо разрешил выполнять все этапы подряд, отменив остановки между
P01–P20. После каждого этапа фиксировать результаты и проверки; визуал и
взаимодействие проверять в Unity через MCP. Интересность требует отдельно
обозначенного человеческого плейтеста. Прочие ограничения канона сохраняются.

- [x] P00 — Зафиксировать проект
- [x] P01 — Формат уровня и символы
- [ ] P02 — Базовый resolver
- [ ] P03 — Конвейер и бомба
- [ ] P04 — Ключ и ремонт
- [ ] P05 — Хрупкость и связанные пары
- [ ] P06 — Мост и печать
- [ ] P07 — Solver и метрики
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
