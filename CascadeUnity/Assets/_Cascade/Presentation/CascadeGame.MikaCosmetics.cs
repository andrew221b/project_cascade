using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly Dictionary<string, Sprite[]> mikaWardrobes = new Dictionary<string, Sprite[]>();
        Sprite mikaBust;

        public void AttachMikaCosmetic(Image image, string id, bool portrait = false)
        {
            if (image == null) return;
            var view = image.GetComponent<MikaCosmeticView>() ?? image.gameObject.AddComponent<MikaCosmeticView>();
            view.Initialize(this, image, id, portrait);
        }

        internal Sprite MikaBadge(string id)
        {
            int index = Array.IndexOf(Cascade.Application.ShopCatalog.TrophyIds, id);
            return index < 0 ? null : TrophyArt(index);
        }

        internal Sprite MikaCostume(string id, bool thinking, bool portrait)
        {
            string asset = id == "sky" ? "SkyCourier" : id == "moon" ? "MoonObserver" : id == "rain_badge" ? "RainExplorer" : null;
            if (asset == null) return null;
            if (!mikaWardrobes.TryGetValue(asset, out var sprites))
            {
                var texture = Resources.Load<Texture2D>("CascadeArt/Cosmetics/" + asset);
                if (texture == null) return null;
                sprites = new Sprite[4];
                for (int pose = 0; pose < 2; pose++)
                {
                    // Each generated pose has a clear centre gutter and a complete silhouette.
                    var cell = new Rect(pose * texture.width * .5f, 0, texture.width * .5f, texture.height);
                    sprites[pose] = Sprite.Create(texture, cell, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                    sprites[pose].name = "Mika " + asset + (pose == 0 ? " hopeful" : " thinking");
                    var bust = new Rect(cell.x + cell.width * .12f, cell.height * .48f, cell.width * .80f, cell.height * .48f);
                    sprites[pose + 2] = Sprite.Create(texture, bust, new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                    sprites[pose + 2].name = "Mika " + asset + " portrait " + pose;
                }
                mikaWardrobes.Add(asset, sprites);
            }
            return sprites[(thinking ? 1 : 0) + (portrait ? 2 : 0)];
        }

        Sprite MikaBasePortrait()
        {
            if (mikaBust == null)
            {
                var texture = Resources.Load<Sprite>("CascadeArt/Journey/Mika").texture;
                mikaBust = Sprite.Create(texture, new Rect(texture.width * .17f, texture.height * .45f,
                    texture.width * .68f, texture.height * .55f), new Vector2(.5f, .5f), 100);
                mikaBust.name = "Mika original portrait";
            }
            return mikaBust;
        }

        void DisposeMikaCosmetics()
        {
            foreach (var variants in mikaWardrobes.Values)
                foreach (var sprite in variants) if (sprite != null) Destroy(sprite);
            mikaWardrobes.Clear();
            if (mikaBust != null) Destroy(mikaBust);
        }
    }
}
