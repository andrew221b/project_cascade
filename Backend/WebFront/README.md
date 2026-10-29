# Flying Ladders public deletion and legal pages

Current production: https://flyingladders.vickand.com/delete-account/ . Only the six patterns in deployment.json use this Worker. The root marketing page, support page, existing images, fonts and scripts remain on the retained Vercel origin. Do not replace these routes with a hostname-wide wildcard. Cloudflare CNAME still targets cname.vercel-dns.com and is proxied so the selected routes execute. workers.dev and previews are disabled.

`ACCOUNT_GATEWAY` is a service binding to the existing ingress, which calls Unity Cloud Code. This Worker has no credentials, database, game profiles, schedule or publisher SDK. It accepts only the fixed same-origin deletion path, exact Origin, JSON code/confirmation and bounded bodies. It forwards no browser cookies, authorization or caller relay headers. Logs omit invocation URLs/query strings; code never enters a URL.

Rebuild Website first, then run `node build.mjs` and `node --test worker.test.mjs`. The build embeds only deletion and privacy/terms/data-request documents plus the two new form assets. Existing resources keep the Vercel implementation. Use the Cloudflare plugin to publish worker.production.mjs with the same service binding and settings; preserve all unrelated main-domain mail/verification/notification routes.

The existing Sites gateway v18 redirects its old /delete-account page to this canonical address. Existing API compatibility routes remain unchanged for installed clients. Current client configuration and future embedded documents use the new URL; no mobile player build was created in this migration.

Rollback: restore the prior selected Worker version, or remove only the six route IDs in deployment.json. The original Vercel deployment remains intact. If removing all selected routes, the record can return to its prior DNS-only state (proxied false, TTL300); never modify the record target, mail records or other zone routes.

Design note, October29: this migration changes only the public deletion entry point, delivery of approved/current legal documents, and exact allowed form origin. Account IDs, ownership, deletion authority, prices, receipt checks, databases and native account flows are unchanged. Repeated accepted deletion codes acknowledge the same irreversible tombstone rather than recreating an account or creating another deletion job.
