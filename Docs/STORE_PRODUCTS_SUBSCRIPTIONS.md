# Store products for Flying Ladders

Use the **same product IDs on App Store and Google Play**. Prices and currency are configured in the stores; the game displays each product's `localizedPriceString` returned by Unity IAP. Do not put currency amounts in the client catalog. Until the stores return product metadata, purchase controls remain unavailable and show a dash instead of an invented price.

| Product ID | Store type | Period / contents |
| --- | --- | --- |
| `cascade.no_ads_week` | Auto-renewable subscription | 1 week |
| `cascade.no_ads_month` | Auto-renewable subscription | 1 month |
| `cascade.no_ads_year` | Auto-renewable subscription | 1 year |
| `cascade.coins_240` | Consumable | 240 coins |
| `cascade.coins_480` | Consumable | 480 coins |
| `cascade.coins_600` | Consumable | 600 coins |
| `cascade.coins_720` | Consumable | 720 coins |
| `cascade.coins_960` | Consumable | 960 coins |
| `cascade.coins_1080` | Consumable | 1080 coins |
| `cascade.no_automatic_ads` | Non-consumable, **legacy restore only** | Lifetime ad removal for existing owners; do not create a new sale listing |

In App Store Connect, put the three subscriptions in **one subscription group** so the player cannot accidentally hold overlapping plans. In Google Play Console, create one auto-renewing base plan per subscription product and no multiple base-plan offers for the same ID. The client registers one Unity IAP `Subscription` per ID. Register and activate all products for the target app, configure localized names and prices, then enable `storeCatalogRegistered` only after sandbox purchase, renewal, expiry, restore, and cancellation tests pass on both platforms.

The ad-removal subscription suppresses only the automatic ad shown after every second completed level. Reward videos for every second hint and parcel extras remain optional. Server verification writes each subscription's verified expiry, not a permanent no-ads flag. The legacy non-consumable remains readable and restorable for previous owners. The annual savings badge is calculated from the twelve monthly store prices in the same currency and disappears if either price is unavailable.

Store setup also requires publisher AdMob IDs, account-bound receipt verification, and published privacy policy/terms URLs in the release configuration. Real-money purchase verification is not possible in the Editor or before the products exist in the stores.
