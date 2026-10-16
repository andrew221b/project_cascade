# My items reference, October 14

Use the supplied 720×1280 screen as geometry and colour target. Reuse shop header, wallet, awning, rounded stock badge, cards, parchment and shared green primary button. New background, pale pedestal, blue spool and storefront are isolated generated layers; no baked text or floating leaves. Keep all sprites proportional, use one uniformly scaled stage inside safe areas.

Display order: spool, hourglass, lantern, wayfinder, lens, repair kit. Four cards per page, zero-stock thumbnails muted, selected object always clear. Shop resumes the selected tool and returns to its original bag visit. No prices, tool effects, inventory IDs or save schemas change. Strong thread is a display name for the undo spool; the prose explicitly describes an extra undo, not repair. EN/RU/UK text auto-sizes with fixed safe bounds.

Soft procedural rays stay behind the item. Depth-blurred leaf materials are limited to item and trophy reference screens; their pool remains independent of equipped trophy effects. Foreground/ad pause and reduced-motion preferences are respected.

Validation evidence will be recorded in Reports/ItemsReferenceOct14.

## Completed validation

- Unity 6000.3.8f1 MCP, the project on the external disk. Zero compilation failures and no shader messages.
- 22 EditMode inventory/economy tests; 23 PlayMode item, collection, shop, inventory and parcel flow tests passed. Last PlayMode run: 2026-10-13 23:07:01Z.
- Native screenshots: all six items, stock 0/1/5, EN/RU/UK with large text, 320×693, 390×844 safe insets, 720×1280, 768×1024. No overflowing or truncated reference labels; 115/115 enabled control centres hit their own buttons.
- Uniform item aspect and white tint checked. Shop returns to the same bag visit; navigation and animation preserve puzzle state. Foreground pause freezes light, reduced motion hides leaves and freezes light. Trophies retain their own ambience and use depth foliage.
- New sharp/soft depth layers have a pooled lifecycle and dispose transient materials. The reused leaf has trilinear mipmaps so it remains clean at tiny sizes.
- 480 portrait frames rendered in Unity for an eight-second 60 fps review clip. This verifies the decorative animation sequence, not target-device frame rate. No phone or player build was made.
- New undo-spool prose is intentionally truthful about an extra undo; the reference’s repair prose would misdescribe the tool.
- Generated illustration layers are faithful recreations rather than identical original illustration pixels. Composition was checked visually against the supplied screen; native captures are the review evidence.
