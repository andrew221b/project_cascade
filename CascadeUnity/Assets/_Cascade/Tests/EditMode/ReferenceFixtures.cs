using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Domain;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Cascade.Tests
{
    internal static class ReferenceFixtures
    {
        internal static string PackageRoot => Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
        internal static JArray Levels => JArray.Parse(File.ReadAllText(Path.Combine(PackageRoot, "Data", "levels.json")));
        internal static JObject Balance => JObject.Parse(File.ReadAllText(Path.Combine(PackageRoot, "Reports", "balance.json")));
        static bool Present(JToken value) => value != null && value.Type != JTokenType.Null;
        internal static LevelDefinition Parse(JObject source)
        {
            string id = (string)source["id"]; string locale = (string)source["locale"] ?? "en";
            SymbolId Symbol(JToken value) => SymbolId.Parse((string)value, locale);
            var start = source["start"].Select(Symbol).ToArray(); var target = source["target"].Select(Symbol).ToArray();
            var segments = new List<TargetSegment>();
            for (int first = 0; first < target.Length;)
            {
                int end = first + 1; while (end < target.Length && target[end].Kind == target[first].Kind) end++;
                var kind = target[first].Kind == SymbolKind.Shape ? SegmentKind.Shapes : SegmentKind.Word;
                segments.Add(new TargetSegment(kind, first, end - first, kind == SegmentKind.Word ? "fixture_" + id + "_" + first : null));
                first = end;
            }
            var floors = source["floors"].Select(x =>
            {
                int shift = (int?)x["shift"] ?? 0; bool mirror = (bool?)x["mirror"] ?? false;
                if (shift < -1 || shift > 1 || (mirror && shift != 0)) throw new InvalidDataException("Invalid reference transform.");
                var transform = mirror ? EntryTransform.MirrorNext : shift == -1 ? EntryTransform.ShiftLeftOne : shift == 1 ? EntryTransform.ShiftRightOne : EntryTransform.None;
                var bomb = x["bomb"]; var key = x["key"]; var repair = x["repair"]; var stamp = x["stamp"]; var linked = x["linked"];
                return new FloorDefinition((int)x["hp"], x["edges"].Select(v => (bool)v), x["fragile"].Select(v => (int)v),
                    Present(linked) ? new LinkedPair((int)linked[0], (int)linked[1]) : (LinkedPair?)null, transform,
                    Present(bomb) ? new BombDefinition((int)bomb["slot"], Symbol(bomb["symbol"]), (int)bomb["ticks"], (int)bomb["damage"]) : null,
                    Present(key) ? new KeyDefinition((int)key["slot"], Symbol(key["symbol"]), (int)key["value"]) : null,
                    Present(repair) ? new RepairDefinition((int)repair["slot"], Symbol(repair["symbol"]), (int)repair["value"]) : null,
                    (int?)x["bridge"], Present(stamp) ? new ContactDefinition((int)stamp["slot"], Symbol(stamp["symbol"])) : null);
            }).ToArray();
            return new LevelDefinition(id, (string)source["node_id"] ?? id, "2.0", (string)source["rules_version"] ?? "2.0", "fixture:" + id,
                start, target, segments, floors);
        }
    }
}
