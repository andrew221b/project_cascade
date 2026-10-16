# Settings reference implementation — 15 October 2026

The four user images are the visual and functional contract. Use the existing Unity/uGUI application, not a separate prototype. Root: Sound & motion, Comfort & help, Languages, Notifications inline toggle, Remove ads, Privacy Policy, Terms of Use, Restore purchases. Sound: Music, Sound effects, Haptics. Comfort: Larger text, Tutorial help. Languages: English, Русский, Українська, selected immediately with one active radio. Remove other visible settings entries, notes and footer buttons; retain old serialized preference fields for save compatibility without resetting the player's choices.

Preserve existing audio/haptics, coaching, localization, notification permission, legal-document, verified store restore and subscription handlers. Never fake an unavailable native permission or store connection. Notifications remain an inline switch and redraw when permission completes; disabled restoration does not create a note absent from the reference. No changes to gameplay, prices, product IDs or serialized field types.

Match the 720×1280 reference geometry, using a uniform content scale, independently fitted scene and a sliced transparent outer wood frame. Fonts, icons and controls must retain aspect; leave sufficient touch targets. Reuse the app's existing back arrow, text fonts and interaction feedback. Generate only the missing background/frame/control artwork. Use the original generated PNGs unchanged in Resources, with Unity sprite regions rather than raster postprocessing.

Required verification before handoff: native Unity screenshots of all four pages at reference size and phone sizes, visual comparison against all four sources, click/permission/store availability checks, locale selection and saved preferences after reload, back navigation to the originating screen, no puzzle mutation, no overlapped/clipped text or controls. Physical-device installation is not part of this request.

## Implemented composition

The root has exactly seven rows plus the gold Restore purchases action and upper-left Back. Its three sections contain exactly three audio/haptics switches, two comfort/help switches, or three language radios. Labels are localized live text; icons, gold selection, radios and switches are independent artwork. Removed the prior visible extra controls/notes/footer. Existing serialized legacy preferences remain intact for compatibility. Existing button feedback is reused. Reference captions use the existing Nunito SemiBold font with bold rendering; the header uses the existing Nunito ExtraBold font. Only the privacy label has the reference's explicit two-line RU/UK break.

One 720×1280 stage scales uniformly. The scene covers the physical canvas without changing its aspect ratio; only straight sections of the transparent frame and wooden panel stretch. The two title ropes extend through the safe top inset. Touch rectangles expand to at least 44 screen pixels without resizing the painted faces. Decoration remains behind raycast targets. Missing generated chevron CanvasRenderer and thin caption weight were identified in native screenshots and corrected before final verification.

## Assets and generation specifications

Tool: **built-in imagegen**, no fallback CLI. The selected generated PNG files were copied unchanged into `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/SettingsOct15/`. Atlas slicing, layout and text happen in Unity; production pixels were not edited by Python or another raster tool. QA montages only resize copies of reference/native screenshots.

Inputs are the four user JPGs in `/tmp/codex-remote-attachments/01a1089e-7eaf-75a1-8066-dbfc48df27a1/7C54E097-0B8D-4B85-B688-1E48CD5896FC/`. Final saved assets:

- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/SettingsOct15/Scene.png` — 942×1670 RGB.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/SettingsOct15/Frame.png` — 941×1672 RGBA, transparent centre.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/SettingsOct15/Surfaces.png` — 1536×1024 RGBA, six surfaces.
- `CascadeUnity/Assets/_Cascade/Content/Resources/CascadeArt/SettingsOct15/Icons.png` — 1448×1086 RGBA, sixteen icons/control states.

Compact record of the final prompt set (generation/edit specifications, rather than a verbatim tool transcript):

1. **Scene / precise reference edit:** Use Photo 2 as the composition reference. Preserve the portrait fantasy sky, distant castles on floating islands, clouds, lake and mountains. Remove the title, ropes, Back control, language panel, foreground frame, daisies and ledge; reconstruct the landscape behind them. No UI, labels or added foreground objects.
2. **Frame / transparent extraction:** Use Photo 2. Keep only the thin wooden side posts, upper leaves, lower wooden ledge and lower-corner white daisies/foliage in their reference proportions. Transparent centre, no upper crossbeam, title or UI. Final targeted edit: remove only extra upper-corner white daisies; preserve the posts, foliage, lower corners and ledge.
3. **Surfaces / transparent six-piece atlas:** Match the four references' warm sculpted wood, cream paper, gold and daisies. Produce a clean 2×3 atlas of a blank suspended floral wooden header with ropes, wooden backing panel, cream row, selected gold row, gold restore button face and blank square face. No labels or icons. Final targeted edit: make every pixel outside the six silhouettes transparent, preserving asset position, colour and interior artwork.
4. **Icons / transparent sixteen-piece atlas:** Match the references' warm copper/brown sculpted game art. Produce a 4×4 atlas: speaker, gamepad, globe, bell; crossed-out speaker, privacy document/shield, terms document, restore arrows; music note, vibrating phone, Aa, help question mark; green ON switch, cream OFF switch, empty gold radio, green checked radio. No other text.

Original generated source paths, output dimensions, alpha and the icon regions are retained in `Docs/SETTINGS_ASSET_PROVENANCE_OCT15.json`; the runtime regions are in `SettingsReferenceArt.cs`. Existing Back artwork, fonts, real settings handlers and button feedback were reused.

## Final validation

- **6/6 PlayMode passed**, Unity 6000.3.8f1, 15 October 2026 05:07:08–05:07:25 UTC. Four new settings integration cases plus two existing coin/store boundary cases. Result: `Reports/SettingsReferenceOct15/playmode.xml`.
- **25 native captures**: all four RU pages at 720×1280, 390×844, 360×640 and 768×1024; EN/UK at 390×844; a RU root capture with simulated safe top/bottom insets. Large text enabled throughout the visual matrix. Inspected all four references next to the actual 720×1280 captures, then phone and locale/tablet sheets.
- Assertions cover uniform scale, exact visible control sets, no truncated/overflowing text, at least 44-pixel nonoverlapping screen-contained touch targets, real EventSystem raycasts through artwork and rendered chevron meshes. Real clicks apply audio/haptics, coaching, large text and language; an actual saved-profile reload retains them. Legal document content and subscription/back routes work. Home/Map/Pause origins return correctly. Puzzle hash and wallet/grants remain unchanged.
- All current `_Cascade` C#, shader and assembly-definition sources in the root matched the tested isolated copy. Final touched source/asset SHA-256 hashes are in `Reports/SettingsReferenceOct15/validation.json`.
- After the user closed the primary Editor's save modal, **official Unity MCP** confirmed current code/resources and captured four additional 390×844 pages in the primary Editor using a temporary profile. Final capture checked text and unchanged puzzle state; current Console returned zero errors/warnings. A stale runtime left by the earlier script reload and an Edit Mode capture probe were restarted/cleaned before this successful run. Evidence: `Reports/SettingsReferenceMcpOct15/validation.json` and four native PNG/JSON pairs. No scene save/discard was performed; the test Play session was stopped afterwards.

Useful previews: `Reports/SettingsReferenceOct15/mobile-screens.png`, `reference-comparison.png`, `locales-and-tablet.png`.

Limits: mobile/tablet resolutions were rendered natively in Unity, not on installed physical-device builds. OS notification prompts and real production-store restoration cannot be exercised in this Editor: those controls reflect actual provider availability rather than faking a green permission or entitlement. All three real localization tables must load in the isolated fixture; only its known cold-preload setup diagnostic is narrowly expected. Other runtime UI errors are not suppressed. No store transactions, backend deployment or performance measurement. Generated artwork follows the references; literal bitmap identity is not claimed.
