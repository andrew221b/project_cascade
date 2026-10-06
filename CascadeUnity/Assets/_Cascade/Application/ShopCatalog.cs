using System;
namespace Cascade.Application
{
    public enum TravelTool { Wayfinder, UndoSpool, ReturnHourglass, SignalLantern, LandingLens }
    // Virtual coin catalog; these are not App Store / Google Play product identifiers.
    public static class ShopCatalog
    {
        public static readonly string[] TrophyIds = { "leaf", "sun", "sky", "moon", "wind_chime", "rain_badge", "copper_acorn", "beacon_brooch" };
        public const int ToolCount = 5;
        public const int StockLimit = 5, KeepsakePrice = 240;
        public static int Price(TravelTool tool) => tool == TravelTool.Wayfinder ? 120 : tool == TravelTool.UndoSpool ? 80 : tool == TravelTool.ReturnHourglass ? 200 : tool == TravelTool.SignalLantern ? 160 : 100;
        public static bool IsTool(TravelTool tool) => Enum.IsDefined(typeof(TravelTool), tool);
    }
}
