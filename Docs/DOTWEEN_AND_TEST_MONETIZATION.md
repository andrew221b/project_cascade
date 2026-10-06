# DOTween and test monetization — 6 October 2026

Design note before dependency, ad policy and configuration changes requested by
the author. Install the unmodified free official DOTween 1.3.030 distribution,
including its licence/readme and original metadata. Use its supported setup for
UI modules. Optional Physics2D is disabled in settings/build symbols because
this project has no Physics2D package. The vendor-generated DOTween.Modules
assembly is referenced by Presentation. No other package upgrades or production IDs.

Motion thesis: the workshop folio settles into the next screen, enameled controls
compress and release, and earned stars/score arrive in a finite celebration.
First-encounter cards open softly. Gameplay physics remain separately authored;
tween completion never changes a row, navigation, currency, inventory or wins.
Rapid navigation cancels prior tweens; disable/focus/ad interruption and Reduced
Motion must settle or pause safely. No extra bloom layers or constant UI loops.

Test monetization is enabled explicitly in settings. Prices are hidden; the
existing coin prices remain unchanged underneath the ordinary production shop.
Test item grants and no-ad entitlement use a session-only copy of the ledger,
with existing stock and per-stage tool limits. Actual earned progress, attempt
and first-encounter metadata continue to save; fake purchases, coins and cosmetics
never become real store ownership. No external receipt validation is fabricated.

Rewarded is an explicit shop offer of 40 coins with no forced gameplay show.
Interstitial is only after the complete level/statistics/parcel flow, after at
least three full level clears, at least three new wins since a previous ad and
180 foreground seconds. Maximum two automatic shows per session; skip finales,
replays, unavailable inventory, and no-automatic-ads ownership. No pending ad debt.
This replaces the original first-session/first-12 protection; the author now asks
for rare visible ads after several levels. Ad callbacks grant inventory only.

Unity Editor uses actual LevelPlay SDK mock units with explicitly labelled
Editor-only identifiers. A local marked preview is used on a test device without
publisher keys. Native release initialization remains gated on registered keys
and the existing consent/age bridge. Store tests require no money or price data;
real App Store/Google Play checkout retains its original verification path.

Sources: https://dotween.demigiant.com/download.php and documentation.php;
the installed LevelPlay/IAP APIs and official Unity skill references. Validation
and remaining limits are recorded in Reports/MotionAds/README.md.

Implemented: 300 ms directional arrival with immediate readability; 90 ms press/
260 ms release of button artwork with stable hit areas; 200–320 ms encounter
card entrance; a 3.2-second star/score/confetti ceremony; finite coin notification.
Owned tweens handle interruption, focus and Reduced Motion. Existing springs,
ropes, falling pieces, backdrop flight and effect detachment remain separately
simulated. The video offer retains a 44-pixel hit area on a resized small screen.
Tools/keepsakes/no-ad entitlement can be tried without prices or spending; stock
cap five and per-stage usage limits still apply. Kept test cosmetics equip only
in memory. Tests used isolated profiles; no phone build/install.
