using System;

namespace Cascade.Domain
{
    public sealed class PuzzleGoal
    {
        public static readonly PuzzleGoal Delivery = new PuzzleGoal();
        public int RequiredStampBits { get; }
        public PuzzleGoal(int requiredStampBits = 0)
        {
            if (requiredStampBits < 0 || requiredStampBits > 15)
                throw new ArgumentOutOfRangeException(nameof(requiredStampBits));
            RequiredStampBits = requiredStampBits;
        }
        public bool IsSatisfied(PuzzleState state) => state != null && state.Status == PuzzleStatus.Won
            && (state.StampBits & RequiredStampBits) == RequiredStampBits;
    }
}
