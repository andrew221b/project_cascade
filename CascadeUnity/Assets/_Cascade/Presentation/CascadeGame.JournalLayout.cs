using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        // Physical footer clearance is shared by journal screens; fractions alone
        // let short portrait / Large Text actions overlap the last fact card.
        void FitMetaPanels()
        {
            if(menuRoot==null||menuRoot.rect.height<=0)return;
            float unit=MenuUnit,height=menuRoot.rect.height;
            const float clear=148; // 16 bottom + 48 back + 16 gap + 52 action + 16 gap.
            RectTransform Find(string name)=>menuRoot.Find(name) as RectTransform;
            void Band(string name,float bottom,float size){
                var r=Find(name);if(r==null)return;
                r.anchorMin=new Vector2(r.anchorMin.x,bottom/height);r.anchorMax=new Vector2(r.anchorMax.x,(bottom+size)/height);
                r.offsetMin=r.offsetMax=Vector2.zero;
            }
            float CopyHeight(string name){
                var r=Find(name);if(r==null)return 72*unit;
                var label=r.GetComponentInChildren<TextMeshProUGUI>();
                float width=menuRoot.rect.width*(r.anchorMax.x-r.anchorMin.x)-32*unit;
                return Mathf.Max(72*unit,Mathf.Ceil(label.GetPreferredValues(label.text,width,Mathf.Infinity).y)+24*unit);
            }
            if(CurrentScreen==GameScreen.Records&&Find("Route summary")!=null){
                float top=.815f*height-16*unit;
                Band("Route summary",clear*unit,76*unit);
                float targetBottom=(clear+92)*unit;
                Band("Record target",targetBottom,Mathf.Max(64*unit,top-targetBottom));
            }
            if(CurrentScreen==GameScreen.Contracts&&Find("Contract freedom papyrus")!=null){
                float size=CopyHeight("Contract freedom papyrus");
                Band("Contract freedom papyrus",clear*unit,size);
                float titleBottom=clear*unit+size+16*unit;
                Band("Contract stop papyrus",titleBottom,44*unit);
                float targetBottom=titleBottom+60*unit;
                Band("Stamp target",targetBottom,Mathf.Max(64*unit,.78f*height-targetBottom));
            }
        }
    }
}
