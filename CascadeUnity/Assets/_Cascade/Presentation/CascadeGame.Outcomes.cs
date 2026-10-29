using System.Collections;
using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        OutcomeCeremony outcome;
        LevelResultTransition resultTransition;
        float outcomeReceiverBottom,outcomeReceiverHeight;
        public OutcomeCeremony Outcome=>outcome;
        void BuildOutcomes()
        {
            var story=Rect("Lantern delivery story",safeRoot,new Vector2(.045f,.41f),new Vector2(.955f,.80f));
            var glow=Rect("Lantern light",story,Vector2.zero,Vector2.one).gameObject.AddComponent<OutcomeLightGraphic>();
            var ribbon=Picture("Stage celebration ribbon",safeRoot,new Vector2(.045f,.79f),new Vector2(.955f,.93f),CompletionArt(1),false);
            var heading=Text("Outcome heading",ribbon.transform,new Vector2(.10f,.25f),new Vector2(.90f,.80f),21,"");heading.enableAutoSizing=true;heading.fontSizeMax=21;heading.fontSizeMin=16;heading.margin=new Vector4(2,2,2,2);heading.wordSpacing=-1.5f;heading.textWrappingMode=TextWrappingModes.NoWrap;StyleCelebrationType(heading,CelebrationType.PaperTitle);
            var shelf=Rect("Mika's outcome ledge",story,new Vector2(.27f,.155f),new Vector2(.73f,.172f));shelf.gameObject.AddComponent<WoodBoardGraphic>();
            var friend=Picture("Mika's outcome",story,new Vector2(.25f,.10f),new Vector2(.75f,.61f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            outcome=gameObject.AddComponent<OutcomeCeremony>();outcome.Initialize(this,story,friend,heading,glow);
        }
        public IEnumerator PlayOutcome(bool success)
        {yield return outcome.Play(success,IsCheckpoint);}
        internal void BeginOutcomeReceiver()
        {
            outcomeReceiverHeight=receiver.rect.height;
            outcomeReceiverBottom=receiver.anchoredPosition.y-outcomeReceiverHeight*.5f;
        }
        public void PlaceOutcomeReceiver(float progress,bool success)
        {
            var canvas=receiver.GetComponentInParent<UnityEngine.Canvas>();if(canvas==null)return;
            float unit=1/Mathf.Min(1,canvas.scaleFactor);
            float target=(success?184:232)*unit+GameplaySillClearance(unit)-8*unit;
            float t=Mathf.Clamp01(progress/.76f),ease=t*t*t*(10+t*(-15+6*t));
            float bottom=Mathf.Lerp(outcomeReceiverBottom,target,ease);
            FixedBand(receiver,bottom,outcomeReceiverHeight);Canvas.ForceUpdateCanvases();machine.Sync();SyncTokenFaces();
        }
        public void SettleOutcome(){if(outcome!=null&&outcome.Running)outcome.Settle();}
    }
}
