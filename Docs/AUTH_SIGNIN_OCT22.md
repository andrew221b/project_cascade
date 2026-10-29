# Sign-in investigation — 22 October 2026

## Current status
Build 202610229 is signed and installed on the owner’s iPhone; app launch confirmed, all four Documents files preserved byte-for-byte. The earlier phone identity is verified as apple-game-center and its journal acknowledged cloud revision 6 before the former home shortcut signed it out. The new build fixes provisional Game Center callbacks, startup auth concurrency, redundant native links, account status versus explicit sign-out, control dimensions and dialog alignment. Final post-correction phone login is confirmed by the owner. Fresh device journal acknowledged cloud revision 8 at 19:36:32 UTC; signedOut=false and no pending account replacement. Unity admin readback HTTP 200 confirms apple-game-center on the same original player. Fresh console has no account errors. Clean-device restore and Android-device login remain untested.

Play Games app 648113837751, dedicated Android Play Signing OAuth and dedicated Web server OAuth are configured; Unity provider and official SDK public settings are saved. Project/credentials remain drafts and owner account is a tester. No app binary upload, store review or public release. Ads remain disabled until the separate real SSV/CMP blockers are resolved.

Validation: 110 controlled actual-client boundary checks; 26 final Android Unity EditMode checks; native account UI PlayMode test across EN/RU/UK at 320/390 pixels. Real Game Center linking/save acknowledgement was independently confirmed before the UX correction. Details below retain the investigation history.

## Design note
Keep the existing Unity Player Accounts client, configured provider, redirect URI and guest identity. Do not alter account ownership or replace saves. Diagnose the game-side unavailable message separately from the expected hosted Unity screen. A background sync must finish before linking, but should not reject an explicit sign-in click. Queue one cancellable request while that sync finishes; retain timeout and generation guards. Log only operation, exception type and numeric SDK code, never tokens, URLs, exception messages or player identifiers.

Baseline: installed iOS build 202610222; provider enabled, client ID and mobile/PC platforms match. A fresh hosted authorization reaches Google successfully without credentials. Device journal shows guest cloud revision 1 saved at 17:57:16 UTC. Full Google/Apple OAuth is not yet verified.

## User-approved native game identity
Use Game Center on iOS and Google Play Games on Android, linked to the existing UGS identity. Keep purchase verification in the stores/backend. Retain legacy Unity identities for existing players; never force-link or merge two wallets. Use the existing account-choice/profile rollback flow. Game Center and Play Games alone do not provide cross-platform identity. Preserve existing under-16 privacy policy until separately changed.

Dependencies: official Apple.Core 3.3.0 and Apple.GameKit 4.1.0 (built from Apple upstream, iOS/macOS Release); Google Play Games 2.3.0 (official Unity package, existing External Dependency Manager 1.2.188 retained). Source commits: Apple 96e4a044572450e9e02757900d2b1d2a390016de, Google 068d8a2227e9588d4669ea4deeb3288e9ac46ce2.

Native Apple plugin requires iOS 15.6 minimum. The Apple build profile and player target are set through Editor prebuild APIs. Android builds fail explicitly until this game's Play Games app ID and server Web Client ID are configured. No OAuth client secret is stored in Unity assets.

Verification so far: 92 actual-client checks on SDK transport doubles; 26 native Unity EditMode checks passed with installed Apple/Google SDK assets. Apple API enabled GAME_CENTER capability, Game Center app detail and the existing editable iOS 1.0 version (no submission). Live Game Center/Play Games linking is still unverified; Unity provider setup and Play Console credentials remain pending.

## Current verified services
Apple App Store Connect confirmed GAME_CENTER capability, app detail and version 1.0 enablement. Unity Authentication Game Center provider is now configured for the exact bundle ID. The setup helper uses the installed Editor SDK with an explicit target project: global CloudProjectSettings can point at a different open Editor; no mutations were made to that other project.

The code has 93 client boundary checks and 26 iOS-target Unity checks passing. Android compilation is being checked separately. Google Play Games requires UnityWebRequestTexture 1.0.0; installed with UPM without changing Google's source.

Still needed for Android: Play Games game/application ID, Android OAuth credential using the Play App Signing SHA-1 (and a separate allowed local signing fingerprint for device development), server Web Client ID and its secret in the Unity Authentication provider only. These are Play Games credentials, not the purchases service-account JSON or AdMob IDs. They have not been invented or reused from another game.

The Mac was temporarily locked during setup; it was subsequently unlocked and Play Games configuration completed. Native Game Center sign-in on the user's phone has not yet been confirmed.

## Native account flow corrections after device testing
The user reached Apple's system Game Center login. The official wrapper resolves its first task when it presents the system controller, before IsAuthenticated is true. Wait for AuthenticateUpdate with an authenticated player or an actual AuthenticateError instead of reporting failure while the system UI is open. Retain cancellation/retry guidance, no token/profile logging, no force linking. A pure signal regression verifies that a provisional unauthenticated callback cannot complete sign-in.

Startup store purchase-identity preparation and background sync could concurrently invoke anonymous sign-in (device SDK error 10000). The actual-client regression failed before the identityPreparations guard; sync now waits for the store identity preparation. Native linking blocks duplicate clicks until complete. Home account control opens cloud status with a separate explicit sign-out action. Dialogs are centred on the real stage, provider controls raised/enlarged, existing skins and 4-pixel icon gaps retained.

Google Play Games project now created in tracker-project-497108 for Flying Ladders (verified app ID 648113837751). Dedicated Android Play Signing OAuth and dedicated server Web OAuth created; server secret stored only under Credentials/Google and configured in Unity Authentication via its installed Editor SDK. Public SDK settings/manifest generated through official PerformSetup. Testers page includes shapeinc25@gmail.com; release-track addition is disabled while no distributed track exists. Game project/credentials remain drafts; no review, binary upload or public publication. The server Web client consent configuration currently limits access to test users.

Current validation: 109 actual-client boundary checks pass (SDK doubles, not a live login). Mobile account UI PlayMode check passes for EN/RU/UK at 320 and 390 pixels. Earlier native iOS/Android targeted EditMode checks passed 26 each. Build 202610227 installed preserving four Documents files byte-for-byte; subsequent user feedback prompted build 202610228. New full login and server-save readback still required after installation.

Device readback before build 228: Unity admin user lookup HTTP 200, external identity type apple-game-center; durable journal revision 6 and last acknowledged sync 2026-10-22T19:09:20Z, then explicit signed-out marker. This proves native linking and an acknowledged cloud save on the owner's phone. A fresh-device restore, Android sign-in and restored purchases still require separate validation.

Final repeat-login correction: after an explicit game sign-out, fresh Game Center/Play Games identity proof is compared exactly to the identity already linked to the restored authenticated UGS session. Matching identities resume the same journal without a redundant link request; different identities still require explicit account choice and never force-link. The regression failed before correction; 110 actual-client checks now pass. Native account UI fixture is reset between languages so clean entry captures do not inherit a previous simulated error. Final device build is 202610229.

Final owner reply: «Вроде все ок». The corrected native sign-in and cloud status flow on build 202610229 has both owner confirmation and device/server evidence. No app review, binary upload or public release was performed.
