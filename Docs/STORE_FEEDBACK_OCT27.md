# Shop feedback and service latency — October 27

## Findings and corrections

The receipt toast used a generic dark panel and entered automatic menu typography / illustrated-number processing. Generic styling could change its foreground and font/material after creation. The toast now reuses the existing pale settings-row skin, a matched InterfaceBody font/material and an explicitly owned reference caption. It stays readable and centred with the shared icon/caption layout, a four-logical-pixel gap and existing internal-inset rules. No generated artwork or new fonts.

IAP 5.4.4 previously awaited UGS setup and purchase identity before connecting to the native store, delaying product/pricing metadata. The native catalog now connects independently, and initialization starts after the known age choice even while the account screen is open. The purchase identity warms in the background only after leaving that screen and when no account operation is active. A buy tap shares any in-flight warmup, always rechecks the active binding, and sets the native account token before PurchaseProduct. Payment/reward verification and idempotent durable grants remain mandatory. Catalog metadata readiness does not imply that networking is instant.

Ad consent/SDK preparation remains dormant for unknown age and while the account gate owns the screen; it starts on entering Home, using existing audience/UMP/ATT restrictions. Verified rewarded polling checks more frequently during the first seconds. The coin-reward callback refreshes the current shop/bag wallet before showing the toast. Publisher flags and production IDs were not changed; no SSV configuration was newly enabled.

Cloud Queue formerly used Max with the 30-second post-save timestamp, so new changes inherited that idle delay. It now brings normal unsent changes forward to at most three seconds without bypassing network-failure backoff. Receipt verification avoids an unnecessary upload when the account is ready and only an unsent dirty snapshot exists; authoritative results merge that snapshot. Actual in-flight/persisted requests still finish first. Reward intent/read calls wait for ongoing saves rather than silently rejecting the tap or waiting for the next poll. Identity-change/deletion/conflict guards remain in place, and missing signed rewards cannot invent coins.

## Validation

- Nine Editor PlayMode tests passed: five native-purchase boundary tests, nine locale/viewport receipt renders in one UI test, direct visible-wallet rewarded update, and two existing ad-flow regressions.
- 138 checks passed on the actual CloudSyncClient with fake SDK/network boundaries, including queued-save timing, receipt grants during a save, avoiding a redundant upload, unsent progress retention, reward requests waiting for saves, missing-SSV rejection and duplicate signed-reward reads.
- iOS presentation code compiled directly with the existing iOS compiler response configuration. Android Editor PlayMode compilation also succeeded. Existing compiler warnings remain.
- EN/RU/UK receipt screenshots visually inspected at phone sizes; all nine layouts pass no-overflow and matching font/material assertions, including larger text and iPad.

No mobile player/Xcode/APK/AAB build, physical installation, live purchase/ad request verification, cloud deployment or store submission was performed. Tests use temporary isolated profiles and fake transports. Existing server deployment and release-readiness limitations are not resolved by these client-only checks. The next authorized device build must confirm cold-start store readiness, real purchase latency and ad grant timing on the actual network.
