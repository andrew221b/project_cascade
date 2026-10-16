# Parcel reveal — October 13

User-authorized reward rule: initial 1–2 cards; one additional rewarded video yields 2–3. Mostly variable gold, occasional shop help; rare cosmetic at early parcel 3 and final campaign parcel 180. Owned cosmetics resolve to an available help item, full help stock resolves to 20 gold. Base gold 12–24, video gold budget remains 40; an occasional extra help item does not alter shop prices.

Recipe 6 stores the base plan in optional Loot fields. Fixed, hashed account parcel order produces the same plan across devices/reloads, with no reroll when opening or changing puzzle mode. Recipes 0–5 and already earned stock remain untouched. Bonus eligibility comes only from the existing verified provider receipt. BonusClaimed/BonusLoot are saved atomically with help inventory; video gold itself is already recorded by the ad receipt. Legacy fields omit default additions to preserve cloud baseline hashes. Cloud module includes these shared sources, so its release must be rebuilt alongside the client.

Claim commits before visual pickup. Pausing, navigation and interruption cannot duplicate a reward. Preview advertising stays session-only. Cloud rewarded offers retain the existing SSV readiness guard. My Things first-ad entry hookup is deferred as the user requested; the screen is live on campaign-earned parcels.

Visuals use the supplied 720×1280 composition with uniform scaling, existing wooden environment/header/card/icon/primary assets, new native image-generation transparent closed/open parcel, unwrapped body, platform, subtitle and amber button. Animation uses one unscaled foreground clock; reduced motion removes shake/spiral.

## Validation and delivery

- Unity Editor 6000.3.8f1 on the moved external-drive project; no phone/player build.
- 69 focused EditMode checks pass: new loot transactions, published parcel recipes, wardrobe and shop-help regression cases.
- 17 PlayMode checks pass: eight parcel flow/lifecycle checks plus all nine existing shop/inventory/trophy checks.
- Native isolated visual review checks three locales, large text, 320×693 and 720×1280, 390×844 with simulated safe insets, one/two/three cards, all active hit targets, first claim/Done, cancelled/confirmed second ad, second claim/return, reduced motion and hidden video offers.
- Two animations are sampled from the actual Unity renderer at 60 fps, 393 frames each; puzzle state hash remains unchanged. Videos and native PNGs are in Reports/ParcelReferenceOct13.
- The closed-box composition PNG is an explicitly posed reference-comparison frame; opening/bonus screenshots and videos show the live animation states.

### Existing validation limitations

A broader 96-case sweep (90 pass, six fail) found six unrelated failures in older cloud/reward assertions. The original-source baseline reproduces all six (and two outdated wardrobe test expectations, corrected to current catalog/new minting). Baseline XML is Reports/ParcelLegacyBaselineOct13/editmode.xml; the current broader sweep is Reports/ParcelBroadRegressionOct13/editmode.xml. These are not claimed fixed by this screen task.

Cold NUnit startup emits the SDK's global `Failed to initialize localization, could not preload string tables` log before any parcel opens. The new fixture explicitly expects only this exact known log and separately requires successful CascadeUI table loads for EN/RU/UK; it does not ignore arbitrary errors. Production language code is unchanged. Native visual review uses the normal scene, and all text fitting is checked from rendered TMP meshes.

Cloud player activation remains off (existing enabledForPlayers=0). The Cloud Code module shares the changed Application/Persistence sources and needs rebuilding/deploying with the client before cloud activation; this task did not activate services or bypass the SSV reward guard. First rewarded entry from My Things is deferred, as explicitly requested.

Asset specifications: Docs/AssetPrompts/ParcelReferenceOct13/prompts.md. New artwork is in Content/Resources/CascadeArt/ParcelReferenceOct13; existing item/trophy icons and common green primary button are reused.
