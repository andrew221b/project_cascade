Карта, перетаскивание и подсказки — 7 октября 2026: выполнено. Пропорции карты сохранены, островки визуально разнесены по высоте, независимое мягкое парение/облачный дрейф/пролёт; Мика читаемого размера и указатель сбоку. Смена режимов без бокового arrival-tween и сброса параллакса. Drop hit-test по видимому телу фигуры с верхним допуском. Закрытые и освоенные подсказки сохраняются глобально (optional SeenCoachPrompts; дизайн-заметка до изменения). Checkpoint Undo непрозрачен; прежний guard геометрии сохранён. Native MCP 23 assertions PASS, визуально 390×844 и 320×693/safeInsets/RU/UK/LargeText, поздний мир; активная пауза не завершает этап, реальный checkpoint сохраняется, Undo/Map одной высоты. Телефон/build/full suite/FPS НЕ проверялись. Docs/MAP_AND_INPUT_OCT07.md; Reports/MapInput/.

Ясность навигации и рамки меню — 7 октября 2026: исправлена повторная «Назад» в справочнике; перелистывание рядом со счётчиком, одинаковые Основы/Назад. Framed Back48px вместо голой стрелки; один возврат в сумке инструментов/коллекции/Без рекламы/монетах. Рамка мастерской расширена в portrait и landscape, действия внутри окна с небом. Шесть позиций 3×2: пять расходников + бесплатный постоянный справочник (существующий рисунок книги); цены, баланс, сохранения/IDs и карта не менялись. Убраны служебные номера B04 из CTA; заблокированные примеры показывают уровень открытия. Исправлен перенос подписи рекорда и отступ перед CTA основных правил. Native Unity MCP: initial56 состояний/все30 GameScreen,56 stable hash,0 overflow/split-word findings; final13 кадров,0 overflow. Ручной осмотр кадров и реальный flow справочник→эффекты→следующий/предыдущий→назад PASS; кошелёк/запас/hash неизменны. Ошибку QA селектора имён исправили; два итоговых кадра магазина пересняты с явным выбором Tools (фильтрация наследовала Trophies). Сохранены13 итоговых PNG, промежуточные только metadata. RU390,UK320 LargeText,EN320 store,wide1440; физические устройства, билды/установка, performance, full suite/livecommerce NOT RUN. Docs/MENU_CLARITY_OCT07.md; Reports/MenuClarity-Final/validation-summary.json.

Визуальный аудит всех экранов и музыкальная динамика — 7 октября 2026: выполнено в текущем Unity Editor. Осмотрены 50 видов экранов/состояний для всех 30 маршрутов (PlayChoice — прежний alias карты, Dialogue — overlay): меню, 3 страницы настроек, карта, магазин/покупки/сумка/трофеи, игровое поле Shapes/Words/Mixed, обучение, диалоги, комикс, награды/статистика, проигрыш/переход, журнал/мастерская/задания/рекорды и служебные состояния. Исправлены реальные наложения: компактная сумка сохраняет Мастерскую; подписи/переключатели настроек имеют внутренние отступы; result Undo48px вместо160 и тематические Retry/Map; счётчик комикса вынесен с тёмного рисунка; заголовки/длинные пояснения не упираются в рамку; объяснение устройства отделено от подписи демонстрации; цифры дерева помещаются в гравировку. Палитра/иллюстрации/правила/экономика/пакеты сохранены. Независимые финальные обзоры21/21 +3/3 выбранных кадров PASS. 47 detailed state audits неизменны; 3 каталога досняты напрямую с встроенной проверкой puzzle hash после ошибки измерителя TMP.wordInfo (не ошибка магазина). Измеритель исправлен и компилируется, повторный bulk scan не проводился. Музыка и FX уже имели независимые настройки; четыре сочетания10/00/01/11 и lifecycle проверены. Девять тем имеют A/B форму; скачок темы меню+11.34→+4.82dB, исходный WAV/meta и 8 миров сохранены, новый balanced sibling выбран. Unity audio PlayMode2/2PASS; initial1/2 сохранён, исправлено ожидание завершения crossfade в тесте. Финальная compilation без ошибок, Editor stopped, QA detached; diff check PASS. Reports/VisualAudit-After/validation-summary.json; Docs/VISUAL_AUDIT_OCT07.md; Docs/AUDIO_FORM_AND_CONTROLS.md. Phone build/install, physical-device/OS accessibility, live commerce, full suite, performance, audio listening и human enjoyment NOT RUN. User Cloud edit сохранён; sidecar/config Impeccable не переписывались.

Конфликты механик, режиссёр и звук ветра — 7 октября 2026: исправлено локально. Полная проверка нового single-round recipe в worker до ввода; unsafe/Unknown → проверенный Recovery или published fallback, старые hashes/версии не менялись. Domain wind включён в solver перед final-row contacts; атмосферные порывы не меняют ряд. Исправлены победная публикация Фонаря, terminal checkpoint discovery, сохранение/restart pending loading, сохранение выбранного меню, reward/purchase capture временного attempt, Retry после неудачного плана и Continue с карты. Найденный native shutdown NullReference в закрытии обучения исправлен отдельным regression. Wind audio следует давлению активной доски, слабый бриз тихий, порыв усиливается/затихает плавно, playback не перезапускается. Unity focused EditMode71/71 + transactions16/16 PASS; 14 уникальных PlayMode сценариев прошли по финальным выбранным отчётам (initial failures/исправления устаревших тестов явно сохранены). Pure C# sweep1289 rawrecipes/672 proven staged plans; это не полный Unity прогон. Два MCP camera кадра390×844 осмотрены; gain0.01936→0.05085, puzzle hash unchanged. Final source review без новых P1/P2; compiled, Editor stopped, Console0 current errors, QA detached. Билд/телефон/full suite/livecommerce/performance/human enjoyment NOT RUN. User Cloud edit сохранён; Docs/WIND_AND_INTERACTION_CONFLICTS.md; Reports/WindLogic/README.md.

AdMob и экономика покупок — 7 октября 2026: LevelPlay9.5.1 удалён из активного UPM/кода/defines/vendor descriptors; Google Mobile Ads Unity11.5.0+UMP и один EDM1.2.188 установлены. Официальные test IDs iOS/Android, consent перед SDK, privacy options, callbacks на Unity thread, кэш/retry/expiry, suspend/resume и прежняя частота рекламы. Test shop отдельно: fake value только в сессии. Монеты240/480, реальные цены из stores; кошелёк≥960 запрещает новые пакеты, pending paid value не теряется. Два инструмента суммарно/этап, один раз каждого; Wayfinder один ход, истекает также при Undo; помощь≤2звёзд/без clean record. Старые флаги loadable, no schema change. Дубликат SKU/чека и failed-save rollback проверены. IAP pending подтверждается после проверки/сохранения, confirmed consumables не восстанавливают монеты. Production build guard. Backend/ReceiptVerifier подготовлен (Apple/Google official APIs), не развёрнут. EditMode35/35 PASS; component PlayMode2/2 PASS (финальный повтор после fullscreen guard,02:25:15UTC; Reports/AdMobStore); Node5/5 PASS. Шесть native Editor camera captures EN/RU/UK/320/390/wide, safe insets; Impeccable ship scoped UI review+documenter, кнопки≥48px; no physical OS/ads/IAP/performance claim. Сборка/установка телефона не повторялась; прошлый export сохранён. Live native ads, dependency platform build, sandbox IAP, hosted verifier, actual publisher IDs/товары/цены — NOT RUN/внешняя настройка. Docs/ADMOB_AND_STORE_RELEASE.md, Docs/ADMOB_STORE_UI.md, Reports/AdMobStore. Пользовательский Cloud edit сохранён.

iPhone build/install — 7 октября 2026: выполнено по запросу автора. Текущий исходник 8adca9c; один Unity iOS IL2CPP export в Builds/CASCADE-iOS-2026-10-07, Succeeded/0 errors/346 warnings. Предыдущий Builds/CASCADE-iOS сохранён. Xcode создал профиль CASCADE с IAP и iPhone; Release workspace BUILD SUCCEEDED. CoreDevice подтвердил установку, запуск и работающий процесс PID51097 на iPhone17. Реальные платежи/реклама, визуальная проверка телефона и performance NOT RUN. Reports/iOS-2026-10-07/README.md. Пользовательский Cloud edit сохранён.

Иконки и логика меню — 7 октября 2026: выполнено. Исправлен NPOT-импорт MenuIcons/OpeningComic/TravelTreasures; исходные PNG/GUID сохранены. Optical slicing + квадратные44px области и вертикальный центр для иконок; масштабирование при смене размера, map bag48×48. Главная остаётся с4действиями, режимы только на карте; сумка доступна с карты/магазина, My things отлична от Workshop. Возвраты Tools/Shop/Bag/Collection/Journal/Requests/Records/Settings упорядочены, родительский магазин и выбор сохранены, исправлена найденная петля. Disabled shop actions сохраняют enamel. 2строки EN/RU/UK добавлены. Focused PlayMode1/1PASS4.7577023s, native button traversal/state hash PASS, 8финальных Editor PNG320/390 RU/UK осмотрены, финальная геометрия44/48px PASS. Compile/Console0errors0warnings, temp profile cleared, Editor stopped. Full suite/phone/build/performance/OSa11y NOT RUN. Docs/NAVIGATION_ICONS_AND_RETURNS.md; Reports/MenuNavigation/README.md.

Внешние кнопки и исчезающее задание — 7 октября 2026: выполнено. Помощь/пауза у безопасных внешних краёв, снаружи верхних канатов; 48px targets сохранены, размеры платформ/фигур не уменьшены. Верхние точки подвеса сдвинуты внутрь для отступа кнопок. Центральное задание fade-in0.28s, hold2.2s, fade-out0.45s; новый уровень/этап обновляет показ, пауза/фон останавливают возраст, скрытая плашка не перехватывает ввод. Native MCP390×844/320×693 визуально осмотрен; runtime проверки fade/pause/state/rope clearance PASS. Compile/Console0errors0warnings, Editor stopped, temp profile cleared. Full suite/build/physical phones NOT RUN. Reports/OuterHud/README.md.

Тихая подсветка соседа — 7 октября 2026: выполнено. Убраны предварительный подъём соседней детали, яркий/красный контур места обмена и звук наведения. Только допустимый сосед получает тонкий контур alpha0.10/scale1.025; источник сохраняет отзывчивый подъём и следование пальцу. Native Unity MCP390×844: фигуры и буквы осмотрены; runtime проверки отсутствия подъёма, слабого контура, скрытого blocked-marker и неизменности состояния PASS; реальный pointer drag коммитит ровно один обмен PASS. Compilation/Console0errors0warnings. Full suite/phone/build NOT RUN; правила и Cloud settings сохранены. Reports/QuietNeighbour/README.md.

Малые крепления и морозная колба — 7 октября2026: сгенерированы два прозрачных спрайта Imagegen (NailHead/FrostFlask), импортированы64/512px. Большие прямоугольные крепления заменены гвоздями ~5px, центральный подвес отведён от счётчика дерева. Длина не зависит от этажа: бомба16px, остальные22px. Подвесной эффект часов теперь колба с настоящей вставкой для фигуры/буквы, новое имя/обучение/справочник/практика/центральная анимация «Бомба охлаждена!», оригинал и чистые пузырьки. Магазинные часы и старые IDs сохранены; бомба ждёт прежние2 обмена. Focused PlayMode2/2PASS14.2434833s; native MCP4кадра390×844, фигура/буква/короткая верёвка/гвозди/анимация/статус; glyph — изолированный review binding КРОТ, product content unchanged. Final compilePASS/console0/Play stopped;840валидных уникальных строк локализации. Full suite/FPS/physical devices/human enjoyment NOT RUN; no phone builds/installs. Docs/NAILS_AND_FROST_FLASK.md; Reports/NailsAndFlask/README.md и prompts.md. Cloud edit preserved.

Счётчик внутри дерева и спокойное предупреждение ветра — 7 октября 2026: убран автоматический мини-ряд/значок под платформой; в предупреждении и результате ветра нет стороны и значков. Число прочности выгравировано по центру реальной прогибающейся доски, размер ограничен её толщиной; отклик меняет цвет, не выводит плашку за дерево. Починена светлая окраска подсказки после смены языка/доступности. Прочность читается из DurabilityResolved/FloorEntered после принятого обмена с учётом ремонта/бомбы; выбор и отклонённый ход не расходуют её. Focused PlayMode1/1PASS14.7280984s, native MCP390×844/320×693 с safe insets, runtime прочность1→слом/посадка→2, UI2; no duplicate forecast. Final compilePASS/console0/Play stopped; TSV835 unique rows. Full suite/FPS/physical devices/human enjoyment NOT RUN; phone builds/installs не выполнялись. Domain/Solver/правила/пакеты/цены/schema/Cloud edit сохранены. Docs/PLATFORM_INLAY.md; Reports/PlatformInlay/README.md.

# CASCADE — TASKS

Диалоги по пути — 7 октября 2026: выполнено. Шесть новых героев с портретами (Брам, Сора, Нери, Тома, Люми, бабушка), Пип/Ру сохранены.40 отдельных разговоров после островков,16 вариантов между вторым и третьим этапами каждого третьего уровня начинаяC02,8 знакомств по регионам; EN/RU/UK. JRPG-рамка снизу с тканевой лентой имени, портретом, мягким входом/появлением текста, первым тапом раскрыть/следующим продолжить, немедленным пропуском/ReducedMotion. После островка: статистика→трофей→разговор→карта. На карте «Поговорить» называет местного героя, знакомит/объясняет текущую цель и начинает открытый уровень либо продолжает точный этап; повторное нажатие не повторяет знакомство. Игра/таймер приостановлены; tween не меняет состояние/receipt; история чтения независима по режимам, автоматические прочитанные сцены не повторяются. Новая попытка Words/Mixed сохраняет старт3этапов. EditMode6/6PASS0.5051124s, native MCP: настоящий checkpointC02, порядок stats/trophy/island, диск,408 переводных реплик на320/LargeText/safeInsets, отдельные портреты/390/wide1440, свежие Words/Mixed и точный возврат Shapes. Compilation/финальная Console0errors/warnings. Phone build/install/full suite/физические девайсы/FPS/humanfun NOT RUN. См. Docs/JOURNEY_CONVERSATIONS.md, Docs/AssetPrompts/JourneyResidents.md, Reports/JourneyConversations/README.md.

Параллельные режимы и праздничные частицы — 7 октября 2026: выполнено. Единственный выбор режима — три вкладки карты; старт ведёт через вступление на карту, отдельный экран выбора убран. Фигуры155 / Слова154 / Микс160 готовых узлов. У каждого свои открытые уровни, звёзды/очки, журнал рекордов, director, обучение/диалоги, попытка и точный этап; schema2 читает schema1 без копирования прогресса в другие вкладки. Магазин/инвентарь/покупки и earned-ресурсы общие, награда за узел однократна. Вкладки52px на320/390; номерные кнопки бирюзовые со светлыми цифрами. Новый шестичастный alpha-атлас конфетти:24 отдельных косметических тел с гравитацией/сопротивлением/ветром/вращением/затуханием, quiet0; целый лист не рисуется. EditMode30/30PASS2.2541812s; native MCP: независимость/возврат во второй этап/диск,6 силуэтов/24 двигающихся объекта/finite fade/ReducedMotion, настоящая цепочка статистика→трофей→карта, стабильные state/rewards, финальные RU LargeText/safeInsets320. Compilation/Console0errors/warnings. Билд/телефон/full suite/физические девайсы/FPS/человеческая интересность NOT RUN. См. Docs/PARALLEL_MODE_JOURNEYS.md, Docs/AssetPrompts/CelebrationPaper.md, Reports/ModeJourneys/README.md.

Пять звёзд и читаемость — 7 октября 2026: новая оценка1–5 за весь уровень по доказанному минимуму, реально выполненным ходам (Undo больше не стирает ход), отменам и активному времени.5: минимум без отмен/помощи/взрыва и быстро;4: идеальный медленный маршрут или минимум+2/≤1Undo. Помощь сохраняет максимум2. Таймер в статистике; пауза, меню, обучение, цель и тестовая реклама исключены. Старые сохранения/результаты сохранены; неизвестное время отображается тире. Кремовые карточки результата/общих меню убраны; заголовки центрированы внутри рамы, единое оформление кнопок/каруселей/диалогов. Новый RGBA-атлас золота/огоньков и два спокойных фона мастерской, оригиналы сохранены. EditMode45/45PASS4.1417433s; native MCP13PNG portrait/smallLargeText/simulatedsafeinsets/wide, runtime clock/5stars/finalpose/immutable receipts48px assertionsPASS, final ReducedMotion/init-order checksPASS. Console0errors/compilationPASS; Play Mode stopped, isolated profile detached. Full suite/formalPlayMode/device/build/install/performance/human enjoyment NOT RUN. Docs/FIVE_STAR_RESULTS_AND_ACTIVE_TIME.md; Reports/FiveStarResults/README.md. User Cloud edit preserved.

Карта целого мира и праздничные награды — 7 октября 2026: завершено локально. Кремовые heading/footer/marker карты убраны; dark ink на цельном небе, читаемые светлые стрелки на teal, amber основной action. 160 существующих уникальных уровней распределены по восьми целым картам:15/15/10/24/24/24/24/24, остров содержит2–5 полных трёхэтапных уровней. 33 исходных story beats, content hashes, IDs, clears и награды сохранены; новых уровней не генерировали. Мика и прогресс отмечают текущий остров; мир показывает оставшееся число уровней, стрелки позволяют выбирать лишь открытые повторы. Лента раскрывается, звёзды загораются последовательно, затем считает очки; finite warm glow/confetti. Свёрток готовится/раскрывается, предмет поднимается и мягко оседает; stage ribbon тоже раскрывается. Локальный DOTween clock учитывает focus до продвижения и ограничивает long-frame jump; Reduce Motion оставляет итог, qualityOff убирает декоративное свечение. Целевые EditMode16/16 PASS, native isolated three-stage C03 witness/result→claimed parcel→map, timeline/focus/final pose/immutable receipts/row/replay/quiet map PASS. Восемь финальных native видов открыты, small320×693/LargeText/safe insets, все проверенные map targets≥48px; финальные arrow-only recaptures двух map views. Начальные probes ошиблись в deferred destroyed component и normalized button names, helper исправлен; ошибки не скрыты в отчёте. Final runtime Console0. Full suite, player builds, physical input/audio listening/performance/human enjoyment NOT RUN; Play остановлен, QA профиль отвязан, unrelated serializer churn снят, Cloud edit сохранён. Docs/MAP_PACING_AND_CELEBRATION.md; Reports/MapCelebration/README.md.

Магазин, трофеи и инструменты в игре — 6 октября 2026: завершено локально. Единый читаемый каталог на двух полках, пять инструментов и восемь трофеев; Трофеи / Trophies / Трофеї во всех трёх языках. Новые Фонарь пути выполняет один доказанный ход, Линза спуска показывает точный следующий ряд без изменения состояния; четыре новых косметических трофея можно носить на карте. Условия помощи (до двух звёзд, без чистого рекорда), запас и доступность видны до использования. Кнопка сумки появляется между Undo/Descend только при наличии запаса. Цены нового earned-coin каталога 160/100 и 240 за трофей описаны в design note; прежние цены, cap5, четыре parcel recipient/cap12, schema1 optional defaults и Domain/Solver сохранены. Новые предметы имеют прозрачный авторский атлас и обязательное первое объяснение. Целевые EditMode46/46 PASS, runtime callbacks/rollback/reload/точный preview PASS, семь финальных native MCP кадров (320/390, Large Text, simulated safe insets), основные маленькие controls48px, runtime Console0. Fresh finish reviewer: шесть scored fixes resolved, scoped SHIP. Play остановлен, QA-профиль отвязан, автосериализация посторонних assets убрана, пользовательский Cloud edit сохранён. Полный suite, player build, physical devices, live commerce, performance и human enjoyment NOT RUN. Docs/SHOP_TROPHIES_AND_FIELD_TOOLS.md; Reports/ShopRefresh/README.md.

DOTween и тестовая монетизация — 6 октября 2026: завершено. Официальный DOTween 1.3.030, модуль UI/asmdef, отключён ненужный Physics2D. Плавные переходы, нажатия, обучение, звёзды/счёт/конфетти и монеты; прерывание, focus и Reduced Motion проверены. Добровольное видео +40 монет; автоматическая реклама после полного уровня/статистики/подарка: ≥3 новых прохождений, ≥180 секунд, ≤2 за сессию, без повторов/финалов/долга. Тестовый магазин включён, цены скрыты; пробные покупки временные, cap5/ограничения инструментов сохранены. Перезапуск подтвердил чистый тестовый инвентарь и сохранение заработанных 40 монет. Целевые EditMode: 30/30 первоначально, 21/21 финальные, 3/3 после исправления перевода (наборы пересекаются). Проверены SDK rewarded callback и local preview; interstitial context синтетический. Native визуально проверены семь видов, small/LargeText/safe insets, tap44px; настоящий C01 из трёх этапов → статистика → карта. Close/CTA исправлены; последний tutorial probe заменён на подходящий fixture/readiness wait, ошибочный assertion сохранён в отчёте. Production ID/catalog/consent/receipt server отсутствуют; физические устройства/реальные покупки/билды NOT RUN. Play остановлен, QA-профиль отвязан, Cloud edit сохранён. Docs/DOTWEEN_AND_TEST_MONETIZATION.md; Reports/MotionAds/README.md.

Первое знакомство с механиками — 6 октября 2026: завершено. 17 обязательных объяснений (12 эффектов досок, два вида ветра, три инструмента) с картинкой/анимированным примером и коротким EN/RU/UK текстом. Игра, канаты и ветер под окном стоят на паузе; сочетания объясняются по очереди. Первый порыв ждёт до импульса, первый ветер на спуске — до перехода этапа. Инструмент не тратится до подтверждения. Изученное сохраняется только после явного нажатия; прерванное окно не считается изученным. Optional Tutorials off не отключает первое знакомство. EditMode18/18PASS; финальная native MCP-проверка всех17 объяснений, пяти портретных окон, focus/инпут/сохранения/неповтора и расхода трёх инструментов PASS. Маленький экран320×693, Large Text и safe insets проверены в Editor; физический телефон и player build NOT RUN. Play Mode остановлен, тестовый профиль отвязан, Cloud edit сохранён. Docs/FIRST_MECHANIC_POPUPS.md; Reports/Introductions/README.md.

Заработанный магазин и Undo — 6 октября 2026: завершено локально; fresh finish verdict SHIP только по scored UI fixes. Scroll 120 ведёт доказанные solver-ом ходы на текущей доске до слома, spool 80 даёт один дополнительный Undo после двух бесплатных, return-hourglass 200 возвращает точный checkpoint перед последним сломом в текущем этапе. Обычный Undo доступен только на целой текущей доске; бесплатные Retry текущего этапа и one-step Hint сохранены. Stock cap 5, один инструмент каждого вида на stage attempt; Retry не возвращает потраченный запас. Использованный купленный инструмент исключает clean record/3 stars, покупка и косметика сами по себе не влияют на результат/режиссёра. Keepsakes 240 покупаются один раз, остаются заработанными через parcels и сопровождают Мику без stats. Coins/stock/attempt сохраняются одной транзакцией с rollback при ошибке; schema 1 additive optional fields, Domain/solver/IDs/packages не менялись. Note: Docs/SHOP_AND_UNDO.md.

Shop: общий shelf с одновременным выбором, оригинальный объект, постоянные count+price, amber только для исполнимого действия, sage для unavailable. Tool bag из Pause/loss, EN/RU/UK eligibility/once-per-stage, arrival/pulse/status исходного предмета и foreground guide hand. Новый прозрачный 2×2 TravelTools.png содержит scroll/spool/hourglass/coin pouch; exact prompt+embedded PNG metadata, provenance scan1 raster/0 missing. Source readability batch поднял benefit/count/price до18px, Large Text до20px; confirmation вынесен отдельно. Старые визуальные verdicts относятся к прежним scope и не засчитываются магазину.

Evidence: initial EditMode 95: 91 PASS/4 outdated-expectation FAIL сохранён; четыре ожидания исправлены, final focused 13/13 PASS (9 shop+4 affected), 17:52:02 UTC. Это не повторный 95-run/full suite. Native runtime UI callbacks PASS: purchases/equip/tools/exact rewind/disk restore/limits, изолированный synthetic 1200-coin wallet. Replay 365 prefixes+157 initial states; EN/RU/UK 1791 entries/597 keys с matching format arguments. 10 native Unity Editor PNGs 320/390/768 с simulated safe insets; min control 50.80px, TMP overflow 0 в проверенной выборке. Финальная recapture принята fresh reviewer: readability/count+price/separate confirmation, quiet sage unavailable/executable amber и короткая EN Follow the hand ribbon resolved; регрессий от scored fixes не замечено. Verdict только по этим UI fixes, не whole-game approval. Tests после visual/copy edits не повторялись. Reports/Shop/README.md. Новый player build/phone export/install, live App Store/Google Play purchase/restore/ads, physical devices/input, performance и human enjoyment/balance/retention NOT RUN. Existing No ads adapter выключен без выдуманных catalog/keys.

Ранние механики и читаемость платформ — 6 октября 2026: новые прохождения v4 вводят ремонт C02, бомбу C03, ключ/сучья C04, ленту C05, печать C06, песочные часы C07, одноразовый мост C08, связанные обмены C09 и хрупкую доску C10. Дальше эффекты комбинируются; режиссёр начинает адаптацию с C06. Solver размещает первые контакты на настоящем полезном пути. Исправлены перегруженные 7/8-символьные рецепты, все 169 изменённых/контрольных определений имеют канонические доказательства трёх этапов; v1–3 остаются поддержаны. Правила Domain/цены/пакеты/исходные IDs не менялись.

Убраны перекрывающиеся подписи сдвига. Нарисованы счётчик дерева, флюгер и обозначения движения; прогноз анимирует реальный ряд над пустой будущей платформой, связанные пары показаны вместе. Знаки фигур на бомбе цветные, подвесы ближе к своим контактам, счётчик бомбы в одну строку. При сломе есть изгиб/подготовка, новый звук треска, 22 щепки по косметической PhysX и позднее затухание 4.3–4.7s. Ветер предупреждает до потери опоры; фоновые порывы измерены и не изменяют канонический ряд. Native MCP: 9 сохранённых видов EN/RU, portrait/wide/320 large text + simulated safe insets, связанные обмены/3 платформы; пересечения прогноза с предметами отсутствуют в проверенной выборке. Целевые runtime 2/2 PASS16.918s + только изменённый порядок ветра 1/1 PASS14.759s; финальные compilation PASS/Console0 errors, Play Mode остановлен. Full suite/phone build/install/physical devices/performance/human enjoyment NOT RUN. Docs/EARLY_MECHANICS_AND_PLATFORMS.md; Docs/PLATFORM_PROPS_ART.md; Reports/EarlyMechanics/README.md.

Три этапа и прогрессия — 6 октября 2026: новые обычные C01–C160 и переигрывания состоят из 3 этапов с визуальным спуском. Статистика за весь уровень (звёзды/очки/ходы/ресурсы) → заработанный подарок с раскрытием → карта; без автоматического перехода к следующему уровню. Открыт только следующий предложенный уровень и уже пройденные; лучшие звёзды/очки обновляются без повторных наград. Карта считает полные уровни до следующей ступени, отдельные режимы сохраняют свой маршрут/этап; доступен существующий финал маяка после всей истории. Новая иллюстрация звезды/ленты/подарка/конфетти, finite motion и quiet. Solver/replay: 637 определений (625 из первоначального прохода + 12 исправленных C06/C08/C27), focused EditMode7/7PASS; четыре разных PlayMode-сценария прошли в целевом пакете и повторе Mixed1/1PASS20.6030164s. Native MCP: C01–C03, 9 приёмников, 3 clear receipts, спуск, статистика/подарок/карта; portrait/wide/RU large text, дополнительная компактная карта. Не запускались full suite/phone build/install/реальные устройства/performance benchmark/человеческий плейтест. Docs/LEVEL_COMPLETION_AND_MAP.md; Docs/COMPLETION_ART.md; Reports/CompletionProgress/README.md.

Древесные ограничители — 6 октября 2026: золотые плавающие кубики заменены двумя нарисованными сучками из BranchStops.png; основание утоплено в доску, движение следует её изгибу и покачиванию. Освещаемые спрайты с прозрачным фоном и сглаживанием; прежние правила закрытых проходов и видимости сохранены. Existing bridge/Undo PlayMode 1/1 PASS, native MCP C08 проверен визуально, движения записаны, puzzle hash не изменился; Console/compilation без ошибок. Phone build/install NOT RUN. См. Docs/BRANCH_STOP_ART.md и Reports/BranchStops.

Компактный игровой HUD — 6 октября 2026: выполнен. Жёлтый круг и Shortest route убраны из интерфейса; расчёт минимальных ходов сохранён. Палец обучения поверх фигур, sorting30 + ZTest Always. Подсказки через3,2сек, закрываются крестиком и не перехватывают ввод в скрытом состоянии. Сверху между канатами: иконка помощи, уменьшенное центрированное задание, пауза; снизу симметрично Undo/Descend. Утонена рамка, исправлен перенос русского номера уровня. Три целевых PlayMode PASS; финальная типографика проверена native MCP на390×844,320×693 с RU/LargeText и1440×900. Compilation/Console ошибок нет; phone build/install NOT RUN по указанию автора. См. Reports/CompactHud/README.md и Docs/COMPACT_GAMEPLAY_HUD.md.

Atmosphere revision (2026-10-06): COMPLETE locally — curved glider depth flight; four independent floating islands, final phone/wide composition review; nine individual A/B chamber scores, twelve acoustic cues and a natural wind loop. Native import readback + six distinct focused PlayMode tests PASS; final audio metrics show no clipping. Phone builds/installation explicitly excluded; physical-device listening/performance and subjective quality NOT RUN. See Reports/Atmosphere/README.md and Docs/AMBIENCE_AND_SCORE.md.

Отклик и магазин — 6 октября 2026: квадрат выделения/силуэты слотов удалены; подъём и контур самого элемента, свободное следование пальцу, возврат соседней детали и сохранение исходной позы при обмене. Падение/встряхивание: до8 pooled Rigidbody в отдельной cosmetic PhysX-сцене, столкновения/повороты/отскоки, затем точные места; платы/тросы получают импульс. Нулевая инициализация датчика и раннее приземление при взлёте исправлены. Domain/Solver/правила/порядок/цены не менялись. Unity IAP5.4.4 + LevelPlay9.5.1 через UPM, required resolver; draft NoAutoAds, проверка чека→сохранение→confirmation, idempotent restore, выключенная конфигурация без выдуманных ключей. EN/RU/UK экраны магазина: покупка необязательна, подарки заработанные. Пользователь подтвердил App Store/Google Play, общую аудиторию, отсутствие товаров/LevelPlay app/ключей. EditMode13/13PASS; initialPlayMode9/10, sensor retry1failure, final affected4/4PASS:10distinct cases имеют PASS evidence. 10 nativeEditor PNGs открыты, safeinsets320/390. Свежий Impeccable reviewer F01:footer45.27px/F02:quiet vertical drag loss; исправлены, footer64.14/52.66px, expanded axes1/1PASS в GameView после закрытия сбойного DeviceSimulator; verdict SHIP только поF01/F02. Реальные SDK-покупки/показы, receipt backend, privacy/ATT, нативные зависимости в билде, телефонный сенсор/тактильность/FPS и интересность NOT RUN. Билд/установка не выполнялись. Docs/TOUCH_AND_STORES.md; Reports/TouchRevision/README.md.

Новая навигация и вступление — 6 октября 2026: главная содержит только Начать игру / Магазин / Настройки / Отключить рекламу; далее отдельный выбор Сюжет / Фигуры / Слова / Всё вместе. Ручные режимы открывают полку с номерами и реальным свайпом; сюжет — три сцены комикса с бабушкой, бурей, последним огоньком и целью Мики, затем карту и диалоги. Большая заливка меню удалена, живой фон мастерской сохранён; новые тематические иконки/комикс/цельный фон неба, exact prompts embedded, scan3/3. Мика уменьшена и стоит на острове, её отдельный target в320px52.5×65.7; кнопки разговора/уровня53.7/56.9px высотой. У карты bounded gravity/mouse parallax, облака на другом слое, quiet0; нет игровых тросов на фоне. Shop даёт заработанные подарки/мастерскую/вещи, реклама уже выключена; SDK/цены не добавлены. EditMode2/2PASS; initial PlayMode5/6 выявил порог свайпа от ширины окна, fixed focused1/1PASS: шесть разных случаев имеют PASS evidence. Expanded map case1/1PASS1.8660373s проверил настоящим вводом поле вокруг Мики и неизменность puzzle state. MCP осмотр16 retained native PNGs EN/RU/UK,320/390/wide, simulatedsafeinsets; F1полосы неба иF2target исправлены, независимый verdict SHIP только по этим двум findings. UnityiOSexport Succeeded30.505509s,0errors/346warnings; MCP envelope без success, on-disk BuildReport и Editorconsole0errors подтверждены. Xcode Release build SUCCEEDED; CoreDevice install SUCCESS, запуск отклонён iOS из-за заблокированного телефона. По новой просьбе пользователя дальнейшие phone actions остановлены; будущие сборки/установки только по отдельной явной просьбе. Full suite/physicalgyro/FPS/Android/OSaccessibility/humanenjoyment NOT RUN; сохранения/Domain/пакеты/Cloud edit сохранены. Docs/NAVIGATION_REWORK.md; Reports/Navigation/README.md.

iPhone устройство — 6 октября 2026: Unity6000.3.8f1 iOS IL2CPP export Succeeded68.336159s, 0errors/346warnings. Xcode26.6 Release собран и подписан командой F8297DBPDU; все конфигурации app target используют эту команду. iPhone17: CoreDevice install PASS, launch PASS, процесс CASCADE остался активен (PID46106). Экран, тактильность, FPS, локализация и плейтест на физическом устройстве не проверены. Первый неудачный signed attempt был с неверно выбранной командой и исправлен. См. Reports/iOSDevice/README.md и Docs/IOS_DEVICE_HANDOFF.md. User Cloud setting preserved.

Один статус эффекта — 6 октября 2026: удалён старый локальный слой Device state/Contact outcome ribbon у всех подвесных инструментов, в том числе преждевременные результаты в preview. Результаты показываются только центральной анимацией оригинала/частиц/ленты; у вооружённой бомбы остаётся лишь счётчик ходов/заморозки. Native MCP: repair/ribbon/bomb previews и центральные RU-статусы390×844, runtime отсутствие старых слоёв; PlayMode3/3PASS17.1861486s: pickup/Undo, original reaction quiet/pause/restore, wind/painted symbol. Правила/данные/Cloud settings сохранены; full suite/build/телефоны не запускались. Reports/StatusCleanup/README.md.

Отклик на касание — 6 октября 2026: мгновенное небольшое подтверждение нажатия + мягкий подъём/увеличение, короткое следование пальцу и ограниченный наклон; quiet сохраняет только намеренное перетягивание. Выбор больше не перезаписывает удерживаемую/возвращающуюся позу. Отмена возвращает деталь за .18s, обмен наследует реальную позицию/размер/угол отпускания без промежуточного скачка. Preview кешируется по соседу; неподвижные одинаковые символы сохраняют свои места при дальнем обмене. PlayMode8/8PASS13.129887s + final continuity2/2PASS4.5364689s (9 разных сценариев); native MCP: фигуры390×844, RU-буквы390×844, quiet320×693. Отмена0ходов, обмен1ход; glyph/body assertions и состояние проходят. Full suite/build/реальные телефоны/latency/haptics/performance/человеческий плейтест NOT RUN. Docs/TOUCH_FEEL.md; Reports/TouchFeel/README.md. Domain/правила/данные/Cloud settings сохранены.

Физический отклик и свет — 6 октября 2026: щепки переведены на pooled Rigidbody/PhysX в отдельной сцене, с массой, вращением, ветром и столкновениями с косметическими платформами. Пауза останавливает сцену, quiet замораживает/погашает остатки. Подъём/обмен/посадка фигур передают импульс дереву и соседним цепочкам каната; падение9.81m/s² с управляемой точной посадкой, индивидуальное покачивание/вес. Настоящий URP clear coat, деформируемая верхняя поверхность дерева, alpha-маски теней, живое освещение каната. Нативно исправлены ошибка shader каната, прямоугольные тени щепок и участие старых target-детей в новой раскладке. PlayMode7/7PASS20.707851s + target1/1PASS1.3164565s. Native MCP:2 итоговые записи,3 сравнения света/теней (22437 изменённых пикселей), mobile pipeline кадр390×844; Lifecycle: при выходе найдены/исправлены раннее уничтожение debris и redraw разрушенного UI при Skip; итоговые3/3PASS4.607344s и native Play/Stop без ошибок. Shader0/Console0; Play Mode остановлен, PC pipeline восстановлен. Full suite/build/физические телефоны/performance/человеческий плейтест NOT RUN. Канаты — ограниченная симуляция точек без самоколлизий/узлов, фигуры — guided physics; фон/UI остаются иллюстрациями. Domain/данные/правила/assets/Cloud settings сохранены. Docs/PHYSICAL_PRESENTATION_AND_LIGHT.md; Reports/PhysicalPresentation/README.md.

Гибкие канаты — 6 октября 2026: вместо одной синусоидальной дуги — косметические цепочки закреплённых точек с инерцией, натяжением, тяжестью, мягкостью изгиба и плавным локальным ветром, разным по сторонам/этажам. Короткие подвесы используют ту же модель. Порыв сообщает импульс; точки крепления следуют уцелевшим платформам, удалённые этажи перестают быть якорями. Кубические кривые и плетение по мировой длине; визуально найденные начальные заломы/избыточная жёсткость исправлены. Pause/Reduced Motion/resize сохранены; gameplay state не меняется. Native MCP:2 итоговые записи двух/трёх этажей с противоположными порывами,2 кадра после разрушения/широкого quiet режима. Адресные PlayMode4/4PASS,19.5599533s; Console0errors/warnings, Play Mode остановлен. Full suite/build/реальные устройства/performance/человеческий плейтест NOT RUN. Docs/FLEXIBLE_ROPE_PRESENTATION.md; Reports/FlexibleRopes/README.md. Cloud settings сохранены.

Оригинал предмета и порыв подвесов — 6 октября 2026: по уточнению автора копия отменена. Тот же Image с символом переносится в центр без Instantiate; использованный подвес/канат/крепление исчезают по существующим contact flags. После эффекта оригинал скрыт в своей исходной иерархии; Undo возвращает тот же объект/положение/шрифт. Усилено спокойное покачивание дерева/канатов; канонический порыв даёт общий импульс с затуханием платформам, канатам и предметам, ограничения .22world/2.4deg сохраняют комфорт. Native MCP:6 кадров оригинала/пустого подвеса и2 записи движения, identity/Undo/hash assertions пройдены. PlayMode4/4PASS,24.7759089s; Console0errors/warnings. Полный suite/build/реальные телефоны/performance/человеческий плейтест NOT RUN. Изменений правил/IDs/данных/Cloud settings нет. Docs/ORIGINAL_TOOL_DEPARTURE_AND_WIND.md; Reports/OriginalDeparture/README.md.

Один канат и чистая последовательность реакции — 6 октября 2026: у каждого предмета один непрерывный канат с ветром и подвесом; копируется только корпус с нанесённым символом. Полёт .42s → пульсация/частицы с .46s → лента статуса с .88s → мягкое завершение1.72s. Reduced Motion статично .55s; pause/Off/Low работают. Чистые sibling атласы, остаточные ореолы отсекаются native UI материалом; двойной Outline удалён. Убрана ранняя вспышка локальной подписи, дым взрыва уменьшен и смягчён. Native MCP:17 двухэтапных свидетелей всех инструментов и кириллицы, одна ограниченная коррекция/подтверждение; оставлены5PNG, контактный лист и запись движения. Адресные PlayMode3/3PASS,19.4421247s; Console0errors/warnings, Play Mode остановлен. Gameplay state/правила/IDs/Cloud settings сохранены. Full suite/build/real devices/performance/human enjoyment NOT RUN. Docs/TOOL_REACTION_SEQUENCE.md; Reports/ReactionSequence/README.md.

Предметы, реакции, дерево и предупреждение ветра — 6 октября 2026: все отдельные бирки убраны; нужная буква/форма нанесена на корпус предмета, включая справку. Новые корпуса, тканевая лента, мотивы эффектов, четыре состояния дерева/щепки, эмаль/дерево для игровых кнопок. Предмет увеличивается из своего подвеса в центр, даёт тематический эффект и мягко исчезает; подпись ниже и по центру. Pause/Reduced Motion/Off/Low соблюдаются. Доска распадается в нескольких местах, щепки по всей ширине падают вниз. Перед каноническим сдвигом ветра — 1.05s направленная подсказка и плавное усиление декоративного покачивания; новых таймеров/правил нет. Вкладки настроек сокращены, отступы текста и значок паузы исправлены; запас ленты внутри рамки уровня. Восстановленная проверка3/3PASS, итоговые7/7PASS (21.0772735s). Native MCP:18 вариантов, EN/RU/UK,320/390,Large Text и asymmetric safe insets;3 записи последовательных кадров ремонта/щепок/ветра. Найденные потери клонированного силуэта и тёмная пауза исправлены, последние косметические отступы подтверждены отдельным кадром/компиляцией без повторного suite. Console0errors/warnings. Build, full suite, физические устройства, performance и человеческий плейтест NOT RUN. Docs/GAMEPLAY_REACTION_MOTION.md; Docs/TOOL_BODY_AND_TIMBER_ART.md; Reports/Reactions/README.md.


Меню — новая композиция и карусели, 6 октября 2026: прежние разрозненные карточки заменены одним спокойным листом мастерской. Выравнивание, контраст, крупные изображения, подписи/состояния и физические отступы футера согласованы. Языки — две карусели с native названиями; язык заданий меняется только после подтверждения, текущий attempt сохраняется. Бонусные уроки — крупные стрелки и счётчик 1–6. Стрелки дневника/печатей/просьб перенесены к «Назад», образец больше не перекрывают. Реакции предметов — цветная анимированная лента, локализованный текст и привязанные к предмету glints; новый sibling BombSafe с погасшим фитилём, исходная бомба сохранена. Native MCP: первый проход 29 вариантов, expanded confirmation 59 сохранённых вариантов EN/RU/UK, 320/390/wide и UK large text + simulated safe insets; исправлены 5 initial и 10 confirmation text failures, отдельно убрано перекрытие образца стрелками. Final focused overflow 0; сохранены только 14 показательных PNG/metrics. Четыре existing PlayMode cases **4/4 PASS**, 14.2090045s: языки, настройки, подвесы/обезвреживание, записи с печатью. Compilation успешна, Console0 errors/warnings, Play Mode остановлен. Начальная проблема Editor toolbar/domain reload записана, не скрыта. Full suite/build/real devices/performance/human enjoyment NOT RUN. Правила/цены/IDs/схемы/пакеты/Cloud settings не менялись. Docs/MENU_FOLIO_RESET.md; Reports/MenuReset/README.md.


Финальное ревью и упрощение бомбы, 6 октября 2026: у бомбы удалены отдельная табличка, деревянная перекладина и шнуры таблички. Нужная буква/силуэт теперь нанесены светлой краской прямо на корпус и движутся вместе с ним; подвес от платформы сохранён. В обучении используется тот же знак, демонстрация увеличена, лишнее пояснение про бирку убрано. Native MCP: просмотрены 26 кадров (22 варианта основных экранов меню, 4 игровых), RU320/EN390/wide1440/UK large text; оставлены 11 показательных кадров. Найдена тесная текстовая область буквы, исправлена и подтверждена только на 3 затронутых кадрах; final overflow findings 0 в выборке. Один расширенный existing PlayMode test: 1/1 PASS, 5.8786s — нанесённый знак/ветер/пауза/Reduced Motion, каноническое обезвреживание RU-бомбы и сохранённые подвесы ремонта. Console 0 errors/warnings, Play Mode остановлен. Ревью не заменяет плейтест человеком; полный suite, player build, реальные телефоны и performance NOT RUN. Правила/IDs/пакеты/цены/сохранения и исходные рисунки не менялись. Reports/FinalReview/README.md.

Читаемость и бирки, 6 октября 2026: бирка с нужной буквой/фигурой теперь висит на двух видимых шнурах от деревянной перекладины и качается отдельно от тяжёлого предмета, с общим ветром и остановкой на паузе/Reduced Motion. Меню получили нейтральные светлые поверхности для заголовков и связанных текстов, тёмную бирюзовую типографику, подписи полок и контрастные состояния выбора/недоступности. Исправлены высота счётчика бомбы, Demo result и длинные подписи кнопок; menu actions сохраняют минимум48physical pixels. Native MCP:22 сохранённых кадра, RU320/EN390/wide1440/UK large text, три live word-tag poses с неизменным hash; полный осмотр/поправка/подтверждение и финальная focused проверка четырёх меню. Initial1/1 Play PASS проверял только движение; native inspection выявил отсутствующий CanvasRenderer у новых шнуров, исправлено и усилено assertion фактической mesh. Final targeted1/1PASS,3.31s; final overflow findings0 в проверенной выборке, Console0errors/warnings, Play Mode остановлен. Последняя поправка меню-кнопок проверена компиляцией/native кадрами без повторного suite. Domain/rules/prices/IDs/packages и существующие рисунки/сохранения не менялись; новый player build, физические телефоны и performance NOT RUN. Reports/Readability/README.md, Docs/MENU_READABILITY_AND_TAGS.md.

Победа, проигрыш и живая мастерская, 6 октября 2026: отдельные конечные сценки — посадка фигур, свет фонаря и цветные искры с Микой при успехе; мягкое покачивание и задумчивый Мика при неудаче. Контролы открываются после сценки, checkpoint короче; pause/focus/skip/disable/Reduced Motion не меняют puzzle state. Полная жёлтая заливка меню заменена цельным видом из мастерской с тихими облаками/светом; сгенерированы только portrait-фон и прозрачный MikaThinking. Новый descent version2 даёт ремонт уже на первом этапе, бомбу и печать на втором; сохранённые version1 планы восстанавливаются с прежними hashes/условиями. Бирка с нужной фигурой/буквой прикреплена к деревянной перекладине предмета; добавлены How to play из паузы и объяснение бирки в демонстрациях. Вес и износ сильнее прогибают дерево, разлом разводит половины вокруг креплений, фигуры/подвесы следуют геометрии, летят щепки; счётчик скрыт во время разлома. 10 distinct EditMode имеют PASS evidence (initial9/10 выявил fuse7 вместо canonical1–6, corrected curve1/1); 132 steady curves solved/replayed,2→5–6→6–11, fallback0 в выборке. PlayMode7/7PASS после исправления startup MPB-constructor; затронутые subsets4/4 и3/3PASS после визуальных поправок. Через Unity MCP осмотрены 18 финальных кадров: timed win/fail, новое меню EN390/RU320/wide1440, guide, первые два этапа, ремонт/износ/разлом/подвесы. Исправлены скачок приёмника, слишком тонкая древесина, наложения текста и опора фигур на ломающиеся половины. Последние поправки видимости счётчика и полей компактной RU-справки подтверждены компиляцией и focused native кадрами; тестовый suite после них не повторялся. Final Console0errors/warnings; Play Mode остановлен. Reports/OutcomeMotion/README.md, Docs/OUTCOME_AND_WORKSHOP_MOTION.md. Новые player builds, реальные Android/iPhone, performance/audio/haptics и human enjoyment/retention NOT RUN.

Три этапа и спуск, 6 октября 2026: новые непройденные доставки C41–C160 состоят из короткой разминки, прибытия с ветром и финала с авторскими устройствами. Один target/multiset сохраняется; новые платформы появляются после явного Go lower. Первые два приёмника дают checkpoint, не clear/награду; Retry повторяет текущий этап, итоговый рекорд суммирует все ходы/hints/Undo. Сохранение хранит optional version1 plan и точные replay пройденных этапов; legacy single saves/уроки/контракты сохранены. Все120primary +12выбранных EN/RU/UK bindings имеют solved/replayed stage curves:2→5–6→6–11 commands, C81=2→5→10; fallback0 в этой выборке. Native EditMode12/12PASS, record extension targeted1/1PASS; четыре затронутых PlayMode имеют PASS evidence (initial1PASS/3FAIL выявил отмену drag новым gate, исправленный3/3PASS; после добавления full-run record/replay1/1PASS). Перед вводом проверяется весь plan и актуальный stage через тот же solver. 650ms косметический спуск несёт meshes, буквы и artwork подвесов; pause/focus/resize/disable/reduced settling не продвигают состояние. Через Unity MCP осмотрены EN390/RU320:stage1,checkpoint,wind,words finale,route note и два travel poses. Исправлены слабый checkpoint copy, caption под repair, счётчик во время travel и неправильная texture белых прямоугольников вместо букв. Final console0errors/warnings, temporary rigs0. Reports/Descent/README.md, Docs/THREE_STAGE_DESCENT.md. Player builds, физические устройства, performance и human enjoyment/retention NOT RUN.

Режиссёр и порывы, 5 октября 2026: после C40 новые доставки чередуют спокойные условия, тесный запас ходов, предсказуемый сдвиг ветром и восстановление после трудностей. Режиссёр учитывает завершённые попытки и осознанные повторы; время размышления и покупки не учитываются. Новые условия проверяются точным solver до ввода; недоказанный вариант заменяется исходным. Retry сохраняет условия, после повторных неудач More wood начинает отдельную попытку. Optional recipe закрепляет условия в сохранении; прежние schema1 saves совместимы. Общий нерегулярный ветер с ограниченными порывами согласует доски, канаты и длинные подвесы, ослабевает при удержании и останавливается на паузе/Reduced Motion. Ветер сдвигает ряд только при посадке через существующий EntryTransform, направление заранее видно и объяснено в наглядной подсказке. EditMode7/7PASS; четыре затронутых PlayMode имеют PASS evidence (первый run2PASS/2ошибки тестовых fixtures, исправленный targeted2/2PASS). C81–C88wind solved8/8 с replay; финальные EN390/RU320 gameplay/guide и More wood осмотрены через Unity MCP после исправления переносов RU и позиции стрелки; Console0errors/warnings. Reports/Director/README.md, Docs/DIRECTOR_AND_WIND.md. Player build, реальные телефоны, производительность и человеческий плейтест/удержание NOT RUN.

Подвесы и ветер, 5 октября 2026: устройства опущены примерно на 19 reference units и качаются от крепления на доске с инерцией; тяжёлые предметы реагируют спокойнее лёгких. Ремни следуют за предметом, у бомбы крепятся к металлической горловине. Один ветер согласует наклон и раскачивание платформ, прогиб канатов и подвесы. Фигуры, буквы, тени и узлы остаются на доске. Пауза/suspension останавливают движение; Reduced Motion задаёт неподвижную позу. Четыре затронутые PlayMode-проверки имеют PASS: исходный run 3 PASS/1 устаревший name-query FAIL, исправленный rope case 1/1 PASS, финальный device case 1/1 PASS. Через Unity MCP осмотрены последовательные C13 390×844 и C81 320×693; исправлен визуальный зазор у крепления бомбы. Console 0 errors/warnings. Reports/Wind/README.md. Это косметическая симуляция; gameplay/rules/assets/packages не менялись. Новые сборки, реальные телефоны и измерение производительности NOT RUN.

Receiver/route correction, 5 октября 2026: ряд центрирован внутри ivory-вкладыша по обеим осям; glyph-геометрия каждой буквы центрируется независимо от метрик шрифта. Исправлено повторное смещение live-букв при RefreshLayout: текст синхронизируется с дощечкой сразу, включая движение. Перед вводом точный solver проверяет окончательный level + goal в worker; solved-only cache, отдельный preparation gate, отмена устаревшей работы, Unknown не выдаётся за минимум. UI показывает кратчайший путь и отдельно прочность дерева. HP уменьшены в103/120 поздних задач; средний отброшенный запас на shortest route5.13→2.17, минимум5–12commands; все10stamp objectives сохранены. C01–C40/IDs/devices/rules/prices не менялись. Старые412pins восстанавливаются из компактного HP-manifest с проверкой hash; прежние попытки продолжаются на прежних условиях. Unity161distinct EditMode имеют passing evidence (160 в основном run, один compatibility test после устранения избыточного timeout), focused PlayMode1/1PASS, повтор после pose-fix1/1PASS. Визуально осмотрены финальные C81Shapes/C38Words390×844 через MCP, Console0errors/warnings; Reports/RouteBalance/README.md. Player build, физические телефоны и человеческий плейтест NOT RUN.

Receiver polish, 5 октября 2026: нижний ряд теперь всегда строится вокруг его настоящего горизонтального центра с равными внешними полями; ячейки получили MiddleCenter layout и небольшой равномерный зазор. Жёлтая заливка гнёзд заменена на глянцевую глубокую бирюзовую эмаль, а общий ivory-вкладыш стал нейтральнее: золотая рама остаётся тёплым акцентом, зато все фигуры читаются по отдельности. У форм немного усилены насыщенность, свет и лак. C81 (Shapes) и C38 (Words) проверены через Unity MCP на 390×844: Reports/Polish/receiver-shapes.png и receiver-letters.png; compilation PASS, Console 0 errors/warnings. Игровые правила, target, сохранения и IDs не менялись; player build/телефоны не запускались.

Morning-air refinement, 5 октября 2026: слабая полупрозрачная дымка добавлена только к дальнему фону, широким горизонтальным слоем с медленным дрейфом; цвет зависит от региона, у боков нет отдельного затухания и никакое размытие не применяется. Reduced Motion останавливает дрейф. Через Unity MCP осмотрены C13 и C81 на 390×844 (Reports/Polish/morning-garden.png, morning-lagoon.png); shader/C# compilation PASS, Console 0 errors/warnings. Сохранение и правила игры не изменены. Реальные телефоны и player build NOT RUN по просьбе автора об экономии проверок.

Surface correction, 5 октября 2026: вместо фактурной деревянной линии — непрерывная мягко затенённая верёвка с пружинным прогибом между креплениями; её край смягчён на телефонном разрешении. Дощечки букв получили широкую глазурную текстуру и более ясный блик, буквы оптически центрированы и имеют жёсткий смещённый подслой; их перемещение по-прежнему идёт тем же tween/pose, что у фигур. Подвесные устройства теперь слегка качаются на двух обновляемых ремнях. Цель и подпись выставлены симметрично. Убрано шейдерное затуманивание боков: фон закрывает экран единым изображением с центральным кропом. Через Unity MCP осмотрены C38, C81 и C13 на 390×844 (Reports/Polish/surface-letters.png, surface-devices.png, finish-lagoon.png); compilation PASS, Console 0 errors/warnings, один атомарный обмен сохранил multiset. Это косметическая пружинная симуляция, не Rigidbody/rope solver; настоящие устройства и человеческая оценка тактильности NOT RUN. Новый player build не делали.

UI/material follow-up, 5 октября 2026: деревянные боковые рейки связывают доски и приёмник; формы получили лаковый блик от Lit-материалов, буквы — лёгкое волокно и рельефную тень, доски принимают мягкую тень от направленного света. Увеличен планер, карта получила читаемую метку текущего острова и ровный верхний бар, меню — иллюстрированный вид из мастерской и тихую пыль, а пауза и настройки — Мику. Для Лагуны сгенерирован отдельный полупрозрачный небесный кораблик, медленно качающийся за платформами; текстура ограничена 512px при импорте. Плашка уровня отделяет текст от опор; цель поднята над приёмником на узком экране. 8 прицельных кадров через Unity MCP: Reports/Polish/finish-*.png; дополнительно повторно осмотрены три меню с иллюстрированной шапкой. Один focused swap: 1 атомарный ход, multiset сохранён, анимация закончилась Resting; Console 0 ошибок после перезапуска Play Mode и в Лагуне. Реальные iPhone/Android, измерение FPS и человеческая оценка удовольствия не выполнялись; нового player build не делали по просьбе автора об экономии проверок.

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
