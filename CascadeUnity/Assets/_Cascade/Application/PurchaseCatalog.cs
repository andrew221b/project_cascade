namespace Cascade.Application
{
    // Draft IDs to register in both stores. Real currency prices belong to those stores.
    public static class PurchaseCatalog
    {
        // Keep the old non-consumable in the catalog only to restore existing owners.
        public const string NoAutomaticAds="cascade.no_automatic_ads";
        public const string NoAdsWeek="cascade.no_ads_week",NoAdsMonth="cascade.no_ads_month",NoAdsYear="cascade.no_ads_year";
        public static readonly string[] NoAdsSubscriptions={NoAdsWeek,NoAdsMonth,NoAdsYear};
        public static bool IsNoAdsSubscription(string product)=>product==NoAdsWeek||product==NoAdsMonth||product==NoAdsYear;
        public const string Coins240="cascade.coins_240",Coins480="cascade.coins_480";
        public const string Coins720="cascade.coins_720",Coins960="cascade.coins_960",Coins600="cascade.coins_600",Coins1080="cascade.coins_1080";
        public static readonly string[] CoinProducts={Coins240,Coins480,Coins600,Coins720,Coins960,Coins1080};
        public const int CoinPurchaseWalletThreshold=ShopCatalog.MaximumTrophyPrice;
        public static readonly string[] Products={NoAutomaticAds,NoAdsWeek,NoAdsMonth,NoAdsYear,Coins240,Coins480,Coins720,Coins960,Coins600,Coins1080};
        public static int Coins(string product)=>product==Coins240?240:product==Coins480?480:product==Coins720?720:product==Coins960?960:product==Coins600?600:product==Coins1080?1080:0;
        public static bool IsKnown(string product)=>product==NoAutomaticAds||IsNoAdsSubscription(product)||Coins(product)>0;
    }
}
