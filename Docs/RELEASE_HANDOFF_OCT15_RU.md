# Релиз Flying Ladders / CASCADE: ID и оставшаяся работа

Сверено 15 октября 2026 с `PurchaseCatalog.cs`, `NativePurchaseStore.cs`, `Configs/release-ids.json`, `Backend/ReceiptVerifier/verify.mjs`, текущими Cloud Code endpoints и release guards. Это актуальный список; старые документы от 7–10 октября описывают прежнюю разовую покупку и прежнюю частоту рекламы.

## 1. Что создать в App Store Connect и Google Play Console

Использовать одинаковые **Product ID** в обоих магазинах. Регистр/точки/подчёркивания сохранить.

| Product ID | Тип | Содержимое / период |
| --- | --- | --- |
| `cascade.coins_240` | Consumable | 240 монет |
| `cascade.coins_480` | Consumable | 480 монет |
| `cascade.coins_600` | Consumable | 600 монет |
| `cascade.coins_720` | Consumable | 720 монет |
| `cascade.coins_960` | Consumable | 960 монет |
| `cascade.coins_1080` | Consumable | 1080 монет |
| `cascade.no_ads_week` | Auto-renewable subscription | 1 неделя, `P1W` |
| `cascade.no_ads_month` | Auto-renewable subscription | 1 календарный месяц, `P1M` |
| `cascade.no_ads_year` | Auto-renewable subscription | 1 календарный год, `P1Y` |

У всех трёх подписок одна выгода: отключение автоматической рекламы на оплаченный период. Добровольные видео за награду остаются. Нет трёх разных уровней преимуществ.

**Apple:** одна Subscription Group, например `No Ads`; три продукта выше в одной группе с одинаковым уровнем доступа и разными периодами. Внутреннее название группы не является Product ID игры. Возможность иметь только одну подписку в группе и доступные периоды описаны в [Apple subscription reference](https://developer.apple.com/help/app-store-connect/reference/in-app-purchases-and-subscriptions/auto-renewable-subscription-information/).

**Google:** именно три subscription product, соответствующих трём ID; у каждого создать и активировать auto-renewing base plan с нужным периодом, ценами и регионами. Например, названия base plan `week`, `month`, `year` — это предлагаемые имена настройки, они не зашиты в код как дополнительные товары. Не заменять три Product ID одним продуктом с тремя планами. Текущий клиент покупает по Product ID; выбор/доступность планов проверить в internal testing. Отдельные акционные offers в этой версии не настроены. [Модель subscription/base plan Google](https://developers.google.com/android-publisher/api-ref/rest/v3/monetization.subscriptions).

**Совместимость:** десятый ID `cascade.no_automatic_ads` имеет тип Non-consumable и остаётся в запросе каталога, verifier и restore только для прежних владельцев. Новая покупка этого ID запрещена клиентом. Если продукт уже продавался, сохранить его ID и права владельцев. Если никогда не продавался, не предлагать его как новый вариант вместо подписок; отсутствие старого продукта также проверить при загрузке реального каталога.

Предметы помощи и трофеи покупаются за игровые монеты: отдельных IAP ID у них нет. Рекламные подарки/подсказки — рекламные placements, а не магазинные продукты.

Цены задаёте в сторах. Клиент получает локализованные цену/валюту из store metadata; пока они не получены, показывает `—`, без выдуманной цены. Годовая экономия рассчитывается по ценам месяца/года в одной валюте. Цифры на референсе не перенесены как реальные цены. Для каждого продукта нужны локализации, цена, регионы, доступность и материалы ревью; также действующие договоры/налоговые/платёжные данные магазинов.

## 2. Публичные значения, которые нужны от вас

| Что прислать / заполнить | Куда | Текущее состояние |
| --- | --- | --- |
| iOS Bundle ID и Android package name | `Configs/release-ids.json`, затем Player Settings и verifier | Пустые; в Player Settings ещё Unity-шаблоны |
| Числовой Apple App ID приложения | `appleAppId`, сервер `APPLE_APP_ID` | Пусто |
| Apple Developer Team ID | `appleTeamId`, подпись iOS | Пусто |
| App Store Connect Issuer ID / Key ID | `appleIssuerId`, `appleKeyId`, сервер | Пусто |
| Unity Player Accounts Client ID | `Configs/authentication.json` → `unityPlayerAccountsClientId` | Пусто |
| Unity production Environment ID, именно GUID | `unityEnvironmentId`, сервер `UGS_ENVIRONMENT_ID` | Имя `production` есть, GUID пустой |
| HTTPS-хост backend verifier | `receiptVerifierUrl`, Secret Manager `CASCADE_RECEIPT_VERIFIER_URL` | Не задан |
| AdMob publisher ID `pub-…` | `admobPublisherId`, сайт/app-ads.txt | Пусто |
| Шесть AdMob ID из следующей таблицы | `Configs/admob.json` | Все пустые |
| Рабочие HTTPS Privacy / Terms / Support URL | `Configs/website.json`, LegalSettings/import | Все три не заполнены/не подтверждены |
| Android key alias и способ подписи | `androidKeystoreAlias`, защищённая конфигурация сборки | Не заполнено |

Unity Project ID уже указан: **`a1b8a760-5a0c-4c2d-8ae7-54c1b411259a`**. Cloud Code module: **`CascadeAccount`**. Данные игрока: защищённый ключ **`cascade_account_v1`**. Это уже выбранные значения, не новые ID для генерации.

| Поле AdMob | Формат / назначение |
| --- | --- |
| `iosAdmobAppId` | `ca-app-pub-…~…`, iOS приложение |
| `androidAdmobAppId` | `ca-app-pub-…~…`, Android приложение |
| `iosRewardedUnitId` | `ca-app-pub-…/…`, iOS rewarded |
| `iosInterstitialUnitId` | `ca-app-pub-…/…`, iOS interstitial |
| `androidRewardedUnitId` | `ca-app-pub-…/…`, Android rewarded |
| `androidInterstitialUnitId` | `ca-app-pub-…/…`, Android interstitial |

У App ID знак `~`, у рекламного блока `/`. Применение: **CASCADE → Release → Apply AdMob IDs from Configs**. Не включать production-флаги до реальных проверок. Баннера в этом списке нет.

Закрытый Apple `.p8`, Google service-account JSON, Android keystore/passwords и серверные bearer-токены устанавливаются на сервере/в защищённой среде подписи; не присылать их в чат и не добавлять в клиентские assets. Публичные Key ID/Issuer ID не заменяют сам закрытый ключ. Нужны права Google Android Publisher API на приложение и серверные сертификаты Apple.

Сейчас verifier использует один `BUNDLE_ID` для обоих магазинов. Если iOS Bundle ID и Android package name отличаются, до развёртывания нужна раздельная серверная конфигурация. Не подменять зарегистрированные значения примером `com.vickand.cascade` из старого `.env.example`.

## 3. Что мне ещё нужно закончить в бэкенде

Это задачи реализации, а не поля, которые вы можете исправить одной галочкой.

1. **Атомарная первая запись и глобальный владелец транзакции.** В `CascadeAccount.cs` для отсутствующих записей ещё используется фиксированный `AbsentAccountLock`. Он не доказывает безопасный create-if-absent на живом Cloud Save. Нужен реальный серверный механизм единственного создания/уникального владения, затем конкурентные тесты. Защиту нельзя заменять безусловной записью. До этого облачные покупки не включать.
2. **Rewarded SSV.** В `NativeAdProvider` пока нет SSV options/подписанного серверного получения награды, у модуля нет endpoint выдачи такой награды. Сейчас `RewardedOffersAllowed` запрещает реальные rewarded offers при включённом облаке. Нужны: серверный билет/привязка игрока и цели, проверка подписи AdMob и transaction ID, одна выдача, повторная доставка/потеря сети и восстановление результата в клиенте. [Официальный SSV](https://developers.google.com/admob/unity/ssv).
3. **Жизненный цикл подписок и возвраты.** Verifier уже проверяет текущую store-информацию и expiry при запросе, включая Apple/Google subscriptions. Но входящих Apple Server Notifications / Google RTDN endpoints и автоматического применения отзыва к ранее выданным правам ещё нет. Нужны обновление/отмена/expiry/grace/hold, повторная доставка, защита от повторной выдачи монет и корректное восстановление прав. Отдельно проверить поведение времени/кэша прав без сети.
4. **Полное удаление аккаунта и данных.** Выход из аккаунта реализован, удаления из игры и завершённого server-side процесса ещё нет. Нужны подтверждение, удаление игрового профиля/identity/связей, обработка pending transactions и опубликованный срок хранения необходимых финансовых доказательств. Google также требует путь подачи запроса вне приложения. [Apple](https://developer.apple.com/help/app-review/guideline-reference/5-1-1-account-deletion), [Google](https://support.google.com/googleplay/android-developer/answer/13327111).
5. **Миграция и объём.** Проверить существующие локальные прохождения без серверного replay proof: локальный прогресс сохраняется, облачная валюта не должна приниматься без доказательства. Определить миграцию старых покупок без account-binding. Для долгого использования нужна архивация журнала при сохранении дедупликации; текущий предел 10 000 grant receipts приводит к запрету новых наград/покупок, а не к бесконечному хранению.
6. **Развёртывание и эксплуатация.** Подготовленный Node verifier ещё нужно разместить под HTTPS, установить store credentials/UGS secrets, развернуть согласованный модуль и каталог рецептов, настроить доступ только серверу к protected/private данным, rate limits, метрики/ошибки, резервирование и восстановление. Выполнить реальные конкурентные, межаккаунтные и межустройственные проверки. Установка флагов не заменяет эти проверки.

Практический порядок: сначала атомарное хранилище → SSV → уведомления подписок/возвраты → удаление/миграция → staging + store sandbox → production. Ваши публичные ID и защищённо установленные credentials позволяют проверить интеграции после реализации.

## 4. Сайт и policies

Сайт и локальные тексты есть в `Website/`, настроен адрес `https://cascade-sky-gardens.active-seed-4233.chatgpt.site`. Публичная доступность не подтверждена этой проверкой: инструмент чтения сайта вернул недоступность; это не доказательство, что сайт выключен.

В текущем `Configs/website.json`: `documentsApproved=false`, `publicLegalUrlsVerified=false`, `ageAudienceReviewCompleted=false`; Privacy/Terms/Support URL пустые. В локальном `Website/content/legal.json` разделы purchases/payments ещё описывают no-ads как разовую non-consumable покупку. Их надо обновить под три автопродлеваемые подписки, оставить только оговорку для старых lifetime-владельцев, затем проверить фактическую обработку данных/возрастную аудиторию, утвердить и опубликовать. После этого применить конфиг сайта к игровым legal assets; не выдавать существующие draft-флаги за утверждение документов. Эти тексты здесь не публиковались и юридическое одобрение не имитировалось.

Нужны также app-ads.txt с правильным publisher ID, поддержка и страница запроса удаления аккаунта, UMP privacy messages/доступ к privacy options, App Privacy/Data Safety по фактическим SDK. ATT настраивается исходя из реального использования tracking.

## 5. Проверки и подача

- Сначала реальные TestFlight/Sandbox и Google internal testing: цена каждого из девяти новых продуктов, покупка/отмена/отложенная покупка, повторный callback, прерывание/перезапуск, restore, expiry/renewal/refund, два store/game-аккаунта и два устройства. Client ID пустой, cloud/purchase verification флаги false, test shop и test ads сейчас включены.
- На телефонах: safe areas, маленький экран/планшет, EN/RU/UK/Large Text, фон/долгая пауза, без сети, звук после рекламы, касания/Undo, память/FPS/нагрев. Native Editor кадры не являются физической проверкой устройства.
- Подпись iOS/Android, реальные Bundle ID, версия/build number, ARM64/зависимости, актуальные SDK. На 15 октября 2026 нужны Xcode 26+ с iOS 26 SDK+; Google new apps/updates — Android 16/API 36+ (кроме действующего исключения). Player Settings `AndroidTargetSdkVersion: 0` означает automatic, а не подтверждённый API 36 в собранном AAB. [Apple требования](https://developer.apple.com/news/upcoming-requirements/?id=02032026a), [Google требования](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-EN).
- Страницы магазинов: название/описание/иконка/актуальные скриншоты, категории/возраст, регионы и цены, рекламная декларация, privacy/data forms, privacy manifests, сведения о подписке/отмене и инструкции ревьюеру. Завершить соглашения/платёжные данные.
- Только после проверки выключить `testMonetization`/`admobTestAds`, включить подтверждённые `storeCatalogRegistered`, `adsReleaseReady`, `productionRelease`, `cloudCodeDeployedAndVerified`, `purchaseVerificationConfigured`. Существующие release guards сохраняются.

Текущая частота рекламы по коду: автоматическая пауза после каждого второго нового завершённого уровня при выполнении остальных условий; каждый второй новый запрос подсказки — rewarded, с сохранением уже выданных подсказок/токенов; рекламный лутбокс запускается добровольной кнопкой. No-ads подписка не отключает добровольные rewarded offers. Старое описание «3 уровня / 180 секунд / 2 за сессию» больше не использовать.

Инструкция конкретно по входу/callback URI/активации: `Docs/ACCOUNT_BACKEND_SETUP_OCT15_RU.md`. Деплой, реальный OAuth/Cloud Save/IAP/ads и мобильная установка в этой задаче не выполняются.
