> Актуальные ссылки и результаты проверки сервисов: [SERVICE_LINKS_CURRENT_RU.md](SERVICE_LINKS_CURRENT_RU.md). Описания прежних проверок ниже сохраняют свой исторический контекст.

# Flying Ladders — App Store preparation, 18 October 2026

## Design note and authorized scope

The owner registered `com.vickand.flyingladders` and asked to prepare the App Store entry and purchases. App Store app ID is `6824290826`; version is `1.0`. This replaces the earlier iOS identity `com.vickand.cascade` for future store builds. It does not migrate an installed app or erase its saves. Android registration was later confirmed by the owner under the same package; see Docs/GOOGLE_PLAY_SETUP_OCT19.md for the separate Android preparation.

Product identifiers remain `cascade.*` to match the existing catalog. Six consumable coin packs and three auto-renewable ad-removal subscriptions are prepared. The legacy `cascade.no_automatic_ads` remains a compatibility identifier, with no new sale listing. Prices were delegated to the agent; selected prices follow observed public puzzle-game offers and improve value per coin at every larger tier. They are a judgment within observed ranges, not a statistical market average. Base USD, Apple's regional equalizations; all 173 current territories excluding China and Vietnam, no automatic new territories.

The App Store management key (`372BM7VQLX`) is distinct from the App Store Server/IAP key (`JP5FFCBZFA`). Only public identifiers belong in project configuration. No private keys, tokens, credential paths or signed upload URLs are included.

The latest owner instruction pauses all further screenshot work and forbids submission for review. No App Store or purchase review screenshots have been uploaded. No binary has been uploaded or submitted. Release is MANUAL. Existing local screenshots are retained only as incomplete internal work.

## Preparation status

Saved metadata and product preparation are documented below from API readback. Production monetization/cloud switches remain disabled pending real service and sandbox validation. A draft store catalog is not evidence of live purchases.

The explicit local iOS build helper now reads the registered bundle/team and version from the two public configuration files, uses the Flying Ladders display name, and restores the Editor's prior settings in `finally`. Persistent ProjectSettings and Android identifiers were not edited. The old installed CASCADE app is untouched. A new store identity creates a separate iOS app container; migration is not asserted. No new device or release build was requested or made after this change.

## Saved in App Store Connect

- App `6824290826`, version `1.0`, EN/RU/UK names, subtitles, descriptions, promotional text and search keywords.
- Games → Puzzle and Word categories; age questionnaire saved, Apple computed **9+**. Advertising and infrequent cartoon/weapon content disclosed; paid random-item boxes are not offered.
- Reviewer name, email, directly supplied phone and guest-access instructions. Phone normalized to `+1` for the supplied Canadian number; no private contact is included in the public evidence report.
- Free app download. App and nine products available in **173 territories**; CHN/VNM excluded. Availability in newly added territories is disabled.
- Six consumables and three same-benefit, same-level subscriptions in group `22491379`; all have EN/RU/UK text and reviewer notes. No legacy non-consumable created.

| Product ID | Apple resource ID | USD base price |
|---|---|---|
| cascade.coins_240 | 6824295287 | 0.99 |
| cascade.coins_480 | 6824295426 | 1.79 |
| cascade.coins_600 | 6824295346 | 1.99 |
| cascade.coins_720 | 6824295317 | 2.29 |
| cascade.coins_960 | 6824295318 | 2.99 |
| cascade.coins_1080 | 6824295319 | 3.29 |
| cascade.no_ads_week | 6824295353 | 1.49 / week |
| cascade.no_ads_month | 6824295355 | 3.99 / month |
| cascade.no_ads_year | 6824295415 | 19.99 / year |

Apple performs regional equalization for coin packs; 173 current subscription territory prices were written from Apple's equalizations. Automatic coin-price schedules may contain prices for unavailable territories; prices do not grant availability. Readback confirms exactly 173 available territories for every product.

The prices are a moderate selection informed by public offers, not identical units or a calculated market mean. Examples checked on 18 October 2026: [Flow Free](https://apps.apple.com/us/app/flow-free/id526641427) offers hints from $0.99 and a $4.99 unlock; [Two Dots](https://apps.apple.com/us/app/two-dots-connect-the-colors/id880178264) has currency offers from $1.99; [Puzzle Page](https://apps.apple.com/us/app/puzzle-page-daily-games/id1308181322) lists $5.99/month and $35.99/year for its broader subscription. Flying Ladders' ad-removal benefit is narrower. Storefront prices may change.

## Public support and legal pages

The owner explicitly approved publishing the developer's name, country and support email on the existing public Site after automatic approval review required separate authorization. Site version **2** is published, commit `25269da9a73eb6995aad1cd92792257668885c62`; original audience, D1 binding and server API routes preserved. All five public routes return HTTP 200 without authentication and contain the expected content:

- https://cascade-sky-gardens.active-seed-4233.chatgpt.site/
- https://flyingladders.netlify.app/support/
- https://flyingladders.netlify.app/privacy/
- https://flyingladders.netlify.app/terms/
- https://cascade-sky-gardens.active-seed-4233.chatgpt.site/delete-account

The store's three localized privacy-policy and support URL fields point to the new pages. The app's accessibility URL describes the actual comfort/language controls on the support page; no untested accessibility nutrition label was asserted. Apple standard EULA is linked in the app descriptions and website terms. Public pages are English; app/product listings are EN/RU/UK.

This publication updates public pages only. It does not establish live account creation, cloud saves, purchase validation, deletion cleanup or subscription notification handling. The existing deletion form and email fallback remain; full connected-service verification is still required. `Configs/website.json` now records public URL verification; offline legal text approval/import and age-audience review flags remain separate and unchanged.

## Intentionally unfinished / requires later work

1. **Screenshots:** paused by the owner, including product-review screenshots. There are no uploaded App Store screenshots. The nine products currently report `MISSING_METADATA`; no claim that they are approved or sellable. Screenshot preparation must be resumed only after a new owner instruction.
2. **Binary and review:** no uploaded/selected binary, no submission, no release. Version stays `PREPARE_FOR_SUBMISSION`, release type `MANUAL`. The local helper is a Development-build helper, not proof of release readiness. The old physical CASCADE app and its saves were untouched.
3. **App Privacy:** official API has no endpoint for the data-usage questionnaire. A manual-entry audit is prepared at `Reports/AppStoreConnectOct18/privacy-accessibility-audit.md`. Final AdMob linkage/tracking/consent, IAP 5.4.4 telemetry and the final activated release services must be resolved before entering truthful production answers. Do not submit “Data Not Collected” based on the current preview flags.
4. **Developer legal/financial declarations:** content rights, trader status/required verified contacts, paid-app agreements, tax and banking must be checked in the owner's store account. They were not invented or marked accepted. Public home address was not published.
5. **Live commerce/account services:** UGS environment/access, Player Accounts client settings, advertising units/UMP/ATT, receipt verification and account deletion need real service and device checks. Apple server notification URLs were not registered against an unverified callback. The proposed callback is `/api/notifications/apple` on the public origin, but Production/Sandbox configuration and signed test delivery must be validated before activation. No receipt key private material was copied into the client or repository.
6. **Google Play, updated 19 October:** package `com.vickand.flyingladders` and service-account access are verified. EN/RU/UK metadata and three DRAFT subscriptions are saved. Six consumables remain blocked by Billing permission until an accepted bundle; the owner has forbidden binary uploads. No Google release exists. Console-only policy forms and email tester-list selection remain unconfirmed; see `Reports/GooglePlayOct19/console-remaining.md`.
7. **Optional store sections:** no invented events, custom product pages, experiments, promotions, ratings or Game Center integration were created. These are not prerequisites for a standard first release.

## Validation and retained evidence

- API readback verifies all nine actual USD prices, three locales each, each product's 173 available territories and excluded CHN/VNM; app download is free.
- After URL changes, final readback is retained at `Reports/AppStoreConnectOct18/verified-store-state.json`. No private keys, auth tokens or full reviewer contact in that report.
- Existing Site backend checks: **28/28 pass**. Site framework build succeeded; local and public HTTP checks passed for root, support, privacy, terms and deletion routes. Browser visual/assistive-technology behavior and live deletion were not audited; no screenshot claim.
- Unity 6000.3.8f1 compile-only batch check in isolated QA copy succeeded with **0 compiler errors** after updating the explicit build helper. No new device build/install or complete release validation performed.
- Production monetization/account/ad flags remain disabled. Persistent Unity ProjectSettings, Android identifiers, packages, scenes, sprites, music and player saves are unchanged by this store task.

## Additional metadata pass on 19 October

At the owner's request to maximize preparation while forbidding binary/image uploads and screenshots, TestFlight EN/RU/UK descriptions, feedback email, website/privacy links and beta reviewer contact/instructions were saved. Contacts were copied from the existing owner-approved private App Review fields, not exposed publicly. Marketing URL and optional Privacy Choices URL were filled for all three store languages. The published privacy page describes access/correction/deletion/consent choices and email requests.

The existing subscription group already had all three languages. Three subscription reviewer notes were clarified to explain the exact interstitial-only benefit and optional rewarded videos without asserting unverified live server operation. No price, ID, territory or group-level change. Independent API readback confirms the new values, no uploaded/selected binary, zero store screenshots, PREPARE_FOR_SUBMISSION and MANUAL release. No TestFlight review, tester invitation or app review was initiated. Evidence: `Reports/StorePreparationOct19/apple-after.json` and `README.md`. Current saved name remains Flying Ladders while an alternative Fitting Ladders name awaits confirmation.
