using Cascade.Domain;
namespace Cascade.Application
{
    public static class StageHintBudget
    {
        public static int For(LevelDefinition level) => level.CampaignNodeId=="C01" ? 3
            : level.InitialSymbolIds.Count>=5 || level.Floors.Count>=3 ? 2 : 1;

        // New hint states alternate free/ad after the first free reveal. Once the
        // stage's free allowance is spent, each further state requires a rewarded hint.
        public static bool RewardedHintDue(int grantedStates,int freeRemaining,bool alreadyRevealed)
            =>!alreadyRevealed&&(freeRemaining<=0||grantedStates>0&&grantedStates%2==1);
    }
}
