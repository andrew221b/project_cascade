using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Application;
using Cascade.Domain;

namespace Cascade.Persistence
{
    // Packaged certificates share the server's recipes; mutable runs never share cached stages.
    public sealed class PublishedTriadStore
    {
        public sealed class Manifest { public string CatalogHash,RecipesHash;public string[] SourceHashes; }
        readonly string catalogHash;
        readonly HashSet<string> sources;
        readonly Dictionary<string,string> recipes;
        public PublishedTriadStore(string recipeJson,string manifestJson)
        {
            var manifest=CloudJson.Decode<Manifest>(manifestJson);
            var stages=CloudJson.Decode<Dictionary<string,DescentStage[]>>(recipeJson);
            if(manifest?.CatalogHash?.Length!=64||manifest.SourceHashes==null||manifest.SourceHashes.Length>3000
                ||manifest.SourceHashes.Any(h=>h?.Length!=64)||manifest.RecipesHash!=Hash(recipeJson)||stages==null||stages.Count!=540)
                throw new ArgumentException("Invalid published campaign certificates.");
            catalogHash=manifest.CatalogHash;sources=new HashSet<string>(manifest.SourceHashes);
            recipes=stages.ToDictionary(pair=>pair.Key,pair=>CloudJson.Encode(pair.Value));
        }
        public static string Hash(string value)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();}
        public DescentRun Bind(LevelDefinition source,string locale,string currentCatalogHash)
        {
            if(source==null||catalogHash!=currentCatalogHash||!sources.Contains(source.ContentHash)
                ||!recipes.TryGetValue(source.CampaignNodeId,out string recipe))return null;
            var run=new DescentRun{Version=6,SourceHash=source.ContentHash,Stages=CloudJson.Decode<DescentStage[]>(recipe)};
            run.Validate(source,locale);return run;
        }
    }
}
