using System;
using System.Linq;
using Cascade.Application;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        Sprite[] completionArt;
        bool parcelReturnsToMap;
        public bool CelebrationMotionAllowed=>applicationFocused&&!applicationPaused&&!nativeAdSuspended&&isActiveAndEnabled;
        public LevelCompletion LastCompletion=>profile.Completion;
        bool IsJourneyStopOpen(int index)=>index>=0&&index<Catalog.PrimaryLevels.Count
            &&(sessionClears.Contains(Catalog.PrimaryLevels[index].Definition.CampaignNodeId)||index==NextJourneyStop());
        public bool IsContentOpen(LevelContent content)
        {
            if(content==null)return false;
            if(!DescentRun.CanDescend(content.Definition))return Catalog.BonusLevels.Contains(content);
            System.Collections.Generic.IReadOnlyList<LevelContent> sequence=content.Mode==PuzzleMode.Shapes?Catalog.PrimaryLevels:
                Catalog.LocalizedLevels.Where(c=>c.Mode==content.Mode&&c.Locale==content.Locale).ToArray();
            return CampaignProgress.IsOpen(sequence.Select(c=>c.Definition.CampaignNodeId).ToArray(),sessionClears,content.Definition.CampaignNodeId);
        }
        Sprite CompletionArt(int index)
        {
            if(completionArt==null){
                var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/CompletionAtlas");if(texture==null)return null;
                // Native alpha bounds of the illustrated atlas, with four pixels of antialiasing gutter.
                var bounds=new[]{new Rect(77,731,470,446),new Rect(612,805,627,258),new Rect(35,78,645,568),new Rect(708,94,517,522)};
                completionArt=bounds.Select(r=>Sprite.Create(texture,new Rect(r.x*texture.width/1254f,r.y*texture.height/1254f,
                    r.width*texture.width/1254f,r.height*texture.height/1254f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect)).ToArray();
            }
            return completionArt[index];
        }
        void DisposeCompletionArt(){if(completionArt!=null)foreach(var sprite in completionArt)if(sprite!=null)Destroy(sprite);}
        TextMeshProUGUI CompletionText(string name,Transform parent,Vector2 min,Vector2 max,float size,UiCopy copy)
        {
            var label=Text(name,parent,min,max,Mathf.RoundToInt(size),copy);label.font=art.titleFont;label.enableAutoSizing=true;
            label.fontSizeMax=size*(Preferences.LargeText?1.1f:1);label.fontSizeMin=size*.78f;label.color=MenuInk;
            label.margin=new Vector4(6,4,6,4);return label;
        }
        void DrawCompletion()
        {
            LoadNavigationArt();
            var attempt=Interaction.Attempt;
            if(profile.Completion?.AttemptId!=attempt.AttemptId.ToString("D")){
                profile.Completion=LevelCompletion.From(attempt,MinimumMoves,descent?.Performance(attempt));PersistProgress();
            }
            var receipt=profile.Completion;
            var ribbon=Picture("Completion ribbon",menuRoot,new Vector2(.06f,.842f),new Vector2(.94f,.978f),CompletionArt(1),true);
            CompletionText("Completion title",ribbon.transform,new Vector2(.16f,.22f),new Vector2(.84f,.80f),25,"Well done!");
            CompletionText("Completed stages",menuRoot,new Vector2(.10f,.79f),new Vector2(.90f,.841f),17,
                new UiCopy(receipt.Stages==3?"Level {0} · all 3 stages complete":"Level {0} complete",receipt.NodeId.TrimStart('C','0')));
            var stars=new Image[3];for(int i=0;i<3;i++){
                float x=.19f+i*.215f;stars[i]=Picture("Earned star "+(i+1),menuRoot,new Vector2(x,.669f),new Vector2(x+.19f,.785f),CompletionArt(0),true);
                if(i>=receipt.Stars)stars[i].color=new Color(.30f,.43f,.41f,.48f);
            }
            MenuReadingSurface("Completion cloth",new Vector2(.08f,.315f),new Vector2(.92f,.648f));
            var hero=Picture("Mika celebrates",menuRoot,new Vector2(.08f,.376f),new Vector2(.36f,.63f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.3f),0,.45f);
            CompletionText("Score label",menuRoot,new Vector2(.36f,.558f),new Vector2(.88f,.618f),18,receipt.NewBest?"New best!":"Score");
            var score=CompletionText("Earned score",menuRoot,new Vector2(.36f,.423f),new Vector2(.88f,.561f),46,UiCopy.Raw(receipt.Score.ToString("N0")));
            CompletionText("Moves statistic",menuRoot,new Vector2(.13f,.336f),new Vector2(.51f,.406f),18,new UiCopy("Moves: {0}",receipt.Moves));
            CompletionText("Undo statistic",menuRoot,new Vector2(.51f,.336f),new Vector2(.87f,.406f),18,new UiCopy("Undos: {0}",receipt.Undos));
            MenuReadingSurface("Earned resources cloth",new Vector2(.08f,.133f),new Vector2(.92f,.287f));
            CompletionText("Resources heading",menuRoot,new Vector2(.12f,.235f),new Vector2(.88f,.282f),17,"You earned");
            CompletionText("Coins heading",menuRoot,new Vector2(.14f,.198f),new Vector2(.46f,.236f),15,"Coins");
            CompletionText("Lights heading",menuRoot,new Vector2(.54f,.198f),new Vector2(.86f,.236f),15,"Lights");
            CompletionText("Earned coins",menuRoot,new Vector2(.14f,.143f),new Vector2(.46f,.202f),25,UiCopy.Raw(receipt.Coins>0?"+"+receipt.Coins:"0"));
            CompletionText("Earned light",menuRoot,new Vector2(.54f,.143f),new Vector2(.86f,.202f),25,UiCopy.Raw(receipt.Lights>0?"+1":"0"));
            CompletionText("Completion reward note",menuRoot,new Vector2(.12f,.100f),new Vector2(.88f,.148f),14,
                receipt.Lights==0?"Replay to improve your stars and score.":WaitingParcel!=null?"A gift is waiting for you!":new UiCopy("New lights for a gift: {0} / 3",Rewards.OrderProgress));
            var next=MenuButton(WaitingParcel!=null?"Collect my gift":"Back to map",.012f,ContinueCompletion,true,.15f,.85f);
            next.name="Continuecompletion";next.GetComponent<Image>().sprite=enamelControls[1];next.GetComponent<Image>().color=Color.white;
            var celebration=menuRoot.gameObject.AddComponent<RewardCelebration>();celebration.Initialize(this,stars,receipt.Stars,score,receipt.Score,CompletionArt(3),false,ribbon.rectTransform);
        }
        public void ContinueCompletion()
        {
            if(CurrentScreen!=GameScreen.Win||profile.Completion==null)return;
            profile.Completion.StatsSeen=true;PersistProgress();if(HasSaveError)return;
            if(WaitingParcel!=null)OpenParcel(true);
            else FinishCompletion();
        }
        void FinishCompletion()
        {
            if(profile.Completion!=null)profile.Completion.Acknowledged=true;
            PersistProgress();if(HasSaveError)return;
            parcelReturnsToMap=false;ShowJourney();TryAutomaticAd();
        }
    }
}
