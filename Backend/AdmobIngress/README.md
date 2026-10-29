# Flying Ladders notification and receipt ingress

Canonical root: https://vickand.com/api/flying-ladders

The Worker handles signed AdMob callbacks, Apple/Google notifications, bounded local purchase verification, deletion capabilities and operational health. All game data, ownership, grants and reconciliation stay in Unity Cloud Code and Private Cloud Save. It has no database or external maintenance timer. The owner selected hourly Unity Scheduler only.

Build with `node Backend/AdmobIngress/build.mjs` from the project. Publish the generated `worker.production.mjs` as module `worker.mjs`, preserving encrypted secret bindings and the existing route. Use the pinned sibling AccountCloud SDK/build dependencies. The official Apple SDK is initialized lazily for the Workers runtime.

Run `node --test Backend/AdmobIngress/worker.test.mjs Backend/AdmobIngress/reliability.test.mjs Backend/AdmobIngress/runtime.test.mjs`. There are 22 Node cases plus one actual workerd/Miniflare case with fake transports. Public signed delivery tests are recorded separately.

Bindings: secret UNITY_SERVICE_KEY_ID, UNITY_SERVICE_SECRET, LEGACY_RELAY_TOKEN; public APPLE_ROOT_CA_BASE64, GOOGLE_PUSH_AUDIENCE, GOOGLE_PUSH_EMAIL; rate limiters PUBLIC_BUDGET (100/min/IP/route/location) and PROOF_BUDGET (10/min/proof/location). These are approximate local abuse limits, not global admission control. Signed publisher callbacks bypass shared-IP throttling and require valid signatures before Unity calls. The compatibility relay authenticates its forwarded IP; request headers cannot select a player identity or a grant.

Keep redirects manual and responses/inputs bounded. Unity authentication/module timeouts are 6/20 seconds. Publisher processing failures remain retryable (503), invalid signatures are denied. Only fixed error codes, route names, types and durations may be logged; redact query strings and disable invocation URL logging. Never log raw callbacks, receipts, purchase tokens, IDs or credentials. Basic Cloudflare real-time issue alerts do not establish an uptime SLA.

See deployment.json for non-secret deployment identity and Docs/UNITY_CLOUD_MIGRATION_OCT27.md for migration, quota/cost boundaries, recovery and actual verification limits. The free Workers plan and Unity quotas are finite. No mobile player build was created.
