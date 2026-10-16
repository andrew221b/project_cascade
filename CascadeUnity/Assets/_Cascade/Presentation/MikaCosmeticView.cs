using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // One worn appearance follows its Image, including pose changes in the loss ceremony.
    // It reads ownership/equipment; it never buys, equips, advances or mutates the puzzle.
    public sealed class MikaCosmeticView : MonoBehaviour
    {
        CascadeGame game;
        Image hero, badge;
        Sprite hopeful, thinking, original;
        string preview;
        bool portrait, thoughtful;
        float pokeTime,previewBadgeScale=1,bodyBadgeScale=1;Vector2? badgeAnchor;
        public void SetBadgeAnchor(Vector2 value,float scale=1){badgeAnchor=value;bodyBadgeScale=Mathf.Clamp(scale,1,1.5f);Apply();}
        public Image PreviewBadge=>badge;
        public void SetPreviewBadgeScale(float value){previewBadgeScale=Mathf.Clamp(value,1,2);Apply();}

        public string DisplayedTrophy { get; private set; }
        public bool WearsCostume { get; private set; }

        public void Initialize(CascadeGame owner, Image image, string previewId, bool isPortrait)
        {
            game = owner; hero = image; preview = previewId; portrait = isPortrait;
            hopeful = Resources.Load<Sprite>("CascadeArt/Journey/Mika");
            thinking = Resources.Load<Sprite>("CascadeArt/Journey/MikaThinking");
            if (original == null || image.sprite == hopeful || image.sprite == thinking || isPortrait) original = image.sprite;
            thoughtful = original == thinking;
            enabled = true;
            if (badge == null)
            {
                var go = new GameObject("Mika worn trophy", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(hero.transform, false);
                badge = go.GetComponent<Image>(); badge.raycastTarget = false; badge.preserveAspect = true;
                badge.type = Image.Type.Simple;
                badge.rectTransform.anchorMin = badge.rectTransform.anchorMax = new Vector2(.5f, .5f);
            }
            Apply();
        }

        public void HideForOtherSpeaker()
        {
            enabled = false;
            if (badge != null) badge.gameObject.SetActive(false);
        }

        public void Poke() { if(preview!=null)pokeTime=.62f; }

        void LateUpdate() { Apply(); }

        void Apply()
        {
            if (game == null || hero == null || original == null) return;
            if (!portrait && (hero.sprite == hopeful || hero.sprite == thinking))
            {
                thoughtful = hero.sprite == thinking;
                original = hero.sprite;
            }
            string id = preview ?? game.EquippedGift;
            var costume = game.MikaCostume(id, thoughtful, portrait);
            hero.sprite = costume != null ? costume : original;
            if(preview!=null){
                float ambient=game.ReducedMotion?0:Mathf.Sin(game.Machine.AmbientPhase*.86f)*.012f;
                float hop=game.ReducedMotion?0:Mathf.Sin((1-pokeTime/.62f)*Mathf.PI)*.085f*(pokeTime>0?1:0);
                hero.rectTransform.localScale=Vector3.one*(1+ambient+hop);
                if(pokeTime>0)pokeTime=Mathf.Max(0,pokeTime-Time.unscaledDeltaTime);
            }
            DisplayedTrophy = id; WearsCostume = costume != null;
            var pin = costume == null ? game.MikaBadge(id) : null;
            badge.gameObject.SetActive(pin != null);
            if (pin == null) return;
            badge.sprite = pin;
            badge.material=hero.material;
            var area = hero.rectTransform.rect.size;
            var source = hero.sprite.rect.size;
            float scale = Mathf.Min(area.x / source.x, area.y / source.y);
            var drawn = source * Mathf.Max(0, scale);
            var point = portrait ? new Vector2((.72f - .17f) / .68f, (.59f - .45f) / .55f)
                : thoughtful ? new Vector2(.735f, .586f) : badgeAnchor??new Vector2(.72f, .59f);
            badge.rectTransform.anchoredPosition = new Vector2(drawn.x * (point.x - .5f), drawn.y * (point.y - .5f));
            float size = Mathf.Max(5, drawn.y * (portrait ? .12f : .072f) * (preview!=null?previewBadgeScale:bodyBadgeScale));
            badge.rectTransform.sizeDelta = Vector2.one * size;
            // The badge is sewn to cloth; its body follows Mika's motion, with a small material glint.
            float pulse = game.ReducedMotion || game.Preferences.EffectQuality == 0 ? 0 : Mathf.Sin(game.Machine.AmbientPhase * .7f) * .025f;
            badge.color = new Color(1, 1 - Mathf.Max(0, pulse), 1 - Mathf.Max(0, pulse), hero.color.a);
        }
    }
}
