# Direct manipulation and store integration — 2026-10-06

User requested lifted/glowing selection, literal finger tracking, a returning partner,
physical tumbling landings and phone-shake feedback. Physics is presentation only:
one committed SymbolId row, unchanged seats/hit regions, no random commands or costs.
The physical landing borrows the existing isolated cosmetic PhysX scene, uses at most
eight pooled bodies, then hands the same visible pieces back to guided seats. Pauses,
focus loss, skip and Reduced Motion must release that ownership safely.

Selection has no gold tile or rectangular target overlay. A contour of the actual
piece and a small target lift communicate selection and a legal destination. Drag
follows the finger within the screen, not a one-neighbour movement clamp; legality
still comes only from the resolver. Exchange preserves the exact release pose.

Phone acceleration is filtered and bounded; ordinary tilt is ignored. The shake
impulse moves the decorative suspension and unsettles pieces briefly without touching
the row or consuming a move. Deliberate gestures take priority over decorative shakes.

User confirmed native Apple/Google purchasing and general audience. Install official
com.unity.purchasing 5.4.4 and com.unity.services.levelplay 9.5.1 through UPM, with its
resolved dependencies. No other gameplay/economy package changes are intended.
Existing P19 ledger, receipts and frequency policy remain the transaction boundary.
Prices come from store metadata. No payment is performed by the agent. Live product
IDs, ads app/ad-unit keys, and native/sandbox validation remain explicit dependencies.
The initial prepared catalog is one optional non-consumable: no-automatic-ads
ownership. Paid cosmetic packs remain a future design option, not a shipped purchase.
No lives, forced paid retries, random paid boxes or paid puzzle strength.
Earned collections remain available. No phone build/deployment is requested this turn.


## Activation handoff

The user confirmed that no store products or LevelPlay app/keys exist yet. The draft
product ID is `cascade.no_automatic_ads`, registered as NonConsumable in both stores;
its price is chosen in those stores and displayed from localized store metadata.
There is no Codeless IAP catalog or second automatic initializer in Assets.
`MonetizationSettings.asset` deliberately keeps both release flags false and keys
empty. Never switch them on simply to make the screen look available.

Installed UPM packages are IAP 5.4.4 and LevelPlay 9.5.1. The LevelPlay installer
imported Mobile Dependency Resolver 1.2.185 and native dependency descriptors for
IronSource 9.6.1 / iOS 9.6.1.0, plus its Unity Ads adapter. Native dependency resolution
in a platform build has not been tested. No renderer or unrelated package upgrade
was requested.

The publisher must register the product, create LevelPlay app/ad units and provide
an HTTPS receipt verifier. `NativePurchaseStore` sends productId, transactionId,
store, receipt, jws and bundleId. The server must verify Apple JWS / Google purchase
proof against the expected app and product; only then return valid=true with exactly
matching productId and transactionId. A failed/absent verifier never grants an item.
The ledger saves before pending-order confirmation and deduplicates restore receipts.
The receipt server itself is NOT implemented or hosted by this change.

Connect a real consent/age/ATT flow to `InitializeAfterPrivacy` and validate its
native configuration before setting adsReleaseReady. General audience is the user’s
confirmed category; it is not guessed consent or a known adult age for every player.
Automatic ads follow C16 only after a saved win and base reward, with first-session,
first-12-deliveries, chapter-final and frequency protections. Optional rewarded
callbacks grant only verified inventory, never puzzle commands. The prepared coins
reward adapter has no player-facing offer until a useful spending loop is designed.

Manual retries from the fail screen also enter a session-only interstitial cadence:
one ad after 2 retries, then after 3, alternating thereafter. The interval resets
only after the ad opens; a failed or blocked show leaves it due. Retry remains
immediate when consent, ad readiness, the 180-second foreground cooldown, no-ads
ownership, or the existing two-ad session cap blocks a show. Completion replay,
recovery, and pause-menu retries do not count.

Official API references checked during integration:
[Unity IAP receipts](https://docs.unity.com/en-us/iap/receipt-validation),
[LevelPlay package integration](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/package-integration).

## Presentation limits

Falling/shaken pieces reuse eight pooled rigid bodies with box collision proxies,
not exact candy-mesh colliders or gameplay bodies. A brief collision/tumble phase
is followed by deliberate interpolation into the canonical seats. At rest, wind,
selection and exchanges remain guided motion. Ropes retain the existing constrained
point-chain simulation. Sensor thresholds still need an actual iPhone/Android pass.

### 11 October — Undo and catalogue clarification
Ordinary Undo and the Undo spool may revert commands on the current board, or an early drop that left the previous board intact. Neither restores a board destroyed by its last exchange or a bomb; only the Return hourglass crosses that boundary. This is an explicit user-requested rule correction. Prices, tool budgets, save schemas and IDs stay unchanged. Inventory is a reference catalogue; activation stays in the gameplay bag. Trophies use one catalogue of six larger cards per page, with no nested Mika/Scene/Frames tabs. Remove the redundant cosmetic-benefit/permanence captions and tool rules from the coin-purchase screen; retain actual gameplay tool limits.
