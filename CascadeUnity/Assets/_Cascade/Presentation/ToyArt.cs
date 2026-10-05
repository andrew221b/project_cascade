using System;
using Cascade.Domain;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed class ToyArt : ScriptableObject
    {
        public Mesh[] shapes;
        public Mesh tile, beam, disk, quad;
        public Material[] colors;
        public Material cream, graphite, gold, paleGold, teal, coral, shadow, backdrop;
        public TMP_FontAsset bodyFont, titleFont;
        public Sprite panel;
        public static ToyArt Load() => Resources.Load<ToyArt>("CascadeArt/ToyArt")
            ?? throw new InvalidOperationException("Build CASCADE toy assets before opening the playable.");
        public int ShapeIndex(SymbolId symbol)
        {
            switch (symbol.Value) {
                case "A": case "T": case "triangle": return 0;
                case "B": case "O": case "circle": return 1;
                case "C": case "square": return 2;
                case "D": case "diamond": return 3;
                case "E": case "star": return 4;
                default: return 5;
            }
        }
        public Mesh SymbolMesh(SymbolId symbol) => symbol.Kind == SymbolKind.Glyph ? tile : shapes[ShapeIndex(symbol)];
        public Material SymbolMaterial(SymbolId symbol) => symbol.Kind == SymbolKind.Glyph ? cream : colors[ShapeIndex(symbol)];
    }
}
