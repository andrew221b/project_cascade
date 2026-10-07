namespace Cascade.Application
{
    // Draft IDs to register in both stores. Real currency prices belong to those stores.
    public static class PurchaseCatalog
    {
        public const string NoAutomaticAds="cascade.no_automatic_ads";
        public const string Coins240="cascade.coins_240",Coins480="cascade.coins_480";
        public const int CoinPurchaseWalletThreshold=960;
        public static readonly string[] Products={NoAutomaticAds,Coins240,Coins480};
        public static int Coins(string product)=>product==Coins240?240:product==Coins480?480:0;
        public static bool IsKnown(string product)=>product==NoAutomaticAds||Coins(product)>0;
    }
}
