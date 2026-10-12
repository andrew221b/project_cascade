# Catalogue selection and return flow — 8 October 2026

The shop, trophy collection and tool bag now separate selection from preview. Tapping a shelf tile changes only the highlight and the selected item's name/short benefit. The **Look closer** button opens the detail; Back returns to that same selected catalogue page. Trophy pager arrows switch to the first item on the adjacent page without opening detail. The previous shared `SelectShopItem` path set `shopDetailOpen=true`, which caused the reported preview jump.

Tool shelves contain the five usable tools. The Field guide tile was removed from **My tools**; its guide content is still available from its existing tutorial entry points. Counts moved above the wooden shelf into each tile. The trophy collection header uses an unambiguous owned/total count instead of the first leaf trophy icon, and unowned parcel fragments appear in the free upper corner of the preview rather than over Mika's feet.

The workshop was a passive gallery of four model illustrations; clearing ten unique levels in each early chapter changed a model tint but conferred no gameplay or cosmetic action. Its entry was removed from **My things** to avoid promising a feature that is not present. No saved workshop progress, rewards, assets, economy, gameplay rule or product ID was changed.

The existing scenery/frame preset IDs remain stable. Their EN/RU/UK display names were rewritten to be more distinct and compact. Tool descriptions were shortened, including the Wayfinder copy so it correctly describes a single safe move.

Native Unity MCP verification: `Reports/CatalogFlow-Oct08/validation.txt` records the selection/preview/pager/Back paths. Eight RU/UK focused screenshots cover shop tools, two trophy pages, a preview, inventory at 390×844 and 320×693 Large Text; `09-bag-ru.png` checks the simplified bag. The first attempted capture ran before the playable had initialized and hit a test-harness null reference; after a fresh isolated Play session the final review completed. Device touch behavior was not directly measured in this pass.
