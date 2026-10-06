using System;
namespace Cascade.Application
{
    public enum TravelTool { Wayfinder, UndoSpool, ReturnHourglass }
    // Virtual coin catalog; these are not App Store / Google Play product identifiers.
    public static class ShopCatalog
    {
        public const int StockLimit = 5, KeepsakePrice = 240;
        public static int Price(TravelTool tool) => tool == TravelTool.Wayfinder ? 120 : tool == TravelTool.UndoSpool ? 80 : 200;
        public static bool IsTool(TravelTool tool) => Enum.IsDefined(typeof(TravelTool), tool);
    }
}
