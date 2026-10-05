# CASCADE — TASKS

UI/material follow-up, 5 октября 2026: деревянные боковые рейки связывают доски и приёмник; формы получили лаковый блик от Lit-материалов, буквы — лёгкое волокно и рельефную тень, доски принимают мягкую тень от направленного света. Увеличен планер, карта получила читаемую метку текущего острова и ровный верхний бар, меню — иллюстрированный вид из мастерской и тихую пыль, а пауза и настройки — Мику. Плашка уровня отделяет текст от опор; цель поднята над приёмником на узком экране. 7 прицельных кадров через Unity MCP: Reports/Polish/finish-*.png; дополнительно повторно осмотрены три меню с иллюстрированной шапкой. Один focused swap: 1 атомарный ход, multiset сохранён, анимация закончилась Resting; Console 0 ошибок после перезапуска Play Mode. Реальные iPhone/Android, измерение FPS и человеческая оценка удовольствия не выполнялись; нового player build не делали по просьбе автора об экономии проверок.

UI/motion polish: IMPLEMENTED + VERIFIED локально — единые меню, читаемые языки и счётчики, акцент действий, плавные канаты с объёмной оплёткой, сохранение веса доски, мягкая посадка/погода/переходы. 47 native stills +32 motion frames;15 distinct PlayMode cases имеют passing evidence, финальный affected run4/4 PASS (13×4 interruption paths). Первоначальные watchdog failures сохранены: измерен Editor1Hz, функциональный watchdog30s; это не performancePASS. Одна новая CASCADE-Polish.app:0errors/346 existing warnings. Mac locked; foreground player и physical devices NOTRUN. Reports/Polish/README.md.

Devices revision: IMPLEMENTED + VERIFIED локально — узнаваемые устройства, предпросмотр эффекта при перетаскивании,6 B01–B06 с катушкой/часами и обучение; карта без карточек/номеров, закрытые острова чёрно-белые, стрелка «Вы здесь».146 distinct EditMode PASS;6 distinct PlayMode имеют passing evidence,2 первоначальных timeout сохранены в отчёте.14 MCP кадров; независимый scoped visual review:ship. Новая CASCADE-Devices.app готова:0errors/346warnings. Native окно заблокировано lock-screen Mac; телефоны и человеческая понятность/интересность NOTRUN. См. Reports/Devices/README.md.

Craft revision: IMPLEMENTED + VERIFIED локально — меню/вещи/карта, непрерывные канаты, контакт фигур с деревом,6 испытаний с лентой. Новая Mac-сборка и краткий замер готового player готовы. Нативное окно новой сборки ожидает разблокировки Mac; телефоны/человеческий плейтест не проверены. См. Reports/Craft/README.md.

P00–P20: COMPLETE в доступном объёме. Unity AI/MCP: работает. Авторская visual/motion/world доработка: COMPLETE локально; готова Mac-сборка для плейтеста.
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
- [x] P06 — Мост и печать
- [x] P07 — Solver и метрики
- [x] P08 — История и подсказки
- [x] P09 — 40 данных и три языка
- [x] P10 — Редактор уровней
- [x] P11 — Серый playable и ввод
- [x] P12 — Визуальный эталон
- [x] P13 — Event player и juice
- [x] P14 — Обучение и экраны
- [x] P15 — Локализация и glyph QA
- [x] P16 — Save и прогресс
- [x] P17 — Мастерство и маленькая мета
- [x] P18 — Интеграционная приёмка
- [x] P19 — Реклама и IAP позже (локальные границы; реальные SDK выключены)
- [x] P20 — Расширять задачами (120 новых задач;8-region визуал завершён)

## Devices — 5 октября 2026, текущий проход

- [x] Удалены непонятные бусины прочности/фитиля, кружки вместо бомбы и отдельные треугольники конвейера.6 новых оригинальных спрайтов, крепления к доскам, отдельные картинки нужной фигуры/буквы, читаемые счётчики.
- [x] Предпросмотр при drag показывает эффект на устройстве, не меняя состояние. Нажатие на устройство открывает его демонстрацию. Эффекты/звук/микроотклик реагируют только на уже подтверждённые события.
- [x]6 новых отдельных B01–B06: катушка для одного дальнего обмена; часы пропускают2 тика бомбы, включая ход получения. Solver/Undo/save/replay и multiset проверены. Старые C/R IDs/уровни/цены не изменены; note Docs/DEVICES_REWORK.md.
- [x] Обучение:11 устройств и6 анимированных задач с бонусами. Тексты EN/RU/UK. Исправлена случайная локализация букв в гнёздах; regression трёх языков PASS.
- [x] Карта: острова вместо числовых карточек; доступные цветные, закрытые обесцвечены; текущая точка со стрелкой «Вы здесь». Убраны pebble-путь и накладки поверх островов.
- [x]146 distinct EditMode PASS (21 +137 с пересечением);6 distinct PlayMode имеют passing evidence. Первоначально3/5,2 timeout без изменения лимита затем2/2; отдельный glyph1/1 и финальный preview2/2. Сводка не означает единого полного6/6 запуска.
- [x]14 финальных MCP кадров, включая320RU/UK, wide, до/после часов и предпросмотр.5 замечаний независимого Impeccable reviewer исправлены и подтверждены. Shared-glyph ошибка дополнительно исправлена/проверена основным агентом.
- [x] Одна новая CASCADE-Devices.app: Succeeded,0 ошибок/346 прежних package/config warnings,238MiB на диске. Старые приложения сохранены.
- [ ] Окно нового standalone: CUA подтвердил заблокированный Mac; NOTRUN. MCP-проверка сцены завершена.
- [ ] Реальные телефоны, производительность нового player, haptics/звук, длительная сессия и человеческое понимание/интересность — NOTRUN.

## Craft — 5 октября 2026, актуальный визуальный проход

- [x] Мастерская/вещи: полки,4 одновременных выбора, Мика и посылка, выровненные EN/RU/UK подписи.2 оригинальных сгенерированных фона, provenance сохранён.
- [x] Единый wide-фон; мягкие края карты; атмосферные слои; широкие боковые копии старого прохода заменены.
- [x] Непрерывные канаты, прогиб/вес, обмен/падение/посадка, контактная тень на древесине, щепки/трещины/круглые метки износа, свет. Физика только визуальная.
- [x]6 отдельных R01–R06: одноразовая дальняя перестановка, Undo/save/solver. Исходные 160 уровней/цены/пакеты не менялись; онлайн остался планом.
- [x]475/476 — полный Edit первоначально; единственный провал R03 исправлен,9/9 Reach PASS.7/7 Play PASS;3/3 после последней поправки контакта (подмножество 7). Полный повтор не запускался.
- [x]18 native кадров, 20 реальных кадров движения;7 выявленных визуальных замечаний закрыты независимым reviewer. Документы/sidecar синхронизированы.
- [x] Один свежий CASCADE-Craft.app:0 ошибок / 346 предупреждений пакетов, 236 MiB. Player C160, 1440×900, M4 Pro: median 16.703 ms, p95 17.520 ms / 240 кадров. GC counter недоступен; Mac был заблокирован, активное окно не подтверждено.
- [ ] Проверить окно новой standalone-сборки после разблокировки Mac (MCP-сцена уже проверена); реальные Android/iPhone, долгая сессия, нагрев, человеческая понятность и интересность — NOTRUN.

## Woodland и финальная локальная сборка — 5 октября 2026

- Дерево/канаты/прогиб/ветер/щепки, состояния фигур, падение и посадка реализованы как presentation simulation. Порядок и контакты определяет прежний resolver; физика не меняет головоломку.
- Восемь регионов,160 остановок,33 страницы; пять новых пар фон/карта, тематические слои погоды/облаков/листвы/планёра. Пропеллер убран. На wide — составные боковые сцены и Мика.
- Девять наглядных демонстраций устройств, actual fuse бомбы, damage forecast, чистый HUD/карта/сумка/финал;393 UI keys/1179 EN/RU/UK entries. Предсказание следующего fuse и Long Reach не реализованы.
- 5/5 узких EditMode,16/16 focused PlayMode и4/4 affected PlayMode PASS (17distinct Playcases). P20 полный466/466 иgraph parity сохранены; лишние повторные прогоны/сборки не делались.
- Native MCP matrix и27event-start кадров, финальные guide phases. Независимый Impeccable reviewer закрыл все4scored замечания после2fix rounds: scoped ship. Tokens/документация обновлены; старые4sidecar prose хвоста исправлены по текущему локальному профилю.
- Одна Mac universal сборка Succeeded,0errors/346installed-package warnings,~227MiB. CUA проверил карту→историю→C01→реальный drag→падение→победу1ход. Player.log без exception/error на этом checkpoint. Первичная shell-launch ошибка сохранена; native запуск успешен.
- Reports/Woodland/README.md и Reports/FinalPlayer/README.md — итоговая evidence. CASCADE.app сохранена в CascadeUnity/Builds, не включена в Git. Cloud-enable изменение пользователя сохранено отдельно.
- NOT RUN: телефоны/производительность/haptics, прослушивание, native редактура, понимание детьми, удовольствие/retention, store/sign/notarization/live SDK. Онлайн только Docs/ONLINE_IDEAS.md, интересность не доказана автоматическими тестами.

## P18 — доступная интеграционная приёмка завершена, 5 октября 2026

- Полные reachable state/acceptance traces совпали C#/Python для157concrete
  puzzles:16900states,139376commands,64532accepted. Invalid/no-op/terminal
  commands включены, first_diff:null, source hashes/per-puzzle digests сохранены.
- Reused P17 evidence47distinct tests: real input, every animated stage
  interruption, focus/pause/disable, bounded tails, reload/save. Pure rules unchanged.
- Large/highcontrast320RU Mixed8/bombguide, UKmap проверены. Исправлены перенос
  Hint→Помощь/Поміч и map page captions→drawnchevrons.3targeted testsPASS:
  pointermap/preferences +directUILocale preservation. Второй полный suite не запускался.
- Fresh Impeccable scopedship для captions/chevron; documenter обновилnative
  guidance, tokens/sidecar preserved.3final frames/round1 evidence; fit0.
- Один120frame Editor sample:median99.97ms/p95102.08ms, statehashunchanged.
  Это не mobile/performancePASS; wholeEditor memory, foreground uncontrolled.
- Device inventory не прочитан через commandAPI; не утверждаем0devices.
  Physicalphone/profile/haptics/OSkill/humanfun NOTRUN. Одинfinalplayerbuild
  выполняется послеP20 и author-requested visualrevision, чтобы не повторятьbuilds.
- Reports/P18 и Docs/P18_INTEGRATION.md содержатactualevidence/границы.

## P17 — завершено в заявленном объёме, 5 октября 2026

- Каждые три новых clear — сохранённая деталь известного подарка. Три детали
  собирают вещь; выбор меняет следующий заказ, pending reserves capacity,
  claim идемпотентен, максимум 12 посылок без повторного filler.
- Четыре миниатюры требуют exact ten-node receipts; C26 skip честно остаётся9/10.
  Три просьбы считают ремонт/мост/печать в успешных доставках разных узлов.
  Ранг хранителя и семь local visits без сброса. Local records pin content,
  mode/locale/goal и отдельно no-hint/clean. Optional stamp не блокирует кампанию.
- Profile schema1 совместим с отсутствующими новыми полями; meta commit до
  animation. Посылки/вещи/журнал доступны вне компактного gameplay HUD.
- 333/333 EditMode PASS; full PlayMode46/47, устаревшее ожидание Map→Settings
  заменено Map→Bag→Settings и targeted1/1PASS. Все47 distinct tests имеют passing
  evidence, второй полный прогон не делался. Reports/P17 и P17Focused.
- 44 final native MCP frames:41 bounded matrix +3 reviewer-requested carousel
  frames, EN/RU/UK320, portrait390 и wide1440. Fit findings пусты. 297 keys/891entries.
  Девять original alpha assets, prompts/provenance сохранены. Console0;330meta чисты.
- Fresh Impeccable review:два finding(документация/3assetframes), оба resolved,
  scoped ship. Documenter обновил PRODUCT/DESIGN, tokens/sidecar byte-preserved;
  pre-existing sidecar drift reported. Reports/P17/impeccable-verdict.md.
- Не запускались player build/physical devices/haptics/native editorial/humanfun.
  Основные40уроков всё ещё короткие; P20 расширит решения, LongReach покаdesign.
  Wooden/motion/pictorial/world revision выполняется послеP17–P20.

## Уточнение автора — следующие работы после P17–P20

- [x] Деревянные платформы на канатах, вес/прогиб/ветер, щепки и посадка фигур.
- [x] Наглядные состояния бомбы/устройств, pictorial guidance, microinteractions.
- [x] Перекомпозиция широкого фона и пропеллера, живые карта и меню с Микой.
- [x] Дополнительные понятные цели/награды: просьбы, мастерские, известные подарки и печати.
- [ ] Человеческий плейтест: вовлечение, удержание, понимание ребёнком без чтения.
- Контракт до изменения поведения: Docs/VISUAL_MOTION_REWORK.md.

## P16 — завершено в заявленном объёме, 5 октября 2026

- Локальный schema1 profile: preferences/mode/heard story, exact pinned attempt,
  remaining command log, UndoCount/HintUsed и CampaignNodeId clear receipts.
  Запись после commit до animation. Возврат фокуса не повторяет entry transforms.
- Проверяемый replay восстанавливает final hash и Undo history. 157 initial states
  и 365 accepted prefixes проверены; optional stamp goal и UK symbols сохраняются.
  Язык/режим/повтор/Undo победы не дают повторный first-clear receipt.
- SHA256, flushed temp, atomic replace, один valid backup. Повреждённый оригинал
  архивируется, newer schema read-only. Невосстановимая попытка не подменяет слово:
  meta остаётся, игрок явно выбирает новую задачу. Ошибка записи останавливает игру
  до успешного retry; committed row не теряется в памяти.
- EditMode **323/323 PASS**, финал 08:28:57–08:29:00 UTC; PlayMode **43/43 PASS**,
  08:22:08–08:24:05 UTC. После PlayMode менялись только recovery copy/button label
  и Editor capture harness; EditMode повторён. Реальные scene/disk/input tests
  покрывают прерывание анимации, reload после landing, preferences/story и retry.
- 15 final native Editor recovery fixtures на 320×693, EN/RU/UK, два bounded passes;
  fit scan пуст, все кадры просмотрены. 224 UI keys / 672 entries. 200 .meta чисты;
  Console 0 errors/warnings. Это UI poses, отдельно от функциональных disk tests.
- Reviewer принял визуальную часть и запросил поправить устаревшее session-only
  описание. Documenter обновил PRODUCT/DESIGN без смены tokens. Тот же reviewer
  оценил единственное замечание resolved, disposition ship; Reports/P16.
- Save tests изолированы от реального профиля. Физический power loss/disk-full,
  mobile OS kill, полный player build, native editorial и human fun не проверены.
  40 уроков всё ещё короткие. P17 начат последовательно: мастерство и награды.

## P15 — завершено в заявленном объёме, 5 октября 2026

- Реальные Unity String Tables: 215 ключей, 645 EN/RU/UK значений. SelectedLocale
  и LocalizeStringEvent; буквенные SymbolIds обходят перевод. Язык меню не меняет
  текущий ряд, попытку, hash или историю. Язык слов подтверждается для новой задачи.
- Шрифты дополнены недостающими UI-глифами. Все UI и 111 слов проверены; NFC,
  reference-графемы, Ё/Е, І/Ї/И, Г/Ґ, Е/Є, O/circle и uppercase import покрыты.
  Повреждённые данные отвергаются без подстановки слова. Saved-attempt recovery — P16.
- Design note Docs/P15_LOCALIZATION.md до изменения packages: Localization 1.5.13,
  Addressables 2.8.1, Scriptable Build Pipeline 2.5.2. Правила/IDs/цены неизменны.
- Финальные EditMode **313/313 PASS**, 07:59:57–07:59:59 UTC; PlayMode **38/38 PASS**,
  08:00:31–08:02:22 UTC. Console 0 errors/warnings. 192 .meta без проблем.
- 39 native MCP Editor captures: 13 состояний × 3 языка, 320×693 / Words390×844.
  Два bounded passes: исправлены пустой translated join и пропущенная кнопка Menu.
  Финальный overflow/missing-string scan пуст. Независимый reviewer: **ship** только
  по P15 readability/consistency в этих кадрах; Reports/P15/impeccable-review.md.
- Addressables StandaloneOSX build PASS; исправлен Handheld guard для Mac compile.
  Это не полный player build. Native editorial, physical devices/haptics, extended
  ZWJ segmentation и human enjoyment не проверялись. Тексты — оригинальный черновик.
- P16 начат последовательно: durable profile/backup/receipts/pinned history. Нет
  объявления, что полная кампания, сложность, награды или весь продукт уже готовы.

## P14 — завершено в заявленном объёме, 5 октября 2026

- Прямое удержание/перетаскивание соседних фигур: фигура и буква поднимаются
  вместе, увеличиваются, сосед подсвечивается; отмена не меняет состояние.
  По умолчанию один Pause и Undo/Hint/Descend. Точный ввод только по выбору.
- Мика, Пип, Ру; карта 40 существующих уроков, короткие пропускаемые диалоги,
  задания по пути. Герой отмечает фактический прогресс; будущие зоны объясняют
  оставшийся путь. Незаконченный уровень продолжается без сброса маршрута.
- 12 оригинальных Journey assets: три карты/зоны, три персонажа и прозрачные
  облака/растения/планер. Раздельная анимация, видимое ветряное колесо,
  три цвета платформ. Планер ограничен свободной полосой и скрыт при hold.
- Новый scope и design note: Docs/JOURNEY_REDESIGN.md. Онлайн только концепция.
  160 задач — будущая цель. Сложность исходных 40, save, награды и прокачка
  этим этапом НЕ объявляются готовыми; они остаются в P15–P20.
- Финальные Unity EditMode **309/309 PASS** (07:17:43–07:17:58 UTC), PlayMode
  **36/36 PASS** (07:14:25–07:16:15 UTC). Первый прогон исправлений поймал
  доступ к удалённой камере при teardown; исправлен, repro XML сохранён.
- 22 native Editor captures: 320×693, 390×844, 1440×900; реальные input tests,
  all-phase glider bounds, story region, future map truth, pause/lifecycle.
  Button minimum 47.99997 rendered px; узкие symbol cells не равны Buttons.
- Свежая Console: 0 errors/warnings. 172 .meta: без пропусков/сирот/дубликатов.
  Точные prompts и provenance сохранены. Rules/IDs/prices/packages неизменны.
- Два bounded self-QA passes, один reviewer fix batch. Все четыре замечания
  оценены тем же независимым reviewer как resolved, disposition ship.
  Это приёмка исправлений P14, не всей игры.
  Свежий documenter обновил DESIGN.md/sidecar/PRODUCT/P14_FLOW.
- Progress/heard story пока session-only. Физические устройства, аппаратная
  вибрация, perf/player build, носители языков и human-fun review не запускались.
  Reports/Journey и Reports/P14 содержат свидетельства; далее P15 по разрешённой последовательности.

## P13 — event player и обратная связь, 5 октября 2026

- Добавлен отдельный visual frame и последовательный event player: обмены,
  связанные пары, входные сдвиги/зеркало, реакции устройств, разрушение,
  падение и посадка в приёмник. Resolver и gameplay callbacks не изменялись.
- Selection/mesh/glyph двигаются вместе. Target остаётся видимым при доставке;
  итоговые controls ждут завершения. Reduced Motion убирает travel/arcs/recoil/
  squash, сохраняя короткие подтверждения. Dust tail не удерживает Undo.
- MotionProfile, 32 pooled particles, 8 voices, 12 оригинальных SFX + 4 варианта,
  музыкальная петля 24 s и отключаемый haptic adapter. Provenance и signal audit
  сохранены; не заявляется прослушивание микса или проверка вибрации на телефоне.
- EditMode **305/305**; финальный PlayMode **21/21**. 40 уровней/92 команды
  через настоящий UI; 13 timed stages × 4 interruptions, destroy, scene reload,
  Reduced Motion, persistent target и bounded FX проверены. Найденный teardown
  NullReference исправлен; первый failing XML сохранён как repro.
- MCP снял 468 промежуточных кадров 12 сценариев, включая отдельный QA-only
  mirror fixture; репрезентативные кадры просмотрены. Максимальный замеренный
  offset glyph/body <0.000001 world unit. Два кадра до target-fix сохранены.
  Reports/P13 содержит XML, timeline/retained PNG, audio и meta audits.
- .meta: 145, без missing/orphan/duplicate GUID. Package pins, rules, IDs,
  данные и баланс не менялись. P14 начат по прямому разрешению продолжать.
- Не запускались: player build, реальные телефоны, performance/GC profiling,
  внешний human playtest, акустическая оценка и аппаратная вибрация.

## P12 — визуальный эталон, 5 октября 2026

- Создана оригинальная небесная мастерская: шесть beveled meshes, буквенная
  плитка, тонкие соразмерные платформы, золотистый receiver, общие URP-материалы,
  контактные тени, новый рисованный фон и декоративное ветряное колесо.
- ShapeMania использована только как просмотренный reference; ассеты не
  копировались. Balsamiq Sans взят независимо из Google Fonts, SIL OFL сохранён.
  Provenance и точный prompt: Docs/P12_ASSET_PROVENANCE.md и P12_BACKGROUND_PROMPT.txt.
- Реальные Unity проверки: EditMode **305/305**, финальный PlayMode **14/14**.
  Все 40 primary по-прежнему проходят через Input System/UI. Проверены freeze
  ambient при Reduced Motion/pause, неизменность state hash, magnifier и ввод.
- Через MCP проверены десять runtime captures: 390×844, 320×693, wide 1440×900,
  Shapes/Words/Mixed8, выбранные фигура/буква, target zoom и Reduced Motion.
  На этих кадрах controls ≥48px без пересечений, device text ≥14px.
- Независимое Impeccable-ревью: четыре material fixes исправлены одним batch;
  reviewer оценил все четыре как resolved, disposition ship в этом объёме.
  Это не release-приёмка всей игры. Reports/P12 содержит XML, кадры и измерения.
- Скомпилировано без ошибок; свежая Console пустая. 118 .meta без missing,
  orphan или duplicate GUID. Rules/Data/package pins/IDs/цены не изменены.
- Не проверены: физические iOS/Android, dp/pt и safe area на устройствах,
  player build, профилирование, полный glyph QA, внешний человеческий плейтест.
  Полноценные event-анимации/FX/audio относятся к P13; flow/onboarding к P14.

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

## P08 — результат 5 октября 2026

- AttemptService commit-ит только принятые команды. Predict не меняет состояние;
  History хранит стабильные immutable snapshots, RouteLength откатывается,
  UndoCount хранится отдельно. Retry сохраняет concrete Level/rules/content/locale.
- Undo через collapse восстановил этаж, HP, bomb phase/ticks/counts, bridge,
  key, repair и stamp. Это закрывает Undo-проверку P06. Publication UI остаётся P10.
- HintRequest можно вычислять отдельно от mutable attempt. SHA256 stateHash
  включает pinned content и все core поля; stale/другая attempt hint не исполняется.
  HintUsed монотонен до Retry. DeadEnd предлагает Undo; Unknown ничего не расходует.
- Unity EditMode: 151/151 PASS, успешная актуальная compilation. Исправлены
  два старых unqualified Application обращения в P00 helper при появлении namespace.
- Отчёты: `Reports/P08/`. Не запускались lifecycle/animation interruptions,
  visual/device input, PlayMode, save persistence и player build.

## P09 — результат 5 октября 2026

- Пять source JSON импортированы как Resources byte-for-byte. Чистый Content
  adapter создаёт immutable definitions, реальные WordId segments и SHA256,
  наследуя rules 2.0 от primary node. Исходные IDs/пакеты/схемы не переписаны.
- 40 primary, 117 localized, 111 words (EN40/RU35/UK36). Default locale en.
  C26 явно shape-only; отсутствует fabricated AAAB word/binding.
- Все 117 concrete graphs совпали с Python по minima/paths/полным метрикам:
  12672 states, 48396 transitions. 40 primary minima также проверены production import.
- Mixed7, Mixed8/two words и общие A между words проверены. Metadata сохраняет
  draft native-review и license-review flags. Это не редакторская release-приёмка.
- Unity актуальная compilation; EditMode 277/277 PASS. Resources реально загружены,
  source/asset bytes совпали. Отчёты: `Reports/P09/`.
- Не запускались fonts/glyph rendering, native-language review, PlayMode, mobile
  и player build. Editor/gameplay/UI следующие этапы.

## P10 — результат 5 октября 2026

- UI Toolkit Workshop: row 3–8, floors 1–4, edges/fragile/linked, entry, все
  устройства, dictionary word placement, Undo/Redo, Validate/Solve/Replay/Export.
- Solver выполняется вне Editor thread; 200000 states, cancel и stale draft gate.
  Warnings показывают on-entry и обходной ключ; метрики не объявляют fun доказанным.
- Export pin-ит schema/rules/hash и goal в отдельном proof, повторно проигрывает
  путь через Resolve. Unknown, unreachable stamp и устаревший proof отвергаются.
  Это закрывает publication gate P06; actual Undo проверена P08/P10.
- Unity текущая compilation; EditMode 291/291 PASS. Через UI создан QA_P10_001,
  Validate/Solve/Replay Won и настоящий Save dialog сохранили source + proof.
  C35 optional UI показал minimum4. Окно визуально проверено через native MCP.
- Координатные native clicks вернули noWindowsAvailable; для UITK buttons/fields
  использован официальный Unity MCP (настоящие pointer/value events). AX menu/save
  actions работали. Это не физический mouse/touch QA. Отчёты: Reports/P10/.
- Scene и Data сохранены. Не запускались gameplay PlayMode, mobile/player build
  и человеческий плейтест. Следующий этап P11 начат по разрешению автора.

## P11 — завершено 5 октября 2026

- Один persistent row, полный target, видимые этажи, adjacent drag/tap-tap,
  точный режим, Drop/Deliver, Undo/Retry/Hint/Pause/target zoom.
- Повторный запуск выявил зависимость виртуального ввода от фокуса Editor.
  Test fixture изолирует фокус InputSystem и возвращает настройки в teardown;
  production pause/focus gate сохранён.
- Два воспроизведённых бага исправлены: второй палец на том же slot больше
  не перехватывает жест; системная отмена над соседом не делает ход. Также
  вторичная кнопка мыши не завершает удерживаемый основной жест.
- Hint не перекрывает precision controls; использованный мост виден закрытым;
  preview показывает signed эффекты; готовый ряд явно предлагает Deliver.
- Актуальные Unity EditMode: 305/305 PASS; PlayMode: 12/12 PASS.
  Все 40 уровней пройдены через настоящий InputSystem/EventSystem/кнопки:
  92 команды, все 15 domain event kinds, точный receiver, те же row views.
  Нет missing/orphan .meta; пять JSON побайтно совпадают с Data.
- Portrait capture через официальный Unity MCP проверен на 390×844 и 320×693:
  C01, составной C38, Mixed8 C32, трёхэтажный Words C37. Это серые технические
  свидетельства, не художественная приёмка. Console после прогонов: 0 ошибок.
- Mac разблокирован; Refresh завершил импорт. Поток Device Simulator NRE
  после reload не повторяется при закрытом Simulator. Повторное открытие
  самого встроенного Simulator не проверялось; второй MCP/SDK не добавлялся.
- По просьбе автора выполнен POC audit: 29/40 задач на 1–2 действия, среднее 2,3;
  28 полностью вынужденных optimal DAG. Контрфактические проверки устройств
  сохранены. Выводы и конкретные требования P14/P17/P20: Docs/POC_REVIEW.md.
- Reports/P11 содержит итоговые XML/JSON/captures; Reports/P11_InputRepro —
  сохранённое воспроизведение двух исправленных input bugs.
- Не проверены: физические телефоны, player build, mobile performance,
  окончательные glyph/visual/motion, человеческое удовольствие. P12–P20
  сохраняют полный scope и ещё не объявлены выполненными.
- Визуальные уточнения автора сохранены: уникальные CASCADE assets, яркая
  мультяшная стилизация, близкие ShapeMania рисовка/палитра, компактные
  платформы, явный приёмник, фон/ambient. Следующий этап по плану — P12.

## P19 — локальные границы завершены, 5 октября 2026

- Один AdService с disabled-by-default core/privacy/provider gates, запретами
  на puzzle/tutorial/fail/retry/language, first-session/12-clear защитой,
  3 новых победы +180 foreground seconds, максимум2/session, no chapter finale.
  NotReady пропускает этот win; rewarded сбрасывает оба интервала.
- Mock callback требует opened + verified receipt; close не выдаёт награду;
  повтор/late confirmation идемпотентны. NoAutomaticAds и HideOffers раздельны.
  Mock non-consumable restore не выдаёт coins/tokens. Нет store catalog/SDK/UI.
- Инвентарь/receipt и HintUsed сохраняются до публикации; failed write/capture
  откатывает профиль, флаг и hint cache. Unknown/stale/revealed не расходуют;
  reload того же attempt/state не списывает второй жетон. Free Hint сохраняется.
- Новое optional profile поле совместимо со schema1; цены/правила/IDs/packages
  не менялись. Контракт до изменений: Docs/P19_MONETIZATION.md.
- Unity текущая compilation, EditMode **344/344 PASS**,10:01:10–10:01:13 UTC;
  Console0 errors/warnings. Reports/P19/editmode.xml. PlayMode/build не повторялись:
  изменений UI/provider integration нет, disk/hint rollback проверены EditMode.
- Официальные Unity IAP/LevelPlay руководства просмотрены; real SDK, sandbox
  purchase/restore/receipts, consent age/device и настоящие показы NOT RUN.
  Core author acceptance отсутствует; ни одного live monetization entrypoint.

## P20 — содержание завершено, 5 октября 2026

- C41–C160:120 структурно разных задач на прежних правилах2.0;360 bindings
  EN/RU/UK из existing dictionary. Original5JSON/C01–C40/hash сохраняются.
  Seeded884кандидата,5–12actions,mean7.8667,10stampdetours; duplicates/
  reflection/Unknown/trivial/fullyforced отфильтрованы. Design note доIDs:
  Docs/P20_CAMPAIGN.md. Это solver-backed authoring, не humanfun доказательство.
- Каждый новый completed graph совпал Python/C#, все locale routes проиграны,
  stamped objectives отдельно, multiset сохраняется, terminalsave replay совпадает.
  EditMode466/466PASS,10:09:24–10:10:42UTC. Reports/P20.
- FocusedPlayMode1/1PASS,10:14:00–10:14:22UTC:6newpaths через actualInputSystem
  directdrag/buttons, predictedstate/rowviews совпадают. Reports/P20Focused.
  Первое имя теста ошибочно выбрало0cases; это не PASS, повтор с точным именем
  дал настоящий1/1. Baseline all40 test и stage witnesses явно ограничены40.
- В playable импортирован extension;8регионов,новый сюжет/карты/погода/wooden
  motion — следующий уже разрешённый автором шаг, не объявлены готовыми.
  Нет полного PlayMode/build rerun; physicaldevices/nativeeditorial/humanfun NOTRUN.
