using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cascade.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cascade.Content
{
    public static class ContentImporter
    {
        public const string SupportedRulesVersion = "2.0";
        public const string SchemaVersion = "2.0";
        static bool Present(JToken value) => value != null && value.Type != JTokenType.Null;
        static string Required(JObject source, string property)
        {
            var token = source[property];
            if (token == null || token.Type != JTokenType.String || string.IsNullOrWhiteSpace((string)token))
                throw new InvalidDataException("Required string: " + property);
            return (string)token;
        }
        public static string Hash(string text)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }
        static JToken Canonical(JToken token)
        {
            if (token is JObject obj) return new JObject(obj.Properties().OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => new JProperty(p.Name, Canonical(p.Value))));
            if (token is JArray array) return new JArray(array.Select(Canonical));
            return token.DeepClone();
        }

        public static ContentCatalog Import(string primaryJson, string localizedJson, string enWordsJson, string ruWordsJson, string ukWordsJson, string extensionJson=null)
        {
            var primarySources=JArray.Parse(primaryJson);var bindingSources=JArray.Parse(localizedJson);JArray journeyOrder=null;
            if(extensionJson!=null) {
                var extension=JObject.Parse(extensionJson);
                if(!(extension["levels"] is JArray levels)||!(extension["bindings"] is JArray bindings))throw new InvalidDataException("Invalid campaign extension.");
                foreach(var source in levels)primarySources.Add(source.DeepClone());
                foreach(var source in bindings)bindingSources.Add(source.DeepClone());
                if(extension["journey_order"]!=null){journeyOrder=extension["journey_order"] as JArray;if(journeyOrder==null)throw new InvalidDataException("Invalid journey order.");}
            }
            var entries = new Dictionary<string, WordEntry>(StringComparer.Ordinal);
            ImportWords(enWordsJson, "en", entries); ImportWords(ruWordsJson, "ru", entries); ImportWords(ukWordsJson, "uk", entries);
            var words = entries.ToDictionary(p => p.Key, p => p.Value.Definition);
            var primary = new List<LevelContent>(); var localized = new List<LevelContent>();
            var sourceByNode = new Dictionary<string, JObject>(StringComparer.Ordinal);
            var puzzleIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (JObject source in primarySources)
            {
                string id = Required(source, "id");
                if (sourceByNode.ContainsKey(id)) throw new InvalidDataException("Duplicate primary ID: " + id);
                sourceByNode.Add(id, source);
                puzzleIds.Add(id);
                var level = ImportLevel(source, words, Required(source, "rules_version"));
                primary.Add(new LevelContent(level, "en", Required(source, "title"), Required(source, "lesson"), true));
            }
            var bindingKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (JObject source in bindingSources)
            {
                string node = Required(source, "node_id"); string locale = Required(source, "locale");
                if (!puzzleIds.Add(Required(source, "id"))) throw new InvalidDataException("Duplicate PuzzleId.");
                if (!sourceByNode.TryGetValue(node, out var parent)) throw new InvalidDataException("Unknown binding node: " + node);
                if(((int?)source["teaching_node"]??0)!=((int?)parent["teaching_node"]??0))throw new InvalidDataException("Binding teaching anchor differs: "+node);
                if (locale != "en" && locale != "ru" && locale != "uk") throw new InvalidDataException("Unsupported binding locale.");
                if (!bindingKeys.Add(node + ":" + locale)) throw new InvalidDataException("Duplicate localized binding.");
                var level = ImportLevel(source, words, Required(parent, "rules_version"));
                var content = new LevelContent(level, locale, Required(parent, "title"), Required(parent, "lesson"), false);
                string mode = Required(source, "mode");
                if (mode != content.Mode.ToString().ToLowerInvariant()) throw new InvalidDataException("Mode differs from bound symbols.");
                localized.Add(content);
            }
            if (primary.Count == 0) throw new InvalidDataException("Empty primary content.");
            if(journeyOrder!=null){
                var order=journeyOrder.Select(t=>t.Type==JTokenType.String?(string)t:null).ToArray();
                if(order.Length!=primary.Count||order.Any(string.IsNullOrWhiteSpace)||order.Distinct(StringComparer.Ordinal).Count()!=order.Length
                    ||order.Any(id=>!sourceByNode.ContainsKey(id)))throw new InvalidDataException("Journey order must contain every primary node exactly once.");
                var positions=order.Select((id,index)=>new{id,index}).ToDictionary(p=>p.id,p=>p.index,StringComparer.Ordinal);
                primary=primary.OrderBy(c=>positions[c.Definition.CampaignNodeId]).ToList();
                localized=localized.OrderBy(c=>positions[c.Definition.CampaignNodeId]).ToList();
            }
            string bundle = string.Join("\n", new[] { primaryJson, localizedJson, enWordsJson, ruWordsJson, ukWordsJson }
                .Select(text => Canonical(JToken.Parse(text)).ToString(Formatting.None)));
            if(extensionJson!=null)bundle+="\n"+Canonical(JToken.Parse(extensionJson)).ToString(Formatting.None);
            return new ContentCatalog(primary, localized, entries, Hash(bundle));
        }

        // Only HP changed in this revision. Exact pins guard reconstruction against future edits.
        public static IReadOnlyList<LevelContent> ImportLegacyDurability(ContentCatalog catalog, string extensionJson, string manifestJson)
        {
            var extension = JObject.Parse(extensionJson); var manifest = JObject.Parse(manifestJson);
            var legacy = new List<LevelContent>();
            foreach (JObject source in ((JArray)extension["levels"]).Concat((JArray)extension["bindings"])) {
                string id = Required(source,"id"), node = (string)source["node_id"] ?? id;
                if (!(manifest[node] is JObject entry)) continue;
                var previous = (JObject)source.DeepClone(); var hp = (JArray)entry["hp"];
                if (hp.Count != ((JArray)previous["floors"]).Count) throw new InvalidDataException("Legacy floor count differs.");
                for (int i=0;i<hp.Count;i++) previous["floors"][i]["hp"] = hp[i].DeepClone();
                var level = ImportLevel(previous,catalog.Words);
                if (level.ContentHash != (string)entry["pins"][id]) throw new InvalidDataException("Legacy content pin differs: "+id);
                var primary=catalog.GetPrimary(node);
                legacy.Add(new LevelContent(level,(string)source["locale"]??"en",primary.Title,primary.Lesson,source["node_id"]==null));
            }
            return legacy;
        }
        static void ImportWords(string json, string locale, Dictionary<string, WordEntry> entries)
        {
            foreach (JObject source in JArray.Parse(json))
            {
                string id = Required(source, "id");
                if (Required(source, "locale") != locale) throw new InvalidDataException("Word pack locale differs.");
                if (entries.ContainsKey(id)) throw new InvalidDataException("Duplicate WordId: " + id);
                var word = new WordDefinition(id, locale, Required(source, "text"));
                if(word.DisplayText!=word.DisplayText.ToUpperInvariant())throw new InvalidDataException("Word pack must use uppercase display text: "+id);
                if (!(source["tokens"] is JArray tokens) || !tokens.Select(t => (string)t).SequenceEqual(word.Graphemes))
                    throw new InvalidDataException("Word NFC graphemes differ: " + id);
                if (!(source["equality_signature"] is JArray signature) || !signature.Select(t => (int)t).SequenceEqual(word.EqualitySignature))
                    throw new InvalidDataException("Word equality signature differs: " + id);
                entries.Add(id, new WordEntry(word, Required(source, "editorial_status"), Required(source, "origin"), Required(source, "license_review")));
            }
        }

        public static LevelDefinition ImportLevel(JObject source, IReadOnlyDictionary<string, WordDefinition> words, string inheritedRulesVersion = SupportedRulesVersion)
        {
            string id = Required(source, "id"); string node = (string)source["node_id"] ?? id;
            string locale = (string)source["locale"] ?? "en";
            if (locale != "en" && locale != "ru" && locale != "uk") throw new InvalidDataException("Unsupported puzzle locale.");
            string rules = (string)source["rules_version"] ?? inheritedRulesVersion;
            if (rules != SupportedRulesVersion) throw new InvalidDataException("Unsupported rules version: " + rules);
            SymbolId Symbol(JToken value) => SymbolId.Parse((string)value, locale);
            if (!(source["start"] is JArray initial) || !(source["target"] is JArray targetArray) || !(source["floors"] is JArray floorArray))
                throw new InvalidDataException("Missing row/floor arrays: " + id);
            var start = initial.Select(Symbol).ToArray(); var target = targetArray.Select(Symbol).ToArray();
            var segments = BuildSegments(source, target, words, locale);
            var floors = floorArray.Select(x =>
            {
                if (!(x is JObject floor)) throw new InvalidDataException("Missing floor object.");
                int shift = (int?)floor["shift"] ?? 0; bool mirror = (bool?)floor["mirror"] ?? false;
                if (shift < -1 || shift > 1 || (mirror && shift != 0)) throw new InvalidDataException("Exactly one entry transform required.");
                var transform = mirror ? EntryTransform.MirrorNext : shift == -1 ? EntryTransform.ShiftLeftOne : shift == 1 ? EntryTransform.ShiftRightOne : EntryTransform.None;
                if (!(floor["edges"] is JArray edges) || !(floor["fragile"] is JArray fragile)) throw new InvalidDataException("Missing edges.");
                var linked = floor["linked"]; var bomb = floor["bomb"]; var key = floor["key"]; var repair = floor["repair"]; var stamp = floor["stamp"];
                if (Present(linked) && (!(linked is JArray pair) || pair.Count != 2)) throw new InvalidDataException("Expected two linked edges.");
                return new FloorDefinition((int)floor["hp"], edges.Select(v => (bool)v), fragile.Select(v => (int)v),
                    Present(linked) ? new LinkedPair((int)linked[0], (int)linked[1]) : (LinkedPair?)null, transform,
                    Present(bomb) ? new BombDefinition((int)bomb["slot"], Symbol(bomb["symbol"]), (int)bomb["ticks"], (int)bomb["damage"]) : null,
                    Present(key) ? new KeyDefinition((int)key["slot"], Symbol(key["symbol"]), (int)key["value"]) : null,
                    Present(repair) ? new RepairDefinition((int)repair["slot"], Symbol(repair["symbol"]), (int)repair["value"]) : null,
                    (int?)floor["bridge"], Present(stamp) ? new ContactDefinition((int)stamp["slot"], Symbol(stamp["symbol"])) : null);
            }).ToArray();
            string wordPayload = string.Join("\n", segments.Where(s => s.Kind == SegmentKind.Word).Select(s => s.WordId + ":" + words[s.WordId].DisplayText));
            string hash = Hash(rules + "\n" + Canonical(source).ToString(Formatting.None) + "\n" + wordPayload);
            int teachingNode=(int?)source["teaching_node"]??0;
            if(teachingNode<0||teachingNode>184)throw new InvalidDataException("Invalid incumbent teaching anchor: "+id);
            var definition = new LevelDefinition(id, node, SchemaVersion, rules, hash, start, target, segments, floors,teachingNode:teachingNode);
            LevelValidator.ThrowIfInvalid(definition, words);
            return definition;
        }

        static TargetSegment[] BuildSegments(JObject source, SymbolId[] target, IReadOnlyDictionary<string, WordDefinition> words, string locale)
        {
            if (words == null) throw new ArgumentNullException(nameof(words));
            var explicitIds = source["word_ids"] as JArray;
            var ids = explicitIds == null ? null : explicitIds.Select(v => (string)v).ToArray();
            int wordIndex = 0; var segments = new List<TargetSegment>();
            for (int first = 0; first < target.Length;)
            {
                if (target[first].Kind == SymbolKind.Shape)
                {
                    int end = first + 1; while (end < target.Length && target[end].Kind == SymbolKind.Shape) end++;
                    segments.Add(new TargetSegment(SegmentKind.Shapes, first, end - first)); first = end; continue;
                }
                WordDefinition word;
                if (ids != null)
                {
                    if (wordIndex >= ids.Length || !words.TryGetValue(ids[wordIndex], out word)) throw new InvalidDataException("Missing word binding.");
                    wordIndex++;
                }
                else
                {
                    int end = first + 1; while (end < target.Length && target[end].Kind == SymbolKind.Glyph) end++;
                    string text = string.Concat(target.Skip(first).Take(end - first).Select(s => s.Value));
                    word = words.Values.FirstOrDefault(w => w.Locale == locale && w.DisplayText == text);
                    if (word == null) throw new InvalidDataException("No dictionary word matches primary target: " + text);
                }
                if (word.Locale != locale || first + word.Graphemes.Count > target.Length) throw new InvalidDataException("Word binding locale/range differs.");
                segments.Add(new TargetSegment(SegmentKind.Word, first, word.Graphemes.Count, word.WordId)); first += word.Graphemes.Count;
            }
            if (ids != null && wordIndex != ids.Length) throw new InvalidDataException("Unused word binding.");
            return segments.ToArray();
        }
    }
}
