## Text frame padding — Oct 27

- [x] Shared action/card layout reserves at least 2 logical pixels on all four sides of the inner writing field, fits painted content on both axes and preserves the 4-pixel icon/caption gap. Larger authored horizontal insets remain.
- [x] Both text factories, notification banners and remaining one-pixel action captions use the minimum inset; the short-phone subscription footer remains visible. Existing AGENTS.md records the rule.
- [x] Unity compilation: 0 C# errors. UiCenteringRegressionTests 7/7 PASS, including a demonstrated failing-before/passing-after fixed-size caption, EN/RU/UK, 320×568/390×844/430×932, long subscription prices and 81 menu combinations. Final native screenshots reviewed.
- [ ] New phone binary and physical-device installation not performed; changes are in source for the next build. No store pricing, app release or website changes.
- Evidence: PROJECT_CASCADE_Codex_Pack_v2/Exports/UI-Padding-2026-10-27/status.txt, validation.json, final.xml, changes.patch and 18 captures.

## Final store release-readiness audit — Oct 27

- [x] Fresh App Store and Google Play API audit completed; EN/RU/UK text matches the saved ASO/Human Writing versions and respects field limits.
- [x] Replaced all 18 medium-iPhone screenshots with lossless RGB copies; all 54 Apple screenshots report COMPLETE, with six files per locale/device set and original order preserved.
- [ ] Release BLOCKED: App Store has no uploaded/linked build; all six coin packs and three subscriptions lack review screenshots and remain MISSING_METADATA. Google Play has only the versionCode 1 QA draft and lacks the 1024×500 feature graphic in all locales.
- [ ] Legal/public privacy and terms are still drafts; documentsApproved and productionRelease remain false. No legal approval, website change, pricing change, review submission or release was performed.
- [ ] Fresh App Privacy/Data safety and Console warnings require Mac unlock. Final target-device, purchase/restore, account/cloud/deletion and ad/consent tests remain unverified; the connected Unity session is another project (DrillGame).
- Evidence: PROJECT_CASCADE_Codex_Pack_v2/Exports/Release-Readiness-2026-10-27/status.txt and JSON snapshots. No compilation or new binary build claimed.

## Store graphic assets — Oct 27

- [x] Replaced the existing CascadeStoreIcon.png with the designer’s 1024×1024 iOS store icon, preserving its GUID/meta and all existing iPhone/iPad references. Backup in PROJECT_CASCADE_Codex_Pack_v2/Exports/Store-Upload-2026-10-27.
- [x] Store screenshots and Google Play icon uploaded and verified through APIs; Apple creative assets uploaded separately.
- [ ] iOS icon is prepared in source; no binary build/upload or app release was performed in this asset task. Unity import/build validation remains to be run.

## Final website/support migration verification — Oct 22

- [x] SMTP SAVED and verified in fresh tabs for both DayDraft/Uskeep: support@vickand.com with distinct sender names, Resend host/465, stored keys hidden. Earlier stale-form handoff inference corrected; no further save is pending. Real delivery is not yet tested.
- [x] Google Play DayDraft/Uskeep privacy/deletion metadata is now published (Console last published Oct 22, no pending changes). Flying Ladders privacy URL remains saved in its draft; working AccountCloud deletion URL preserved.
- [x] Flying Ladders TestFlight EN/RU/UK links/feedback + Beta Review email, App Store Accessibility URL, Unity Player Accounts Privacy/Terms, and Android/iOS AdMob privacy URLs updated and verified. No consent message or game release published.
- [x] Both legacy Netlify redirects are live: Flying Ladders wildcard 301, Uskeep marketing-only 301 with legacy join/deletion/association endpoints retained (200). Uskeep changes pushed to its GitHub website source; auto-deploy successful.
- [x] Final HTTP smoke: three product roots 200, main Vickand root remains auth 302, root app-ads.txt 200. Detailed evidence: Reports/WebsiteMigrationVickandOct22/MIGRATION_VICKAND_2026-10-22.md.
- [ ] Remaining: real mail/auth/device checks, Flying Ladders legal approval/normal store release and AdMob crawl after listing, next normal DayDraft App Store update for locked live metadata, review/commit local application source edits for their next releases.

## Support mailbox and store migration — Oct 22

- [x] Published all three product sites with support@vickand.com; DayDraft live support function recipient updated with JWT preserved.
- [x] Google Play contact website/email saved and verified for all three apps; tracks unchanged. App Store URLs/contact email saved for Flying Ladders, Uskeep and draft DayDraft 1.0.1.
- [x] Uskeep live Supabase Site URL and exact deletion callbacks saved; old native callback preserved. Public root app-ads.txt configured while main Vickand site remains private.
- [x] Chrome follow-up: Google Play Uskeep/DayDraft privacy and deletion URLs saved and submitted (Changes in review), Flying Ladders privacy URL saved in draft and functional AccountCloud deletion URL preserved; DayDraft live Auth Site URL saved/reloaded with native callback retained.
- [ ] SMTP forms prepared in separate Chrome tabs; owner must enter vickand.com sending key and save both. AdMob/Unity console checks, legacy Netlify redirects and end-to-end mail/device verification remain.
- [x] After explicit owner approval, deployed the dedicated signed webhook ingress with server-side Vercel bypass; invalid signatures are rejected and main site remains private. Published AccountCloud version 11 from exact live source with five pages of contact/marketing link changes; build and live privacy redirect verified.
- Full status and evidence: Reports/WebsiteMigrationVickandOct22/MIGRATION_VICKAND_2026-10-22.md.

## Website Vickand subdomain migration — Oct 22

- [x] Published https://flyingladders.vickand.com on Vercel with Cloudflare DNS; HTTPS, legal/support routes, account-request redirect and app-ads.txt verified.
- [x] Updated source website metadata, provider disclosure, Configs exports, Unity legal settings/offline documents and local backend marketing redirects; preserved account APIs and draft/noindex approval flags.
- [x] Website checker passed 6 pages and 153 internal references; existing support mailbox preserved.
- [ ] Remaining Google Play privacy/deletion fields, AdMob/Unity-console URLs and old Netlify redirects need unlocked browser/account access. Store contact metadata and AccountCloud marketing redirects are already live. No game/store release or real deletion/email roundtrip claimed.
- Details: Website/MIGRATION_VICKAND_2026-10-22.md.

## Unity deletion credential connected — Oct 20

- [x] Saved owner-provided pair separately at MY DISK root; no credential values/Authorization header in source or reports.
- [x] Real Unity API validation: temporary development profile, read/write/readback/delete all four Cloud Save classes, read-only production access, admin player DELETE 200 followed by GET 404. Test data/profile removed.
- [x] Added two masked server secrets; preserved existing secrets; redeployed unchanged saved source version with runtime revision 4. Public health: deletionConfigured=true, publishersConfigured=false.
- [ ] Complete Cloud Code shared-secret connection, corrected backend publication/publisher configuration, full deletion flow and scheduled retries; AdMob IDs still pending. No replacement Unity pair needed.
- Evidence: `Reports/UnityServerKeyOct20/`. No Unity/runtime source change, store upload, screenshot or device install; no source-publication boundary bypassed.

## Unity server-key search and owner steps — Oct 20

- [x] Expanded MY DISK search: 15,522 small text/config files inspected; no complete exported Unity service-account key pair found. Archives, protected/system paths, installers/media and generated dependencies excluded; absence from the entire disk is not claimed.
- [x] Verified current official Unity instructions and project-scoped Cloud Save Editor / Authentication Editor roles; documented adding a new key to the existing service account, preserving old keys and saving locally.
- [ ] Owner provides path to an existing pair or saves a newly generated Key ID / Secret Key; no key created or cloud permissions changed in this task.
- Evidence: `Reports/OwnerSetupOct20/unity-key-search-expanded.json`, updated owner guide. Read-only search/documentation; no runtime tests or deployment.

## Google RTDN Pub/Sub configuration — Oct 20

- [x] Owner IAM changes applied: dedicated topic IAM get/set and attachSubscription access now pass.
- [x] Granted Google Play Publisher only on flying-ladders-rtdn; created authenticated flying-ladders-rtdn-push with exact identity/audience, retries, 7-day retention and no idle expiration. Read-back HTTP 200/ACTIVE; existing RevenueCat subscription and other topic unchanged.
- [x] Direct backend preflight with a freshly Google-signed ID token: HTTP 503; no real receipt, reward or Pub/Sub message created. Do not mark delivery operational. IAM inspection API remains disabled; signing-agent delivery not independently verified.
- [ ] Publish prepared backend through an approved route, provision server/Unity secrets, verify real Pub/Sub delivery and store notifications. AdMob IDs and Unity server-pair file still outstanding.
- Evidence: `Reports/GoogleRtdnOct20/`, updated owner guide. No Unity/runtime changes, build, screenshot, store upload or new publication attempt.

## Remaining live-service inputs — Oct 20

- [x] Rechecked existing Google credential and topic/catalog access without mutations: topic and catalog HTTP 200, topic IAM management unavailable, named authenticated push subscription HTTP 404.
- [x] Rechecked public backend health: HTTP 200/storage available, publisher and deletion credentials still unconfigured. Six AdMob fields remain empty; bounded local search found no exported Unity server key pair.
- [x] Replaced duplicate owner guide; removed completed agreements/payment/test-account setup actions, clarified internal publishing token versus receipt-service secret and browser-based Google/Apple login.
- [ ] Remaining: six AdMob app/unit IDs and cabinet settings; Unity server-pair file; secure secret provisioning and approved server publication; real sign-in/purchase/SSV/refund/deletion validation.
- Evidence: `Reports/OwnerSetupOct20/`, `Docs/LIVE_SERVICES_ACCESS_OCT19_RU.md`. Documentation only; no build, screenshot, runtime test, store upload or production mutation.

## Account backend security — Oct 18 (activation blocked)

- [x] Atomic SQL first-account create/CAS and immutable transaction/subscription-chain ownership; legacy protected-save import.
- [x] Server-issued AdMob SSV intents, signature verification, durable deduplicated receipts and actual client SDK integration; late rewards reconcile on sync.
- [x] Apple V2 and authenticated Google RTDN handlers, current-state reconciliation, grace/expiry/refund and token-replacement handling.
- [x] Tombstoned account deletion, retryable provider cleanup, single-use external code page, local deletion recovery and EN/RU/UK actions.
- [x] Worker version 1 deployed; 157 Cloud Code + 68 client + 28 Node checks passed; real Unity runtime compiled, localization built. Module prepared locally.
- [ ] UGS deployment/activation: local service account returns project HTTP 403; Player Accounts client ID and publisher/ad/deletion credentials missing.
- [ ] Live SSV, renewals/refunds, all-environment erasure and maintenance scheduler verification; external production HTTP probes currently return Cloudflare 403/1010.
- Evidence: `Reports/BackendReleaseOct18/`, `Docs/ACCOUNT_CLOUD_RELEASE_OCT18.md`; deployed source: `Backend/AccountCloud`.

## Clean menu composition — Oct 14

- [x] Remove foliage from controls, inventory panels and reward/item cards; preserve bright colors and rounded borders.
- [x] Five map landmarks only, two background islands in menus/gameplay, none behind pause; limit ambient leaves/craft.
- [x] Nine localized thematic world headings; fix first-world sky replacement and use clear sky without baked islands.
- [x] 43 PlayMode checks plus final 7-world-lifecycle checks; 76 native captures and 360 continuous frames.
- Evidence: `Reports/CleanCompositionOct14/`, `Docs/CLEAN_MENU_COMPOSITION_OCT14.md`.

## Unique world islands — Oct 14

- [x] Replace reused first-world cutouts with 54 complete authored island sprites, six for each world.
- [x] Share correct world artwork across map, all menus and gameplay; natural aspect, distance haze, clean alpha, fitted terrace bridges.
- [x] Fix confirmed stale TMP sprite-name registration after scene reload; glyph appearance unchanged.
- [x] 7 EditMode + 17 PlayMode tests; native nine-world/three-language review and 360-frame animation. No player build.
- Evidence: `Reports/UniqueWorldIslandsOct14/`, `Docs/UNIQUE_WORLD_ISLANDS_OCT14.md`.

## Thematic world menus — Oct 14

- [x] Follow all nine worlds on map and menus; preserve existing layout and actual pause/result world.
- [x] Clean alpha-only frame/floor mattes, world lighting, slow sky drift and thematic weather; night observatory scenery, no old blue-sky remnants.
- [x] Six airborne craft, rising cottage smoke, bounded gameplay flight band; reduced motion/quality/background lifecycle respected.
- [x] 5 EditMode + 15 PlayMode tests; native nine-world, EN/RU/UK safe-area screenshots and 540-frame motion review. No player build.
- Evidence: `Reports/WorldMenusOct14/`, `Docs/WORLD_MENUS_OCT14.md`.

## Map reference screen — Oct 14

- [x] Rebuild world overview from the supplied reference: open leafy corners, bright castle sky, five separate islands, linked rope stairs, clean locks, real star plaques, completed ribbon, localized game title, mode strip and common green Continue button.
- [x] Preserve nine worlds, level browsing/replay guards, pinned attempt, half-star totals, mode saves and existing journey/resume actions.
- [x] Islands and plaques sway together; stairs follow both live endpoints. Foreground foliage/daisies, mostly blurred falling leaves, pulsing green contour and light respect background, reduced motion and quality.
- [x] Uniform layout inside safe areas; attached sprigs follow actual viewport corners; new deep sandstone floor is uniformly cropped without stretching or duplicated-row seams.
- [x] 4 EditMode + 10 PlayMode checks passed. EN/RU/UK and four sizes visually reviewed; 104/104 native hit checks, no clipped labels, zero bridge attachment error throughout 360-frame 60 fps clip. No player build.
- Evidence: `Reports/MapReferenceOct14/`, `Docs/MAP_REFERENCE_OCT14.md`, `AssetPrompts/MapReferenceOct14/`.

## Pause reference screen — Oct 14

- [x] Rebuild pause composition from the supplied 720×1280 reference; seated apple Mika, lantern, daisies, wooden/castle backdrop and four controls. Reuse common primary button and header.
- [x] Home, Settings/back, Restart and Continue operate through existing routes and guards; preserve exact paused attempt and retry’s pinned puzzle.
- [x] Foreground-only lamp flicker, gentle daisies and natural depth leaves remain animated while gameplay is paused; app background, reduced-motion and quality preferences respected.
- [x] 17 EditMode + 14 PlayMode checks passed; old optional-parameter save fixture corrected.
- [x] Native EN/RU/UK, large text, safe areas and four sizes reviewed; 52/52 button hit checks, no clipped labels; six-second 60 fps clip. No player build.
- Evidence: `Reports/PauseReferenceOct14/`, `Docs/PAUSE_REFERENCE_OCT14.md`.

## My items reference screen — Oct 14

- [x] Rebuild Supplies as a single-item 720×1280 reference composition; new backdrop, blue spool, pale pedestal, storefront and star wallet; reuse shop header, canopy, parchment, cards and common primary button.
- [x] Four-card strip, zero-stock silhouettes, contained quantity pills with shadows, item-specific shop navigation; preserve prices, tool behaviour and save IDs.
- [x] Smooth item sunlight behind the sprite and depth foliage on item/trophy screens; pause, reduced motion and quality controls respected.
- [x] Native Unity visual review across EN/RU/UK, large text, safe insets and four viewport sizes; 115/115 active control hit checks, zero copy overflow.
- [x] 22 EditMode + 23 PlayMode checks passed; 480-frame 60 fps review clip. No phone build.
- Evidence: `Reports/ItemsReferenceOct14/`, `Docs/ITEMS_REFERENCE_OCT14.md`.

## Parcel reference screen — Oct 13–14

Implemented recipe-6 initial 1–2 reward cards and one verified video bonus of 2–3, durable/atomic claim and duplicate fallback, reference parcel artwork/layout, synchronized opening and spiral pickup, return to origin. 69 focused EditMode and 17 PlayMode checks passed. Native visual/click/localization/safe-area evidence and two 60fps sequences in Reports/ParcelReferenceOct13; detailed limitations in Docs/PARCEL_REFERENCE_OCT13.md. My Things first-ad entry and production cloud deployment remain explicitly outside this screen milestone.

## «Мои вещи» по исправленному зелёному референсу — 13 октября 2026

- [x] Четыре плашки с реальными счётчиками и переходами: огоньки → карта, предметы → инструменты, баланс → монеты магазина, трофеи → принадлежащая коллекция. Primary награды использует существующий видео-бонус; недоступна — серая.
- [x] Новый зелёный декор панелей, пакет предметов, значок видео-награды, прозрачная Мика, emerald-лист и два чистых островка; существующие фон/стрелка/заголовок/primary/монеты/фонарь сохранены. Пропорции и отступы проверены; на длинном экране рамка не повторяется, пол цельный.
- [x] Островки и дальний горизонт двигаются; естественные зелёные листья с падением, покачиванием и порывами ветра в подходящих меню и дневной карте. Пул ограничен, нет звука и блокировки нажатий; Reduced Motion/Effects Off соблюдены.
- [x] Unity MCP: 25 игровых кадров RU/EN/UK, разные размеры и safe-area, 164/164 raycast, без переполнений/обрезки. 9/9 PlayMode tests, Console без ошибок. Изолированный профиль снят, Play Mode остановлен. Reports/InventoryReferenceOct13; Docs/INVENTORY_REFERENCE_OCT13.md. Phone build не создавался.

## Яркие трофеи и аккуратные счётчики магазина — 13 октября 2026

- [x] Плашки количества с тенью по форме и внутренними отступами; карточки/иконки немного увеличены, «Куплено» помещается между листьями ленты. Покупка шире и центрирована с ценой.
- [x] Четыре новых прозрачных ярких иконки: золотое солнце, красная лента, голубая луна, синее облачко. Общие для магазина и коллекции; старые источники и остальные ассеты сохранены.
- [x] Unity MCP: 94 снимка RU/EN/UK и разных размеров/safe-area; 38×3 описаний магазина и 46×3 коллекции без переполнения/обрезки, счётчики внутри плашек/карточек, 8/8 PlayMode tests. Console без ошибок; временные профили сняты, Play Mode остановлен. Reports/ShopBrightnessOct13; Reports/CollectionBrightnessOct13; Docs/SHOP_BRIGHTNESS_OCT13.md. Phone build не создавался.

## «Мои трофеи»: два состояния, лупа и нижняя коллекция — 13 октября 2026

- [x] Экран открывает выбранный принадлежащий трофей сразу; нижний ряд из четырёх предметов и стрелок, зелёное «Применить», красное «Отменить», отметка «Применено». Верхние плашки, стрелки, карточки, свиток, Мика, иконки и каменная панель переиспользованы. Догенерированы прозрачная рамка лупы и красный вариант primary.
- [x] Лупа увеличивает настоящий участок изображения Мики вместе с реальным надетым значком; значок заметнее только в этом просмотре. Все 17 костюмов центрированы, лупа скрыта. Цвета/пропорции существующих спрайтов сохранены. Продолжение пола устраняет полосу неба на высоких телефонах.
- [x] 46 содержательных коротких описаний EN/RU/UK по структуре референса; цены, правила, ID и формат сохранения не менялись. Локальная примерка эффекта не меняет надетый стиль; применение/снятие и закрытие коллекции корректно управляют существующими эффектами.
- [x] Unity MCP: 40 игровых снимков, 353/353 raycast; 138 вариантов текста коллекции и 114 магазина без обрезки, все проверки лупы/центра/цвета/аспекта проходят, Console без ошибок. 8/8 PlayMode flow tests, включая все 17 костюмов. Изолированный профиль снят; Play Mode остановлен. Reports/CollectionReferenceOct13; Docs/COLLECTION_REFERENCE_OCT13.md. Сборка телефона не создавалась.

## Новые состояния магазина и примерка только в коллекции — 13 октября 2026

- [x] Доступные трофеи цветные; купленные серые с лентой «Куплено», выбранный сохраняет золотое выделение. Купленная кнопка серая и неактивная. Активная пагинация коричневая, недоступная белая; цены/ID/сохранения не менялись.
- [x] Из магазина удалён переход в примерку и применение. Полный экран Мики и применение/снятие стиля доступны в «Моих вещах». Повторная покупка не списывает монеты и не переключает стиль.
- [x] Догенерированы лента «Куплено», серый primary и плавно закруглённая подложка. Сохранены иконки/Мика и .meta заменённых ассетов; исправлены карточки, плашки количества, отступы, свиток описания, баланс и центрирование покупки по ширине локализованного текста.
- [x] Unity MCP: 54 игровых снимков, 522/522 raycast, все 38 товаров на RU/EN/UK без переполнения/обрезки; 6/6 PlayMode tests, Console без ошибок. Изолированный профиль снят, Play Mode остановлен. Reports/ShopRevisionOct13; Docs/SHOP_PURCHASE_STATES_OCT13.md. Сборка телефона не создавалась.

## Логотип по центру и реклама выше — 13 октября 2026

- [x] Логотип главного меню центрирован; значок отключения рекламы поднят в верхнюю свободную область и больше не опускается из-за дополнительной высоты неба.
- [x] Unity MCP: 27 снимков RU/EN/UK, обычный/узкий экран и safe area, 153/153 нажатия; изменённые подписи не переполняются, Console без ошибок. В safe area пересекаются только прозрачные углы прямоугольных областей (0 пересекающихся непрозрачных пикселей). Reports/HomeTopAlignmentOct13. Сборка телефона не создавалась.

## Каменная панель в шаблоне трофея — 13 октября 2026

- [x] Сгенерирована широкая неглубокая каменная площадка по референсу; заменён отдельный PreviewPlatform с сохранением .meta. Мика стоит на поверхности, свиток и название не перекрыты; шаблон общий для магазина и коллекции.
- [x] Визуально проверено в Unity MCP: 720×1280 и 390×844, RU/EN/UK. Повторная проверка магазина: 42 снимка, 372/372 raycast; переполнений и ошибок Console нет. Reports/TrophyStoneOct13. Сборка телефона не создавалась.

## Края главного меню и единый зелёный primary — 13 октября 2026

- [x] Реклама возвращена наверх; логотип не пересекается с ней, включая промежуточный формат 700×1280. Три кнопки меню стоят на одной линии; магазин и Play имеют общий горизонтальный центр.
- [x] Заменены обрезанные рамки и primary; используются чистые прозрачные силуэты с внешним запасом. Края старой площадки восстановлены единым фрагментом; передний контур дерева скрывает островки, сохраняя настоящие просветы. Боковые стойки непрерывны, движущийся облачный слой имеет мягкие границы.
- [x] Один зелёный спрайт из главного меню используется в основных действиях покупки/применения, общем MenuButton/NavigationButton и переходах между этапами. Непосредственно меняется только середина рамки; углы/листва масштабируются пропорционально высоте. Мика и предметные иконки сохранены.
- [x] Unity MCP: RU/EN/UK, крупный текст, safe area; 27 игровых снимков (588×1280, 720×1280, 700×1280, 390×844, 320×693). 153/153 raycast-попадания; нулевое смещение ряда/центра магазина, пересечений рекламы с логотипом нет. Изменённые подписи помещаются; Console без ошибок. 6/6 PlayMode tests. Два устаревших ожидания существующего теста результата обновлены под текущую победу/справку.
- Проверка, сравнение до/после и известный отдельный символ-разделитель дневника: Reports/HomeCornersOct13. Сборка телефона не создавалась.

## Магазин и примерка по четырём референсам — 13 октября 2026

- [x] Новое сгенерированное оформление: яркое деревянное окружение, вывеска/баланс, навес, вкладки, карточки, свитки, кнопки и площадка под Микой. Используются самостоятельные спрайты с чистой прозрачностью. Существующие иконки предметов/трофеев и Мика сохранены; исправлены границы старых атласных областей без изменения их изображения.
- [x] Сетка помощи/трофеев 3×2, монет 2×3; предпросмотр одинаковый из магазина и коллекции. В коллекции только принадлежащие игроку трофеи; магазин позволяет цветную примерку перед покупкой. Снятие стиля выключает его атмосферу в коллекции. UI сохраняет единый масштаб; персонаж и иконки не растягиваются, рамки используют защищённые углы.
- [x] Шесть наборов: 240 / 480 / 600 / 720 / 960 / 1080. Четыре новых изображения наборов. Баланс и предполагаемые ценовые множители описаны в Docs/COIN_PACK_BALANCE_OCT13.md; крупнейший набор сам по себе меньше стоимости одного экземпляра всех шести инструментов (1230). Цены инструментов и доход не изменены. Денежные цены берутся из магазина платформы.
- [x] Unity MCP / Play Mode: визуальная сверка RU/EN/UK, 720×1280, 390×844, 320×693, крупный текст, safe area и длинные цены. 42 снимка; 372/372 реальных raycast-попадания; переполнений TMP и ошибок актуальной Console нет. 12/12 EditMode economy tests, 5/5 PlayMode trophy-flow tests, 13/13 receipt verifier tests. Изолированный QA-профиль снят; реальные покупки и пользовательский прогресс не затрагивались.
- [x] После предыдущей полировки главного меню выровнены три иконки, сохранена перспектива пола и пропорции композиции на узких экранах. Проверки/снимки: Reports/MenuProportionsOct13.
- [ ] До живых продаж зарегистрировать четыре новых consumable SKU, настроить цены в App Store/Google Play, опубликовать обновлённые Cloud Code/verifier и проверить настоящие sandbox-покупки. Эти внешние действия здесь не выполнялись. Сборки телефона не создавались.
- Итоговые изображения, сравнение с исходниками и результаты: Reports/ShopReferenceOct13.

## Полировка живого главного меню — 13 октября 2026
- Удалены четыре самостоятельных декоративных листка на небе. Трофеи рисуют свои эффекты отдельными слоями за персонажем и рамой; без экипировки эти слои выключены.
- Переиспользованные облака получили плавное смещение. Два небольших гостя используют существующие четыре позы птиц/насекомых, начинаются за краем кадра, летят дугами за Микой и чередуют стороны. Соблюдают Reduced Motion и качество эффектов.
- Три квадратные кнопки используют один спрайт рамки, одинаковый размер и состояния нажатия; иконки отделены от фона. Зелёная Play сохраняет главный акцент. Убраны прямоугольные остатки исходного фона; деревянная площадка восстановлена цельно.
- Play Mode через Unity MCP: 720×1280, 390×844, 320×693, RU/EN/UK, симуляция safe area; визуально просмотрено. 5/5 настоящих raycast-попаданий; переполнений подписей нет. Движение облаков подтверждено; после удаления тестового эффекта VisibleCount=0; эффекты off/Reduced Motion скрывают посетителей. Console после проверки: 0 ошибок.
- Отчёт и скриншоты: Reports/MenuPolishOct13. Сборка телефона не выполнялась.

## Главное меню по референсу — 13 октября 2026
- Новый главный экран: логотип Flying Ladders, насыщенная Мика, три квадратные кнопки на деревянной площадке, зелёная кнопка Play с листвой, отдельная кнопка Remove ads. Действия и локализация сохранены.
- Мика, логотип, иконки/рамки кнопок и декоративные детали вырезаны из предоставленного референса. Чистое окружение восстановлено с Imagegen; верх и нижняя площадка сохраняют перспективу и пропорции. На высоких экранах расширяется небо, персонаж и площадка не растягиваются.
- Семь самостоятельных движущихся островков; декоративные листки удалены; дерево/листва рисуются перед дальними островами. Мика сохраняет работу надетых трофеев.
- Проверено в Play Mode через Unity MCP: 720×1280, 390×844, 320×693; RU/EN/UK; симуляция верхней/нижней safe area. Переполнения подписей не обнаружены.
- Пять переходов и реальных EventSystem raycast-попаданий по кнопкам проходят проверку; Play открывает карту или первый пролог согласно существующему прогрессу. Проверено движение 7/7 островков и остановка 7/7 при Reduced Motion. Console: 0 ошибок.
- Сравнение непрозрачной внутренней области Мики (104283 пикселя): средняя абсолютная разница RGB между референсом и Unity = 0/255; opacity и цвета не приглушены.
- Итоговые игровые скриншоты и сравнение: Reports/MenuReferenceOct13. Сборка телефона не создавалась.

## Полёт птиц/насекомых и экран результата — 11 октября 2026

- [x] Птицы, насекомые и декоративные гости начинают маршрут за краем кадра; если экран открылся посреди их полёта, они ждут следующего пролёта. Маршруты получили более широкие боковые дуги и возвратный поворот, чтобы не читаться как прямое падение сверху вниз.
- [x] Частота визитов увеличена умеренно: боковые визиты в игровом мире 32→28 секунд, на карте 40→36 секунд; одновременный рой не добавлен.
- [x] «Мій щоденник» на странице «Мои вещи» помещается в одну строку на узком экране; проверено при крупном тексте. На результате увеличены медальоны статистики, счёт вписывается в медаль рекорда, заголовок уменьшается под длинные локализованные варианты, звёзды отделены от ленты, Мика уменьшена и опущена.
- [x] Unity: 19/19 focused EditMode motion tests, 2/2 PlayMode visitor lifecycle tests; native visual review всех 9 игровых миров и 17 собранных видов, puzzle hash unchanged. FiveStarReview PASS: captures 390×844, 360×648 и 320×693, включая крупный текст и украинский заголовок дневника; актуальная Console без ошибок. Play Mode остановлен, временный профиль снят. Reports/FiveStarResults, Reports/WildlifeMotion-Oct08, Reports/CascadeWildlifeFollowup.
- [ ] NOT RUN: сборка/установка на телефон и измерение физической производительности.

## Экран победы и три звезды — 11 октября 2026

- [x] Экран победы перестроен для 360px: широкая лента, три крупные звезды, увеличенный рекорд, крупные награды, затем время/ходы/отмены. Добавлены прозрачные авторские спрайты состояний звезды и Мики с поднятым фонарём; Мика находится за нижним действием. Анимация идёт слева направо через пустое/половинное/полное состояние, свет фона усилен, существующее движение мира сохранено.
- [x] Новый результат использует RatingVersion 2 и шкалу 1–3. При первой загрузке старые 1–5 результаты переводятся в 1–3 без изменения очков, разблокировок или пройденных уровней; список уровней и итог острова используют три позиции. Design note: Docs/FIVE_STAR_RESULTS_AND_ACTIVE_TIME.md.
- [x] Финальная геометрия сверена с референсом: медаль рекорда широкая и ниже (.78 ширины × .20 высоты); у трёх звёзд видимый интервал, награды и статистика подняты к исходным координатам образца. Мика рисуется при alpha=1; в состоянии «Забрать трофей» боковых действий нет.
- [x] Добавлены золотые sprite-глифы `×` и `:` к атласу цифр; ширина × согласована с цифрами. Заголовок победы теперь regular с лёгкой тёплой обводкой.
- [x] В Unity визуально проверены RU 390×844 и UK 320×693, включая крупный текст. Полный FiveStarReview PASS: нет текстового переполнения, проверены звёзды/промежутки, счёт без разделителя, ×/:, наградная статистика, Мика, отсутствие боковых действий и неизменность результата. Сумка не показывает лишний папирус «Выбери предмет». Экран настроек в узком RU-крупном тексте также не обрезает названия. Captures/runtime: Reports/FiveStarResults.
- [x] Последний адресный PlayMode набор — 2/2 PASS: снятие трофея выключает листовой preview и emitter; карточки/сумка не возвращают папирус выбора.
- [x] iPhone Development build 202610114: Unity `Succeeded`, 0 ошибок / 358 предупреждений; Xcode exit 0, arm64, iOS 15 minimum. Установлен на подключённый iPhone 17. macOS `codesign --verify --deep --strict` возвращает `CSSMERR_TP_NOT_TRUSTED` как для этого, так и для предыдущего артефакта; штатная установка на устройство при этом завершилась успешно. Reports/iPhoneInstall-20261011-142711.
- [x] Обновлённый iPhone Development build `202610115`: Unity `Succeeded`, 0 ошибок / 358 предупреждений; Xcode Release `exit 0`. Версия 0.1.0 / build `202610115` подтверждена в установленном приложении на iPhone 17. Повторная проверка запуска не завершилась: CoreDevice оборвал подключение после установки. Подпись локально сообщает `CSSMERR_TP_NOT_TRUSTED`, как и предыдущий артефакт. Reports/iPhoneInstall-Oct11-202610115.

## Линейная игровая сумка и экран этапа — 11 октября 2026

- [x] Шесть инструментов игровой сумки теперь раскрываются одним горизонтальным рядом из центральной сумки и так же собираются обратно. Сохранены порядок, счётчики, 44px+ touch targets на узком экране, доступность и существующая unscaled-анимация.
- [x] Удалён отдельный папирус-инструктаж «Выбери предмет»; заголовок выбранного инструмента и описание предмета сохранены.
- [x] Экран промежуточного этапа больше не выводит Undo с ошибочным числом. «Карта» занимает всю нижнюю строку под «Следующим этапом».

## Первый вход и Мика на победе — 11 октября 2026

- [x] Добавлен одноразовый стартовый экран: Apple, Google или продолжение без входа. Он показывается и для уже существующего локального профиля после обновления, пока выбор не сделан. Пропуск оставляет прогресс в локальном сохранении телефона; после подключения Unity Player Accounts вход защищает прогресс и покупки между устройствами. До заполнения авторского Client ID кнопки входа честно недоступны, а экран объясняет причину.
- [x] Иллюстрация Мики для победы заменена на новую позу по предоставленному референсу: те же одежда, палитра и персонаж, поднятый фонарь, прозрачный фон и обрезка за нижней кнопкой.

## Правки отмены, трофеев и магазина — 11 октября 2026

- [x] Обычные Undo / Undo spool не возвращают разрушенную платформу (обмен / взрыв); это задача Return hourglass. Отмена локального обмена и спуска с целой платформы сохранена, в том числе после загрузки. Краткое сообщение тупика выбирает доступное действие: отменить / повторить.
- [x] Трофейные частицы полностью видимы; листьям/лепесткам добавлены инерция, меняющийся ветер, вращение и возврат только за границей экрана. Reduced Motion и приостановка сохранены; игровой state не меняется.
- [x] Снятие листового трофея останавливает не только экипированный фон, но и preview-эмиттер в каталоге; предпросмотр магазина остаётся активным только пока он показан.
- [x] Трофеи объединены в один каталог: крупные карточки 3×2, без Мика/Фон/Рамки. Увеличены также карточки инструментов. Заголовок RU без переноса, боковые отступы, группа посылки по центру. Убраны отмеченные кнопки каталога предметов, «Мои пути», лишние подписи трофеев и правила инструментов на экране монет. Применение осталось в игровой сумке, обучение не указывает на удалённые кнопки.
- [x] Native Unity: 59/59 EditMode и 10/10 различных актуальных PlayMode проверок (последний исправленный тест сумки повторён отдельно). Подтверждены сохранение, отмена, часы, отсутствие Undo на проигрыше, предмет применяется один раз, непрозрачность и дрейф. Старые неуспешные прогоны сохранены; validation.json объединяет последние результаты по каждому тесту.
- [x] Шесть актуальных native Editor captures RU, 390×844 и 320×693 с Large Text; верстка визуально просмотрена. Первый синтетический fixture посылки нарушил receipt-инвариант и был заменён валидным свежим профилем; защиту сохранений не ослабляли. Финальный Stop Play Mode без ошибок сохранения; compilation failed=False, текущая Console: 0 errors. Reports/UiReview-Oct11.
- [ ] NOT RUN: весь каталог уровней, полный тестовый набор, физический телефон, FPS, реальные ads/IAP. Новый iPhone build в этом проходе не собирался. Цены, ID и схемы сохранений не менялись.

## Установка на iPhone — 11 октября 2026

- [x] По прямому запросу удалены старый Unity iPhone export и его Xcode DerivedData; исходники, отчёты, скриншоты и сохранения оставлены.
- [x] Текущий CascadeUnity экспортирован в iOS Development: Succeeded, 0 errors / 358 warnings. Свежий Xcode Release build: exit 0; подпись проверена, все 34 файла Data совпали с экспортом, включая boot.config и global-metadata.dat.
- [x] CASCADE 0.1.0 / 202610111 установлен поверх com.vickand.cascade на iPhone 17. Версия подтверждена устройством; профиль до/после установки совпал побайтно. Приложение запущено и процесс остался активен после 12 секунд.
- [x] ProjectSettings сохранён побайтно; 372 C# source hashes не изменились. Reports/iPhoneInstall-Oct11-202610111 содержит журналы и подтверждения.
- [ ] NOT RUN: EditMode/PlayMode suites, визуальный/касательный/звуковой QA, FPS и реальные рекламные/магазинные транзакции. Проверен запуск приложения.

## Установка на iPhone — 10 октября 2026

- По явному запросу собраны текущие изменения (новые 10 акустических тем, HUD padding, адаптивные подвесы и горизонтальный таймер). Unity 6000.3.8f1: iOS Development export Succeeded / 0 errors / 360 warnings (включая package compute-shader variants). Xcode Release exit 0; Apple Development signature verified. CASCADE 0.1.0 / 202610101, прежний com.vickand.cascade.
- Установка на iPhone 17 подтверждена devicectl success, поверх существующей версии, без uninstall/reset. Автозапуск отказан iOS: устройство заблокировано. Запуск, звук, визуал, касания, FPS и сохранения на устройстве NOT VERIFIED.
- ProjectSettings.asset восстановлен побайтно (SHA-256 совпадает с исходным). Другой открытый проект OVERFLOW не менялся. Первоначальная остановка отдельного LicensingClient устранена использованием действующего Hub licensing IPC; не изменяли лицензии/аккаунты. Новые release IDs/monetization flags не выставлялись.
- Reports/iPhoneInstall-Oct10: unity-build.txt, xcode-build.txt, build-metadata.json, install.json, launch.json, source-hashes.json. Воспроизводимый локальный helper DevicePlayerBuild.cs, только явный вызов; не store submission.

## Подвес бомбы на тесных этажах — 9 октября 2026

- По снимку C403 stage1: бомба накладывается на следующую доску/прочность, таймер повёрнут. В текущем исходнике counter-rotation уже был, но фиксированный размер подвеса мог превышать весь доступный промежуток; clamp получал несовместимые верхнюю/нижнюю границы. Точное воспроизведение снимка в Editor НЕ подтверждено.
- Все подвесные предметы подгоняются по свободной высоте с учётом диагонали при раскачивании, реакции, верёвки и таймера. Текст бомбы теперь сосед корпуса в иерархии, следует под его нижней границей без вращения, скрывается вместе с использованным устройством. Финальный constraint удерживает предмет после столкновений вдали от следующей доски. Domain/ходы/бомбы/IDs не менялись.
- Tools/check_hanging_fit.py: baseline невозможного footprint воспроизведён; 11,988 angle/gap/scale checks PASS. Добавлен PlayableTests.LateCampaignBombTimerStaysUprightAndClearOfTheNextDeck (NOT RUN). Native Unity не ответил; свежая визуальная проверка и консоль недоступны. iPhone build/install NOT RUN.

## Акустическая музыка и отступы HUD — 9 октября 2026

- Все 10 активных тем заново сочинены/отрендерены с реальными CC0-сэмплами пианино, арфы и виолончели. 64 такта: куплеты, развивающиеся припевы, бридж и переход в начало; 2:59–3:53 вместо коротких GM-циклов. Обновлены 11 WAV с сохранением .meta, резервный music ссылается на новое меню. Сохранены старые WAV и источники/лицензия; Docs/ACOUSTIC_SCORE_OCT09.md.
- Верхние подсказка/пауза используют ширину и центр нижней Actions-панели; 48px touch targets и прежняя геометрия верхних канатов сохранены.
- PASS: 10 разных score/form/audio/seam/headroom checks, 11 действующих references, streaming/import flags, происхождение 28 CC0 samples; 330 screen/zone routing cases + fallbacks. Reports/AcousticScore-Oct09. Native Unity не ответил: импорт, прослушивание и визуальные отступы в игре NOT VERIFIED. Полная компиляция и iPhone build/install NOT RUN.

## Свежая PNG-карта экранов — 9 октября 2026

- Все 57 кадров пересняты в текущей native Unity версии, 0 ошибок захвата. Отдельный QA-профиль, без изменения сохранений игрока. Обычная карта без автоматического offer; отдельные кадры раскрытой inline-сумки, личной миссии, полок и предпросмотров. Удалённые Requests / подтверждение языка не включены как существующие окна.
- Reports/ScreenRoadmap-Fresh-Oct09-v2/cascade-navigation-clear.png: 4400×23050, 34 коротких маршрута, 103 подписанных перехода, крупные подписи, отдельные возвраты; формат предыдущей карты сохранён. Все источники свежие, архивных подстановок нет. screens.json содержит UTC и CurrentScreen; navigation-clear-sources.json фиксирует источники.
- Native capture compilation PASS; полнота переходов и геометрия подписей PASS, карта и ключевые состояния визуально осмотрены. Capture helpers используют явный camera render / Editor frame pump для работы без repaint Game view. Проверка на физическом телефоне, полный игровой QA и новый iPhone build в этом проходе NOT RUN. Предыдущий установленный билд сохранён.

## Установка на iPhone — 9 октября 2026

- По новому явному запросу пользователя собрана текущая Development iOS версия 0.1.0 / 202610091. Unity 6000.3.8f1: экспорт Succeeded, 0 ошибок; Xcode Release: exit 0. Сцена CascadePlayable, актуальная награда recipe 3 подтверждена в загруженной сборке Editor. Подпись Apple Development проверена codesign с системным keychain.
- com.vickand.cascade установлен и запущен на доступном iPhone 17; результат devicectl success. Сборка не удаляет другие приложения. Временные bundle/name/team/build-number настройки проекта восстановлены после экспорта. Реальные ID и флаги монетизации не изменены.
- Доказательства: Reports/iPhoneInstall-Oct09/{unity-build.txt,xcode-build.txt,install.json,launch.json,build-metadata.json}. Native build выполнен, прежняя ошибка компиляции Editor больше не воспроизводится. Визуальная проверка телефона, касания, сохранения/resume, FPS и live purchases/ads NOT RUN; Mac CUA остаётся заблокирован.

## PNG карты для удобного чтения — 9 октября 2026

- Reports/ScreenRoadmap-Oct09/cascade-navigation-clear.png: короткие локальные маршруты слева направо, до четырёх карточек; крупные Arial-подписи, спокойный фон, отдельные возвраты без пересечения основных стрелок. Цвет дополнен словами. Никаких диагнозов или личных данных на изображении.
- Сохранены все 98 переходов исходной карты. Пять связей, прежде указанных в сносках, тоже показаны карточками и стрелками. Старые экраны без входа вынесены отдельно. Кадры остаются архивными от 8 октября; сумка над кнопками и предложение миссии явно без свежих снимков.
- Проверены полнота связей, границы подписей и отсутствие наложений на карточки. Проведена визуальная проверка общего вида и крупного фрагмента. Исходная карта сохранена. Новый Unity capture / iPhone build NOT RUN.

## Полезные награды посылок — 9 октября 2026

- Новые посылки (recipe 3) гарантируют +20 монет вместе с текущим фрагментом/трофеем/инструментом; монетная посылка и компенсация дубликата дают 40. Старые заработанные/неоткрытые receipt 0–2 сохранены без изменения. Цены, SKU, интервалы и gameplay не менялись; design note: Docs/RELEASE_FIXES_OCT09.md.
- Экран открытой посылки показывает дополнительный доход. Claim/save атомарен; повтор и слияние двух одинаковых получений не увеличивают награду. Общие источники клиента и Cloud Code рассчитывают одну награду. Прирост за все 540 уровней ограничен 1,080 монетами сверх прежних посылок.
- Проверено: 109 PASS / 0 FAIL чистых C# NUnit assertions (монеты, legacy receipts, rollback, reload, два устройства, инструменты, косметика, cloud reconciliation), Reports/ParcelRewards-Oct09. Cloud Code boundary: 80 PASS на fake SDK transport. Fresh native Unity UI/PlayMode и live cloud NOT RUN; iPhone build NOT RUN. Перед релизом redeploy Cloud Code с recipe 3.

## Проверка кампании и холст дизайнера — 9 октября 2026

- По уточнению пользователя сделан именно единый PNG с карточками и подписанными стрелками, без необходимости открывать HTML: Reports/ScreenRoadmap-Oct09/cascade-navigation.png (5200×13380, 71 карточка, 98 переходов, 45 уникальных настоящих архивных снимков). Возвраты/автоматические события/условные ветки и две рекламы показаны отдельно; повтор карточки — тот же экран в другой ветке. Новая inline-сумка и offer миссии честно помечены без снимка. Мастерская, старое меню просьб, старое подтверждение языка и Contracts отмечены как не имеющие прямого входа в текущих меню; новых путей не выдумано. Подписи/стрелки сопоставлены с текущими callbacks; финальная геометрическая проверка 0 пересечений подписей между собой и с карточками, PNG осмотрен. Tools/build_navigation_png.py.

- Адаптивные личные миссии теперь берут фактический published baseline и его ведущую механику вместо потенциально более лёгкой динамической копии. После двух clean wins выбирают минимум +2 optimal actions в прежнем ограниченном диапазоне; при Struggle>=2 откладывают новые предложения, уже предложенное/начатое не меняют. Запас/покупки не входят в выбор сложности; все свидетели проходят без расходников. Исправлен недостаточный поиск вариантов для C360 после перехода на настоящий baseline. Final pure NUnit15/15 PASS (initial published14/15 сохранён); две confidence-проверки и десять источников обычных миссий, save/mode/replay/cancellation. Tools/check_personal_missions.py; Reports/CampaignRoadmap-Oct09/adaptive-missions. Production Domain/Solver/Application/Content/Persistence и Presentation source compile PASS вне Editor; это не native импорт. Unity доступен через MCP, но native Console показала internal backend BuildFinishedMessage error, loaded assembly остаётся старой; запрос repair cache выполнен, результат native восстановления НЕ подтверждён (Mac снова locked). Полный визуальный обход уровней и phone build/install не запускались по просьбе пользователя. Лист сверху в трофеях — счётчик открытой коллекции, не валюта; leaf pin — косметика из трёх parcel fragments.

- Дополнительно проверена зависимость готового решения от каждого entry/gust: 372 перестановки при входе и 12 игровых порывов реально меняют ряд, а удаление соответствующего воздействия делает прежний сертифицированный маршрут неуспешным. Первая платформа ни в одном опубликованном этапе не содержит entry/gust. Повторный полный audit PASS; новых нестыковок этот проход не выявил.

- Проверен фактический packaged catalogue, а не только динамическая генерация: 540 уникальных триад, 1620 заново найденных оптимальных решений, 6471 primary/localized replay, 335 meaningful bomb windows, 646 benefit-ablation checks, 3292 lower-durability counterexample probes. Все свидетели сохраняют multiset; нет стартового wind/entry transform и пустых неиспользуемых платформ. Tools/audit_published_campaign.py; Reports/CampaignRoadmap-Oct09/audit-result.txt.
- Исправлена просадка четвёртого мира в опубликованных рецептах и длинные повторения ведущей механики: 62 заменённых набора. Среднее optimal actions/stage миров 1–9: 3.03, 3.87, 4.37, 4.89, 4.91, 4.94, 5.39, 5.68, 5.81. Это показатель маршрута, не доказательство человеческого удовольствия. На некоторых тематических узлах один принцип повторяется, но все триады различны; серия до восьми подряд сокращена, максимум теперь четыре. Сохранены source/node/order/store IDs и цены/правила.
- Старые варианты сохранены в server-only архиве. Для повторно изменённых узлов добавлены node@recipe-digest keys; сервер выбирает архив строго по реальному first-stage hash и проверяет весь replay. Backend boundary suite: 80 PASS, fake SDK transport, не live cloud.
- Холст: Reports/ScreenRoadmap-Oct09/roadmap.html; 59 настоящих архивных снимков от 8 октября, 10 последовательных групп, 4 явно ожидающих fresh capture состояния. Увеличение, сохранение/очистка комментария проверены в браузере; ошибок JS нет. Экспорт JSON, обзор PNG, designer-pack.zip. Портативный self-contained HTML. Старые удалённые/QA экраны отделены от действующего пути.
- Fresh PlayMode, кадры новой сумки, миссии/preview, 6→7, touch/resume и субъективная интересность BLOCKED/NOT RUN: Mac locked, основной Editor stale compilationFailed/no fresh assembly despite refresh/reload, изолированный batch Editor не инициализировал license IPC. Два только наших batch процесса завершены. Основной проект/реальный профиль/телефон не затронуты, iPhone build NOT RUN. User could not unlock; no password/security bypass attempted.

## Продолжение: уровни, ввод и панель инструментов — 9 октября 2026

- Старое сохранение с пройденным C01 больше не запускает вводный урок обмена на следующих уровнях из-за отсутствующей новой отметки управления; повтор C01 сохраняет возможность урока двух способов. Добавлена отдельная PlayMode проверка (NOT RUN: Mac locked).
- Исправлен сброс признака прохождения кампании при асинхронной загрузке. Первые 14 позиций получают разные механики; шестая (C187) — мост, седьмая (C188) — зеркало. Кнопка следующего уровня запускает выбранный маршрут без параллельного диалога карты; получение посылки сохраняется.
- Заменены шесть точных повторов трёхэтапных рецептов. Все 540 опубликованных наборов различаются; проверены 6471 primary/localized прохождения. Генератор проверяет необходимость эффектов, минимальную прочность и бомбы до публикации; новые варианты прошли те же проверки. Старые незавершённые попытки сохраняют свои рецепты; сервер принимает сохранённые предыдущие доказательства изменённых уровней.
- Исправлены вход на нижнюю платформу, зависание подготовки при активном уроке и потеря удерживаемого касания. Механический урок допускает самостоятельные действия; ручной спуск с целой платформы можно отменить обычным Undo. Отмена после разрушения сохраняет правило песочных часов.
- За повторное прохождение предусмотрен отдельный ограниченный резерв: две монеты за каждую достигнутую звезду повтора, до 10 монет на уровень суммарно между режимами/устройствами. Повтор на две звезды после первого результата на три даёт четыре монеты, одинаковые повторения не фармятся. Рекорды сохраняются. Сервер проверяет два независимых реальных прохождения и рейтинг награды.
- Одна игровая сумка раскрывает пять иконок с количеством над кнопками, без перехода в каталог. Повторное нажатие сворачивает; выбранная иконка появляется перед анимацией применения. Сохранение и расход инструмента атомарны до анимации. Целевой ряд освобождает место панели. Undo больше не превращается во вторую сумку. Ускорены прямые действия из старого меню предметов; переходы меню/острова и вход в игру анимированы, Reduced Motion соблюдается.
- Готовы Configs/admob.json (6 публичных ID), Configs/release-ids.json и команда CASCADE → Release → Apply AdMob IDs from Configs. Полный список внешней настройки/релизных незавершённых механизмов — Docs/RELEASE_SETUP_OCT09.md. Реальные ID не выдуманы, тестовые флаги пока сохранены.

Выполнено: 61/61 native EditMode; четыре успешных native PlayMode проверки посадки/Undo/жестов до последнего изменения панели; 76/76 серверных проверок на fake SDK transport; 540 различных рецептов / 6471 прохождение после публикации. Последние Presentation (player и editor) и PlayMode fixtures отдельно проверены компилятором Unity без ошибок. Предыдущие два запуска проверки 6 → 7 остановились на непрочитанном комиксе и получении посылки в тестовом профиле; fixtures исправлены, неуспешные отчёты сохранены.

НЕ ЗАВЕРШЕНО: финальный native набор InlineBagJourney-Oct09 и визуальная проверка новой панели/анимации. Mac заблокирован; Editor сообщает scriptCompilationFailed, не загружает свежую сборку и не позволяет запустить набор, при этом MCP Console возвращает пустые ошибки. Запрошена разблокировка, отдельные сборки исходников не выдают CS ошибок. Не считать новую панель или финальный переход 6 → 7 проверенными в игре до этого запуска. iPhone build/export/install, live stores/ads/cloud, проверка удовольствия от игры NOT RUN. Мобильная установка ожидает отдельного запроса автора.

Ветер, живность, подвесы и музыка — 9 октября 2026: декоративные порывы снова заметно двигают дерево, верёвки и фигуры, не отправляя игровую команду и не показывая предупреждение перестановки. Добавлены разнообразные кривые полёта, ускорение/замедление, изменения видимого масштаба/глубины, более частые посещения и заметные насекомые. При посадке птица тормозит и учащает взмахи; крылья складываются из фактической последней позы. Вместо заранее назначенных горизонтальных доков — отдельные тела, сила тяжести, инерция, ограничение длины верёвок и контакты; густые подвесы получают достаточную длину в пределах свободного пространства. Перерасчёт размеров не оставляет пересечения в Reduced Motion. Музыкальные переходы используют прошедшее время, а не ограниченный шаг физики; реальные входы проверены во всех девяти мирах. Все десять установленных WAV совпадают с обновлёнными партитурами, соло ниже на октаву; настроение музыки не доказано анализом сигнала. Защита финалов от рекламы сверяется с порядком JourneyStory, а не числом в ID. Текущий интервал рекламы сохранён; показ возможен после возврата на карту, следующий уровень идёт без рекламы. Автор отложил внесение production ID; тестовая монетизация остаётся до конфигурации и проверки сервера. Полный список — Docs/RELEASE_SETUP_OCT09.md.

Проверки продолжения: 25/25 итоговых targeted EditMode PASS; native все девять музыкальных тем/возврат/сброс выхода, два теста pooled wildlife, ветер/pause/Reduced Motion и самый тесный общий контакт пяти предметов PASS в отдельных пересекающихся наборах. Два кадра UK320 просмотрены в Unity; состояние/маршрут не изменены, один AudioListener. Предыдущие неуспешные воспроизведения сохранены в Reports/LifePhysics-Oct09*; актуальные доказательства — LifePhysics-Oct09-final-poses, LifePhysics-Oct09-contact-final, LifePhysics-Oct09-verified и визуальная папка. Мобильные билды/установка, физический iPhone, live store/ads/backend и human enjoyment NOT RUN. Расчётной просадки в середине реального порядка кампании нет; не выдавать сортировку числовых ID или solver-метрики за проверку интересности. Пакеты, игровые правила, цены, product IDs и пользовательские ассеты сохранены.

Аудит и исправления перед релизом — 9 октября 2026: продолжены по запросу автора, мобильные сборки/экспорт/установка не выполнялись. Обычные сохранения пишутся одним фоновым работником с объединением очереди; выигрыши, покупки, облачные изменения, уход в фон и переходы с потерей пути остаются прочными барьерами. Ошибки записи видны и доступны для повторения. Добавлены ограниченное сжатие облачных данных, проверка новых прохождений на сервере, привязка покупок к авторизованному игроку, таймауты покупок, восстановление после потерянного ad-close callback и корректная задержка автосинхронизации. Свежие опубликованные уровни используют готовые проверенные рецепты; каталог, исходный уровень и содержимое рецептов сверяются, черновики/старые данные используют генератор. Доказательства отдельных рекордов звёзд и счёта сохраняются и объединяются отдельно. Серверные базовые снимки восстанавливают полную историю чеков через проверенный неизменяемый префикс; ни один финансовый чек не удалён. Убраны квадратичные поиски чеков. История с достигнутым прежним пределом не начинает новую покупку/видео, повторные подтверждения старых оплат допустимы. Разведены подвесные предметы с общим контактом, включая пять типов одновременно; логические контакты не изменялись. EN/RU/UK добавленные сообщения обновлены. Цены, опубликованные правила, package versions и product IDs сохранены; design notes — Docs/RELEASE_FIXES_OCT09.md.

Проверки этого аудита: широкий pure C#/NUnit sweep 555 PASS / 0 FAIL / 2 SKIP, затем проверки затронутых финальными исправлениями систем в Unity — 44 PASS экономических случаев, 31 PASS доказательств/совместимости; 66 PASS серверных границ на поддельном SDK transport и 12 PASS verifier на mocked store adapters. Проверены 540 рецептов / 6471 primary/localized прохождение, эффекты/бомбы, случайные переходы и один serial персональных миссий по всему каталогу. Проверка 1074 реальных clear-доказательств + 10000 синтетических чеков + 8 устройств: 3043832 байт хранения, 1456532 байт ответа; полный merge около178 мс на этом Mac. В Unity проверены вход в поздний уровень через готовые рецепты, меню/магазин/возврат, suspend-save/Undo, туториал двух жестов и Undo, музыка, смена языка/прочитанные истории и отсутствие перекрытия пяти предметов. Старые неудачные/устаревшие fixture-отчёты сохранены; исправленные случаи отдельно проверены, весь Unity suite повторно не запускался. Native карта/список острова UK320 Large Text и подвесные предметы визуально просмотрены; тестовые профили отключены. 880 Assets metadata / 0 duplicate GUIDs, новые .meta сохранены, копии ресурсов совпадают, 6 исторических test exports восстановлены. Источники проверки — Reports/ReleaseAudit-2026-10-09/validation-final.json и focused ReleaseFixes-Oct09-* отчёты.

До релиза остаются открытые задачи: атомарное создание облачного аккаунта/владельца покупки на реальном серверном хранилище, AdMob SSV, автоматическое применение refund/revocation, удаление аккаунта, согласованная миграция старых неподтверждённых результатов, архив чеков для роста сверх прежнего потолка; это незавершённая серверная работа, а не только заполнение ID. Нужны реальные ID/ключи, deployment verifier и Cloud Code, настройки Player Accounts/production/магазинов/UMP/подписи и store listing. Физический cold-start/resume/input/FPS/память/нагрев, live cloud concurrency, реальные sandbox покупки/restore/refund/ads и игровые сессии людей NOT RUN. Полный список имён полей и настройки для автора — Docs/RELEASE_SETUP_OCT09.md. Cloud live flags не включались, реальные цены/ID/секреты не подставлялись.

Острова, переигровка и комиксы — 7 октября 2026: тап по открытому острову показывает его уровни, звёзды и лучший счёт; закрытые уровни не запускаются. Текущий уровень продолжает сохранённый этап, пройденный явно запускается заново. Добавлены кнопки между открытыми мирами с отдельным Home, компактнее верх/низ карты, больше места и разнесение островов. Комиксы сохраняют 3:2 без NPOT-искажения; тематическая реплика с портретом и полным текстом собрана под иллюстрацией. Исправлены контраст ночного списка, поиск старого heading и teardown callback. 10 focused native coroutine cases PASS; после визуальных правок scoped navigation и 27-panel geometry/lifecycle reruns PASS. Девять финальных Unity кадров проверены, RU390/UK320 LargeText/simulated safe insets, кнопки>=44px, no final overflow/aspect errors. Правила/цены/IDs/schema/packages сохранены; QA-профиль/драйвер убраны. Полный suite, build/install, physical-device/performance/human enjoyment NOT RUN. Docs/MAP_ISLAND_BROWSE_OCT07.md; Reports/MapIslandBrowse/README.md.

Косметические трофеи — 7 октября 2026: восемь видимых комплектов, три новых костюма Мики в двух позах и портретах, пять нашивок/значков на одежде, восемь тем атмосферных украшений. Примерка показывает Мику и результат; покупка за прежние 240 монет либо заработок через прежние фрагменты и 15/30/45/60 уникальных прохождений. Экипировка общая для всех режимов и runtime-иллюстраций; история/баланс/Domain/Solver/цены/IDs/schema/packages сохранены. Тестовая примерка отделена от постоянной экипировки. Девять focused pure ownership cases и 183 native runtime assertions PASS, 92 Presentation scripts compile0errors/7existingwarnings. Восемь Unity кадров визуально проверены, RU390 и UK320 LargeText/simulated safe insets; независимый review новых блоков PASS. Драйвер и QA-профиль убраны. Полный suite, билд/установка, physical-device/performance и human enjoyment NOT RUN. Docs/TROPHY_COSMETICS_OCT07.md; Reports/TrophyCosmetics/README.md.

Practice — финальная проверка 7 октября 2026: отдельные входы уже были убраны; закрыт оставшийся запуск тренировки через старые R/B записи в «Моих маршрутах». Эти записи скрыты из меню, данные сохранены; общий menu guard не разрешает тренировочные определения. Обучающие фильмы и палец в текущем уровне остаются. Presentation compile 0 errors/7 existing warnings; native tests, visual recapture и build NOT RUN для этой точечной правки. Docs/NAVIGATION_RESUME_FIX.md.

Кнопки меню и компактная статистика — 7 октября 2026: обычные действия приведены к существующему синему enamel-спрайту со светлыми надписями; тёплое состояние сохранено у выбранных вкладок. Из результата убраны номер уровня и лишняя надпись о трофее; иконка, значение и подпись каждой метрики стоят на одной вертикальной оси. Все видимые разделители ·/• убраны из Presentation и EN/RU/UK translation values; новые ключи additive, старые ключи сохранены. Domain/Solver, баланс, цены, пакеты и сохранения не менялись. Portable compile 87 Presentation scripts: 0 errors, 7 прежних warnings. Один native Unity batch: 135 focused runtime checks PASS, четыре итоговых кадра проверены визуально (RU390×844; UK320×693 LargeText, simulated safe insets). Новых ассетов, player build, установки, полного suite и проверки на физическом устройстве не было. См. Docs/MENU_BUTTON_COPY_OCT07.md и Reports/MenuDetails/README.md.

Визуальное обучение и тематические надписи — 7 октября 2026: внедрены26 изолированных8-секундных демонстраций (обмен тапами/перетягиванием, спуск/дерево, Undo/Hint, дальний обмен/сумка, контакты, ограничения, входные трансформации, ветер,5предметов). Минимум текста, существующие фигуры/объёмные буквы/доски/эффекты/палец; после X палец ведёт к реальному безопасному действию, мягкий контур/кнопка, завершение по accepted outcome или явному успешному применению. Показ не меняет state/stock; Close не использует предмет. Future-board эффекты не показываются заранее; retry очищает незавершённое; checkpoint discovery сохраняется с новым attempt. Нельзя считать Undo/предметы освоенными простым спуском; нельзя считать Swap/FarSwap выполненными Drop. IDs20–25 append,0–19 сохранены; schema/правила/цены/packages не менялись, design note до изменения.5 cached SDF ролей для заголовков/информации/цифр/лент; Goldscore на enamel, контрастные значения на небе. NativeUnity19/19focusedEditMode PASS,36 isolated runtime checks PASS;234resolver fixtures Shapes/Words/Mixed×EN/RU/UK PASS; latest87Presentation portable compile0errors/7existingwarnings; updated19caseportablefixturePASS. Settled фильмы/guide/палец визуально проверены независимо, RU/UK320LargeText/safeInsets, overflow0. Статистика переснята после завершения церемонии. Финальная независимая проверка settled RU/UK статистики PASS: цифры читаемы, нет наложений/обрезания. Неподтверждённая дополнительная правка контура отменена; сохранён проверенный материал. Native retirement пропущенного жеста после последних регрессионных assertions не повторён (Mac заблокирован); source compile/portable19cases PASS. Временный runner удалён; последний native review остановил Play. No phone build/install/fullsuite/FPS/child comprehension claim. Reports/VisualLessons; Docs/VISUAL_LESSONS_OCT07.md. Cloud edit сохранён отдельно.

Карта без лишних счётчиков — 7 октября 2026: убраны задача/обратный отсчёт до нового мира, общий campaign total, подпись «Вы здесь»/0из3/стрелка. Один локализованный счётчик «Пройдено: x/y» и крупнее earned star/total; островам выделено63% safe-root высоты вместо47%, пропорции сохранены. Mode tabs48px, Mix/Микс/Mix; компактные нижние действия48px, Back/Bag одинаковые48px, gaps≥8px. Мика с мягко пульсирующим ореолом по alpha исходного спрайта; ReducedMotion постоянный, не перехватывает касания. Два ограниченных native Unity/CUA MCP прохода:9 scoped runtime checks PASS, RU390/UK320LargeText/simulated safeInsets, shader supported/TMP overflow0/hash unchanged. Первая находка — слабый ореол — исправлена вместе с равными Back/Bag; финальные кадры визуально осмотрены, independent scoped copy/layout PASS. Renderer pulse измерен; субъективная заметность движения/телефон не проверены. No build/install/full suite/FPS. Reports/MapDistill; Docs/MAP_DISTILL_OCT07.md. Временный runner удалён, Editor остановлен; правила/экономика/сейвы/unlocks сохранены. Новый только UI-key описан до изменения; старый Mixed key сохранён.

Звук, отклик и возвраты меню — 7 октября 2026: исправлены обрывы SFX из-за паузы головоломки в меню и очистки при переходах. Потоковые темы после сброса аудиовыхода явно загружаются и возобновляются с сохранённой позиции; раздельные Music/Sound, мягкие gain/crossfade сохранены. Кнопки получают один короткий звук; умеренные native haptics с cooldown120ms для тапов, обменов и приземлений. Back сохраняет страницу входа, категорию/предмет магазина и текущий поход; Трофеи не отправляют в магазин при выборе. Сокращены15 сообщений EN/RU/UK, значения импортированы; правило кратких actionable текстов сохранено в AGENTS. Portable compile Presentation82/Test7:0errors,7existingwarnings. Native Unity через CUA MCP:9/10 focused coroutine fixtures PASS в первом проходе; unloaded streaming case воспроизведён, исправлен и отдельно1/1 PASS. Все10 целевых сценариев прошли по совокупности проходов, это не полный TestRunner suite. Ненулевой listener PCM: menu music/world music/SFX отдельно;4 финальных кадра RU390/RU320LargeText/simulated safeInsets визуально осмотрены, independent copy review без новых регрессий, TMP overflow0. Reports/AudioNavigation, Docs/AUDIO_MENU_FEEDBACK.md. Временный runner удалён; Editor остановлен. Phone build/install, physical speaker listening/haptic feel, Android player/full suite/FPS NOT RUN. Правила, экономика, schema, IDs и Cloud edit сохранены.

Ночной мир, сюжет и возврат в игру — 7 октября 2026: выполнено. Добавлена Звёздная обсерватория после Большого маяка: 24 уровня C161–C184, пять островов, ночное небо/созвездия, четыре независимых парящих острова задника, тёплый ключевой и холодный заполняющий свет, собственная музыка и сова Астер. Старые 637 определений сохранены; JSON расширен двумя append-блоками без форматирования старых объектов. Всего 184 основных уровня и 549 языковых вариантов. Перед каждым из девяти миров — три кадра сюжетного комикса; 45 вступлений островов, эпизоды между уровнями, индивидуальные характеры жителей, EN/RU/UK и история отдельно по режимам. Continue после проигрыша повторяет текущий этап; игра/контрольная точка/финальная квитанция сохраняются, практика убрана из игроковых меню, побочные задания не заменяют активный поход; стрелки скрыты до открытия второго острова, закрытые миры недоступны. Native Unity через CUA MCP: 60 pure NUnit-method calls и 11 runtime coroutine fixtures PASS, семь экранов без TMP overflow, simulated safe insets/Large Text. Первичная ошибка инициализации новой ночной окраски исправлена; три фикстуры поправлены под реальные этажи/открытия/режим. Свежий визуальный reviewer: scoped SHIP, четыре находки устранены за две ограниченные серии; финально пересняты только заголовки комикса. Reports/NightJourney, Docs/NIGHT_NATIVE_VALIDATION.md. Промпты 7/7 ассетов встроены; временный runner удалён, Editor остановлен. Full suite/device/build/FPS/live ads/IAP/human enjoyment/subjective listening NOT RUN. Cloud edit сохранён.

Читаемость платформ и экрана проигрыша — 7 октября 2026: выполнено. Уроки entry-эффектов показываются только перед настоящим будущим приземлением; начальное зеркало не обещает повторного эффекта на приёмнике. Сгенерированы зеркало, ролики и ветровая платформа, по четыре состояния повреждения; одинаковое оформление в игре и демонстрации. Убраны мелкие стрелки и лишний мини-ряд; светлый счётчик встроен по центру доски. Мика отделена от ленты и цели, текст вписан в ткань ленты; Retry тематическая, Undo/Map одинаковой высоты и непрозрачны. Два ограниченных native Editor прохода через CUA MCP: 6 NUnit-методов вызваны через reflection + 48 runtime проверок PASS; настоящие Mirror/Rollers Drop, live gust/HP, EN390×844/RU320×693 LargeText/safe insets и возврат из настроек. Финальные кадры визуально осмотрены; Reports/PlatformClarity. Временный runner удалён; Editor остановлен. Full suite/device/build/FPS NOT RUN. Правила, solver, ID, сейвы и Cloud edit сохранены. Docs/PLATFORM_EFFECT_CLARITY.md.

Связь веса элементов с платформой — 7 октября 2026: выполнено. Тап и удержание снимают контактный вес; доска распрямляется и передаёт мягкий импульс подвеске, возврат снова нагружает дерево. Фигуры и буквенные плашки опираются нижней точкой на местный изгиб, наклоняются и немного скользят; подъём начинается из текущей позы без скачка. Управляемая физическая презентация, правила/solver не изменены. Native Editor через CUA MCP: 14 проверок PASS, Shapes/Words RU390×844/safe insets, реальный pointer drag=один обмен/ход, пауза/Reduced Motion. Три кадра и motion frames визуально осмотрены; Reports/LoadContact/contact-motion.gif. Временный runner удалён; Editor остановлен. Full suite/phone/build/FPS NOT RUN. Docs/LOAD_CONTACT.md; Cloud edit сохранён.

Праздничная статистика — 7 октября 2026: выполнено. Один прозрачный generated atlas: наградной медальон очков и три значка (шаги, отмена, секундомер). Золотые очки на бирюзовой эмали, короткие подписи, отдельные отступы, мягкое последовательное появление и блик; Reduced Motion уважён. Исправлена общая перекраска очков меню. Native Editor RU390×844 и UK320×693/LargeText/safe insets визуально осмотрены; TMP overflow0, ceremony завершена, hash головоломки неизменен. После native MCP timeout проверка запущена через CUA MCP в изолированном профиле; временный runner удалён, Editor остановлен. Полный suite/телефон/build/FPS NOT RUN. Docs/AssetPrompts/ResultMedals.md; Reports/ResultMedals. Правила, scoring, rewards и Cloud edit сохранены.

Живой порыв, фон и звёзды — 7 октября 2026: выполнено. Новые descent v5: с C02 порыв после обмена на верхней доске среднего этапа; с C04 −1 прочности, слабая доска может сломаться. Solver/preview/replay считают один и тот же переход и минимум заранее; последний этаж без scheduled gust, приёмник без ambient gust. Старые v1–4 сохранены. Первое знакомство LiveGust=19 останавливает игру, EN/RU/UK. Усилены предвестник/верёвки/платформы, воздух/дуга перестановки и существующий слом со щепками. Островки игрового фона заметнее парят, лодка в трёх мирах, крупнее параплан. Звёзды по очереди крупно входят и садятся в пять мест, с бликами и правильным слоем; ReducedMotion сохранён. На узкой карте зазор между островом и заданием. EditMode29/30: все новые gust/replay/proof PASS; один старый массовый тест кривой Descent timed out180s, НЕ считать пройденным. Native MCP15PASS + визуальные кадры390/320/safeInsets. No phone build/install/full-suite/performance/listening claims. Docs/LIVE_GUST_OCT07.md; Reports/LiveGust/.

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


Локальные напоминания — 7 октября 2026: добавлены default-off preference NotificationsEnabled, чистый календарный planner и local iOS/Android service; один неповторяющийся reminder около18:00 следующего местного дня, cancel foreground/disable, запрос OS только по включению. Settings получает отдельную вкладку Reminders; пять вкладок включая Cloud горизонтально прокручиваются с min112×52pt, EN/RU/UK9 новых keys append, старые IDs сохранены. Editor/build hook выключает iOS startup authorization/APNs/remote capability и Android exact scheduling, сохраняет pending при restart. Design note до schema/package/ID changes: Docs/LOCAL_REMINDERS.md. Portable current C# compile PASS,11/11 actual NUnit cases PASS; static locale completeness/uniqueIDs PASS. Root owns package references/wiring и per-device preservation при cloud restore. Native permission/delivery/build/UI/full suite NOT RUN этой подзадачей; Reports/LocalReminders/validation.txt.

## Cloud account and local reminders — 7 October 2026

- Implemented Authentication + Cloud Code + Protected Cloud Save account architecture, explicit project/environment, bounded device checkpoints, monotonic mode progress, coherent resumable attempts, server-owned economic baselines and idempotent store grants. Local saves remain atomic; a durable outbox retries offline without duplicating purchases. Account switch intent survives restart; newer server sequence never reapplies an older economic delta. Device consent/preferences stay local. Native IAP awaits the same service/environment initializer and confirms only after a durable verified server grant.
- SDK packages installed through Unity Package Manager Client API; notification settings disable remote push/startup permission/exact alarm. Settings gains Cloud and Reminders tabs. One opt-in local reminder for next evening, cancelled on foreground/disable. Design notes: Docs/CLOUD_PLAYER_STATE_OCT07.md, Docs/LOCAL_REMINDERS.md.
- PASS: 28 pure reconciliation/journal/reply NUnit tests; 11 reminder tests; 31 Cloud Code boundary assertions with actual SDK/fake transport; 11 receipt verifier tests. Actual .NET 9 Release module compile/publish passed. Scope limits: fake transport is not live concurrency validation.
- NOT ACTIVATED: dashboard and CLI authorization could not be confirmed; local linked project ID/name must be compared with the user's Cascade Project. Client flags remain off. First account AND transaction-owner creation rely on an undocumented synthetic absent-key guard, deliberately fail closed, and require live validation or a documented atomic provisioning store before enabling. Store credentials/catalog, cryptographic player-binding, refund ingestion and AdMob SSV remain production integrations. Complete activation checklist: Docs/UNITY_CLOUD_ACTIVATION.md. No phone build/install. Native Settings frames reviewed at390×844 and320×568: selected tab scrolls into view after resize, cloud status no longer overlaps tabs, controls/text stay inside the workshop window. Current native UI import/capture passed; Android/iOS OS delivery and live network behavior remain unverified.

Final cloud pass: native iOS/Android Presentation conditional source compile0errors each; no player build. Recovered reply marker+conflict saved atomically with profile; server strips local markers. Existing acknowledged conflict now reopens with a visible retry status. Non-preview rewarded offers are hidden while cloud is on until AdMob SSV, avoiding legitimate rewards rejected by cloud.

## Story dialogue and comic framing — 7 October 2026

- Replaced the tall map/checkpoint dialogue panel with a compact, textured paper strip: portrait on the left, speaker and short line together, a 44-point Next target, quiet dismissal, and line progress. The same paper treatment is used by the world comics; their illustrations keep the source aspect ratio and now sit inside four wooden rails. The opening entry uses that illustrated world-comic dialogue flow too.
- Source geometry and whitespace checks passed. Superseded by the native Unity completion below: paper/frame and editor safe-area layouts are now reviewed. Physical device review and player build remain unrun.

## Island pacing and first-use lessons — 7 October 2026

- Expanded the journey to 540 primary levels: 45 islands with 12 stops each. The 356 new puzzles rise from short tutorial routes toward a stable 8–12-action challenge. Existing C01–C184 definitions, IDs and hashes remain intact; the route inserts practice after each island’s original lessons. The Words route has one documented 11-stop exception at C26 because that target has no natural matching four-letter binding in the current three dictionaries. See `Docs/CAMPAIGN_PACING_OCT07.md`.
- Added journey ordering/teaching metadata, a scrolling island replay list, and old-save progression across inserted levels without replay rewards. First-use films and the guided hand now wait for the actual mechanic action; leaving a floor does not mark an unseen effect learned. The comic/dialogue entry uses the same compact visual language.
- Validation run: all 356 new levels matched their exact C# solver minima and prepared valid three-stage runs; import returned 540 primary / 1,617 localized variants; map ordering, 45×12 layout, and fresh/old progress passed a scoped .NET check; six game assemblies and the EditMode test assembly compiled with zero errors. Unity Play Mode visuals and phone build were not run because the Editor did not answer the view request and no device build was requested.

## Trophy parcels and Mika looks — 7 October 2026

- Expanded the cosmetic catalog from eight to twelve trophies with four two-pose Mika outfits. The new looks cost 960 virtual coins and unlock for purchase at 60/180/300/420 unique clears. The first twelve parcels retain their published rewards; later deterministic parcels arrive every twelve unique clears, with rare looks at 84/192/300/408 and 20 coins for other or duplicate prizes. No consumable tool is awarded by a parcel until cloud stock grants can be verified. See `Docs/TROPHY_PARCELS_OCT07.md`.
- Owned looks stay colorful, locked looks are gray, and the collection has a first-use hand cue and Mika tap response. Four focused reward/shop checks passed against the compiled game assemblies. Unity Play Mode, shader compilation, and device compositing were not run; the generated outfits' faint red-edge pixels have alpha at most 4/255 and import with alpha transparency.

## Settings, first-use films, audio and haptics — 7 October 2026

- Replaced the clipped Settings page strip with five equal icon tabs inside the workshop window and a masked, vertically scrolling content area. Live Unity Game View review confirmed the 390×844 Sound and Cloud pages: tabs, toggles, status and Back remain within the wood frame; music, sound effects and vibration have independent controls.
- Added an opaque enamel reading field to first-use popups so the underlying puzzle and HUD cannot show through. Captured all 26 isolated demonstration films at three moments each, plus both tap and drag swap cycles, from Unity Play Mode; reviewed the resulting contact sheets. The live follow-up hand was checked over the actual piece after closing a film. Kept the eight-second film loop and eased hand movement. Strengthened wind demonstrations with air trails (no arrow tips), visible board/token sway, and a smaller windsock above the landing row. After unlocking the Mac, Unity recompiled with zero C# errors and a fresh 81-frame capture completed; the three wind lessons were visually rechecked and the windsock no longer overlaps the landing diamond.
- The real puzzle resolver supplies the film outcomes; presentation and finger guidance do not execute gameplay commands. Contact sheets and status are in `Reports/LessonFilms-Oct07`.
- Wind SFX follows the same rig pressure as visible sway, with gain, pitch and low-pass changing together; music keeps loop positions during mute/pause/device reconfiguration, with a distinct menu score and separate score for each of nine worlds including Star Observatory. The source composition uses multiple voices and an A/B phrase. All referenced audio clips resolved and decoded as nonzero PCM; this is an asset check, not a speaker listening test.
- Added differentiated native iOS/Android pulses for breeze, gust, crack/break, bomb, landing, stars and parcel. The first pulse is aligned with the visible event; short button ticks do not suppress an ensuing impact. Unity focused PlayMode `Reports/AudioHaptic-Oct07/playmode.xml`: 6/6 passed. Physical iPhone/Android haptic feel, speaker balance, player build and complete frame-by-frame human motion review were not run. The Editor printed a separate `GameViewLanguageMenu`/`EditorStyles.popup` exception from Unity Localization on domain reload; the final film capture and gameplay then ran without a project exception.

## World lighting and ambient wildlife — 7 October 2026

- Added nine distinct single-source sun/moon lighting profiles with ambient sky fill; the Observatory uses cool lunar light from the authored moon side and cool mist. Each world receives its own ecology/flight visits using eight original species, three wing poses and two reusable near/far meshes. Actual source rectangles and torso pivots preserve generated wings; no mipmaps/bilinear filtering prevent neighboring atlas bleed. Source PNGs are unchanged.
- Added gameplay glints and rare tapered meteors; the night map shares timing behind its island art. Night crickets use an original sparse phrase, long quiet intervals, cue ducking and explicit mute/pause/focus/disable handling. All scenery reads the shared presentation phase, with reduced-motion/quality controls and no gameplay mutation. Design/provenance: Docs/WORLD_ATMOSPHERE_OCT07.md.
- Current Unity-engine Presentation source compile PASS, zero C# errors. After unlocking/restarting the Editor, native Play Mode captured all nine worlds at two poses plus night meteor/reduced mode and night map390×844/320×693. Reviewed daytime contact sheet and native night/map/skiff frames; corrected the earlier skiff/piece overlap and first-meteor property-block exception. Runtime checks passed for meteor, reduced motion, effects off, same-phase stability, one directional source and unchanged puzzle hash. Scoped native PlayMode cricket lifecycle test1/1 PASS (quiet interval, no overlapping phrases, pause/disable stop), Reports/WorldAtmosphere-Oct07/playmode.xml. No phone build/install, speaker listening or physical-device audio/performance review.

## Expanded Mika wardrobe and chest economy — 7 October 2026

- Supersedes the earlier twelve-trophy catalog note: sixteen cosmetics, with four new original two-pose Mika looks (Explorer hat, Frog suit, Star knight, Fox mask). Purchase is only with virtual coins:480/960/1200/720 respectively; no stars or new real-money SKU. Removed the purchase-only clear gates on the earlier four premium looks while preserving old earned ownership, IDs, prices and receipt checksums. Contract/provenance/math: Docs/TROPHY_EXPANSION_OCT07.md.
- Rare later chests extend the existing recipe with cosmetics,20 coins and one capped tool. New look finds start at132/240/348/456 unique clears; later cosmetics average one per54 unique clears. Replays never farm first-clear chest rewards. Duplicate cosmetics/full tool stock convert to20 coins. Inventory and receipt claim save atomically, rollback on failure, and reconciliation does not double-grant across devices. Published legacy receipts remain immutable. New recipe/tool receipts require the matching updated client and Cloud Code module; no live deployment performed.
- Earned virtual coins, tools, chest claims, purchases and equipment now share one durable ledger even with native test ads/IAP enabled. Session test monetization does not replace or spend durable inventory. Cloud-applied profiles rebind that ledger. Owned shelf art is colorful, closed art gray; color try-on does not grant ownership. Four costumes use shared equipment throughout Mika sprite views and portraits, with themed leaf/rain/star/petal ambient loops and a small eased tap response. The first-use finger points at the shelf/hero and selection alone does not dismiss it.
- PASS:85 scoped pure-C# checks with standalone Unity-compiler/Mono NUnit assertion runner;43 linked Cloud Code SDK checks using fake transport and Release module compile. Scoped Presentation/Editor source compile and fresh native Unity import passed. Fifteen native Play Mode screenshots were reviewed at390×844 and320×693, including UK Large Text/long hat title, all new looks, map/dialogue, fingertip poses, tap rise/rest and tool chest. Fixed missing translation, page touch size and disabled label contrast; checked targets >=44×44px and inside capture. Reports/TrophyWardrobe-Oct07.
- NOT RUN: live Cloud Save/deployment, native store/device purchases, physical-phone build/install, device performance/speaker/haptic feel or exhaustive frame-by-frame motion/human enjoyment. Existing Unity Cloud activation limits still apply. No phone build requested.

## Completed world comics and story dialogue — 7 October 2026

- Every one of nine worlds now has four illustrated pages and two brief spoken lines per page:36 pages/72 comic lines. Expanded character motives, histories and dialogue beats across island openings, payoffs, visits, occasional asides and checkpoints; repaired premature water/wheel claims and five RU/UK gender agreement keys.117 new localized keys appended without changing published IDs or story receipt formulas. Arc/compatibility: Docs/STORY_ARC_OCT07.md and Docs/STORY_PRESENTATION_OCT07.md.
- All story flows use the compact paper strip with portrait left, short spoken text and clear Next action. Added an original transparent paper sprite with preserved curls, corrected actual wood-rail source bounds and kept illustration proportions. Reading reserves the name/control gaps; changing speakers keeps the picture still. Fixed old comic/portrait identity conflicts for Pip the frog and Rue the elderly fox using new sibling atlases. Original images/.meta and actual player saves are untouched.
- PASS: native Unity EditMode4/4 and PlayMode4/4. Flow checks cover all36 illustration bindings, stable picture placement, preserved row/hash/middle stage, mode-scoped receipts, reload and island navigation. Forty-eight isolated native Unity captures operated through CUA MCP were reviewed as contact sheets and full-size frames:all36 comic pages, nine resident dialogues, small RU/UK Large Text comics and one UK Large Text dialogue. All226 captured button targets are in bounds and >=44×44px. Reports/StoryComic-Oct07. Final scoped whitespace check passed; fresh native source import compiled without project C# errors.
- NOT RUN: phone build/install, physical-device presentation/performance, voice acting, external language review or exhaustive frame-by-frame human motion/enjoyment evaluation. Unity Localization's separate EditorStyles.popup exception on editor repaint was observed; the project capture completed48 frames and targeted tests passed. Editor Play Mode stopped after review.

Follow-up — 8 October: lower map actions now hide while a conversation is open and restore on closing, removing the exposed edge beneath the paper strip. Added focused restore assertions to the existing flow test. Native recompilation/PlayMode/visual confirmation of this last small change is pending: Mac locked again immediately before the run. Prior48-frame review and8 passed tests remain valid for the preceding version. Unlock requested; no phone build.


## Mandatory guided action — 8 October 2026

- First-use practice now accepts only the exact command/control displayed by the
  hand. The state-bound Application gate covers selection, tap/drag/precision,
  descent, undo, hint request/application, retry, bag and inventory use before
  mutation. Passive wind remains observable. Pause/resume retain practice;
  successful teaching unlocks normal play until the next unseen mechanic.
- Replaced ordinary-solution fallback with a bounded lesson-aware route to the
  actual effect and a solvable continuation. Preparatory moves remain guided;
  current/next-board contacts cannot be skipped. Entry finger uses the real
  proved action. Unavailable practice defers without mastery. Save failure
  releases a successfully consumed action and uses existing recovery.
- PASS:55 scoped pure-source checks (14 incumbent interaction,10 strict gate,
  12 route,19 lesson completion), current Application/Presentation compilation
  and current PlayMode test-source compilation. Seven incumbent warnings.
  Reports/GuidedLessons-Oct08; contract Docs/GUIDED_LESSON_INPUT_OCT08.md.
- NOT RUN: native Unity TestRunner, new native visual captures, device build/
  install or full suite. CUA MCP confirmed the Mac locked; unlock requested.
  Three isolated native flow cases and four screenshots are prepared. Previous
  comic footer native follow-up remains pending as well; its source compiles.


## Settings column and contrast — 8 October 2026

- Removed horizontal category strip. Five equal-width teal category buttons now
  form one column with square icons, aligned ivory captions and clear chevrons.
  Each opens its own settings page; one Back returns to the category list, and
  its Back returns to the original caller. Existing preferences/keys/assets stay.
- Content viewport has72px top/80px bottom reserves,52px rows and12px gaps;
  vertical scroll handles short screens and wrapping status notes. Switch states
  use dark teal/green. Settings heading keeps dark teal after the night world.
- PASS: current Presentation source compile (zero errors, seven incumbent
  warnings); updated settings navigation test-source compile. No native runtime
  test or visual result claimed: CUA MCP confirmed Mac locked. Native390/320px
  EN/RU/UK review pending unlock. Reports/SettingsColumn-Oct08. No phone build.


## Screen cosmetics and extra world life — 8 October 2026

- Added12 permanent coin-only trophies (8 atmosphere animations,4 game frames);
  all28 trophies are split into Mika/Atmosphere/Frames catalogue categories.
  Scene and frame slots equip/remove independently of Mika, with durable
  save rollback, ownership checks and three-way cloud reconciliation.
- Generated36 wildlife poses (9 species ×4) and4 hollow themed rims. Shared
  measured torso pivots/alpha rectangles keep previews and scene poses aligned.
  Reusable capped visitors vary by world/island/stage, with curved depth routes;
  the equipped scenery adds its own family. Frame art changes the actual receiver
  rim and safe edge ornaments, preserving puzzle geometry and state.
- PASS:93 pure-source checks,50 linked Cloud Code fake-SDK checks, current
  Application/Presentation compile (0 errors;7 incumbent Presentation warnings),
  all4 focused native PlayMode cases after final layout adjustments.
- Unity MCP recovered; native review covers EN/RU/UK catalogue at390/320px and
  all9 world visitor routes in paired motion poses. Short Scene labels, preserved
  chip type-size limits, larger frame previews and small outer-corner accents
  resolve the observed narrow-screen issues.31 new EN/RU/UK keys imported into
  native String Tables. No phone build, package install or live cloud deployment.
  Redeploy linked module before cloud
  activation of new IDs. Evidence: Reports/ScreenCosmetics-Oct08; design note:
  Docs/SCREEN_COSMETICS_OCT08.md.

## Night world final recheck — 8 October 2026

- Fresh Unity MCP review: one blue directional source,16 active stars, paired
  meteor poses, pause/resume phase preservation, Reduced Motion and Effects Off.
  Four native night captures inspected at390/320px; puzzle hash preserved.
  Cricket lifecycle PlayMode1/1 PASS; current Console had no runtime errors.
- Corrected the observed320px Ukrainian Undo crowding: action copy Скасувати,
  8px footer-label insets and12px auto-size floor. Native tables imported and
  final narrow capture inspected with no TMP overflow. Evidence:
  Reports/NightRecheck-Oct08. No rule/catalog/package changes or phone build.
- Read-only animation review recommends distinct wing poses, species-specific
  flap/glide/hover timing and layered Mika facial/accessory motion. This proposed
  art/rig expansion is not implemented. Unity Play Mode stopped after review.

## Reward copy and interface typography — 8 October 2026

- Removed duplicate ownership confirmations from opened tool/cosmetic rewards,
  duplicate coin amounts and repeated shop acquisition notices. Incomplete
  trophy progress and save failure messages remain. Reward ribbon copy is short
  and stays within its illustrated paper.
- Added Alegreya ExtraBold display headings, Nunito SemiBold descriptions and
  Nunito ExtraBold controls/numbers with shared teal/gold/ivory SDF materials.
  Original fonts/assets remain; no catalogue, rules, price or package changes.
  Optional display-font reference is documented in Docs/TYPOGRAPHY_OCT08.md.
- Shortened tool explanations in EN/RU/UK while retaining their limits;16 Mika
  trophy captions now describe ambience without repeating the item name. Gave
  descriptions more room, kept consequence text on one line and corrected the
  narrow trophy page-counter size and acquisition leading.
- PASS: native Unity compilation and static font coverage for all localized UI
  characters; each new font uses one static atlas.20 native captures cover menus,
  rewards/statistics and all6 tool/guide variants, including320px Ukrainian Large
  Text. Final targeted checks report no TMP overflow; incomplete prizes keep one
  parts counter. Final Console has0 errors/warnings. These temporary visual
  fixtures do not test acquisition/economy; full gameplay suite and phone build
  were not run. Original player saves were isolated. Play stopped and temporary
  profile override cleared. Evidence: Reports/Typography-Oct08.

## Compact trophy details and resource counters — 8 October 2026

- Trophy details show the name and one short visual-benefit line. Removed the
  repeated ownership and cosmetic-scope paragraphs; incomplete parcel parts
  remain a small preview badge. Purchase/equip behaviour and prices unchanged.
- Catalogue headers share a centred title and Back button aligned with the
  wallet. Coin/star/trophy counts and virtual prices use compact icon × amount
  groups. Applied readable ivory lettering on sky/enamel and kept dark teal
  reward counts on pale result backgrounds. Metric values remain unchanged.
- Hidden the rewarded-video control while ads are unavailable; when ready it
  reads Video +40. Existing ad lifecycle/reward logic unchanged. Fixed the
  shared typography pass overwriting light labels on blue coin cards; shortened
  the existing help-limit explanation without changing those limits.
- PASS: native Unity compile, final Console0 errors/warnings, native MCP visual
  inspection of11 captures covering shop/collection/tools, map/island/results
  and coin packs, including320px Ukrainian Large Text. Final four captures
  have0 TMP overflow; one trophy description line,0 duplicate status labels,
  Back/wallet vertical difference0, unavailable video control0 / ready1.
  Early coin-card overflow findings in runtime.txt are resolved in
  final-runtime.txt. Isolated player profile; per-capture gameplay hash guard.
  Purchases/ad grants, full gameplay suite and phone build were not run.
  Play stopped and profile override cleared. Evidence: Reports/CompactShop-Oct08.

## Island character dialogue — 8 October 2026

- Added45 unique two-line conversations, one around the midpoint of each island
  across all9 worlds.90 new EN/RU/UK keys deepen resident worries, humour,
  relationships and Mika's growth without premature repair/homecoming payoffs.
  Retained the existing openings, payoffs, comics and nine later-world asides.
- Reused the LevelAside entry path and stable7000+node receipt formula; viewed
  or skipped scenes remain saved independently per mode and do not repeat on
  replay. No new save schema, puzzle/economy rule, price, catalogue or package.
- Paper dialogues now recalculate height when localized copy arrives and
  reserve fixed header/footer space,44px controls and at least8px text/control
  clearance. Shared fonts confirmed in native Unity; no separate font system.
- PASS:5 focused native EditMode checks and2 PlayMode cases, native compilation,
  final Console0 errors/warnings. Native270-line geometry review at320px Large
  Text has0 text/bounds/gap violations, minimum gap8.00px and unchanged puzzle
  hash.12 native captures cover all9 resident portraits, Ukrainian Mika and
  standard390px English. All45 new anchors exist in each locale's Words route.
  Initial legacy test assumptions were corrected to fit current speaker metadata
  and island-list navigation; production behavior was not changed for them.
- Reduced Motion captures and seeded temporary world availability are visual
  fixtures. Full suite, new motion timing, physical devices, live cloud and phone
  build were not run. Play stopped and profile override cleared. Design/evidence:
  Docs/ISLAND_DIALOGUE_OCT08.md; Reports/IslandDialogue-Oct08.

## Ten more trophies and economy balance — 8 October 2026

- Added6 generated two-pose Mika looks: mushroom cap, bee costume, winter hood,
  sky pirate, cloud jester and star mage. Added4 animated screen styles: ribbon
  kites, paper cranes, dream bubbles and aurora.38 total trophies; permanent
  coin ownership, independent slots, shared portraits and grayscale locked art.
- New prices600–1440 coins; ten additions cost9720. Existing28 prices stay.
  Income is40 per unique published clear plus100 per ten clears throughout
  the540-node campaign (27000 before chests). This removes the route-order
  mismatch in client/cloud bonuses without reducing prior earned value.
- Future bonus chests use recipe2; original0/1 receipts retain their prizes.
  All ten new finds fit within the campaign at60/96/168/204/276/312/384/420/
  492/528 clears. Existing stock5, two tools per stage and assisted rank cap2
  stay. Duplicate cosmetics/full stock convert to20 coins; replay adds no chest.
- Raised coin-pack initiation threshold to the1440 catalog maximum; verified
  pending purchases still deliver. New cosmetic paid grants are rejected.
  Earned scenery ownership/equipment and duplicate two-device claims reconcile
  atomically. No new real-money SKU, package or profile schema.
- Added20 EN/RU/UK keys with short benefits. Four-item scenery shelves have a
  wider preview. Native narrow review found title clipping; corrected label
  height/width while retaining padding and shared fonts. Snow accents match
  winter clothing. Scene movement uses bounded, nonblocking existing pools.
- PASS:109 standalone pure C# cases;59 linked Cloud Code SDK cases with fake
  transport;77 native Unity EditMode cases and2 final PlayMode cases. Native
  MCP visual inspection covers18 captures and60 narrow title/benefit checks
  at320px Large Text across3locales. Actual virtual purchases/equips persist
  in the isolated availability fixture with exact9720 spending and unchanged
  puzzle hash. Scenery movement, reduced motion and effects-off checked.
- Final native runtime Console0 errors/warnings. Compilation retains existing
  deprecated advertising API warnings. Full suite, phone build, physical device,
  live commerce/cloud deployment and player enjoyment/retention were not tested.
  Temporary profiles isolate player/cloud data; Play stopped and override cleared.
  Redeploy the linked cloud module before activating new catalog/recipe validation.
  Design/evidence: Docs/TROPHY_ECONOMY_OCT08.md; Reports/TrophyEconomy-Oct08.

## 2026-10-08 — spacing, reading surfaces and island story
- Applied Impeccable layout review to existing portrait UI. Shared 12-unit menu-row gaps, smaller normal captions, larger accessibility captions, paper-backed dark headings/counters and grouped trophy descriptions; removed heavy outlines. Imported the actual McLaren UI font from local ShapeMania; existing Cyrillic fallback retained. No economy or puzzle rule changes.
- Map separates home/bag/stars, reserved world-arrow/title row, and mode tabs. Island view yields space to a compact bottom dialogue. Open-island tap opens its level list; first-use fingertip cue follows Mika and remembers an actual island tap. Comic heading centered.
- Authored 90 additional localized lines: 45 midpoint conversations now four pages. Arrivals and island transitions now four pages; a fully read transition remembers the arrival it already introduced.
- Validation: 5 native EditMode StoryConversationTests passed; 14 representative native Editor captures plus targeted shop redraw captures in Reports/Spacing-Oct08, including 320×693 large-text and a map with both world arrows. Impeccable mechanical layout scan returned no findings (does not understand all Unity layout behavior). Runtime Console had no errors. Final grouped-paper/padding refinement rendered at both phone sizes; dark-world bare-copy contrast preserved.
- Scope limit: physical iPhone/Android, every individual settings subpage and gameplay/result variants were not newly rendered in this pass. No phone build/install. Editor stopped and isolated review-profile override cleared after review.

## 2026-10-08 — trophy layout and shared navigation correction
- Removed automatic papyrus behind headings/counters and the second trophy-description line; dialogue paper remains. Enlarged Mika's selected-item preview, lifted shelf art above planks, aligned the wallet on the right, and lowered both shop actions with a 12-unit gap.
- Button captions now share fixed 14-unit normal / 19-unit Large Text sizing across menu, gameplay, result, dialogue and comic controls, including preference refreshes. No per-frame caption shrinking. Shortened the Ukrainian tool-tab caption to prevent a split word at 320px.
- Generated two ivory/gold arrow sprites. Both shared navigation renderers use them: heavier upper arrows and thinner inline/pager arrows. Three horizontal slices preserve readable tips while shortening the shaft; existing button actions and hit targets are retained.
- Native MCP compilation and visual review: final shop at 390×844 and 320×693, narrow Ukrainian shop, settings, comic, gameplay and Large Text home. Earlier representative map/collection/dialogue/English captures also remain in Reports/TrophyRefine-Oct08. Final runtime Console: no errors. Changed-source whitespace check passed.
- No economy, gameplay rules, package, product ID or profile-schema changes. No automated suite, phone build or physical-device visual review in this UI pass. Isolated review profile only; Play stopped and profile override cleared after inspection. Design: Docs/UI_SPACING_OCT08.md.

## 2026-10-08 — wider shared grid and complete screen gallery
- Reduced workshop side inset from 8% to 4% and shared content/action inset to 5.5%. Menus, settings, catalogue tabs/shelves, island lists, map navigation, dialogue and gameplay actions use the wider grid. Registered columns recalculate on viewport resize; existing 12-unit control / 8-unit item gaps, caption sizes and touch heights are preserved.
- Applied Impeccable context/layout guidance and inspected native Unity contact sheets. The bounded visual pass exposed dark bare headings/counters on night surfaces; corrected their contrast with the existing light foreground.
- PASS: native Unity compilation and final runtime Console with zero errors; 45 final native captures with zero capture failures; Impeccable mechanical layout scan with no findings. Main screens, every settings page, day/night map, all store categories, gameplay modes, pause/checkpoint/win/loss, lootbox, comics/dialogue, tutorial/help, journal/records and recovery are in Reports/AllScreens-Oct08. Includes narrow Ukrainian shop and narrow Large Text menu.
- Added a self-contained enlargeable gallery and three numbered overview sheets. Screenshot fixtures use isolated progress/rewards, a valid solved route and actual result states; they do not touch player/cloud data. Native Editor portrait captures use simulated safe insets, muted audio and Reduced Motion.
- No economy, rules, packages, schemas or IDs changed. Automated gameplay suite, physical-device/Simulator, animation/audio quality and phone builds were not run for this width-only pass. Editor Play stopped and temporary profile override cleared. Design: Docs/WIDE_UI_GRID_OCT08.md.

## 2026-10-08 — layered bird and insect motion
- Reworked all 17 existing ambient bird/insect species with generated separate body, wing and tail layers. Mika and other characters are unchanged. Original assets and metadata are retained; no package, rules, economy, save schema or ID changes.
- Eight smoothly interpolated wing phases, short bird flap bursts followed by gliding, anticipated banking, delayed tails and flexing wingtips. Butterfly flutter is irregular; dragonflies pause then dart; night moths orbit decorative light. Calibrated painted wing angles make the glide visibly spread rather than a frozen upstroke.
- Tight silhouette meshes remove neighboring atlas fragments. Paired-wing insects avoid redundant layers. Existing world/island/cosmetic species assignments, quiet intervals, shared ambient pause clock, camera rebinding and Reduced Motion / quality gates remain. Five creatures maximum; two shared atlases/materials and cached deformation buffers.
- PASS: 4 native EditMode motion cases and 2 final native PlayMode lifecycle/placement cases. Final Unity MCP review recorded 9 world contexts, a 17-species contact sheet and an 8-second 24fps enlarged motion clip; inspected final assembly, glide pose and normal-speed playback. Runtime Console: zero errors; gameplay state hash unchanged.
- Evidence: Reports/WildlifeMotion-Oct08/preview.html, motion.mp4, world-0.png through world-8.png, species-contact.png and test XMLs. Design/art note: Docs/WILDLIFE_MOTION_OCT08.md. Physical-device performance, audio/haptics and phone builds were not tested in this visual-only pass. Play stopped and the isolated review-profile override cleared.

## 2026-10-08 — costume paper labels and bird perching
- Selected trophy/costume names use the existing papyrus sprite, dark lettering, internal padding and one line. Page headings and wallet remain unframed. Native bee costume captures reviewed at 390×844 and 320×693.
- One existing wildlife slot approaches, folds its wings, rests and takes off from empty visible platforms (5s idle) or the receiver rim, shapes and clear working-board edges (11s idle). Uses actual visual support points and inclination; no additional actor, gameplay weight, collider, input target or puzzle mutation.
- Touch/keyboard input, accepted moves, wind and disappearing/fractured supports cause departure. Landing is gated during tutorials, transitions, pause, hidden gameplay and Reduced Motion / Effects Off. Post-departure cooldown prevents immediate repeat visits. Board spacing was corrected using measured mesh gaps during native review.
- PASS: 2 native EditMode timing/folding cases; native four-support approach/rest/foot-contact/input-departure/cooldown assertions; occupied-support idle gate, gust and support-loss checks. Final runtime Console: zero errors; puzzle state hash unchanged. Visually inspected native rest/board/paper captures and normal-speed landing/takeoff recording. Evidence: Reports/WildlifePerch-Oct08. Design: Docs/WILDLIFE_MOTION_OCT08.md.
- Full gameplay suite, every costume/localization, physical iPhone/Android and device performance were not newly tested. No rules, prices, packages, storage schemas or product IDs changed. Play stopped and isolated review-profile override cleared.

## 2026-10-08 — clarity across all native UI screens
- Applied frontend-design, Impeccable distill/critique and Product Design audit to the existing native game. Kept the wood/gold/teal/sky art and existing McLaren/Nunito stack; removed the global 20-point menu-text floor and assigned fixed heading/name/action/compact-tab/supporting roles.
- Main menu has Play, Shop and Settings; ad settings remain reachable inside Settings. Reading uses existing papyrus with dark ink and padding. Repeated status/marketing copy removed; text describes only the current object or next action.
- Catalogue shelves open large item/cosmetic details with one short benefit and a grouped coin price. Back restores the same shelf/category; My things restores the same item detail. Inventory and collection stay separate. No prices, rules, packages, persisted schemas or product IDs changed.
- Journal, workshop, requests and records use labelled icon facts instead of overlapping prose. Records no longer overlap replay controls. Narrative dialogue/comic lines are measured and split at the actual Canvas width without deleting words, speakers or receipt IDs. Comic art retains its aspect and wooden frame. Large Text tabs, dialogue action and settings clipping corrected.
- PASS: 53 fresh native Unity captures with isolated QA progress, portrait safe-area insets, 390×844 / 320×693, EN/RU/UK and Large Text examples; all images inspected. Two remaining narrow-layout defects were corrected with targeted recaptures, without repeating the full gallery. Navigation flow passed; puzzle hash and coin balance unchanged. Two focused native EditMode pagination tests passed. Final Console: zero errors.
- PASS: catalogue integration with first-use tool teaching; native finger target moves from required shelf item to Use; its separate foreground canvas prevents menu occlusion and contact follows the actual rectangle centre. Other tool selections blocked; puzzle hash and coins unchanged. Two extra native captures inspected; 55 images in the gallery.
- Evidence: Reports/ClarityFinal-Oct08/gallery.html, audit.md, screens.json, flow.txt, responsive-fixes.txt, tool-coach.txt and editmode.xml. Design note: Docs/UI_CLARITY_OCT08.md. Play stopped, background running disabled and isolated profile override cleared. Phone builds/devices, audio/haptics, VoiceOver and real child comprehension were not newly tested.

## 2026-10-08 — source-based lighting in every world
- Replaced flat ambient fill with sky/ground irradiance and cached HDR sky reflections. Nine sun/moon profiles and a separate warm workshop context share the existing single directional light. Passing cloud changes direct light and reflected intensity using the paused presentation clock; Reduced Motion freezes optional changes.
- Rounded runtime copies of toy mesh front caps, retaining original silhouettes, UVs, assets and metadata. Existing URP PBR/clear coat, wood and rope surfaces respond to the real key and geometric shadows. Lifted contact shadows follow light direction, soften and fade with height.
- Illustrated islands, clouds, foliage, wildlife, map and cosmetic receiver use the same source through bounded 2.5D relief. Sky/stars and painted warm windows retain radiance. Removed the duplicate moon in the map shader. UI remains readable. No package, pipeline asset, price, rule, save schema or ID changes.
- PASS: 5 native EditMode lighting/geometry/lifecycle cases; Unity MCP compilation; 17 final native image contexts covering nine gameplay worlds, direction/lift/cloud diagnostics, mobile render quality, map/result/workshop/shop. Inspected native contact sheets and full-size frames; final Console zero errors; one light in the entire active scene; puzzle hash unchanged. Result image uses a completed presentation pose after portrait reflow, not timing validation.
- Evidence: Reports/WorldLight-Oct08/gallery.html, overview-1.jpg, overview-2.jpg, validation.txt and editmode.xml. Design: Docs/WORLD_LIGHT_OCT08.md. Play stopped, isolated profile override cleared, temporary HDR cubes remaining: zero. Illustrated buildings remain 2.5D rather than reconstructed/occlusion-traced geometry; phone build/device performance and full gameplay suite were not newly tested.

## 2026-10-08 — fitted platform life, visual teaching and varied ambient life
- Tightened the runtime plaque crop without modifying the atlas; preserved aspect ratio and filled the timber-face height at its moving centre. Enlarged/repositioned the number in the paper area. The demonstration uses the same fitted plaque.
- Removed legacy delayed top coaching text and its dismiss button. Existing demonstration/finger/restricted real-action teaching remains; learnt swaps do not replay ordinary finger coaching. Blocking preparation failure text and the brief chapter announcement remain distinct.
- Expanded the pool from five to seven cached rigs: two bird species and multiple insects per world, with existing stage/cosmetic visitors. Private presentation session salts vary visit timing, routes and landing choices; no gameplay RNG or state mutation. Random visible wood positions and shapes, avoided nearby repeated supports, varied resting/cooldown and approach direction. One bird perches at a time; occupied surfaces wait 11 idle seconds and input/wind/support loss causes departure.
- Flights use safe gaps between actual rendered pieces/platforms with hanging-device clearance. Larger near flights stay inside these gaps; map/low quality retain their bounds, and Reduced Motion / Effects Off suppress optional wildlife.
- PASS: six native EditMode motion cases and five distinct PlayMode lifecycle/placement/guided-action cases across focused runs. Corrected 20 fps test-sampling aliasing of a 4 Hz wing beat with 60 fps sampling over the same 60-second window; initial report retained, targeted rerun passed. Nine world configurations exercised. Eight native captures and six-second flight preview inspected; final runtime Console zero errors; puzzle hash unchanged.
- Evidence: Reports/LivingGameplay-Oct08/preview.html, validation.txt and focused XML reports; design: Docs/LIVING_GAMEPLAY_OCT08.md. No prices, rules, packages, IDs or persisted schemas changed. No phone build/device performance or full gameplay suite. Play stopped, background execution disabled and isolated profile override cleared.


## 2026-10-08 — shared arrows, aligned counters and one parcel action
- Applied frontend-design and Impeccable typography/craft review to the existing McLaren/Nunito native UI. Enlarged the authored PNG arrow insets and inline variants; guide/trophy arrows and 22-unit counters now share one 48-unit row. Numeric facts have a stable larger role, chapter-grid numbers retain prominence, and result/ribbon text compensates for portrait Canvas scale.
- Pending parcels show one Collect action and no unexplained pre-reveal name or Back button. After the existing saved claim, the sole Next action returns to the existing origin/completion flow. Preserved localization keys; Unwrap now reads Collect / Получить / Отримати and Leaf pin has an explicit badge translation. No rules, prices, packages, saved schemas, product or trophy IDs changed.
- PASS: 53 native Unity screen/state captures inspected in one batch; four final targeted captures after the ribbon-inset correction. Actual Collect saved exactly one receipt; Next returned Home without duplicating rewards or changing the puzzle. Guide arrows advanced/restored the page; native checks confirmed common centres, authored texture, 48-unit targets, readable digits and fitting narrow UK Large Text. Three existing EditMode parcel persistence/rollback/idempotence cases passed. Final runtime Console: zero errors.
- Evidence: Reports/NavigationType-Oct08/gallery.html, final-overview.jpg, controls.txt, audit.md and editmode.xml. Design: Docs/UI_NAVIGATION_TYPE_OCT08.md. Play stopped, background execution disabled and isolated profile override cleared. No phone build/install, physical-device or full gameplay-suite test. Impeccable C# detection had no findings; its stale design-context metadata is documented separately without rewriting DESIGN.md.

## 2026-10-08 — localized journal and Ukrainian reading clearance
- Request giver's bare name sits above the character; its separate papyrus nameplate is removed. Reading paper remains. Shortened three request goals in EN/RU/UK with the same conditions and keys. Shared responsive records/requests/trails reserve a physical 16-unit gap above footer actions. Fact numerals and Large Text name/progress rows have sufficient measured height.
- Corrected the narrow Ukrainian disabled trail caption, final comic action width, two-line comic heading and right-aligned wallet/trophy icon/number grouping. Caption sizes stay stable. Existing unopened parcel name hiding, explicit leaf-badge translation, short Ukrainian shake caption and Ready! translations confirmed in current native assets.
- PASS: 12 distinct native Unity captures inspected; three final focused confirmations. RU 390×844 and UK Large Text 320×693. All eight measured dialogue pages and all 16 night-comic chunks fit their actual reading/action bounds; navigation terminates. Two existing pagination tests passed. Final Console zero errors; puzzle hash and wallet unchanged. Earlier diagnostic failures are retained separately.
- Evidence: Reports/LocalizedJournal-Oct08/gallery.html, fresh-overview.jpg, confirmation.jpg, validation.txt, audit.md and editmode.xml. Design: Docs/JOURNAL_LOCALIZED_LAYOUT_OCT08.md. No rules, prices, packages, schemas or IDs changed. No phone build/install, physical-device, full gameplay-suite or device-performance test. Play stopped, background execution disabled and temporary profile override cleared.

## 2026-10-08 — compact map navigation, complete spoken pages and night captions
- Replaced framed direction buttons with the authored compact gold arrow artwork on transparent 44-unit tap targets. Settings-section and menu-row arrows use the same art; Home and Shop remain distinct icons. Map title and mode tabs are compact, and world navigation moved from beneath Shop to above the single level action. The separate Talk button is gone; automatic island and world story beats remain.
- Widened and lowered the paper dialogue strip, floated the portrait above it, reduced body/action type, and split spoken lines at complete sentences. The Bram/Mika storm-and-bridge exchange now appears as four full pages in RU and narrow UK Large Text. Night loss ribbon stays on one line; the row caption sits on paper instead of disappearing on a moving cloud.
- PASS: five focused native map/dialogue/night captures, final night-caption recapture, native settings-arrow image review, and three EditMode dialogue pagination tests. No gameplay hash changes during captures or native Console errors. Evidence: Reports/MapDialogue-Oct08 and Reports/NightContrast-Oct08; design: Docs/MAP_DIALOGUE_OCT08.md and Docs/NIGHT_TEXT_CONTRAST_OCT08.md. No prices, rules, packages, schemas or IDs changed. Play stopped, temporary profile detached and background playback disabled. Full screen gallery, phone build and physical-device testing were not run in this pass.

## 2026-10-08 — framed dialogue names and quieter failure
- Removed the failure-only "Your row" paper/text, the repeated retry summary and the "Try another way" ribbon. The goal, shown row and action buttons remain visible; win and checkpoint headings are unchanged.
- Moved each speaker name out of the dialogue body into a cream-and-wood frame that physically overlaps the portrait and top edge of the papyrus. The name stays legible without taking reading width or leaving a sky gap.
- PASS: three focused native captures at RU 390×844, UK Large Text 320×693 and night failure. Both dialogues still show four complete pages without overflow; native assertions checked frame overlap and absence of the removed captions. No Unity Console errors or puzzle-state changes during captures. Evidence: Reports/MapDialogue-Oct08/02-dialogue-ru.png, 04-dialogue-uk-large.png, 05-night-fail.png and validation.txt. No phone/device test or full gallery rerun. No rules, prices, packages, schemas or IDs changed.

## 2026-10-08 — route record metrics without paper cards
- Replaced the two large papyrus facts with one transparent line: footsteps × best move count and star × actual earned rating, with compact captions below each icon. The former star/"Without help" pairing displayed a clean-route move count as if it were stars; the clean-route record is retained in saved data, while this screen now shows the real rating. A missing rating displays `—`.
- PASS: Unity native RU 390×844 and UK Large Text 320×693 focused captures, assertions for metric row/footer spacing/no paper cards and unchanged puzzle/wallet, localization table rebuilt, zero Console errors. Evidence: `Reports/LocalizedJournal-Oct08/13-records-inline-ru.png`, `14-records-inline-uk.png`, `validation.txt`; design note: `Docs/RECORDS_INLINE_OCT08.md`. Play stopped and temporary profile detached. No phone build, physical-device run or full gallery rerun in this pass.
- Follow-up: route arrows moved onto the level-ribbon row with dark sprite outlines; metric captions removed and both icons enlarged. Fresh RU 390×844 and UK Large Text 320×693 native captures passed alignment, spacing, no-caption and puzzle/wallet assertions. The iPhone build requested afterward is tracked separately below.
- Final inset: papyrus widened and arrow centres moved inward so both 48-unit targets fit within its ends. Native RU/UK Large Text captures and direct hit-area bounds assertions pass (`Reports/LocalizedJournal-Oct08/validation.txt`). The corrected version was subsequently installed and launched on the paired iPhone.

## 2026-10-08 — fresh iPhone build and installation
- Exported the current one-scene Unity iOS IL2CPP player to a new preserved `CASCADE-iOS-2026-10-08` folder: BuildReport Succeeded, 0 errors, 346 existing-category warnings. Xcode 26.6 Release workspace build and automatic development signing succeeded for `com.vickand.cascade`.
- CoreDevice installed and launched the new CASCADE app on the paired iPhone 17; a subsequent process list still showed it running. No uninstall or save reset. Evidence: `Reports/iOS-2026-10-08/README.md`, `unity-build.txt`, `xcode-build.log`, `install.json`, `launch.json`, `processes.log`. The earlier RU/UK route visuals were checked in Unity before export; touch, haptics, audio, performance and screen appearance on the physical iPhone were not measured in this pass.

## 2026-10-08 — release readiness review
- Compared current production guards, monetization/cloud settings, store catalog and iOS build identity with current Apple/Google submission requirements. Blocking project work, store setup and owner inputs are recorded in `Docs/RELEASE_READINESS_OCT08.md`. This is an audit, not a production build or payment/advertising validation.

## 2026-10-08 — corrected route arrows on iPhone
- Exported a fresh iOS IL2CPP player with the inward record arrows: Unity BuildReport Succeeded, 0 errors, 1 warning (missing localized iOS App Info metadata). Xcode Release build succeeded; installed the signed `com.vickand.cascade` app over the existing copy on the paired iPhone 17, then launched it. The install did not request data reset. Evidence: `Reports/iOS-2026-10-08-arrows`. Physical-screen appearance and gameplay interactions were not directly captured in this pass; Unity RU/UK focused visual checks passed earlier.

## 2026-10-08 — catalogue selection, counts and navigation
- Shelf taps now only select and show a name/benefit; the retained **Look closer** button opens detail. Trophy pager arrows no longer call the preview-opening path. Back from detail restores the selected catalogue page, and shop ↔ bag/items return routes were exercised. Unavailable pager arrows keep the same authored arrow instead of reverting to a blue button.
- Tool counts sit above shelves, the non-tool guide tile is removed from My tools, trophy collection count is owned/total, and parcel fragments no longer overlay Mika. Scenery/frame preset display names and short tool copy improved in EN/RU/UK without changing IDs or prices. Workshop's inert gallery entry is hidden from My things; its assets and progress remain intact. Design note: `Docs/CATALOG_FLOW_OCT08.md`.
- PASS: final native Unity MCP flow review and eight RU/UK captures plus a ninth bag capture, including 320×693 Large Text. Evidence: `Reports/CatalogFlow-Oct08`. The first attempted capture ran before scene initialization and threw a test-harness null reference; a fresh isolated Play run completed. Physical-device touch was not checked in this review; the subsequent iPhone build is tracked below.

## 2026-10-08 — catalogue iPhone build
- Fresh Unity iOS IL2CPP export and Xcode Release build succeeded; the corrected catalogue app was installed over the existing CASCADE on the paired iPhone 17 without an uninstall. Immediate CoreDevice launch was denied because the device was locked. Evidence and exact limits: `Reports/iOS-2026-10-08-catalog/README.md`. Await an unlocked phone to confirm launch; direct on-device touch remains untested.

## 2026-10-08 — startup watchdog and menu stability
- Retrieved an iPhone watchdog report for the reported splash hang: the main thread was blocked in Localization/Addressables `WaitForCompletion` from `CascadeGame.InitializeUiLanguage`. Disabled synchronous localization both in the asset and its editor builder, removed the explicit wait, and kept captions readable while tables load.
- Put My things on Home, enabled the previously inert Journal button, removed Shop-to-bag loops, simplified language selection, removed settings section icons and catalogue shelf rails, grouped currency/count with page titles, and kept item descriptions on preview screens. Added a brief reduced-motion-aware transition. Design: `Docs/MENU_STABILITY_OCT08.md`.
- PASS: Unity MCP native catalogue/menu flow and 13 RU/UK screenshots (`Reports/CatalogFlow-Oct08`); Unity iOS export (0 errors), Xcode Release build, and install over the existing app (`Reports/iOS-2026-10-08-menu`). iPhone launch/relaunch is pending because CoreDevice reports that the device is locked. Physical touch and frame-rate profiling were not completed in this pass.


## 2026-10-08 — authored level logic and tutorial repetition
- Confirmed the screenshot's mirror platform came from the generated C01 third stage: its only permutation ran before input, and a repeated two-swap warm-up preceded every campaign puzzle. Fresh starts now use the authored floors directly. Legacy attempts retain their recipe, hashes and replay; Retry keeps the same puzzle. See `Docs/CAMPAIGN_LOGIC_OCT08.md`.
- Removed ambient scenery wind tutorials, honored tutorial opt-out across films/practice/controls, retired deferred examples per board, and shortened the legacy checkpoint ribbon in EN/RU/UK. Kept the gameplay header to the level number instead of a story instruction.
- Completed solver proofs and exact command replays for all 540 primary puzzles plus 1,617 localized bindings (`Reports/CampaignLogic-Oct08/authored-proofs.csv`). The broad NUnit audit was marked timeout despite finishing and writing every witness; 23 other EditMode tests and 5 focused PlayMode tests passed. The audit timeout/cached-witness test optimization was not rerun.
- Inspected 9 native portrait captures and a bounded confirmation of the localized ribbon, including large UK text and the actual roller landing. No physical touch/gameplay or frame-rate claim. Unity iOS export succeeded with 0 errors and the same 346 existing warnings; Xcode Release build and installation over the existing iPhone app succeeded. CoreDevice accepted launch and terminate/relaunch; the new process was still present in the subsequent snapshot. Physical touch/game-ready UI was not observed on the phone.


## 2026-10-08 — touch controls, first Undo and animated restoration
- Small finger movement stays a tap using a threshold scaled to the Canvas; the hit area covers the lower edge of the 3D body. A new deliberate touch settles only an ordinary committed exchange, without queueing commands or skipping contacts/falls.
- Both drag and tap/tap are shown together in the first explanation. Undo appears automatically at its first legal availability, then points to the actual control. Acknowledgements are stored once using existing coach keys; old profiles get the clearer version once. Tutorials off bypass lessons and release the preparation gate on cached puzzles without cancelling an active pointer.
- Undo restores the authoritative snapshot immediately and animates the existing visible pieces on separate upper/lower paths; unchanged duplicates stay in place. Removed the oversized procedural diamond over the score, and kept the result animation on real foreground time during slow frames. No rules/prices/schema/stable IDs changed. Design note: `Docs/TOUCH_UNDO_OCT08.md`.
- PASS: 17 EditMode input/conflict tests and 6 focused PlayMode input/lesson tests, plus two checks after the return-path and cached-level refinements and a held-drag check with tutorials disabled. Inspected the UK 320px explanation and final native moving/restored Undo and settled result captures. Final device build and native evidence are tracked in `Reports/TouchUndo-Oct08`. Physical-device touch latency/FPS have not been measured.

- Final device build: Unity export succeeded (0 errors, same 346 existing warnings), Xcode Release BUILD SUCCEEDED; installed over the existing iPhone app without resetting data. A transient CoreDevice disconnect on the first attempt recovered on one retry. Launch evidence is in `Reports/iOS-2026-10-08-touch-undo`.
- Launch verification is currently pending: CoreDevice rejected launch because iPhone is locked (10002); asked the user to unlock it. This is not a confirmed game crash. Physical touch latency/FPS and game-ready UI remain unverified.


## 2026-10-08 — related three-stage levels and clear mechanic consequences
- Fresh campaign levels use pinned version 6 triads: stage one teaches one primary principle, stages two/three vary the task without repeating the automatic effect lesson. Early curriculum introduces one mechanic at a time; later recipes reuse the authored mechanic vocabulary and useful combinations. Every remaining board takes a meaningful exchange or entry; ribbon charges must be used and repair adds necessary durability. Legacy version 1–5 and authored attempts resume unchanged. Design note: `Docs/LEVEL_TRIADS_OCT08.md`.
- Added brief EN/RU/UK tutorial explanations and actual durability/key/ribbon/frost/stamp consequence captions. The first stage allows Undo before winning. Completed levels offer Next level and Map with a persisted destination through parcel collection; visible level numbers follow campaign order. Centred the complete map star/icon group. Birds and insects visit more often; vacant boards/receiver margins permit earlier bird landing, occupied supports retain a longer idle gate and takeoff on input. No domain rule, price, package or stable-ID changes; saved application recipe/destination fields are additive and documented.
- PASS: exact resolver replay for 540 primary recipes plus 1,617 localized bindings, 6,471 total stages; 12 focused EditMode cases including save/hash compatibility and meaningful repair/ribbon/frost; six focused PlayMode flows including next level, first Undo, independent later stages and wildlife lifecycle. Three final recipe tests and the two corrected tutorial flows passed again after the final fixes. Ten native portrait captures inspected (UK 320px Large Text included). Native bird landing/rest/departure retained the puzzle hash; a quiet minute reached five pooled actors without adding new rigs. No physical touch latency, FPS or human-enjoyment claim.
- Evidence: `Reports/LevelTriads-Oct08`, `Reports/LevelTriads-Final-Oct08`. Earlier development/test failures and an initial native helper timeout at the legitimate unseen world prologue are retained; the final affected tests and map follow-up passed. Play stopped and isolated profile/background execution cleared. Current iPhone build/install is tracked in `Reports/iOS-2026-10-08-triads`.

- Final numbering follow-up: headings, map task and visit action use the selected mode’s offered sequence; the actual Next-level PlayMode flow passed with the rendered `Level 2` heading. Evidence: `Reports/LevelTriads-Number-Oct08/playmode.xml`. Initial iOS export/Xcode build passed, but is superseded by the final export containing this correction.

- Final iPhone delivery: Unity iOS export Succeeded (0 errors, 1 existing localization warning), Xcode Release BUILD SUCCEEDED after removing this task’s superseded generated output to recover from local disk exhaustion. Installed over the existing app on iPhone 17; initial launch and terminate/relaunch accepted, with the new process present afterward. No uninstall/reset. Evidence: `Reports/iOS-2026-10-08-triads`. Physical game-ready UI, touch latency and FPS remain unobserved.

## 2026-10-09 — map star alignment and island progress (started 8 October)
- Centred the star emblem itself between Home and My things; its count extends right. Removed the multiplication mark only from the island-list earned/max stars. Every island now shows completed/total levels for the selected mode, including locked islands. Counts follow moving island anchors at their outside edges and render above clouds, avoiding adjacent-island ambiguity and occlusion.
- Added restrained warm illumination to the current available island and one intermittent hand cue after map inactivity. The cue cycles available islands, resets on input, and is suppressed by dialogue, background/suspension, reduced motion or disabled tutorials. It never intercepts input or changes progress. The existing discovery receipt and game IDs remain intact. Design note: `Docs/ISLAND_PROGRESS_OCT08.md`.
- PASS: native icon-midpoint checks at actual layouts; five mode-specific counts with no overflow; actual island tap/back preserves attempt/hash/reward count; discovery receipt; idle cue visibility and reduced-motion/tutorial suppression; Words progress isolation. Seven native captures inspected, including UK 320px Large Text and the night map. One placement correction and a cloud-layer correction were verified. Evidence: `Reports/IslandProgress-Oct08/native-review.txt`, `preview.html`. Unity Console has no errors. Full gameplay/domain suites were not rerun for these presentation-only changes; physical touch latency/FPS remain unmeasured.
- Play stopped and isolated review profile/background execution cleared before export. Unity export finished before the user stopped further iPhone work on 9 October; no Xcode build or installation followed. Evidence and the explicit delivery stop are recorded in `Reports/iOS-2026-10-08-map-progress`.


## Personal resident missions — 9 October 2026

- Requests removed from Journal; former routes return to the map. Optional resident dialogue offers Accept / Decline after 3–5 new campaign completions per mode, with persisted decline cooldown and story priority. No timer while idle or repeated offers from retries/replays.
- Separate deterministic three-stage recipes use the production resolver, known mechanics and localized source symbols. Solver proves every stage and a modest increase in minimum actions; unproved/duplicate/out-of-band recipes never reach an offer. Closed-path and hourglass timing receive constructive variants. Design: `Docs/PERSONAL_MISSIONS_OCT09.md`.
- Campaign attempt/checkpoint backed up before acceptance; active mission, offer and backup survive save/restart and mode changes. Mission completion counts separately and does not grant campaign clears, stars, parcels or director difficulty. Return restores the campaign.
- UK Mixed is Мікс. Map star and value now share the exact center axis, with value below the emblem to remove optical offset. Native EN dialogue and narrow UK Large Text map inspected.
- Validation: 12 focused solver/save cases passed across the initial run and corrected closed-path/hourglass reruns. Full native PlayMode mission flow passed: decline cooldown, acceptance, save/restart, map resume, all three stages, separate completion and exact campaign restoration. Reports: `Reports/PersonalMissions-Oct09`, `Reports/PersonalMissions-ClosedPath-Oct09`, `Reports/PersonalMissions-Hourglass-Oct09`. No broad suite or player compilation loop; no iPhone export/build/install until the user explicitly requests it.


## Required effect budgets — 9 October 2026

- Found three concrete bypasses in the expanded mission/campaign sample: hourglass mission stages 1 and 3 in C07, and a secondary repair in C500 stage 1. Earlier witness-use checks did not prove necessity of every benefit.
- New candidates now require both a winning production-resolver certificate and exhaustive Unsolvable counterfactuals with each repair, key, hourglass or ribbon individually disabled, keeping wood, row, hazards and other effects identical. Unknown/cancelled searches never pass. Multi-platform hourglass bombs use two durability damage to prevent recovering a missed freeze on a later board. Details: `Docs/EFFECT_BUDGET_OCT09.md`.
- Verified all 540 primary triads and 6,471 primary/localized stage witnesses after strengthening generation. Detailed campaign/mission sample: 48 stages replayed, 28 individual benefit-removal searches, zero bypasses/unknowns. Executed 23 actual focused NUnit methods on production Domain/Solver/Application/Content/Persistence using bundled Mono: required benefits, mission difficulty/distinctness/cancellation, save/mode switch and triad checkpoint/undo. All passed.
- Evidence: `Reports/EffectBudget-Oct09`; reusable focused runner: `Tools/check_effect_budgets.py` (optional `--full` for catalog regeneration). Initial failed ablations retained. Unity-native rerun not completed after this logic change: Mac locked and MCP switched among open Editors; native interface layouts were unchanged. No iPhone build/export/install. Existing pinned saves retain their recipes; new generation uses the stronger proofs.


## Lower, bittersweet score and level arrival — 9 October 2026

- Re-rendered the active menu and all nine world scores: octave-lower solo, lower clarinet in place of high flute, warm seventh/ninth harmony, close string voice leading, descending replies and restrained phrase dynamics. Tempo/duration/individual motifs retained. Ten WAVs installed at their existing paths; corresponding .meta hashes and ToyAudio GUID references retained. Composer baseline remains reproducible; pre-change active WAVs backed up in /private/tmp/cascade-oct09-audio-before. Ambience/SFX and inactive original/fallback tracks untouched.
- Found the audible-arrival issue: map and Playing selected the same world clip. Map/story/browsing now use menu music; Playing/Pause/results/checkpoints use the current world. Background clip loading finishes before crossfade; initially delayed/independently stopped incoming source recovers. Existing two sources, 1.6-second reversible transition, independent preferences and playback positions retained.
- Completed actual-source routing execution: 330 screen/zone cases plus two fallbacks PASS. All ten rendered loops pass notation/PCM checks: unchanged tempo/duration, first-beat solo 12–13 semitones lower, distinct hashes, zero clipping, no boundary step; every source spectral centroid lower. B/A contrast 0.74–3.37 dB. Signal checks do not establish a listening verdict.
- Added a native regression for real map action → audible gameplay track, nine world themes, stopped-source recovery and map return; updated the earlier crossfade check for async readiness. Native import/readback and PlayMode regression NOT RUN: live MCP points to another project; CUA input to CascadeUnity is unavailable. Requested user to unlock/open intended editor. Speaker/headphone/device listening and performance NOT RUN. No compilation/build loop or iPhone export/build/install.
- Repair wording already describes durability for more exchanges. Existing production-resolver budget and individual-benefit necessity proofs remain unchanged; ordinary wood converts +2 durability to two more exchanges, while fragile/blast/gust costs change that conversion. No scripted rescue or hidden extra moves. Human enjoyment is not proven by solving.
- Design/limits: Docs/AUDIO_BITTERSWEET_OCT09.md. Evidence: Reports/Audio-Oct09. Reusable checks: Tools/review_cascade_music.py and Tools/check_score_routing.py.

## Platform durability and first-platform transformations — 9 October 2026

- Replaced constructive-route durability as the final budget with production-solver calibration: try every lower positive durability on every board, repeat after reductions, account for changed landing/wind timing, reject Unknown/cancellation. Require the shortest certificate to retain its intended lesson and all meaningful boards. Early descent and timed gust retain the durability needed to perform that lesson; no arbitrary extra allowance. Existing individual-benefit necessity checks remain mandatory.
- Live gust moved to the second board with a lower playable board underneath; mirrors/rollers already activate below the start. All freshly generated campaign/mission first boards have no entry transform or timed gust. Periodic scenery pressure impulses no longer kick the puzzle rig/devices; gentle breeze, wildlife and real resolver-driven gust reactions remain. Saved pinned recipes retain exact replay/hash compatibility. Design: Docs/DURABILITY_CALIBRATION_OCT09.md.
- PASS: all 540 primary triads, 6,471 exact primary/localized stage replays, 2,948 lower-HP probes (zero retained lesson/all-board solutions or Unknown), 641 individual benefit ablations (all Unsolvable). Executed 43 focused NUnit methods, including campaign/mission budget/layout coverage, four combined wind/contact/fragile/bridge/freeze transitions, mission difficulty/cancellation/save/mode switch and checkpoint/Undo. Evidence: Reports/Durability-Oct09; runner: Tools/check_durability_budgets.py. Initial invalid last-board gust placement corrected and retained in evidence; intermediate synthetic-fixture ablation was removed because those fixtures test transition order rather than generated-budget necessity.
- Unity-native visual/PlayMode checks NOT RUN: MCP project requests did not finish; bounded attempts stopped. Physical FPS/touch/player enjoyment not established. No iPhone export/build/install and no compilation-only verification loop.

## Only the active island's map counter — 9 October 2026

- Removed the world ordinal (1 / 9) from map footer. Only the exact island carrying Mika now creates a completed/total badge; all island tap targets, world arrows, mode-specific progress and availability remain.
- Badge stays horizontally beside its island, choosing right for the left half and left for the right half. Combined island/badge half-widths plus 28 scaled units provide clearance; the existing moving anchor follows island drift and the overlay remains above clouds. No extra vertical placement over neighbouring islands and no tap interception.
- Updated the existing native review helper to assert one Mika badge, correct selected-mode progress, no world ordinal and no island touch-envelope overlap. Review output goes to a new folder to preserve earlier evidence. Source/reference/diff checks passed; no new mirror tests or compilation-only check.
- Native visual/helper run NOT RUN: Unity MCP root request did not finish; computer use confirmed the Mac is locked. No iPhone build/export/install. Design update: Docs/ISLAND_PROGRESS_OCT08.md.

## Aligned island-list heading blocks — 9 October 2026

- Island-list title anchors are symmetric around screen centre; Back keeps its existing left position. Island world subtitle is explicitly centred. Star/earned-total group now sizes to the actual icon and digits instead of centring a fixed 164-unit box with empty space, aligning all three visible blocks.
- Updated the existing native portrait review to check the title/subtitle/star-total centre axis in normal EN and narrow UK Large Text. Source/reference and diff checks passed. Native visual/review execution NOT RUN because the Mac remains locked; no repeated compilation checks or iPhone build/export/install. Design: Docs/ISLAND_PROGRESS_OCT08.md.

## Hanging device clearance and branch alpha — 9 October 2026

- Replaced the 16-pixel bomb/22-pixel tool cord with a shared desired 64-pixel suspension for bomb, key, repair kit, stamp, ribbon and frost flask. Current rotated/animated body bounds and the bomb timer limit length against the next deck or receiver caption with 10-pixel clearance; dense layouts shorten the cord rather than overlap. Original hooks, body attachment points, pendulum motion, pause/reduced motion and gameplay contacts remain.
- BranchStops PNG already contains transparent sky pixels. Runtime branch material now uses the existing world-lit IllustratedSurface shader, whose alpha clip is unconditional, avoiding dependence on a potentially stripped runtime URP Lit alpha-test variant. Original PNG, .meta, atlas halves and dimensions preserved. Player variant stripping is a hypothesis, not a reproduced diagnosis.
- Source/asset/shader reference checks and targeted diff whitespace checks PASS; evidence: Reports/HangingDevices-Oct09/source-review.txt. Native visual/PlayMode/player checks NOT RUN: bounded Unity MCP project-root query did not return; previous native access found a locked Mac. Pending: all six bodies, 1–4 floors, narrow screens, animated reactions/countdown, day/night branch edges. No compilation-only verification or iPhone export/build/install.

## Bomb deadline and ignored-hazard bypasses — 9 October 2026

- Reproduced the gap before changing generation: seven campaign triads contained eight bombs whose fuse met/exceeded initial wood, including C03's 2/2. C37 stages 1–2 and C120 stage 3 admitted actual winning explosion/abandonment routes. Earlier safe-witness/budget checks did not prove bomb challenge. Baseline counterexamples retained in Reports/BombChallenge-Oct09/baseline.csv.
- Fresh campaign/mission recipes now keep fuse below initial wood and reject an initially auto-defused bomb. Normal bomb boards need at least two useful exchanges. Every candidate must exhaustively prove that delivery after any explosion or abandonment is Unsolvable; Unknown rejects. This searches all legal swaps, charged reaches and early descents with the production resolver, including recovery on later boards and existing contacts/wind/fragile/landing timing. Benefits still require their individual removal proofs. No domain damage/failure/contact/scoring rule or schema/ID changes. Design: Docs/BOMB_CHALLENGE_OCT09.md.
- PASS: all 540 campaign triads, 6,471 primary/localized stage replays, 349 bombs with zero unsafe delivery routes, 3,121 lower-durability probes and the retained individual-benefit checks. Executed 54 focused NUnit methods (deadline/no deadline, explosion recovery/abandonment, Unknown/cancellation, campaign/mission risk, combined wind/fragile/bridge/key/repair/freeze transitions, mission/save/checkpoint/Undo), plus one separate actual ProfileStore check preserving an old equal-fuse recipe and in-progress hash. Evidence: Reports/BombChallenge-Oct09; runner: Tools/check_bomb_challenges.py --full (or --save-only).
- Pinned active recipes deliberately retain their prior rules/hash for save compatibility; fresh generation receives the fix. Unity-native visual/play/player validation NOT RUN; solver evidence does not establish human enjoyment or every imaginable layout. No iPhone export/build/install or compilation-only verification loop.

## Cyrillic companion for McLaren — 9 October 2026

- Selected the existing licensed Balsamiq Sans Regular after comparing actual font letterforms with McLaren; rounded hand-drawn forms and close cap height. McLaren's Cyrillic fallback now points to it rather than geometric Nunito; real Balsamiq Bold is connected for weight 700. Headings/buttons/puzzle glyphs inherit the change; Nunito reading copy already uses the same face in EN/RU/UK. Typography regeneration preserves the new links.
- Checked every Cyrillic letter in localized UI plus Russian Ё/ё, Ukrainian Ґ/ґ Є/є І/і Ї/ї and apostrophes against both baked assets. Original atlas/glyph tables and .meta/GUIDs retained, static 1024 atlases, matching 80/8 sampling/padding and scale 1; no new package/atlas/runtime font work. Source/reference/diff checks PASS. Evidence: Reports/CyrillicTypography-Oct09/coverage.txt; design update: Docs/TYPOGRAPHY_OCT08.md.
- Native rendering, locale switching and narrow-screen layout NOT RUN: bounded Unity MCP root query did not return. No compilation-only verification or iPhone export/build/install.


## Mac storage audit — 9 October 2026 (awaiting cleanup approval)

- Read-only storage audit requested for Unity/Xcode/build artifacts plus system and other application caches. Current free space: 63.87 GiB (~68.6 GB), Data volume ~86% used. No files deleted, no applications closed, and no packages/settings/source/assets changed.
- Exact 41-directory base cleanup proposal: 30.73 GiB allocated-block estimate. Includes 15.90 GiB generated Xcode DerivedData build/index/compiler output, 6.89 GiB generated output in /private/tmp/shape-release-xcode-derived-20261008, 2.14 GiB Chrome web/code caches, and 5.80 GiB downloaded npm/pip/Homebrew/updater caches. SourcePackages, installed tools and project dependencies are excluded.
- Separate optional proposal: only shape_sorting-final/Library/Bee, 16.11 GiB generated Unity build cache. Requires separate explicit approval and a closed, inactive project. No ShapeMania files were changed by this audit. Active Cascade Library/Temp and Codex caches are preserved.
- Simulator devices/data (35.26 GiB), Xcode DeviceSupport (13.20 GiB), Archives, user files, saves/backups, Codex/Cursor history, Asset Store/PackageCache, Unity installations, macOS Updates/swap/db and system-managed caches remain outside deletion scope. No broad /tmp, Library/Caches or Trash removal.
- Capacity estimate: Cascade Assets 0.35 GiB, Library 4.25 GiB; current free space exceeds a conservative agent-estimated 30 GiB working allowance. This is not a Unity requirement or proof of a successful rebuild. After approval, remeasure targets, exclude open/changed/unexpected paths, clean only approved exact paths and compare df before/after.
- Evidence: Reports/StorageAudit-2026-10-09/review.txt and plan.json (exact targets, individual sizes, root identities, conditions, exclusions); git-before.bin and unity-cache-sizes.json. du/df/diskutil, running processes/open cache files and Git cache tracking inspected. Protected macOS directories were partly inaccessible; bounded Containers/Group Containers scans timed out and broad home/Library scans were interrupted. No personal file contents inspected.
- Cleanup/post-clean disk verification NOT RUN: awaiting the user's required approval. Unity reimport, player build and gameplay/tests NOT RUN for this storage audit.


## Approved Mac cache cleanup — 9 October 2026 (complete)

- User approved the entire presented base plus optional Bee plan with “Очисти все”. Xcode, Chrome and Unity Hub quit normally; no force-quit, source saving or package/settings changes. Cascade Editor remained open. All 42 target roots matched the reviewed identities, and open-file checks before each removal found no active files.
- Deleted all 42 exact approved targets, including only ShapeMania Library/Bee. No skipped targets/errors; all deleted target directories verified absent. Removed allocated directory estimate: 46.74 GiB. Actual free-space gain during deletion: 36.19 GiB (38.86 GB), measured with statvfs/df rather than inferred from du sums.
- Verified 6907 protected source filesystem entries in Cascade and ShapeMania Assets/Packages/ProjectSettings before and after: zero metadata differences. SourcePackages, PackageCache, Xcode Archives, simulator devices/data, personal files, source/assets/scenes/settings/.meta, Codex history and system-managed files preserved.
- Fresh free space: 103.54 GiB (111.18 GB), above the conservative agent-estimated 30 GiB Cascade working allowance. Disk capacity check PASS. Unity reimport/player build/tests NOT RUN: capacity was verified, not a successful build or gameplay. First builds/downloads after cache removal may take longer.
- Evidence: Reports/StorageAudit-2026-10-09/result.txt, cleanup-result.json, preflight.json and protected-source-before.json. The original target list remains in review.txt; plan.json records completion and approval.


## Addressables / Graphics Settings startup exception — 9 October 2026

- Captured the reported Cascade native Console exception: Addressables 2.8.1 CleanTemporaryPlayerBuildData → RemovePlayerBuildLinkXML → AssetDatabase.DeleteAsset → Unity 6000.3.8f1 RenderPipelineGlobalSettingsAssetProcessor.OnWillDeleteAsset. Baseline retained in Reports/AddressablesGraphicsException-Oct09/native-baseline.json.
- URP configuration/source loss hypothesis contradicted: serialized global-settings GUID resolves to the existing asset, native PC_RPAsset/global asset loading succeeds, and GraphicsSettings/QualitySettings/global asset/.meta/manifest/lock SHA256 values are unchanged by investigation. Project Settings window already closed; generated Addressables link.xml absent. Unity's published 6000.3 processor dereferences Graphics provider inspector/globalSettings during deletion, supporting a likely editor initialization/window restoration issue; exact null member and original timing not confirmed.
- Recovery: native public Refresh with Graphics Settings closed, then two script-reload requests. Original Console entry preserved before clearing it for a clean retest. Final native editor idle (not playing/compiling/updating); Console 0 errors/0 warnings. No production code, packages, graphics settings or scenes changed, no temporary probe files remain.
- Limits: initial diagnostic compilation failed (internal URP type/namespace); corrected internal-reflection probe rejected by MCP namespace security. XML deletion probe rejected because interactive confirmation is unsupported via MCP; it did not create files. Exact original deletion timing, Addressables/player build, runtime/visual/gameplay/tests NOT RUN. Editor recovery verified, permanent recurrence fix not claimed.
- Evidence: Reports/AddressablesGraphicsException-Oct09/result.txt, native-final.json, protected-baseline.json and validation.json. If it recurs, close Graphics/Project Settings before Refresh/reload; preserve configured URP assets and avoid PackageCache patches.

## Attached adventure-map image split — 9 October 2026

- Exported 35 complete white diagram blocks as numbered PNG slides, grouped into 10 sections in reading order. Included full section crops, separate headers, intro, unchanged original JPEG, offline HTML gallery, CSV list and crop manifest. Archive: Outputs/AdventureMapSlides-2026-10-09/Adventure_Map_Slides.zip.
- Original attachment is only 244 × 1280 pixels. Native-size CoreGraphics crops preserve its existing detail; no AI regeneration, pixel enlargement or additional JPEG compression. Small text remains limited by the source resolution. No Unity assets, source, settings or packages changed.
- PASS: all 56 PNG exports match their exact decoded source rectangles; intro and full sections cover every original row; original-copy SHA256 matches; gallery links resolve; all 61 archive files match their originals and ZIP integrity passes. First/last slides and a full section inspected visually. Evidence: Outputs/AdventureMapSlides-2026-10-09/verification.json. Unity tests/build not applicable to this image export.
- At the user's request, started a temporary LAN download service bound to the Mac's current 192.168.0.40 address. It serves only the generated ZIP through an unguessable path, with all other paths returning 404. Verified GET bytes against the archive and HEAD length from this Mac; access from another device has not been tested. Connection details: Outputs/AdventureMapSlides-2026-10-09/share-status.json. Link depends on the running service, current network address and an awake Mac; no router/firewall settings changed.
- User clarified that the shared link should open the ready HTML version. Replaced the archive-only service with a bounded static gallery service on the same LAN address/port and private path. Only exported gallery files and the original ZIP alias are exposed, without directory listing or arbitrary filesystem access. Browser verified the gallery title, 35 slide figures and all 46 displayed images loaded; gallery visually inspected and left open. Other-device connectivity remains untested.
- User then requested plain slides without HTML. Created output/pdf/Adventure_Map_Slides.pdf: 35 image-only pages, one complete original diagram block per page, with no added UI, copy or labels. All 35 embedded images match source PNG pixels exactly; all pages rendered with Poppler, and first/middle/last reviewed visually. Source detail remains limited to the attached 244 × 1280 JPEG. Evidence: Outputs/AdventureMapSlides-2026-10-09/pdf-verification.json.
- LAN service now exposes only this PDF: both the original shared root and slides.pdf return application/pdf with bytes verified against the saved file; old HTML paths return 404. In-app browser could not display the PDF (blank viewport); closed that blank tab and requested the saved PDF in Codex's file panel (queued). Other-device/client viewing remains untested. No project assets/settings/code/packages changed.
- User requires readable slides as individual files; the existing exports DO NOT satisfy this quality requirement. Rechecked the attached JPEG and saved original: identical SHA256, both 244 × 1280 pixels. The 35 separate PNG crops are 117 × 55/56 pixels (last 113 × 42); neither packaging nor PDF enlargement can recover their lost text detail. Thread attachments contain only that montage and the later phone screenshot; no larger source found there. Awaiting a full-resolution source or its exact local location before further export; no fabricated text or further blurred versions produced. Evidence: Outputs/AdventureMapSlides-2026-10-09/quality-audit.json.
- Rechecked the user's new attachment D54CFDF7-A4AD-47C4-A2D8-DC0D9D8CA546/1-Photo-1.jpg: JPEG 244 × 1280, 83,571 bytes, byte-for-byte identical to the first attached montage. No higher-resolution input arrived; readable individual-slide export remains pending the full-resolution original. No further blurred copies generated.

## Full-resolution individual PNG slides — 9 October 2026

- User directed the search to an adjacent chat. Read-only thread inspection found the full source in “Выполнить инструкции из MD”: Reports/ScreenRoadmap-Oct09/cascade-navigation-clear.png, 4400 × 23050 pixels. Earlier source-quality conclusion was limited to this chat's compressed attachments and missed that existing local source. Original and adjacent chat preserved; no messages sent or Unity/source/assets/settings/packages changed.
- Exported 38 separate native-size PNG files, grouped and numbered in reading order: 34 complete route blocks at 2101 × 1011 and four archival screens at 971 × 761. Outputs/CascadeSlides-FullResolution-2026-10-09/Cascade_Slides_PNG.zip contains only those PNG files. Full source snapshot and crop manifest retained outside the archive. No HTML/PDF versions or image upscaling added.
- PASS: all crops match source pixels exactly, all bounds and file dimensions verified, ZIP has exactly 38 PNG entries matching the exported files and passes integrity validation. First/middle/last reviewed visually; route captions and arrows are readable. Evidence: Outputs/CascadeSlides-FullResolution-2026-10-09/verification.json. This resolves the readable individual-image export request using the existing full-resolution source; images retain the source's archived screen captures and labels.

## Account onboarding, branch coach and English store artwork — 9 October 2026

- Prepared Apple/Google entry through the installed Unity Player Accounts browser flow. Linking retains the guest identity/save; existing-account loading requires a choice and uses a separate SDK profile, retaining the original guest session for cancellation/recovery. Delayed sign-in callbacks survive an in-flight save. Public setup file: Configs/authentication.json; Unity import menu validates it before writing. User explicitly deferred configuration; live cloud/purchase flags remain off. No packages, product IDs, prices or canonical profile schema changed.
- New-device local reminders default on and request OS permission once after startup is ready. Persist the request marker before the prompt; denial disables reminders, existing saved opt-outs remain off. Never request remote registration. Real OS permission/delivery checks NOT RUN; screenshot-only QA instances suppress their own notifications.
- Added the revised blocking-branch coach receipt; an unseen branch is eligible even on a later stage and is marked taught only after its interactive lesson. Removed the unconditional later-stage suppression of unknown mechanics. Full native lesson replay NOT RUN; discovery/domain/practice fixtures passed.
- Generated ten selected art originals and applied the new flat iPhone/iPad icon and two-layer Android launcher icon. Icon Composer source and default/dark/clear/tinted previews exported successfully with Apple's ictool. English promotional set: first three illustrated compositions, final two lifestyle images, all using real game screens. Apple large/medium iPhone and 13-inch iPad each have five opaque PNGs; Play has five 1080×1920 PNGs, 512px 32-bit icon and device-free 1024×500 feature. Apple header 3840×1646 and search creative 1920×1280 included.
- Captured 12 actual native screens on dedicated iPhone 17 Pro Max and iPad Pro 13-inch simulators. Updated the countdown's English singular form and kept its label upright while the body swings, then rebuilt/recaptured. No physical iPhone install in this turn; existing phone/profile untouched. Dedicated capture controller is excluded from production players. Source manifests retain device, timestamp and SHA256. Native UI is inserted intact, never generated; background/photo composition and sizes rendered separately.
- PASS: 103 actual pure C#/NUnit tests (reminder policy/save, cloud journal/reconciliation, mechanic discovery and level validation); final native Simulator export with zero errors, Xcode simulator build exit 0, Editor compilation/current icon importer and blank-config rejection. Existing SDK/shader warnings remain (359 in the Simulator export); zero errors is not zero warnings. PASS: 20 screenshot formats, 5 fixed-size upload assets, 12 source hashes and archive integrity. Android runtime/store captures, live Apple/Google login, purchase/cloud round trip and account deletion NOT RUN.
- Release work remains: configure Player Accounts Client ID and deploy/verify CascadeAccount/protected Cloud Save/receipt verification; implement the complete game-account/data deletion path before enabling account creation in a release. Unity's account-management portal alone does not delete this game's UGS data. Verify phone notification permission/delivery and Android runtime; finish store privacy/deletion disclosures. Detailed setup/limits: Docs/ACCOUNT_STORE_ASSETS_OCT09.md and Docs/RELEASE_SETUP_OCT09.md. Deliverables: Reports/StoreAssets-Oct09/CASCADE-English-Store-Pack.zip. No listing upload or publication.

## Animated CASCADE website, Privacy/Terms and Restore — 9 October 2026

- Created Website/ using requested impeccable/frontend-design workflows: original animated floating-island hero, Mika, clouds, varied bird/insect paths, day/night art, actual iPhone captures, adjacent-swap example, accessible mobile menu and reduced motion. Dependency-free static output with six pages, FAQ/support/data-request routes, truthful metadata/JSON-LD, robots/sitemap/llms, canonical/redirect/header configuration, optional consent-gated measurement and app-ads.txt generation from a real publisher ID. Public indexing remains off while publisher details are incomplete. No invented store links, contacts, ratings or release claims.
- Wrote detailed fact-based English Privacy Policy and Terms from one shared source and exported them to offline Unity readers. Added root Settings Restore purchases via the existing verified native restoration path; EN/RU/UK legal controls; approved public links/config importer and mobile store-submission legal guard. No prices, product IDs, packages or puzzle rules changed; design note: Docs/WEBSITE_LEGAL_OCT09.md.
- PASS: six HTML pages, 153 internal links/assets/fragments, structured data and website/game document parity, 1483 KiB scene asset budget; actual local HTTP 200/301/404 behavior; desktop/mobile visual checks, interactive example solved through UI, no overflow/broken images/JS errors. Native Unity import compiled; isolated QA verified all three Settings controls, both complete document scroll endpoints and Close back to Settings, with unchanged puzzle state. Initial QA name lookup failed because button names strip spaces; corrected the QA selector. End capture sets scroll after resized layout and asserts final character lies inside viewport. Final Console has zero errors. Player profile preserved.
- Deliverables: Reports/Website-Oct09/site-preview.png and CASCADE-Website-Ready.zip (59 files, ZIP integrity verified); setup/config: Configs/website.json, Website/README.md. Temporary local preview was reviewed in Codex; the owned server was stopped at final teardown. No phone build/install, live restore transaction, cloud account deletion or production Core Web Vitals test in this task.
- BLOCKED external upload: automatic approval review rejected uploading site source, proprietary artwork and legal drafts to the private Sites repository without explicit transfer/publication approval. Site identity registered only; no source push/version/deployment. Do not bypass through another channel. Local deliverable complete.
- Remaining for public release: real publisher name/country/address/support/privacy email, confirmed audience and document approval; user-approved hosting and signed-out public URL verification; final store links, AdMob publisher ID/ad units, Search Console/Webmaster verification and optional consented analytics IDs. Complete/verify actual game-account/data deletion before enabling release account creation; finish accurate App Privacy/Data Safety and native store/consent/device verification.

## First tutorial Undo, hanging ropes and result ceremony — 9 October 2026

- First unseen interactive Undo lesson now restores the real snapshot without Undo/move rating penalties or free-Undo cost. Exemption requires a strict current-state Undo practice gate and the UI's unseen-lesson flag. Ordinary Undo remains charged; original no-argument API preserved. No save schema, products, prices or IDs changed. Design note: Docs/TUTORIAL_COMPLETION_OCT09.md.
- Continuous ropes remain enabled throughout the visible rig, including terminal receiver frames/checkpoints and wood destruction; screen navigation still owns whole-rig visibility. Widened completion/stage ribbons and constrained completion titles to one line. Stars reveal after menu arrival, rise separately with a soft overshoot/glints/chimes, then metrics reveal. Non-gift papers now fully fade before the finite 3.2-second ceremony stops. Reduced motion settles immediately; presentation never awards or advances gameplay.
- PASS: 85 actual pure C#/NUnit tests, including lesson exemption, normal/suggested Undo costs, repeat prevention and save restoration. Added assertions to the existing first-Undo PlayMode fixture; native test-runner execution of that fixture NOT RUN. Corrected a pre-existing personal-best fixture's missing earned-clear synchronization; production replay-credit guards unchanged.
- Native isolated C01 reproduction passed the actual first Undo lesson, all three stages, an optimal five-star receipt and terminal/checkpoint rope visibility. Captured/inspected expanded English ribbon and intermediate star reveal. Review setup initially stopped on unrelated Swap/ClosedPath coaches; supplied pre-taught setup and resumed final stage with tutorials off. Player profile/background restored; Editor stopped and final compilationFailed=false. No phone build/install.
- Validation limit: native RU/UK switching stayed English after Addressables localization preload failed; those captures are not evidence of translated runtime loading. Review assertions now reject that false positive. Also preserved Unity IAP Editor authoring's remote Services Gateway Service Unavailable error; no SDK suppression/package changes. Evidence: Reports/TutorialCompletion-Oct09/native-review.txt, pure-suite-summary.json, native-console-preserved.json and UI captures. Last native imagery preceded the final paper-fade correction; endpoint is checked against the 3.2-second lifetime, final sources imported, but that correction was not recaptured.

## Undo after falling, failure ribbon and direct victory — 9 October 2026

- Ordinary Undo now restores the last atomic snapshot after automatic collapse, bomb damage, manual descent and rejected delivery. It also exits the failed UI before animating the old presented row back to its restored floor. Won stages remain final; the two-Undo allowance, normal rating cost and hourglass rewind to an earlier floor remain. No prices, products, IDs, packages or save/content schemas changed. Design note: Docs/UNDO_OUTCOME_OCT09.md.
- Failure reuses the existing illustrated completion ribbon with localized Try again. Mika's ledge follows aspect-fit sprite trim padding and the thin timber's visible center. Final success retains destruction/fall/receiver landing, then opens the stars directly, without the intermediate Mika/goal ceremony. Checkpoint ribbons remain.
- PASS: 107 actual pure C#/NUnit tests, including automatic/bomb collapse, failed receiver, save/reload, subsequent input, ordinary allowances, purchased-tool transactions and first tutorial exemption. Native isolated reproduction passed actual C02 first descent/failed receiver + Undo, all three C02 stages, final-delivery frames without the intermediate panel, a separate two-floor collapse + animated Undo, failed receiver + Undo + subsequent winning touch. Failure and victory captured/inspected at 390x844; refined foot/ledge placement was recaptured with the same geometry now committed in source.
- QA setup initially assumed C02 contained a second wooden floor and later failed to acknowledge its checkpoint dialogue; corrected the review, preserved setup diagnostics and completed the native run. Mac was locked; native MCP rendering worked, using foreground callbacks only in the isolated QA profile. Player/device save untouched. No iPhone build/install or physical-touch test. Evidence: Reports/UndoOutcome-Oct09.
- Final source import passed with compilationFailed=false; Editor stopped, default profile/background restored. After preserving and clearing only review setup diagnostics, final Console has zero errors. Physical device input/animation timing NOT RUN.

## Hint lamp auto-play exploit — 9 October 2026

- Confirmed direct cause: second lamp tap called ApplyHint, executing the solver command; subsequent taps requested unlimited new commands. Removed both hint-execution APIs. Lamp now only shows/hides a pointer; swaps/descent require player input. Explicit inventory-charged Signal Lantern remains a separate one-move tool.
- Default proposed policy: one free revealed step per stage attempt, then explicit bag selection. Undo/save reload only allow reopening the already granted state. Retry/new stage starts a fresh allowance. Optional FreeHintStateHash persisted atomically; failed save rolls back grant and hint flag. No prices/products/IDs/packages changed. Design note: Docs/HINT_LAMP_OCT09.md.
- PASS: 135 actual pure C#/NUnit tests, including repeated hints, manual moves, stale/unknown offers, Undo/reload, save failure, explicit tools and inventory caps, existing tutorial/input/shop/ad regressions. No production ApplyHint/TryApplyHint callers remain. Native source import reported compilationFailed=false.
- PASS native isolated review: 254 actual Hint-button callbacks, pending-search burst, show/hide/re-show, manual two-tap swap, exhausted allowance, empty bag, save restoration, actual Undo and owned Wayfinder. Lamp opens the stocked bag without spending; explicit Wayfinder selection consumes exactly one and never moves the row. First fixture seeded the ad-preview ledger instead of actual inventory; corrected fixture only and preserved diagnostic. Final Console zero errors/warnings, Editor stopped and normal profile/background restored. First Hint tutorial and physical device touch NOT RUN this turn; no phone build/install. Evidence: Reports/HintLamp-Oct09.

## Integrated passage latch and story rewrite — 9 October 2026

- Replaced floating gold one-use link with a board-mounted timber stop, inset plate and brass pivot. It folds open / springs upright from the presented edge state and follows timber sag/roll; Undo reverses it. It also respects an initially closed edge/key opening. No resolver, puzzle rule, price, package, ID or save-schema changes.
- Rewrote all 45 island midpoint exchanges as concrete obstacle/decision pairs; removed their redundant second exchanges. Corrected outgoing letter vs old archive note chronology, tied Sora's apology to Neri's response, connected Gran's window signal and her received letter. Opening comic now grounds sorting/delivery in the cargo lifts and receiver. Nine optional mission requests name concrete deliveries/beneficiaries. EN/RU/UK source provided in Data/story_revision_oct09.tsv and integrated localization source.
- Openings no longer replay previous payoff; payoffs no longer pre-play the next opening; revisits use current island context. Checkpoint banter now occurs once per world instead of repeating throughout it. Existing receipt IDs preserved. Design rationale: Docs/STORY_LATCH_OCT09.md.
- PASS: 96 pure C#/NUnit regressions, including all comic pages/translations, story receipts, scene counts, three-mode history and one-use bridge/Undo rules. 125 EN/RU/UK entries authored (123 live), placeholder parity and source coverage checked. Native swap → closed stop → actual Undo → reopened stop passed; final open/closed/Undo and full English dialogue captured and visually inspected at 390×844. Refined the first render's thin timber: normalized/cropped atlas UV for the elongated mesh and widened the stop; recaptured final version. Final compilationFailed=false; normal profile/background restored, Editor stopped. Mac locked, native rendering used. Runtime RU/UK loading, external language edit and physical device test NOT RUN; no iPhone build/install. Evidence: Reports/StoryLatch-Oct09.

## Family store images and platform readability — 9 October 2026

- Replaced lifestyle 04 with mother/father/daughter/son and 05 with two diverse families of friends (four parents, four children). Creative 03 and lifestyle 05 now show actual Words gameplay; lifestyle 04 shows Shapes. Updated all three phone upload sizes and ZIP; inspected final compositions. PASS: 20 screenshot dimensions/color types, 5 fixed-size assets, 12 unchanged native source hashes, all 9 revised exports match archive. These are existing Simulator captures in new compositions, not a new iPhone build.
- Upcoming mirror/roller/landing-wind discovery now happens at mount, before a command; active hazards retain teaching priority. No already-applied entry lesson or hidden later-board contacts. Real resolver events still complete practice. PASS: 98 actual pure C#/NUnit tests for discovery, guided practice, routes and linked/fragile rules.
- Replaced purple linked-pair brackets with hemp belt/brass pulleys on the board and in lesson demos. Added localized high-contrast splintered fissures at each fragile edge. Durability plaque chooses a position clear of mechanism marks. Rules/prices/IDs/packages/schemas unchanged; note: Docs/PLATFORM_READABILITY_OCT09.md.
- Native isolated review confirmed all four introductions appear without any move, controls release after acknowledgment, and an actual linked swap exchanges four positions atomically. First visual pass exposed missing CanvasRenderer on the new custom graphic; fixed the component requirement. Final recapture pending (Unity MCP disconnected after reload; Mac locked). Review now automatically restores its temporary profile/background and stops Play Mode on successful completion; source-reload resumes only an already active isolated review. No device build/install or physical touch validation.

## Parallel-sky mode conversations — 9 October 2026

- Automatic story gates now consult active plus archived narrative receipts: read world comics, island openings/asides/checkpoints/payoffs no longer replay just because the player changes Shapes/Words/Mixed. Existing per-mode clears, attempts, stars, rewards and serialized lists remain independent and untouched. Legacy saves work by querying their existing archives, without migration.
- Added short first-arrival and first-return conversations for each destination mode, with the destination world's resident and Mika acknowledging a parallel sky. Each variant plays once, then tab switching stays quick. A first-ever selection with no story memory keeps normal onboarding. Save failure cannot show a successful arrival. Removed a redundant second ShowJourney call in ChooseSoloMode that could immediately dismiss the new dialogue.
- New narrative-only receipt IDs 9000–9002 / 9010–9012 documented in Docs/PARALLEL_MODE_STORY.md; no schema, product, price, reward or package changes. Authored 15 EN/RU/UK lines in Data/parallel_skies_localization.tsv, ui_localization.tsv and shipped localization tables; existing localization IDs preserved.
- PASS: 56 actual pure C#/NUnit tests (parallel memory, save/reload, independent journeys, world comics, authored story, level definitions). Shipped localization YAML parses with unique IDs and all 45 translated values matching source. Updated native PlayMode flow regression and navigation fixtures, but PlayMode/UI animation and runtime language loading NOT RUN this turn (previous Unity connection lost while Mac locked). No iPhone build/install.


## Platform effect readability and view lifetime — 10 October 2026

- Strengthened localized fragile fissures/exposed timber; removed the first draft's lightning-badge appearance. Enlarged illustrated branch obstructions and seated them in riveted brass keyhole feet; opened sockets reflect the real key event. Kept physical one-use hinged stops. Linked pairs now have spoked brass pulleys/hemp belt. Added opposing mirror arrows, directional roller arrows and static gust-grain markings. Durability inset avoids these marks. Markings follow wood deformation and fade during destruction. Presentation only; no rules, IDs, prices, schemas or packages changed. Design note: Docs/PLATFORM_READABILITY_OCT10.md.
- Found and fixed a real Canvas rebuild exception when changing floor counts: mechanism overlays mounted before the new machine rig tried reading old floor meshes. Guard now checks the actual nested floor-view identity, not merely its index/count. Native QA now refuses success if Unity logs errors and has a bounded batch timeout.
- PASS final native isolated Play Mode: four introductions before a move; actual linked atomic exchange; fragile exchange consumes two durability; key opens the displayed passage; one-use stop closes; actual gust exchange; Undo restores each of four snapshots; dense six-piece/four-floor render at 320x693. Seventeen native renders captured, final boards visually inspected. Final Editor exit 0, no C# errors/runtime exceptions; temporary profile/background restored. Targeted diff whitespace check passed.
- Preserved initial invalid gust-fixture diagnostic and intermediate view-rebinding/guard failures; corrected the fixture rather than relaxing level validation, then completed a clean final run. Pure domain suite not rerun (no domain change). Physical iPhone input, whole-campaign human enjoyment and player build/install NOT RUN this turn. Evidence: Reports/PlatformReadability-Oct10/editor-final.log, native-review.txt and visual-review.txt.


## Fresh iPhone installation and old-build cleanup — 10 October 2026

- Built current platform-effect fixes as CASCADE 0.1.0 / 202610102, explicit development iOS arm64 IL2CPP player; same com.vickand.cascade and existing signing team. Unity export succeeded (0 errors, 360 existing-style shader/package warnings); Xcode Release succeeded and deep/strict code-sign verification passed. ProjectSettings and affected gameplay-source hashes remain unchanged after temporary build settings restore.
- Installed in place on the paired iPhone 17, verified the installed build number, launched successfully and checked its process remains running. Backed up the real profile before update and compared the device copy after update: byte-for-byte identical. No uninstall or save reset. Physical visual/touch/audio/pause-resume smoke testing NOT RUN; launch/process lifetime is not whole-game QA.
- Removed 8 old generated export/DerivedData directories after installation and save verification, reclaiming 25.6 GiB. Checked these contained no Git-tracked files. Kept only the fresh export under CascadeUnity/Builds; retained original assets, screenshots and diagnostic reports. Current compiled app retained under Reports/iPhoneInstall-Oct10-latest/DerivedData.
- Evidence and cleanup manifest: Reports/iPhoneInstall-Oct10-latest; original scene and source assets preserved. New build number/report location are local build metadata, not new store/product IDs.

## Google/Apple account activation preparation — 10 October 2026

- Public `Configs/authentication.json` is now validated strictly (project, identifiers, exact field types, duplicate/unknown fields, activation dependencies). Check/Apply menus and a player-prebuild hook apply it via native SerializedObject. Empty Client ID keeps account/cloud features off, including stale settings. Default SDK redirects and openid/offline_access scopes; no provider secrets or new Unity packages/profile schemas/product IDs.
- Actual CloudSyncClient now bounds browser waiting, supports cancellation/retry, ignores callbacks after cancellation/disposal, refreshes buttons after releasing a busy operation, prevents purchase identity races, and rolls a profile replacement back if committing its new cloud journal fails. Existing-account loading remains explicit; no force-link or merging foreign wallets.
- Added `Tools/prepare_accounts.py` for config check, actual-source checks/local module publication, UGS dry-run and explicit module deployment. Public/remote ID and secret locations, callbacks, operator activation steps and remaining release requirements: Docs/ACCOUNT_ACTIVATION_OCT10.md; deployment env template: Configs/account-deployment.env.example. Real flags remain false; no remote deployment performed.
- PASS:43 client/config checks against SDK transport doubles,80 actual Cloud Code endpoint checks with fake SDK transport, Release module publish, native Unity final import/config/localization validation exit0 with no C# compile errors. Reports/AccountActivation-Oct10. NOT RUN:live OAuth/Cloud Save, physical-device/cross-device login or purchases, phone UI/build/install, CLI deployment or online NuGet vulnerability audit (NU1900). Full game-account/data deletion remains a release blocker already recorded in Docs/ACCOUNT_STORE_ASSETS_OCT09.md; the Unity identity portal is not a substitute.

## 10 October — calm bird and insect flights

- [x] Replace whole-card edge-on yaw with painted dorsal bodies for all 17 species; keep one wing pair through turns, avoid translucent double wings and preserve perched side poses. Existing artwork preserved.
- [x] Broad return arcs, smooth speed changes, quieter body flutter, stable dragonfly hover direction, longer visits and glides; slower light-seeking moth orbits. Shared ambient clock remains authoritative.
- [x] Limit simultaneous fauna to three main visitors (including a perched bird) and one extra; alternating insects, non-overlapping extra visits, longer perch rests/cooldowns. Preserve all cosmetic families and accessibility/quality controls.
- [x] PASS: 16 native EditMode tests; two native PlayMode lifecycle tests; nine-world native 180-second sampling, max four visitors/one extra, stable 49-renderer pool, freeze/Reduced Motion, unchanged puzzle hash. Final capture log has zero recorded runtime errors.
- [x] Visually inspected native dorsal species renders and final flight frames; recorded actual Fly movement for four styles over 18 seconds at 12 fps. Evidence: Reports/WildlifeFlight-Oct10. Design: Docs/WILDLIFE_FLIGHT_OCT10.md. No phone build/install or physical-device performance measurement.


## 10 October — receiver suspension, delivery and hanging saw

- [x] Add a heavier bounded receiver suspension shared by rim/sockets/target glyph faces, delivered pieces and brass/hemp rope attachments. Layout anchors remain canonical; pause/suspend and Reduced Motion are respected.
- [x] Replace free-body lateral launch/bounce/return during ordinary descent with continuous gravity-to-seat presentation. Phone acceleration rocks the hanging rig without launching figures or letter tiles. Preserve original row/state, cancellation gates and Undo.
- [x] Add receiver light strands, target aura and travelling motes during final delivery; committed wrong rows repel together into the comparison area before the existing failure screen. One pooled mesh; no gameplay decisions in animations.
- [x] Replace displayed branch-opening key with a generated pruning saw, including tutorial film, action animation and EN/RU/UK copy; retain Key mechanic/domain/persisted IDs. Remove redundant branch padlock inlays and their strength-plate offset. Preserve one-use bridge hardware.
- [x] Give hanging bodies support inertia, longer cords where space permits, explicit cord/timer reservation and off-centre grip fitting. Dense layouts allocate wider gaps under hanging contacts. Dedicated timber forward/shadow/depth shader always clips transparent cracks, including stripped player Lit variants.
- [x] Automatically discover FarSwap before another gesture, both for owned reach and after ribbon collection; ribbon contacts request a new scan after presentation/practice settles.
- [x] PASS: final native isolated portrait Play Mode QA, including actual lower-board descent/Undo, wind reaction, suspended freeze and unchanged hash, two automatic reach lessons, dense six-piece/four-floor hanging saw+bomb at 320x693, both committed deliveries, target letters following without drift. Visually inspected native delivery strands, hanging contacts, transparent cracks, word receiver and attachments. Native review exit 0 with zero recorded game errors. 53 native EditMode tests and two targeted PlayMode tests passed (virtual accelerometer, bounded paused descent). Updated obsolete tests that required jumping/tumbling pieces.
- [x] Preserved intermediate diagnostics and corrected the missing coach markers/unreachable failure fixture rather than relaxing preparation rules. Initial desktop landscape measurements were replaced by an explicit portrait target; native QA also prompted actual grip clearance and floor-gap fixes. Final evidence: Reports/ReceiverMotion-Oct10/native-delivery.log, native-review.txt, editmode-final.xml, playmode.xml, native renders. A transient MCP relay port collision is logged in the test-run process; both game tests still passed, no C# compile/shader error or game exception.
- [ ] Physical iPhone tilt/touch/frame-time testing and player shader-variant verification NOT RUN. No iPhone build/install or whole-campaign human enjoyment assessment this turn. Design and imagegen prompt: Docs/RECEIVER_PHYSICS_OCT10.md; source assets/metas and unrelated work preserved.

## 10 October — bounded hints, shared teaching and useful parcels

- [x] Replace the single-state lamp allowance with stage budgets: three in the opening level, one in short stages, two for longer rows or three-plus floors. Persist unique grants through Undo/reload, keep repeated viewing free, alternate free/rewarded new hints, and require an accepted video after the free allowance is spent. Hints never execute a command. Failed persistence rolls back allowance and inventory.
- [x] Move Hint to the lower controls with a counter, make Undo round, and add the upper question-mark film replay. Replay has no live practice/finger; already seen mechanics are shared across mode histories. Closing a tool film spends nothing; explicit confirmation performs one eligible use.
- [x] Add an optional, explicit rewarded hint offer and distinguish hint receipts from coin celebration. A cancelled/unavailable video grants nothing; token readiness refreshes controls; reloaded paid information can reopen with an empty wallet. Real AdMob/SSV validation remains pending configuration.
- [x] On 10 October, change the optional rewarded-hint cadence to every second new hint while free stage hints remain, then require rewarded video for further hints. Existing stage-specific free limits and per-state receipt checks remain. RU/UK offer copy updated. Design note: `Docs/HINT_LAMP_OCT09.md`.
- [ ] NOT RUN: Unity EditMode/PlayMode or visual review; the headless Unity licensing service was unavailable earlier in this session. Ad delivery still depends on configured AdMob IDs and privacy approval.
- [x] Fix Landing lens on the final delivery and open its actual resolver prediction immediately; preserve live state and release input on closing. Wayfinder rejects ready rows, Signal lantern accepts a proved safe descent. Verified all five tool transactions, limits, rollback and restored usage; show the specific reason for an ineligible stocked tool. Clarify two uses per stage, each type once, and the two-star cap for the whole level.
- [x] Generate and integrate eight transparent, parcel-only Mika brooches. Recipe v4 guarantees eight distinct looks plus four small coin parcels among the first twelve new parcels; subsequent parcels mix cosmetics, tools and coins. Duplicate compensation stays bounded. Preserve old fixed receipts/claims, shop prices and product IDs; prohibit fake or shop ownership of the exclusives. Remove obsolete gift-selection affordance for the new recipe.
- [x] PASS: 142 native EditMode tests, zero failed; isolated native portrait Play Mode QA with 300 actual lamp callbacks, exhausted/reloaded allowances, Undo, tool-film cancellation, Lens/Wayfinder effects, cap feedback, passive replay, cross-mode lesson memory, collection action and all eight sprite resources. Final process exited 0; no recorded game exception or C# compile error. Visually inspected the final lower controls, counter, tray limits and lens prediction. Evidence: Reports/StageHelp-Oct10/final-validated.log, editmode-final.xml, native-review.txt and native renders. EN/RU/UK tables: 1816 valid nonempty keys, 5448 translated entries.
- [ ] NOT RUN: live rewarded SDK/SSV, Cloud Code deployment, physical iPhone touches/performance, whole-campaign human enjoyment assessment. No phone build/install. Existing authenticated-cloud rewarded offers stay blocked until server verification is integrated. Design, appended IDs, reward/save changes and imagegen prompt: Docs/HINTS_PARCELS_OCT10.md.

## 10 October — inline shopping, sixth tool and clear purchase feedback

- [x] Replace tool shopping's extra detail screen with persistent grid/title/benefit/price and direct Buy. Cosmetic shopping also buys/equips beneath its grid; collection previews remain. Six cards align in a 3×2 grid; inventory retains its own sky treatment and the shop has a canvas canopy and warmer cards.
- [x] Remove unreachable old tool-detail and overlapping inventory-caption layouts. Name/benefit/limits/primary/Shop controls now share one responsive stack with gaps and bounded lower edge. Brief EN/RU/UK limits explain each type once, two tools per stage and the two-star whole-level cap. Reserve wallet space while keeping the heading centered.
- [x] Keep an insufficient-wallet Buy tappable: show the exact missing amount, highlight wallet/price, give reject audio/haptic feedback and a restrained button movement. Feedback survives deferred Destroy and UI redraw/contrast updates; Reduced Motion suppresses movement. No funds or stock are consumed on rejection.
- [x] Append Repair kit (140 virtual coins, stock max five): +2 current-board durability, no row/contact/bomb/gust action, once per stage and under the shared assistance cap. Existing prices/IAP IDs and parcel schedules unchanged. Reuse toolbox artwork with a brass hammer/wrench mark; include isolated tutorial film, lower tray, inventory and animation.
- [x] Persist sixth stock and the replay repair boundary, roll back failed transactions, retain old omitted-field JSON/hash compatibility, and handle Undo/hourglass across the repair. Update shared account inventory financing/import/merge without enabling or deploying remote services.
- [x] PASS: 163 native EditMode tests; isolated native portrait UI QA through actual Button callbacks on EN/RU/UK, 390×844 and 320×693, measured no stack overlap/TMP overflow, exact purchases/rejections and kit application. Final native review exited 0 with no recorded game exception or C# compile error. Visually inspected the final shop, bag, compact Cyrillic layouts and icon treatment. Evidence: Reports/ShopInline-Oct10/native-verified.log, editmode-final.xml, native-review.txt and native renders. Native localization imported 1827 keys/5481 translated entries; new script metas generated by Unity.
- [ ] NOT RUN: physical-device touch/haptics/frame-time, real store/ad transactions, live account sync/backend deployment. No iPhone build/install. Shared Cloud Code module must be redeployed before future account activation. Design/optional data fields: Docs/SHOP_INLINE_OCT10.md.

## 10 October — lateral animated field bag

- [x] Replace the upward field tray with two lateral wings containing all six tools/counts. Center the bag; ease Undo/Hint left and Descend right, then restore canonical slots on closing. Remove the safe-area override that erased the bag's assigned offsets. Fit assistance limits on their own paper, clear of target symbols and controls.
- [x] Use one unscaled progress for lateral reveal, elastic scale, control visibility and continuous receiver clearance. Reverse from the current position, gate moving/offscreen hit regions, settle on navigation/suspension/Reduced Motion, and refresh machine anchors only when lower geometry moves. No prices, rules, schemas, IDs or gameplay transitions changed.
- [x] PASS: isolated native Unity Play Mode using actual bag/card callbacks; 390×844 and 320×693, EN/RU/UK with Large Text, six 44+px targets, readable counts/limits, no card/receiver/bag overlap, exact close restoration, 24 interrupted reversals, unscaled animation, Reduced Motion, Shop return, pause/resume and one actual repair-kit use. Row and durability remain unchanged during toggling. Visually inspected native renders. Final process exited 0 with no game exception or C# compile error. Evidence: Reports/ToolBelt-Oct10/native-verified.log, native-review.txt; design: Docs/TOOL_BELT_OCT10.md.
- [ ] NOT RUN: physical iPhone touch/performance. No phone build/install this turn.

## 10 October — manual collectible activation and removal

- [x] Stop automatic Mika appearance/background activation on the third legacy fragment or whole cosmetic parcel. Show a brief EN/RU/UK location message and direct View in bag link to the collected look. Ownership, old parcel recipes, rewards and existing saved selections remain intact.
- [x] Expose Use/Remove for the Mika slot in collection and inline Shop, like scene/frame styles. Removal immediately disables global trophy foliage and keeps the collectible. Use a durable transaction with failed-save rollback; preserve the independent scene/frame slots. Three-way cloud merge respects removal, unchanged local choices and account equipment on joining. No new save field or ID.
- [x] PASS: 72 focused native EditMode tests and isolated native Play Mode through real reward/collection/Shop callbacks; third leaf completion, explicit start/stop, saved/reloaded off choice, ownership retention, navigation and EN/RU/UK narrow portrait/Large Text reward layout. Visually inspected native renders; reserve title space before the collection counter. Evidence: Reports/ManualTrophies-Oct10/native-verified.log, native-review.txt, editmode.xml; design: Docs/MANUAL_TROPHIES_OCT10.md.
- [ ] NOT RUN: physical-phone touch/performance and live cloud deployment. No phone build/install. Shared Cloud Code module must be redeployed before account activation.

## 10 October — readable circular saw contact

- [x] Replace the displayed hand saw with a generated transparent circular blade and a large live shape/letter center. Aspect-fit shared gameplay/demo/film markers; move the gameplay cord to the top hanging eye and localize the title in EN/RU/UK. Preserve original art/metas and Key/reaction/save IDs.
- [x] Reserve additional visual space beneath saw contacts in dense decks without changing floor order or gameplay rules.
- [x] PASS: native isolated portrait QA exited 0; actual contact and newly opened exchange, EN/RU/UK letter faces at 320px Large Text, dense saw+bomb wind with unchanged state hash, and lesson film. Visually inspected native shape/UK/dense/lesson renders. Evidence: Reports/SawDisc-Oct10/native-passed.log and native-review.txt. Prompt/asset/validation details: Docs/SAW_DISC_OCT10.md.
- [ ] NOT RUN: physical iPhone, separate device-demo menu, whole EditMode suite. No phone build/install. Earlier review-fixture diagnostics preserved and corrected.

## 10 October — simple rainbow receiver threads

- [x] Remove receiver ellipses, wrapping double strands and motes. Keep one thin seamless thread per arriving piece, with a softly edged moving rainbow gradient inside it. Preserve gameplay, delivery timing and save IDs.
- [x] PASS: native isolated portrait Play Mode review exited 0 on the first run; shader, real successful/rejected shape and word deliveries, suspended colour freeze, phase advancement, unchanged puzzle hash and cleanup. Visually inspected two native colour-flow frames. Evidence: Reports/ReceiverThread-Oct10/native.log and native-review.txt; details: Docs/RECEIVER_THREAD_OCT10.md.
- [ ] NOT RUN: physical iPhone, player shader variants, EditMode suite or performance benchmark. No phone build/install.

## 10 October — four-platform clearance

- [x] Add a minimum gap before distributing contact-weighted vertical space, reuse the gap buffer and update anchors with receiver/bag safe-area layout. Bound hanging props against the whole tilted next deck and its live rendered row, including the upright timer reserve. Preserve floor order, durability, IDs and gameplay rules.
- [x] PASS: final isolated native Unity Play Mode exited 0 with no recorded game exception or C# compile error. All 97 packaged four-deck stages at 390×844 and 320×693 (194 initial layouts); dense alternating wind, bag reveal/close, sampled real witness descent and animated Undo, unchanged puzzle hash. 2,923 geometry / 5,616 contact samples; minimum art/timer clearance 13.5 logical points. Visually inspected normal and narrow bag native renders. Evidence: Reports/FourFloorSpacing-Oct10/native.log and native-review.txt; details: Docs/FOUR_FLOOR_SPACING_OCT10.md. Earlier namespace and empty-bag fixture diagnostics preserved and corrected.
- [ ] NOT RUN: physical iPhone/performance, whole EditMode suite, exhaustive visual witness playback or human-enjoyment assessment. No phone build/install.

## 10 October — iPhone build 202610103

- [x] Export the current Unity project for iOS, build and verify the signed app, and install it over com.vickand.cascade on the connected iPhone 17. Unity export: zero errors, 348 warnings; final Xcode build and signature verification exited 0. Preserve bundle ID, signing team, store product IDs, game rules and source assets.
- [x] Diagnose the initial device startup failure from its actual console: missing global-metadata.dat after a disk-space packaging workaround. Restore the original Xcode Data resource phase, rebuild, and verify all 34 exported data files in the final signed app before reinstalling.
- [x] Verify byte-identical profile immediately before/after the corrected installation, successful launch and the live app process after 12 seconds. Restore raw ProjectSettings byte-for-byte against the original hash. Evidence: Reports/iPhoneInstall-Oct10-202610103/build-metadata.json, final-packaged-data-hashes.json, corrected-launch.json and corrected-processes.json; earlier failure diagnostics retained.
- [ ] NOT RUN: whole-campaign device playthrough, physical touch/tilt/audio/performance QA, live purchases/ads or store submission. This is a local development player. Future source changes require a fresh Unity export; generated output optimizations and their limits are documented in build-note.md.

## 10 October — contextual effect lessons

- [x] Limit the gameplay question button to effect tutorials found on the current and remaining floors of this level. Remove general controls and stocked-tool lessons from that list; hide the button on levels without such effects and outside active play.
- [x] Replace the small “Next lesson” text link with a larger, high-contrast enamel arrow and reserve space for it in the film layout.
- [ ] NOT RUN: Unity compilation, tutorial interaction, or visual review; no iPhone build was requested.

## 10 October — hide Undo after failure

- [x] Hide the Undo button when a round is lost; keep it during play and at a completed-stage checkpoint where the finished move can still be reviewed.
- [ ] NOT RUN: Unity compilation or device visual review.

## 10 October — map star count

- [x] Remove the multiplication sign before the star total in the map header; keep the star icon and count.
- [ ] NOT RUN: Unity compilation or visual review.

## 10 October — coin economy rebalance

- [x] Halve first-clear and ten-clear milestone income; raise virtual tool and cosmetic prices by 50% while preserving IAP IDs/pack sizes and parcel/replay/ad rewards.
- [x] Add a one-time carryover for older saves and historical-price cloud inventory validation so existing balances and purchases remain usable.
- [x] Record the economic rationale and revised values in `Docs/COIN_BALANCE_OCT10.md`.
- [ ] NOT RUN: Unity tests/compilation, cloud sync round-trip, store sandbox, or device build.

## 10 October — wildlife exits and side-flight arcs

- [x] Side-lane birds and insects now enter and leave beyond the viewport; removed the rig's edge clamp that flattened authored routes into vertical drops. Added varied entry/exit heights and broad inward arcs.
- [x] Added an EditMode regression test for both screen edges and five wildlife styles.
- [ ] NOT RUN: Unity validation/test execution or visual review. The connected Unity Editor points at a different checkout (`/Users/andrew/Desktop/OVERFLOW`); no iPhone build requested.

## 10 October — checkpoint ribbon timing

- [x] Let the stage-clear ribbon complete a slower 1.65-second reveal on intermediate stages; the checkpoint state keeps it visible until the player chooses to descend.
- [ ] NOT RUN: Unity PlayMode or visual timing review; the connected editor targets a different checkout. No iPhone build requested.

## 10 October — mostly-coin loot boxes

- [x] New parcel recipe is mostly coins: 44 coin boxes, 8 shop-tool boxes and 2 very-rare cosmetic trophies across the 54-box campaign (81.5% / 14.8% / 3.7%). Coin drops grant 12, duplicate items convert to 20. Existing receipt recipes remain valid and claimed cross-device rewards take precedence. Details: `Docs/LOOTBOX_REWARDS_OCT10.md`.
- [x] Improve the box opening anticipation, pop, reward fly-out arc and overshoot/settle while preserving the durable claim flow and existing celebration effects.
- [x] Add focused reward-distribution, cloud-merge and legacy-claim tests.
- [ ] NOT RUN: Unity compilation/EditMode/PlayMode or visual review. Connected editor targets `/Users/andrew/Desktop/OVERFLOW`; no iPhone build requested.

## 10 October — remove-ads shortcut

- [x] Add a top-right home-screen shortcut to the existing no-ad purchase screen, using the crossed-out advert icon already in the navigation atlas; hide it after ownership.
- [ ] NOT RUN: Unity visual review; the connected editor targets `/Users/andrew/Desktop/OVERFLOW`. No iPhone build requested.
## 10 October — settings, shop, journal, dialogue and completion polish

- [x] Increase right padding for Settings and My Things arrows; reserve caption space beside the arrows.
- [x] Give settings papyrus notes more height, use an existing localized test-shop label, and center journal facts.
- [x] Center the completion ribbon title and metric captions; add Retry while retaining Next level and Map actions.
- [x] Bring Play closer to its arrow on level cards; inset the comic advance button from the speech-paper edge.
- [ ] NOT RUN: Unity visual review and tests. The iOS export did not start: the headless Editor repeatedly lost its Licensing Client and reported `com.unity.editor.headless` unavailable; stopped after the reconnect timed out. `devicectl` also could not initialize CoreDevice to enumerate the paired iPhone. No app was installed from these changes.

## 10 October — rewarded hint cadence

- [x] Require an accepted rewarded video on alternating new hint states; show a video marker on the lamp during those turns. If video is unavailable or declined, grant nothing and preserve free hints. Repeat viewing of the same hint remains free.
- [ ] NOT RUN: Unity EditMode/PlayMode tests and visual review. No iPhone build requested.

## 10 October — checkpoint action label

- [x] Rename the checkpoint button to “Next stage” with Russian and Ukrainian translations; normal in-stage descent stays unchanged.
- [ ] NOT RUN: Unity visual review. Text-only localization change.

## 10 October — wind-driven platform sway

- [x] Lengthen the visual gust envelope and increase deck sway/roll response without changing puzzle mechanics or Reduced Motion behavior; add a PlayMode regression check.
- [ ] NOT RUN: PlayMode test and visual review; the connected Unity Editor is open on `/Users/andrew/Desktop/OVERFLOW`, not this project.

## 10 October — parcel claim and wind surprise

- [x] Separate opening a parcel from claiming its reward; add an optional, verified, once-per-parcel +40 coin rewarded video beside Claim, with EN/RU/UK copy and durable duplicate prevention.
- [x] Remove wind-specific auto/replay tutorials and proactive coach warnings. Keep wind mechanics and deterministic solver-checked transitions unchanged.
- [ ] NOT RUN: Unity EditMode/PlayMode and visual review; the connected Unity Editor is open on `/Users/andrew/Desktop/OVERFLOW`, not this project. `dotnet` is unavailable here, so only diff/localization and source-level checks ran.

## 10 October — receiver target shadow artifacts

- [x] Disable cast/receive realtime shadows only on the small target-symbol meshes; their lit materials retain shape while preventing pixelated dark artifacts on the receiver.
- [ ] NOT RUN: Unity visual review and PlayMode tests; the connected editor targets a different checkout. No build requested.

## 10 October — dead-end hint feedback

- [x] Keep the hint button available for proven dead-end states and show a short localized ribbon: “No winning path. Undo.” / “Тупик. Отмените ход.” / “Глухий кут. Скасуйте хід.” The warning does not consume or count as a hint.
- [x] Add PlayMode and EditMode regression assertions for the feedback, unchanged puzzle state, and unspent hint.
- [ ] NOT RUN: Unity EditMode/PlayMode tests; the connected editor targets a different checkout. No build requested.

## 10 October — interstitials between failed-level retries

- [x] Count only manual Retry actions on the fail screen; alternate eligible interstitials after 2 then 3 retries, session-only.
- [x] Preserve consent/readiness, no-ads ownership, 180-second foreground cooldown, and two-show session cap; retries continue immediately when an ad cannot show.
- [x] Add cadence and AdService policy regression tests, including failed-ad retry behavior; document the monetization decision in `Docs/TOUCH_AND_STORES.md`.
- [ ] NOT RUN: Unity EditMode/PlayMode tests and device ad validation; the connected editor targets a different checkout. No build requested.

## 11 October — Unity startup/compilation disk-pressure follow-up

- [x] Opened the actual `CascadeUnity` project in Unity 6.3 and inspected its live Console: 0 errors, 0 warnings, 1 informational Analytics message.
- [x] After Unity regenerated the ignored `Library/Bee` cache and rebuilt script assemblies, `Editor.log` contains no current C# compiler errors, disk-full failures, missing-Input-System-type errors, or missing-artifact errors; 199 script assemblies are present.
- [x] Disk space recovered (121 GiB free at last check). The earlier asset-database/Bee failures were caused by the machine running out of storage.
- [ ] NOT RUN: player build, EditMode/PlayMode suites, and visual gameplay QA. Unity's Editor.log still records entitlement-group 404 messages from its Licensing client; these did not appear as Console errors in the live project and are outside the game's C# compile diagnostics.
- [ ] The reported `VirtualArtifacts/Primary/...` path is absent from this project and remains a separate unresolved preview/file reference.

## 11 October — restrained phone tilt and shake response

- [x] A gradual lateral phone tilt now gives one bounded lean and rearms only after returning near neutral. Shakes stay responsive but cap angular velocity; seated pieces rock around their planted feet and settle without vertical bobbing. No sound, haptics, row launch, or puzzle-state change.
- [x] PASS: `PhoneShakeKeepsPiecesSeatedAndRespectsReducedMotion` — virtual accelerometer shake plus gradual tilt, held-angle latch/rearm, bounded seats, unchanged puzzle hash, silent output, no haptic request, and Reduced Motion. Unity Console: 0 errors/warnings. Evidence: `Reports/PhoneTilt-Oct11-Final/playmode.xml`.
- [ ] NOT RUN: iPhone build/device feel; no build or installation was requested.
- [ ] Separate wind check `GustMovesDeckAndRopesMoreThanAmbientWithoutChangingThePuzzle` measured 2.81° floor roll against its 2.4° assertion. Floor-roll code was not changed as part of this piece-motion fix.

## 11 October — victory screen alignment and Mika opacity

- [x] Vertically center the victory headline on its ribbon; raise the score label and digits within the plaque while retaining clear separation.
- [x] Render the multiplication mark smaller than the reward amounts and use one shared digit height for both reward counts.
- [x] Enlarge the reward art and stat medallions without stretching them; raise and modestly enlarge Mika.
- [x] Keep Mika at full opacity while the menu's entrance `CanvasGroup` fades in, so her color does not wash out during the transition.
- [x] PASS: isolated `FiveStarReview` runtime review and captures at 390×844, 360×648, 320×693 (Large Text), and 1440×900. Added assertions for score placement, equal reward digit height, smaller ×, undistorted stat icons, and Mika's parent-fade isolation. Unity Console: 0 errors/warnings. Evidence: `Reports/FiveStarResults/result-ru.png`, `result-ru-360.png`, `result-uk-small.png`, and `runtime.txt`.
- [ ] NOT RUN: full Unity Test Runner suite, iPhone build, and device review.

### 11 October — natural character names, story dialogue and illustrated menu counters

- [x] Replaced displayed cast names in EN/RU/UK: Martha / Марта, Walter / Вальтер, Ben / Бен, Rose / Роза, Nora / Нора, Clara / Клара, Oscar / Оскар, Alice / Алиса, Grandma Ada / Бабушка Ада, Felix / Феликс. Checked inflected RU/UK forms; portrait IDs, receipt IDs and save schemas are unchanged.
- [x] Rewrote all 72 world-prologue lines as nine connected scenes with individual motives, mistakes, humour and consequences. Polished empty island payoffs/banter and all mode arrival/return exchanges. Existing practical mid-island conflicts retained. Editorial rationale: `Docs/STORY_MENU_OCT11.md`.
- [x] Between-stage caption: Next / Далее / Далі, through the existing `Next stage` localization key.
- [x] Menu number displays reuse `VictoryDigitsOct11` and `VictorySymbolsOct11`: wallets, tool stock, prices, coin packs, journal facts and map/icon counters. Small multiplier, proportional fitting, light outline, refitting on rectangle resize. Page indicators remain ordinary readable text. Existing source values remain authoritative; prices/rules unchanged.
- [x] PASS: 56 actual pure C#/NUnit tests via `Tools/check_parallel_story.py`; source TSV shape, unique keys and absence of legacy display names verified.
- [x] PASS: native Unity compilation; Console 0 errors / 0 warnings. `StoryMenuReviewOct11` captured Shop, Supplies, CoinPacks, Journal, all nine residents in comics and a map conversation in EN/RU/UK (390×844; Ukrainian Large Text 320×693). Counter artwork/input/containment and all three between-stage translations checked. Representative PNGs visually inspected. `Reports/StoryMenu-Oct11/runtime.txt`.
- [ ] NOT RUN: full native TestRunner suite, iPhone build/device installation. Current request did not ask for a new build. Native review uses an isolated temporary profile, not the player's save.

### 11 October — retain heroine name

- [x] User correction: heroine remains Mika / Мика / Міка. Restored name and its inflected forms throughout EN/RU/UK dialogue and menu copy; other cast names remain as revised. Source/portrait/save IDs unchanged.

### 11 October — victory reward/statistic gaps and claim-button clipping

- [x] Lowered the record label toward the score while retaining a gap. Shifted both reward illustrations right, reduced reward digit height equally and brought ×1 closer to its lantern.
- [x] Statistic medals have separate horizontal columns and visible vertical space before numerals. Captions remain below the numerals.
- [x] Fixed the layout cause of the clipped CTA: `MenuBottom` retains its requested height instead of always overwriting it to 48. Victory primary action stays 64 physical UI pixels tall; claim action spans 80% of the safe width, with padding, no wrapping and fitted localized typography.
- [x] PASS: native `FiveStarReview`, including bounded one-line claim caption and at least 6 rendered pixels between statistic icons and their numbers / adjacent columns. Checks run inside the capture at its actual viewport size (camera-space Canvas cannot be measured by world-space scale alone).
- [x] PASS: visually inspected fresh RU 390×844, RU 360×648 and Ukrainian Large Text 320×693 frames in `Reports/FiveStarResults`; EN wide 1440×900 captured and checked. Receipt, score and puzzle hash preserved. Final native Console has no errors/warnings.
- [ ] NOT RUN: iPhone build/install, full TestRunner suite, physical-device rendering. Isolated temporary QA profile only.

### 11 October — compact claim action and visible Mika

- [x] Reduced the victory primary button height from 64 to 56 UI units and claim width from 80% to 72% of the safe area. Kept the localized caption on one line with padding.
- [x] Raised Mika by 28 UI units without enlarging or dimming her; head, raised lantern and upper torso are visible above the button.
- [x] Visually inspected native Unity RU 360×648 and Ukrainian Large Text 320×693 captures; text remains inside the compact button and Mika is visible. Native FiveStarReview passed across 320/360/390 and wide viewports; final Console has no errors/warnings. Temporary QA profile cleared.
- [ ] NOT RUN: iPhone build/install or physical-device rendering.

### 11 October — victory illustration hierarchy

- [x] Reduced the ribbon headline from 35 to 27 UI units, retaining centered, single-line fitted localized text. Expanded the three-star row from 19% to 27% width per star, preserving visible gaps.
- [x] Reward illustrations are smaller than stars and larger than the statistic medals. Reduced statistic medal scale from 1.18 to 0.90. Made the score plaque shallower to clear the enlarged stars while retaining score-digit height.
- [x] PASS: native FiveStarReview checks the rendered aspect-fitted hierarchy (stars > both rewards > all statistic medals), text bounds and gaps at 320/360/390 portrait and 1440 wide. Visually inspected fresh RU390 and Ukrainian Large Text320 frames; final Console has no errors/warnings. Temporary QA profile selection cleared.
- [ ] NOT RUN: iPhone build/install, physical-device rendering or full TestRunner suite. Upper ribbon safe-area overflow identified in the preceding inspection is still outstanding.

### 11 October — strengthen reward/statistic size contrast

- [x] Slightly enlarged coin and lantern illustrations; reduced statistic medal scale from 0.90 to 0.80. Stars remain largest. Both reward illustrations exceed every statistic medal by at least 30% in their rendered aspect-fitted span.
- [x] PASS: native FiveStarReview across 320/360/390 portrait and 1440 wide; fresh RU390 and Ukrainian Large Text320 frames visually inspected. No Console errors/warnings; temporary QA profile selection cleared.
- [ ] NOT RUN: new phone build/install, full TestRunner suite or physical-device rendering.

### 11 October — larger reward art and closer statistic icons

- [x] Enlarged coin and lantern illustrations by 20% and retained clear gaps beside ×40 / ×1. Expanded the star row to retain stars as the largest illustrations.
- [x] Lowered Time / Moves / Undos medal anchors by 0.15 of their metric-row height, bringing them close to the numerals with at least 6 rendered pixels of space in checked viewports.
- [x] PASS: native FiveStarReview at 320/360/390 portrait and 1440 wide; visually inspected fresh RU390 and Ukrainian Large Text320 frames. Receipt/state unchanged; final Console has no errors/warnings; temporary QA profile selection cleared.
- [ ] NOT RUN: new phone build/install, physical-device rendering or full TestRunner suite. Upper ribbon safe-area overflow remains outstanding as previously documented.

### 11 October — safe-area victory layout and smaller statistic digits

- [x] Removed the 48-unit upward ribbon offset that escaped the safe area. Ribbon now sits 8 UI units inward, with a shallower height; the star, record and reward rows moved down. Rebalanced record caption and number regions inside a shallower plaque.
- [x] Reduced statistic digit cap height from 0.84 to 0.62 of its area (~26% smaller); lowered the three statistic groups by 2.5% of the usable height and preserved their icon-to-number gaps. Adjusted caption spacing and Mika position to keep the lower composition clear.
- [x] Replaced the previous reversed ribbon/star gap assertion with checks of the actual safe-area ceiling and separate ribbon/star and star/plaque screen-space gaps. Native FiveStarReview passes at 320/360/390 portrait and 1440 wide with simulated insets and Ukrainian Large Text; fresh RU390 and UK320 frames visually inspected. Final Console has no errors/warnings; temporary QA profile selection cleared. Previously documented upper ribbon safe-area overflow is resolved.
- [ ] NOT RUN: new phone build/install, physical iOS/Android rendering or full TestRunner suite.

### 12 October — restore record size and tighten the star row

- [x] Restored record-plaque height from 13% to 16.5% of usable screen height, placing most of the added height upward into the former gap. Restored the earlier score glyph placement and cap-height ratio; the caption has room to use its intended font size.
- [x] Moved outer stars toward the center, retaining their size. Raised the star row by 1% of usable height; reduced visible ribbon/star and star/record gaps while preserving the inward top safe-area margin.
- [x] Native gap checks now measure sprite silhouettes from source alpha bounds with correct aspect fit and imported texture scaling, so transparent PNG margins do not count as visible overlap/gap. PASS: FiveStarReview at 320/360/390 portrait and 1440 wide, including Ukrainian Large Text. Fresh RU390 and UK320 visually inspected; final Console has no errors/warnings; temporary QA profile selection cleared.
- [ ] NOT RUN: phone build/install, physical-device rendering or full TestRunner suite.

### 12 October — subtle star and record-plaque size adjustment

- [x] Reduced star dimensions by ~3.3%, preserving centers and the tighter row. Increased the record plaque height by ~3% upward from its fixed lower edge; adjusted glyph cap-height ratio to retain the existing score-digit size.
- [x] PASS: FiveStarReview at 320/360/390 portrait and 1440 wide, including Ukrainian Large Text, safe-area and visible-silhouette gap checks. Visually inspected RU390, short RU360 and UK320; final Console has no errors/warnings; temporary QA profile selection cleared.
- [ ] NOT RUN: phone build/install, physical-device rendering or full TestRunner suite.

### 12 October — sequential hollow-star fill, all counters and real half ratings

- [x] All three hollow stars arrive one by one; gold fills only after they settle, left to right, using a continuous horizontal fill. Unawarded stars stay hollow. Score, coins, lanterns, time, moves and undos all count from zero to the committed receipt using the same eased clock. Reduced-motion and background-clock behavior retained. Confetti removed from the victory ceremony where it obscured the fill; parcel confetti retained.
- [x] User explicitly requested real 1.5 / 2.5 ratings. Added version-3 exact half-unit grading and additive omitted-by-default half flags to completion/best/replay receipts. Old values stay intact. Half stars appear on result, map and level cards; totals include the illustrated decimal point. Replay income remains a bounded maximum per node (one coin per half-star), never an animation grant. Updated cloud merging and the actual PublishedCatalog replay verifier. Design note: `Docs/VICTORY_ANIMATION_OCT12.md`.
- [x] PASS: 17 targeted actual C#/NUnit cases, including genuine three-stage 1.5 and 2.5 witnesses, save/load, legacy JSON, bounded duplicate replay income, cloud merge and published replay verification/forgery rejection. Broader earlier selection: 100 pass / 7 fail; reconstructed pre-task rating-only baseline: 84 pass / the same 7 fail messages and values. Existing stale five-star/economy cases are not represented as passing. See `Reports/HalfStar-Oct12/validation-notes.md`.
- [x] PASS: native animation phase/monotonic counter/immutable receipt checks; captured 21-frame GIF. Genuine persisted 2.5 native RU390/UK320 result and map/card checks passed. Latest final-layout review passed at 320/360/390 and 1440 wide. MCP switched to another open project; continued via the original CascadeUnity window and added native review menu commands.
- [ ] PENDING: a fresh capture after the final small all-empty hold extension (fill now begins at 1.50 s) and confirmation of temporary-profile cleanup. Mac locked during the last preparation; user asked to unlock. Added automatic QA-profile cleanup on return to Edit Mode/domain reload to prevent leaving the editor on a test save.
- [ ] NOT RUN: Cloud Code deployment/SDK module rebuild (cloud disabled; .NET9 SDK unavailable), live cross-device sync, device build/install or full native TestRunner suite. Account replay verifier itself was compiled/executed with actual source/content and a local Newtonsoft JSON utility.

### 12 October — illustrated numerals across game UI and atlas edge cleanup

- [x] Reuse the existing gold, dark-outlined 0–9 and number-symbol art for standalone counters and digits embedded in localized TMP labels across the shared safe-area canvas. Preserve each original localized string and layout advances; keep the compact island completed-level badge (for example `4 / 12`) in its current map style as requested.
- [x] Let result counters allocate only the glyphs they need, hide stale glyphs when a source count clears, and keep long counts fitted to their label rectangle.
- [x] Share alpha-trimmed, 12 px guttered, full-rect crops for ambient-cosmetic catalogue icons and drifting trophy leaves, preventing atlas-neighbor bleed in both places while keeping the art centered.
- [x] Expanded `StoryMenuReviewOct11` to check illustrated labels and TMP overflow in shop, supplies, coin packs, journal, island level rows and gameplay HUD in EN/RU/UK, including Ukrainian Large Text.
- [x] PASS: compiled the current Domain, Solver, Content, Application, Persistence, Presentation and Editor.Playable source sets with Unity 6.3 Roslyn into temporary outputs. No compile errors. Existing warnings remain in unrelated presentation/build scripts. `git diff --check` passed for the affected files. Evidence: `Reports/IllustratedNumerals-Oct12/validation.txt`.
- [ ] NOT RUN: import/compile inside the live Unity Editor, execute the expanded runtime review, inspect its screenshots, full Unity Test Runner, device builds or device rendering. The Mac is locked and the connected Unity MCP is attached to another project; the runtime review code compiles, but its visual/overflow assertions have not been executed.
- [ ] Next native review: unlock the Mac, open the CascadeUnity Editor, run `StoryMenuReviewOct11` on its isolated QA profile, then inspect portrait 320/360/390, wide, and Ukrainian Large Text outputs before treating layout as visually verified. Design note: `Docs/ILLUSTRATED_DIGITS_OCT12.md`.

### 12 October — opt-in morning/evening local reminders

- [x] New profiles keep reminders off until the player enables them; request OS permission only after that choice. No remote push/token is used. When backgrounded with consent, queue 09:00 and 19:00 local reminders for the next 14 days; refresh the rolling queue after the player returns. Rotate short EN/RU/UK greetings by scheduled day; some evening notices include the locally saved count of cleared paths. Opening the app or switching reminders off removes only CASCADE's queued/delivered notifications. Privacy copy now explains the on-device schedule and lock-screen progress count. No save schema, package or gameplay IDs changed; design note in `Docs/LOCAL_REMINDERS.md`.
- [x] PASS: Application and Presentation compile with Unity 6.3 Roslyn for Editor, iOS and Android conditional paths; 16 targeted actual NUnit cases pass under Unity Mono; 9 new locale keys resolve in EN/RU/UK; policy JSON parses; `git diff --check` passes.
- [ ] NOT RUN: Unity Editor import, OS permission prompts, scheduled delivery/reboot/time-zone behavior on physical iPhone/Android, mobile builds/install or full Unity Test Runner.
- [ ] RELEASE INPUTS: legal contact is now set; still need domain/public URLs; final app display name and registered bundle/package IDs; launch regions and age audience; store listing approval, product registration/prices and production AdMob IDs; Unity Player Accounts Client ID and verified production project/environment; secure Apple/Google signing setup; completed account/data-deletion path; signed-off physical-device release QA. Store/app IDs, ad IDs and public URLs remain blank.


### 12 October — publisher contacts in website and game legal copy

- [x] Set publisher `Andrii Plashevskyi`, country `Canada`, postal contact `173 Victor Lewis Dr, Winnipeg, MB R3P 1Z9, Canada`, and privacy/support email `shapeinc25@gmail.com` in `Configs/website.json`. Rebuilt the static website and the matching in-game Privacy Policy and Terms from the shared legal source.
- [x] PASS: `node Website/scripts/build.mjs`; `node Website/scripts/check.mjs` (6 pages, 153 links/assets/fragments, legal-document parity). Verified the exact contact details in website and in-game legal text. Documents remain draft/unapproved and non-indexed; no domain/public URL or legal approval was invented. No deployment performed.
- [ ] Unity Cloud Dashboard: Mac was locked and could not be opened, so remote project/service status was not inspected or changed. Local config points to project `a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`, environment `production`; cloud/player-account/purchase-verification flags remain off.

### 12 October — verify Unity Cloud Code packages

- [x] Confirmed through the connected Unity Package Manager for the actual `CascadeUnity` project: `com.unity.services.cloudcode@2.10.3` and `com.unity.services.deployment@1.7.2` are already installed. The Deployment package is currently present transitively; no package changes were needed. Temporary installer bootstrap removed.
- [x] PASS: Unity MCP resolved project root to `/Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/CascadeUnity` and Package Manager returned both packages as installed. No cloud module was deployed and no Dashboard settings were changed.

### 12 October — Cloud Save activation check

- [x] Confirmed local packages already include Cloud Save `com.unity.services.cloudsave@3.4.0`, Cloud Code `@2.10.3`, and Authentication `@3.6.1`. Gameplay sync uses the server-authoritative `CascadeAccount` Cloud Code module, which stores data through Cloud Save; the client does not write player data directly.
- [x] Confirmed project link in `ProjectSettings.asset` is `a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`. Local gates still disable cloud (`enabledForPlayers: 0`, `playerAccountsConfigured: 0`); the Player Accounts Client ID is empty and deployment/verification flags are false. The game will not attempt production sync even if the Dashboard service toggle is enabled.
- [x] Existing offline validation: 43 client/config checks and 80 Cloud Code boundary checks against fake SDK transport; source compiled and module published locally. These do not verify live Cloud Save.
- [ ] LIVE CLOUD NOT VERIFIED: Unity MCP app-server could not initialize (`No space left on device`); host volume has about 115 MiB free. Dashboard status, module deployment, and authenticated read/write smoke test remain unverified. No production data or cloud settings were changed.

### 12 October — reuse ShapeMania UGS service account

- [x] Searched the current shell environment, Cascade config, standard UGS CLI config locations, and likely deployment env-file names under Desktop without printing or opening secret values. No UGS service-account credentials/config were present; Cascade has only the deployment `.env.example`.
- [ ] NOT RUN: service-account authentication or Cascade project role check. Unity MCP is currently unavailable because the host disk has about 114 MiB free; no account access or project permissions were changed.

### 12 October — live visual check of illustrated numerals

- [x] Connected to the active Unity 6.3 Editor through Unity MCP and confirmed the open project is `CascadeUnity`, scene `CascadePlayable`. Prepared and removed a disposable QA profile; no production save or purchase was touched.
- [x] Captured current Unity renders for victory, shop tools, coin packs, supplies, map, island levels and gameplay HUD. `FiveStarReview.Run()` passed its native layout checks, including the 1800 score, themed ×40/×1 rewards and 0:33 time, fitted victory text, star spacing, Mika visibility, and absence of redundant bag guidance. Unity Console had no errors after the review.
- [x] Evidence: `Reports/FiveStarResults/result-ru.png`, `shop-ru.png`, `bag-ru.png`, `Reports/IllustratedNumerals-Oct12/coin-packs-ru.png`, `fresh-map-ru.png`, `fresh-levels-ru.png`, `fresh-game-hud-ru.png`; fresh map/HUD review note at `Reports/IllustratedNumerals-Oct12/fresh-review.txt`.
- [x] Exited Play Mode and removed the exact temporary QA profile. `CascadeGame.EditorProfileDirectory` is clear.
- [ ] NOT RUN: Dashboard activation/deployment. Chrome’s saved Unity passkey reaches a macOS Touch ID prompt, which timed out without user confirmation. UGS CLI `status --json` also remained running for 20 seconds without reporting a result; no credentials were exposed or cloud settings changed. Cloud gates remain disabled pending authenticated access and live checks.


### 12 October — numeric glyph alignment and clipping

- [x] Re-cropped the gold digit sheet using measured glyph bounds scaled to Unity imported texture dimensions, plus even gutters. This prevents clipping and neighboring atlas pixels. Added width allowance for outlines.
- [x] Map best scores use a period grouping mark; comma counters use the period glyph; slash/minus dividers are larger; platform durability 1 is 12maller. The shop purchase-limits explanation remains plain text. No prices, rules, packages, IDs or save schema changed. Design note: Docs/ILLUSTRATED_DIGITS_OCT12.md.
- [x] Unity MCP native result/shop review passed after crop scaling; screenshots checked at 390 and 360 widths, shop copy stayed plain, and no review errors were logged.
- [ ] NOT RUN after final separator-size and width-inset edits: Unity recompile, fresh map/coin-pack/HUD captures, full Test Runner. MCP command compilation failed writing a temporary DLL because the host disk was full. No phone build or device check.

### 12 October — owned trophy collection and shop preview

- [x] The Trophies collection now lists owned trophies only; unowned catalogue items remain available through the Shop. The empty collection state has a short message.
- [x] Added a localized Preview button below the selected trophy's purchase/action button in the shop. It opens the full-colour appearance view with the cosmetic effect active, without equipping or granting the item; back returns to the shop.
- [x] Preserved existing catalogue IDs, prices, save data and ownership rules. Design note: `Docs/SHOP_TROPHIES_AND_FIELD_TOOLS.md`.
- [x] PASS: `git diff --check`; static checks confirm the shop route, full-colour preview path and EN/RU/UK localized labels. Updated `ScreenCosmeticFlowTests` for owned-only collection and shop-preview behavior.
- [x] Removed five older generated exports, four old Xcode `DerivedData` folders, and the obsolete 1.37 GB unstripped archive; kept compact diagnostics. A Unity iOS export (`iPhone-20261012-095759`) was briefly created during the previous turn, then fully deleted after the user instructed that no phone build should remain. `CascadeUnity/Builds` now contains only `.DS_Store`; no iPhone build/export remains.
- [x] Xcode could not finish the deleted export: automatic signing had no Apple account/provisioning profile, and unsigned `ibtool` reports the iOS 26.5 platform is not installed. Kept both Xcode logs in `Reports/iPhoneInstall-TrophyPreview-20261012`.
- [x] PASS: Through Unity MCP Play Mode, verified the empty Russian collection shows no item cards and its localized empty-state copy; an unowned shop item's Preview opens an opaque/full-colour Mika view without changing coin balance or equipped style; Back returns from preview to the shop grid and then to the collection. Russian shop root and detail screenshots were visually checked at 390x844: `Reports/TrophyPreview-MCP/shop-root-ru-final.png`, `shop-detail-ru-final.png`.
- [x] PASS: Latest clean-profile run exited Play Mode with the temporary profile cleared and Unity Console at zero errors/warnings. `git diff --check` passed for affected trophy-flow files and this task record.
- [x] QA note: an earlier exploratory profile used a hand-seeded parcel without matching clear-history and failed save validation at shutdown. That disposable profile was discarded; it did not touch the production save. The final runtime pass used a clean profile and made no synthetic reward changes.
- [ ] NOT RUN: Unity Test Runner did not yield a result file through its Editor API. A broad Play Mode run was stopped after unrelated `JourneyBrowseTests` profile-teardown validation errors; those are not counted as trophy-flow results.
- [ ] NOT RUN: Phone build, installation or device rendering, per the user's latest instruction. No app was uninstalled and no device save was touched.

### 12 October — shop counter and purchase button cleanup

- [x] Reduced only the `7` in the shop trophy pager, shifted the shop coin wallet left, and optically enlarged only the wallet `8`. Fixed the numeric counter to recalculate when its label's layout rectangle changes, so the selected item's coin price appears after layout settles.
- [x] Removed a stale illustrated-number overlay when a localized price caption becomes ordinary text (`Buy` / `Купить`); this had rendered the word as tiny gold character fallbacks. Empty `0 / 3` fragment badges stay hidden.
- [x] PASS: Unity MCP RunCommand compilation and fresh Play Mode review. Visually checked Russian shop root, wallet `×8`, Mika preview, and coin packs at 390×844. Runtime inspection confirms `Купить` white, price `× 360` present, no stale counter, and no `0 / 3` badge. Unity Console had zero warnings/errors on the clean review run. Evidence: `Reports/ShopNumberFix/verified-root.png`, `verified-wallet-x8.png`, `verified-preview.png`, `verified-coins.png`, `verification.txt`.
- [x] No price, rule, package, schema or ID changes; no phone build. Temporary review profile is cleared on exiting Play Mode.

### 12 October — result hierarchy, readable banners and optional stamp choice

- [x] Restored the intended result-screen hierarchy: stars are largest; the coin and lantern reward art is larger than each Time / Moves / Undos medal; the three statistic medals remain the smallest. Reduced only the illustrated multiplication sign beside reward counts. The trophy button keeps its previous height.
- [x] Replaced the hazy stretched result Mika with a transparent authored-proportion cutout, moved her upward with uniform scaling, and rendered her above the warm screen wash while keeping her behind the result UI.
- [x] Extended reaction copy long enough to read: ordinary notices 3.0 s, tool/effect notices 4.2 s, wind 2.35 s, reduced-motion notices 2.6 s, and the rewarded-coin toast 3.7 s.
- [x] Stamp practice is guidance-only. Any otherwise legal wrong swap remains accepted and simply leaves the optional stamp bonus uncollected. Added a Play Mode regression covering that exact choice; the shared resolver rule remains unchanged.
- [x] PASS: Unity compile/import; native `FiveStarReview.Run()` and victory animation review; fresh `Reports/FiveStarResults/result-ru.png` visually inspected. Pure Application runtime check confirms the wrong stamp choice is accepted with `StampBits == 0`; banner runtime check confirms a 3.0 s generic hold remains visible after 1.25 s. No build or device install was made.
- [ ] NOT RUN: full Unity Test Runner or physical-device rendering. The new stamp Play Mode test compiled but was not executed by Test Runner in this pass.

### 12 October — final victory layout and aligned next-level actions

- [x] Kept caption and score inside the teal plaque, raised the score slightly to balance the caption/number and number/lower-edge spacing. Moved each reward illustration together with its amount to the right while preserving stars > rewards > statistic-icon hierarchy. Removed the redundant uGUI Outline from the already outlined multiplication artwork and added explicit space before its digit.
- [x] Replaced the stretched, hazy Mika asset with a transparent compact cutout. Found and fixed the actual importer distortion: NPOT ToNearest was resizing the source to 1024x1024; NPOT None preserves 1173x1341. Image preserves aspect with uniform scaling, full white tint, and renders above the sun wash.
- [x] Next-level Retry and Map actions now share the main button's 62-unit height, 24-unit bottom offset and visual center; enlarged icon frames and pictograms. Claim-trophy state continues to have no side actions.
- [x] PASS: Unity compilation and native layout review across small/portrait/wide cases; fresh animation review confirms sequential hollow arrivals, delayed left-to-right fill, monotonically increasing score/reward/stat counters, exact final values and unchanged receipt/state. Animation lasts 4.85 s, with count-up over 2.30 s and small settling pulses. Earlier animation check on a profile already moved to another level had no ceremony and was discarded; the clean prepared-profile rerun passed.
- [x] PASS: final runtime footer-center/height assertions and visually inspected fresh claim and next-level renders at 390x844 and 320x693 after the import fix. Evidence: `Reports/FiveStarResults/final-claim-ru.png`, `final-next-390.png`, `final-next-320.png`, and `animation-runtime.txt`. Final Play Mode Console contained no errors/warnings; exited Play Mode.
- [ ] NOT RUN: new device build/install, physical-device rendering, or full Test Runner. The phone's installed version is not asserted to include these source changes.

### 12 October — restore only multiplication and point contours

- [x] Added a thin dark contour traced from each illustrated symbol's alpha silhouette, including multiplication, colon and period. Extended only the symbol rendering mesh into its transparent gutter so diagonal tips are not clipped flat into a rectangular outline. Symbol layout rectangles, original gold artwork size, numeral spacing, result layout, Mika and animation timings are unchanged.
- [x] Unity compiled the C# and contour shader. Fresh isolated-profile native result capture: `Reports/FiveStarResults/symbols-contour-ru.png`; no phone build/install or full Test Runner.

### 14 October — subscriptions and unrestricted help

- [x] Added week/month/year remove-automatic-ads subscriptions, store-localized price slots, a dedicated Russian/English/Ukrainian offer screen, and server-verified expiry. The old permanent purchase remains valid for existing owners. Product IDs and store setup are in `Docs/STORE_PRODUCTS_SUBSCRIPTIONS.md`.
- [x] Design note: using a hint or owned help item no longer caps level stars. Stars still reflect moves, time, undos, and board damage; prior earned receipts are not rescored. Removed obsolete two-star and two-tools copy.
- [x] Backend receipt verifier tests pass (15/15). The Unity MCP calls currently time out despite an installed, approved relay. Unity compilation, screen comparison, live store sandbox purchases, and device tests remain unverified.

### 14 October — gameplay help modal reference correction

- [x] Compared the actual help screenshot with the supplied Russian reference. Replaced the narrow gold/green look with a full-width warm parchment panel, matching title plaque, larger 3×2 cards and stock pills, a wide description scroll, and wider buttons. The empty state now shows only the generic instruction; selecting a card shows its title and description.
- [x] Added isolated modal art for the lantern, key presentation, and pickaxe, while keeping the existing six inventory IDs and their actions. The visual key and pickaxe still represent the current Landing Lens and Repair Kit mechanics; those game rules were not changed in this UI pass.
- [x] Prevented help-tray layout recalculation during scene teardown, which had produced a `NullReferenceException` when Play Mode exited with the tray open.
- [x] Opening the help tray and switching UI locale now refreshes its heading, buttons and selected-item copy together; verified EN → RU while the tray stayed open.
- [x] PASS: compiled and rendered the updated screen in an isolated Unity project at 720×1280 and 320×693; visually checked empty and selected states. Evidence: `Reports/GameplayReferenceOct14/field-help-final-720.png`, `field-help-final-selected.png`, `field-help-final-small.png`. Unity's Search indexer raised an unrelated startup `ArgumentOutOfRangeException`; there were no gameplay compile errors or tray teardown exceptions in the final run.
- [ ] NOT RUN: physical phone build or in-app purchase flow; Unity MCP remains unavailable in this session.

### 14 October — grounded Mika compositions

- [x] Restored a separate stone floor under the seated Mika, bag and lantern on Pause. The world sky shader had replaced the original backdrop's baked floor, leaving the whole scene suspended on some aspect ratios. The floor stays in the reference stage and follows the existing world lighting.
- [x] Added visible landing ledges under standing Mika in the journal, journey ending, account entry and empty trophy collection. Existing home, inventory and owned trophy screens already had supports; the failure pose is on its rope-suspended bench, and the map character remains attached to its island. Removed independent bobbing from the newly supported standing Mika poses.
- [x] Fixed the empty trophy paper/layer order and caption position, and shortened the account message in EN/RU/UK so it stays inside the parchment.
- [x] PASS: local Unity 6000.3.8f1 batch compilation and fresh graphics capture from a disposable profile. Visually checked Pause at 720×1280, 390×844 and 768×1024, snow/night Pause at 390×844, Journal, JourneyEnd, AccountGate (RU/UK), and the truly empty Collection at 390×844. Evidence: `Reports/MikaGroundingOct14/`. `git diff --check` passed.
- [ ] NOT RUN: physical-device rendering and full Unity Test Runner. Unity MCP transport was closed, so this review used a local batch Editor clone. The Unity Search indexer still logs its unrelated startup `ArgumentOutOfRangeException`; no gameplay C# compile error appeared in the final log.

### 14 October — reskin the first-world framing in every world

- [x] Replaced world-specific column/tower borders with the original first-world map, menu and gameplay silhouettes. All worlds now share the same frame geometry and gameplay mesh slicing. Removed per-world arch clipping and 40/120/45-unit frame zoom/bleed offsets; preserved the old generated assets on disk.
- [x] Added one transparent 4×2 atlas of compact thematic corner details through built-in image generation. The shared shader changes materials and corner detail RGB inside the original frame alpha. World skies, lighting, atmospheric life and the approved island layouts remain thematic. Source, saved asset path and exact prompt are in `Docs/WORLD_FRAME_RESKIN_OCT14.md`.
- [x] Decorative menu framing now reaches the physical canvas edge; safe areas continue to contain controls. Restored the existing boardwalk extension and applied the same seasonal material to the platform/floor pieces, removing sky gaps and mismatched paving colours.
- [x] PASS: current source compiled in the live Unity Editor; the native review ran through Unity MCP. Compared fresh settled maps for all nine worlds, plus menus, inventory, Pause and gameplay. The review wrote 39 captures and checks equal map geometry, identical gameplay mesh/source art, physical frame coverage, non-blocking decorative frames and all four bridge attachments. Evidence: `Reports/FrameReskinOct14/verification.txt`, `all-worlds.png`, and individual screenshots.
- [x] QA: the Editor initially executed an older loaded review assembly. Those intermediate captures were overwritten after reimporting and confirming the current compiled assembly. Final evidence includes the new viewport assertions, inventory and Pause captures.
- [ ] NOT RUN: physical-device build/rendering, full Unity Test Runner and a device performance profile. This pass does not claim a measured device frame rate.

### 14 October — polish inventory floor seams, frame cutouts and card alignment

- [x] Replaced the repeated floor strips in Inventory, Collection and Pause with one continuous viewport-reaching surface. Generated long wooden and sandstone floor assets in the existing art style; themed lighting still applies. Frame details draw above the ground.
- [x] Removed pale sky remnants and rectangularly clipped side foliage from the menu cutout; reused modest complete theme sprigs. Corner accents retain their own alpha and follow menu opacity. Frame architecture and gameplay/map cutouts are preserved.
- [x] Centred the Inventory, Items, Collection, Pause and field-help headings. Inventory title/count boxes share a centre; reduced and centred item icons in inventory, shop, item strip, trophy strip, help and parcel cards. Aligned catalogue stock symbols and numbers.
- [x] PASS: current Unity compilation and native capture through MCP. Visually inspected all nine inventory themes, neighbouring menus, 720×1280 / 390×844 / 320×693 / 768×1024 and EN/RU/UK. Final focused review produced 20 native screenshots and 14 inventory layout checks, with viewport ground coverage/frame-order checks also on trophies and Pause. Final runtime Console has no errors or warnings. Evidence: `Reports/MenuPolishOct14/verification.txt`, `all-worlds.png`, `related-screens.png`; implementation/assets/prompts: `Docs/MENU_POLISH_OCT14.md`.
- [x] Re-ran the existing nine-world frame/bridge review after shared frame alpha changes: 39 native screenshots and 29 checks for source frame geometry, gameplay mesh, viewport bounds and bridge attachment. Evidence: `Reports/FrameReskinOct14/verification.txt`. The subsequent side-foliage cleanup is menu-only and was checked in the final focused captures.
- [ ] NOT RUN: physical-device build, full Unity Test Runner or hardware performance profiling. No economy, purchase IDs, game rules or save schema changes in this visual pass.

### 14–15 October — reference failure composition, solid platforms and screen light

- [x] Rebuilt the failure composition against the supplied 720×1280 reference. Mika and the three silver medallions use source-image pixels with explicit contour meshes; generated the narrow bench, bare action faces and map scroll. Preserved live/localized values, retry/map actions, reduced button greenery and world-specific animated scenery. The source image and generated assets/prompts are documented in `Docs/REFERENCE_FIDELITY_OCT14.md`.
- [x] Kept Mika, bench and suspended results on one moving assembly. Both upper/lower hemp cords follow their actual attachments after suspension updates; upper cords extend to the physical screen edge on tall viewports. Removed the compressed inset failure layout and legacy result controls.
- [x] Closed the longitudinal sky slit between the gameplay plank's opaque crown and load-bearing top. Pieces and contact shadows now use the same visible support height. Ordinary, mirror, roller, wind and durability/fracture presentations remain integrated; no puzzle-state or economy changes.
- [x] Restored original wooden heading geometry across the supplied menu families while retaining themed material/light. Sky clouds no longer receive solid-surface relief lighting, removing sharp colour rims. Fixed blank subscription emblem/calendar/paper loads, including imported texture downscaling, and stale help-shop captions when switching EN/RU/UK.
- [x] PASS: current source compiled in the live Unity Editor and the final isolated-profile review ran through MCP. Final run completed at 00:11 on 15 October: 18 menu/failure captures and 26 gameplay/help/board captures; 18 screen assertions passed. Checked full failure canvas, a single retry/map pair, exact-source medal family, rope attachment errors, nonblank subscription art, text fit and all six help selections in EN/RU/UK with large text at 320×693. Visually inspected final screen/board sheets, tall night failure and small localized layouts. Final runtime Console: zero errors/warnings; Play Mode exited and production scene restored. Removed the temporary Editor review object. Focused tracked-file `git diff --check` passed.
- [x] Evidence: `Reports/ReferenceFidelityOct14/verification.txt`, `all-reference-screens.png`, individual 720×1280 and 390×844 captures; `Reports/ReferenceFidelityBoardsOct14/all-board-states.png`, native failed attempt and 320×693 localized help/failure captures. Static screenshot pixel identity of every world background, animated frame and live value is not claimed; approved themed/less-busy scenery differs from the source.
- [ ] NOT RUN: a new phone/tablet build, physical-device rendering, full Unity Test Runner, store sandbox transaction or hardware performance profile. Existing installed builds are not asserted to include these changes.

### 15 October — interrupted resume/trophy request and economy recheck

- [x] Found the original controls-on-map reproduction and its successful correction: `Reports/ForegroundBaselineOct14/playmode.xml` failed one of two cases; `Reports/ForegroundFinalOct14/playmode.xml` passed both. The latter covers three one-hour suspension cycles after failure/map navigation and a final delivery interrupted by backgrounding with one durable completion.
- [x] Rechecked current pure production C# with 135 actual NUnit cases: 135 PASS, zero failures/skips. Includes repeated help/hints, atomic reward and purchase rollback, replay credits, old rewards, price migration preserving spendable money, cloud journals/conflicts, immutable autosaves, retryable write failures and decoded payload limits. Evidence: `Reports/ResumeEconomyOct15/pure-suite.json`, `pure-suite-summary.json`; reusable runner: `Tools/recheck_resume_economy.py`. This is not native PlayMode coverage.
- [x] Recompiled the actual Cloud Code module and passed 87 boundary assertions on fake SDK transport. Updated obsolete rating/catalog/prize fixtures while retaining ownership, forged proof, duplicate request, two-device and legacy receipt checks. Added new loot persistence/claim/retry checks. Publisher receipt verifier: 15/15 PASS with mock adapters. No live account or store call, deployment, economic rule, price, ID or player-schema change in this recheck.
- [x] Corrected the help policy note to reflect the user's explicit removal of the help star cap. Fixed old tests that demanded five-star new replay credits, an unrelated trophy from two legacy leaf fragments, or unchanged gross income across a price migration. The current checks preserve the relevant replay/claim and spendable-wallet invariants.
- [x] Native resume/trophy verification completed in an isolated temporary source copy via Unity CLI; the primary Editor's modal/unsaved scene was preserved. Final current source passed 15/15 cases, including both foreground scenarios, both trophy-render/resume scenarios, seven loading/lifecycle cases and three Pause cases. Batch fixtures now hold a real portrait render target; the old map-return fixture marks its introduction read and checks the numbered Continue caption. Failed intermediate evidence is retained; final result: `Reports/ReferenceAuditOct15/playmode.xml`.
- [x] Added `ResumeEconomyRecheck` menu hooks and a dirty-scene guard to avoid blocking MCP on this dialog in future. The final guard compiled in the isolated Unity; its guarded menu was not invoked on the primary dirty scene. Details and limits: `Docs/RESUME_ECONOMY_RECHECK_OCT15.md`.
- [ ] NOT RUN: current complete Unity Test Runner, physical-device suspend/resume/rendering, new installed build, live backend deployment or store sandbox transactions. Do not treat the original two passing tests as verification of every subsequent presentation change.

### 15 October — compare supplied reference states and repair shared menu edges

- [x] Compared the ten new reference images and earlier help/Pause/failure/subscription references with native current screens. Kept the green inventory variant, agreed restrained controls/backgrounds, shared first-world framing geometry, thematic world light/details and live player values. Retained the approved owned-trophy magnifier/carousel flow and purchase-only shop.
- [x] Restored a single continuous central floor to all reference menu surfaces, including shop, coins, items and parcels. Ground rebinding updates the existing layer; Items starts at its reference rear edge. Returned the Home no-ads shortcut to its reference logo band and reused the existing blue spool in the shop grid.
- [x] Native comparison exposed clipped stems/foliage in the original side-post matte. Generated one transparent original-style side-post repair, saved unchanged in `WorldFramesOct14/MenuSidePostsOct15.png`; only the middle menu posts blend to it. Original beam, rope corners, lantern, foot supports, map and gameplay source art remain. All nine themes share the same geometry/material workflow. Prompt and provenance: `Docs/REFERENCE_AUDIT_OCT15.md`.
- [x] PASS: Unity 6000.3.8f1 compiled the current source/shaders in a temporary local copy with graphics; 15/15 PlayMode cases passed. The focused reference test then passed again with bridge/frame/floor assertions and actual open/selected help states. Captured and visually inspected 46 native PNGs, including nine maps/inventories/shops, supplied selected/purchased/equipped/parcel states and earlier auxiliary screens. Every capture preserves puzzle state; each map has four attached bridges with error below 0.001. Real one-free-undo sequence activates the owned-spool Apply button. No primary scene save/discard, live account mutation, deployment or device build.
- [x] Evidence: `Reports/ReferenceAuditOct15/playmode.xml`, `reference-final.xml`, `validation.json`, `screens.png`, `world-maps.png`, `world-inventory.png`, `world-shops.png`. Intermediate failed orientation/old-caption fixtures retained. Exact bitmap identity, device performance and inclusion in an installed build are not claimed.

## Requested changes verification — 15 October 2026

- [x] Rechecked every current packaged level with the production resolver: 540 distinct complete triads, 1620 optimal stage certificates, 6471 primary/localized witness replays, 335 live bomb deadlines, 646 benefit ablations, 3292 durability probes, 372 causal entry permutations and 12 causal gusts. Fresh evidence: `Reports/CampaignVerificationOct15/`. No campaign regeneration or claim of human enjoyment.
- [x] Found and completed a remaining unlimited-help gap: removed the five-per-type inventory gameplay cap across purchases, preview, loot resolution, ledger validation and cloud reconciliation. Preserved owned-item consumption, prices, IDs and save field types. Checked/wide arithmetic prevents stock/spending overflow and supports larger displayed totals. Design note: `Docs/UNRESTRICTED_HELP_OCT14.md`.
- [x] Current source passed 179 actual pure C#/NUnit cases, 69 native Unity EditMode cases and 98 Cloud Code boundary assertions using fake SDK transport. New cases cover all six tool types bought above five, durable reload, seven repeated uses, parcel tools above five, concurrent funded purchases and unfunded server stock rejection. Retained prior native lifecycle/map/board/motion evidence; did not claim those were all rerun after this small inventory change. Evidence: `Reports/RequestedChangesVerificationOct15/`, updated `Reports/ResumeEconomyOct15/validation.json`.
- [x] Documented exact policy and verification limits in `Docs/REQUESTED_CHANGES_VERIFICATION_OCT15.md`: one free undo and hint alternation are per attempt/stage; cached hints/earned tokens remain valid; help does not cap stars. Nine world position variations preserve island dimensions. Physics belongs to presentation only. Main Editor unsaved scene preserved; no device build or external deployment; live ads, physical-device FPS and subjective enjoyment remain unverified.

## First Home Play route — 15 October 2026

- [x] Fresh Home Play opens the existing opening comic, then starts the first playable campaign level directly. The closing comic action says Play. Its covered first-island introduction is remembered, without inserting a map/dialogue screen before gameplay.
- [x] The first campaign victory's primary action goes to the map, including any pending legacy next-level destination. Later Home Play opens the map directly; actual unfinished attempts remain pinned, and unread later-world comics wait for level selection. Standalone/replayed comics retain their existing default map callback. No new profile fields, receipt IDs or gameplay rules.
- [x] Native integration caught and fixed a comic Canvas pre-render callback surviving into the direct puzzle rebuild. Layout now unsubscribes when the comic is no longer active. Failed durable comic saves roll back newly read receipts and never start the first level.
- [x] PASS: 7/7 native Unity 6000.3.8f1 PlayMode cases, including five new first-play scenarios and two existing foreground/navigation regressions. Real comic button clicks, all three first-level stages, first victory → map → chosen second level, unfinished-first-level save/reload, returning player, failed save and default comic destination covered. Evidence: `Reports/FirstPlayFlowOct15/`; design/validation: `Docs/FIRST_PLAY_FLOW_OCT15.md`. Isolated temporary profile/project used; primary unsaved scene preserved; no device build/install or backend deployment.

## Four reference settings screens — 15 October 2026

- [x] Rebuilt Settings, Languages, Sound & motion and Comfort & help around the supplied four references. Exact requested visible control sets; removed the previous extra settings/notes/footer. Generated matching landscape, transparent side frame, wooden/cream/gold surfaces and sixteen icons/control states through built-in imagegen. Reused Back art, fonts, button feedback and actual settings/store/legal/notification handlers. Legacy preference fields remain compatible. Sources, saved assets and generation specifications: `Docs/SETTINGS_REFERENCE_OCT15.md`, `Docs/SETTINGS_ASSET_PROVENANCE_OCT15.json`.
- [x] Uniform reference proportions, continuous safe-inset title ropes, independently fitted landscape/frame, centred icon placement and correctly weighted localized text. Native comparison caught and fixed unwanted line wrapping, thin/opening arrows, missing chevron CanvasRenderer and thin captions. All enabled buttons receive actual EventSystem raycasts; expanded touch regions do not overlap.
- [x] PASS: 6/6 native PlayMode cases, including four settings cases and two existing store boundaries. Verified sound/haptics/comfort/help/language application and durable reload, actual legal documents, subscriptions/back, Home/Map/Pause returns, unavailable native actions, unchanged wallet and puzzle. Captured and visually compared 25 native images across four resolutions, all three locales and simulated safe insets with large text enabled. Root current C#/shaders/assemblies match tested copy. Evidence: `Reports/SettingsReferenceOct15/`.
- [x] After the user closed the primary save modal, official Unity MCP verified the current loaded implementation and captured all four pages again at 390×844 using a temporary profile in the primary Editor. Final runtime Console: zero errors/warnings. Stale hot-reload runtime and an Edit Mode probe were restarted/cleaned before the successful capture. Test Play session stopped; no scene save/discard. Evidence: `Reports/SettingsReferenceMcpOct15/`.
- [ ] NOT RUN: physical-device build/install/rendering, native OS notification prompt, real store restoration/transaction or device performance profile. No gameplay rules, prices, IDs, packages or save schema changes; no backend deployment. Bitmap identity is not claimed for generated artwork.

## Welcome, account Home states and reference/header review — 15 October 2026

- [x] Rebuilt the welcome screen from the three new F830DE8D references, reusing the shared wood frame, stairs, logo and feedback. Added five built-in imagegen assets: waving Mika, smiling lantern Mika, blank surfaces/account/provider icons, castle landscape and umbrella flying boat. Native crop/scale preserves proportions; ropes attach to the physical beam; one continuous floor covers the tablet width/bottom without UV distortion. Tall phones keep the content legible. Source PNGs remain unchanged. Asset paths, prompt set, provenance and implementation: `Docs/ACCOUNT_REFERENCE_OCT15.md`, `Docs/ACCOUNT_ASSET_PROVENANCE_OCT15.json`.
- [x] Apple/Google use the installed secure Unity Player Accounts hosted flow, with waiting/cancel/failure and missing-configuration behavior. Guest choice persists and failed saves cannot dismiss onboarding. Home sign-in/sign-out icon derives from committed account state; existing-save choice and recovery controls are reachable on the welcome screen. Loading another account cannot expose its Home state before local profile/journal commit. First Play comic → level one → map remains intact.
- [x] Added optional backward-compatible `CloudJournal.SignedOut` device state. Sign-out preserves local progress, wallet and pending request, pauses background sync/verified purchases until explicit sign-in, survives restart and blocks identity/purchase races. It does not unlink/delete accounts. Actual-source client transport checks: 65 PASS; existing server boundary checks: 98 PASS. No external account/server mutation.
- [x] Corrected the gameplay plaque/lamp offset caused by reserving a hidden extra help button. Verified Pause title/corner alignment and trophy headers/previews, plus all supplied reference states. Added 44px touch coverage, real raycast and uniform-floor assertions. Fixed the legacy Pause Retry fixture to render in portrait rather than trigger the landscape gate.
- [x] PASS: final current source passed 21/21 native PlayMode scenarios. Captured/reviewed 109 native PNGs across four sizes, three locales, large text and safe insets; includes nine-world reference map/inventory/shop states and settings recheck. Root C#/shaders/assemblies match tested copy. Current primary Editor recaptured the new pages and headers through official Unity MCP with a temporary profile; final runtime Console had zero errors/warnings. Evidence: `Reports/AccountReferenceOct15/`, `Reports/AccountReferenceMcpOct15/`. Failed intermediate evidence is retained separately.
- [x] Wrote the exact remaining operator setup in `Docs/ACCOUNT_BACKEND_SETUP_OCT15_RU.md`: UPA Client ID, project/environment/provider callbacks, module/Protected Cloud Save, activation and real-account verification. Public configuration remains valid but inactive; no secrets/IDs/prices/packages were changed.
- [ ] NOT RUN: live Apple/Google authorization, live Cloud Save/cross-device transfer, backend deployment, physical-device build/install, store transactions or device profiling. Linked Home captures are visual fixtures. The existing account/data-deletion release item remains open; sign-out is not deletion. No claim of literal original bitmap identity.

## Release ID handoff, sky footer and regional life — 15 October 2026

- [x] Reconciled actual client catalog, public manifest and server allow-list: six consumable coin packs, three auto-renewing no-ads subscriptions and one legacy restore-only lifetime ID. Wrote `Docs/RELEASE_HANDOFF_OCT15_RU.md` with exact Product IDs, required publisher/account/ad/hosting values, active prices-from-store behavior and full remaining release/operator/backend work. Historical setup links to the new handoff. Current policies still need the subscription-model update/approval; public legal URLs/flags remain incomplete. No IDs, prices, credentials or release-ready flags invented.
- [x] Lifted gameplay actions above the physical sill using shared proportional frame geometry and safe-inset clearance; receiver/precise-input/floors move with them. Side frames and gameplay rules retained.
- [x] Added two built-in imagegen alpha atlases for four regional rigs and nine seasonal flecks. Source PNG bytes preserved, Unity metadata included. Reused existing wildlife ownership and added painted-frame landings; bird/owl/bee/cricket behaviors differ, winter fauna is seasonal, planted feet stay stable and input starts departure. New menu effect pool safely rebinds after Editor reload and never receives raycasts. Generation prompts/provenance: `Docs/WORLD_LIFE_ASSETS_OCT15.json`.
- [x] Batched gameplay weather into one reusable mesh/material, increased visible seasonal detail and distributed sparse effects so they do not disappear behind the same UI. Fixed a reproduced negative-epsilon wing-phase array exception; regression coverage includes the floating-point boundary.
- [x] PASS: final 21/21 native PlayMode scenarios, 34/34 native motion/lighting EditMode cases, 32 new phone/tablet/safe-area captures plus 38 account/header/Pause/trophy regression captures. Nine worlds have visible-pixel weather checks, planted contacts, menu sets and actual touch checks. Current runtime source matches tested clone. Existing server boundaries 98 PASS and receipt-verifier mocks 15 PASS. `Reports/WorldLifeReleaseOct15/`, `Docs/RELEASE_AND_WORLD_LIFE_OCT15.md`.
- [x] Official Unity MCP imported current changes and used an isolated menu-driven review to capture five current main-Editor phone views after dynamic-command discovery was unavailable. Fresh runtime scene, final Console zero errors/warnings, Play stopped, original unsaved Editor scene preserved. `Reports/WorldLifeMcpOct15/`.
- [ ] Still required for release: real atomic first-account/transaction ownership, AdMob SSV, incoming subscription/refund lifecycle notifications, account/data deletion, migration/long-term receipt retention and production deployment/monitoring. Real provider/store/ad/device validation and signing/store submission remain pending. No phone build/install or backend deployment in this task.

## Category-wide level uniqueness — 15 October 2026

- [x] Audited actual bound three-stage levels separately for Shapes, Words and the alternating Mixed route in EN/RU/UK. Found 8 hidden gameplay duplicates in 6 groups missed by recipe metadata fingerprints: independent stage reflections, symbol renaming and inactive defaults must not create uniqueness. Preserved failing baseline in `Reports/CampaignUniquenessOct15/Before/`.
- [x] Replaced only `C199`, `C209`, `C238`, `C284`, `C288`, `C293`, `C304`, `C309` with solver-proved alternatives retaining the principal mechanic and bounded difficulty. Other 532 recipes, all node IDs/order/source definitions/locale/word files remain unchanged. Exact old recipes archived for existing server-verified clears. Design note and results: `Docs/CAMPAIGN_UNIQUENESS_OCT15.md`.
- [x] Added a reusable actual-C# audit and made the normalized per-category check a publication gate. Final counts: 500/500 Shapes, 534/534 Words per locale, 540/540 Mixed per locale; no repeated complete level even ignoring stage order. Individual stage reuse is separately enumerated; no claim of globally unique sub-stages.
- [x] PASS: 121 checker/package/save assertions, 6471 primary/localized stage replays, 1620 optimal stage certificates, 3294 lower-durability probes, all existing causal effect/atomic multiset/no-idle-board checks. Restored 32 old localized/primary checkpoints with working undo. Actual server boundary suite: 124 PASS, including archived earned clears and tamper rejection. Published source/Resources check passed. Reports: `Reports/CampaignUniquenessOct15/`.
- [x] Official Unity MCP imported updated TextAssets with existing GUIDs, confirmed all 9 actual runtime category/locale sequences and captured/reviewed all 24 changed stages at 390×844. Final Console zero errors/warnings; Play stopped; temporary profile only; unsaved Editor scene preserved. `Reports/CampaignUniquenessMcpOct15/`. The first harness attempt hit the expected category story gate and was corrected without changing gameplay; failure evidence retained.
- [ ] NOT RUN: physical-device build/install/performance, human enjoyment/playtest of every route, live server deployment. Before release, rebuild/deploy Cloud Code with current embedded recipes and legacy archive alongside the client.

## Map header, compact landing, living flowers and spaced islands — 15 October 2026

- [x] Reproduced the reported map composition with fresh native baseline captures. Confirmed the inherited continuous floor consumed 14.8–21.1% of the viewport and a second painted floor existed in the frame. Corner shader motion moved colour inside a fixed alpha outline; white flowers were excluded from the old colour mask.
- [x] Shortened both map floor layers to a 5.5% physical landing, kept supports and visitor contacts coherent, and placed the entire continue control above sky. Other menus retain their existing floor behavior. Existing source art and metadata preserved.
- [x] Applied the annotated follow-up: wider title plaque above the corner controls, centred and fitted full names, five original-size islands distributed with clear space and upper islands raised. Preserved small per-world layout differences. Removed decorative map bridges under the user's explicit fallback authorization; gameplay bridge mechanics/assets remain intact.
- [x] Added smooth rooted corner bending including alpha/white flowers, respecting reduced motion, low quality and background pause. Isolated native frame renders confirm moving corner contours in all nine worlds; no new per-frame objects or gameplay mutations.
- [x] PASS: 216 current main-Editor MCP captures across nine worlds, EN/RU/UK, regular/large text and four phone/tablet sizes; actual visible-glyph containment, physical ground clearance and real button raycasts. Four additional safe-inset mobile captures; all 90 solid-island silhouette pairs clear, original sizes retained. Visual review of all nine maps and long/small headers. Evidence: `Reports/MapPolishOct15/`, `Docs/MAP_POLISH_OCT15_RU.md`.
- [x] PASS: 11/11 native PlayMode cases in an isolated project using actual current sources: map flow/state/motion, safe headers and corner-pixel proof, all reference menus/world frames, gameplay footer/weather and wildlife contact regressions. Main MCP disconnected during the final extra-check dispatch; validation continued in the isolated Editor without resetting the user's scene. Temporary profiles/test copy cleaned after preserving evidence.
- [ ] NOT RUN: physical-device build/install or device FPS/GPU profiling. No rules, content IDs, economy, packages or backend deployment changes.

## Screen inventory, retired asset cleanup and grounded map controls — 15 October 2026

- [x] Audited current screen routes and captured the five compositions still awaiting the new visual treatment: island level selection, journey ending, mechanics guide, word-language confirmation and save recovery. Report and native phone overview: `Docs/SCREEN_AND_ASSET_AUDIT_OCT15_RU.md`, `Reports/AssetCleanupOct15/remaining-screens.jpg`.
- [x] Removed the uncalled old map and catalogue presentations, old journal and four obsolete page renderers. Kept public compatibility entry points and enum values; redirected them to the current map, shop and My things. Preserved all progression, economy and record data. Verified the ending → My things → ending return.
- [x] Deleted 62 confirmed retired/duplicate assets and only their paired metadata, 93.82 MiB of raw source files. One byte-identical music duplicate; no decoded PNG duplicates. Shared existing cards/buttons, all 54 world islands, current framing, tool icons, comic panels and wildlife remain intact. No new art generated. Manifest, hashes, replacement mapping and verification tool retained; recovery archive outside Unity at `/private/tmp/cascade-asset-cleanup-oct15-backup.zip`.
- [x] Honoured the latest map clarification: lower mode selection and continue controls; continue artwork rests on a coherent short brick landing occupying 7.5% of physical height while controls clear gesture insets. Preserved island sizes, header, per-world paths and corner motion.
- [x] Fixed the jumping comic illustration by reserving a stable dialogue band. Connected current map landing controls to dialogue hide/restore. Updated stale comic test preconditions for portrait gameplay, optional save parameters and phone text chunks.
- [x] Validation: 28 unique selected native PlayMode cases passed across the final regression and corrected final comic run; 206 native captures retained. Includes 54 map viewport/language/font cases, four safe-area maps, nine-world corner motion, actual asset reuse, legacy redirects/state preservation, reference screens, settings, account, 36 comic panels and world life. All 1,873 remaining first-party active files matched the disposable project tested without the retired files. No remaining reference targets a deleted GUID; exact duplicate groups after cleanup: zero. Evidence: `Reports/AssetCleanupOct15/evidence.json`. Intermediate failures remain recorded separately.
- [x] Main Editor scene and user saves preserved; temporary validation checkout removed after collecting evidence. The asset deletion was applied only after the native missing-resource simulation passed.
- [ ] NOT RUN: full historical test suite, player build/install or physical-device performance profiling. No backend deployment, prices, IDs, game rules, content recipes or package changes.

## Aircraft depth, atmospheric scale and uncut landing edges — 15 October 2026

- [x] Reproduced the draw-order problem in native captures: map/home/welcome aircraft could render above the architectural frame; the map's opaque floor also cut across its lower plants. Corrected the shared stage order so sky aircraft stay behind the frame and the short map landing retains complete foreground corners.
- [x] Gave menu aircraft the distant islands' sky projection and haze; reduced the home/welcome airship without moving its centre. Gameplay aircraft now use the existing island shader, regional haze and lighting, smaller silhouettes and sufficient depth clearance through eight motion phases in all nine worlds. Removed the old gameplay glider from menu rendering and replaced full flat-card rotation with gentle yaw/bank.
- [x] Removed the vertical paving seams below the home/welcome buttons. Reused the complete existing ground artwork, including post bases and plants, over the old floor strips. Full-width texture coordinates and a three-band mesh preserve paving/platform proportions while only the middle timber absorbs extra viewport height. No bitmap assets or metadata changed; no duplicate art generated.
- [x] PASS: all 22 selected native PlayMode cases have passing latest results. The final broader run passed 21 cases; the account fixture then passed all four cases after updating its obsolete uniform-crop expectation to verify proportional outer ground bands. Includes actual positive-control frame occlusion (zero opaque-frame leak pixels in 28 menu states), nine gameplay worlds/eight phases, unchanged attempt hashes, working buttons and four phone/tablet sizes with EN/RU/UK and safe insets. 481 source files match the isolated project. Native evidence and visual comparisons: `Reports/AircraftFrameDepthOct15/`; implementation note: `Docs/AIRCRAFT_FRAME_DEPTH_OCT15_RU.md`.
- [x] Visually reviewed fresh nine-world home/map/gameplay captures and the welcome composition. Preserved the main Editor's unsaved scene and user saves; used a disposable native Editor because MCP did not respond. Test captures and results retained before removing the disposable project.
- [ ] NOT RUN: full historical suite, player build/install or physical-device FPS/GPU profiling. No economy, rules, IDs, purchases, backend or package changes.

## Individual stage uniqueness, rooted flowers and coherent sky flight — 15 October 2026

- [x] Confirmed the gap in the earlier whole-level audit: individual stage repeats remained (158 extra occurrences in Shapes, 161 in each Words locale, 162 in each Mixed locale). Tightened the publication gate to reject any repeated normalized stage, including symbol renaming, reflection and reuse at another triad position.
- [x] Replaced only 162 stages in 97 nodes through the production constructor/solver; preserved 1458 stages, 443 complete recipes, all node IDs, campaign order and source definitions. Retained each principal mechanic and bounded difficulty increases. Kept the earliest occurrences; archived every exact previous recipe in the existing server archive. Recorded design limits and actual difficulty changes in `Docs/STAGE_AND_SCENERY_UNIQUENESS_OCT15_RU.md`.
- [x] PASS: zero repeated stages in 1500 Shapes stages, 1602 Words stages per locale and 1620 Mixed stages per locale. 1287 checker/package/save assertions; restored 388 old primary/localized checkpoints with their original undo availability. 1620 optimal certificates, 6471 replays, causal effect/entry/gust checks, 3386 reduced-durability probes, atomic multisets and no idle boards. Publisher `--check` passed. Server module: 140 PASS with fake SDK transport and current recipes/archive. Reports: `Reports/StageUniquenessOct15/`.
- [x] Stopped emitting whole daisies, dandelions and night flowers into the sky. Reused the existing atlas on fixed painted-leaf/frame contacts, with source-frame nine-slice projection in menus and stem-centred sway in gameplay. Rain, snow, leaves, feathers and lights retain airborne motion. No new bitmap assets or replacement art generated.
- [x] Unified menu/gameplay aircraft motion with different profiles for all six existing craft images, correct source-facing direction, gradual scale/altitude changes, slope-related bank and invisible offscreen direction changes. Preserved island haze, lighting, frame depth, quality limits and background/reduced-motion controls. Home's authored aircraft uses a restrained hovering arc. Decorative motion cannot change gameplay or move menu buttons.
- [x] PASS: 69 selected native EditMode cases (campaign, effects, durability, published recipes, saves and personal missions) and 36 selected native PlayMode cases, using latest results across recorded runs. Includes real frame-pixel occlusion, 36 flower roots on actual painted frame pixels, fixed gameplay stems, six minutes of sampled trajectories per craft/world, current menu/reference/map/account/weather regressions, and 27 representative revised-stage phone captures/replays. Corrected obsolete test fixtures for portrait gameplay, the authored home background and actual framed settings caption. Initial failures retained.
- [x] Saved 398 native mobile PNGs and two 72-frame motion sequences recorded on one fixed 390×844 camera target; visually reviewed rooted scenery, revised boards and motion contact sheets. 482 current sources match the tested copy. Evidence: `Reports/StageSceneryOct15/evidence.json`. The main Editor's unsaved scene and user saves were preserved; disposable project removed after collecting evidence. MCP did not respond, so validation used an isolated native Editor.
- [ ] NOT RUN: full historical suite, player build/install, physical-device profiling or live server deployment. Before release, rebuild/deploy Cloud Code with the updated client recipes and legacy archive. No new purchase IDs, prices, package or save-schema changes.

## Current story screens for designer and cancelled flow remnants — 15 October 2026

- [x] Corrected the screen inventory after the user's clarification: the full mechanics catalogue and separate puzzle-language confirmation were cancelled, not awaiting redesign. Removed catalogue pagination while retaining explanations for the current gameplay effect. Removed the language confirmation renderer; legacy entry points apply/persist the shared language immediately or forward to the current language settings page. Preserved enum values, saves and active puzzle state.
- [x] Confirmed that island level selection still uses the previous composition; left it unchanged for the user's design review. Captured the actual current ending, all five recovery presentations, all 115 dialogue beats/chunks in the nine four-panel world comics, and 37 map/other dialogue frames covering all nine residents, island opening/progress/completion, level aside, mode crossings/return, personal invitation and checkpoint. Total: 159 native 390×844 PNGs with simulated phone insets, plus contact sheets and a designer ZIP/HTML gallery. No new visual design or game bitmap assets generated.
- [x] PASS: latest results for eight unique selected native PlayMode cases, including five existing world/comic navigation cases, the complete current capture, shared-language persistence/reload and cancelled-route regression. The initial capture-only fixture used too many story receipt IDs; corrected the fixture range. A later check caught delayed persistence on the compatibility language entry; added immediate durable saving and both affected cases passed. Intermediate XMLs retained separately. All 481 current C#/shader sources match the disposable validation copy after excluding only the recorded capture-method insertion.
- [x] Evidence: `Reports/StoryScreensOct15/evidence.json`, original captures under `Native/`, refreshed inventory in `Docs/SCREEN_AND_ASSET_AUDIT_OCT15_RU.md`. The open main scene and user saves were preserved; MCP did not respond, so capture/validation used a disposable native Editor.
- [ ] NOT RUN: full historical suite, physical-device build/install or device performance. Recovery states and the personal invitation are isolated presentation fixtures; the gallery covers every kind of dialogue window, not every repeated story text in the full campaign. No economy, campaign, purchase, backend, package or save-schema changes.

## Illustrated map controls, completion ribbon and background ghosts — 15 October 2026

- [x] Reused the existing nine-sliced wooden/leaf Home PlayButton for the wider continue action and mode-selector surround. Kept the shared cream strip/green active face, balanced icon/caption geometry and retained the short brick landing and gesture insets. No new bitmap assets or duplicate art.
- [x] Attached the centred Completed / Завершён / Завершено label to its own proportionate green ribbon, with enough height for actual visible glyphs and large text. First native layout run caught the enlarged Shapes label leaving its box; corrected the caption areas and repeated the full layout/click capture successfully.
- [x] Reproduced two gameplay-island meshes remaining active behind the map. Prevented them reactivating outside gameplay; immediately hide gameplay islands, glider/skiff/extra craft and gameplay frame when changing screens, including paused menus. Retained the separate menu atmosphere behind the frame and verified scenery restores on return to gameplay. Final direct-before-render positive control isolates 3301 changed pixels at the ghost-island positions; attempt hash unchanged.
- [x] PASS: latest results for seven unique selected native PlayMode cases across recorded runs: all-world/language/mobile footer and ribbon checks, menu/pause scenery lifecycle, actual receipt stars, map/settings/home return, world menu layout, aircraft/frame occlusion and nine-world/eight-phase gameplay aircraft. Preserved first failure separately. Saved 61 final native PNGs, layout/click metrics and comparison sheets. All 481 current C#/shader sources match the tested disposable project. Evidence: `Reports/MapControlsOct15/evidence.json`; note: `Docs/MAP_CONTROLS_OCT15_RU.md`.
- [x] Preserved the main Editor scene and user saves; MCP did not respond, so validation used a disposable native Editor. Results and screenshots retained before cleanup.
- [ ] Deferred at the user's request: side frame/building composition and the concern about cropped/jagged side decoration. Kept current frame geometry, island positions/sizes and background crop unchanged.
- [ ] NOT RUN: full historical suite, physical-device install/build or performance profiling. No economy, gameplay, campaign, purchase, backend, package or save-schema changes.

## Positioned bird and insect sound design — 15 October 2026

- [x] Added five short CC0 recordings: two songbird phrases, gull, barn owl and a seamless bumblebee loop. Reused the existing NightCrickets clip. Recorded authors, license pages, download/decoded/output hashes, source slices and reproducible preparation; no synthetic test pack or raw-source duplicates added to Resources. Five new WAVs total 435420 bytes, under 1 MiB decoded.
- [x] Bound sound to painted live gameplay rigs, regional visitors, menu/frame residents and the home bird. Stereo follows actual screen position, gain follows projected size/opacity, and invisible actors fade out. Bees sound while flying, crickets while resting; butterflies/moths/dragonflies stay silent. Two reused new voices, sparse bird phrases, existing cricket voice, cue ducking and independent Sound/Music controls. Pause/focus/ad/device-reset/disable stop owned sounds and return begins quietly. No animation or puzzle-state changes.
- [x] PASS: 12 unique selected native Unity PlayMode cases across the final two runs: seven new wildlife/audio-output cases plus five cricket/wind/flight/frame-landing regressions. Actual AudioListener PCM is nonzero and switches channel dominance with the visible actor. Nine-world planted contacts still pass; nine native captures retained. All 485 current C#/shader sources and five new WAVs match the tested disposable project; copied native-generated import metadata. First failures retained separately: too-short cooldown fixture corrected; legacy controls test lacking a portrait target replaced by successful portrait coverage.
- [x] Evidence: `Reports/WildlifeAudioOct15/evidence.json`; sources and limits: `Docs/WILDLIFE_AUDIO_OCT15_RU.md`. Unity MCP did not respond, so used an isolated native Editor without touching the open unsaved scene or user saves. Disposable project removed after retaining evidence.
- [ ] NOT RUN: physical phone/Bluetooth listening, mobile player build/install, device profiling or full historical suite. No rules, economy, product IDs, backend, package or save-schema changes.

## Visible atmosphere, dialogue and transitions — 15 October 2026

- [x] Reused existing seasonal atlas and world frames; made weather/attached flower movement visible across menus/gameplay, including Settings. Hidden gameplay scenery/wildlife cannot render or chirp behind menus.
- [x] One cream map mode selector without a wooden surround; measured dialogue actions with inner padding, whole dialogue turns, readable names/counters and night paper contrast.
- [x] Rewrote 177 localized turns in EN/RU/UK around the storm-damaged route to Gran's beacon, preserving all 1944 localization keys and story receipts. Source: `Data/story_polish_oct15.json`; existing-key applicator: `Tools/apply_story_polish_oct15.py`.
- [x] Layered screen arrivals through existing transition/button owners; stable architecture/hit targets; interrupted, backgrounded and Reduced Motion states settle safely.
- [x] PASS: 15/15 selected native weather/menu/audio/transition regressions before latest story/button revision. Final map selector mobile regression passed. Preserved earlier failures and superseding results in `Reports/AtmosphereStoryPolishOct15`.
- [x] PASS: repeated final native story/button/contrast checks: 432 comic and 918 map dialogue turns; final borderless mode selector mobile regression. Native-generated locale tables/shared key data, metadata, captures and logs retained; 487 current C#/shader sources match the tested clone. `Reports/AtmosphereStoryPolishOct15/evidence.json`; `Docs/ATMOSPHERE_STORY_TRANSITIONS_OCT15_RU.md`.
- [x] Built and signed iPhone player 0.1.0 (202610153), com.vickand.cascade, arm64. Unity export and Xcode Release build succeeded; strict deep signature verification passed. Existing signing reused. Generated pod targets use iOS 15.0 to match the project and Xcode 27 requirements. Retained export/app/logs; `Reports/iPhoneInstallOct15/README.md`.
- [x] Final transition capture/regression after mode/dialogue changes: 2/2 PASS; 60 current native frames retained. All runtime resource assets match the built copy.
- [x] Installed build 202610153 on the paired physical iPhone after the user unlocked it on the same network. CoreDevice confirms version/bundle; launched process is alive. No uninstall/profile reset. First screenshot shows the iOS Local Network prompt.
- [ ] Confirm rendered game screen after the user dismisses the system prompt. Physical gameplay/FPS/audio remain unverified.

## Natural fauna contacts, flight and centred mobile controls — 15 October 2026

- [x] Bees/crickets cling along vertical frame supports with planted feet, braking approaches, contact shade and continuous release orientation; birds keep existing leafy landing supports. Shared ambient clock and puzzle state remain unchanged.
- [x] Replaced Home's old near-linear sprite flight with shared curved routes, continuous wing articulation, species/visit cadence, glides, hovering and rear turns. Preserved separate old tails/hindwings. Audio follows the visible side/rear pose and retains existing pooled voices/lifecycle.
- [x] Reused existing 17 rear views and wing/tool/trophy/provider/card assets. Generated four missing dorsal bodies in one transparent atlas and two horizontal wingless flight bodies in a second atlas; original upright regional bodies remain for landing. Native importer preserves dimensions and iPhone ASTC 6x6 settings.
- [x] Centred provider icon/caption groups and guest button; reduced and centred shop item/trophy/coin illustrations within existing cards. No product ID, economy, backend, package or gameplay rule changes.
- [x] PASS: 33 EditMode + 12 PlayMode selected native checks; EN/RU/UK, two phone sizes and both text sizes; actual rendered rear/tail pixels, flight/perch body switching, menu/gameplay frame contacts across nine worlds, takeoff continuity, screen lifecycle and seven audio cases. 179 native PNGs, intermediate failures and final logs retained. All 492 C#/shader sources match the tested copy. Evidence: `Reports/FaunaUiOct15/evidence.json`; note: `Docs/FAUNA_UI_POLISH_OCT15_RU.md`.
- [x] Unity export and Xcode Release signing succeeded for 0.1.0 (202610154), com.vickand.cascade, arm64; strict deep signature passed. CoreDevice confirmed successful installation on the same paired iPhone. Evidence: `Reports/iPhoneFaunaUiOct15/README.md`.
- [ ] New-build launch/physical screen/FPS/audio remain unverified. The user stopped further build/install work; cancellation was attempted but installation had already completed. No subsequent launch or device checks. **Do not build/sign/install/launch/check the phone again until the user explicitly asks**, overriding earlier standing installation authorization. Source fixes and 45 passing native checks are retained.

## Shared aircraft traffic, fauna perspective and pause paving — 15 October 2026

- [x] Reproduced four simultaneous menu aircraft and old frame paving strips over the continuous pause floor. Retained native before captures and measurements.
- [x] Unified legacy glider/skiff/home craft with thematic traffic: two staggered visitors maximum (one at reduced quality), separate heights/depth, varied durations/headings/curves, species of craft drift/bank, offscreen wraps, haze and size. No fixed home duplicate. Actual exposed aircraft pixels are checked beside Mika; free flight remains behind the frame and in front of opaque sky.
- [x] Shared bird/insect body-length and distance scale across home, gameplay and stage visitors; removed insect enlargement multipliers, adjusted frame insects, added consistent depth-dependent size/haze/pace. Native full silhouettes confirm bee smaller than bird on the same plane.
- [x] Pause's lower frame reuses existing transparent continuous posts rather than painting old paving strips over the floor. Verified actual pixel changes and continuous ground at four phone/tablet sizes and three world palettes. No new raster assets, rules, prices, IDs, backend, package or save changes.
- [x] PASS: 35 EditMode + 26 unique PlayMode checks across retained runs; nine-world traffic/occlusion/haze/landing/audio regressions, UI flow, real silhouette comparison and 144 native motion frames. 187 final PNGs. All 495 C#/shader files and 861 Resource files/metadata match the tested isolated project. `Reports/FlightPerspectivePauseOct15/evidence.json`; `Docs/FLIGHT_PERSPECTIVE_PAUSE_OCT15_RU.md`.
- [x] Respected user hold: **no app build/export, signing, install, launch or phone checks**. Native Editor tests only; main unsaved scene and saves preserved. Physical performance and full historical suite not run.

## World score routing, musical development and behavioural ambience — 15 October 2026

- [x] Reproduced the map's explicit menuMusic routing in native PlayMode before fixing it. Journey map/dialogue/comic/attempt/result contexts now use the displayed/active world; Home keeps its own score. Kept two streaming voices, 1.6-second smooth crossfades, reversible navigation, position recovery and independent mute controls. Verified actual listener PCM for all nine map themes and level entry in every world.
- [x] Reworked ten original 64-bar acoustic compositions: immediate world-specific motifs, question/answer periods, functional dominant/tonic resolutions, harmonic-minor leading tones, related-key middle sections and developing reprises. Reused credited VSCO2 acoustic samples and all existing active music GUIDs/import settings; did not restore the retired menu duplicate. Ten distinct 178.6–232.7-second masters, valid seams/headroom and notation checks; artistic judgement still requires human listening.
- [x] Added eleven short CC0-derived phrases, keeping the previous five WAVs byte-identical: flight contacts, perched robin/wren songs, additional gull/owl calls, three real cricket chirps. Per-individual history/timbre/cooldowns, neighbour replies, painted-position pan/distance, flight-responsive bee buzz, silence on landing/offscreen/background/ad/mute. Reused two wildlife voices plus the existing cricket voice; total 16 resident mono clips / 3,082,240 decoded float bytes. Sources, exact slices/licenses/download URLs retained.
- [x] Replaced continuous cosmetic wind with variable swells, optional weaker second breaths, direction changes and real calms. Removed the separate automatic gameplay pressure tail; additional prop recoil follows the actual weather. Explicit mechanic gusts remain responsive during calm. Wind EQ removes low rumble; no resolver/economy/backend/package/profile changes.
- [x] PASS: 27 EditMode + 25 unique PlayMode checks across retained native runs (52 total), including real audio output in all nine worlds, rapid fade reversal, lifecycle/mute/reset, behavioural voices, calm/explicit gusts and weather pixels on a 390×844 target. Corrected three old regression fixtures for current borderless selector and portrait/finished mechanic teaching. Retained intermediate failures and superseding results, 51 weather frames and audition. All 531 Assets C#/shader files and 860 Content/Resources files/metadata match the tested isolated copy. `Reports/AudioWorldsOct15/evidence.json`; `Docs/AUDIO_WORLDS_POLISH_OCT15_RU.md`.
- [x] User hold respected: **no player build/export, signing, installation, phone launch or phone checks**. Main unsaved scene, player profiles and protected backup untouched. Physical iPhone audio/performance and full historical suite not run.

## Annotated mobile UI polish — Oct 15–16

- [x] Shared 4 logical px icon/caption spacing and centring by painted glyph bounds; persist user preference for future projects. Centre boards, card art, compound counters and prices.
- [x] Larger Welcome board; shared green/amber actions across checkpoint, failure, legal, dialogue and remaining overlays; readable adaptive subscription footer.
- [x] Finger-following catalogue swipes with elastic bounds/spring arrival, stable action hit targets and no accidental card clicks; apply to tools, trophies, collection and map.
- [x] Rooted seasonal sprigs for all worlds, stronger frame flower motion and remove floating failure bushes. Respect reduced motion, focus and quality.
- [x] Actual checkpoint numbers/slash aligned; fix same-render stale localized numeral-copy mesh. Retain native failing reproduction and passing regression.
- [x] 34 unique PlayMode checks passed; 270 marked phone variants plus short-phone/footer, nine-world, gesture and dialogue/transition checks. Verified 1377 source/resource files match tested clone. No player build, signing, installation, device launch or main unsaved scene change.
- Evidence: `Reports/UiPolishOct16/evidence.json`, `Reports/UiPolishOct16/index.html`, `Docs/UI_POLISH_OCT16_RU.md`.


## iPhone installation requested by user — 16 October 2026

- [x] User explicitly authorized this build/install, superseding the earlier hold for this deployment. Unity iOS IL2CPP export succeeded (0 errors); Xcode Release build/sign succeeded, strict deep signature passed. Retained export/app: `CascadeUnity/Builds/CASCADE-iOS-202610161` and its DerivedData sibling.
- [x] CoreDevice confirmed installation of 0.1.0 (202610161), com.vickand.cascade, on the paired physical iPhone 17; installed-app query matches, launch succeeded and game process remained alive. Includes current world music/routing, island/tutorial screens and frame seams.
- [x] All 1445 source/resource/scene/package snapshot files matched the tested export copy; main source and unsaved scene preserved. No uninstall/data reset/restore; no existing CASCADE app was found before install. Protected backups untouched. Evidence: `Reports/iPhoneOct16/README.md`.
- [ ] Physical game screen, gameplay/FPS/audio remain unverified: first capture was early black startup; later foreground was another app, whose capture was moved outside the repo. Future builds/installations require another explicit request.

## Frame and floor seams across screens — 16 October 2026

- [x] Follow-up music recheck after tutorial/frame changes: 3/3 native runtime tests passed again for all nine audible map themes, map-to-level entry, return home, rapid fade reversal and foreground/output recovery. Existing revised compositions remain connected; 1.6-second crossfade, same-world map/level continuity. Evidence: `Reports/AudioWorldsOct15/RecheckOct16/`. No player build/install/device check.

- [x] Reproduced the reported lower side paving strips on NoAds/Shop (6,774 actual changed pixels at 390×844). Pause's earlier special-case repair was already effective. Found the same old paving over sky on Win/JourneyEnd/SaveRecovery.
- [x] Applied the existing transparent side posts down to the bottom of every ordinary clean menu frame. Preserve authored map/settings frames and complete floor layers. No new raster assets, control/text/economy/rule/ID/package/save changes.
- [x] PASS: 6/6 selected native PlayMode checks, 213 final PNGs; 30 screen/overlay states at four portrait sizes, 10 menu states in nine worlds, rendered A/B strip comparison, full-width floor/frame bounds, unchanged attempt hashes and existing aircraft/frame occlusion regression. Maximum remaining old-strip pixel difference: zero. All 552 C#/shader and 890 Resource/metadata files match the tested copy.
- [x] Before/after gallery and retained intermediate evidence: `Reports/FrameSeamsOct16/index.html`, `Reports/FrameSeamsOct16/evidence.json`; note: `Docs/FRAME_SEAMS_OCT16_RU.md`. Main unsaved scene and saves preserved; no player build/export/sign/install/phone launch. Physical device/FPS and full historical suite not run.

## Tutorial reference windows — 16 October 2026

- [x] Replaced the old teal mechanic modal with the supplied parchment/wood/flower composition for all 27 lessons: inset heading/body, red close, animated resolver examples and shared jade action. Uniform reference scale and separate system safe area preserve proportions; touch targets grow to 44 pixels.
- [x] Single-page lessons hide pagination. Replay topics use dots and masked spring swipes; first control lesson has separate drag/two-tap pages. Freeze replay topics while the modal is open so ordinary control refreshes cannot change the selected subject.
- [x] Reused FieldHelpPanel, HeaderBoard, action skins, tools, blue spool, hand, wood, counter, plants and rays. Generated only a separate undo ribbon and one four-shape atlas; provenance and stable metadata retained. Once-per-opening GPU blur releases its captured texture on close; lowest effect quality skips capture. Respect focus/reduced motion and preserve isolated example calculations.
- [x] PASS: 10/10 selected native PlayMode checks. 243 mobile/locale/large-text cases and 260 final native PNGs, real pointer raycasts, dot/swipe/close, Words/Mixed, result poses, animation suspension, low-quality fallback, first undo/repair teaching and disabling lessons. Updated older portrait/capture/unseen-Repair fixtures. All 551 C#/shader files and 890 Resource files/metadata match the tested copy. Evidence: `Reports/TutorialReferenceOct16/evidence.json`; gallery: `Reports/TutorialReferenceOct16/index.html`; note: `Docs/TUTORIAL_REFERENCE_OCT16_RU.md`.
- [x] No player build/export/sign/install/phone launch; main unsaved scene and user saves preserved. Physical device performance and full historical suite not run.

## Island level reference — 16 October 2026

- Replaced the old island browse list with the supplied open-sky reference composition: world board, parchment progress, raised number badges, three rating stars, record column and amber/green/locked actions. Existing replay/resume/unlock/category guards and persisted values remain intact.
- Generated 9 distinct world backgrounds with their large islands, plus one shared blank row and progress parchment. Reused the existing heading, back/action skins, gold star/grayscale material, lock/play/retry, clouds, planted decorations and weather. All provenance and metadata retained.
- Uniform portrait scaling, safe-area handling, 4 logical pixel content gaps, inset localized titles and labels, 44 pixel touch bounds, inertia, scrollbar and short-list auto-hide. Stencil masks clip all star materials; final screenshots explicitly check visible card/frame pixels.
- PASS: 8 selected native PlayMode checks in an isolated Unity 6000.3.8f1 Editor. 89 final native PNGs: 9 worlds × EN/RU/UK × 3 phone sizes with Large Text, plus 720 reference, compact/tablet, scrolled end and category captures. All 27 category/world combinations retain their existing level filters. Swipe/play/replay/locked guard/pinned stage and weather/focus/reduced-motion checks pass. Main source/art and metadata match the tested copy.
- No player build/export/sign/install, physical-device launch or device profiling. The user's build hold remains active. Main unsaved scene untouched. Evidence: `Reports/IslandLevelsOct16/evidence.json`; gallery: `Reports/IslandLevelsOct16/index.html`; design/reuse: `Docs/ISLAND_LEVEL_REFERENCE_OCT16_RU.md`.

## Welcome lettering — 16 October 2026

- [x] Follow-up user adjustment: reduced the base Welcome heading size from 78 to 68, retaining the gold style, centred fit and Large Text multiplier. Verified the single-value source change; earlier screenshots/test evidence show size 78. No new build/install.
- [x] Enlarged the actual Welcome label on the existing hanging account sign, with gold lettering, a dark outline and subtle shadow. Fit long RU/UK translations with side padding; reuse existing font/board/logo. Layout owns and releases the local material.
- [x] Existing native portrait/locale matrix passed (1/1; 38 captures, seven Welcome variants). Final phone renders reviewed in EN/RU/UK; source matches tested copy. Evidence: `Reports/WelcomeLetteringOct16/`. No new bitmap, player build or phone installation for this edit.

- [x] Follow-up requested screenshot: captured actual size 68 with Large Text off at 390×844 in native Play Mode; capture and overflow check passed. Retained in `Reports/WelcomeLetteringOct16/size68/`. Temporary capture fixture restored; no player build/install.

## Welcome sky life and device language — 16 October 2026

- [x] Welcome opens during an existing umbrella-ship crossing, with staggered later fleet visits; birds arrive shortly after opening. Reuse existing art, haze, perspective, wing poses and masks. Clamp the home wildlife clock to prevent slow startup frames from missing the admission window; respect focus, quality and Reduced Motion.
- [x] Auto-select ru for Russian, uk for Ukrainian, en for all other device languages before the welcome is drawn; apply the same automatic language to new word puzzles. Manual choice persists. Add optional compatible FollowSystemLanguage preference, preserving legacy ru/uk choices; design/migration note in `Docs/WELCOME_LIFE_SYSTEM_LANGUAGE_OCT16_RU.md`.
- [x] PASS: 11/11 selected native PlayMode checks, including actual visible pixels/movement on 390×844, frame occlusion, language fallback, legacy saves, manual override/reload and existing home/settings/account regressions. 17 retained Welcome PNGs; seven matching source files. Evidence: `Reports/WelcomeLifeLanguageOct16/`. No new bitmap, player build, installation, physical-device check or main scene/save change.

## Welcome Mika face identity — 16 October 2026

- [x] Corrected the existing waving WelcomeMika sprite through built-in imagegen, using HomeMika as the identity reference: cheek/mouth proportions and exactly two central upper incisors, retaining the waving pose and clothing. Replaced the existing production image, preserving its GUID/import settings and sprite crop; no additional runtime texture path. Original generation retained; provenance: `AssetPrompts/WelcomeIdentityOct16/generation.json` and updated account asset record.
- [x] PASS: existing native welcome/home/header matrix (1/1; 38 captures, EN/RU/UK, phone/tablet and safe area). Final mobile frames inspected; seven Welcome variants retained plus before/after. Source PNG and metadata match tested copy. `Reports/WelcomeIdentityOct16/`. No player build, signing/install, physical-device check or main scene/save changes.

## Welcome character animation trial — 16 October 2026

- User requests a GitHub checkpoint before animation changes, then a welcome-only layered animation trial: breathing, irregular blinking/gaze, ears/whiskers, planted weight shifts, tail/bag inertia, flowing scarf and an occasional greeting wave. No touch reactions, gameplay changes, new packages, player build or phone install.
- Local review captures/exported documents stay outside new Git additions; production code/assets, provenance and docs form the checkpoint. Preserve earlier tracked reports and original metadata.
- [x] Published and remotely verified checkpoint `0bd731f60f3d7d9d9f484850b2fe9c9c9c293238` on `codex/welcome-motion-oct16` before animation edits; main untouched. Reused the existing project deploy key through repository-local SSH configuration. The independent unborn Website repository remains on disk.
- [x] Added a welcome-only continuous skin of the corrected base Mika illustration: irregular blinking/gaze, planted breathing/weight shift, head/ear/whisker motion, greeting wave, damped bag/tail and wind-driven anchored scarf. No pointer interactions. Eye textures are the only new bitmaps; original sprite preserved. Costume variants retain their existing static art pending their own weights; sewn badges follow the base rig.
- [x] PASS: final 8/8 native PlayMode checks, including actual pixel isolation, fixed soles/muzzle, 30/120 Hz stability, triangle orientation, lifecycle/accessibility, badge/costume fallback, birds/ships, phone/locale matrix and account/progress regression. Fixed wave weights reaching the far ear; an intermediate cloth hypothesis was reverted. Final tested source matches main workspace. Evidence, 192-frame native preview and seven Welcome matrix captures: `Reports/WelcomeMotionOct16/`; implementation/provenance: `Docs/WELCOME_CHARACTER_MOTION_OCT16_RU.md` and `AssetPrompts/WelcomeMotionOct16/generation.json`.
- [x] Trial remains local above the published checkpoint. No player export/build, signing, installation, physical iPhone profiling or main-scene changes.

## Welcome Mika gesture refinement — 16 October 2026

- [x] Refined the existing welcome-only rig: shoulder anticipation, visible hand arcs with delayed wrist, C2 gesture envelopes, damped return, session-specific tempo/amplitude, planted counterweight, moving-visitor gaze, occasional double blink and independently timed ear twitches. Scarf gust smoothing, neighbour motion transfer and calm settling; existing artwork/eye textures reused. No touch reactions or other screens added.
- [x] Measured the prior implementation before editing; retained the intermediate motion-only result. Cached mesh topology and used uGUI's native bulk vertex path; cached chain rest lengths, moved bone matrices out of spring substeps, skipped static feet and unchanged shader values. Eyelid textures sampled only during visible blinking. Full-quality CPU simulation+mesh median 0.557655 → 0.400162 ms (28.2% less), low-quality 0.149323 → 0.109637 ms (26.6% less), 0 managed bytes in each 600-frame warmed-up loop. These are native Editor CPU measurements, not physical-device FPS or GPU metrics.
- [x] PASS: final 11/11 native PlayMode checks. All 480 preview frames audited for skin folds; 42-second simulations for three seeds × two qualities check later waves, visible arcs and continuity. Feet/muzzle, 30/120 Hz stability, eye-only blink pixels, focus/accessibility/lifecycle, live quality changes, costume/badge, birds/ships, phone/locale matrix and account/progress regressions passed. New 60 fps native preview and evidence: `Reports/WelcomeMotionPolishOct16/`; documentation updated. No player build, signing/install or main-scene changes.

## Welcome secondary physics — 16 October 2026

- [x] Replaced clock-driven secondary target offsets with source-space particle masses, gravity/air drag and XPBD bend/length constraints. Two pinned root sections move with the body; constrained velocities are reconstructed after solving. Tail/cloth differ in mass, compliance and damping. Bound artwork to closest chain sections and cached section transforms to preserve width; excluded scarf from head/whisker weights.
- [x] Added a rigid bag pendulum driven by attachment acceleration, body angular acceleration and gravity, with rest preload, damping and soft torso contact. Lower strap follows the same hinge; pouch excluded from breathing deformation. Body remains controlled planted breathing/balance animation, now smoothed with a subtle hip lean; no ragdoll, touch input, new art/packages or gameplay changes.
- [x] PASS: final 13/13 native PlayMode tests, including calm settling and impulse decay, moving anchors, rigid pouch geometry, section width, all previous 11 regressions and 480 native mesh frame audits. Impulse link stretch <=0.0923%, moving-root stretch <=0.0594%, anchor error 0 source pixels. No per-frame managed allocation in the warmed component loop. Cached CPU simulation+mesh median: full 0.420987 ms, low 0.132426 ms (before affine/solver caching 0.564748/0.167839 ms; earlier simpler model 0.400162/0.109637 ms). These are Mac Editor component measurements, not device FPS/GPU results.
- [x] Evidence and 60 fps native preview: `Reports/WelcomePhysicsOct16/`; current design/limits and XPBD source in `Docs/WELCOME_CHARACTER_MOTION_OCT16_RU.md`. Initial core check 4/4, first full 13/13, final cached full 13/13. Fixed diagnostic JSON numeric formatting on Mono before final export. No build, signing/install, physical-phone profiling or main-scene/save changes. Changes remain local above the published checkpoint.

## Welcome secondary motion visibility — 16 October 2026

- [x] Corrected the user's observed lack of visible secondary motion. Earlier amplitudes became subpixel after the original illustration was reduced to a phone screen; spring stability checks did not establish visual readability. Increased planted breathing/balance, added a muscle-driven tail gesture with physical overlap, softened the scarf and its wind-loaded torsional root, and let the rigid bag hinge follow chest movement. Smoothed cloth/tail weight boundaries to preserve the painted silhouette under strong gusts and later gestures.
- [x] Added six tapered, anchored whisker curves with short irregular sniffing gestures. Reuse the unchanged corrected illustration and eyelid assets; no new bitmap, package, input reaction, other-screen animation or gameplay change. Whiskers use one small non-interactive UI mesh. Retain static alternate costumes, focus/ad pause, quality controls and Reduced Motion.
- [x] PASS: final 14/14 native PlayMode checks (67.59 s), including actual rendered 390×844 and 320×693 meshes, opaque artwork samples and visible whisker pixels. At 390 width, measured relative motion ranges: tail 4.33 px, scarf 4.20 px, bag 3.03 px, chest 2.53 px, whiskers 3.67 px. These are upper-quartile sample ranges after subtracting hip/muzzle movement, not physical-device FPS. Existing 480-frame and 42-second skin checks, feet/muzzle/pouch rigidity, physics, badge attachment, EN/RU/UK layouts, account/progress and sky life regressions passed.
- [x] Simulation + both UI mesh builders: full 0.518743 ms, low 0.228274 ms median native Mac Editor CPU, 0 managed bytes in each warmed 600-frame loop. Final source/meta match isolated tested project. Evidence: `Reports/WelcomeReadableOct16/`. New embedded mobile preview uses the same native frames, with optional character close-up. No player build, signing, install, physical-device profiling or main-scene/save changes.

## Welcome scarf contact and gust cycle — 16 October 2026

- [x] Reproduced the raised scarf in calm with a failing targeted test: after 10 s without wind its tip still sat at source y519.25. The original illustration depicts a flying scarf; the previous rest pose retained it. Reused the exact original texture through a separate cloth mesh and shared shader mask, preserving the body, face and tied collar. No new bitmap or package. The loose end starts down the shoulder, air pressure lifts it, and torsion/damping return it to shoulder support when wind stops. Existing free-link inertia/constraints remain.
- [x] PASS: full 15/15 native PlayMode checks (71.87 s), followed by 1/1 enhanced 30/120 Hz check of the separate cloth against identical wind inputs; runtime unchanged between runs. In calm the tip rests at (298.73,830.41), a gust reaches the original streaming region, and the cloth returns within 3 source pixels after calm resumes. All 540 native frames in the 18 s actual wind cycle audit both body and cloth triangles. Phone/locale, feet/face/bag, zero-wind settling, focus/quality/Reduced Motion, badge/costumes, account/progress and wildlife regressions pass.
- [x] Simulation + body/whisker/scarf mesh median native Mac Editor CPU: full 0.675794 ms, low 0.281343 ms, 0 managed bytes per warmed 600-frame loop. Cloth adds 561/153 vertices and one owned material/draw layer, sharing the same PNG. Evidence: `Reports/WelcomeScarfDrapeOct16/`; source/meta match tested project. Native full-frame captures and a compact close-up preview retain the entire calm → gust → calm sequence. No player build, signing/install, physical-device profiling or main-scene/save changes.

## First visible mechanics, grounded artwork and Mika pose bank — 16 October 2026

- [x] First unseen mechanics auto-open their existing animated film even with optional coaching disabled, including the next visible board's branch/bomb. Preview acknowledgement leaves the active board unrestricted; seen lessons remain retired and replay through `?`. Hidden boards are excluded. IDs and puzzle rules unchanged.
- [x] Checkpoint Next/Map reuse shared green/amber skins with responsive 4.6:1 proportions. Welcome Mika lowered onto the platform. Corrected half-height plank mesh scaling and seated both branch variants into the moving board's crown. Existing artwork reused; no new game graphics.
- [x] Native pose bank: 10 main representations and 34 costume poses, with source manifest; no extra animation added to other screens. Phone-friendly inline gallery verified at 320 px, including first/last/previous selection and image decoding.
- [x] PASS final 12/12 native PlayMode + 23/23 EditMode checks. Native phone captures in EN/RU/UK; four-size checkpoint/frame regression. Broader 23/28 run includes every tutorial/phone/locale combination. The other five failures reproduce with all changed lesson/board/result systems restored to prior code in the isolated copy; preserved as pre-existing findings, not reported as passing. Reports, comparison and tested hashes: `Reports/GameplayPolishOct16/`; policy/limits: `Docs/GAMEPLAY_POLISH_OCT16_RU.md`.
- [x] Final source files match tested copy. No player build, signing/install, physical-phone run, primary-scene change or user-save mutation.

## Wind-reactive flowers and quieter welcome idle — 16 October 2026

- [x] Replaced wind-independent plant loops with shared wind-driven leaning plus smaller individual motion. Rooted menu/gameplay batches, attached frame/header sprigs, pause daisies and both foliage shaders respond to the existing weather signal; gameplay includes actual machine gusts. Welcome scarf and menu flowers share the environment clock. Existing art/pools reused, with stems/supports, accessibility and effect-quality behavior retained.
- [x] Reduced welcome torso inflation/vertical breathing; bag attachment follows the quieter chest. Added restrained knee flexion below the pelvis with planted soles. Tail now sways continuously with smooth changes in effort/amplitude instead of short gestures separated by stillness. Limited the first oversized experiment after skin-fold checks; no new animation on other mouse screens.
- [x] PASS: final 25/25 native PlayMode checks (112.93 s), including 60 s calm behavior, 30/120 Hz stability, geometry across seeds/qualities/later waves, 540 rendered body/cloth frames, phone pixels, eye/feet, lifecycle, scarf/bag/whiskers, badge/costumes, EN/RU/UK, rooted flowers across worlds and pause regressions. Belly relative-body range 13.66 → 2.16 source px; knees 12 source px with fixed soles; tail five-second ranges 10.26–37.92 source px, max 30 Hz step 0.64 source px. Torso/balance excursion <=1.50 rendered px on 390×844 and 320×693 captures.
- [x] Native Mac component CPU: 0.739792 / 0.300517 ms for full/low, zero managed bytes in warmed 600-frame loops. Mesh counts unchanged. This is Editor CPU, not physical-device FPS/GPU performance. Reports, before/after source/evidence and 18 s native preview: `Reports/WelcomeWindIdleOct16/`; notes: `Docs/WELCOME_WIND_IDLE_OCT16_RU.md`. Final source/shaders match validation copy; no build/signing/install, original-scene changes or user-save mutation.

## Remaining screen compositions — refreshed capture, 16 October 2026

- [x] Rechecked the earlier inventory against current routes. Island selection and tutorials have since received their reference redesign; retired mechanics catalogue and language confirmation are excluded. Journey ending, recovery, story dialogue and comic compositions still await a dedicated full redesign. Legal readers retain their plain layout with updated shared Close actions.
- [x] Captured/reviewed 12 fresh native 390×844 RU screenshots: ending, three actual dialogue contexts (map/gameplay/separate), intro comic, all five staged recovery reasons, Privacy and Terms. Two isolated capture cases passed; no runtime/presentation changes. Originals, per-screen metadata and temporary capture source: `Reports/RemainingScreensReviewOct16/`. Phone-friendly gallery verified separately. Temporary case removed from the validation copy; no build, install, primary scene or real-save changes.

## More visible welcome tail — 16 October 2026

- [x] Increased the welcome tail's continuous arc and broadened its skin falloff at the fixed body. Preserved smooth varying amplitude, tail physics, rigid bag, quiet breathing and planted soles; reused the same illustration/topology.
- [x] Actual rendered tail motion increased from 3.98 / 3.74 to 7.61 / 7.19 px on 390×844 / 320×693. Added a minimum 6 px phone-readability assertion. Calm five-second ranges 25.84–76.31 source px; maximum 30 Hz step 1.25 source px.
- [x] PASS: targeted 4/4, final 15/15 native welcome checks (69.03 s) including 540 audited capture frames, geometry across seeds/qualities, frame-rate stability, face/feet, secondary attachments and lifecycle. Full/low component CPU 0.677878 / 0.273373 ms with zero warmed managed allocations; no extra vertices. Evidence and 18 s native preview: `Reports/WelcomeTailRangeOct16/`. No build, install, main-scene or real-save changes.

## Distinct character poses and dialogue animation — 16 October 2026

- [x] Added pose-specific motion to all existing main Mika representations, 17 costumes in both poses, and all ten dialogue portraits. Apple reaches the mouth, opens/chews and retains two bite marks; sad Mika sighs/looks around; victory has two anticipatory hops and a separately rigid lantern pinned to the paw. Standing/thinking poses keep planted soles, slower breathing, knees, variable tail arcs, delayed bag/lantern motion and outfit-specific fabric/accessories reacting to wind.
- [x] Dialogue/world-comic articulation follows text cadence, punctuation and completion. Eye/face identity and glasses retained. Reused existing welcome eyelids/mouth for Mika and original props; only three compact bitmap fragments added (fabric, resident eyelids, resident mouths). Costume face/lantern landmarks measured for all 34 variants. Magnifier shares the real pose's motion. No new touch reactions.
- [x] Preserved app suspension, reduced motion/quality, existing preview touch actions and game state; cached topology and substepped springs. Fixed actual pause-picture arm origin and long victory-lantern skin pinching found during native review. Nested canvas raycaster preserves the existing preview button.
- [x] PASS: final 37/37 native PlayMode checks (130.66 s), including 6192 continuously rendered/audited frames across 52 variants, 156 pose/face/gesture stills, 22 actual phone-screen/dialogue captures at 390×844 / 320×693, 42 s skin checks, 30/120 Hz behavior, rigid glass/paw contact, apple contact, idle support, lifecycle and welcome/pause/comic/collection regressions. Warmed simulation/mesh loops allocate zero managed bytes; CPU figures are Mac Editor components, not phone FPS. Final source matches validation copy.
- Evidence, 52 native review clips and staged screenshots: `Reports/CharacterPosesOct16/`. Notes and remaining ambiguous balls/burning wording: `Docs/CHARACTER_POSES_MOTION_OCT16_RU.md`; imagegen provenance: `AssetPrompts/CharacterMotionOct16/PROVENANCE.md`. No build, signing, install, original-scene or real-save changes.
- [x] Final material follow-up: 3/3 (38.51 s). Matched original WorldSurface grading/warm lantern protection in worlds 0/1/5/8 against actual rendered pixels; repeated native phone screens and locale/control hit checks. Animation/mesh code and white-light pose appearance unchanged. Updated final source hashes and staged captures.

## Retired presentation cleanup — 16 October 2026

- [x] Removed 26 confirmed retired PNGs (36.16 MiB with obsolete source) and four old source files plus paired metadata; removed uncalled workshop/journal/guide/parcel layout helpers and decorative map bridge renderer. Kept public compatibility routes, GameScreen values, current context help and all persisted progression/economy.
- [x] Audited dynamic Resource loaders and serialized GUID consumers; consolidated obsolete fallbacks and importer expectations. Both Resources roots, 288 remaining first-party PNGs and publishing icons checked: no identical RGBA images or colliding resource keys. Previous 62-file cleanup verification still passes. Recovery archive outside Unity; exact hashes/reasons in `Reports/AssetCleanupOct16/retired-assets.json`.
- [x] Native final regression PASS 20/20 (64.84 s), source matched. Expanded run 59/61; both failing ribbon/aircraft assertions reproduce identically with pre-cleanup sources/assets restored in the disposable copy. Baseline and failures retained in evidence; no new cleanup regressions. Notes: `Docs/PRESENTATION_CLEANUP_OCT16_RU.md`. No player build, signing, installation, primary scene or real-save changes.

## Detailed motion across the pose bank — 16 October 2026

- [x] Extended all 52 existing actor variants plus welcome with independent ears, sniff/muzzle/painted-whisker motion, pose-specific cheek/brow expressions and small nods. Added delayed tail/cloth tips, bag flap and visible outfit/resident accents; preserved rigid glasses/masks, teeth, props, planted supports and existing welcome scarf physics. No new interactions, textures, render layers, topology or packages.
- [x] Final native PASS 27/27 (107.20 s): full 52-pose skin sweeps, 6192 rendered frames, 30/120 Hz, rigid glasses/lantern/teeth, apple contact, phone screens, lifecycle, quality and zero warmed managed allocations. Widened/reduced only problem tail/cloth falloffs detected during testing. Source matched disposable validation copy; component timings remain below 0.74 ms on Mac Editor, not a device-FPS claim.
- Evidence: `Reports/CharacterDetailsOct16/`; notes: `Docs/CHARACTER_DETAILS_OCT16_RU.md`. Compact native video staged for inline review. No player build, signing, install, primary scene or real-save changes.


## Authored apple animation and character recheck — 16 October 2026

- [x] Reproduced the visibly detached apple arm; acknowledged that earlier passing point/skin checks missed the anatomy. Discarded the repaired puppet version after native closeup review.
- [x] Generated 12 complete, registered character states; replaced the separate arm/IK/masks with authored frames and offline bidirectional transitions. Calibrated known +8/-8 pixel displacement exactly; one global ease, retained bite, own authored eyelids, preserved tail/cloth idle. Removed obsolete hidden-sleeve PNG + .meta.
- [x] Native PASS 29/29 across 52 variants and Welcome; final Apple-only eyelid refinement PASS 3/3 with full/low 480x640 motion and actual 390x844/320x693 RU/UK screens/dialogues. Visually inspected all four pose pages at three ages, core screens and Welcome. Warmed simulation/mesh loop still allocates 0 bytes; not an iPhone GPU/FPS claim.
- Evidence and exact generation prompts: `Reports/CharacterArmAttachmentOct16/`; correction note: `Docs/CHARACTER_APPLE_ANIMATION_OCT16_RU.md`. Final native MP4 provided for review. No player build, signing, install, primary scene or real-save changes.


## Complete authored speech expressions — 16 October 2026

- [x] Reproduced the small-mouth-over-closed-smile bug. Native whole-jaw comparison failed on all 46 variants before the fix; preserved original screenshots/metrics and source.
- [x] Replaced borrowed generic mouth stamps with closed/half/full face expressions edited from each exact source: 10 speakers, 2 additional Mika sources and both poses of 17 costumes. Six shared atlases, 92 edited states; original upper faces, poses and other graphics reused. Exact-source texture identity prevents two Mika sources colliding.
- [x] Removed the old ResidentMouths PNG/meta, procedural jaw deformation and unused experimental speech flow textures. One opaque drawn expression per speech phase, no doubled mouths/tongues; vowels reach full opening, punctuation rests. No runtime optical-flow or new packages.
- [x] Native PASS 31/31 for character/Welcome regression; final phase-policy refinement PASS 3/3 including full/half/transition pixel checks for 46 faces, 180 natural-speech frames and actual mobile/dialogue captures. Visually inspected four pages of intermediate faces plus main speaker motion. Runtime snapshots exactly match the tested copy.
- Evidence/video/prompts: `Reports/CharacterSpeechOct16/`; notes: `Docs/CHARACTER_SPEECH_OCT16_RU.md`. No player build, signing, install, primary scene or real-save changes; no iPhone GPU/FPS claim.


## Level-list half ratings and shared action alignment — 16 October 2026

- [x] Confirmed existing fractional ratings in native rendering: 1/1.5/2/2.5/3, exact gold half-star mesh and island total; persisted/restored/merged 2.5 remains exact. No rating rules or save-schema changes.
- [x] Reproduced the excessive visible icon/caption gap. Shared CenteredContentRow now measures actual glyph geometry, centres the whole horizontal/vertical group, and invalidates its cache when padding changes. Wider inner caption area in existing Play/Replay buttons; shared skins and navigation retained.
- [x] Native PASS 9/9 list suite (nine worlds, EN/RU/UK, 320/390/430, normal/large action text, scrolling and published level routing). Final first-layout refinement PASS 4/4 including action/rating rechecks, core-screen EN phone smoke and RU tutorial; rating persistence/rules PASS 13/13 EditMode. Selected native screenshots visually inspected.
- Evidence: `Reports/LevelListAlignmentOct16/`. No player build, signing, install, primary scene or real-save changes.


## Illustrated world completion and canonical Mika gesture — 16 October 2026

- [x] Added the reference-style completion screen after results/rewards and before the next world's unseen comic/map. Requires all offered world levels in the active category; existing HeardStory receipts 9600–9640 record explicit acknowledgement, without save-schema/rating/economy/backend changes. Home and next-world actions preserve progress; ninth world offers the journey map. Enum appended to preserve older screen values.
- [x] Reused nine skies/island sets, seasonal frame, tutorial paper and shared green/amber buttons. Tight island Sprite regions remove export gutters without new PNGs or importer changes. Shared centred 4-logical-pixel icon/caption groups.
- [x] Generated one canonical-Mika 12-state transparent atlas; whole connected sleeve/arm/face gesture, warm smile and authored blink, additional rooted ear motion. Offline bidirectional vectors reused from the existing native tool; same registered resting paw contacts the board. Focus/pause/reduced-motion/quality policies respected.
- [x] Native final PASS 11/11 (6 new-screen + 5 world-comic scenarios): 81 world/locale/phone captures, compact phone/tablet, actual disk reload, routing/one-time acknowledgement and 300 motion frames. Sources/graphics match the tested copy; relevant screenshots visually inspected. Two older GuidedLessonFlow button expectations fail identically in a control without the new hooks; evidence retained, no full-project-green claim.
- Art, generation prompt, XML/logs, screenshots and video: `Reports/WorldCompletionOct16/`; design/receipt note: `Docs/WORLD_COMPLETION_OCT16_RU.md`. No player build, signing, installation, primary-scene or real-save changes.


## Reference-style world comics and animated speakers — 16 October 2026

- [x] Reskinned the existing Prologue route to the supplied comic reference: seasonal frame/sky, large heading, whole framed illustration, raised speaker portrait, separate name plaque, complete paper turn and bottom Skip/Continue actions. Reused all 36 existing panels and shared art/skins; generated one transparent timber-frame PNG only, with exact prompts retained.
- [x] Localized paper height uses both speakers on the active panel, preserving its position and 3:2 aspect across turns. Fixed legacy heading-font override, page-counter room, deliberate title/name padding and Skip contrast on clouds. Shared centring/touch targets; original receipts, failed-save handling and destinations preserved.
- [x] Existing full-face expressions animate the speaking portrait, including blink/ears and mouth opening; background/focus/reduced-motion policies retained. No detached limb or borrowed mouth overlay.
- [x] Native final PASS 16/16: 648 complete localized-turn layouts, 85 phone/reference/compact/tablet captures, actual raycast hits, first-play/reload/save-failure/world routing, unchanged puzzle state and 240 speech frames. Relevant native shots and nine-world sheet visually inspected; 11 tested files match, 3 new GUIDs unique.
- Evidence, prompts and video: `Reports/ComicReferenceOct16/`; notes: `Docs/COMIC_REFERENCE_OCT16_RU.md`. No player build/sign/install, primary Editor or real-save changes; full project suite and iPhone FPS not claimed.


## Full-screen character requests and conversations — 16 October 2026

- [x] Replaced the compact OpenConversation ribbon with the supplied reference composition: themed sky/islands, large animated character, name board, complete parchment turn/count, bottom actions and continuous stone ground. Applied to personal offers, island exchanges and checkpoint dialogue; underlying menu and pause state restore on close.
- [x] Reused all existing skies/island sets, portraits/full busts, costumes, paper, foliage and shared button skins. One small generated Lumi ear repair fixes the source atlas’s clipped ear tips; original eyes/mouth and authored expressions retained. Other art reused without duplicate character assets; exact prompts saved. Existing full-face animation and focus/reduced-motion policies retained. Whole Mika/costume sprites masked behind the paper avoid the old portrait's cut scarf.
- [x] Measured whole localized turns and names, stable scene height, safe-bottom placement on tall phones, actual 44-pixel-or-larger hit regions on short phones with Canvas scaling, shared centred content and grounded wind-driven plants. Callback/receipt/mission/save/rating/economy behavior retained.
- [x] Native main PASS 13/13; final ear-contour refinement PASS 2/2 (including exact half/full/transition jaw pixels for 46 face variants): 81 phone/world/locale combinations, 459 authored-scene locale passes, compact phone/tablet, real pointer hits, complete personal-mission decline/accept/reload/three-stage/campaign-restore flow and five existing comic/navigation scenarios. 88 new-screen PNGs, 27 authored-dialogue PNGs and 240 native motion frames; representative shots and nine-world overview visually inspected. Thirteen tested files match; four new GUIDs unique.
- Evidence/video: `Reports/ConversationReferenceOct16/`; notes: `Docs/CONVERSATION_REFERENCE_OCT16_RU.md`. No player build/sign/install or primary Editor/real-save changes; full project suite and device FPS not claimed.


## The High Lantern mystery, Rin and nine-world character arcs — 16 October 2026

- [x] Replaced the storm-only premise with a complete fairy-tale mystery: missing High Lantern, pre-storm timing, crate/seal/forged-order/tide/key evidence, Felix's motive and deliberate concealment, Ada's deferred north route, a late glider rescue, lantern return and supervised restitution. Lower-island lamps explain ropes and exact-row delivery without changing puzzle rules.
- [x] Renamed the displayed protagonist Rin / Рин / Рін while preserving internal Mika sprite/animation identity. Added distinct voices, flaws and development for all nine residents, concrete callbacks to earlier events, relationships and a gradual progression from asking for reassurance to coordinating the final delivery. Notification and costume copy also use the current name.
- [x] Authored and connected 501 story positions / 1503 EN-RU-UK values across comics, island openings/progress/payoffs, checkpoints/asides, mode return memory, goals, missions and world endings. Preserved existing event IDs, save progress and 483 numeric table IDs via Unity's rename API. Removed obsolete page monologues; the beacon stays unlit until the actual finale.
- [x] Fixed a real duplicate pre-level exchange: an island opening could repeat through the legacy world-introduction branch. Corrected baseline reproduced it; new actual-launch regression passes after the fix. Shortened tight completion headings, narrowed and centered their inset and required eight logical pixels of painted-text padding. Existing art/animations reused; no new bitmap assets.
- [x] Native final evidence: 24/24 EditMode, 18/18 PlayMode, then 14/14 final flow/heading checks (43 unique tests). Covered 432 comic-turn and 918 dialogue-turn layouts, EN/RU/UK, Large Text, 81 phone/world/locale combinations, compact touch targets, actual mission flow, first-play/comic/save navigation and padded endings. Selected final captures visually reviewed; 22 tested source/data/table files match the main project. No compiler or runtime exception regressions in final logs.
- Script and character bible: `Docs/LANTERN_STORY_BIBLE_OCT16_RU.md`; complete Russian reading script: `Docs/LANTERN_STORY_FULL_RU_OCT16.md`; canonical authoring/translations: `Data/story_lantern_oct16.json`; evidence: `Reports/LanternStoryOct16/`.
- No player build, signing/install, primary Editor/real-save changes or new generated art. Full unrelated suite and physical iPhone validation not claimed; independent literary/native-reader review not claimed.


## Continuous stage descent — 16 October 2026

- [x] Reworked the between-stage descent into 1.18-second minimum-jerk camera travel with a soft start and stop. Both rigs retain fixed separation, two complete braided suspension ropes and connecting spans; the outgoing checkpoint cloth travels with its stage. Clouds and distant islands follow the same depth with slower parallax, while the timber frame masks travel and controls return with a gentle fade.
- [x] Corrected missing sliced receiver artwork, illustrated counters and TMP sprite submeshes in the snapshots. Preserved renderer visibility, independent materials/property blocks, original uGUI paint order and inherited/local opacity. Flushed counter layout and already-loaded translations before capturing new-stage UI. No new production bitmap assets or packages.
- [x] Held the live wood/rope spring pose under the moving copy and removed the competing entrance tween so arrival matches the live rig. Existing stage acceptance/persistence precedes animation; completion never grants progress or rewards. Focus, pause, reduced motion and navigation release temporary rigs and restore controls.
- [x] Native final PASS 13/13: seven focused descent scenarios plus six existing story/comic navigation scenarios. Both actual GoLower transitions, monotonic continuity, exact live pose, visible rope/receiver/counter snapshots, material ownership, persisted stage/hash, repeat-click gate, interruptions and EN/RU/UK covered. 110 native motion frames, compact 320-pixel phone/themed captures and a pixel comparison of the strength number at the live handoff (mean RGB error 2.23/255). Representative transition frames visually inspected; eleven source/meta files match the tested copy and the new GUID is unique.
- Evidence and short native-frame video: `Reports/DescentMotionOct16/`. No player build/sign/install, primary Editor or real-save changes. Full unrelated suite and physical iPhone FPS not claimed.


## Centred controls and new story screens — 16 October 2026

- [x] Reproduced two real failures before editing: a comic paragraph was 19.44 reference units off the paper's horizontal centre, and a plain reference action lacked any shared centring owner. Kept native before evidence.
- [x] Centred the visible multi-line comic/NPC paragraph as one readable block, with symmetric horizontal/vertical paper insets and room reserved for the page counter/name. Kept whole authored turns and stable same-panel geometry; adjusted paper height where necessary. Centred comic name/title reading insets on their boards.
- [x] Added a one-time pre-paint fallback through the existing CenteredContentRow for unbound simple reference buttons. Explicit icon/label groups, vertical stacks, subscription calendars and other composite cards retain their layout. No incomplete rows on icon-only controls; combined icon/label contents keep the four-logical-pixel gap. Existing artwork reused.
- [x] Native main PASS 18/18, additional controls PASS 5/5 (20 unique scenarios). Three new regressions include all page/phone/locale changes and both a plain action and an icon action. Existing nine-world comics/conversations, all authored turns, short phone/tablet, EN/RU/UK, Large Text, touch areas, callbacks/save routing and portrait animation passed. Main-menu sweep, subscription/shop footers, all settings pages and welcome/home/header layouts checked. Representative fresh comic/dialogue and compact late-world captures visually inspected; seven source/meta files match the tested copy; new GUID unique.
- Evidence and fresh screenshots: `Reports/UiCenteringOct16/`. No production bitmap assets, packages, story/progress changes, player build/sign/install or primary Editor/real-save changes. Physical iPhone and full unrelated suite not claimed.


## Writing-field and glyph centring correction — 16 October 2026

- [x] Reproduced the user's remaining visible comic defect independently by rendered pixels: title +4 px, speaker +3 px above the parchment centre. Previous rectangle-only centring claims did not cover this; retained before evidence.
- [x] Replaced empty font line metrics with actual glyph bearings in shared rows, vertical stacks and wrapped story text. Measured reading fields for the existing foliage board, nine map title skins, welcome and settings signs; mapped through real nine-slice/aspect geometry. Bound parcel/settings headings to the shared layout. Existing assets and four-logical-pixel icon/caption gap retained.
- [x] Native verification: 21 unique scenarios PASS after targeted reruns; 324 independent pixel comparisons across nine actual world skins × EN/RU/UK × three phone sizes, including Large Text. Maximum residual 1.0 device px. All comic panels, conversation turns, related control layouts and touch/save/route checks passed. Corrected obsolete inset and snow colour classification checks; explicit world/localized-name assertions prevent previewing world zero repeatedly.
- [x] Visually inspected fresh comic/NPC, tutorial, pause, shop, no-ads, welcome and settings captures. Eleven source/test files match the isolated tested copy.
- Evidence: `Reports/PlaqueCenteringOct16/`. Build and iPhone installation remain stopped at the user's request; no production PNG, rule, economy, real-save or primary Editor changes. Physical iPhone rendering not claimed.


## Portraits attached to their name boards — 16 October 2026

- [x] Reproduced Ada's actual-alpha silhouette gap: 2.62 logical px on a compact phone. Transparent bottom margins differ between residents; preserveAspect and entrance scale also affect contact.
- [x] Docked the painted bust base behind the nameplate rim in WorldComicLayout, retaining sprite/animation UVs and existing art. No runtime texture scans or new assets.
- [x] Native 9/9 scenarios PASS. Independent texture-alpha/rendered-board-mesh check covered 216 turns, ten speakers, nine worlds and three phone sizes; minimum support overlap 5.33 logical px. Complete localized comics, compact/tablet, speaking/focus, callbacks/save routing and dialogue/text centring passed. Fresh Ada/Rin and animated captures visually inspected.
- Evidence: `Reports/PortraitContactOct16/`; two source/test files match the isolated copy. Build/install remain stopped; no primary Editor, real-save or production bitmap changes.


## Respect settings row alignment — 16 October 2026

- [x] Applied the user's scope clarification: action buttons, information boards and item cards use centred content; navigation/settings/form/list rows retain authored leading labels and trailing controls. Updated AGENTS.md explicitly.
- [x] Reproduced the regression natively: the shared fallback changed a settings navigation caption from Left to Center. It now respects explicit directional alignment; settings icon/label columns, switches, radios and chevrons remain in place. Restore purchases explicitly retains the shared centred icon/caption action layout and four-logical-pixel gap.
- [x] Native 5/5 PASS: four pages, EN/RU/UK, phones/tablet, Large Text/safe areas, semantic row alignment, trailing controls, parameter persistence/reload, legal/store/back routes and normal action centring. Fresh settings pages visually inspected; three source/test files match the isolated copy.
- Evidence: `Reports/SettingsAlignmentOct16/`. No new assets, gameplay/economy/save changes, build/install or primary Editor changes.


## Requested iPhone installation — 16 October 2026, build 202610162

- [x] User explicitly authorized this deployment. Synchronized all 2,568 current Assets/Packages/ProjectSettings/Data inputs into the isolated build copy; main inputs unchanged during export. Preserved main Editor state, old exports and backups.
- [x] Unity iOS IL2CPP export succeeded (0 errors/360 warnings); Xcode Release build and existing Apple automatic signing succeeded. Strict deep signature check passed. Latest settings, board/glyph and portrait corrections included.
- [x] Installed 0.1.0 (202610162), com.vickand.cascade, on the paired iPhone 17 over Wi-Fi. CoreDevice verified the exact installed build, launch and a subsequent live game process. No uninstall/reset/restore or production configuration changes.
- Evidence: `Reports/iPhoneInstall-202610162/`; export and signed app retained under Builds/CASCADE-iOS-202610162 and its -Device sibling. Physical visual/audio/FPS validation not claimed; further build/install requires a new user request.


## Reported Descend/tutorial and character motion repairs — 17 October 2026

- [x] Reproduced repeat films, blocked Descend during swap coaching, non-looping speech and non-finite secondary motion at uneven frame durations. Preserved before evidence; strengthened the numeric regression to explicitly reject NaN/infinity.
- [x] Accumulated genuine fixed 120 Hz decoration ticks; softened/bounded scarf lift while retaining visible tail motion and resting cloth contact. Acknowledged films persist a viewing marker before optional practice; swap guidance no longer locks unrelated controls. Fresh mechanic introductions, effect replay and mastery semantics retained; no save schema/enum/rule change.
- [x] Reduced durability plaque and type to sit inside the timber. Reused intact authored apple-bite drawings without optical-flow deformation/overlaid idle eyes; retired unused runtime deformation bytes. Slower dialogue jaw cadence now loops with phrase pauses. No new PNG art.
- [x] Native latest PASS 24/24 distinct targeted scenarios, including disk persistence, retry/interruption, real Descend, fresh mechanics/replay, touch/pause, multi-stage flow, EN/RU/UK phone layouts, 7,200 uneven updates, 30/120 Hz physical constraints, feet/cloth contact and independent intact-drawing pixel comparisons. Thirteen source/meta files match the tested copy; GUID unique; no C#/shader compile errors. Fresh welcome, plaque and bite frames visually reviewed.
- Evidence: `Reports/ReportedMotionOct17/`. Four unrelated broad-suite failures reproduced on the previous implementation and recorded separately; full unrelated suite not claimed green. Main Editor and real saves preserved. No player build/install; iPhone remains on 202610162 pending a new deployment request.


## Completion actions inside the timber frame — 17 October 2026

- [x] Reproduced missing Next after the first clear and Retry/Map extending behind foreground frame art. Removed the first-result-only map redirect at the user's request; first and later ordinary results offer the same available Retry/Next/Map actions. Updated the first-play flow note; kept explicit Map, pending parcels and campaign-end handling.
- [x] Added a Win-only shared footer fit inside the side reserve, minimum 44-logical-pixel touch widths, existing square amber faces/pictograms, centred primary action and four-pixel gaps. Reused compact Next/Дальше/Далі on narrow views instead of crowding long translations. No new production art.
- [x] Native final PASS 5/5. Actual opening comic/three-stage first victory, map and next-node paths, retry without duplicate reward, nine phone/locale combinations (320/390/430 widths × EN/RU/UK), Large Text, real translated captions, full frame/control bounds, touch size and visible glyph padding. Fresh screenshots visually inspected; seven source/meta files match the test copy, GUID unique, no new C#/shader compilation errors.
- Evidence: `Reports/CompletionFooterOct17/`. Extended only the test fixture's existing precise cold-localization diagnostic expectation through teardown while independently requiring actual tables/translations. Main Editor and real saves preserved; no player build/install. Full unrelated suite not claimed.


## Inventory translation, level-finish continuity and fixed upper timber — 17 October 2026

- [x] Reproduced missing first-use Toolbag text in all locales and identified two stale undo source keys. Added three matching EN/RU/UK TSV/StringTable entries; actual first-use help, tray and all tool captions now resolve.
- [x] Restricted both existing frame shaders' upper-corner wind deformation to vegetation sampled from the fixed original artwork. Independent comparisons preserve all 60,530 menu / 10,600 gameplay timber pixels; foliage still moves. Corrected baseline moved 40,905 menu timber pixels.
- [x] Added a short landed-row beat and finite 0.64-second dissolve to the already committed result; removed competing result-menu entrance and early scenery hiding. Temporary image never blocks controls and is released on navigation, focus/pause, disable and Reduced Motion. No new PNG assets, reward callbacks or gameplay mutations.
- [x] Reproduced a 148.593-unit checkpoint start jump. Receiver animation now starts at its actual geometry and meets the stable checkpoint position/height including sill clearance; endpoint/skip settlement is exact.
- [x] Native final PASS 9/9 with first-play and real-time stage descent, reward/retry invariants, interruption/cleanup, EN/RU/UK help and all mechanics, pixel checks and fresh phone-size captures. Fourteen affected source/data/meta files match the disposable validation copy; new GUIDs unique; final log has no C#/shader compile errors.
- Evidence: `Reports/InventoryFinishFrameOct17/`. Main Editor and real saves preserved. No new player build/install; iPhone remains on 202610162. Full unrelated suite and physical-phone smoothness were not revalidated.


## Brighter green current-island highlight — 17 October 2026

- [x] Strengthened the existing silhouette glow and gently brightened only the current island with mint light. Preserved current/pinned-island logic, geometry, text, interaction and progression; reused existing artwork/shaders with one owned map material and no new PNGs or particles.
- [x] Existing native map-flow checks PASS 2/2, temporary capture PASS 1/1. Visually reviewed first/ninth worlds at 390×844 RU and 320×693 UK with Reduced Motion; three production files match validation copy, no C#/shader compile errors.
- Evidence: `Reports/CurrentIslandLightOct17/`. No player build or phone installation; physical iPhone still has 202610162.


## Parcel interstitial routing and missing extra-video offer — 17 October 2026

- [x] Reproduced three failures: due automatic ad skipped before a parcel, Next bypassing that check, and the bonus offer disappearing when no video is ready. Preserved before source/XML.
- [x] Moved the existing single automatic eligibility check to the shared saved-completion continuation. Close/failure resumes that same completion from the foreground loop; duplicate presses and stale navigation cannot bypass/reopen it. Unready ads never block earned parcels. Existing alternating-level cadence, replay/final/no-ads/privacy guards, reward amounts and save schema retained.
- [x] Separated bonus eligibility from availability. Shared button stays visible with translated loading/unavailable status, enables on video readiness without replaying the reveal, and respects hidden offers/verified bonuses. No provider consent/retry/configuration change or new art.
- [x] Native PlayMode PASS 13/13; EditMode PASS 36/36; repeated the readiness case for six full EN/RU/UK phone renders at 320×693. Forty-nine distinct checks passed, no C#/shader compile errors; six affected files match validation copy, regression GUID unique.
- Evidence: `Reports/ParcelAdsOct17/`. Native SDK video delivery on the physical iPhone remains unverified; tests use controlled preview/readiness. Main Editor, real saves and phone app preserved. No new player build/install; phone remains on 202610162.


## Mixed-mode shape size — 17 October 2026

- [x] Matched mixed-row shape bodies to the shared ordinary three-shape footprint. Reused board sizing and existing meshes; kept letter tiles and target receiver sizes. Dense rows retain adaptive containment inside the existing row spacing; gameplay and save state unchanged.
- [x] Native temporary visual review PASS for 18 phone cases (390×844 / 320×693, EN/RU/UK, 5/7/8 pieces), with exact reference-width assertions in the ordinary 390px mixed row; selected-letter motion check PASS. Visually reviewed ordinary and dense phone renders. No permanent sizing test added.
- [x] Two additional old PlayableTests checks fail identically on the saved before sources (target magnifier pause; loaded sag), so the broader selection is 2/4 and is not claimed green. Source hashes and before/after XML preserved in `Reports/MixedShapeSizeOct17/`; no C# compile errors.
- No player build or iPhone installation; physical phone remains on 202610162.


## Localization preload error — 17 October 2026

- [x] Reproduced the screenshot exception on rapid language changes: baseline strict regression 2/3 PASS, 1/3 FAIL with the exact `Failed to initialize localization, could not preload string tables` error. Identified overlapping locale initialization resetting the database preload handle.
- [x] Serialized all runtime locale assignments in the player loop, coalesced queued choices and refreshed reference captions once the final language loads. Existing preference persistence and system-language detection preserved; no synchronous wait, error masking, package/asset/schema/rule changes.
- [x] Added strict cold RU/UK and rapid-choice/reload regressions; removed the old preload-error allowance from AccountReferenceTests. Native validation PASS 7/7 including prior system/legacy language, settings and inventory cases; zero localization and C# compile errors. Four affected source/meta files match the validation clone; new GUID unique.
- Evidence: `Reports/LocalizationStartupOct17/`. No player build or iPhone installation; physical phone remains on 202610162, so this fix is not yet installed there.


## Shorter hanging-effect action beat — 17 October 2026

- [x] Reduced tool effect display from 4.2s to 2.8s (Reduced Motion: 2.6s to 2.1s), trimming only the final hold. Arrival/pulse/caption reveal/fades, wind and non-tool timing preserved; no rules or state changes.
- [x] Temporary clone-only native actual-contact review PASS in both motion modes: readable caption, pause/resume, returned input, consumed prop hidden and unchanged committed state/move count. Measured normal reaction 2.800s / full command 3.125s; Reduced reaction 2.101s / command 2.160s, excluding explicit pause. Phone screenshot visually reviewed; source matches tested copy. No permanent sizing/timing test added.
- Two older PlayableTests failed before the intended timing checks (first-use teaching; reaction not observed); baseline was not rerun and the broader selection is not claimed green. Evidence: `Reports/HangingEffectTimingOct17/`. No build/install or physical-phone timing review.


## Explicit ad scenario and device test screen — 17 October 2026

- [x] Recorded user-approved override in `Docs/ADS_FLOW_OCT17.md` and marked C16 cadence superseded. Interstitial every second new whole level, including world finals; retries/stages/replays do not count. No-ads removes interstitial only.
- [x] Added verified, parcel-scoped zero-value opening receipt. Closed parcel waits for Open with video; cancellation/unavailability leaves it pending with Later. Separate More with video remains after base rewards. Initial proof awards no currency/hints; existing base/bonus claims and sandbox isolation preserved. Reset all entry state, including trophy collection entry.
- [x] Enabled clearly labelled local ad screens by default in existing test monetization, including device runtime. Two-second interstitial and five-second foreground rewarded simulation; early close/confirmation gives no reward. Real SDK consent/callback path preserved with local testing off. Reused existing skins/art with shared button layout.
- [x] Native EditMode PASS 42/42; PlayMode PASS 18/18; final five scenario cases repeated PASS after the test artwork and collection-entry refinement. Sixty distinct checks, 11 final phone captures, EN/RU/UK and small-phone fit, cancellation/late callback, no-ads, serialization and save-failure checks; no C# compile errors. Sixteen affected source/meta files match the clone, two new GUIDs unique.
- Evidence: `Reports/AdScenarioOct17/`. No player build/install; iPhone still has 202610162. Local simulation validates the flow; real AdMob delivery and physical-phone callbacks were not tested.


## Authored platform damage, physics and clean iPhone handoff — 17 October 2026

- [x] Replaced rejected procedural crack overlay with image_gen-authored intact/fine/deep/broken states for all four platform styles. Cracks span the full board; the heavily damaged state stays connected until final break. Preserved asset GUID/import settings and archived the old atlas; removed overlay-only code/shader/metas.
- [x] Weak-edge crossing strengthens the whole authored board state with a restrained recoil/chip cue. Aligned state crops by attachment bolts. Seated bridge stop and debris collision surfaces on the current crown; rope-hinged break carries supported pieces. Solid pooled splinters tumble/fall, respect pause/Reduced Motion, fade in-camera and retire outside camera. Receiver figures use their visible centres.
- [x] Moved durability plaque to the sampled painted lower edge, with its complete bottom inside the board during sag/roll; existing size/art retained.
- [x] Native targeted validation PASS 6/6, phone visuals/47-frame actual break sequence reviewed; EN/RU/UK at 320/390px, cosmetic state invariants and cleanup verified. Earlier three legacy first-use fixture failures retained and full-suite green not claimed. Evidence: Reports/PlatformStatesOct17/.
- [x] Unity iOS IL2CPP export 202610171 succeeded: 0 errors/355 warnings; 2,583 authoritative build inputs unchanged during export. Signing/clean-device install is recorded in the deployment report once completed.

- [x] Signed and installed 0.1.0 (202610171) on the physical iPhone 17; exact version and clean container verified before first launch, no saved profile or backup restored. Preserved older exports/apps and primary Editor; evidence: Reports/iPhoneInstall-202610171/.
- First foreground launch is blocked by iOS device lock (error 7); requested unlock. Physical launch/liveness, visuals/audio/FPS not yet claimed.


## Shop papyrus/footer spacing — 17 October 2026

- [x] Lowered the paginated trophy paper, copy and buy action together; balanced arrow/paper and paper/action gaps using painted-edge padding. Extended the existing swipe viewport to the moved paper. Reused skins and icon/caption centring; no other layout or gameplay changes.
- [x] Native temporary phone review PASS for nine combinations (588/390/320px × EN/RU/UK, large text at 320). Gaps equal within 0.001px; body/title untruncated, button clear of the physical bottom. Final renders visually reviewed; source matches clone. No permanent low-impact layout test.
- Evidence: Reports/ShopPapyrusOct17/. No new build/install; iPhone 202610171 does not include this follow-up.


## Pause idle without apple bites — 17 October 2026

- [x] Disabled authored apple-bite frames and scripted chewing only in pause. Retained the original whole apple/arm cutout, ordinary blinking, gaze, ears, head, breathing, scarf, tail and whisker idle. Other callers keep the previous default.
- [x] Native targeted PlayMode PASS 2/2; 20-second simulated pause review confirms zero bite/frame/mouth and unchanged other idle motion. Original authored-bite regression still passes; final screenshot reviewed, three source hashes match validation clone. No permanent preference test added.
- Evidence: Reports/PauseAppleIdleOct17/. No build/install; iPhone 202610171 does not include this follow-up.


## Gameplay control reference proportions — 17 October 2026

- [x] Enlarged top controls and bulb/help glyphs. Reused cream/green square card skin for spool and bag, larger centred illustrations, white stock/plus badges and existing floral trim. Disabled cream remains opaque; no new PNGs/import/GUID changes.
- [x] Made the central action taller/shorter with a centred caption, inset the footer inside the posts and lifted precision controls above the taller footer. Kept rope attachment width, rules, commands and state unchanged.
- [x] Native targeted baseline/final PlayMode PASS 1/1 each. Final nine phone/language combinations (588/390/320 × EN/RU/UK, Large Text at 320), visible non-overlapping controls, exact square proportions, caption centring, EventSystem hits, actual help/bag callbacks and precision clearance PASS. Final RU/UK screenshots reviewed; six source hashes match clone. No permanent low-impact layout test added.
- Evidence: Reports/GameplayControlsOct17/. No build/install; iPhone 202610171 does not include the latest HUD, pause or shop spacing follow-ups.


## Victory hero clearance and accurate comic progress — 17 October 2026

- [x] Lowered the existing victory mouse by 28 logical units, retaining size, hop and all other pose motion. Native nine phone/language cases (588/390/320 × EN/RU/UK, Large Text at 320) sample 20 seconds/two hop cycles: conservative clearance from the Moves caption is 9.661–22.058px, with the original 0.035 maximum hop.
- [x] Replaced artwork-panel progress with actual readable-turn progress. Every Next now increments; final count matches the full comic. Extended the existing all-nine-worlds regression without changing panel indices, story receipts or saves. Baseline reproduced 1/4 instead of 1/8.
- [x] Native targeted PlayMode PASS 3/3: all worlds/panels and pinned stage, nine victory cases, nine comic phone/language cases with actual button navigation and unchanged puzzle state. Final selected renders reviewed; three source hashes match clone. No permanent positioning test or new assets.
- Evidence: Reports/CompletionComicOct17/. No build/install; physical iPhone 202610171 does not include these follow-ups.


## Gameplay padding, receiver centres and one tutorial action — 17 October 2026

- [x] Reduced side controls to 64×64 and central action height to 56; kept the whole row inside the frame. Fitted spool/bag illustrations into the actual cream field with ≥2 logical pixels on every side. Reused skins/art; preserved badge ownership after checkpoints and shared caption centring.
- [x] Removed the goal-shape vertical bias and centred the entire socket row in the painted cream receiver. Knots/continuous ropes and fracture pivots now follow the painted suspension pins; receiver ends meet the painted eyes.
- [x] Tutorial has only Try it: removed close and secondary film button graphics, passive page indicators with swipe retained, hand targets updated. All 27 mechanics × EN/RU/UK checked.
- [x] Native targeted 8/8 then final HUD/platform 7/7 PASS (12 distinct checks): nine HUD phone/language cases, exact receiver/shape alignment at stages, actual input, tutorials/swipe/cleanup, pin/rope alignment during motion/fracture, all platform styles, splinters and weight/state invariants. Final renders reviewed; 15 source hashes match clone. The obsolete corner-based fracture assertion was replaced with actual-pin coverage; its intermediate failure is retained.
- Evidence: Reports/HudAlignmentOct17/. No build/install; iPhone 202610171 does not include these follow-ups. No full-suite or physical-device validation claimed.


## Distinct melodic music for menus and all worlds — 17 October 2026

- [x] Replaced home/nine gameplay scores and added nine separately arranged world-menu themes. Home 78 BPM; maps 70–86; levels 100–126. Original tonal/pentatonic chamber material with articulated rhythmic motion, different acoustic voices and developing periods. Calibrated seven instrument banks; replaced layered ornaments with sequential melody and shortened reverb. Existing music GUID/imports retained; sources stay outside Assets.
- [x] Separate map/comic/dialogue and gameplay routing; smooth 0.85-second handoff through quiet, preserving the incoming first note and reversible playback. Updated the existing library/builder; no new audio manager, sampler, packages or save/rule changes.
- [x] Objective 19-track harmony/PCM/tuning/loop/source/import validation and 352 C# routing cases PASS. Native first 9/10, final 2/2 (10 distinct passing tests): every world/menu, real map entry, reversal/first note, mute, focus/reset/owner lifecycle and related audio regressions. Existing broad fixture preparation corrected; intermediate failure retained. Final 44 tested file hashes match clone.
- Evidence and listening previews: Reports/MusicOct17/. Artistic quality needs human listening; no full-suite, iOS build, installation or physical-device audio check claimed. Current iPhone build 202610171 does not contain this music revision.


## Normal checkpoint action proportions — 17 October 2026

- [x] Shortened only Checkpoint Next/Map controls, kept equal sizes and centred stack inside the existing safe band; minimum 48px-high target at supported phone sizes. Reused the shared green/amber skins and switched old light captions to the common InterfaceTitle font with optical centring. No new art or other screen changes.
- [x] Native baseline/final 1/1 PASS each; nine phone/language cases (588/390/320 × EN/RU/UK, Large Text at 320), art/fonts, frame/receiver clearance, no overlap/truncation, EventSystem hits, unchanged puzzle state and actual Next/Map navigation. Final RU390/UK320 renders reviewed; runtime source hash matches validation clone. Intermediate target-size/fixture failures retained transparently.
- Evidence: Reports/CheckpointButtonsOct17/. No permanent visual test, full suite, iOS build or install; iPhone 202610171 remains unchanged.


## Rooted flowers on spool and bag — 17 October 2026

- [x] Attached reused bouquets by their opaque leaf bases to the actual painted upper rim corners, clear of icons and stock/plus badges. Preserve contact while ButtonFeel scales on press/release and while the controls resize. Two runtime files; no new assets/components or other control changes.
- [x] Native baseline 1/1 and final 2/2 PASS: nine phone/language HUD cases plus both controls pressed/released at three sizes, source-rim alpha >0.94, no icon/badge overlap, max base/frame error 0.0000px, original input/padding/state checks retained. First geometric placement refined after texture inspection; final renders reviewed and source hashes match clone.
- Evidence: Reports/ToolFlowerMountsOct17/. No permanent visual test, full suite, build/install or physical-device check; iPhone 202610171 remains unchanged.


## Install current iPhone build 202610172 — 17 October 2026

- [x] Exact authoritative 2,601-input snapshot; Unity iOS IL2CPP export 0 errors/355 warnings; Xcode Release success; original/retained strict signatures pass. Latest music/checkpoint/flower validation hashes match. Main Editor and prior artifacts untouched.
- [x] Installed on paired physical iPhone 17; exact 0.1.0 (202610172), launch and process liveness confirmed by CoreDevice. Includes all current post-171 work.
- [ ] Previous profile recovery unresolved: pre-install container access failed even after unlock; normal install executed without uninstall/reset/restore, but prior profile was absent afterward and a new profile was created. No compatible local backup available; signing/storage entitlements unchanged. No claim of preserved progress; user informed.
- Evidence: Reports/iPhoneInstall-202610172/. Retained export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610172 and -Device. No physical visual/audio/FPS or full-suite validation claimed. Further deployment requires another user request.


## Softer shadows, first-entry performance and living bird calls — 17 October 2026

- [x] Fixed disabled mobile soft shadows using Low filtering, existing 1024 atlas / one cascade and fitted 24-unit distance; timber and rope shader quality variants included. Native identical-pose hard/soft/no-shadow captures visually checked.
- [x] Lazy active-world skies, catalog/mode/locale-keyed journey caches with live progress checks, reused label-scan storage and removal of repeated world-index lists. Optional first-frame asynchronous next-screen prefetch / existing worker certificate parse. No artwork/resolution downgrade, save/rule/schema/package changes or new audio sources.
- [x] Four new CC0 robin/wren phrases; visible call bursts, quiet offscreen answers, stereo/pitch variation and pause/reading/mute/focus/ad/reset lifecycle. Same two reusable sources; new decoded PCM bounded to 1.28 MB.
- [x] Baseline 1/1, final targeted native 22/22 and independent final performance/visual capture 1/1 PASS. In fresh Editor processes: level preparation 171.6 → 113.6 ms; map 70.8 → 45.8 ms; allocated Editor memory 824.3 → 680.8 MiB. Frame CPU remains similar; these are not weak-phone FPS claims. Broader pre-refinement 31/36; two map layout failure families also reproduce on original-source control and remain unrelated/open.
- [ ] Physical older-phone release-player cold load, sustained CPU/GPU/memory and speaker/headphone mix verification; no new iOS build/install done.
- Evidence: Reports/PerformanceOct17/; notes: Docs/PERFORMANCE_BIRDS_OCT17.md. Real saves and original scenes untouched; disposable instrumentation removed.


## Local ACE-Step 1.5 and coherent music preview — 17 October 2026

- [x] Explained the old Python/VSCO2 note-by-note procedure; acknowledged user rejection and separated technical checks from artistic quality. Unity was playback only, ACE-Step was not previously used.
- [x] Installed official pinned ACE-Step 1.5, isolated Python 3.12, compatible dependencies and complete turbo/embedding/VAE/1.7B models on the external disk. Added local Studio/API launchers; offline Apple MLX generation works.
- [x] Inspected Shape Mania: five music clips and 244 short sounds; three cached electronic music packages inventoried, five complete loops copied for local review without changing originals. No isolated melodic instrument bank found or third-party mashup made.
- [x] Generated one actual 96-second instrumental preview, seed/request/raw master retained. Stereo 48 kHz PCM signal checks and AAC export pass. Studio launcher loads offline and serves UI configuration. Validation servers stopped afterward.
- [ ] Listening acceptance of melody, harmony, phrasing and emotional direction; no claim that numerical tests establish taste. Unity music not replaced; no iPhone installation requested or performed in this task.
- Evidence: Reports/ACEstepOct17/; installation/run notes: Docs/ACESTEP_OCT17.md. No Unity package, runtime, source-audio, save or gameplay changes.


## ACE-Step form review, portable sample and world identities — 17 October 2026

- [x] Analysed the actual first take beyond basic PCM checks: moderate ~88–90 BPM, 4.94 dB interior RMS variation, tonal pitch profile and changed middle / return distributions. Kept the distinction between a signal-based inference and a human audition; exact verse/refrain and taste are not claimed verified.
- [x] Exported the full 96-second preview as MP3 and decoded it back to check duration/channels/finite samples/peak. Added chat delivery and retained report/output copies.
- [x] Saved separate future briefs for all nine worlds: menu 74–86 / gameplay 84–100 BPM, independent melodies and characteristic instrumentation; clear theme/refrain contrast, snow and rain identities, positive bittersweet mood.
- [ ] Listening acceptance and generation/integration of the remaining world tracks; no claim these are already rendered. Unity audio assets and iPhone installation unchanged.
- Evidence: Reports/ACEstepOct17/musical-analysis.json, analysis.png, mp3-export.json and world-music-direction.json; notes: Docs/ACESTEP_OCT17.md.


## Prepare a cyclic ACE-Step master — 17 October 2026

- [x] Corrected the distinction: the first 96-second fade-ended audition was not a prepared loop. Retained it and the raw original; produced a separate 80-second lossless cycle with contrasting middle and return.
- [x] Selected matching interior phrase/onset/pitch contexts and rendered a 2/3-second end-continuation/start overlap offline, preserving cycle length. Actual last→first waveform samples match adjacent source samples; no extra wrap step, fade to zero or silent interval.
- [x] Decoded PCM24 master and MP3 seam audition; finite samples/duration/channels/peak checks pass. The 24-second audition crosses the real loop seam at 12 seconds. Future nine-world briefs now explicitly require cyclic form and matching bar/harmonic/phrase boundaries.
- [ ] Human listening acceptance and target Unity import/device playback verification; no Unity runtime/assets or iPhone build changed.
- Evidence: Reports/ACEstepOct17/Loop/; master/output copies under AI_Tools/ACE-Step-Output/Cascade; no runtime DSP added.


## Review all nine musical identities against the story — 17 October 2026

- [x] Confirmed actual nine-world order/canonical RU names and reviewed current briefs against WorldPrologues.cs. Clarified actual state: one generic generated composition, one cyclic edit, zero rendered world-specific compositions.
- [x] Added distinct metre/pulse, melody contour/hook, development, story purpose, pitfalls and separate menu/gameplay treatment to every world. Replaced uniform bright-middle formula with context-appropriate contrast.
- [x] Corrected Great Beacon's premature-finale risk and Star Observatory's overly tranquil premise to match the unresolved theft/revelation/shared-repair arc. Separated rain/lagoon/snow musical identities and recorded album-level palette/hierarchy/emotional arc.
- [ ] Actual rendered-world listening, taste and whole-soundtrack cohesion remain unverified. This review does not claim audition, additional audio generation, Unity integration or iPhone installation.
- Evidence: Reports/ACEstepOct17/composition-design-review.json and revised world-music-direction.json; notes: Docs/ACESTEP_OCT17.md.


## Complete all world compositions with local ACE-Step — 17 October 2026

- [x] Completed 19 selected compositions (24.38 minutes): reused the prepared ACE home take and generated 18 distinct menu/gameplay pieces for all nine reviewed world identities. 20 actual render attempts including rejected alternatives; requests, selected seeds, originals and provenance retained. No speed/pitch variants or procedural note reconstruction.
- [x] Prepared lossless phrase/context-matched loop masters, retained internal musical rests, checked PCM/wrap/encoded previews, measured final-loop dynamics and distinctness, and aligned loudness using fixed gain with true-peak headroom. Weak lagoon/snow candidate alternatives replaced; rejected versions/evidence retained. Numerical checks are not artistic listening approval.
- [x] Installed all 19 WAVs at existing GUID paths. Importer metadata/library/references/runtime code unchanged; prior WAVs archived outside Assets. Packaged full MP3 catalogue/playlist/seam examples and two nine-world montages.
- [x] Native Unity Editor PlayMode 8/8 PASS: all 19 imported streams wrap with live listener output; existing all-world menu/gameplay routing, handoff/reversal, mute/focus/reset and related audio checks. Tested clone/master/meta hashes match; temporary review test removed. Pure C# routing evidence retained.
- [ ] Physical-device audio/performance and subjective full-album listening acceptance; no iPhone build/install in this task.
- Evidence: Reports/WorldScoreOct17/; external masters/listening catalogue: AI_Tools/ACE-Step-Output/Cascade/WorldScoreOct17.


## Install current iPhone build 202610181 — 18 October 2026

- [x] Exact 2613-input authoritative snapshot; all 19 prepared ACE-Step loop masters and importer metadata match. Unity 6000.3.8f1 iOS IL2CPP: 0 errors/360 warnings; Xcode Release succeeded; original and retained strict signatures passed.
- [x] Installed over existing CASCADE on paired physical iPhone 17; exact 0.1.0 (202610181) confirmed. Launch and process liveness after four seconds confirmed by CoreDevice.
- [x] Current profile and recovery file privately backed up twice and byte-identical after update before first launch. No uninstall/reset/restore; main Editor, source inputs and older builds preserved.
- Evidence: Reports/iPhoneInstall-202610181/README.md. Retained export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610181 and -Device. Existing music validation 8/8 retained; manual physical visual/audio/FPS/resume and full suite not rerun for deployment. Further installation requires a new user request.


## Current island clarity and Mix map symbols — 18 October 2026

- [x] Removed the active island's 16% mint wash and extra material. Original authored colours/details remain; a separate exact-size alpha contour pulses green outside the island, with Reduced Motion/low-effects fallback.
- [x] Mix circle/heart use the same painted dimensions as Shapes, with a separate A at the same font/size as Words. Shared-height symbol rows retain deliberate centring and the four-logical-pixel caption gap. No new PNGs/importer changes.
- [x] Native existing map regressions 3/3; final capture review 1/1. 59 native PNGs: nine worlds, EN/RU/UK, 320/390px, Large Text, pulse extrema and actual selected modes. Final C#/shader compilation has no errors; four source hashes match tested clone. Initial capture fixture hit the normal mode-introduction story; corrected fixture and retained failed evidence. Temporary test removed.
- Evidence: Reports/MapFocusMixOct18/. No new phone build/install or physical audio/visual/FPS benchmark; installed iPhone remains on 202610181. Real saves, main scene and existing art preserved.


## Mix progression from the opening — 18 October 2026

- [x] Three-slot levels alternate Shapes/Words from the first Mix level; every four-or-more-slot level combines a dictionary word with shapes. Actual first three: Shapes, Words, Mixed. All 540 node positions retained in every locale; normal Shapes/Words routes unchanged.
- [x] Added lazy bijective Mixed source variants, preserving equality, sensor bindings, mechanics, budgets and published optimal paths. Kept historical content for pinned saves/checkpoints/replays. Reused exact recipe bytes; repinned source manifests. Design/compatibility: Docs/MIXED_JOURNEY_OCT18.md.
- [x] Native 16/16 EditMode plus two distinct PlayMode checks pass; actual first three gameplay rows and Mixed reload inspected. 11,223 stage-certificate replays; all category/locale stage/whole-level uniqueness gates pass. Server boundary 153 PASS with fake transport, including new/short Mix and forged-proof rejection.
- Evidence: Reports/MixedJourneyOct18/. No live cloud deployment, new player build, phone installation, physical input/FPS check or full historical suite. Current iPhone remains on 202610181; real saves/main scene preserved.


## Install current iPhone build 202610182 — 18 October 2026

- [x] Exact 2619-input authoritative snapshot includes the validated Mix journey, map focus/selector fixes and all 19 prepared music masters with unchanged importer metadata.
- [x] Unity 6000.3.8f1 iOS IL2CPP: 0 errors/360 warnings; Xcode Release succeeded; original and retained strict signatures passed.
- [x] Installed over existing CASCADE on the paired physical iPhone 17; exact 0.1.0 (202610182) confirmed. Launch and process liveness after four seconds confirmed by CoreDevice.
- [x] Current profile and recovery file privately backed up twice and byte-identical after update before first launch. No uninstall/reset/restore; main Editor, source inputs and older builds preserved.
- Evidence: Reports/iPhoneInstall-202610182/README.md. Retained export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610182 and -Device. Existing feature validation retained. Manual physical visual/audio/FPS/resume and full suite not rerun for deployment. No cloud deployment. Further installation requires a new user request.


## Raise home account and remove-ads shortcuts — 18 October 2026

- [x] Reproduced both buttons staying in the logo band. Raised both 90 reference pixels using one shared value in construction and live adaptive layout; existing horizontal positions, skins, sizes and actions retained.
- [x] Native Unity PlayMode: three distinct checks passed. Ten final captures verify EN/RU/UK, three phone aspect ratios and linked-account visual state; touch targets, raycasts, alignment and neighbouring screens checked. Temporary capture fixture removed.
- Evidence: Reports/HomeShortcutOct18/README.md. Physical iPhone remains on build 202610182; this correction is in source and has not been installed. No saves changed.


Home shortcut follow-up — 18 October 2026: moved both upper home shortcuts inward by 2 logical canvas pixels each, compensating for reference-stage scale. Initial positions and live layout use the same inset. Height, dimensions, skins and callbacks unchanged. Targeted diff checked; native tests not rerun for this minor spacing change. Evidence: Reports/HomeShortcutInsetOct18/summary.json. Physical iPhone remains on build 202610182.


## Media audio in silent mode — 18 October 2026

- [x] Added iOS Playback + MixWithOthers category with a small AVFoundation bridge. System media volume, game music/effect toggles, Unity interruption ownership and foreground-only playback remain in control.
- [x] Configure once per allowed playback session and after focus/ad/output reset, with no native polling. Android hardware keys explicitly target STREAM_MUSIC on the current activity/UI thread.
- [x] Existing native PlayMode audio checks 5/5 pass; actual iOS ARM64 compilation/link + exported symbol verified; iOS/Android managed branches compile; actual AVAudioSession simulator probe verifies category/options, 20 repeat calls, unchanged volume and recovery after category reset.
- Evidence and limits: Reports/MediaAudioOct18/README.md and Docs/MEDIA_AUDIO_OCT18.md. Physical silent switch/Android device and full new player not tested; iPhone remains on build 202610182. No user saves modified or device install performed.


## Wildlife contact and coupled platform physics — 18 October 2026

- [x] Replaced invented frame/shape-tip perches with real support contacts; tray contacts follow its actual suspension transform; authored toe tips for all ten bird species remove padding gaps. Continuous landing/takeoff wing clock and gentler flight curves/corridor mapping.
- [x] Both textured timber halves detach into the existing pooled local physics scene with the splinters, remain opaque in flight and retire off camera or by bounded stranded lifetime. The row starts descending promptly after release.
- [x] Actual mass now drives sag, inertia, damping and load-centre torque; lifting removes load; block feet meet painted wood; physical proxies share the same mass.
- [x] 56 distinct targeted native checks pass (33 EditMode, 23 PlayMode), including 30/60/120 integration steps, actual hold/return, large fragments and regional contact close-ups. Latest final run 14/14; toe-contact follow-up 6/6. Five legacy PlayableTests failures reproduce unchanged on pre-change sources; no tests disabled.
- Evidence and validation limits: Reports/LivingPhysicsOct18/README.md, summary.json and Captures; Docs/LIVING_PHYSICS_OCT18_RU.md. Domain/Solver/Data/packages/settings and source raster art preserved. Temporary review fixture removed. Device build/install and physical low-end FPS not performed; iPhone remains on 202610182.


## Install current iPhone build 202610183 — 18 October 2026

- [x] Exact 2627-input authoritative snapshot includes the latest Home shortcuts, media audio, wildlife/platform physics, Mix/map fixes and all 19 prepared music masters with unchanged importer metadata.
- [x] Unity 6000.3.8f1 iOS IL2CPP: 0 errors/360 warnings; Xcode Release succeeded; original and retained strict signatures passed.
- [x] Installed over existing CASCADE on the paired physical iPhone 17; exact 0.1.0 (202610183) confirmed. Installed; foreground launch was not confirmed. See launch-state.json for the device response.
- [x] Current profile and recovery file privately backed up twice and byte-identical after update before first launch. No uninstall/reset/restore; main Editor, source inputs and older builds preserved.
- Evidence: Reports/iPhoneInstall-202610183/README.md. Retained export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610183 and -Device. Existing feature validation retained. Manual physical visual/audio/FPS/resume and full suite not rerun for deployment. No cloud deployment. Further installation requires a new user request.


## Platform splinter breakup — 18 October 2026

- [x] Replaced two whole falling timber halves with 16 closed, textured, staggered splinters; original board and top hide on the same release frame.
- [x] Pooled local physics supplies gravity, rotation, lower-deck rebounds and low-friction forward slip; invisible released decks cannot catch debris. Dust/sparks preserve active timber; quality/reduced-motion paths retained.
- [x] Nine distinct targeted native PlayMode checks pass; final affected checks 3/3. Final cascade: 8 observed rebounds; all 16 fragments leave camera by 5.156s before fallback. Native frame sequence inspected; 37 captures and numerical trajectories retained.
- [x] All 2623 unrelated player inputs from 202610183 unchanged, including Domain/Solver/Data/packages/settings/raster assets/meta. No save changes.
- Evidence: Reports/PlatformSplintersOct18/README.md and summary.json; Docs/PLATFORM_SPLINTERS_OCT18_RU.md. No new phone build/install, physical FPS benchmark or full-suite claim; iPhone remains on 202610183 with prior breakup.


## Install current iPhone build 202610184 — 18 October 2026

- [x] Exact 2627-input authoritative snapshot includes the latest Home shortcuts, media audio, wildlife/platform physics, the new splinter breakup, Mix/map fixes and all 19 prepared music masters with unchanged importer metadata.
- [x] Unity 6000.3.8f1 iOS IL2CPP: 0 errors/355 warnings; Xcode Release succeeded; original and retained strict signatures passed.
- [x] Installed over existing CASCADE on the paired physical iPhone 17; exact 0.1.0 (202610184) confirmed. Installed; foreground launch was not confirmed. See launch-state.json for the device response.
- [x] Current profile and recovery file privately backed up twice and byte-identical after update before first launch. No uninstall/reset/restore; main Editor, source inputs and older builds preserved.
- Evidence: Reports/iPhoneInstall-202610184/README.md. Retained export and signed app under CascadeUnity/Builds/CASCADE-iOS-202610184 and -Device. Existing feature validation retained. Manual physical visual/audio/FPS/resume and full suite not rerun for deployment. No cloud deployment. Further installation requires a new user request.

## Menu music audition from archived theme — 18 October 2026

- [x] One local ACE-Step source-conditioned cover of the archived pre-ACE menu, seed 181026078, strength 0.75. Sparse piano/guitar homesick, romantic, nostalgic direction; all other compositions retained.
- [x] Full 147.68s render archived; 92.28s phrase-matched loop prepared, 48kHz stereo PCM24 and playable MP3 plus seam sample. Fixed gain only, −20 LUFS; decode, clipping, silence and wrap-continuation checks passed.
- [x] Original source and every shipped Atmosphere file/importer hash unchanged; generation server stopped. No Unity rebuild or phone install. Artistic listening approval and final in-game replacement pending the user's audition.
- Evidence: Reports/MenuHomeRevisionOct18/README.md, request.json and review.json; masters under external ACE-Step-Output/Cascade/MenuHomeRevisionOct18.

## Shared chamber world score — 18 October 2026

- [x] Refined the home melody/harmony and prepared nine distinct world themes in the approved intimate style. Strict diatonic guides, existing acoustic recordings, rare tuned winter bells; every selected master retains a source harmonic fingerprint similarity of at least 0.9799.
- [x] Fixed local ACE-Step MLX source initialization to match its PyTorch implementation; retained reversible backups, diffs and source hashes. Rejected freer/majorized takes archived. No AI runtime ships in the game.
- [x] Installed ten phrase-matched stereo PCM24 loops with unchanged GUIDs/importers. Map/comics/gameplay share each world's performance; removed nine retired map tracks/references after reference checks and archive verification. Musical WAV footprint reduced by 150,031,116 bytes.
- [x] Native Unity PlayMode 9/9 passed: all ten actual streaming wraps and listener output, all nine map worlds, map/level source-position continuity, home/world handoff, mute/focus/output recovery and related cue/haptic checks. Pure routing: 352 selections plus fallbacks. Main scene/save, core rules and packages unchanged; no new iPhone install or physical audition claim.
- Evidence and listening limits: Reports/ChamberWorldsOct18/README.md, native/, per-track reviews and EngineFix/. MP3 delivery: chat visualization directory chamber-worlds-oct18, including a 126-second world preview and ten-track ZIP.

## App Store Connect preparation — 18 October 2026

- [x] Registered app `6824290826` / `com.vickand.flyingladders`: saved EN/RU/UK metadata, Puzzle/Word categories, age questionnaire (Apple 9+), reviewer contact/instructions, free download and manual release.
- [x] Created six consumable coin packs and three ad-removal subscriptions with three-language metadata, shared same-level group, USD base prices and Apple regional pricing; verified app and all nine products available in the explicitly approved 173 territories excluding CHN/VNM, no automatic future territories.
- [x] Published owner-approved support/privacy/terms and preserved deletion page on existing public Site version 2. Five routes verified unauthenticated HTTP 200; linked privacy/support fields for all store locales. Existing Site backend checks 28/28 and framework build passed.
- [x] Saved public store identity/catalog configuration; explicit iOS helper reads registered identity and version, restores previous Editor settings. Isolated Unity compile-only succeeded, 0 compiler errors. No device build/install or save changes.
- [x] Prepared App Privacy/accessibility evidence and Google Play metadata draft without asserting unverified provider behavior, accessibility support, rights or financial declarations.
- [ ] Screenshots and product-review screenshots paused by owner; none uploaded. No binary uploaded or review submission. Products remain MISSING_METADATA. Production services/release guards remain off pending real configuration and sandbox/device checks.
- Evidence and remaining owner-only fields: Docs/APP_STORE_SETUP_OCT18.md; Reports/AppStoreConnectOct18/verified-store-state.json, public-pages-verification.json, privacy-accessibility-audit.md and compile-verification.json. Phone remains on previous build 202610184; no screenshot or app release authorization is implied.


## Google Play setup and protected key — 19 October 2026

- [x] Replacement service-account JSON copied exactly to the explicitly requested MY DISK root, mode 0600; read directly there. OAuth and Android Publisher access verified for the expected service/project/app. Old invalid-key blocker resolved; no private credential stored in the repo/player.
- [x] Committed and read back EN/RU/UK store texts and public support email/URL. Corrected Ukrainian cancellation wording to Google Play. No screenshots uploaded.
- [x] Created and read back three DRAFT ad-removal subscriptions (week/month/year), translations and Google-converted regional prices for 173 returned regions excluding VN. Plans not activated. Six coin drafts are prepared; Google creation rejected until a binary requests Billing.
- [x] Explicit isolated offline Android QA build route preserves normal production guards and live-service switches. Unity build: 0 errors/16 warnings; bundle/signature/package/API36/ARM64/Billing/no-ad-ID and six native ELF16KiB checks passed. Signed AAB retained locally outside the repo. No physical Android/full-suite claim.
- [x] Owner then forbade uploading the build: active upload stopped immediately, existing edit showed zero bundles and zero releases on every track, unfinished edit deleted. No internal or production release; do not resume upload without a new instruction. iPhone/save data unchanged.
- [ ] Browser-only App content/IARC/category/pricing declarations, final Families/Data Safety, and largest existing email tester-list selection remain unconfirmed. Prepared answers distinguish saved forms from drafts. Current name Flying Ladders; possible Fitting Ladders rename awaits clarification.
- Evidence: Docs/GOOGLE_PLAY_SETUP_OCT19.md; Reports/GooglePlayOct19/verified-store-state.json, console-remaining.md, readback-*.json, aab-validation.json, native-16k-validation.json and stopped-upload-state.json.


## Maximize store metadata without uploads — 19 October 2026

- [x] Rechecked the exact Apple app and saved EN/RU/UK TestFlight descriptions, feedback email, site/privacy links and beta reviewer contact/guest instructions. Copied the already owner-approved private review contact without publishing it.
- [x] Filled Marketing URL and optional Privacy Choices URL for all three App Store languages. Existing subscription-group localizations complete; clarified three product reviewer notes to describe interstitial-only ad removal without an unverified server-operation claim.
- [x] Independent API readback confirms these values. Apple: no uploaded/selected build, zero store screenshots, PREPARE_FOR_SUBMISSION/MANUAL. Google: three DRAFT subscriptions, zero coin products, zero bundles/releases; read-only edit discarded. No screenshots made, files uploaded, invitations sent or review submitted.
- [ ] Google consumables await an accepted Billing bundle, currently forbidden. Console-only privacy/content/IARC forms, final Families and production processing, legal/financial declarations and largest email tester list remain unconfirmed. Saved name stays Flying Ladders pending explicit rename clarification.
- Evidence: Reports/StorePreparationOct19/README.md, summary.json, apple-after.json and google-after.json. Existing player source/assets, prices, IDs, cloud/ads flags, iPhone and saves unchanged in this additional pass.


## Live services access audit — 19 October 2026

- [x] Rechecked current configs: Player Accounts Client ID absent, six production AdMob IDs absent, production cloud/reward/receipt/deletion verification false. Existing Google Play and App Store metadata access is separate and already verified.
- [x] Fresh project-scoped `ugs env list` failed with permission denial (exit 1); raw CLI/auth output suppressed. Current blocker is project permissions, not a proven missing local key.
- [x] Prepared exact owner access checklist and documented the existing hosted Google/Apple flow, callbacks, AdMob OAuth and restricted API creation limits. No LevelPlay or new native sign-in package installed.
- [ ] Unity permissions, Player Accounts Client ID and AdMob access/IDs are required for live deployment and device/sandbox checks. Existing production flags remain off. Owner now authorizes a test-build upload after service preparation; no immediate upload of the offline AAB, screenshot, review or public release occurred.
- Evidence: Docs/LIVE_SERVICES_ACCESS_OCT19_RU.md and Reports/LiveServicesAccessOct19/status.json.


## Unity Cloud role recheck and module deployment — 19 October 2026

- [x] Owner-issued roles now allow environment access and Cloud Code deployment with the existing configured CLI credential. Production GUID read back; separate development environment created. No replacement Unity key required.
- [x] Owner-confirmed Player Accounts Client ID saved. Live OAuth entry rejects offline_access and accepts openid for desktop/mobile; importer corrected. Isolated native Unity compilation/config assertions passed; Unity-created settings and meta retained, cloud/purchase/reward flags remain off.
- [x] 68 actual client and 157 Cloud Code checks passed. Current CascadeAccount deployed and read back in development/production, all eight endpoints present. Disposable development sign-in and PurchaseIdentity returned 200; ReadAccount stops at missing Unity Secrets; temporary player deleted.
- [x] Existing public Site version 2 redeployed with public project/environment/bundle/Sandbox metadata and complete known deletion-environment list. API health/storage reachable; publisher and deletion readiness still false. Existing DATA_KEY and bearer secret preserved.
- [x] Corrected CLI helper to deploy .sln (the .ccmr Editor reference silently yields an empty CLI plan), updated obsolete synthetic-lock deployment guidance. Python syntax/diff checks passed.
- [ ] Two Unity Secret Manager values with Cloud Code access remain absent; secure bearer provisioning pending. Auto-review rejected a command that would emit the Google private key and a generated token into output before execution; nothing transferred/rotated, owner informed and separate approval requested. Publisher/deletion secrets, push identity/maintenance, six AdMob IDs, live sandbox/SSV/account/device checks remain outstanding.
- Evidence: Docs/LIVE_CLOUD_DEPLOYMENT_OCT19.md; Reports/LiveServicesAccessOct19. No store binary/screenshot upload, review submission, invitations, iPhone install, save reset or production activation.


## Google server key follow-up and Worker correction — 19 October 2026

- [x] Owner explicitly approved the Google JSON transfer. Auto-review still rejected stdout disclosure; no key copied into source/player/public data or Sites, no existing secret rotated. Continued local GoogleAuth calls without secret output.
- [x] Verified Google ID-token minting, Play catalog access to all three Flying Ladders subscriptions and Pub/Sub access. Created only the dedicated flying-ladders-rtdn topic. Existing RevenueCat topic/subscriber unchanged; push subscription/IAM policy rejected for missing permissions.
- [x] Site runtime revision 3 applies public expected push email/audience. Production signed/unsigned webhook probes expose pre-existing 503, not successful notification processing.
- [x] Reproduced exact Vinext Worker failure, traced Apple SDK global-scope randomness; applied lazy official SDK factories and supported native fetch transport. Full build and 28 Node/SQL checks passed; all five actual Google-ID-token checks pass on the rebuilt local Worker. No crypto weakening or package upgrade. Source fix is retained locally; game service switches remain off.
- [ ] Source publication is blocked: official helper inside sandbox cannot resolve Git host; secret-bearing handoff to unsandboxed helper was rejected even after destination/helper checks. Separate exact-scope publication permission requested. Direct private JSON import cannot proceed until a supported secure source channel is available. Another Google service-account email is pending from owner; production Secrets, publisher credentials, push IAM/Play configuration, AdMob and full live checks remain outstanding.
- Evidence: Docs/GOOGLE_SERVER_SETUP_OCT19.md and Reports/LiveServicesAccessOct19/{google-publisher-key-check,google-rtdn-topic,google-rtdn-push-setup,google-rtdn-direct-smoke,fixed-worker-google-smoke,worker-source-hashes}.json. No store binary/screenshots/review or device/save changes.


## Owner action recheck and recovered Apple key — 19 October 2026

- [x] Searched owner-authorized MY DISK key locations without printing secret contents. Found SubscriptionKey_JP5FFCBZFA.p8, verified EC parsing and an actual App Store Server API Sandbox notification-history request for Flying Ladders: HTTP 200. No replacement Apple key is needed. Other apps' keys/configuration were not changed.
- [x] Current Google JSON still reads three subscription products (200). Dedicated topic IAM and authenticated push creation still fail 403; exact blocker remains iam.serviceAccounts.actAs. No subscription created or credentials rotated. No JSON for the alternative uskeep-revenuecat identity found in the searched accessible JSON files; absence is not asserted globally.
- [x] Rechecked Unity environment access and production CascadeAccount module visibility. Site health remains reachable (200); publisher/deletion credential readiness remains false. Google synthetic invalid receipt probe returned 404 No application was found; this is not proof of live receipt access or a successful transaction.
- [x] Updated existing Russian owner guide with exact Google IAM principals/scopes/clicks, later Play RTDN step, Apple agreement checks, six AdMob IDs/consent/SSV steps and Unity environment-secret fields. Distinguished immediate owner actions from later server/device validation.
- [ ] Server source publication/secure secret provisioning remain blocked as previously recorded. Existing Unity CLI login is not proof that deletion service credentials are already in Sites. No snapshots, binaries, App Review submission, device install/save change, or production feature activation in this recheck.
- Evidence: Reports/LiveServicesAccessOct19/owner-actions-recheck.json; Docs/LIVE_SERVICES_ACCESS_OCT19_RU.md.


## HumanWriting story and character voices — 19 October 2026

- [x] Read the HumanWriting skill and natural-writing checklist; reviewed the existing nine-world mystery and scene order. Retained culprit, scenes/speakers, gameplay, receipts and saved progress.
- [x] Reworked 225 authored positions / 675 EN/RU/UK values, including all 72 comic turns, selected island scenes, nine mission invitations and nine world transitions. Rin now grows from uncertain questions to practical leadership; residents speak through their trades and choices. Reduced explanatory morals and replaced several with actions or concrete observations.
- [x] Fixed contradictory wording of Ada's forged transport order. Its instruction is consistent at the post and beacon; Felix admits the hidden complete plan as a separate deliberate concealment. Lantern return still follows the glider rescue; repair responsibility and the lit north route remain.
- [x] Applied through the existing canonical importer/exporter. Actual Unity 6000.3.8f1 preserved all 483 story numeric table IDs; 5880 TSV values match and 5922 table values are non-empty. Addressables built in isolated QA. Four verified string-table assets copied back with unchanged asset GUIDs; fonts/atlases unchanged.
- [x] 32 unique checks passed: 27 EditMode checks, four text/layout checks and the full localized comic/pointer test. Coverage includes mobile comic turns, island dialogue, invitations and world-end boards in EN/RU/UK. Fixed the existing comic test to await asynchronous locale redraw before clicking; all assertions retained. QA-only image-export adaptations recorded.
- [ ] No physical-device build/install or independent human visual/literary-reader review. No screenshots created, prices/auth/cloud/configuration or saves changed. Runtime C# edits contain only string-literal changes; the additional test edit is the verified locale wait.
- Evidence: Docs/LANTERN_STORY_FULL_RU_OCT16.md (current Oct19 revision), Docs/LANTERN_STORY_BIBLE_OCT16_RU.md, Reports/StoryHumanWritingOct19/verification.json.


## Story context and relevance recheck — 19 October 2026

- [x] Re-read all 501 Russian authored positions in scene order with HumanWriting, checking replies, knowledge, props, consequences and context. Revised 44 positions / 132 EN/RU/UK values. Removed contrived callbacks, clarified Nora/Clara exchanges, kept originals with their owners, corrected stopped-clock use and aligned eight comic page captions.
- [x] Preserved mystery/cast, scenes, gameplay, receipts, saved progress and 483 numeric story table IDs. Four runtime C# files changed only in string literals. Rebuilt real localization tables and Addressables in isolated Unity; 5880 TSV values match, 5922 table values are non-empty; four verified assets copied back, GUIDs/fonts/atlases unchanged.
- [x] Final source passed 27 EditMode tests and two full PlayMode dialogue/comic layout tests, including EN/RU/UK phone sizes, large text and comic navigation. QA-only image-export suppression is recorded; original capture helper restored afterwards.
- [ ] No independent literary reader or physical-device review this pass. Technical tests do not certify prose quality. No screenshots, build/install, store submission, pricing/service or save changes.
- Evidence: Docs/STORY_CONTEXT_REVIEW_OCT19_RU.md, Docs/LANTERN_STORY_FULL_RU_OCT16.md, Reports/StoryContextOct19/verification.json.


## HumanWriting place names and cliché edit — 19 October 2026

- [x] Re-read all 501 authored Russian story positions and 46 collection descriptions with HumanWriting. Replaced nine decorative world labels with existing landmarks/functions/residents; revised 20 story positions and 46 item descriptions in EN/RU/UK. Removed canned morals/personification and gendered Rin descriptions while retaining plot, character choices and grounded humour.
- [x] Stored new name values in existing canonical supporting_copy; map/comic/island headings retain existing lookup keys and art bindings. Export now uses these values. Existing source/numeric IDs, gameplay, prices, receipts, progress and scene speakers retained. Five runtime C# files changed only inside string literals.
- [x] Real Unity table rebuild preserved 483 story IDs and nine world-name IDs. 5880 TSV values match, all 5922 locale values are non-empty; Addressables built. Four verified assets copied back, meta/GUIDs/fonts/atlases unchanged.
- [x] Story/name changes passed 27 EditMode and three PlayMode tests covering localized comic headings/pages, island dialogues and world-completion boards at phone sizes. Actual description font/box settings passed 552 checks for 46 items, three languages, shop/preview and normal/large text; zero truncation/missing glyphs. Rechecked those metrics after one final Ukrainian grammar correction; story/name/UI-test subjects unchanged.
- [ ] No independent literary reader or physical-device review. No screenshots, player build/install, store changes, price changes or save reset. QA-only capture-export suppression recorded and restored afterwards.
- Evidence: Docs/STORY_NAMES_AND_STYLE_OCT19_RU.md, Docs/LANTERN_STORY_FULL_RU_OCT16.md, Reports/StoryPlainOct19/verification.json.

## Finish live services except advertising — 20 October 2026

- [x] Reopened the exact existing Site source through the official workflow; prior Git/source-token publication block did not recur. Published request-scoped official SDK initialization, Apple public-verification separation and native-fetch OCSP transport, then isolated failed maintenance/deletion items and corrected per-environment Cloud Save headers. Final source `80a0b1069afd359981ee2f2433cecffc9a5ef2cf`, Site version 6, runtime revision 5, deployment succeeded. No secret in source/archive; DATA_KEY/service bearer preserved.
- [x] 34/34 backend tests and complete Vinext build passed. Failed publisher refreshes do not block account erasure/other events; failed erasures remain retryable while other users complete. New tests prove per-environment headers and replay-safe pending state. No Unity source change or repeated Unity compile in this backend pass.
- [x] Sent one actual Pub/Sub TEST through the dedicated topic; hosted callback returned 200 at 14:00:27.435Z. Unsigned request rejected 401. Existing integrations for other apps preserved. This is not a real store renewal/refund.
- [x] Installed official public Apple root certificates; saved and read back Sandbox notifications V2 URL. Production URL unchanged. Apple Sandbox actual TEST delivered after the native-fetch transport fix: Apple SUCCESS, hosted HTTP 200. All official chain/signature/OCSP checks retained. Read all nine existing Apple products (200, MISSING_METADATA); screenshots/submission remain deferred.
- [x] Checked health and external deletion/legal/support pages (200). Production PurchaseIdentity 200; ReadAccount 422/9009 missing secret; development identity cold call 504. Both temporary users deleted (200). Scheduler list 403 using owner's pair. These do not constitute completed cloud sync/purchases/deletion.
- [x] Prepared `/Volumes/MY DISK/FlyingLadders/Credentials/Server/FlyingLadders-ServerConnection.json` with mode 0600 and fresh connection token, outside repository. It is NOT applied to Sites or Unity. Refreshed exact remaining guide/evidence: Docs/CLOUD_REMAINING_MANUAL_OCT20_RU.md and Reports/CloudFinishOct20/verification.md.
- [ ] Publisher private-key import remains blocked: automatic review rejected a command that would emit Apple/Google credentials in stdout, before execution. No workaround/bypass or key transfer was attempted afterward. Need a protected import channel, matching Unity production secrets and Scheduler/Triggers access; actual unattended maintenance and real device/store/erasure tests remain pending. Activation flags stay false.
- [x] Respected current exclusions: no advertising configuration/behavior change, new store build upload, screenshot or submission; no catalog, price, ID or schema change. Preserved unrelated dirty project files/assets/meta.

## Failure result transition and decoration — 21 October 2026

- [x] Failed delivery now skips the old receiver ceremony and goes directly to the stable “Don't give up!” screen; the captured gameplay dissolve is retained for victory only.
- [x] Removed the four unattached daisy sprites from the failure layout while preserving its islands, weather and other frame decorations.
- [x] Added regression assertions for the hidden “Try again” ceremony, removed daisies and absence of a failure dissolve.
- [x] Follow-up found the existing Editor on MY DISK and completed native compilation/PlayMode/visual validation in an isolated project copy. The earlier CLI-inventory limitation is resolved; see `Reports/FlowerSupportsOct21/`. No player build or phone install.

## Flower support lifecycle audit — 21 October 2026

- [x] Reproduced the remaining four floating daisies in native Unity: failure hides the architectural frame while its separate weather mesh kept drawing rooted artwork. Keep rooted art tied to that exact frame visibility; preserve rain/snow/pollen/light effects.
- [x] Hide menu/heading plants when their actual support is absent, disabled or transparent. Previous Retry bouquets and Try again flash fixes remain in place; original art and scenes preserved.
- [x] Native Unity 6000.3.8f1: 8 unique PlayMode cases have PASS evidence, latest affected run 2/2 PASS. Failure matrix 54 cases, menu matrix 72 cases, 216 hidden/transparent support checks and 3 unbound pools; actual retry input, focus/accessibility, frame-contact pixels, wind/pause and dialogue regression checks passed.
- [x] Visually reviewed 70 final native captures plus the original reproduction. Source hashes match the tested copy; initial failing reports and test-precondition corrections retained. No full suite, player build, device install or user-save mutation.
- Evidence: `Reports/FlowerSupportsOct21/README.md`, `verification.json`, XML/logs and before/after captures. Disposable QA project removed after preserving evidence.

## Unity owner setup confirmed — 21 October 2026

- [x] Owner reports Scheduler/Triggers roles and both production connection secrets added. Real Scheduler list now succeeds (exit 0); ReadAccount no longer reports missing Secret Manager values, but Account backend unavailable. Temporary Unity user deleted (200), no real account modified. Prepared service-token read probe returns 401; hosted publisher keys still absent. Evidence: Reports/CloudConnectionOct21/verification.json.
- [ ] Agent still owns protected import of the existing Apple/Google publisher keys and matching hosted service token, then cloud/device/store/deletion tests and unattended maintenance. Do not request another key or repeat the owner's completed Unity steps. No retry/bypass of rejected plaintext credential output; no ad or store-upload change.


## Story causality and natural dialogue — 21 October 2026

- [x] Read all 501 authored story positions and 46 collection descriptions as editor and reader. Rewrote 192 story positions and 14 supporting strings in EN/RU/UK. Corrected disconnected replies, repair/job causes, chronology, prop ownership and stale scene headings.
- [x] Separated authorised lens work from the unauthorised Lantern transfer. Replaced fake inkblot suspense with checkable permission/equipment evidence. Explicit admission follows 516/540 levels; rescue still precedes source return. Optional jobs contain no required clue or premature admission.
- [x] Native Unity 6000.3.8f1: 36 EditMode and four unique PlayMode tests PASS across the documented runs. Full main-route turns and all comic turns checked at 320/390 EN/RU/UK; all invitation templates also at 430. Added real layout coverage and fixed text gutter/height calculation; assertions retained.
- [x] Rebuilt and applied actual locale tables with all 1,974 IDs and .meta files preserved. Final C# source matched the isolated tested copy. No player build/device install or human playtest.
- Evidence: Docs/STORY_CAUSALITY_REVIEW_OCT21_RU.md, Docs/LANTERN_STORY_FULL_RU_OCT16.md, Reports/StoryCausalityOct21/README.md and verification.json.

## Secure publisher connection and exact Unity diagnosis — 21 October 2026

- [x] Imported existing Apple/Google files and matching service bearer directly to the owner-verified backend through one-time signed HTTPS; AES-GCM sealed record under unchanged DATA_KEY. No private values printed/committed/archived. Site version 7/env revision 7 deployed. Protected read 200, health publisher/deletion/storage true. Temporary authorizer removed, import endpoint 404, temporary import key deleted.
- [x] Persisted published source and immutable migration in Backend/AccountCloud. 40 backend tests, 4 actual local Worker checks and 157 current Cloud Code boundary checks passed. Fresh exact linux-x64 .ccm deployed to production; runtime confirms safe per-field diagnostics.
- [x] Existing Scheduler access works. Real disposable production sign-in 200, ReadAccount 422/9009 explicitly `Backend URL secret is not an absolute URL.` Both Secret Manager fields are retrieved; no endpoint/token value emitted. All probe users deleted (200), real players unchanged.
- [ ] Owner: correct only CASCADE_RECEIPT_VERIFIER_URL to plain HTTPS /api/verify, retaining Cloud Code access. Agent: recheck stored Unity token via connected call, complete scheduled maintenance and full disposable account/purchase/deletion/device verification. Production flags remain false. Ad work, screenshots, store build uploads and review excluded.
- Evidence: Reports/SecureConnectionOct21/; current exact instructions: Docs/CLOUD_REMAINING_MANUAL_OCT20_RU.md. This supersedes earlier blocked key import/401 prepared-token/403 Scheduler notices.

## Owner-provided AdMob IDs — 21 October 2026

- [x] Applied exact six Android/iOS publisher IDs to Configs/admob.json, existing game settings and matching Google SDK App ID fields. Preserved GUIDs, unrelated fields and test/release/age switches.
- [x] Installed numeric rewarded slots 8065944779,1665469215 in existing Sites ADMOB_REWARDED_UNITS, redeployed unchanged source version 7 with env revision 8 (succeeded). Protected read/health 200. Two existing focused SSV/reward ownership and signature/tampering tests passed.
- [ ] Real native/device ad requests, AdMob SSV dashboard verification and consent/age integration remain unverified. Test mode stays enabled. Current connected Editor is another project, left untouched; no native import for this project claimed. No game build, phone installation, screenshot, store upload or review.
- Evidence: Reports/AdMobConnectionOct21/; current setup: Docs/ADMOB_CONNECTION_OCT21_RU.md. Previously deferred ads now include this explicit ID connection request only; no live traffic enabled.

## MY DISK workspace consolidation — 21 October 2026

- [x] Consolidated the project repository, Android artifacts, device backups, ACE-Step music and project credentials under `/Volumes/MY DISK/FlyingLadders` with stable typed subfolders.
- [x] Moved Google, Unity, server and Android credentials; copied the two shared Apple keys while retaining their DayDraft originals. All consolidated credential files use mode `0600` and credential directories are owner-only.
- [x] Updated known absolute paths in project configuration and documentation. Added a root workspace guide and a non-secret organization report.
- [x] Left Cascade-named outputs inside the active `drop_it` Unity checkout in place so that project references are not broken.
- Evidence: `/Volumes/MY DISK/FlyingLadders/README.md`, `Reports/DiskOrganizationOct21/README.md`.

## Store and cloud release audit — Oct 21

- [x] Revalidated public/server URLs, protected server token, Unity production Cloud Code, disposable-user deletion, maintenance Scheduler and Trigger. The former `Backend URL secret is not an absolute URL` failure is resolved.
- [x] Activated Google Play week/month/year subscription base plans in 173 regions; verified EN/RU/UK listings and Publisher access.
- [x] Verified Apple V2 notification URLs; Sandbox test notification delivered `SUCCESS`. Audited all nine products: prices/availability/localizations present, review screenshots absent.
- [x] Fresh signed Android catalog-bootstrap AAB validated and uploaded as version code 1, then moved to custom closed-testing track `internal-closed-testing` as a draft; Google readback confirms one accepted bundle and no built-in internal/production/beta/alpha release.
- [ ] Fresh iOS build validation and owner-authorized upload; Apple 1.0 still has no linked build.
- [x] Created and activated all six Google coin products with EN/RU/UK listings and Google-converted prices in 173 regions.
- [ ] Finish RTDN console test, policy/Families/Data Safety forms and tester assignment.
- [ ] Add nine Apple IAP review screenshots, make the owner Content Rights declaration, finish release metadata and perform real device sign-in/purchase/restore/refund/deletion tests. Rewarded SSV remains excluded until advertising is validated.
- Evidence: `Reports/ReleaseReadinessOct21/`, `Reports/GooglePlayBootstrapOct21/`, `Docs/CLOUD_REMAINING_MANUAL_OCT20_RU.md`. One internal draft AAB is uploaded; no screenshot, review submission or public release.


## Google Play catalog bootstrap and coin activation — 21 October 2026

- [x] Built a fresh signed Unity AAB at package `com.vickand.flyingladders`, version 1.0/code 1. Bundletool, JAR signature, signer match, API 36/min 25, non-debuggable state, ARM64, Billing permission, no AD_ID/AdServices/ads auto-init and six ELF 16 KiB checks passed.
- [x] Uploaded the exact verified hash to Google Play, created custom closed-testing track `internal-closed-testing`, and placed version code 1 there with status `draft`. API readback confirms the built-in internal/production/beta/alpha tracks are empty; testers remain unassigned and no invitation was sent.
- [x] Created and activated six consumable coin products with the existing Product IDs/prices, EN/RU/UK listings and 173-region availability. Re-read all six as ACTIVE. Re-read all three subscription base plans as ACTIVE in 173 regions.
- [x] Restored normal production authentication/cloud configuration immediately after the isolated bootstrap build; native account configuration validation passed. Removed build-generated Gradle templates and restored the plugin manifest.
- [ ] The uploaded binary intentionally remains an undistributed catalog bootstrap with local/test services. Real Play Billing purchase/acknowledgement/consume/restore, cloud sign-in and backend receipt checks still require a later live-services build and licensed tester device.
- [ ] Google Play console-only policy/Families/Data Safety/IARC forms, tester-group assignment and RTDN console test remain. No screenshots, review submission or public release were performed.
- Evidence: `Reports/GooglePlayBootstrapOct21/README.md`, `aab-validation.json`, `native-16k-validation.json`, `closed-final-readback.json`; retained AAB under `Builds/Android/CatalogBootstrap-20261021`.


## Google Play Dashboard form access — 21 October 2026

- [x] Rechecked current policy evidence, uploaded AAB, store catalog, live-service intent and mixed 9+ audience. Prepared exact safe Dashboard answers and current Families/Data Safety blockers in `Docs/GOOGLE_PLAY_DASHBOARD_FORMS_OCT21_RU.md`.
- [x] Confirmed most Finish setting up your game questionnaires are web-console-only; Android Publisher service-account access cannot save them. Two read-only attempts to inspect an authorised Chrome Play Console tab were stopped after macOS Automation access did not return; no browser or store form changed.
- [ ] Owner must enable macOS Automation/Chrome JavaScript from Apple Events and leave the authenticated Flying Ladders Play Console tab open. Then fill safe forms through the UI; do not submit an unverified mixed-audience Families/Data Safety attestation.


## Netlify marketing-site build — 21 October 2026

- [x] Declared Pillow in Website/requirements.txt and documented base/build/publish settings.
- [x] Replaced macOS-only sips social JPEG conversion with portable Pillow processing; preserved original artwork and unrelated source.
- [x] Clean Python 3.12 venv initially lacked PIL; pip install -r requirements.txt, full standalone build and existing six-page/153-link check PASS with sips blocked. Verified eleven WebPs and social JPEG. No Linux runner or remote Netlify rebuild.
- [x] Delivered ready static folder/ZIP in FlyingLadders/Builds/Website/NetlifyOct21; evidence in Reports/WebsiteNetlifyOct21. Existing Website/dist and Unity/offline legal assets untouched.


## Flying Ladders website and policy refresh — 21 October 2026

- [x] Replaced public Cascade branding, names, metadata and old social artwork with Flying Ladders; responsive lockup and cache-versioned styling/social URLs.
- [x] Set marketing canonical URLs to the owner-selected Netlify origin; kept account/purchase/deletion endpoints separate and unchanged. Added app-ads.txt using the existing publisher ID.
- [x] Updated privacy, terms, support and deletion instructions for the actual 16+ age choice, under-16 guest treatment, current cloud storage, subscription catalog and deletion cleanup. Synced offline legal texts; corrected account-service public copy locally without changing its APIs.
- [x] Full workspace and standalone builds/checks PASS: 6 pages, 153 internal references; changed account TSX lint PASS; desktop/mobile browser validation includes the 320 px overflow fix. Packaged static and source ZIPs with screenshots and backups.
- [x] Published verified static ZIP to existing Netlify project, deployment 6ad8db3cf4623e2ffd860bd5. Public HTTPS/hash/redirect/404 checks PASS. Verified new public legal URLs and updated future game legal/support links. Legal approval/indexing flags remain unchanged.
- [x] Follow-up service-link task below deployed account-service public redirects and saved live store metadata URLs. Installed game binaries retain built-in URLs until a later game build.
- Evidence: Reports/WebsiteFlyingLaddersOct21/; deliverables in FlyingLadders/Builds/Website/FlyingLaddersNetlifyOct21/.


## Flying Ladders Netlify domain — 21 October 2026

- [x] Renamed the existing project/default subdomain to flyingladders.netlify.app, exactly as requested.
- [x] Updated public website/legal/support links and metadata; preserved account/purchase endpoints and product IDs.
- [x] Built and checked six pages/153 internal references, published deployment 6ad8dd2b91d7ce4ab7fdb891, and verified public HTTPS response hashes and deletion redirect.
- Evidence: Reports/WebsiteDomainOct21/. Current archives: FlyingLadders/Builds/Website/FlyingLaddersDomainOct21/.


## Flying Ladders public service links — 21 October 2026

- [x] Saved and API-verified 10 Apple localization/app records across App Store Connect and TestFlight; Google developer website saved and API-verified. Google privacy policy saved in Console pending Publishing overview review; no review submitted and tracks unchanged.
- [x] Saved the new privacy URL for only Flying Ladders Android/iOS in AdMob. Other games and global settings untouched; no consent message published.
- [x] Saved Unity Player Accounts privacy/terms links and player-visible Flying Ladders client name. Reload confirmed persisted values; technical IDs, OAuth settings and project identity unchanged.
- [x] Published Sites account backend version 10: old public pages redirect to Netlify; original deletion form/APIs preserved. All 41 backend tests and production health/redirect/form checks passed.
- [x] Published Netlify deployment 6ad8e48781d3b830e2318c3c with crawlable app-ads.txt, accessibility anchor and Unity identity contacts. Six-page/153-reference source validation passed; 10 live response hashes match the final build.
- [ ] Separate release follow-up: Google policy review/Data safety/Advertising ID, AdMob crawl/consent/device validation, publisher legal approval and a future game build remain. No real purchase/account deletion, binary upload or app release in this task.
- Evidence: Reports/ServiceLinksOct21/; current owner guide: Docs/SERVICE_LINKS_CURRENT_RU.md. Source/static archives: FlyingLadders/Builds/Website/FlyingLaddersServiceLinksOct21/.


## Native game accounts — 22 October 2026

- [x] User selected Game Center for iOS and Google Play Games for Android; Unity Authentication remains the game identity. Guest mode, age restrictions, purchase verification and explicit save replacement are preserved. No automatic cross-platform identity merge.
- [x] Fixed sign-in rejected during a background save; one cancellable request waits for the in-flight write. Regression failed before correction. Fixed repeated account replacement after an incomplete candidate profile and retained recovery choices after cancellation.
- [x] Actual-client checks: 110 PASS on controlled SDK transport doubles. Native iOS-target Unity EditMode: 26 PASS; real OAuth is a separate device check.
- [x] Official Apple.Core 3.3.0 + Apple.GameKit 4.1.0 built/installed. Official Google Play Games 2.3.0 installed preserving the existing EDM 1.2.188; required UnityWebRequestTexture 1.0.0 resolved with Package Manager.
- [x] Apple API enabled GAME_CENTER for com.vickand.flyingladders, app Game Center detail and existing editable iOS version 1.0. Unity Authentication Game Center provider configured through the installed Editor SDK with an explicit project ID. No review or binary upload.
- [x] iOS Game Center build 202610224 exported, compiled and code signature verified; it is validation evidence, not the final device build.
- [x] Final native iOS build 202610229 compiled, signature/Game Center entitlement verified and installed. All four Documents files preserved byte-for-byte; app launch confirmed.
- [x] Owner confirmed the corrected 202610229 sign-in/status flow. Fresh device journal acknowledged revision 8 at 19:36:32 UTC; signedOut=false, no pending account replacement/deletion. Unity admin readback HTTP 200 confirms apple-game-center on the same original player. Fresh console contains no account errors.
- [ ] Clean-device/cloud restore and Android physical-device sign-in remain untested.
- [x] Android-target Play Games SDK compilation and 26 native Unity EditMode checks passed.
- [ ] Play Games app/server OAuth credentials, Unity provider and Android device sign-in remain unverified. Dedicated Play Games project, Android Play Signing OAuth and server Web OAuth are created, provider configured in Unity and public SDK settings generated. Owner account is a Play Games tester; publication and full Android device login are still unverified.
- [ ] Broader live-services checks remain open: real AdMob SSV console still rejects with 403; CMP is unpublished, live ads remain disabled, and no live purchase/refund/renewal test has been performed. Game backend migration to user-owned hosting is deferred while sign-in is fixed.
- Evidence: Reports/NativePlatformAccountsOct22/; design/investigation: Docs/AUTH_SIGNIN_OCT22.md. Raw Editor logs may contain transient Hub credentials and are not copied to reports.

- [x] Device 202610227: Game Center linked successfully (Unity admin readback lists apple-game-center), cloud journal acknowledged revision 6; user had then signed out using the former home shortcut. Startup anonymous sign-in race reproduced and fixed. Authentication signal now waits beyond the system-controller presentation callback. Home shortcut opens cloud status and offers explicit sign-out separately.
- [x] Provider controls raised/enlarged and account dialogs centred on the actual phone stage using existing skins. EN/RU/UK native UI check passed at 320/390 pixels; 110 controlled actual-client checks passed. These checks do not replace full device restore testing.
- [x] iOS 202610228 exported successfully with these device-feedback corrections; signing/install and final phone validation were completed below.

- [x] Same native identity repeat-login regression failed before correction; proof identity must match the existing verified UGS link exactly to skip redundant linking. Other accounts retain explicit save choice. Final build 202610229 export succeeded; code-signing/device install verification completed below.

- [x] Final 202610229 Unity export: Succeeded, 0 errors. Xcode build/signature verified. Android final SDK compilation + 26 targeted Unity EditMode tests passed; 110 actual-client boundary checks and EN/RU/UK phone layout test passed. Real Android sign-in and store purchase/renewal/refund checks remain open.


## Store screenshots and account UI — October 22
- [x] Sync status on the existing pale plaque; larger padded native account button, centered provider/caption with 4 logical pixel gap. EN/RU/UK phone and tablet PlayMode validation passed.
- [x] Captured 54 real native screenshots (iPhone 18 Pro 1206×2622, iPad Pro 13-inch M5 2064×2752, Android 35 1080×1920); six scenes per device and locale EN/RU/UK: four gameplay, trophies, map.
- [x] Separate offline capture identity and profile; no native-device progress reset or production service configuration change. Android optimized capture has no development watermark. Every final PNG validated for dimensions, opaque RGB format and unchanged native RGB pixels.
- [x] Organized delivery and ZIP under FlyingLadders/StoreScreenshots/2026-10-22. No store uploads or review submissions. Evidence: Reports/StoreScreensOct22/FINAL_STATUS.json and Docs/STORE_SCREENS_OCT22.md.


## Age question alignment — October 22
- [x] Reproduced original misalignment with a screen-level geometry regression. Moved the board to the stage centre and nested the question/actions under one layout owner; removed mixed top-left/bottom-relative placement.
- [x] Preserved yellow No and exactly 16 logical Canvas pixels between actions. Question no longer intersects Yes; matching button bounds, centred content and localized caption fit.
- [x] Both age-gate PlayMode tests passed, EN/RU/UK at 320×693, 390×844, 430×932 and 768×1024, including both answer routes to Home. Visual renders inspected. Evidence: Reports/AgeGateAlignmentOct22. Physical iPhone installation not performed in this task.


## Account primary action / status proportions — October 22
- [x] Enlarged the actual painted Game Center/Play Games action (592×148 target; 190-high skin), compacted status to 504×72 and raised it from bottom offset 310 to 380. Clear artwork gap; centred content and 4 logical pixel icon/caption gap preserved.
- [x] Two NativeAccount PlayMode tests passed, including 36 locale/viewport/text-size/status combinations. Phone/tablet visuals inspected. Evidence: Reports/AccountControlSizesOct22. Source/UI change only; physical iPhone install not performed.


## Settings age caption — October 24
- [x] Confirmed missing `Age group: {0}` key in authoring TSV and runtime localization tables; added EN/RU/UK entries without changing age policy or settings row layout.
- [x] Actual Settings → Comfort & help age row PlayMode regression passed all nine locale/age variants, with exact localized text and no overflow. Table audit: 5925 values, zero gaps. Evidence: Reports/AgeCaptionOct24. Physical device installation not performed.
- [x] Addressables player content rebuild succeeded (Android); Unity batch process exited normally.


## Store builds and metadata — October 27
- [x] Publisher confirmed flyingladders.vickand.com/support@vickand.com, approved the prepared Privacy/Terms, requested removal of draft, confirmed App Privacy publication and rights to all materials.
- [x] Local site generated without draft after documentsApproved=true; 6 pages/153 references passed. Existing noindex/account backend routes preserved. Public publication is still blocked by missing Vercel team grant (403); live pages still show draft.
- [x] Added native iOS ATT callback and AppTrackingTransparency linkage; adult ad initialization waits for UMP and ATT. Child restrictions and refusal access preserved. Production build uses real existing monetization configuration and active release guards, not preview scopes.
- [x] Unity EditMode monetization/audience checks: 31 PASS. Normal iOS export and Android AAB: 0 errors. Device purchase/consent/ad execution is not implied by these checks.
- [x] iOS 1.0 (1) archive/export/server validation/upload succeeded. Apple processing VALID and APP_STORE_ELIGIBLE. Attached to editable version 1.0, release MANUAL. Created local distribution certificate/profile without revoking existing certificates; private material retained only in external Credentials.
- [x] TestFlight IN_BETA_TESTING: build assigned to internal Owner testing group and the Account Holder. Readback confirms build and tester. Updated EN/RU/UK test instructions and removed obsolete preview simulation text.
- [x] App Privacy 12 types completed and published in Chrome after explicit publisher confirmation. Content Rights USES_THIRD_PARTY_CONTENT saved and separately read back after explicit rights confirmation.
- [x] Android 1.0/code 2, API 36, ARM64/IL2CPP, signed AAB uploaded; bundletool validation/signature and six native ELF 16-KB alignments passed. Existing internal-closed-testing track now contains code 2 draft. No production review/release.
- [x] Google feature graphic 1024x500 adapted from designer reference using ImageGen, original preserved; uploaded in 3 locales. Final asset readback: Apple 54 COMPLETE; Google each locale icon 1, phone 6, tablet 6, feature graphic 1. Public support/marketing contacts verified.
- [ ] First Google closed-test rollout requires Console: API validation rejects completed releases on a draft app. Chrome stopped executing input (noWindowsAvailable on coordinate/scroll calls), so final Data safety Save/review and Advertising ID/Families/account warnings are not claimed complete. Existing selected 10 data types and preview read; email/ID sharing matches Unity Player Accounts documentation, so no speculative change was made.
- [ ] Vercel team vickand grant, production legal deployment and live no-draft verification remain. User requested plugin publication and is currently away from computer.
- [ ] Owner deferred Apple IAP review screenshots/metadata until physical TestFlight testing. Test purchases/restoration/renewals, native account restoration/deletion, Android sign-in, ATT/consent/ads; verify account Business/Paid Apps Agreement/trader obligations in UI, then submit review. No agreements accepted or production release started.
- Evidence: Exports/Store-Release-2026-10-27/status.txt in the shared workspace; Reports/StoreReleaseOct27 in this project. IPA: FlyingLadders/Builds/iOS/Store-20261027-1/AppStoreExport/FlyingLadders.ipa; AAB: FlyingLadders/Builds/Android/Store-20261027-2/FlyingLadders-1.0-2.aab.


## Lossless iOS storage — October 27
- [x] Shared lossless LZ4 HC packaging enabled for future store and local-device iOS builds; release/debug mode retained. Android compression unchanged pending actual comparisons.
- [x] Added detailed future build-size JSON reporting by file role, source type and largest packed resources. Runtime art/audio/import settings were not changed; no new speculative asset removals.
- [x] Direct C# compilation passed for iOS/Android Editor assemblies, ten option-boundary checks passed; 943 resource/settings hashes unchanged. No player/Xcode/APK/AAB build, device install or store upload was run. Evidence: Reports/LosslessStorageOct27/validation.json.
- [ ] Measure actual size savings and device loading/rendering on the next explicitly authorized player build. Approximately 297 MiB potential savings is a compression experiment, not a verified new install size.


## 2026-10-27 — age, native account and purchase flow fixes

Implemented adult age → account choice → Home, balanced Yes artwork, native authentication before slow guest service work, and monetization startup after first-run account choice. Purchase binding is cached only for its authenticated player/profile/project/environment; receipt verification waits behind a background save. Verified durable grants update coin feedback before store acknowledgement; replay stays idempotent. Busy/loading/deferred captions and foreground-queued confirmation replace transient Coming soon and lost notifications. Existing catalogue, prices, server receipt checks, identifiers, packages and age policy remain unchanged.

Validation: 20/20 targeted PlayMode tests; 125 actual-client/config checks with controllable SDK transports. EN/RU/UK phone layouts included. Captures: Reports/AgeGateFlowOct27. Known separate baseline: CompletionFooterActionsFitInsideTimberAtPhoneSizesAndLocales fails Retry completed level hit-area clearance (expected >=28.75, actual 19.648819), reproduced with original runtime files. Device-native login/payment latency still needs TestFlight/Play testing. Store handoff completed: iOS 1.0 (2) is VALID / APP_STORE_ELIGIBLE / IN_BETA_TESTING in the existing owner group, attached to App Store 1.0 with MANUAL release; EN/RU/UK beta notes verified. Android AAB 1.0/code 3 accepted and freshly read back in internal-closed-testing draft (test not live). Both Unity builds had zero errors; IPA signing/export, AAB signature/API36/ARM64 and 16-KB ELF alignment verified. No public production submission. Final evidence: workspace Exports/Flow-Fixes-2026-10-27.


## Shop receipt text and service readiness — October 27
- [x] Receipt feedback now uses the existing pale skin and owns a matching font/material, with centred icon/caption and 4 logical pixel gap; avoids generic text/digit restyling. EN/RU/UK phone/tablet renders verified.
- [x] Native catalog prepares independently of UGS; purchase binding warms safely after account UI and is rechecked before payment. Ads retain age/consent gates and prepare on Home.
- [x] Queued cloud changes no longer inherit the 30-second idle timestamp; verified purchases skip redundant unsent uploads, and rewarded calls wait for active saves. Wallet refresh precedes reward feedback; validation/idempotency remain enforced.
- [x] Nine Editor PlayMode tests and 138 actual-client/fake-transport checks passed; iOS C# compilation passed. Evidence: Reports/StoreFeedbackOct27/validation.json. No player build, install, server deployment or live transaction test run.
- [ ] Confirm real payment, signed ad reward and cold-start readiness timings on the next explicitly authorized device build.


## Unity backend migration and ad/tutorial fixes — October27
- [x] Production CascadeAccount now owns profiles, balances, purchases/chain ownership, direct Apple/Google receipt validation, reward intents/SSV grants, subscription reconciliation and deletion in Unity Cloud Code + Private Cloud Save. No external SQL/verifier fallback remains.
- [x] Migrated5 account payloads/storage versions exactly,2 purchase owners and2 deletion fingerprints. Purged all9 legacy game/credential tables and verified0 rows; no DB backup created. Sites v15 removes the D1 binding and obsolete external secrets. Website pages/art unchanged.
- [x]205 isolated C# checks (163 existing +15 atomic +27 publisher/vault/SSV) and49 Node checks passed. Live24 first-write race had1 winner;18 owner requests had1 owner. Two actual anonymous first-sync/read/pending-reward/deletion cycles passed; player admin access rejected; QA identities/snapshots erased. Temporary migration/probe endpoints retired. Evidence: Reports/UnityBackendMigrationOct27.
- [x] Hourly Unity Scheduler + active Maintenance trigger verified by API; automatic18:00:16 UTC execution observed. Cleanup visits used shards, protects current snapshots, retries account erasure and covers both historical environments.
- [x] Condensed and localized unavailable-video feedback; ribbon wrapping/height verified EN/RU/UK at320/390 with large text. Real map glyphs now retain round circles and matching symbol/letter sizes. First-use lessons persist actual acknowledgement and discover features after restored floor/attempt changes. Final targeted Editor cases:6 passed across recorded runs; visual captures inspected. Evidence: Reports/AdsLessonsOct27.
- [x] Native AdMob preloading retries recoverable consent/init failures and loads rewarded independently after account gates permit it. Consent/age checks and production unit IDs retained. No player build/install/store upload performed in this work.
- [x] AdMob callback verified by Google and saved on Vickand for both iOS/Android; values re-read after reload. serverRewardsConfigured enabled in JSON and Unity settings for the next build. Real ad fill/payment/native login results remain unverified; Google app readiness limitations are separate.

## Vickand AdMob ingress — October27
- [x] Published https://vickand.com/api/flying-ladders/admob/ssv in a separate Cloudflare Worker; callbacks go directly to Unity Cloud Code and Private Cloud Save. No DNS/mail/website UI/database changes. Existing three routes and root response hash retained.
- [x] Fixed Workers incompatibility with fetch redirect:error; manual mode preserves upstream credential boundary.11 Node and1 actual Workers runtime checks passed; live health200, unsigned400, wrong signing key403.
- [x] Real Google AdMob signed verification passed for iOS rewarded1665469215; URL saved and re-read after reload.
- [x] Android rewarded8065944779 verified, saved and read back after reload. Ingress timeout aligned with Unity execution/cold start; client reward readiness enabled in JSON/asset. Source and sanitized evidence: Backend/AdmobIngress and Reports/AdmobCallbackVickandOct27. No mobile build/install/upload.
- [ ] Real rewarded fill and device grant still require testing in the next explicitly authorized player build.

## Server-ready test build upload — October28
- [x] Reproduced and fixed CloudSyncClient.Queue shortening successful-sync cooldown;97 targeted EditMode checks passed. Original failure retained.
- [x]22/25 expanded PlayMode checks passed; three tutorial/parcel/Undo checks fail identically with the original Queue. Evidence retained; no failing tests disabled.
- [x] iOS1.0(3) and Android1.0/code4 built with0 errors. IPA archive/export/codesign and AAB signature/API36/ARM64/16-KB alignment checked. DrillGame Editor left running.
- [x] Google accepted AAB4, committed as draft in internal-closed-testing; independent readback confirmed checksum and other tracks unchanged. Closed test not live.
- [x] Apple build3 VALID and IN_BETA_TESTING in existing Owner testing; EN/RU/UK notes and draft1.0 association read back. MANUAL release retained; no public release.
- [ ] Real device sign-in, billing/restore/refund and rewarded grant still require validation; the three baseline gameplay failures remain open. Evidence: Reports/ServerReadyOct28.

## Native services and session-only rounds — 2026-10-28
- [x] Reproduced iPhone 3 startup exception before cloud/IAP/ads initialization; fixed and covered saved-hint rendering.
- [x] Removed unfinished round restoration while preserving earned progress and transaction evidence; return Home after cold start or ten-minute absence.
- [x] Native-only mobile account buttons, Back above Welcome, responsive account door during catalog/sync work; EN/RU/UK verified.
- [x] Real iPhone 4 boot/sync and unchanged purchase ledger verified; 59 final EditMode + 6 PlayMode passed.
- [x] Built/signed final iOS 5 and Android 6; Android 6 draft uploaded and independently read back.
- [x] iOS 5 VALID / IN_BETA_TESTING in Owner testing; App Store 1.0 remains PREPARE_FOR_SUBMISSION / MANUAL, no App Review/public release.
- [ ] Complete fresh device purchase + rewarded SSV interaction; physical Android test; keep earlier tutorial/Undo failures visible.
See Docs/NATIVE_ACCOUNT_FLOW_OCT28_RU.md.


## First-use help and ad availability — 2026-10-28
- [x] Reproduced missing question-mark on ordinary boards; keep contextual mechanics plus basic manual available EN/RU/UK.
- [x] New far-swap mechanic no longer suppressed by optional control-coaching preference; fresh-profile first film verified.
- [x] Reproduced stale loading status after finished consent; separate active request from completion, retain CanRequestAds and backoff.
- [x] 78 EditMode and 14 unique PlayMode checks passed; actual phone-size tutorial frames inspected.
- [x] Correct iOS production IDs and server-reward setting; callback health and both app-ads.txt endpoints verified. No test IDs enabled.
- [x] iOS6 built/exported/uploaded before user stop; Apple VALID. Kept out of tester groups and left draft on5 per new explicit no-build/upload instruction.
- [x] With explicit consent extracted only tutorial flags and deleted full temporary save; first controls already acknowledged. Current AdMob readiness limitation and both callbacks read back; fresh signed Google test succeeded.
- [x] Source-only purchase identity/catalog parallel preparation, coalesced catalog requests and durable-in-session parent-approval UI;14/14 expanded Editor PlayMode checks. No further player build/upload.
- [ ] Actual ad show/grant, first native store latency and real Family Sharing/Family Link round-trip remain unconfirmed. Test device not registered; production serving requires store linking/readiness.
See Docs/ADS_TUTORIAL_OCT28_RU.md.

- 2026-10-28 AdMob: explicit Android request completed; Test apps Android1.0 On and iOS1.0 On verified after page reload. No build/upload; disable both before production1.0. Actual Android ad display still unverified.

- 2026-10-28 backend read-only audit: 61 Node cases passed (49 AccountCloud legacy/gateway regressions + 12 active AdMob ingress). Unity metadata confirms active Unity authority and recent hourly maintenance; first/second health samples 16.219s/0.659s are not a load test. Apple V2 and Google OIDC/retry configuration read live. Open BE-01..06: maintenance throughput/failure isolation/deadlines; index/GC capacity; repeated remote preparation and cold latency; temporary notification/local-verifier hostname; unverified WAF/alerts/load/recovery/E2E; single Apple environment and rotation procedure. C# 205 checks are previous Oct27 evidence, not rerun. No server source/config/deploy or player build/upload changed. Evidence: workspace Exports/Backend-Audit-2026-10-28/audit.json.


## Server hardening — Oct 28

- [x] Deployed CascadeAccount 20261028.2: indexed due work, adaptive CAS directory leaves, immutable publication nonces, independent retries, cancellation budgets, token/config caching and combined account/effects reads. Storage route Unity2; customer data retained.
- [x] Fixed reward response ordering so an acknowledged receipt loads its committed wallet before return. Checkpointed account erasure drains private work, preserves minimal replay tombstones and retries failures independently.
- [x] Sixteen hourly Unity Scheduler/trigger pairs verified and staggered across the hour. Owner declined workers.dev/Cloudflare timer; no external schedule exists.
- [x] Permanent Vickand notification/receipt ingress deployed, scoped rate protection, signature/OIDC checks, query-redacted logs, basic Worker issue alerts and Google dead-letter review queue configured. Apple sandbox TEST SUCCESS; signed Google test HTTP 200; unsigned inputs rejected.
- [x] Old hostname remains a thin compatibility proxy with a separate relay secret; no game database binding or visual website edits. Source URL and endpoint validation updated for the next explicitly authorized player build.
- [x] 296 C# checks + 76 Node/Workers checks passed. Live isolated six-request first-write race, idempotent retry, service-access rejection, unsigned reward rejection and complete disposable-account erasure passed.
- [ ] Larger production-load, provider-outage recovery, real purchase/renewal/refund and physical rewarded-ad end-to-end tests remain unverified. Apple production notification TEST requires repeating after first public release (currently API401). Client URL EditMode validation and the explicitly authorized iOS/Android build/upload follow-up are recorded below under Backend client test delivery.
- Evidence: Exports/Backend-Hardening-2026-10-28 in the task workspace; detailed operations in Docs/UNITY_CLOUD_MIGRATION_OCT27.md.


## Backend client test delivery — October 28, builds 7
- [x] With fresh user authorization, built and signed iOS1.0(7) and Android1.0/code7. Bundle IDs/version, iOS Game Center/distribution entitlements, Android API36/ARM64/16-KB ELFs and both signatures verified.
- [x] Apple VALID / IN_BETA_TESTING in existing Owner testing; linked to App Store1.0 draft, PREPARE_FOR_SUBMISSION / MANUAL retained. No App Review/public release.
- [x] Google code7 and checksum independently read back in internal-closed-testing draft; other tracks unchanged. Closed test is not live.
- [x]60 final EditMode and24 final PlayMode passed. Fixed active-only result footer lookup during same-frame redraw; corrected preview/locale test lifecycle. Original87/95 run and passing reruns retained.
- [x] Reviewed672 source/config hashes, including documented Unity PlayerSettings serialization; temporary signing keychain removed. DrillGame Editor left running. AdMob test modes unchanged.
- [ ] Real store-device sign-in, payment/approval/refund/renewal, rewarded grant and larger-load validation remain open; earlier three baseline gameplay failures are still documented.
Evidence: Reports/BackendClientDeliveryOct28/delivery.json; Docs/STORE_BUILD_OCT28_RU.md.


## Ad, tool tutorial and payment startup fixes — source-only October28
- [x] Reproduced and fixed cold Google callback executor deadlock; retained consent/child treatment.
- [x] EN/RU/UK inventory closes before first-use film; confirmation consumes once, cancellation consumes nothing, stale attempts rejected.
- [x] Reproduced late cloud hint grant leaving old video icon; cloud notification repaints controls, failed ads grant nothing.
- [x] Removed purchase identity RPC/save wait for authenticated sessions using unchanged server IDs, fallback and server receipt verification; SDK authentication coalesced.
- [x] Fixed prior account-switch race during reward preparation/read; early native sign-in during ordinary warmup remains available.
- [x]18 iOS +18 Android PlayMode,71 EditMode,139 actual-client/fake-SDK checks and3 purchase-latency checks passed. Actual tutorial captures reviewed.
- [ ] Fresh player build/upload needs explicit request. Existing builds7 keep old code. Real ad fill/SSV and native sheet latency need device checks; browser remains disabled in Work. Earlier three gameplay baseline failures remain separately recorded.
Evidence: Reports/AdToolsPurchaseFollowupOct28/verification.json and Docs/ADS_TUTORIAL_OCT28_RU.md.


## Ad/tools/purchase test delivery — October 28, builds 8
- [x] Fresh explicit user authorization; iOS1.0(8) and Android1.0/code8 built, signed, validated and uploaded.
- [x] Apple VALID / IN_BETA_TESTING, existing Owner testing group; App Store1.0 remains PREPARE_FOR_SUBMISSION / MANUAL. No public release or App Review submission.
- [x] Google checksum read back in internal-closed-testing draft; unrelated tracks unchanged.
- [x] Verified fixes included on both platforms: ad callback pump, bag/tutorial handoff, late hint repaint and removal of pre-payment network wait; existing server verification retained.
- [x] Source tests18Android+18iOS PlayMode,71EditMode,139account client and3payment preparation checks passed before build; exact source hashes rechecked. Distribution entitlements, bundle metadata, signatures and Android16-KB libraries verified.
- [ ] New user preference: disable AdMob Test apps on iOS and Android version1.0. Source already uses live IDs/flags; console switch blocked by Work browser permission. No claim that it was turned off.
- [ ] Physical device store-payment/rewarded/sign-in checks remain; earlier three baseline gameplay failures remain documented.
Evidence: Reports/AdToolsDeliveryOct28/delivery.json.


## Ad availability and non-blocking solver help — October29
- [x] Verified actual AdMob states: Flying Ladders both limited/requires store review, ShapeMania both ready. Test apps bothOFF, no mode changes. Public store URLs404; iOS ID search unavailable.
- [x] Reproduced next-hint blockage before fix (0/1). Added rate-limited free solver help when rewarded video unavailable, bounded loading wait, no placeholder or fake reward. Rule note Docs/ADS_AVAILABILITY_OCT29_RU.md.
- [x] Existing currency, signed rewards, store IDs, consent/age gates and schemas retained. Stale/canceled search, save rollback, legacy timing, hidden offers, full grant ledger and late real SSV covered.
- [x] Final 63 final-edit-ios-2, 20 final-play-ios-2, 20 final-play-android passed; EN/RU/UK mobile captures reviewed.
- [ ] Physical iPhone SDK failure code/live ad fill remains unverified; user cannot connect USB now. Public app/store linkage/readiness approval remains external.
- [ ] New player build/upload requires fresh authorization. No build/install/backend/store deployment done in this task.
Evidence: Reports/AdAvailabilityOct29/verification.json.


## AdManagerPro migration — October29
- [x] User-requested import of AdManagerPro from ShapeMania; existing Google SDK11.5 retained without duplicate native framework or SDK upgrade/downgrade.
- [x] All6 Flying Ladders App/Interstitial/Rewarded IDs configured for iOS/Android, testModeOFF and Google app settings match.
- [x] SDK initialize/load/retry/show delegated to AdManagerPro. Existing game bridge retains consent/age, pause, signed reward intents/SSV and wallet authority. Added plugin callbacks and cancellation/coalescing/reset support.
- [x] 35 final-edit-ios, 26 final-play-ios, 26 final-play-android passed, including actual SDK Editor transport and adjacent reward flows. No imported GUID collisions. Design/adaptation note Docs/ADMANAGERPRO_MIGRATION_OCT29_RU.md.
- [ ] Physical native ad fill and live SSV after migration remain unverified; AdMob review/store linkage restriction unchanged.
- [x] Source migration verified; subsequent user-authorized iPhone-only installation recorded below. Store build8 unchanged.
Evidence: Reports/AdManagerProMigrationOct29/verification.json.


## Canonical deletion and legal-site migration — October29
- [x] Published own-domain EN/RU/UK deletion form and same-origin bounded API via six exact-prefix Cloudflare routes; root/support/existing assets remain Vercel. Existing main-domain mail and callback routes preserved.
- [x] Live disposable Unity account deletion accepted202; profile reads blocked422; Authentication identity erased404; repeated confirmation idempotent and account stays blocked.
- [x] Sites v18 old deletion URL redirects308 to canonical product host; legacy API compatibility preserved. Current source settings and future embedded documents use the canonical URL.
- [x] Approved legal documents published without draft and updated to actual Unity data storage;7-page/173-link checker and89 focused local checks passed.
- [x] Apple/Google API release metadata read back;54 Apple screenshots COMPLETE, support URLs/contact correct; Apple8 VALID/MANUAL draft; Google alpha8 completed and separate closed draft8.
- [ ] Mac unlock required for Google Console-only deletion URL/declarations and visual browser validation. Browser IAB disabled; native Chrome locked.
- [x] Owner explicitly deferred a new player build on October29 ("Пока без новой сборки"); no build/upload performed. Future embedded-document/direct-URL delivery waits for a separate new request. Owner-deferred Apple IAP metadata/screenshots and device tests remain.
- Evidence: Docs/RELEASE_MIGRATION_OCT29_RU.md and task workspace Exports/Deletion-Migration-2026-10-29.


## Flying Ladders trailer — 29 October 2026

Completed the requested portrait 1080×1920 and landscape 1920×1080 trailers, 40s/30fps, using Remotion and the Instavar planning tools. Owner's latest direction incorporated: 24 visual shots, no shot longer than2.4s, nine one-second world shots, consecutive swap/drop/target framing, then the genuine C176 native three-star celebration, collection, Mika and logo. English captions, existing game artwork/music/SFX and a locally authored rhythm bed. All game footage re-captured in Editor at1080×1920 with a temporary offline profile; native C176 result verified:1200points/3stars/20coins. Native reward animation accelerated for editorial pacing. Both temporary capture helpers and their generated metas removed from Assets; copies retained in trailer source. No mobile build/upload, store changes, gameplay rule changes or real-player data access.

Delivery: /Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/Exports/FlyingLadders-Trailer-2026-10-29/FlyingLadders-Trailer-Vertical-1080x1920.mp4 and FlyingLadders-Trailer-Wide-1920x1080.mp4. Editable portable archive FlyingLadders-Trailer-Source.zip; project is /Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/flying-ladders-trailer. Studio at http://localhost:3123 remains available.

Verification: TypeScript passed; both final MP4s contain1200H.264/yuv420p frames plus48kHz AAC, exactly40seconds. All video/audio frames decoded without errors. Final loudness measured -16.0LUFS, true peak -1.1dBFS, no clipping. Selected final rendered frames and both one-frame-per-second contact sheets inspected, including layouts/crops/native stars. Maximum shot timing checked from the authored timeline. No claim of continuous human playback or listening acceptance. Standard bundled headless-shell stalled; installed Chrome in a separate temporary headless profile rendered successfully. No user browser profile touched. Prior no-new-mobile-build constraint remains active.


## AdManagerPro local iPhone installation — October29
- [x] Fresh user request: build and install only on the connected iPhone. Local iOS1.0(202610291) built and signed with existing development credentials.
- [x] Signature, exact bundle/version, Game Center entitlement and AdMob iOS App ID verified. CoreDevice confirmed installation success; no uninstall or save-content read.
- [ ] Launch verification awaiting unlock: OS returned Locked, user notified. Live ads/SSV not exercised.
- [x] No App Store Connect/Google Play upload or backend deployment; remote stores remain build8.
Evidence: Reports/AdManagerProDeviceOct29/device-verification.json.


## Rewarded icon regression and UMP investigation — October29
- [x] Reproduced missing-film icon before fix (0/1); removed cached-ad-readiness from offer icon choice. Real tokens/revealed hints still show bulb.
- [x] Existing free-help banner localized EN/RU/UK only after committed Step; Unknown cannot falsely claim a delivered hint. No fake ad screen or currency/token grants.
- [x] Identity-free consent/ATT/SDK/account gate and signed-intent diagnostics; explicit sign-out/ownership semantics retained.
- [x] Final16 iOS+16 Android Editor PlayMode;154 AccountFlowChecks fake-transport assertions. Mobile EN/RU/UK captures reviewed, independent patch review completed.
- [x] AdMob live status read again; saved GDPR/UMP draft for Flying Ladders iOS+Android, EN/RU/UK, EEA/UK/Switzerland, refusal ON.
- [x] User explicitly approved UMP publication; native Chrome confirmed Published and Unpublish switch ON for both Flying Ladders apps. EN/RU/UK, EEA/UK/Switzerland, refusal ON. Propagation may take up to one hour per AdMob. Evidence Reports/RewardedIconOct29/admob-ump-published.png.
- [ ] Current iPhone SDK failure/fill/SSV unverified: paired Wi-Fi tunnel disconnected, no USB journal.
- [x] No new player build/install/upload or Test apps toggle.
Evidence: Reports/RewardedIconOct29/verification.json; Docs/ADMANAGERPRO_MIGRATION_OCT29_RU.md.


## AdMob configuration readback after UMP publication — October29
- [x] Read-only live console check: both AppIDs/four correct format-unitIDs match current game settings; both rewarded callback URLs are canonical vickand.com/api/flying-ladders/admob/ssv; reward amount1, compatible Open Lootbox label.
- [x] All four units use Google optimized / All prices; app/unit frequency caps No cap. Test apps iOS1.0 and Android1.0 OFF. Published UMP message remains ON for2apps/English+2.
- [x] Policy center: No current issues; No disapproved apps. Public root and product app-ads.txt bodies contain the exact publisher line.
- [ ] Flying Ladders app-ads.txt verification not shown in console; both apps remain Requires review / Limited ad serving / Add store to lift limit. Public store linkage/readiness approval remains necessary.
- [ ] Native device SDK response, real fill and end-to-end committed SSV grant remain unverified after consent publication. No blanket claim that production ads work.
- [x] No AdMob settings saved/changed, no build/install/upload or real ad request.
Evidence: Reports/AdmobConfigurationAuditOct29/verification.json and screenshots.


## Flying Ladders director’s cut — 29 October 2026

Completed the explicitly requested second directing/editing pass. Both40s/30fps formats are under /Users/andrew/Desktop/PROJECT_CASCADE_Codex_Pack_v2/Exports/FlyingLadders-Directors-Cut-2026-10-29, with a new portable source archive. V1 files remain unchanged.26visual shots, max2.5s; source-space camera tracking and larger real gameplay; alternating landscape framing; puzzle-specific native target references; short Nunito ExtraBold titles; cloud chapter wipes; reward light accents; native three-star C176 result retained. Continuous source ranges preserve the basic solution's causal order. Nine worlds each have a one-second shot.

Sound rebuilt from owned game theme/SFX, with pitch-preserving tempo conversion from estimated80.53 to120BPM, original3/4 pulse retained, transient trimming and a documented frame cue sheet. Standard BT.601/full-range raw master converted correctly to BT.709/limited delivery; sampled RGB preservation checked. Final AAC stereo48kHz:−16LUFS,−2.44dBTP. H.264/yuv420p1200frames,40s. Full decode and TypeScript passed; every visual shot previewed in both orientations. Fixed star clipping, premature camera tracking and goal references after review. Both final MP4s opened at0, unmuted volume1, played continuously to the final brand card in native QuickTime. No measured dropped-frame count or subjective human listening/user acceptance claimed.

No paid service, external upload/publication, store change, player build, gameplay rule change or real-player data change. Existing no-new-mobile-build constraint remains active. Remotion Studio for the editable directors compositions remains at http://localhost:3124. Instavar's public creative/review workflow informed the pass; its broken account link was not required or bypassed.


## Rewarded correction local iPhone delivery — October29
- [x] Fresh explicit iPhone build request; user specifically requested Wi-Fi installation. Built iOS1.0(202610292) locally using existing signing credentials.
- [x] Unity build succeeded with0errors; Xcode build/signature, exact bundle/build, ARM64, Game Center and GADAppID verified. CoreDevice confirmed installation success over paired Wi-Fi. No uninstall/save read.
- [x] Device launch confirmed by CoreDevice.
- [x] All883 snapshotted source/config files unchanged except hash-proven generated iPhone SENTIS_ANALYTICS_ENABLED serialization; verified7-file patch unchanged. No guessed rollback.
- [x] No Android build, store upload, AdMob toggle or backend deployment. Native fill/live SSV still unverified.
Evidence: Reports/RewardedFixDeviceOct29/device-verification.json.


## Latest rewarded integration store delivery — October29, builds9
- [x] Fresh explicit request: built/signed/verified/uploaded iOS1.0(9) and Android1.0/code9 from the same verified code as local iPhone202610292.
- [x] Apple VALID/IN_BETA_TESTING, existing Owner testing; manual App Store1.0 draft bound to9. No App Review/public release.
- [x] Google AAB/SHA256 read back independently; internal-closed-testing draft9; all other tracks unchanged. This does not start the closed test.
- [x] Both Unity builds0errors; iOS distribution/GameCenter/GADAppID and Android API36/ARM64/signature/bundletool/16KB checked. Seven patch hashes unchanged;883 source/config review only generated Android SENTIS define delta.
- [x]16iOS+16Android prior PlayMode and154 fake-transport account assertions recorded; no unrun device checks claimed. Existing keys reused and temporary signing keychain cleaned. No AdMob settings change or neighboring Unity interruption.
- [ ] Live store build ad fill/SSV/purchases still unverified; AdMob public-store linkage/readiness remains open.
Evidence: Reports/RewardedStoreDeliveryOct29/delivery.json; Docs/STORE_BUILD_OCT29_RU.md.


## 2026-10-29 — Puzzle Break trailer revision (user feedback)
Completed both40s/30fps formats in Exports/FlyingLadders-Action-Cut-2026-10-29. New Remotion compositions ActionCut-Vertical/Wide; V1/V2 preserved.21visual shots, maximum3s. Full-frame portrait gameplay with centred top advertising copy and no separate bottom sky/mask. Native published three-floor levels filmed across all nine worlds (C197,C261,C304,C337,C362,C423,C445,C485,C526), plus native EN Words/Mixed; solver-command capture proof records actual wins. Four distinct native trophy previews (sky pilot,frog suit,star knight,bee costume), short native Snow Spire conversation and genuine three-star C176 result. Alternating camera push/pull and tracking, cloud/light transitions, varied illustrated landscape benefit cards; repeated Target Row inset removed. HumanWriting pass: calm puzzle break, thinking and family play; advertising slogans without invented critic attribution or review scores. Original game music and SFX retimed; dialogue music ducked. Only native woven reaction TEXT renderers hidden in clean capture to avoid overlapping promotional copy; gameplay logic, effects and event timing preserved. Separate offline temporary Editor profile; all three owned temporary capture helpers removed after use. No mobile player build or store changes were made for this video task.
Validation: TypeScript passed;21shot previews in both formats;42delivered-frame color comparisons; full decode of both1200-frame outputs; H.264/yuv420p limited BT.709, stereoAAC48k,-16.07LUFS,-2.20dBTP. Both quality outputs and compact wide copy played from00:00 to final branding in QuickTime with volume1/unmuted and no seeking/player error; no claim of instrumented dropped-frame or subjective human listening acceptance. Phone wide CRF18 from same raw Remotion master, audio bit-copied; SSIM0.994574 against CRF17 quality output. ZIP CRC and exact MP4 hashes passed. Phone ZIP101631278bytes uploaded to Google Drive; metadata readback size matched. https://drive.google.com/file/d/1iPZ9oF2KJfgxow14NdVGt-NfteUdF-jv/view?usp=drivesdk . Portable source archive retained locally.


## 2026-10-29 — Google Ads acquisition asset pack
Prepared Exports/FlyingLadders-GoogleAds-2026-10-29/FlyingLadders-GoogleAds-Ready.zip (24589018bytes) and uploaded it to the user's Google Drive. Metadata readback size matched. https://drive.google.com/file/d/1xn6MCKWJ0IuvaS1Frvg47g6vL4NQ3T8D/view?usp=drivesdk .20ready PNG image assets:6landscape1200x628,8square1200x1200,6portrait1200x1500. Per the user's clarification, every final image is a SINGLE genuine game screenshot crop, proportionally resized; no AI redrawing, compositing, new gameplay, replacement graphics, added copy, buttons, rating/review claims or synthetic background. Initial generated key-art concept was rejected and excluded from the upload archive. New native2160px-wide portrait/square screenshots were captured in a separate Unity Editor using an isolated temporary offline profile and actual puzzle commands (C197,C362,C445,C526,EN C19) plus actual frog/sky-pilot/star-knight collection previews. Owned temporary Editor helper removed. No player builds/store/campaign/budget changes made for this asset task.
HumanWriting:60validated text assets in6sets (EN main/calm/think-together/discover, RU main, UK main), each5headlines<=30characters and5descriptions<=90characters. Primary imagery/trailers use English game UI; RU/UK copy is marked as additional localisation material. START-HERE.txt explains one5+5set per ad group,20image maximum, source detail crops, YouTube hosting for the existing trailers and links to checked Google App campaigns specifications. Copy-paste CSV/JSON and plain-text-only files included; not claimed as an account-specific Google Ads Editor import.
Validation: all20image dimensions, RGB/sRGB, file size<5MB, source SHA256 and exact pixel equality against native crop/resize passed; all60text limits passed. All images visually checked in aspect contact sheets; corrected cropped control slivers/transient token overlap before delivery. ZIP CRC and exact file hashes passed. Native masters, capture source, crop coordinates and validation scripts retained locally; rejected generative artwork excluded from the ready pack.


## Android safe-area/system Back audit — October29
- [x] Read-only source/AAB9 audit: Screen.safeArea dynamic layout and safeRoot hierarchy present; no physical Android navigation-modes validation claimed.
- [x] Source remediation below adds app-level OS Back routing; the originally audited build9 remains unchanged.
- [x] Source now uses Unity's documented Activity workaround with a build guard. Audited build9 still uses Unity6000.3.8f1/GameActivity3.0.5; no local ANR reproduction or replacement build claimed.
- [x] No code/settings changes, new build or upload performed by this audit.
Evidence: Reports/AndroidNavigationAuditOct29/audit.json.

## Android Back/ANR remediation and yellow age label — October29
- [x] Yellow age button text black; EN/RU/UK390×844 screenshots visually inspected and text-fit assertions passed.
- [x] Committed native Back + legacy Escape route top overlays, bag/help, map parents and settings; pause/resume and root background; no purchase/deletion/tool approval. Pending parental approval does not lock navigation. World completion exits through authored Home action.
- [x] Select Activity and reject affected GameActivity before Android builds; preserve SDK manifest overrides and JNI names during future minification.
- [x] Android lifecycle autosaves enqueue existing atomic background writer; avoid hidden UI reconstruction. Critical durable economy unchanged.
- [x]59 unique target-specific Editor checks passed (Android28/iOS27/policy4),16 native fake-dispatcher assertions, API36 Java and Android-only C# branch compilation. Reports/AndroidNavigationOct29/verification.json.
- [ ] Physical Android gesture/3-button/IME/SDK/lifecycle/slow-device stress pending a separately authorized player build. No build/upload performed; build9 is unchanged. No zero-ANR guarantee.
Design/limits: Docs/ANDROID_NAVIGATION_OCT29.md.

## Publisher address — October29
- [x] Replace publisher address with Bulgaria, Varna, bul. Vladislav Varnenchik 36a, 9000 and publisher country with Bulgaria in Flying Ladders site configuration, public legal HTML/TXT, embedded documents/LegalSettings and retained legacy legal files.
- [x] Publish only four changed legal assets through the existing Cloudflare Worker, preserving routes, root Vercel artwork, deletion service binding and handler logic. Exact live source readback/hash and public HTTP200/new-address checks passed.
- [x] Website7 pages/173 references +5 Worker checks passed. No player build/upload.
- [x] Related Uskeep app/website publisher data and local legal HTML updated;6 localized legal checks and website checker passed.
- [ ] Uskeep live site/main Vickand publication still needs Vercel team grant (403). User cannot approve currently; no live change claimed for these sites. GitHub main site connection has read-only access.
Evidence: Reports/PublisherAddressOct29/verification.json.

## Legal document close/footer controls — October29
- [x] Privacy/Terms: replace bottom Close action with top-right48px square game button and centered graphical cross;12px safe-edge inset.
- [x] Bottom Open on website action now56px high, with16px bottom and reading-area gaps. Keep existing gold skin, translations and external URL behavior.
- [x] Reserve header space for the close control and fit RU/UK titles on small phones; retain scrolling and full document text.
- [x] Existing Editor visual review passed18 document/locale/size variants (320×568 large text,390×844,768×1024), plus existing Android legal-Back check. Screenshots visually inspected; temporary review edits restored exactly. ProjectSettings preserved.
- [x] No player build/install/upload. Evidence: Reports/LegalControlsOct29/verification.json.

## Subscription legal links without plates — October29
- [x] Remove AmberAction skins from Terms/Privacy on Remove ads; use bold warm-white labels with a dark drop shadow and transparent touch targets. Existing documents/actions and other purchase controls preserved.
- [x] Correct footer fit for expanded touch regions. Keep lower rows together; recover compact-phone space from a proportional reduction of the existing emblem, not smaller copy or overlapping hit targets.
- [x] Existing visual review passed18 variants: EN/RU/UK ×320×568 large text,390×844,768×1024 ×available/owned-year states. No visible backing, fitted labels, safe bounds, nonoverlap and document-click behavior verified; real renders visually inspected.
- [x] Temporary review restored exactly; project settings unchanged. No new artwork or player build/upload.
Evidence: Reports/SubscriptionTextLinksOct29/verification.json.


## Remove ads full reference composition — October29
- [x] Rebuild available/owned states from the supplied second/third references: separate emblem, scroll, calendar/cards, floral selected state, red computed savings ribbon, and framed action artwork.
- [x] Put management above restoration after purchase; grey alternate plans, graphic owned check, readable localized period labels and dates. Keep store metadata/entitlements and native actions.
- [x] Terms/Privacy remain bold unframed shadowed links. Remove renewal plate/wording; retain a plain recurring-billing disclosure.
- [x] Compare actual Unity renders beside both reference viewports at 1:1 scale. Regenerated artwork is a close recreation, not pixel-identical; review boards document the difference.
- [x] 24 rendered state/locale/size variants passed; final iOS Editor regression passed27 selected-period/locale/phone cases including large text and digit visibility. 4px icon gaps, centring, safe bounds and nonoverlapping touch regions checked.
- [x] Temporary preview prices/profile fixture restored; ProjectSettings exact known hash restored; no mobile build/install/upload.
Evidence: Reports/NoAdsReferenceOct29/verification.json and comparison PNGs.


## Catalogue illustrations visually centred — October29
- [x] Confirm preserveAspect used top-left RectTransform pivots, pinning wide frames to the top and tall art to the left despite centred image slots. Source PNG padding also displaced some costumes.
- [x] Add shared CataloguePicture: centre the aspect-fit pivot without moving/resizing its authored slot, then compensate measured source-alpha centre. Apply to shop, collection and inventory cards; preserve original sprites, worn-character pivots, prices and actions.
- [x] Measure46 trophy sprites and6 tool sprites. Small cached static focus table; no runtime texture readback.
- [x] Capture all7 visible shop pages in EN/RU/UK at588×1280 and320×693 (42 renders). Independently verify actual visible bounds, not just image rectangles. Visually inspect page2/page6/page7 and all-pages sheet.
- [x] Final iOS Editor regression passes all shop pages/locales/sizes plus tools, collection and inventory thumbnails. Earlier failed test profile setup was corrected; no save validation was weakened.
- [x] Restore temporary visual fixture and exact ProjectSettings. No artwork changes, player build/install/upload.
Evidence: Reports/CardIllustrationCentresOct29/verification.json, comparison.png and all-pages.png.


## Island list bottom clearance — October29
- [x] Increase persistent bottom content padding from12 to96 reference units, retaining safe-area extension. Use one shared constant for initial content height and responsive layout.
- [x] Existing real pointer-drag/scrollbar regression passed, plus9 EN/RU/UK and compact/phone/tablet settled-bottom captures. Final card stays fully visible with breathing room; locked actions/navigation/progress unchanged.
- [x] Visually inspect390 RU and320 EN renders; restore exact ProjectSettings. No player build/install/upload.
Evidence: Reports/IslandScrollBottomOct29/verification.json.


## HUD badge type and subscription action clearance — October29
- [x] Correct tiny undo-number/bag-plus type: replace default12px centred-row padding with shared2px badge fitting, bold24×ControlUnit type and visible-glyph centring; reapply after preferences/locale/layout refreshes.
- [x] Use existing rounded green/gold skins and independent rooted decoration for larger inner button surfaces. Fit two-line purchase copy and icon/caption actions with minimum2px insets.
- [x] Use identical full gold Restore purchases button/bounds in both entitlement states. Keep Terms/Privacy styling and positions; place selected-period billing sentence below the links.
- [x]24 paywall renders passed across3 locales/4 sizes/2 states; final iOS Editor checks passed18 readable-badge configurations and27 long-price padding cases. Screenshots visually inspected.
- [x] Restore temporary price fixture and exact ProjectSettings. No player build/install/upload.
Evidence: Reports/PaywallAndHudBadgesOct29/verification.json.

## Shared button design system — October29
- [x] Add five shared presets and reusable prefabs: primary green, secondary yellow, tertiary label, primary icon and secondary icon. Reuse existing art and InterfaceTitle; centre icon + label with4 logical pixels and at least2 pixels inside the real writing field.
- [x] Unify full-action dimensions, icon slots and type; use the failure screen's yellow Map + scroll across results. Replace Find in shop with Shop + storefront. Keep compound card controls and their artwork intact.
- [x] Integrate reference factories, HUD, home/account, pause/results, tool tray, age choice and legal actions. Keep readable stock/plus circles independent of square-button size; prevent conflicting anchors/aspect fitters.
- [x] Final12 unique targeted iOS Editor tests pass across EN/RU/UK and compact/mobile sizes, including126 action-view configurations, long prices, badges, card art, real list gestures, Map and retry state. Initial footer clearance failure resolved and retested without weakening bounds.
- [x] Inspect actual Unity renders, save five-template sheet and side-by-side game screens; restore exact ProjectSettings. No player build/install/upload or service/store configuration changes.
Evidence: Reports/ButtonDesignSystemOct29/verification.json, game-screens.png, button-library.png. Design contract: Docs/ButtonDesignSystem.md.


## 2026-10-29 — Calm Cut trailer revision delivered
- Rebuilt both trailer aspects in Remotion as CalmCut-Vertical (1080×1920) and CalmCut-Wide (1920×1080), 58.5s / 1755 frames / 30fps. Preserved previous versions. User's request for calmer, clearer pacing supersedes the prior maximum-three-second-shot preference.
- Nine coherent chapters, twelve visual shots: complete main puzzle over 9s with viewing pauses; lagoon, words, snow and observatory; genuine three-star reward with completed-pose hold; three native outfits for 3s each; two native dialogue lines for 4.5s each; 4.5s brand ending. Only actual source frames and established game art used. Exact frame mappings retained in calm-edit-map.json.
- Removed flashes, cloud wipes, punch zooms, synthetic percussion and transition whooshes. Original Great Beacon music near original 80BPM, 18-frame dissolves, gentle camera, sparse quiet native SFX and dialogue duck. Persistent titles through outfit/story sequences.
- Final H.264/AAC files: Exports/FlyingLadders-Calm-Cut-2026-10-29/FlyingLadders-Calm-Vertical-1080x1920.mp4 and FlyingLadders-Calm-Wide-1920x1080.mp4. Typecheck, full decode both, exact frame/dimension checks, BT.709 conversion, -17.00 LUFS / -3.58 dBTP measured, 38 master-vs-delivery color samples (max mean RGB error 3.522/255), all-shot composition previews, first/last-frame visibility, ZIP CRC and MP4 SHA256 passed.
- Vertical final played uninterrupted in QuickTime unmuted at volume 1 through final brand frame, without observed player errors. Wide native uninterrupted playback remains uncompleted because Mac locked again before opening it; wide full decode and frame QA passed. No subjective human listening approval or dropped-frame instrumentation claimed.
- Phone delivery ZIP: 91,536,089 bytes, exact Google Drive metadata size verified. https://drive.google.com/file/d/1_GPL2MBPSaoN6q3OdXY5JOX0iy6jMwmy/view?usp=drivesdk . Existing sharing permissions unchanged.
- Verification, provenance, edit decisions, audio cues and delivery metadata retained beside outputs. No Unity game source changes, player build, store upload, ad campaign or spend in this task.

## Fresh stage vocabulary and shapes — October29
- [x] Add frozen 46,572-word offline EN/RU/UK pool for 3–8 letters, provenance/notices, family filtering and individually shuffled familiar opening tiers. Reproducible compiler and readable source archive included; no runtime dependency.
- [x] Share consumed-word history across Words/Mixed and cloud merges; preserve it across cold starts. Change every stage/round and retry; no repeat before each finite bucket is exhausted. No repeated word across the full Words+Mixed campaign audit.
- [x] Offer all 540 stable nodes in each category. Mixed contains a real word plus shapes from the first round. Use six actual visible shapes, exclude adjacent/retry duplicates and solver-prove 16 necessary alternative triads.
- [x] Add version 7 stage-pinned receipts and shared server reconstruction while retaining exact catalog hashes and old checkpoint/proof formats. Personal missions use the same draw path with their independently certified boards.
- [x] Verify 14,580 witness replays and 14,580 shortest-path minimums; 68 EditMode +30 unique PlayMode tests and1,120 local server checks passed; results recorded in Reports/StageVarietyOct29. Inspect actual EN/RU/UK first-Mixed and eight-slot phone renders; fix source-locale preparation and compact stage-label overflow found by those checks.
- [x] Source/content only. No player build, installation, store upload or live deployment; compatible Cloud Code must precede the next authorised version 7 client release.
Evidence: Reports/StageVarietyOct29/verification.json; contract and rollout: Docs/STAGE_VARIETY_OCT29.md.

## Legal links centred on footer plank — October29
- [x] Raise Terms/Privacy by24 reference units and align their centres at240/480 on the720-wide composition in both entitlement states. Preserve existing widths, type, shadows and URLs.
- [x] Leave the recurring-billing block byte-for-byte unchanged. Review24 real Editor renders in EN/RU/UK at320/390/588/768 widths, including both states and compact large text; existing27 localized padding cases pass.
- [x] Remove temporary capture fixture and restore exact ProjectSettings. No player build/install/upload; neighbour Unity project untouched.
Evidence: Reports/LegalFooterPlacementOct29/verification.json, footer-preview.png and source.diff.

## UI layout follow-up after legal footer move — October29
- [x] Run broad existing menu/story/settings regressions. Confirm the legal hit-region overlap came from the last footer move; reproduce the comic-centre issue with old footer coordinates to separate its origin.
- [x] Keep approved footer ink positions and unchanged billing; separate at-least 44px legal hit regions from Restore purchases. Reacquire the comic caption's current centring owner after shared binding.
- [x] Inspect actual compact renders and fix clipped comic/dialogue primary actions with an opt-in primary/tertiary row layout; preserve shared primary size and callbacks.
- [x] Nine unique Editor tests pass, including 126 shared-action views with full screen bounds, 81 menu-text views, 72 subscription/shop touch states, 216 comic pages, 27 story contexts, 25 settings views, 27 long-price states and 12 gameplay/checkpoint/win views. The stale Settings test now recognises the already-existing inline Agegroup action.
- [x] Restore exact ProjectSettings; preserve gameplay/economy, legal billing, artwork and shared centring component. No player build/install/upload or live deployment. Known pre-existing synthetic RUB-glyph warning recorded in the report, separate from geometry checks.
Evidence: Reports/UILayoutRegressionOct29/verification.json, review.jpg and source.diff.

## Shared buttons completion audit — October29
- [x] Find the remaining unbound custom map controls: register Travel as PrimaryGreen and home/settings as IconSecondary, supplying their existing face images. Shared 480×104 /96×96 dimensions,34-unit text and48-unit icon slots now apply.
- [x] Canonicalise Map aliases and retired Find in shop captions. Synchronise live Continue/Map palette, scroll and centred content immediately in comics/conversations; reuse the scroll and restore prior icons, keeping callbacks intact.
- [x] Verify all five existing preset/prefab assets without regeneration; preserve all22 asset/meta hashes and all5 card-factory hashes. Shop/My things cards and contextual compound controls retain authored layouts.
- [x] Ten unique final Editor tests pass in EN/RU/UK across320/390/430 widths:126 action views,12 gameplay/result views,216 comic pages,27 story contexts,90 menu-text states and72 footer touch states, plus badges/card-centre checks. Inspect actual game/story/library render sheets.
- [x] Remove temporary inventory fixture; retain meaningful live-caption and active group-centre regressions. Restore exact ProjectSettings. No player build/install/upload, live service change or control of neighbour Unity.
Evidence: Reports/ButtonCompletionOct29/verification.json, game-screens.jpg, story-actions.jpg, button-library.png and source.diff.

## Restore authored menu icons — October29
- [x] Restore the original main-menu My things / Shop / Settings illustration tiles, their frame, large icons and stacked captions. Restore the baked account door and No Ads shortcuts as well. Explicitly opt these5 controls out of shared action styling, retaining the Home Play action and all other shared defaults.
- [x] Remove the Play triangle from map Continue and centre the sole text label, retaining shared dimensions/type and existing navigation callback.
- [x] Inspect the original unification diff for analogous semantic icon substitutions; no further unexpected replacements found. Card factories, gameplay icon source, shared button view and22 design asset/meta files retain their pre-fix hashes.
- [x] Four final Editor tests pass:18 Home +18 map configurations in EN/RU/UK,320/390/430 widths, both text preferences, actual raycast/minimum touch/non-overlap checks and4 Home navigation actions, plus126 broad action views,90 menu-padding states and12 gameplay/result views. Visually inspect actual before/after renders.
- [x] Restore exact ProjectSettings and remove this task's scratch data. No mobile player build/install/upload; neighbouring Unity project not controlled.
Evidence: Reports/MenuIconsRestoreOct29/verification.json, comparison.jpg and source.diff.

## GitHub source checkpoint — October29
- Save the accumulated Flying Ladders client, native platform integrations, Unity cloud/server work, website sources, licensed word pools and assets on codex/welcome-motion-oct16.
- Include the latest authored Home icon restoration and text-only map Continue action; latest targeted Unity Editor validation passed4/4 tests. No new player build or store upload for this checkpoint.
- Exclude local Cloudflare/Unity caches, temporary test scenes, crash dumps and generated website output. Preserve Website sources as regular files; no nested gitlink or remote history rewrite.
