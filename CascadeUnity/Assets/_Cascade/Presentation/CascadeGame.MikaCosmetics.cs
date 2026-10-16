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
        Sprite[] parcelBrooches;
        Sprite ParcelBrooch(string id)
        {
            int index=Array.IndexOf(Cascade.Application.ShopCatalog.ParcelTrophyIds,id);if(index<0)return null;
            if(parcelBrooches==null)parcelBrooches=NavigationSlices("Cosmetics/ParcelBrooches",4,2);
            return parcelBrooches[index];
        }

        public void AttachMikaCosmetic(Image image, string id, bool portrait = false)
        {
            if (image == null) return;
            var view = image.GetComponent<MikaCosmeticView>() ?? image.gameObject.AddComponent<MikaCosmeticView>();
            view.Initialize(this, image, id, portrait);
        }

        internal Sprite MikaBadge(string id)
        {
            int index = Array.IndexOf(Cascade.Application.ShopCatalog.MikaTrophyIds, id);
            return index < 0 ? null : TrophyArt(index);
        }

        internal Sprite MikaCostume(string id, bool thinking, bool portrait)
        {
            string asset = id == "sky" ? "SkyCourier" : id == "moon" ? "MoonObserver" : id == "rain_badge" ? "RainExplorer"
                : id == "sky_pilot" ? "SkyPilot" : id == "moth_mask" ? "MothMask"
                : id == "garden_festival" ? "GardenFestival" : id == "cloud_gardener" ? "CloudGardener"
                : id == "explorer_hat" ? "ExplorerHat" : id == "frog_suit" ? "FrogSuit"
                : id == "star_knight" ? "StarKnight" : id == "fox_mask" ? "FoxMask"
                : id == "mushroom_cap" ? "MushroomCap" : id == "bee_suit" ? "BeeSuit"
                : id == "winter_hood" ? "WinterHood" : id == "pirate_hat" ? "PirateHat"
                : id == "jester_suit" ? "JesterSuit" : id == "constellation_mage" ? "ConstellationMage" : null;
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
                    // Hats, mask tassels and ears must remain inside every portrait variant.
                    var bust = new Rect(cell.x, cell.height * .45f, cell.width, cell.height * .55f);
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
            if(parcelBrooches!=null)foreach(var sprite in parcelBrooches)if(sprite!=null)Destroy(sprite);
            if (mikaBust != null) Destroy(mikaBust);
            if (unavailableTrophyMaterial != null) Destroy(unavailableTrophyMaterial);
            if (ownedTrophyMaterial != null) Destroy(ownedTrophyMaterial);
            foreach(var sprite in noAdsReferenceSprites.Values)if(sprite!=null&&sprite.name.StartsWith("Runtime subscription illustration ",System.StringComparison.Ordinal))Destroy(sprite);
        }
    }
}
