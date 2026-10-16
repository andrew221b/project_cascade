using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform globalCosmeticAmbiance;
        internal bool CosmeticAmbianceMotionAllowed=>CelebrationMotionAllowed&&!MechanicPopupOpen
            &&CurrentScreen!=GameScreen.Pause&&CurrentScreen!=GameScreen.SaveRecovery
            &&previewAd==null&&landingPreview==null;

        public void CosmeticAmbiance(RectTransform parent,string id,bool preview)
        {
            if(parent==null)return;
            string name=preview?"Trophy preview atmosphere":"Equipped trophy atmosphere";
            var layer=parent.Find(name) as RectTransform;
            if(layer==null){
                layer=Rect(name,parent,Vector2.zero,Vector2.one);
                layer.SetAsFirstSibling();
            }
            var view=layer.GetComponent<TrophyAmbiance>()??layer.gameObject.AddComponent<TrophyAmbiance>();
            view.Configure(this,id,preview);
        }
        // The canvas layer remains behind every safe-area control. Rebinding it on
        // navigation/equip changes the appearance without creating another emitter.
        public void CreateGlobalCosmeticAmbiance()
        {
            if(safeRoot==null||profile==null)return;
            var canvas=(RectTransform)safeRoot.parent;
            if(globalCosmeticAmbiance==null){
                globalCosmeticAmbiance=Rect("Cosmetic atmosphere behind the interface",canvas,Vector2.zero,Vector2.one);
                globalCosmeticAmbiance.SetAsFirstSibling();
            }
            CosmeticAmbiance(globalCosmeticAmbiance,EquippedGift,false);
            CosmeticAmbiance(safeRoot.parent as RectTransform,EquippedScenery,false);
            bool opaque=CurrentScreen==GameScreen.Home||CurrentScreen==GameScreen.JourneyMap||CurrentScreen==GameScreen.Shop||CurrentScreen==GameScreen.Supplies||CurrentScreen==GameScreen.Collection||CurrentScreen==GameScreen.CoinPacks||CurrentScreen==GameScreen.TravelBag||CurrentScreen==GameScreen.Parcel||CurrentScreen==GameScreen.Pause;
            globalCosmeticAmbiance.gameObject.SetActive(!opaque);
            var scenery=canvas.Find("Equipped trophy atmosphere");if(scenery!=null)scenery.gameObject.SetActive(!opaque);
            UpdateScreenCosmetics();
        }
    }
}
