using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace Cascade.Domain
{
    public enum SymbolKind { Shape, Glyph }

    public readonly struct SymbolId : IEquatable<SymbolId>
    {
        public SymbolKind Kind { get; }
        public string Locale { get; }
        public string Value { get; }
        public string PersistedId { get; }
        public bool IsValid => !string.IsNullOrEmpty(Value);

        private SymbolId(SymbolKind kind, string locale, string value, string persistedId)
        {
            Kind = kind;
            Locale = locale;
            Value = value;
            PersistedId = persistedId;
        }

        public static SymbolId Parse(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new FormatException("Empty symbol ID.");
            var parts = id.Split(':');
            if (parts.Length == 2 && (parts[0] == "s" || parts[0] == "shape"))
            {
                if (string.IsNullOrWhiteSpace(parts[1])) throw new FormatException("Empty shape.");
                return new SymbolId(SymbolKind.Shape, string.Empty, parts[1], id);
            }
            if (parts.Length == 3 && (parts[0] == "g" || parts[0] == "glyph"))
            {
                if (parts[1] != "en" && parts[1] != "ru" && parts[1] != "uk")
                    throw new FormatException("Unsupported puzzle locale: " + parts[1]);
                var graphemes = GraphemeText.Split(parts[2]);
                if (graphemes.Count != 1 || string.IsNullOrWhiteSpace(graphemes[0]))
                    throw new FormatException("A glyph ID must contain one visible grapheme.");
                return new SymbolId(SymbolKind.Glyph, parts[1], graphemes[0], id);
            }
            throw new FormatException("Unknown symbol namespace: " + id);
        }

        public bool Equals(SymbolId other) => Kind == other.Kind && Locale == other.Locale && Value == other.Value;
        public override bool Equals(object obj) => obj is SymbolId other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397 ^ (Locale == null ? 0 : StringComparer.Ordinal.GetHashCode(Locale))) * 397
                    ^ (Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value));
            }
        }
        public static bool operator ==(SymbolId left, SymbolId right) => left.Equals(right);
        public static bool operator !=(SymbolId left, SymbolId right) => !left.Equals(right);
        public override string ToString() => !IsValid ? "invalid" : Kind == SymbolKind.Shape
            ? "shape:" + Value : "glyph:" + Locale + ":" + Value;
    }

    public static class GraphemeText
    {
        public static ReadOnlyCollection<string> Split(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            text = text.Normalize(NormalizationForm.FormC);
            var elements = new List<string>();
            var enumerator = StringInfo.GetTextElementEnumerator(text);
            while (enumerator.MoveNext()) elements.Add(enumerator.GetTextElement());
            return elements.AsReadOnly();
        }
    }
}
