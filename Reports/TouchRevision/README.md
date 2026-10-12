# Touch, physical landing and store preparation — 6 October 2026

Scope: direct touch, original-piece exchange, pooled physical landings and device
shake feedback; extend the existing shop with an optional native no-ad product.
No gameplay rule, campaign content, save schema or published price changed.

## Checks actually run

- `editmode.xml`: 13/13 policy/ledger tests PASS, including unverified/wrong-product
  rejection, save-before-grant and duplicate native restore.
- `playmode.xml`: first batch 9/10 PASS. The sensor's first real value was incorrectly
  treated as a shake after a zero initialization sample. Fixed by waiting for a real
  gravity sample; failed evidence retained.
- `../TouchRevisionSensor/playmode.xml`: calibration fixed; angle assertion exposed
  premature ground settling while the pieces were still ascending. Failed evidence
  retained; no assertion was weakened.
- `../TouchRevisionFinal/playmode.xml`: 4/4 PASS after the fixes: sensor/shake and
  Reduced Motion, physical fall/pause/seat restoration, drag pose handoff, unavailable
  store and combined ad/focus suspension. Together the runs cover 10 distinct passing
  gesture/physics/store cases. This is not a fresh full-project test suite.
- Unity MCP reported no console errors after the final native captures.

## Native visual evidence

Ten retained actual Unity Editor renders, all opened and inspected. Gesture images
are 390×844: `tap-selected.png`, `tap-exchange.png`, `drag-held.png`,
`drag-exchange.png`, `landing-contact.png`, `shake.png`, `letter-held.png`.
`shop-small-ru.png` is320×693; `purchase-ru.png` is390×844; `purchase-small-ru.png` is320×693. Synthetic safe insets
(.018,.04,.018,.066), isolated QA save, no modified real player save.
The first gesture/fall batch used a synthetic two-floor C01 presentation fixture,
not newly published campaign content. Final shake confirmation uses actual C03.
Camera output was held at a fixed size during motion; no mid-motion layout refresh
was used to make a pose look correct. Corrected shake/purchase captures replace their
first-round images. A bad early selection capture was discarded, not used as proof.

The original mesh and embossed glyph move together. Selection/destination use a
piece contour, never a square slot image. Drag follows actual pointer coordinates;
release gives the partner an upward return arc. Falling bodies tumble/bounce/collide
in the isolated scene, then guide back to the committed row's seats. At most eight
reused collision bodies, no shake-frame array allocation, fixed90Hz bounded stepping.
Box proxies approximate shape contact; normal wind and exchanges stay scripted.
The existing rope point-chain and board springs receive the shake impulse.

## External activation still required

Official IAP5.4.4 and LevelPlay9.5.1 packages and required resolver were installed
through Unity. The draft product is `cascade.no_automatic_ads` (NonConsumable).
The user has no registered products/app/ad units. Release flags are false, keys are
empty, store initialization is gated, prices are not invented. The prepared purchase
manager verifies against a publisher HTTPS receipt server before saving entitlement
and confirming a pending order. That server is not implemented/hosted here.
A real privacy/age/ATT bridge must precede ad initialization. See
[activation contract](../../Docs/TOUCH_AND_STORES.md).

NOT RUN: real iPhone/Android sensor or haptic feel, native dependency resolution in a
build, real store purchase/restore/refund, verified production receipts, ads/consent,
OS accessibility, device FPS/thermal profiling or human enjoyment. No phone build
or installation was performed for this task. Static screenshots do not prove motion
timing or gameplay fun.

## Independent finish review

Fresh Impeccable reviewer inspected all nine initial retained captures and relevant
implementation. Disposition FIX: F01 return-to-play target45.27px at390, and F02
Reduced Motion erased intentional vertical drag. F01 now uses the thematic footer
with a52px minimum; its390/320 recaptures measure actual bounds. F02 separates
decorative lift from direct x/y tracking; the expanded pointer test covers vertical
and diagonal travel in both modes. Final verdict: SHIP in a verdict pass limited to F01 and F02; both resolved. This is not whole-product certification.

F01 bounds:390×844 footer64.14px at y42.81;320×693 footer52.66px at y35.15,
inside the simulated safe content. F02 final report:
`../TouchRevisionReviewFinal/playmode.xml`,1/1PASS with normal/quiet vertical and
diagonal drag. `../TouchRevisionReview/playmode.xml` failed in SetUp/TearDown on
Unity Device Simulator DevicePackage.HandleList, with no test assertion run. Closed
that optional Editor window and repeated the same check in Game View; no vendor
code or package change was made for the workaround.
