> Актуальные ссылки и результаты проверки сервисов: [SERVICE_LINKS_CURRENT_RU.md](SERVICE_LINKS_CURRENT_RU.md). Описания прежних проверок ниже сохраняют свой исторический контекст.

# Google Play: Finish setting up your game — текущий статус

Проверено 21 октября 2026 для `com.vickand.flyingladders`.

## Что уже сохранено удалённо

- AAB version code 1 принят Google Play.
- Закрытый трек `internal-closed-testing` создан, релиз остаётся `draft`, тестеров нет.
- Шесть consumable-наборов монет и три подписки ACTIVE в 173 регионах.
- EN/RU/UK store listing и контакты поддержки сохранены.
- Privacy policy сохранена.
- Ads: Yes.
- Content rating завершён; итоговые IARC-категории рассчитаны Google.
- Government apps: No.
- Financial features: No.
- Health: No.
- Категория: Game → Puzzle.

На момент первоначальной записи App Dashboard показывал **7 of 11 complete**. Последующая проверка ссылок показала **9 of 11**; актуальные оставшиеся действия перечислены в SERVICE_LINKS_CURRENT_RU.md.

## Data safety — черновик полностью заполнен

Шаги 1–4 заполнены, все 10 выбранных типов данных имеют статус `Completed`, черновик сохранён и проверен после перезагрузки:

- Email address;
- User IDs;
- Purchase history;
- Approximate location;
- Crash logs;
- Diagnostics;
- Other app performance data;
- App interactions;
- Other actions;
- Device or other IDs.

В Preview отражены сбор и передача для Unity Player Accounts/Cloud Save/IAP и AdMob, TLS, privacy URL и удаление аккаунта через `https://cascade-sky-gardens.active-seed-4233.chatgpt.site/delete-account`.

Финальная кнопка `Save` заблокирована самой Console: сначала нужно завершить Target audience. Черновик не отправлялся на review.

## Что осталось в Dashboard

1. **Sign in details.** Console требует отдельные reviewer credentials и обязательное подтверждение, что они дают полный доступ ко всем функциям, включая платные товары и подписки. Гостевой режим этого не подтверждает. Нужен выделенный аккаунт ревьюера с выданными тестовыми entitlements либо релизная конфигурация, где ревьюер действительно получает такой доступ.
2. **Target audience.** Выбранная владельцем аудитория: 9–12, 13–15, 16–17, 18+. Раздел заблокирован до завершения Sign in details.
3. **Data safety.** После Target audience открыть сохранённый Preview и нажать `Save`; это сохраняет анкету, затем изменения отдельно попадут в Publishing overview.
4. **Store listing.** EN/RU/UK тексты уже сохранены. Графика и screenshots намеренно не загружались по указанию владельца.

Закрытый тест остаётся draft. Новая сборка, screenshots, tester invitations, review submission и production rollout не выполнялись.

## Зафиксированные ответы

- Privacy policy: `https://flyingladders.netlify.app/privacy/`.
- App category: Game → Puzzle.
- Ads: Yes. Планируются automatic interstitial после каждого второго нового пройденного уровня и добровольные rewarded; no-ads отключает только automatic interstitial.
- App access: основной игровой цикл доступен через Play as guest, но общих reviewer credentials с полным платным доступом пока нет.
- News app: No.
- Government app: No.
- Health features: No.
- Financial features: No. Виртуальные монеты не являются банковской, инвестиционной, кредитной, денежно-переводной или криптовалютной функцией.
- Social/dating, chat и UGC: No.
- Content rating facts: сказочные неудачи и бомбы-препятствия; нет крови, реалистичного насилия, секса, брани, алкоголя/наркотиков, азартных игр, денежных призов, платного случайного лута, чата или UGC. Есть необязательные покупки. IARC рассчитывает рейтинг сам.
- Account deletion URL: `https://cascade-sky-gardens.active-seed-4233.chatgpt.site/delete-account`.

## Что нельзя подтверждать вслепую

Владелец выбрал аудиторию 9–12, 13–15, 16–17 и 18+. Поэтому финальные Target audience/Families и Data safety должны соответствовать смешанной детско-взрослой аудитории.

Текущая production-заготовка AdMob всё ещё имеет `childDirected=false`, `underAgeOfConsent=false`, тестовый режим и неподтверждённый возрастной/consent flow. Unity Cloud Save предупреждает, что для детских пользователей может требоваться Verified Parental Consent. Для финального live-релиза нужно либо:

1. реализовать и проверить нейтральный возрастной экран, детский режим рекламы/аккаунта и нужное родительское согласие; либо
2. отдельным решением владельца убрать 9–12 из target audience.

Без этого нельзя честно завершить Families attestation для будущей live-сборки.

## Data safety: консервативная основа для финальной live-сборки

После включения заявленных сервисов необходимо раскрыть как минимум:

- Unity Player Accounts: email, user/personal identifier, approximate location и other app activity в соответствии с документацией Unity;
- Unity Authentication/Cloud Save: Player ID, IP address и облачный игровой прогресс;
- Google Mobile Ads: approximate location derived from IP, user product interactions, diagnostics и device/account identifiers; цели advertising, analytics и fraud prevention;
- покупки: purchase history и серверные purchase tokens/entitlements для app functionality и fraud prevention;
- account deletion: in-app path и публичный web URL.

Финальные collected/shared/optional/required ответы следует сверять с фактически загруженной live-сборкой. Catalog-bootstrap AAB намеренно не инициализирует live ads/accounts и не является доказательством будущего поведения.

## Доступ агента к веб-формам

Android Publisher service account не предоставляет API для большинства App content/Dashboard анкет. Для заполнения от имени владельца нужен авторизованный Play Console в Chrome и разрешение macOS на управление Chrome.

1. Открыть Flying Ladders в Chrome на Mac и оставить вкладку Play Console активной.
2. macOS → System Settings → Privacy & Security → Automation → разрешить Codex управлять Google Chrome. Если там показан Terminal, разрешить и его.
3. Chrome → View → Developer → Allow JavaScript from Apple Events.
4. Доступ подтверждён и использован. Production rollout, review submission и tester invitations остаются отдельно запрещены без нового указания.
