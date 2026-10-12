# CASCADE website

An inhabited floating-garden scene, a tiny reorder example, actual iPhone gameplay, day/night art, support, Privacy Policy and Terms of Use. English site; legal menu labels also localized in the game.

Build: `node scripts/build.mjs`. Validate: `node scripts/check.mjs`. Preview: `python3 scripts/serve.py` → http://127.0.0.1:4173.

When opened standalone, the build uses `content/site-config.json`. In the full CASCADE workspace it mirrors `../Configs/website.json` and exports the same offline Privacy and Terms into the Unity project. All identifiers are public configuration; no keys or secrets belong here. `dist/` is the deployable static output.

Publisher and support/privacy contact details are set in `Configs/website.json`. Legal pages remain visibly marked draft and crawler indexing remains off until the audience treatment is confirmed, the documents are reviewed and approved, and public URLs are verified. Live deletion and cloud/native purchase verification require the game backend; this website does not perform those actions.

No source upload or deployment was completed: automatic approval review requires explicit user permission to transfer the website source, game artwork and legal drafts to Sites and publish a private Site. Local files are ready. `.openai/hosting.json` records the registered Site identity; it is not proof that the Site is live.

See `../Docs/WEBSITE_LEGAL_OCT09.md` in the main workspace for full setup and validation limits. The public-facing site does not display this technical setup text.
