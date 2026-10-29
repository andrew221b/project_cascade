# Что действительно осталось — 21 октября 2026

Проверено на живых сервисах 21 октября в 09:55 UTC. Ошибка Unity `Backend URL secret is not an absolute URL` устранена. Production Cloud Code успешно выполнил `CascadeAccount/ReadAccount` (HTTP 200), сервер принял защищённый запрос (HTTP 200), расписание `cascade-account-maintenance` и его Trigger существуют. Публичные страницы поддержки, конфиденциальности, условий и удаления аккаунта отвечают HTTP 200. Новые Unity-секреты и новые ключи сейчас не нужны.

## Точная причина текущих ошибок

1. **Google Play, создание монет:** Android Publisher возвращает `Can't create product. To fix, request billing permission.` В Play Console ещё нет ни одного принятого AAB. Локальный AAB содержит `com.android.vending.BILLING`, но Google узнаёт о разрешении только после загрузки bundle для `com.vickand.flyingladders`. Старый AAB от 19 октября не загружать: после него проект и live-конфигурация менялись. Нужен свежий AAB.
2. **Apple, `MISSING_METADATA`:** у всех девяти покупок есть цены, регионы и три локализации. У каждой отсутствует обязательный App Review screenshot. URL уведомлений к этой ошибке не относится.
3. **Apple production StoreKit 401:** запрос отклоняется Apple до обращения к callback. Sandbox использует тот же callback и доставил тестовое уведомление со статусом `SUCCESS`. Это не ошибка URL и не мешает TestFlight; production надо перепроверить после привязки одобренной сборки и покупок.

## До внутреннего теста Google Play

1. Собрать и проверить свежий AAB с увеличенным `versionCode`, Billing permission, production IDs и выключенными test-ad/test-monetization флагами только после проверки детского рекламного режима.
2. С разрешения владельца загрузить AAB во внутренний трек. После принятия Google создать шесть consumable-продуктов и перечитать каталог. Три подписки и их base plans уже `ACTIVE` в 173 регионах.
3. В Play Console выбрать существующий список/Google Group тестеров. API сейчас показывает ноль назначенных групп; автоматический выбор «самого большого списка» невозможен без конкретного имени группы.
4. В Monetize → Monetization setup → Real-time developer notifications указать `projects/tracker-project-497108/topics/flying-ladders-rtdn` и выполнить Test notification. Topic, защищённая push-подписка и серверный endpoint уже подготовлены.
5. Заполнить ручные формы: App access, Ads, IARC content rating, Target audience 9–12/13–15/16–17/18+, Families, Data safety и Advertising ID. Черновик фактов: `Reports/GooglePlayOct19/policy-draft.json`. Attestation Families нельзя подтверждать до проверки финального AAB, возрастной обработки рекламы и входа детей.

Текущий monetization asset намеренно не является release-ready: `storeCatalogRegistered=0`, `adsReleaseReady=0`, `testMonetization=1`, `admobTestAds=1`, `productionRelease=0`. Защитный pre-build guard правильно остановит store candidate. Для аудитории 9–12 и взрослых нельзя просто включить общий child-directed флаг для всех: нужен нейтральный возрастной выбор и разные правила рекламы/идентификаторов для ребёнка, неизвестного возраста и подтверждённого взрослого либо изменение заявленной аудитории.

## До внутреннего теста Apple

1. Собрать и проверить свежий iOS archive, затем с разрешения владельца загрузить его в App Store Connect и добавить внутренних TestFlight-тестеров.
2. Проверить на физическом устройстве Apple/Google вход через Unity Player Accounts, покупку, восстановление, перезапуск и удаление аккаунта. TestFlight использует Sandbox.
3. Для самого внутреннего TestFlight app screenshots и отправка App Review не требуются. Для подготовки девяти IAP к review нужны девять review screenshots.

## До релиза

- Apple: добавить девять review screenshots покупок, store screenshots приложения, заполнить App Privacy, age rating, export compliance, review contact и Content Rights; привязать build и покупки к версии 1.0. Первые IAP подаются вместе с версией приложения. Не отправлять без отдельного разрешения владельца.
- Google: завершить Data safety/Families/IARC по фактическому финальному AAB, завершить закрытое тестирование по требованиям аккаунта, проверить реальные покупки/RTDN/refund и только затем продвигать release.
- AdMob: подтвердить UMP и возрастную обработку смешанной аудитории, затем провести настоящий rewarded SSV. `serverRewardsConfigured` остаётся `false` до этой проверки.

## Что уже готово

- Сервер, URL, Unity secrets, Cloud Code, Scheduler/Trigger и удаление временного Unity-пользователя проверены.
- Apple notification URLs V2 для Sandbox и Production заполнены; Sandbox delivery успешна.
- Google Play API, store listing EN/RU/UK и три подписки в 173 регионах готовы.
- Unity 6000.3.8f1 скомпилировал текущие account-runtime/editor исходники, применил production Player Accounts и purchase verification, обновил EN/RU/UK таблицы; native validation завершилась `PASS`.
- Bundle/package ID: `com.vickand.flyingladders`; название: `Flying Ladders`.
- Скриншоты, загрузка сборок и отправка на review не выполнялись согласно текущему ограничению владельца.

Полный снимок проверки: `Reports/ReleaseReadinessOct21/`.
