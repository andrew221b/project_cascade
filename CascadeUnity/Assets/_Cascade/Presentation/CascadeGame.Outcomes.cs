using System.Collections;
using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        OutcomeCeremony outcome;
        public OutcomeCeremony Outcome=>outcome;
        void BuildOutcomes()
        {
            var story=Rect("Lantern delivery story",safeRoot,new Vector2(.10f,.41f),new Vector2(.90f,.80f));
            var glow=Rect("Lantern light",story,Vector2.zero,Vector2.one).gameObject.AddComponent<OutcomeLightGraphic>();
            var heading=Text("Outcome heading",story,new Vector2(.02f,.81f),new Vector2(.98f,1),30,"");heading.font=art.titleFont;
            var shelf=Rect("Mika's outcome ledge",story,new Vector2(.27f,.155f),new Vector2(.73f,.172f));shelf.gameObject.AddComponent<WoodBoardGraphic>();
            var friend=Picture("Mika's outcome",story,new Vector2(.22f,.10f),new Vector2(.78f,.79f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            outcome=gameObject.AddComponent<OutcomeCeremony>();outcome.Initialize(this,story,friend,heading,glow);
        }
        public IEnumerator PlayOutcome(bool success)
        {machine.LivingWorld.SetQuietFlight(!IsCheckpoint);yield return outcome.Play(success,IsCheckpoint);}
        public void PlaceOutcomeReceiver(float progress,bool success)
        {
            var canvas=receiver.GetComponentInParent<UnityEngine.Canvas>();if(canvas==null)return;
            float unit=1/Mathf.Min(1,canvas.scaleFactor);
            float bottom=Mathf.Lerp(82,success?184:232,Mathf.SmoothStep(0,1,Mathf.Clamp01(progress/.76f)));
            FixedBand(receiver,bottom*unit,100);Canvas.ForceUpdateCanvases();machine.Sync();SyncTokenFaces();
        }
        public void SettleOutcome(){if(outcome!=null&&outcome.Running)outcome.Settle();}
    }
}
