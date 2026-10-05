# Решения реализации

## P11 — проверка proof of concept и исправление ввода

По уточнению автора художественная работа ожидает завершения текущей проверки
по плану. Будущие ассеты CASCADE уникальны; ShapeMania — только ориентир рисовки
и близкой палитры. Сохраняются яркая мультяшная тема, компактные платформы,
явный приёмник и единая стилизация окружения/эффектов.

Новые PlayMode воспроизвели два дефекта настоящего InputSystem пути: потерю
владельца жеста при втором пальце на том же slot и ложный commit при
TouchPhase.Canceled. TokenSlot теперь сохраняет pointer ID, игнорирует другие
кнопки мыши и проверяет отмену. Core rules и стоимость команд не меняются.
Настройки фокуса InputSystem изолированы только внутри test fixture и
восстанавливаются в teardown; production по-прежнему прекращает ввод в фоне.

Hint применяется повторным нажатием собственной кнопки, освобождая точный
режим от перекрытия. Edge visibility получает доступ из Domain IsEdgeOpen
(общая существующая проверка), не из дублированной view-логики. Deliver —
только контекстное имя прежнего явного Drop на последнем этаже, не новый ход.

Слабая глубина части учебных fixtures зафиксирована в Docs/POC_REVIEW.md и
воспроизводимом read-only аудите. Существующие ID/Data/баланс не переписаны.
P20 должен добавить небольшую подборку осмысленных маршрутов на тех же правилах,
с отдельными proof/метриками и целью, а не просто увеличить число обменов.

## Продолжение плана

Автор 4 октября 2026 явно отменил остановки между этапами и поручил
последовательно выполнить весь план с проверкой механик, gameplay и UI/UX.
Приёмка и честные ограничения каждого этапа сохраняются.

## P01 — typed IDs и immutable definitions

Поля нового C# формата соответствуют C03/C12. Исходные JSON схемы и ID не
меняются. Короткие namespaces `s:`/`g:` адаптируются к typed Shape/Glyph;
значение символа не переименовывается и одинаковые буквы в разных сегментах
имеют одно равенство. PersistedId хранит исходное написание ID для экспорта.
Графемы нормализуются NFC; символы не разбиваются по UTF-16 char.

## P04 — signed repair fixture

Приёмка P04 требует фактический `HP1 − cost2 + repair1 = 0`. Поэтому формула
цены `1 + crossedFragileCount` подключена к resolver в P04 для одного ребра.
P05 добавляет полный atomic linked set и проверяет общую цену обеих связей.
Это порядок реализации существующего правила C04/C05, без изменения баланса.
Предсказание эффекта использует pure Resolve и не присваивает состояние попытке;
отдельный альтернативный resolver для preview не создаётся.

## P07 — reference glyph shorthand

Реальные base configurations C29/C30/C40 используют ещё более короткий `g:C`.
Он импортируется в glyph namespace с явно заданной PuzzleLocale (EN по умолчанию),
сохраняя исходный PersistedId. Localized `g:en:C`/`g:ru:К`/`g:uk:К` остаются
явными. Исходные JSON, значения символов, IDs и схемы не переписываются.
Test-only adapter генерирует target segment metadata для абстрактных base patterns;
production word bindings/metadata внедряются отдельно в P09.

## P09 — content adapter и общие типы

Исходные пять JSON копируются в Resources без изменения байтов. Отдельный
чистый C# адаптер выводит immutable metadata (schema/rules 2.0 и SHA256),
segment boundaries из существующих word_ids/словарей. Existing IDs сохраняются.
Localized rules наследуются от parent node, как в исходном reference-пакете.
PuzzleMode перенесён из Application в Domain для общих consumers, значения
Shapes=0/Words=1/Mixed=2 сохранены; сериализованных usage этого нового enum нет.
C26 остаётся shape-only, без искусственного binding. Права/редакторские статусы
словарей сохраняются; технический импорт не объявляет их release-ready.

## Unity AI и MCP для CASCADE

Пользователь установил официальный Assistant `2.20.0-pre.1`, Generators
`1.7.0-pre.1` и Inference `2.6.1`, затем поручил исправить 999+ ошибок и
наладить MCP. Проверка Console, Editor.log и .meta обнаружила 1365 GUID,
совпадающих между Assistant и Generators. Package Manager помечает отдельный
Generators deprecated и прямо сообщает, что Assistant включает все его функции.

Решение: удалить только `com.unity.ai.generators` через Package Manager,
сохранить установленный Assistant `2.20.0-pre.1` и Inference `2.6.1`.
Без переименования GUID, удаления пользовательских ассетов или второго MCP
провайдера. Прочие пакеты не обновлять. До MCP мутаций подтвердить project root
CASCADE; подключение ShapeMania не использовать для изменений нового проекта.

## P10 — authoring proof

Новый Editor-only черновик хранит editable данные в hidden ScriptableObject с Unity Undo.
Экспорт использует прежнюю JSON структуру уровня; отдельный `*.proof.json` имеет
новый локальный формат `cascade.authoring-proof/1` с schema/rules/hash, goal bits,
minimum и command path. Он не меняет исходные Data, ID или runtime schema.
Proof проверяется повторным Resolve replay и отклоняется после изменения draft/goal,
при Unknown или Unsolvable. Поиск работает вне Editor thread с бюджетом и cancel.

## P11 — UI и ввод

uGUI/TMP используют штатные TMP essentials, импортированные официальным API
без examples и без package changes. New playable имеет отдельный scene, одну
Camera/Canvas/EventSystem и стандартный InputSystemUIInputModule. SampleScene
сохранена в build list после новой CASCADE сцены. Введён pure Application input
gate: один pointer, один accepted command, без очереди. Gray presentation только
закрывает/открывает gate; commit выполняется до visual callback. Target не меняется
из-за жестов. Временные [A] placeholders различают shapes и glyphs до P12.
