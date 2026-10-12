using System;
using System.Collections.Generic;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Cascade.Editor.Authoring
{
    [Serializable]
    public sealed class ContactDraft
    {
        public bool enabled;
        public int slot;
        public string symbol = "s:A";
        public int value = 1;
        public int ticks = 3;
        public int damage = 1;
        public JObject Source(bool bomb = false) => !enabled ? null : bomb
            ? new JObject { ["slot"] = slot, ["symbol"] = symbol, ["ticks"] = ticks, ["damage"] = damage }
            : new JObject { ["slot"] = slot, ["symbol"] = symbol, ["value"] = value };
        public static ContactDraft Read(JToken source) => source == null || source.Type == JTokenType.Null
            ? new ContactDraft() : new ContactDraft { enabled = true, slot = (int)source["slot"], symbol = (string)source["symbol"],
                value = (int?)source["value"] ?? 1, ticks = (int?)source["ticks"] ?? 3, damage = (int?)source["damage"] ?? 1 };
    }

    [Serializable]
    public sealed class FloorDraft
    {
        public int hp = 3;
        public bool[] open = { true, true };
        public bool[] fragile = { false, false };
        public bool linked;
        public int firstEdge;
        public int secondEdge = 2;
        public EntryTransform entry;
        public bool bridge;
        public int bridgeEdge;
        public ContactDraft bomb = new ContactDraft();
        public ContactDraft key = new ContactDraft();
        public ContactDraft repair = new ContactDraft();
        public ContactDraft stamp = new ContactDraft();

        public void Resize(int edges)
        {
            int old = open.Length; Array.Resize(ref open, edges);
            for (int i = old; i < edges; i++) open[i] = true;
            Array.Resize(ref fragile, edges);
        }
        public JObject Source() => new JObject {
            ["hp"] = hp, ["edges"] = new JArray(open), ["fragile"] = new JArray(fragile.Select((v, i) => new { v, i }).Where(p => p.v).Select(p => p.i)),
            ["linked"] = linked ? new JArray(firstEdge, secondEdge) : null,
            ["shift"] = entry == EntryTransform.ShiftLeftOne ? -1 : entry == EntryTransform.ShiftRightOne ? 1 : 0,
            ["mirror"] = entry == EntryTransform.MirrorNext, ["bridge"] = bridge ? (JToken)bridgeEdge : null,
            ["bomb"] = bomb.Source(true), ["key"] = key.Source(), ["repair"] = repair.Source(), ["stamp"] = stamp.Source() };
        public static FloorDraft Read(JObject source)
        {
            var floor = new FloorDraft { hp = (int)source["hp"], open = source["edges"].Select(t => (bool)t).ToArray(),
                linked = source["linked"] != null && source["linked"].Type != JTokenType.Null,
                bridge = source["bridge"] != null && source["bridge"].Type != JTokenType.Null,
                entry = (bool?)source["mirror"] == true ? EntryTransform.MirrorNext : (int?)source["shift"] == -1 ? EntryTransform.ShiftLeftOne
                    : (int?)source["shift"] == 1 ? EntryTransform.ShiftRightOne : EntryTransform.None,
                bomb = ContactDraft.Read(source["bomb"]), key = ContactDraft.Read(source["key"]),
                repair = ContactDraft.Read(source["repair"]), stamp = ContactDraft.Read(source["stamp"]) };
            floor.fragile = new bool[floor.open.Length];
            foreach (int edge in source["fragile"].Select(t => (int)t)) floor.fragile[edge] = true;
            if (floor.linked) { floor.firstEdge = (int)source["linked"][0]; floor.secondEdge = (int)source["linked"][1]; }
            if (floor.bridge) floor.bridgeEdge = (int)source["bridge"];
            return floor;
        }
    }

    public sealed class LevelDraft : ScriptableObject
    {
        public string puzzleId = "DRAFT_001";
        public string nodeId = "DRAFT_001";
        public string title = "A small detour";
        public string lesson = "swap";
        public string locale = "en";
        public string[] initial = { "s:B", "s:A", "s:C" };
        public string[] target = { "s:A", "s:B", "s:C" };
        public string[] wordIds = Array.Empty<string>();
        public FloorDraft[] floors = { new FloorDraft() };
        public bool requireStamps;
        [SerializeField] string envelope = "{}";

        public void Read(JObject source)
        {
            envelope = source.ToString(); puzzleId = (string)source["id"]; nodeId = (string)source["node_id"] ?? puzzleId;
            title = (string)source["title"] ?? puzzleId; lesson = (string)source["lesson"] ?? "authoring";
            locale = (string)source["locale"] ?? "en";
            initial = source["start"].Select(t => (string)t).ToArray(); target = source["target"].Select(t => (string)t).ToArray();
            wordIds = (source["word_ids"] as JArray)?.Select(t => (string)t).ToArray() ?? Array.Empty<string>();
            floors = source["floors"].Cast<JObject>().Select(FloorDraft.Read).ToArray(); requireStamps = false;
        }
        public void ResizeRow(int count)
        {
            count = Math.Max(3, Math.Min(8, count)); int old = target.Length;
            Array.Resize(ref target, count); Array.Resize(ref initial, count);
            for (int i = old; i < count; i++) target[i] = initial[i] = "s:A";
            foreach (var floor in floors) floor.Resize(count - 1);
        }
        public void ResizeFloors(int count)
        {
            count = Math.Max(1, Math.Min(4, count)); int old = floors.Length; Array.Resize(ref floors, count);
            for (int i = old; i < count; i++) { floors[i] = new FloorDraft(); floors[i].Resize(target.Length - 1); }
        }
        public void ApplyWord(WordDefinition word, int slot)
        {
            if (word.Locale != locale || slot < 0 || slot + word.Graphemes.Count > target.Length)
                throw new ArgumentException("The word must fit the target and use the puzzle language.");
            for (int i = 0; i < word.Graphemes.Count; i++) target[slot + i] = "g:" + locale + ":" + word.Graphemes[i];
            // Rebuild IDs in target order, including repeated/adjacent words, through the dictionary in the window.
            wordIds = Array.Empty<string>();
        }
        public JObject Source()
        {
            var source = JObject.Parse(envelope); source["id"] = puzzleId; source["node_id"] = nodeId;
            source["title"] = title; source["lesson"] = lesson; source["locale"] = locale;
            source["rules_version"] = ContentImporter.SupportedRulesVersion;
            source["start"] = new JArray(initial); source["target"] = new JArray(target);
            source["floors"] = new JArray(floors.Select(f => f.Source()));
            if (wordIds.Length != 0) source["word_ids"] = new JArray(wordIds); else source.Remove("word_ids");
            source.Remove("mode"); return source;
        }
        public LevelDefinition Definition(IReadOnlyDictionary<string, WordDefinition> words) => ContentImporter.ImportLevel(Source(), words);
        public PuzzleGoal Goal() => new PuzzleGoal(requireStamps ? floors.Select((f, i) => f.stamp.enabled ? 1 << i : 0).Aggregate(0, (a, b) => a | b) : 0);
    }
}
