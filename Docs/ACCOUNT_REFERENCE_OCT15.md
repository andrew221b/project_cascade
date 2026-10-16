# Account welcome and Home reference update — 15 October 2026

The three supplied F830DE8D reference JPGs define the welcome and Home compositions. Keep actual Apple/Google/guest actions, a truthful account-dependent sign-in/sign-out shortcut, live localized labels, the first-Play comic/level/map route, original button feedback and thematic environment. Reuse existing frame, platform, logo and art where appropriate; generate missing cutouts/control faces only through built-in imagegen. Verify native phone proportions, safe insets, touch targets, text, and upper-panel alignment in gameplay/Pause/trophies against existing references. No player build/install or external deployment.

## Account behavior and compatibility note

Keep the installed Unity Authentication 3.6.1 / Unity Player Accounts browser flow. Its supported `StartSignInAsync(bool isSigningUp=false)` API does not accept a Google/Apple provider hint. Both branded buttons initiate the existing secure hosted flow, with provider-specific waiting guidance; do not invent OAuth URL parameters, embed secrets or replace the configured identity system. Document the hosted provider selection clearly. Empty public Client ID and disabled production flags stay inactive until the operator configures the service.

Add a durable device-only signed-out state to the existing cloud journal as an optional boolean defaulting to false. Signing out never unlinks/deletes the identity, clears progress, merges wallets or silently creates another cloud identity. Save local work before sign-out, reject sign-out while an account/purchase request is active, pause cloud synchronization and verified purchases while explicitly signed out, and require an explicit successful account flow before resuming them. Keep recoverable guest/account journals and pending requests. Test cancellation, failed durable writes, stale callbacks, restart, conflict choice and no replayed grants. Old journals retain their current signed-in behavior.

Keep account-conflict choices reachable on the welcome screen; they previously existed only in a Settings page removed by the settings-reference update. Never automatically load another account or treat a cancelled/failed browser sign-in as successful. Preserve first-entry/guest behavior and expose a way back when this screen is opened from Home.

The committed account, rather than a browser identity alone, owns the Home state. Keep the welcome/conflict screen visible while an account load or journal commit is pending. A failed/empty replacement keeps recovery controls accessible. The SDK's temporary candidate identity must not become the Home signed-in state before its local profile and journal have committed.

## Validation contract

Capture current native welcome, guest Home, authenticated Home and relevant header states at reference and phone resolutions, plus safe insets and EN/RU/UK. Check actual raycasts, buttons, state-dependent icons, persistence and unchanged gameplay/wallet. Run actual CloudSyncClient transport tests, relevant Unity PlayMode cases and current Console checks. Configured provider login/backend operations require real operator configuration; no local fixture may be reported as a production OAuth test.

## Implemented visual contract

- Reused the established wood frame/foreground, stairs, logo, fonts, Home navigation cards and feedback. Generated separate waving Mika, smiling lantern Mika, blank welcome/provider/guest surfaces, account/provider icons, landscape and reference umbrella flying boat. Text remains live and localized. Source artwork is unchanged; runtime Sprite regions select authored silhouettes.
- The original reference geometry uses uniform scaling. A compact 620-unit content window on tall/narrow phones keeps the controls and Mika legible; reference-size and tablet content remain proportional. Physical framing is independent. The landscape covers the physical canvas with uniform cropping. One continuous original stair/floor texture reaches both viewport sides and bottom; its sampled horizontal and vertical scales are equal. This removes the tablet's exposed rectangular background bands without stretching wood grain.
- Welcome sign ropes continue into the physical upper beam through safe insets. Mika's boots meet the same upper stair used in Home. The reference umbrella aircraft drifts gently behind the interface, follows world lighting, preserves aspect and cannot intercept touches or change the puzzle.
- Home shows Sign in for a guest and Sign out for a committed linked account. Its account and no-ads controls share one vertical centre. New touch bounds remain at least 44 screen pixels without overlapping other controls.
- Fixed the gameplay heading's horizontal shift when the extra help control is hidden: the lamp occupies the actual right edge and the plaque centres between visible controls. Existing Pause title/corner geometry and trophy header/preview routes were checked against the supplied references; unrelated layouts were preserved.

## Final saved assets and prompts

Tool: **built-in imagegen**, no fallback CLI. Final PNGs in `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/AccountOct15/`:

| Saved file | Size | Purpose |
| --- | --- | --- |
| `WelcomeMika.png` | 1024×1536 RGBA | Full waving pose, no lantern, real transparency |
| `HomeMika.png` | 1024×1536 RGBA | Full smiling lantern pose, real transparency |
| `Controls.png` | 1159×1358 RGBA | Blank header/green/gold faces; sign-in/out, Apple, Google and guest icons |
| `Scene.png` | 942×1670 RGB | Reference castle landscape without foreground/UI/character |
| `Airship.png` | 1374×1145 RGBA | Separate umbrella-sail wooden flying boat |

Final generation/edit prompt specifications (compact record of the prompt set):

1. **WelcomeMika / background-extraction and identity-preserve:** Reference Photo 1; only the full-body smiling orange mouse-squirrel, right hand waving and left holding the satchel strap. Preserve reference anatomy, ears, eyes, tail, scarf, cream shirt, patched green overalls, brown bag and boots. All fingers/feet/tail visible. No lantern, sky, wood, platform, shadow, UI or text; genuine alpha including silhouette gaps; same warm polished storybook art.
2. **HomeMika / background-extraction and identity-preserve:** Reference Photo 2; only the full standing friendly open-mouth smile, same clothes/identity/pose, bag and whole glowing lantern/handle in the right hand. Entire silhouette/whiskers/boots visible with transparent margins; no sky, frame, floor, UI or text.
3. **Controls / transparent two-column four-row atlas:** Match all three references. Row 1: blank floral wooden welcome header without rope shafts, blank green enamel provider face with wood rim and end daisies. Row 2: blank amber guest face, cream/brown sign-in door/arrow square. Row 3: matching sign-out door/arrow square, white Apple icon. Row 4: multicolour Google G/white disc, dark-brown guest bust. No labels, numbers, grid lines, joined shadows or background. Keep true transparent alpha and reference materials.
4. **Scene / precise-object-edit:** Reference Photo 1 composition, Photo 2 style. Preserve bright soft sky, clouds, hazy mountains and castle islands at their original edge-weighted positions. Remove all foreground posts/frame/ropes/lantern/floor/stairs, character, welcome sign, logo, buttons and aircraft; reconstruct only the landscape. Portrait 9:16, no UI/text, no new central islands and no extra contrast/sharpening.
5. **Airship / background-extraction and identity-preserve:** Reference Photo 1 right-hand sky object. Only the complete cream umbrella canopy, mast, delicate suspension ropes and tiny warm wooden boat/basket, preserving orientation/proportion and reference sunlight/art. Real transparent alpha, generous margin, no sky/UI/character/frame/ground. Use the reference umbrella-sail boat, not a zeppelin or a closed round balloon envelope.

Original generated paths, exact saved files, bounds and SHA-256 values are in `Docs/ACCOUNT_ASSET_PROVENANCE_OCT15.json`. All selected PNGs match their generated originals byte for byte; only read-only QA montages were resized. Texture import disables mipmaps and NPOT resizing; no packages or store IDs changed.

## Verification completed

- **21/21 native Unity 6000.3.8f1 PlayMode scenarios passed**, current final source, 15 October 2026 06:52:44–06:54:10 UTC. Includes four new account cases plus first-Play comic/level/map, settings, Pause and provided-reference integration cases. `Reports/AccountReferenceOct15/playmode.xml`.
- **109 native screenshots**: 38 account/Home/header/Pause/trophy states at 720×1280, 390×844, 320×693 and 768×1024, EN/RU/UK, large text and safe insets; 25 fresh settings captures; 46 supplied-reference/world-state captures. All current `_Cascade` C#, shaders and assembly definitions matched the tested isolated copy.
- Verified guest choice and actual profile reload, recoverable failed save, missing-provider notices and dismissal, Home login/back, both Home icon states, rendered text and aspect ratios, nonoverlapping 44px touch targets and real EventSystem hits, visible-header centring, continuous viewport ground and equal UV scale. Aircraft motion changes neither puzzle hash nor input hit testing.
- The linked Home images are explicitly **visual fixtures**, not live authenticated sessions. Actual client authority was tested through the production `CloudSyncClient` compiled against controllable SDK boundaries: **65 checks passed**, including durable sign-out, pending journal/wallet preservation, restart, late/cancelled browser callbacks, identity/purchase races and replacement-account commit timing. Existing server endpoints passed **98 boundary checks** on fake SDK transport. No live Cloud Save assertion.
- Native review found small Home account touch bounds, exposed tablet bands and too much empty space above the composition; these were corrected and recaptured. The previous Pause Retry fixture used a wide batch Game view, which activated the landscape suspension gate. It now holds a real portrait RenderTexture and verifies the actual retry/save behavior. Intermediate failures and compile diagnostics are retained under `BeforePolish`, `BeforeViewportPolish` and `BeforeGroundCover`, distinct from final evidence.
- Repeated current mobile captures in the primary Editor through official Unity MCP, using a temporary profile. Final primary runtime Console returned zero errors/warnings. No scene was saved/discarded. Native PlayMode testing also used a separate temporary project/profile.

Final evidence and source/asset hashes: `Reports/AccountReferenceOct15/validation.json`. Main previews: `mobile-screens.png`, `tablet-screens.png`, `reference-comparison.png`, `locales.png`, `reference-audit-1.png`, `reference-audit-2.png`, `settings-recheck.png`. Primary Editor evidence: `Reports/AccountReferenceMcpOct15/`.

Operator instructions are in `Docs/ACCOUNT_BACKEND_SETUP_OCT15_RU.md`. Client ID and production flags remain unfilled/inactive by design. Physical-device build/install, real provider authorization, cross-device transfer, production deployment, live store transactions and measured device performance were not run. Exact bitmap identity is not claimed for newly generated art. The existing separate account/data-deletion release task remains open.
