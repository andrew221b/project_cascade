# Trophy parcels and cosmetic pacing — 7 October 2026

Keep the eight published trophy IDs, price of 240 virtual coins and the first
12 three-part parcels unchanged. Add four new cosmetic-only looks for Mika:
`sky_pilot`, `moth_mask`, `garden_festival`, `cloud_gardener`. They cost 960
virtual coins each, with account-wide unique-clear gates of 60, 180, 300 and
420 respectively; replays and mode switches do not farm the gates. The eight
published looks stay at 240. No new look is a real-money product. They do not affect puzzle rules,
stars, score, moves, wood, wind or tool stock. Old saves and parcel claims keep
their meaning.

After the initial 36 unique clears, award one extra parcel per 12 new unique
clears. A cosmetic appears at clears 84, 192, 300 and 408, then repeats at
516; the other parcels contain 20 earned coins. A cosmetic already owned
at claim time converts to 20 coins, so buying early never produces a dead
duplicate. This adds at most 1.7 coins per later clear on average and cannot
alter the finite tool-stock limits. The deterministic order avoids rerolls or
device-clock dependence; replaying a cleared level adds no parcel.

Store/collection show earned trophies in full color and unavailable trophies in
gray; selection still previews the real look on Mika. Cost is shown by a coin
icon plus a number. A short first-open hand cue points to a trophy; touching
Mika gives a small, reduced-motion-aware response. The new costume sheets keep
Mika's two existing poses and are reused on all runtime Mika illustrations.

Cloud reconciliation must merge later parcel receipts and derived coin rewards
from the same unique-clear authority. Treat claimed receipts as append-only,
and reject any receipt beyond a published clear count. No ad or payment callback
is allowed to issue a parcel. Consumable tools remain excluded from parcels
until the server can authorize their stock as part of its economy ledger.
