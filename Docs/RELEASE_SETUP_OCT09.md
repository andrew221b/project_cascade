# Что осталось перед релизом CASCADE

Аккаунты магазинов и AdMob уже есть. Ниже — конкретная конфигурация проекта и оставшаяся работа. Пустые поля нельзя заменять случайными значениями. Секреты держать на сервере/в Secret Manager, не в Unity и не в переписке.

## Готовые файлы для заполнения

- `Configs/admob.json`: вставить шесть публичных AdMob ID. App ID содержит `~`, ID rewarded/interstitial — `/`. Затем в Unity: **CASCADE → Release → Apply AdMob IDs from Configs**. Команда проверяет все поля и переносит App ID также в настройки SDK. Пустые, тестовые или повторяющиеся ID отклоняются до изменения активных настроек. Тестовые/релизные флаги автоматически не переключаются.
- `Configs/release-ids.json`: перечень остальных публичных ID и адресов. Этот файл служит списком настройки; Bundle ID/package name, Unity Environment ID и серверные поля не меняются автоматически. Project ID, имя окружения и модуля, три Product ID уже внесены.
- `Backend/ReceiptVerifier/.env.example`: шаблон серверного окружения. Файлы ключей Apple/Google и токены устанавливаются на сервере; в два JSON выше их добавлять нельзя.

## 1. Завершить серверные механизмы

- Атомарное создание первого облачного аккаунта и запись владельца транзакции. Для существующих записей проверка версии есть; использованный защитный токен отсутствующей Cloud Save записи нельзя считать действующим create-if-absent. Нужен серверный механизм с гарантией единственного создания и проверкой одновременных запросов. Без него включать облачные покупки нельзя.
- Серверное подтверждение rewarded AdMob (SSV), проверка подписи, уникальной транзакции и конкретного игрока. Пока при включённом облаке реальные предложения rewarded отключены: клиентский callback не является подтверждением награды.
- Обработка возврата/отзыва покупок через Apple App Store Server Notifications и Google Real-time Developer Notifications; обновление прав без повторной выдачи расходуемой валюты. Текущий verifier отклоняет отозванные новые запросы, но автоматическое применение возврата к уже выданным правам не закончено.
- Удаление аккаунта из игры: локальные/облачные данные, авторизация и подтверждение результата. Сохранение обезличенных финансовых доказательств требует определённого срока хранения. Для приложений с созданием аккаунта Apple требует возможность начать его удаление из приложения.
- Проверить переход старых результатов без доказательства прохождения в новую облачную проверку. Локальный прогресс сохраняется, неподтверждённый результат не выдаёт облачную валюту. Для существующих игроков нужна проверенная миграция либо повторное прохождение.
- Для долгого масштабирования нужна серверная архивация истории чеков с сохранением защиты от дублей. Текущий предел — 10 000 наград; теперь при заполнении новые покупки/видео не начинаются. Проверка восьми устройств и 10 000 чеков проходит, но это не бесконечное хранилище.

## 2. Покупки в обоих магазинах

Создать именно эти продукты; код менять не требуется:

| Product ID | Тип |
|---|---|
| `cascade.no_automatic_ads` | Non-consumable / отключение автоматической рекламы |
| `cascade.coins_240` | Consumable / 240 монет |
| `cascade.coins_480` | Consumable / 480 монет |

Заполнить цену, локализованные названия/описания, доступность и материалы проверки продуктов. Проверить договоры, налоговые и платёжные сведения. Привязать продукты к приложению с тем же Bundle ID / package name.

## 3. AdMob

В `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/MonetizationSettings.asset`:

| Поле | Значение из AdMob |
|---|---|
| `iosAdmobAppId` | iOS App ID, `ca-app-pub-…~…` |
| `androidAdmobAppId` | Android App ID, `ca-app-pub-…~…` |
| `iosRewardedUnitId` | iOS rewarded unit, `ca-app-pub-…/…` |
| `iosInterstitialUnitId` | iOS interstitial unit |
| `androidRewardedUnitId` | Android rewarded unit |
| `androidInterstitialUnitId` | Android interstitial unit |

В `CascadeUnity/Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset` поля `adMobIOSAppId` и `adMobAndroidAppId` должны совпадать с соответствующими App ID выше. App ID и Ad Unit ID различаются.

Настроить UMP consent/privacy messages и доступ к privacy options; определить возрастную политику. ATT нужен при использовании отслеживания, а не автоматически для всякой рекламы. Настроить developer website / app-ads.txt по инструкции AdMob. Сначала реальные SDK с тестовыми блоками, затем production units.

## 4. Unity Cloud — да, для текущей реализации

Используются Authentication, Player Accounts, Cloud Save и Cloud Code. Уже указан Project ID `a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`; проверить принадлежность вашему проекту. Потребуются:

- Окружение `production`; права доступа, лимиты/квоты и оплата используемых сервисов.
- Authentication / Unity Player Accounts, разрешённые платформы и мобильные callback/deep-link настройки. Анонимный вход не заменяет привязанный аккаунт для восстановления после переустановки.
- Cloud Save protected player data и private custom data с записью только сервером.
- Развернуть обновлённый Cloud Code модуль `CascadeAccount` вместе с клиентом, включая опубликованные рецепты и отдельные доказательства рекордов звёзд/счёта. Проверить конкурентные записи и размеры данных на живом сервисе.
- После изменения уровней обновлять готовые рецепты через `python3 Tools/publish_campaign_recipes.py`; `--check` проверяет 6471 прохождение и соответствие игровых ресурсов серверным данным. Клиент и модуль развёртываются согласованно.
- В Unity Secret Manager: `CASCADE_RECEIPT_VERIFIER_URL` = HTTPS URL `/verify`; `CASCADE_RECEIPT_VERIFIER_TOKEN` = секрет доступа к verifier.

В `CloudPlayerSettings.asset`: `projectId`, `environment=production`, `moduleName=CascadeAccount`; только после проверки включить `enabledForPlayers`, `playerAccountsConfigured`, `purchaseVerificationConfigured`.

Unity Economy, Multiplayer, Vivox, Analytics и Cloud Build не обязательны для текущего проекта. Автоматический запуск сборок в Unity Cloud отдельно не нужен.

## 5. Сервер проверки покупок

Развернуть `Backend/ReceiptVerifier` на HTTPS-хостинге. Настройки из `.env.example`:

| Имя | Что указать |
|---|---|
| `BUNDLE_ID` | Точный Bundle ID / package name вашего приложения; пример проекта — `com.vickand.cascade`, проверить по зарегистрированному приложению |
| `SERVICE_BEARER_TOKEN` | Сильный случайный серверный секрет; совпадает с `CASCADE_RECEIPT_VERIFIER_TOKEN` |
| `UGS_PROJECT_ID` | Unity Project ID |
| `UGS_ENVIRONMENT_ID` | ID production-окружения из Unity Dashboard, не название `production` |
| `APPLE_ENVIRONMENT` | `Sandbox` для проверки, `Production` для релиза |
| `APPLE_APP_ID` | Числовой Apple ID приложения в App Store Connect |
| `APPLE_KEY_ID` | Key ID ключа App Store Server / In-App Purchase API |
| `APPLE_ISSUER_ID` | Issuer ID App Store Connect |
| `APPLE_PRIVATE_KEY_FILE` | Серверный путь к закрытому `.p8` |
| `APPLE_ROOT_CA_FILES` | Серверные пути к доверенным корневым сертификатам Apple |
| `GOOGLE_APPLICATION_CREDENTIALS` | Серверный путь к JSON service account с доступом Android Publisher API и приложению в Play Console |
| `PORT` | Порт сервиса, обычно 8080 за HTTPS-прокси |

Старое Unity-поле `receiptValidationUrl` не используется для этого server-only пути; токен туда вставлять нельзя.

## 6. Подпись и настройки сборок

В Player Settings выставить зарегистрированные iOS Bundle Identifier и Android package name. Сейчас там остаются шаблонные Unity identifiers; старый Xcode export использовал `com.vickand.cascade`, это не подтверждение регистрации такого ID в магазинах. Они должны совпасть с магазинами и verifier. Нужны Apple Developer Team ID / provisioning, Android keystore и key alias, актуальные version/build numbers. Ключи подписи хранить отдельно от репозитория.

На 9 октября 2026 требования: загрузки Apple с Xcode 26+ и iOS 26 SDK+; новые приложения/обновления Google Play — target Android 16 / API 36, если нет действующего исключения. Проверить поддержку выбранной версии Unity и пакетов при реальной сборке.

Релизные флаги в MonetizationSettings: `testMonetization=false`, `admobTestAds=false`, `productionRelease=true`, `storeCatalogRegistered=true`, `adsReleaseReady=true`. Включать после закрытия серверных пунктов и проверок. Промежуточные мобильные сборки должны использовать Development Build; release guard теперь не обходится выключением production-флага.

## 7. Проверки перед подачей

- На iPhone/Android: холодный запуск, повторное открытие после выгрузки/блокировки, потеря сети, повреждённое/большое сохранение, память/FPS/нагрев, быстрые касания/drag/Undo, музыка и реклама после возврата в игру. Editor не подтверждает эти показатели.
- Покупка, отмена, отложенное одобрение, восстановление no-ads, повторные callbacks, два устройства/аккаунта, возврат денег. Реальные Sandbox/TestFlight и Play internal testing.
- AdMob: consent, отказ, rewarded/interstitial, close-before-reward, уход в фон, отсутствие сети и отсутствие заполнения. SSV выдаёт ровно одну награду.
- EN/RU/UK на маленьких экранах, Large Text, ночные фоны, все подвесные предметы, уровни с несколькими эффектами. Проходимость проверена автоматически; интересность требует игровых сессий людей.
- Crash reporting / мониторинг серверных ошибок, лимитов и отказов; способ отката конфигурации/сохранения.

## 8. Страницы магазинов

Название, описание и keywords, иконка, скриншоты, поддерживаемые языки, категории/возраст, Privacy Policy и support URL, App Privacy / Data Safety по фактическим SDK, сведения о рекламе и покупках, privacy manifests / разрешения, инструкции ревьюеру. Проверить права на графику, музыку и шрифты. Закончить соглашения, платёжные/налоговые сведения и регионы публикации.

От вас нужны реальные идентификаторы приложений/рекламных блоков, данные Apple API и Google service account, домен/хостинг verifier, публичные URL и решения о возрастной аудитории/данных. Закрытые ключи устанавливаются безопасно в серверном окружении. Мобильные сборки и установка выполняются только после вашего отдельного запроса.

Официальные источники: [Cloud Save write locks](https://docs.unity.com/en-us/cloud-save/concepts/write-locks), [AdMob SSV](https://developers.google.com/admob/unity/ssv), [UMP](https://developers.google.com/admob/unity/privacy), [Apple account deletion](https://developer.apple.com/help/app-review/guideline-reference/5-1-1-account-deletion), [Apple SDK requirements](https://developer.apple.com/news/upcoming-requirements/?id=04282026a), [Google target SDK](https://developer.android.google.cn/google/play/requirements/target-sdk?hl=en).

## Частота рекламных пауз сейчас

Автоматический показ — при возврате на карту после сохранённой победы, статистики и подарка: минимум три новых полных уровня и 180 секунд активного времени с начала сессии/предыдущей рекламы; максимум два показа за сессию. Финалы миров, повторные прохождения, обучение, проигрыш, Retry и этапы внутри уровня исключены. Кнопка следующего уровня ведёт дальше без рекламной паузы. Нет готового объявления — возможность пропускается. Добровольное rewarded — только по нажатию в магазине/инвентаре; его показ тоже сбрасывает интервал. Тестовый режим останется до ваших ID и проверки production-конфигурации.
