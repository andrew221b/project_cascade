using System;
using System.Linq;
using Cascade.Application;

namespace Cascade.Persistence
{
    // Optional evidence for new clears; old saves remain readable and playable.
    public sealed class ClearReplay
    {
        public string SourceHash;
        public int Version;
        public Cascade.Domain.StageVariation Variation;
        public AttemptRecord[] Stages;
        public bool IsBounded()=>SourceHash!=null&&SourceHash.Length==64&&(Version==6&&Variation==null||Version==7&&Variation?.IsBounded()==true)&&Stages!=null&&Stages.Length==3
            &&Stages.All(s=>s!=null&&s.Commands!=null&&s.Commands.Length<=1000&&s.Commands.All(c=>c!=null&&c.Length<=24));
        public static ClearReplay Capture(DescentRun run,AttemptService final)
        {
            if(run==null||(run.Version!=6&&run.Version!=7)||!run.IsFinal)return null;
            return new ClearReplay{SourceHash=run.SourceHash,Version=run.Version,Variation=run.Variation,Stages=run.Completed.Concat(new[]{AttemptRecord.Capture(final)}).ToArray()};
        }
    }
}
