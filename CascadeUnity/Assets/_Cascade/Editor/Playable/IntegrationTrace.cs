using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Content;
using Cascade.Domain;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Playable
{
    public static class IntegrationTrace
    {
        static string State(PuzzleState s)=>string.Join("|",string.Join(",",s.Row.Select(v=>v.PersistedId)),s.ActiveFloor,s.Durability,
            s.BombTicks,s.BombPhase.ToString().ToLowerInvariant(),s.KeyUsed?1:0,s.RepairUsed?1:0,s.BridgeUsed?1:0,
            s.StampBits,s.DefusedCount,s.ExplodedCount,s.AbandonedCount,s.Status==PuzzleStatus.Playing?"":s.Status.ToString().ToLowerInvariant());
        public static void Export()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling||EditorUtility.scriptCompilationFailed)throw new InvalidOperationException("Needs completed Edit mode compilation.");
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            string Read(string name)=>File.ReadAllText(Path.Combine(root,"Data",name));
            var catalog=ContentImporter.Import(Read("levels.json"),Read("localized_levels.json"),Read("words_en.json"),Read("words_ru.json"),Read("words_uk.json"));
            var reports=new List<object>();
            foreach(var content in catalog.PrimaryLevels.Concat(catalog.LocalizedLevels)) {
                var resolver=new PuzzleResolver(content.Definition);var initial=resolver.CreateInitialState();
                var queue=new Queue<PuzzleState>();queue.Enqueue(initial);var visited=new HashSet<PuzzleState>{initial};
                var trace=new StringBuilder();trace.Append(State(initial)).Append('\n');int transitions=0,accepted=0;
                while(queue.Count>0) {
                    var before=queue.Dequeue();int count=before.Row.Count;
                    var commands=Enumerable.Range(-1,count+1).Select(PuzzleCommand.Swap).Concat(new[]{PuzzleCommand.Drop});
                    foreach(var command in commands) {
                        var result=resolver.Resolve(before,command);transitions++;if(result.Accepted)accepted++;
                        trace.Append(command.Kind==CommandKind.EarlyDrop?"D":command.Edge.ToString(CultureInfo.InvariantCulture)).Append('|')
                            .Append(result.Accepted?1:0).Append('|').Append(State(result.State)).Append('\n');
                        if(result.Accepted&&visited.Add(result.State))queue.Enqueue(result.State);
                    }
                    if(visited.Count>200000)throw new InvalidOperationException("Trace budget exceeded: "+content.Definition.PuzzleId);
                }
                byte[] bytes=Encoding.UTF8.GetBytes(trace.ToString());using(var hash=SHA256.Create())
                    reports.Add(new{id=content.Definition.PuzzleId,states=visited.Count,transitions,accepted,
                        sha256=BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()});
            }
            string folder=Path.Combine(root,"Reports/P18");Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"csharp-trace-digests.json"),JsonConvert.SerializeObject(reports,Formatting.Indented));
            Debug.Log("CASCADE integration trace: "+reports.Count+" concrete puzzles exported.");
        }
    }
}
