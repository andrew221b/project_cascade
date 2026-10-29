using UnityEngine;

namespace Cascade.Presentation
{
    // One coordinate system owns the board, question and both actions. Gap uses
    // logical Canvas pixels, so it stays sixteen on phones and tablets alike.
    public sealed class AgeChoiceLayout:MonoBehaviour
    {
        RectTransform panel,question,yes,no;
        Vector2 lastSize;float lastUnit;
        public void Initialize(RectTransform paper,RectTransform prompt,RectTransform first,RectTransform second)
        {panel=paper;question=prompt;yes=first;no=second;Fit();}
        void OnEnable()=>Canvas.willRenderCanvases+=Fit;
        void OnDisable()=>Canvas.willRenderCanvases-=Fit;
        void LateUpdate()=>Fit();
        void Fit()
        {
            if(panel==null||question==null||yes==null||no==null)return;
            float unit=CenteredContentRow.Unit(panel);
            if(lastSize==panel.rect.size&&Mathf.Abs(lastUnit-unit)<.0001f)return;
            lastSize=panel.rect.size;lastUnit=unit;
            const float promptHeight=96,promptGap=24;
            Vector2 buttonSize=yes.GetComponent<GameButtonDesign>()?.DesiredSize(new Vector2(352,96))??new Vector2(352,96);
            float width=buttonSize.x,height=buttonSize.y;
            float gap=16*unit,stackHeight=promptHeight+promptGap+height*2+gap;
            float y=(panel.rect.height-stackHeight)*.5f;
            Place(question,32,y,panel.rect.width-64,promptHeight);y+=promptHeight+promptGap;
            Place(yes,(panel.rect.width-width)*.5f,y,width,height);y+=height+gap;
            Place(no,(panel.rect.width-width)*.5f,y,width,height);
        }
        static void Place(RectTransform rect,float x,float y,float width,float height)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);
            rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition=new Vector2(x,-y);
        }
    }
}
