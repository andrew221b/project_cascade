# Current publication — 29 October 2026

The product origin is https://flyingladders.vickand.com . The root/support/existing artwork remain hosted by Vercel. A narrow Cloudflare Worker now serves /delete-account, its two form assets, /privacy, /terms and /data-request on the same host. The form calls /api/account-deletion through a service binding to the permanent Unity ingress. Profiles and deletion state live in Unity Cloud Save, not Cloudflare D1. Approved documents are live without draft labels. See Backend/WebFront/deployment.json and Docs/RELEASE_MIGRATION_OCT29_RU.md. The older deployment notes below are historical.

# Flying Ladders website

Static marketing, support and legal website for https://flyingladders.netlify.app. The separate account service remains at https://cascade-sky-gardens.active-seed-4233.chatgpt.site; account, receipt and deletion API addresses are not marketing-site addresses.

Install Python dependencies with `python3 -m pip install -r requirements.txt`, then run `npm run build` and `npm run check`. Preview with `python3 scripts/serve.py` at http://127.0.0.1:4173.

For Netlify, deploy this folder as the repository root, or set Base directory to `Website` in the full project. Build command: `npm run build`. Publish directory: `dist`. Keep `requirements.txt` beside `package.json` so Pillow is installed before building. No macOS tools are required.

Standalone builds use `content/site-config.json`. Full-project builds mirror `../Configs/website.json` and also export matching offline Privacy and Terms to the Unity project. Configuration here is public; never include credentials. `dist/` is the deployable output, not the source tree.

The current policies describe the binary 16+ age choice, local guest play for under-16s, optional linked cloud profiles, weekly/monthly/yearly ad-removal subscriptions, legacy restoration, purchase verification, local reminders and account deletion. Account operations use the separate account service, not Netlify. `/delete-account` on Netlify directs visitors to the information page, which links the verified ownership flow.

Document approval and public indexing retain their existing disabled state. Public legal URLs were verified after the successful Netlify deployment. Store links stay pending until actual listings are verified. No build flags, purchase product IDs, account API endpoints or game rules are changed by this website update.

Brand artwork reuses the existing native game art. The old social poster containing CASCADE has been replaced by the existing high-resolution character icon. Legacy browser preference keys remain for compatibility.

Public service metadata now points to this origin in App Store Connect, TestFlight, Google Play, AdMob and Unity Player Accounts. Google Play changes are saved pending review. The support accessibility anchor is `/support/#accessibility`; Unity identity deletion/contact information is included on the data-request and privacy pages. Draft robots permit `/app-ads.txt` while retaining the existing page-indexing restriction. These saves do not publish a game release or a new AdMob consent message.
