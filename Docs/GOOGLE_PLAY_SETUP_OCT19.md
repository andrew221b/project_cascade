# Google Play preparation — 19 October 2026

## Design note and authorization

The owner explicitly registered Android package `com.vickand.flyingladders` and authorized reusing service account `sub-purchases@tracker-project-497108.iam.gserviceaccount.com` in project `tracker-project-497108` for Flying Ladders. This registration is owner-supplied; live Play ownership and permissions have not yet been verified. Only this application's package is in scope; DayDraft settings are not to be changed.

The owner authorized preparing an internal test release and selecting the largest existing tester list. Screenshots and production publication are explicitly forbidden. No production track mutation, app screenshot creation/upload, or new tester invitations are authorized by this preparation.

Target audience explicitly chosen by the owner: **9–12, 13–15, 16–17, 18+**. This is a target audience selection, not an IARC rating. The game includes children and must satisfy the applicable Families requirements before claiming compliance. Do not mark an 18+ audience to avoid those requirements.

Keep the existing nine `cascade.*` sale identifiers. Base USD prices reuse the selected App Store prices; Google-specific region/tax conversion must come from Google, not Apple's price equalizations. Subscriptions have one auto-renewing base plan each, P1W/P1M/P1Y, with the same ad-removal benefit. The legacy non-consumable is restore-only and is not a new sale product. Drafts must not be represented as remotely saved or active.

## Current access result

The specified local JSON matches the owner's service-account email and project, but Google rejects OAuth signing with `invalid_grant: Invalid JWT Signature`. The key ID in that exact file is absent from the account's current publicly published certificates. No Play API request or remote write succeeded. This proves a credential problem, not whether Play app permissions are sufficient.

Automatic approval review rejected a proposed search for alternative key files in the old keys directory and Downloads as credential probing from sources not explicitly authorized. That search was not executed and was not retried. The owner has been asked to provide the exact path to a newly created valid JSON key for the same service account. No private key/token is stored in the repository, copied into the client, or included in reports.

## Prepared artifacts and live restrictions

Local drafts will be collected in `Reports/GooglePlayOct19`. Public identity and desired states are in `Configs/google-play.json`. No Google Cloud IAM roles, existing service-account keys, DayDraft resources, production release or tester membership have been changed remotely. Later sections record the authorized Flying Ladders subscription draft prices.

## Explicit internal preview build design

The supplied replacement key was copied exactly to the owner-requested root of `MY DISK` with mode 0600. OAuth now succeeds for the expected identity; the Flying Ladders app is readable. No existing APK/AAB or release is present. The current Google product API accepts subscription drafts but rejects coin product creation until the app requests billing permission.

Prepare a signed, non-debuggable ARM64/API 36 internal QA AAB in an isolated copy. It retains the existing local test shop/ad simulation and disables connected player services. This is a playtest, not proof of live payments/ads/cloud. A dedicated Editor build scope explicitly permits this internal preview in the monetization preprocessor only; the existing `RequiresReleaseChecks` function and normal production path remain unchanged. The legal guard still runs. The scope rejects production/live monetization, live ads and connected accounts.

The internal-only Gradle hook removes the native Mobile Ads auto-init provider and advertising identifier/ad-services permissions, and explicitly declares normal Play Billing permission. All such edits are confined to generated Gradle manifests while the explicit build scope is active. The QA copy uses the already published, owner-approved legal text and URLs; main runtime settings remain untouched. A new upload keystore is created in a protected folder outside the repository for this empty app if required, with credentials never placed in source or command arguments. Artifact validation must verify signing, exact package/version, non-debuggable manifest, ARM64, target API, billing permission and absence of ad identifier permissions before upload.

A test release is to be created only on `internal`, in draft state, with the requested existing tester list selected if exposed by the available Console/API surface. No production track, screenshots, new invites, or public release are authorized.

## Verified replacement-key access and metadata

The owner supplied a replacement JSON file and explicitly authorized the root-disk copy. Mode 0600 and exact byte identity were verified; the Android Publisher OAuth flow and the app APIs now succeed. The earlier invalid-key block is resolved. The credential remains outside Git and outside the player.

EN/RU/UK listings and public email/support contact were committed and read back. A remaining Ukrainian Apple-specific cancellation phrase was corrected to Google Play and committed in a second metadata-only edit. All four tracks were empty before each commit; no screenshots, binary, production release or tester invitations were part of these commits. Google's API rejected `changesNotSentForReview=true` for this app; the metadata-only commit used `changesInReviewBehavior=ERROR_IF_IN_REVIEW` without that unsupported parameter. No claim is made that Google skips its own metadata review.

Three subscriptions (`cascade.no_ads_week`, `cascade.no_ads_month`, `cascade.no_ads_year`) and their `week`/`month`/`year` plans were created as DRAFT, with EN/RU/UK metadata, USD 1.49/3.99/19.99 baseline and Google's conversions for 173 returned regions excluding VN (CN was not returned). None was activated. Six consumables were prepared, but creation was rejected with the provider message to request billing permission. First-bundle upload and readback are still required before treating them as created.

Most App content questionnaires, IARC and email tester-list counts are not exposed by the available API. The policy draft and console-remaining.md distinguish prepared answers from saved forms; no unverified Families or Data Safety claim was submitted.

## Final local Android build and owner stop

The signed non-debuggable internal AAB was built successfully with Unity 6000.3.8f1: 0 errors, 16 warnings. Android SDK tools needed a temporary path without spaces and an isolated Build Tools 35 installation. Main runtime assets, production flags and existing development tools were preserved. The build helper restores previous PlayerSettings and Android tool paths in `finally`.

The retained AAB is `/Volumes/MY DISK/FlyingLadders/Builds/Android/Internal-20261019/FlyingLadders-1.0-internal.aab`, 370,633,945 bytes. Bundletool validation and JAR signature verification passed. Exact package `com.vickand.flyingladders`, version 1.0/code 1, API 36/min 25, ARM64 only; Billing present; AD_ID, AdServices permissions and Mobile Ads auto-init provider absent. All six native libraries passed ELF LOAD alignment checks for 16 KiB pages. The signing fingerprint matches the dedicated protected upload key; no private signing material is in Git. Physical Android execution and the complete gameplay suite were not run.

After the owner explicitly said not to upload the build, the running upload process was interrupted immediately (exit 130). The existing edit was read without creating another edit: its bundle list was empty and all production/beta/alpha/internal tracks had zero releases. That unfinished edit was then deleted successfully. No accepted bundle or test release remains. Do not resume or repeat any binary upload without a new user instruction. The six coin products therefore remain blocked; the metadata and three independent subscription drafts remain saved.

The current saved name is Flying Ladders in all three Google locales. The owner asked about Fitting Ladders; a clarification is pending before renaming store branding. This does not authorize changing bundle/package/product identifiers.


## Update: accepted catalog bootstrap and active products — 21 October 2026

A new explicit owner instruction authorised one AAB upload for billing setup. The previously recorded upload prohibition no longer applies to this completed catalog-bootstrap operation. The exact validated artifact (version 1.0/code 1) was accepted, and an `internal` release was committed with status `draft`. Production, beta and alpha remain empty; no testers or screenshots were added.

After acceptance, the six existing consumable IDs were created and their `buy` purchase options activated. Each has EN/RU/UK metadata and Google-converted availability in 173 regions. All six read back as ACTIVE. The three no-ads base plans also read back as ACTIVE in 173 regions.

This is catalog configuration, not proof of a completed transaction. The bootstrap binary deliberately uses the isolated internal-preview service state. Real billing launch, purchase token acknowledgement/consumption, server verification and restore must be tested with the later live-services build and a licensed tester account/device. Current evidence is in `Reports/GooglePlayBootstrapOct21/`.


## Update: custom closed-testing track — 21 October 2026

At the owner’s request, version code 1 was moved from the built-in `internal` track into a new custom closed-testing track named `internal-closed-testing`. The release remains `draft`. The built-in internal, alpha, beta and production tracks now contain no release. The closed track has no tester assignment or invitation; the owner will add testers later in Play Console. Current readback is stored in `Reports/GooglePlayBootstrapOct21/closed-final-readback.json`.
