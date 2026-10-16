using Cascade.Domain;
namespace Cascade.Application
{
    public static class StageHintBudget
    {
        public static int For(LevelDefinition level) => int.MaxValue;

        // New hint states alternate free/ad after the first free reveal, without a stage allowance cap.
        public static bool RewardedHintDue(int grantedStates,int freeRemaining,bool alreadyRevealed)
            =>!alreadyRevealed&&grantedStates%2==1;
    }
}
