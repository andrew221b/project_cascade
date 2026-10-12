# CASCADE — English store artwork

Five images per store: 01–03 illustrated compositions with genuine gameplay inside a phone; 04–05 lifestyle photographs with hands, smiling parents and children and genuine gameplay. All displayed gameplay comes from the dedicated iOS Simulator build, not the Editor or image generation. Promotional backgrounds and people are generated artwork. The phones are unbranded; no multiplayer or testimonial claim is made.

Upload folders:

- **Apple/iPhone-Medium:** five opaque 1206×2622 PNGs.
- **Apple/iPhone-Large:** five opaque 1320×2868 PNGs.
- **Apple/iPad-13:** five opaque 2064×2752 native game screens. The app currently supports iPad, so include these.
- **Google/Screenshots:** five opaque 1080×1920 PNGs, numbered in upload order; first three show gameplay.
- **Apple/icon-1024.png:** opaque square icon without a baked corner mask. The new icon is applied in Unity.
- **Apple/Cascade.icon:** layered Icon Composer source, validated with Apple's ictool. Rounded appearance previews in IconComposer-Previews are for review, not flat-icon upload.
- **Apple/header-3840x1646.png:** 21:9 product-page header.
- **Apple/search-1920x1280.png:** 3:2 search-result creative. Use the corresponding creative-asset slot if available in App Store Connect.
- **Google/icon-512.png:** 32-bit 512×512 PNG below 1 MiB.
- **Google/feature-1024x500.png:** opaque feature graphic without device imagery.
- **Google/adaptive-foreground.png / adaptive-background.png:** Android launcher layers.

Raw iPhone and iPad captures and timestamp/hash manifests are in **Raw**. Native iPhone capture: iPhone 17 Pro Max, iOS 26.5, 1320×2868. Native iPad capture: iPad Pro 13-inch (M5), iOS 26.5, 2064×2752. simctl captures the actual native framebuffer with --mask=ignored; the top system area is retained as captured. The Play compositions use the shared Unity game UI from those iOS captures; Android-specific runtime validation is still required before publishing.

The local capture build uses a separate bundle and staged progress/inventory for review; it does not modify the user's installed phone game. The capture controller is excluded from production builds. Gameplay screenshots remain intact inside their display frames; resizing and layout are deterministic. Editable layout renderer: Tools/render_store_assets.swift. Artwork briefs: prompts.md.

Verified against current primary sources on 9 October 2026:

- [Apple screenshot sizes](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/)
- [Apple actual-app screenshot requirements](https://developer.apple.com/help/app-review/guideline-reference/2-3-3-screenshots/)
- [Apple creative-asset sizes](https://developer.apple.com/help/app-store-connect/reference/app-information/creative-assets-specifications/)
- [Google Play graphic assets](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en)

This package prepares image formats and composition; store approval and production account/ads/purchase configuration have not been performed.

## Family/Words revision

Lifestyle 04 shows mother, father, daughter and son. Lifestyle 05 shows two families of friends (four parents and four children) with diverse racial backgrounds. Creative 03 and lifestyle 05 show the native Words-mode capture; lifestyle 04 shows Shapes. Photographic backgrounds regenerated; existing native screenshot hashes unchanged. This revision does not include a new phone build or claim that those captures show later platform-marker changes.
