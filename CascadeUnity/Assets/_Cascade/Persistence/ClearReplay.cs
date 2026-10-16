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
        public AttemptRecord[] Stages;
        public bool IsBounded()=>SourceHash!=null&&SourceHash.Length==64&&Version==6&&Stages!=null&&Stages.Length==3
            &&Stages.All(s=>s!=null&&s.Commands!=null&&s.Commands.Length<=1000&&s.Commands.All(c=>c!=null&&c.Length<=24));
        public static ClearReplay Capture(DescentRun run,AttemptService final)
        {
            if(run?.Version!=6||!run.IsFinal)return null;
            return new ClearReplay{SourceHash=run.SourceHash,Version=6,Stages=run.Completed.Concat(new[]{AttemptRecord.Capture(final)}).ToArray()};
        }
    }
}
