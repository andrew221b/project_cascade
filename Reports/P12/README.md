# P12 — original toy presentation

Implemented six original beveled silhouettes, letter tile, thin proportional
platform, golden receiver, shared URP materials, simple contact shadows, a
painted sky workshop and view-only wind wheel. One orthographic camera and one
key light; no gameplay Rigidbody or Collider. Fonts independently sourced under
SIL OFL. Asset sources and exact background prompt are in Docs/P12_*.

## Actual verification

- EditMode: **305/305**, completed 2026-10-05 04:57:15 UTC.
- PlayMode after the final review fixes: **14/14**, completed 05:06:03 UTC.
  Includes all 40 primary levels via real Input System mouse/button routing,
  touch cancellation/second pointer/Undo/Hint/precision/bridge behavior, ambient
  freeze under Reduced Motion and pause, unchanged state hash, and target zoom.
- Ten settled runtime camera captures through official Unity MCP: 390×844,
  320×693, 1440×900; shapes, words, Mixed8, selected shape/letter, zoom and
  Reduced Motion. All were opened and inspected. These are Editor captures.
- Per-capture JSON records screen-space control dimensions and device text.
  Gameplay control bounds are at least 48px (float tolerance) and do not
  overlap; device text is at least 14px. `layout_audit.json` records results.
- Fresh independent Impeccable review: first disposition **fix** (glyph/body
  alignment, small hit regions, device label size, Unicode precision arrows).
  One batch applied. Verdict pass scored **all four resolved**, disposition
  **ship for those P12 fixes**; it is not overall release acceptance.
- Original shipped raster metadata scan: two rasters, zero missing provenance.
- Latest compile successful, fresh Console empty. The capture helper initially
  lacked a direct Content assembly reference; it was fixed before final runs.

## Capture corrections

Immediate rendering during a camera resolution switch produced invalid old
layout frames. Captures now wait for settled Canvas frames and force the full
layout before rendering. Background rendering is enabled only for the Editor
capture sequence; input is temporarily unsuspended for the screenshot and then
restored. Capturing asserts that the gameplay state hash did not change.
The invalid experimental PNGs were removed, not presented as final evidence.

## Honest limits

No physical phone, mobile build, device safe-area/touch-comfort test, GPU profile,
external human playtest or full EN/RU/UK glyph QA was performed in P12. The 48px
reference measurements are not a claim about physical device dp/pt behavior.
Full state-event animation, FX/audio/haptics are P13; full screens/onboarding
P14; full glyph QA P15; save/meta/integration later. Existing rules, content IDs,
JSON bytes, package pins, prices and user assets were preserved.

The review used the direction contract and actual ShapeMania reference captures.
There is no approved composite mockup or formal QUALITY BAR card, so no exact
reference-fidelity or comparative ceiling claim is made.
