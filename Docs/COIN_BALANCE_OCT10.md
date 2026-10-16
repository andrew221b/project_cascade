# Coin balance adjustment — 10 October 2026

First-clear currency was reduced from 40 to 20 coins, with the ten-clear
milestone bonus reduced from 100 to 50. A fresh 540-clear campaign now yields
13,500 clear coins before parcels, replay records, and optional ads (previously
27,000). This lowers predictable free income without changing puzzle rules,
parcel contents, replay credits, rewarded-ad grants, or real-money product IDs
and pack sizes.

Virtual prices rose by 50%: tools now cost 120–300, core Mika cosmetics 360,
and the most expensive cosmetics 2,160. The existing 240/480 packs stay equal in
value per coin; the purchase prompt threshold tracks the new top price. The
result is a longer earned-coin path while keeping a small pack useful for a
tool and a larger pack useful for a core cosmetic.

Revision-0 saves receive a one-time carryover equal to the difference between
their old and new clear-income totals, preserving their existing wallet. Cloud
inventory validation uses historical minimum prices so already-owned items do
not become invalid after the catalog reprices; new purchases are charged and
verified at current prices. Existing parcel receipts and grant IDs are
untouched.

Validation: tests were updated but not run; Unity compilation, cloud round-trip,
and store sandbox were not run. No iPhone build was made.
