# CASCADE website and legal documents — 9 October 2026

## Implemented

Website is in `Website/`: a buildless static site with a small dependency-free build script. New original transparent garden island, the game's Mika/cloud/bird/insect artwork, curved sprite flights, layered float and cloud motion, pointer depth, day/night switch, native iPhone captures, a real adjacent-swap example, responsive navigation, FAQ, support, account/data requests, detailed English Privacy Policy and Terms of Use. Reduced motion and a persistent pause control are supported; animation work stops offscreen and in hidden tabs. No autoplay audio.

Public configuration lives in `Configs/website.json`; no secrets belong there. A mirrored `Website/content/site-config.json` makes the standalone Site source reproducible. The website and Unity offline documents are generated from one `Website/content/legal.json` source. Preserve artwork originals; only delivery resizing/WebP/JPEG encoding is used. `Website/DESIGN.md` records the surface concept and palette using impeccable and frontend-design.

Legal UI is available from the Settings root, so it does not require cloud sign-in. Full English documents scroll in a safe-area reader and Close returns to the same menu. EN/RU/UK menu labels are localized; the reader states that the full documents are English. The native Restore purchases action is now also on the Settings root. It calls the existing verified, idempotent native restoration path and never grants/spends coins in the button callback. Unconfigured store restoration is visibly disabled with an explanation.

## Design note

No products, prices, grants, gameplay rules, existing IDs or package pins changed. Additive LegalSettings and `termsOfUseUrl` configuration supply requested documents and links. The store-submission guard refuses missing/unapproved legal details or drafts; Development previews remain available. This is a release validation rule, not a change to puzzle rules.

## Run and build

From the repository root:

- `node Website/scripts/build.mjs` regenerates the static `Website/dist` pages, web artwork derivatives, and the two Unity `Resources/CascadeLegal/*.txt` documents.
- `node Website/scripts/check.mjs` checks page titles, one H1 per page, metadata, structured data, local links/fragments/assets, alt text/dimensions, no unconditional tracking, draft indexing state, offline document parity and scene payload.
- `python3 Website/scripts/serve.py` serves the local preview at `http://127.0.0.1:4173`. This is a strict server with real 404 responses and trailing-slash redirects.
- Unity menu: `CASCADE / Release / Apply website and legal configuration`. This populates LegalSettings after the build. `publicLegalUrlsVerified` must remain false until public access has actually been checked.

The local preview has no store links or contact submission until real configuration is supplied; it does not invent URLs or pretend to send a form. Email links, when configured, open the visitor's mail application.

## SEO and the supplied checklist

Implemented: static HTML without JS required for content; unique titles/descriptions and semantic headings; canonical URLs; consistent clean routes and trailing-slash redirects; a genuine 404 page; local cross-linking and verified fragments; useful image alt text and fixed dimensions; self-hosted fonts and optimized/lazy images; mobile layout; crawlable FAQ; truthful VideoGame/WebPage/FAQPage JSON-LD without fabricated ratings, offers or awards; Open Graph/social art; robots.txt, sitemap.xml, and a factual llms.txt for machine readers. Unconfigured `app-ads.txt` returns 404; entering a real `pub-…` AdMob publisher ID generates Google's required entry.

The site is currently a local/private review draft. `publicIndexingEnabled=false` and missing publisher approval produce `noindex` and `Disallow: /`. Public indexing must be enabled only after the site is public and approved. A sitemap file does not itself cause indexing, and structured data does not guarantee rich results. llms.txt is an advisory reader file, not a promise of AI indexing or a ranking factor.

Config hooks: Google Search Console verification; Yandex Webmaster verification; a single optional measurement provider (`none`, `ga4`, `gtm`, or `yandex`) and its ID. No measurement network request occurs before opt-in. Measurement choices have equally prominent allow/decline controls. Revocation clears recognized measurement cookies and reloads without initializing providers. GTM tags must independently honor the declared denied ad consent; do not publish an unrestricted container. Optional events: puzzle_demo_complete, store_visit, support_email_start, scene_change. No game save, identity or email value is sent in these events. Session replay/Webvisor is disabled.

Remaining after deployment: verify the hosting provider honors redirect/header files and genuine 404s; confirm public unauthenticated policy access; connect the final domain and update canonical/sitemap; submit sitemap to Search Console and Yandex; set actual measurement goals/container rules if wanted; collect real-device/production Core Web Vitals. Local cached-browser timings are not a production CWV certification. Commercial transparency is present (pre-release availability, support, virtual-purchase limits, restoration/refunds) without invented launch dates, pricing, social proof or sales copy.

## Complete before public legal use

1. Fill publisher legal name, country and public correspondence address, support email and privacy email. Confirm the target age/audience, store categories and actual child/privacy treatment; set `ageAudienceReviewCompleted` only when confirmed.
2. Review/approve the text for the publisher's jurisdiction, deployed providers and operational retention schedule; set `documentsApproved` only after that review. The drafted privacy text explicitly describes current local play and optional services without promising unimplemented login or deletion. Material future feature changes require updating the notice.
3. Complete and verify actual game-account/data deletion before enabling account creation. This task supplies the external request information route, not a working deletion backend. The existing Unity Player Accounts portal is not deletion of CASCADE data in other UGS services.
4. Enable public hosting with the user's approval, then verify `/privacy/`, `/terms/`, `/support/`, `/data-request/` from a signed-out browser. Set `publicLegalUrlsVerified` only after checking; this makes external links available in Unity. Keep the offline copies in the app.
5. Fill App Store/Google Play URLs when the listings exist. Enter the final support/privacy/terms URLs in `Configs/release-ids.json` and the corresponding stores. App Privacy / Data Safety / consent declarations must match the deployed SDK configuration.
6. Add a real AdMob publisher ID for app-ads.txt and use this public site as the developer website. Configure actual production ad IDs separately in `Configs/admob.json`.
7. Live native Restore transactions, cloud transfer and OS permission behavior still require configured store/cloud accounts and device testing. No release build or phone installation was performed in this task.

## Validation evidence

`Reports/Website-Oct09/` contains desktop/mobile website captures, a mobile legal page, local HTTP response checks, Unity import results and isolated Unity legal-reader captures where available. The static page validator passed for six HTML pages and 153 internal links/assets/fragments; the critical scene assets are about 1.45 MiB. The interactive example was solved through its actual UI in two exchanges. Responsive browser checks found no horizontal overflow, broken images or JS errors. Desktop/mobile visual inspection used one initial batch and one confirmation batch. The native Editor imported legal controls/localization with compilationFailed=false and disabled unapproved public links.

## Publication state

Site identity was registered as `appgprj_6ac91521499c8191ac413a2e3694998e`; its expected URL is stored in the hosting manifest and config. Registration is not deployment. Automatic approval review rejected the source/artwork/legal-draft upload to the external Sites repository because it requires explicit user permission for that transfer and private publication. No source was pushed and no live website is claimed. Local source, preview and deployable files remain complete. Do not bypass that rejection through another upload channel.

## Checked primary references

- Apple App Review: https://developer.apple.com/app-store/review/guidelines/
- Google Play User Data: https://support.google.com/googleplay/android-developer/answer/10144311?hl=en
- Google account deletion: https://support.google.com/googleplay/android-developer/answer/13327111?hl=en-GB
- Google AdMob Unity privacy/UMP: https://developers.google.com/admob/unity/privacy
- Unity privacy hub: https://unity.com/legal/privacy-policy
- EU individual data-protection rights: https://commission.europa.eu/law/law-topic/data-protection/information-individuals_en
- Apple refunds: https://support.apple.com/en-us/118223
- Google Play refunds: https://support.google.com/googleplay/answer/2479637?hl=en
