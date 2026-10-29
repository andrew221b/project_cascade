# Flying Ladders button system

The shared controls live in `Assets/_Cascade/Content/Resources/CascadeArt/DesignSystem/Buttons/` (inside CascadeUnity). Each of the five roles has a ScriptableObject preset and a reusable prefab. Existing game factories attach the same `GameButtonDesign` view, so screen-specific constructors no longer own the finished action geometry.

| Role | Reference size | Existing artwork | Use |
| --- | --- | --- | --- |
| PrimaryGreen | 480 × 104 | CleanMenuOct14 / GreenAction | Main text actions |
| SecondaryYellow | 480 × 104 | CleanMenuOct14 / AmberAction | Secondary actions; Map |
| TertiaryLabel | 480 × 104 | No background | Text links; authored inline legal slots remain compact |
| IconPrimary | 96 × 96 | InventoryStatCard | In-game undo, shop, replay, My things |
| IconSecondary | 96 × 96 | SquareButton | Pause, hint, help, settings, home, close/back |

Sizes are expressed in the game's 720 × 1280 reference composition and scale together through the safe viewport. Every action has at least a 44 logical pixel touch area. Full actions use InterfaceTitle at a common 34-unit maximum (+10% with Larger text); captions fit the actual writing field on both axes. Icons use a common 48-unit slot. The visible icon/caption group is centred together with a 4 logical pixel gap. Text keeps at least 2 logical pixels inside the measured inner writing field; the decorative rim does not count as available space.

`GameButtonDesign` owns the skin, finished dimensions and one content row. `CenteredContentRow` owns visible-glyph centring and fitting. Layouts still own where buttons go. Existing callbacks, interactability, purchase state and reward state remain owned by their original controllers. Source artwork, product IDs, prices and rules are not changed by this system.

Map uses the failure screen's existing scroll icon and the same yellow preset on failure, checkpoint, victory, comics and conversations. Shop discovery actions say “Shop” / “Магазин” and have the storefront icon. Square controls have no extra caption below their icon; stock circles remain separate and keep their readable badge layout.

## Preserved compound controls

Shop / My things item cards, coin-offer purchase strips and nested level-card actions retain their authored dimensions, illustrations and layouts. Settings/list/form rows retain their leading-aligned reading layout. Mode/category tabs and paging arrows keep their contextual layouts. These are compound selection/navigation controls, not full primary/secondary action rows.

Terms and Privacy keep their existing inline slots and shadowed label appearance. The shared tertiary prefab provides the same background-free role for future controls.

## Updating or using a template

Edit a role's preset once, then use **Cascade → UI → Refresh button prefabs** to regenerate the five template assets. Screen factories can call `RegisterButtonDesign(button, role)` after creating content. Set `PreserveSlot` only for intentionally inline text links. Use `MapAction()` for a Map action. Do not add a second content layout to a bound action; the view removes competing legacy rows.

No runtime PNG readback, texture regeneration, package change or player build is required for these UI assets. Editor tests cover mobile geometry, localisation, navigation and preserved card controls. Device builds require the user's separate request.

### Follow-up: navigation rows and invisible hit regions

The mobile regression review on 29 October found that a shared 480-unit primary action needs explicit row placement when next to a tertiary Skip action. `ConversationReferenceLayout.PlaceActionPair` is opt-in for comic/dialogue rows: preserve the primary size, fit the label action into the remaining space, use the shared 4-logical-pixel gap and keep both controls inside the viewport. Other screens keep their authored layout owners.

`GameButtonDesign.ContentOffset` defaults to zero. Legal links use it to retain the approved painted caption position while extending their invisible hit targets downwards, clear of Restore purchases. Targets remain at least 44 physical pixels and do not overlap. The billing disclosure stays exactly where it was. Comic layout also reacquires the active centring row after shared binding reparents the caption; a retired component must not position content in the new writing field.

Evidence: `Reports/UILayoutRegressionOct29/verification.json`. Validation now checks complete control bounds and both subscription entitlement states, alongside text fit and centring. These are Editor checks, with no player build or service change.

### Completion audit: live captions and map factories

A follow-up inventory found an unbound Travel action and map header controls built from custom image factories. They now explicitly select PrimaryGreen / IconSecondary and supply the existing face as the target graphic, preserving the home/settings glyphs without creating a second frame or back arrow. Travel now shares the 480 × 104 action size, 34-unit font and 48-unit icon slot.

`GameButtonDesign.CanonicalCaption` resolves Map navigation aliases and the retired Find in shop copy. A bound text action can switch between Continue and Map: refresh switches the preset, visible icon and complete centred content row together. The scroll is reused, original icons return when leaving Map, and callbacks are untouched. Comic/dialogue controllers refresh the view immediately after changing the caption; conversation final captions are canonicalised once at entry.

The prefab test loads the five existing assets without regenerating them. Final mobile checks cover EN/RU/UK, 320 × 568, 390 × 844 and 430 × 932, including large text, live caption changes, map navigation and complete control bounds. Card factory source and all 22 button preset/prefab/material/meta files retain their pre-audit hashes. Evidence: `Reports/ButtonCompletionOct29/verification.json` and actual Unity render sheets. No player build or device installation.

### Authored Home artwork is preserved

The user's later instruction restores the main Home menu's original illustrations. The three navigation tiles (My things, Shop, Settings) keep their 165 × 157 authored slots, NavButtonFrame, original BagIcon / ShopIcon / SettingsIcon and visible stacked captions. The account door uses AccountReference / SignIn; Remove ads uses HomeOct13 / NoAds. Those five controls opt out through `Button(..., sharedDesign: false)`, so shared action binding cannot replace their baked artwork or hide their labels. Their existing minimum touch sizing, safe positioning, account age guard and navigation callbacks remain. The Home Play action continues to use PrimaryGreen.

The map's Travel action retains PrimaryGreen, shared dimensions and typography, with a single centred text label and no play glyph. Other system and gameplay icons continue to use their established meanings. The follow-up reviewed the original unification diff and found the account/no-ads replacements alongside the three Home tiles; all five are restored.

Validation: four targeted Editor tests pass, including 18 Home +18 map configurations across EN/RU/UK, three phone sizes and both text-size preferences, real raycasts, separate touch regions and four Home navigation routes. Existing broad menu/card/gameplay checks also pass. See `Reports/MenuIconsRestoreOct29/verification.json` and `comparison.jpg`. No player build or installation.
