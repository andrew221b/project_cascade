using System.Collections.Generic;
namespace Cascade.Persistence
{
    public static class CloudReplyReconciler
    {
        public static bool CanAcknowledgeApplied(PlayerProfile current,string requestId,long requestSequence,long serverSequence)
            =>requestId!=null&&requestSequence>0&&serverSequence>=requestSequence&&current?.CloudAppliedRequestId==requestId;

        // A recovered journal can precede an operation that the server already accepted.
        // In that case the old pending snapshot is not a trustworthy economic baseline.
        public static CloudMergeResult Apply(PlayerProfile remote,PlayerProfile sent,PlayerProfile current,IReadOnlyList<string> nodes,
            long requestSequence,long serverSequence,bool serverEconomyConflict)
        {
            if(serverEconomyConflict||serverSequence>requestSequence)
                return new CloudMergeResult(CloudReconciler.Merge(remote,current,current,nodes).Profile,true);
            return CloudReconciler.Merge(remote,sent,current,nodes);
        }
    }
}
