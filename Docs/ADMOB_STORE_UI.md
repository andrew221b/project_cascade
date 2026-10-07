# Coin shop UI — 7 October 2026

## Scope and authority

This is an Operate extension of the incumbent Sky Wind Workshop shop. It adds the Coins tab and two optional coin offers within the existing painted world. It records the implemented surface after the scoped `ship` review in [Reports/AdMobStore/impeccable-review.md](../Reports/AdMobStore/impeccable-review.md). It establishes no new visual world, approved comp or design-token source.

[PRODUCT.md](../PRODUCT.md) supplies the product facts: 240/480 coin packs, equal value without a bulk bonus, a displayed-wallet purchase threshold of 960, optional rewarded videos granting 40 coins, and unchanged tool/assistance consequences. [DESIGN.md](../DESIGN.md) remains the visual authority; this document supplements its shop implementation guidance. Publisher activation and release work are recorded separately in [ADMOB_AND_STORE_RELEASE.md](ADMOB_AND_STORE_RELEASE.md).

## Inherited assets and materials

| Asset or component | Concrete use |
| --- | --- |
| `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Craft/WorkshopPortraitQuiet.png` | Portrait workshop surround: honey timber, hemp knots, lanterns, hanging cloth and a clear blue-sky reading field. `LayeredWorld` falls back to the preserved `WorkshopPortrait.png` if the quiet asset is absent. |
| `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Craft/WorkshopRoomQuiet.png` | Wide workshop surround; `LayeredWorld` retains `WorkshopRoom.png` as fallback. |
| `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/TravelTools.png` | Existing painted gold-coin pouch, selected by `TravelArt(3)`, appears beside the wallet, in both offers, in preview ads and in the coin acknowledgement. The authored atlas split is retained. |
| `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Reactions/EnamelControls.png` | Sliced brass-framed teal enamel for offers, idle tabs, explanatory/status surface and Back; warm gold enamel for selected Coins and purchase actions. `CascadeGame.ControlArt.cs` owns the inherited control sprites. |
| `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/Fonts/Title.asset` and `Body.asset` | Existing Balsamiq Sans TMP lettering. Quantities and wallet use the title face; supporting copy uses the incumbent text helpers. |
| `MenuTransition.cs`, `CascadeGame.MenuLayout.cs`, `CascadeGame.Flow.cs` | Shared entrance, safe-area menu layout, physical control sizing and contrast treatments. |

Headings and wallet figures use dark teal ink against the clear sky. Teal controls carry ivory lettering; gold controls carry dark ink. Offer surfaces remain authored enamel. The surface does not add a generic cream content sheet, substitute illustrations or a new palette. Disabled controls inherit the existing muted enamel treatment.

## Layout and navigation

The reading order is heading and Back chevron → live pouch/wallet → Tools/Trophies/Coins tabs → 240 offer → 480 offer → assistance and store-status note → Back to shop. Tools and Trophies return to the existing catalogue; Coins stays selected in gold. Both the heading chevron and bottom action return to Shop.

`CascadeGame.CoinPacks.cs` places the two offer rows at horizontal anchors .085–.915, with top anchors .699 and .494 and a row height of .180 of the menu root. Each row places the pouch on the left and its quantity, use description and action on the right. The lower note occupies .095–.905 horizontally and .121–.301 vertically. The bottom action uses the shared 52-pixel menu band. These are observed layout constants, not new design tokens.

The shared root stays inside the safe area and keeps a bounded central column in the wide view. `CounterButton` registers each tab with the existing 48-pixel row sizing; `MenuUnit` compensates when the Canvas scales down. Actions keep single-line captions with automatic sizing. Supporting copy may wrap. The compact Russian fixture exercises Large Text and simulated insets while retaining both offers, note and return action within the first viewport.

The ordinary shop retains its actual illustrated objects on two shelves, selected-object explanation, tool stock, wallet and optional `Video · +40` offer. Its Tools/Trophies tabs now share the third Coins destination.

## Copy and states

| State | Implemented visible behavior | Evidence limit |
| --- | --- | --- |
| Test shop | Offer actions say `Try +240` / `Try +480`, with localized RU/UK equivalents. The note explicitly says `Test shop · no charge`. The ordinary catalogue heading says `Test shop`. | All six captures are test-mode Editor output. |
| Available production product | Action uses `Buy · {localized store price}` from Unity IAP metadata. No cash price is authored in this UI. | Source inspected; live store price and purchase flow NOT RUN. |
| Wallet at or above 960 | New pack actions read `You have enough coins` and become unavailable. This threshold is not a balance cap: already-paid pending orders still deliver. | Source/product fact; this state is not one of the six captures. |
| Busy, deferred or retry | Status can say `Checking purchase…`, explain waiting for approval while play remains available, or explain failure/retry. Retry reconnects through the store adapter. | Source inspected; production state transitions NOT RUN in this documentation pass. |
| Unconfigured/unavailable | Status says `The shop is not open yet`; an unavailable offer says `Coming soon`. Editor production purchases remain unavailable. | Source inspected; current screenshots use preview purchases. |
| Coin grant | Wallet refreshes, then a localized pouch toast reports the actual added amount. The supplied Russian capture shows 240 in the wallet and `Получено 240 монет`. | Static state inspected; timing and perceived motion NOT RUN. |

The coin note states: `Up to two tools per stage. Help limits this run to two stars.` The ordinary shop additionally states `Tools: up to 2 stars. No clean record.` Buying currency alone does not apply assistance. Existing five-tool prices, eight 240-coin trophies, limited stock and eligibility rules remain product/application facts rather than presentation decisions.

The source-defined preview ad uses the same pouch and teal/gold controls inside a full-viewport dimmed backdrop and safe-area sheet. It identifies itself as `Test ad`, offers a Close action, and enables collection after five foreground seconds for a rewarded preview (two seconds for an automatic preview). Its countdown stops while unfocused/paused. No preview-ad screenshot is included in this six-capture evidence set; this paragraph records source behavior only.

## Motion and acknowledgement

The inherited `MenuTransition` begins with readable opacity .82, a bounded horizontal offset of at most 24 menu units and a .30-second OutQuint DOTween arrival. Navigation does not wait for this animation. Reduced Motion settles immediately; the shared animation allowance controls pause/resume, and disabling the component settles its visual pose.

`CascadeGame.PreviewAds.cs` implements a finite, input-transparent coin toast. It reuses the pouch, expands from scale .94 to 1 over .30 seconds with OutBack, holds for 1.7 seconds, then fades over .30 seconds. Reduced Motion omits the scale travel. Tween completion destroys the presentation object only; it does not grant currency or change puzzle state. `CascadeGame.Store.cs` refreshes the menu from store changes and shows the toast after the grant notification. Pouch sway uses the existing `WorldMotion` component.

These timings were read in source. Still screenshots establish the rendered acknowledgement, not its perceived rhythm, interruptibility on physical devices or human enjoyment.

## Exact evidence and validation limits

All six PNGs below were opened during this documentation pass. Their matching JSON dimensions and button measurements were inspected. The capture producer is `CascadeUnity/Assets/_Cascade/Editor/Playable/CoinShopReview.cs`; it uses `PlayableCapture.CaptureSettled`, a local preview-ad override, EN/RU/UK locale changes and a simulated safe-inset vector (.018, .04, .018, .066). It resets insets to zero for wide output. These are Unity Editor camera renders, not physical-device captures.

| Capture and measurement file | Fixture/state | Buttons | Minimum measured height |
| --- | --- | ---: | ---: |
| [coins-en.png](../Reports/AdMobStore/coins-en.png), [JSON](../Reports/AdMobStore/coins-en.json) | 390×844, EN, zero wallet | 7 | 48.000061 px |
| [coins-small-ru.png](../Reports/AdMobStore/coins-small-ru.png), [JSON](../Reports/AdMobStore/coins-small-ru.json) | 320×693, RU, Large Text, simulated safe insets | 7 | 47.999969 px |
| [coins-added-ru.png](../Reports/AdMobStore/coins-added-ru.png), [JSON](../Reports/AdMobStore/coins-added-ru.json) | 390×844, RU, +240 acknowledgement and live 240 wallet | 7 | 48.000061 px |
| [coins-uk.png](../Reports/AdMobStore/coins-uk.png), [JSON](../Reports/AdMobStore/coins-uk.json) | 390×844, UK, 240 wallet | 7 | 48.000061 px |
| [coins-wide.png](../Reports/AdMobStore/coins-wide.png), [JSON](../Reports/AdMobStore/coins-wide.json) | 1440×900, UK, zero simulated insets, bounded column | 7 | 51.184875 px |
| [shop-en.png](../Reports/AdMobStore/shop-en.png), [JSON](../Reports/AdMobStore/shop-en.json) | 390×844, EN, catalogue and optional video offer | 12 | 48.000000 px |

The 47 measured button rectangles meet the 48-physical-pixel height target within floating-point tolerance; their widths also exceed 48 pixels. The compact minimum differs from 48 by .000031 pixels. The inspected text remains visible without clipping; all compositions retain their complete content. Every JSON reports `minimumDeviceTextPixels: 0.0`, so these files do not establish a measured text-size floor or text contrast ratio.

[editmode.xml](../Reports/AdMobStore/editmode.xml) records 35/35 selected EditMode checks passed on 7 October 2026 at 02:22:52–02:22:55 UTC. This documentation pass inspected the report and did not rerun the tests. The visual review also did not rerun economy checks.

Physical iOS/Android devices, OS gestures, pt/dp equivalence, accessibility services, SDK ad delivery, consent behavior, native dependency/build execution, sandbox purchases, store metadata, deployed receipt verification and perceived motion were **NOT RUN** here. No native design detector ran. The scoped ship disposition covers this Editor-rendered shop extension and its supplied visible states; it is not a mobile-release, commerce or human-enjoyment certification.

## Preservation and existing drift

This documentation role adds this file only and makes no edits to `DESIGN.md`, `.impeccable/design.json`, existing illustrations/fonts/material tokens, user assets, `.meta` files or unrelated code. The session context had already reported preexisting sidecar design drift and an unset `buildPath`; neither is repaired as a side effect.

During the initial read, the historical integration paragraph at `DESIGN.md:650` stated that LevelPlay 9.5.1 was installed and that the verifier was not implemented. This stale implementation history was explicitly reported against `PRODUCT.md:255` and the new integration. The parent implementation task subsequently corrected that paragraph to Google Mobile Ads Unity 11.5.0 with UMP, LevelPlay removal and the unhosted `Backend/ReceiptVerifier` preparation. This role observed the correction without editing `DESIGN.md`. The visual world and token authority are preserved.
