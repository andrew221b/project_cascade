using System;
namespace Cascade.Application
{
    public enum TravelTool { Wayfinder, UndoSpool, ReturnHourglass, SignalLantern, LandingLens, RepairKit }
    // Virtual coin catalog; these are not App Store / Google Play product identifiers.
    public static class ShopCatalog
    {
        public static readonly string[] MikaTrophyIds = { "leaf", "sun", "sky", "moon", "wind_chime", "rain_badge", "copper_acorn", "beacon_brooch", "sky_pilot", "moth_mask", "garden_festival", "cloud_gardener", "explorer_hat", "frog_suit", "star_knight", "fox_mask", "mushroom_cap", "bee_suit", "winter_hood", "pirate_hat", "jester_suit", "constellation_mage", "paper_crane", "tiny_comet", "dew_bell", "night_flower", "cloud_kite", "fern_compass", "amber_wing", "moon_boat" };
        public static readonly string[] SceneryTrophyIds = { "petal_drift", "firefly_dance", "butterfly_garden", "swallow_flock", "dragonfly_stream", "lantern_flight", "snow_wings", "comet_trails", "ribbon_kites", "origami_cranes", "bubble_dream", "aurora_veil" };
        public static readonly string[] FrameTrophyIds = { "garden_frame", "sky_frame", "copper_frame", "lunar_frame" };
        public static readonly string[] TrophyIds = JoinTrophies();
        public static readonly string[] BonusTrophyIds = { "sky_pilot", "moth_mask", "garden_festival", "cloud_gardener" };
        public static readonly string[] NewTrophyIds = { "explorer_hat", "fox_mask", "frog_suit", "star_knight" };
        public static readonly string[] ExpansionTrophyIds = { "mushroom_cap", "bee_suit", "winter_hood", "pirate_hat", "jester_suit", "constellation_mage", "ribbon_kites", "origami_cranes", "bubble_dream", "aurora_veil" };
        public static readonly string[] ParcelTrophyIds={"paper_crane", "tiny_comet", "dew_bell", "night_flower", "cloud_kite", "fern_compass", "amber_wing", "moon_boat"};
        public static bool IsParcelExclusive(string id)=>Array.IndexOf(ParcelTrophyIds,id)>=0;
        public const int ToolCount = 6;
        // Save representation boundary only; inventory has no gameplay capacity cap.
        public const int StockLimit = int.MaxValue, KeepsakePrice = 360;
        public const int MaximumTrophyPrice = 2160;
        static readonly int[] ExpansionPrices={1080,1260,1440,1620,1980,2160,900,1080,1440,1620};
        static readonly int[] SceneryPrices={720,900,1080,1260,1080,1440,1260,1620,900,1080,1440,1620},FramePrices={1080,1260,1440,1800};
        static string[] JoinTrophies()
        {
            var ids=new string[MikaTrophyIds.Length+SceneryTrophyIds.Length+FrameTrophyIds.Length];
            Array.Copy(MikaTrophyIds,ids,MikaTrophyIds.Length);
            Array.Copy(SceneryTrophyIds,0,ids,MikaTrophyIds.Length,SceneryTrophyIds.Length);
            Array.Copy(FrameTrophyIds,0,ids,MikaTrophyIds.Length+SceneryTrophyIds.Length,FrameTrophyIds.Length);return ids;
        }
        public static bool IsScenery(string id)=>Array.IndexOf(SceneryTrophyIds,id)>=0;
        public static bool IsFrame(string id)=>Array.IndexOf(FrameTrophyIds,id)>=0;
        public static bool IsScreenCosmetic(string id)=>IsScenery(id)||IsFrame(id);
        public static int TrophyPrice(string trophy)
        {
            if(IsParcelExclusive(trophy))return 0;
            int expansion=Array.IndexOf(ExpansionTrophyIds,trophy);if(expansion>=0)return ExpansionPrices[expansion];
            int scenery=Array.IndexOf(SceneryTrophyIds,trophy);if(scenery>=0)return SceneryPrices[scenery];
            int frame=Array.IndexOf(FrameTrophyIds,trophy);if(frame>=0)return FramePrices[frame];
            int index=Array.IndexOf(NewTrophyIds,trophy);
            return index>=0?720+index*360:Array.IndexOf(BonusTrophyIds,trophy)>=0?1440:KeepsakePrice;
        }
        public static int PurchaseClearMilestone(string trophy)=>0;
        // Optional permanent appearances earned by unique account campaign clears.
        // The original four IDs still use their three parcel fragments.
        public static int EarnedClearMilestone(string trophy)
        {
            int index = Array.IndexOf(TrophyIds, trophy);
            return index >= 4 && index < 8 ? (index - 3) * 15 : 0;
        }
        public static int Price(TravelTool tool) => tool == TravelTool.Wayfinder ? 180 : tool == TravelTool.UndoSpool ? 120 : tool == TravelTool.ReturnHourglass ? 300 : tool == TravelTool.SignalLantern ? 240 : tool == TravelTool.RepairKit ? 210 : 150;
        // Lowest historical prices keep cloud inventory from appearing over-budget after rebalance.
        public static int HistoricalPrice(TravelTool tool) => tool == TravelTool.Wayfinder ? 120 : tool == TravelTool.UndoSpool ? 80 : tool == TravelTool.ReturnHourglass ? 200 : tool == TravelTool.SignalLantern ? 160 : tool == TravelTool.RepairKit ? 140 : 100;
        public static int HistoricalTrophyPrice(string trophy)
        {
            if(IsParcelExclusive(trophy))return 0;
            int expansion=Array.IndexOf(ExpansionTrophyIds,trophy);if(expansion>=0)return new[]{720,840,960,1080,1320,1440,600,720,960,1080}[expansion];
            int scenery=Array.IndexOf(SceneryTrophyIds,trophy);if(scenery>=0)return new[]{480,600,720,840,720,960,840,1080,600,720,960,1080}[scenery];
            int frame=Array.IndexOf(FrameTrophyIds,trophy);if(frame>=0)return new[]{720,840,960,1200}[frame];
            int index=Array.IndexOf(NewTrophyIds,trophy);return index>=0?480+index*240:Array.IndexOf(BonusTrophyIds,trophy)>=0?960:240;
        }
        public static bool IsTool(TravelTool tool) => Enum.IsDefined(typeof(TravelTool), tool);
        public static bool TryTool(string id,out TravelTool tool)=>Enum.TryParse(id,out tool)&&IsTool(tool)&&tool.ToString()==id;
    }
}
