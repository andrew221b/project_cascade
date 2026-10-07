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
        Sprite[] completionArt,rewardEmblems,celebrationPaper;
        bool parcelReturnsToMap;
        public bool CelebrationMotionAllowed=>applicationFocused&&!applicationPaused&&!nativeAdSuspended&&isActiveAndEnabled;
        public LevelCompletion LastCompletion=>profile.Completion;
        bool IsPinnedJourneyStop(int index)=>index>=0&&index<Catalog.PrimaryLevels.Count&&journeyActive
            &&Catalog.PrimaryLevels[index].Definition.CampaignNodeId==node&&(Interaction.Attempt.Current.Status==PuzzleStatus.Playing||IsCheckpoint);
        bool IsJourneyStopOpen(int index)=>IsPinnedJourneyStop(index)||index>=0&&index<Catalog.PrimaryLevels.Count
            &&JourneySequence().Any(c=>c.Definition.CampaignNodeId==Catalog.PrimaryLevels[index].Definition.CampaignNodeId)
            &&(sessionClears.Contains(Catalog.PrimaryLevels[index].Definition.CampaignNodeId)||index==NextJourneyStop());
        public bool IsContentOpen(LevelContent content)
        {
            if(content==null)return false;
            if(!DescentRun.CanDescend(content.Definition))return Catalog.BonusLevels.Contains(content);
            var sequence=JourneySequence();
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
        void DisposeCompletionArt(){foreach(var sprites in new[]{completionArt,rewardEmblems,celebrationPaper,resultMedals})if(sprites!=null)foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);}
        public Sprite CelebrationPaper(int index)
        {
            if(celebrationPaper==null)celebrationPaper=NavigationSlices("Workshop/CelebrationPaper",3,2);
            return celebrationPaper[index];
        }
        Sprite RewardEmblem(int index)
        {
            if(rewardEmblems==null)rewardEmblems=NavigationSlices("Workshop/RewardEmblems",2,1);
            return rewardEmblems[index];
        }
        TextMeshProUGUI CompletionText(string name,Transform parent,Vector2 min,Vector2 max,float size,UiCopy copy)
        {
            var label=Text(name,parent,min,max,Mathf.RoundToInt(size),copy);label.font=art.titleFont;label.enableAutoSizing=true;
            label.fontSizeMax=size*(Preferences.LargeText?1.1f:1);label.fontSizeMin=size*.78f;label.color=MenuInk;
            label.margin=new Vector4(6,4,6,4);return label;
        }
        Sprite[] resultMedals;
        Sprite ResultMedal(int index)
        {
            if(resultMedals==null){
                var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/ResultMedals");
                var bounds=new[]{new Rect(50,737,664,357),new Rect(766,693,416,460),new Rect(130,97,447,486),new Rect(735,97,445,489)};
                resultMedals=bounds.Select(r=>Sprite.Create(texture,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect)).ToArray();
            }
            return resultMedals[index];
        }
        void DrawCompletion()
        {
            LoadNavigationArt();
            var attempt=Interaction.Attempt;
            if(profile.Completion?.AttemptId!=attempt.AttemptId.ToString("D")){
                profile.Completion=LevelCompletion.From(attempt,MinimumMoves,descent?.Performance(attempt));PersistProgress();
            }
            var receipt=profile.Completion;
            string rank=receipt.Stars==5?"Brilliant!":receipt.Stars==4?"Beautifully done!":receipt.Stars==3?"Well done!":receipt.Stars==2?"Delivered!":"Made it!";
            var ribbon=Picture("Completion ribbon",menuRoot,new Vector2(.08f,.85f),new Vector2(.92f,.98f),CompletionArt(1),true);
            CompletionText("Completion title",ribbon.transform,new Vector2(.16f,.22f),new Vector2(.84f,.80f),26,rank);
            CompletionText("Completed stages",menuRoot,new Vector2(.08f,.79f),new Vector2(.92f,.842f),17,
                new UiCopy(receipt.Stages==3?"Level {0} · all 3 stages complete":"Level {0} complete",receipt.NodeId.TrimStart('C','0')));
            var stars=new Image[5];for(int i=0;i<5;i++){
                float x=.085f+i*.166f;stars[i]=Picture("Earned star "+(i+1),menuRoot,new Vector2(x,.686f),new Vector2(x+.166f,.784f),CompletionArt(0),true);
                if(i>=receipt.Stars)stars[i].color=new Color(.15f,.29f,.30f,.40f);
            }
            var hero=Picture("Mika celebrates",menuRoot,new Vector2(.065f,.405f),new Vector2(.34f,.666f),Resources.Load<Sprite>("CascadeArt/Journey/Mika"),true);
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,1.3f),0,.45f);
            var scoreMedal=Picture("Score medal",menuRoot,new Vector2(.345f,.452f),new Vector2(.955f,.636f),ResultMedal(0),true);
            var scoreLabel=CompletionText("Score label",scoreMedal.transform,new Vector2(.13f,.53f),new Vector2(.87f,.74f),17,receipt.NewBest?"New best!":"Score");scoreLabel.color=MenuPaper;scoreLabel.margin=new Vector4(2,0,2,0);navigationLightLabels.Add(scoreLabel);
            var score=CompletionText("Earned score",scoreMedal.transform,new Vector2(.16f,.19f),new Vector2(.84f,.56f),42,UiCopy.Raw(receipt.Score.ToString("N0")));score.color=new Color(1,.88f,.46f);score.margin=new Vector4(2,0,2,0);
            var scoreGlint=Rect("Score glint",scoreMedal.transform,Vector2.zero,Vector2.one).gameObject.AddComponent<AwardGlintGraphic>();scoreGlint.Initialize(this,3);scoreGlint.raycastTarget=false;
            CompletionText("Rank guidance",menuRoot,new Vector2(.35f,.405f),new Vector2(.94f,.455f),14,receipt.Stars==5?"A perfect, swift route.":"Aim for fewer moves and undos.");

            string[] names={"Moves","Undos","Time"};string[] values={receipt.Moves.ToString(),receipt.Undos.ToString(),receipt.ActiveSeconds.HasValue?
                ((int)receipt.ActiveSeconds.Value/60).ToString()+":"+((int)receipt.ActiveSeconds.Value%60).ToString("00"):"—"};
            for(int i=0;i<3;i++){
                float left=.07f+i*.29f;
                var metric=Rect("Award metric "+i,menuRoot,new Vector2(left,.278f),new Vector2(left+.28f,.397f));
                metric.gameObject.AddComponent<CanvasGroup>();
                Picture(names[i]+" medal",metric,new Vector2(.02f,.38f),new Vector2(.40f,.97f),ResultMedal(i+1),true);
                var value=CompletionText(names[i]+" statistic",metric,new Vector2(.46f,.38f),new Vector2(1,.97f),25,UiCopy.Raw(values[i]));value.margin=new Vector4(2,0,2,0);
                CompletionText(names[i]+" statistic heading",metric,new Vector2(0,.015f),new Vector2(1,.35f),15,names[i]);
            }
            Picture("Earned gold illustration",menuRoot,new Vector2(.105f,.156f),new Vector2(.285f,.263f),RewardEmblem(0),true);
            CompletionText("Earned coins",menuRoot,new Vector2(.285f,.163f),new Vector2(.465f,.251f),28,UiCopy.Raw(receipt.Coins>0?"+"+receipt.Coins:"0"));
            Picture("Earned lights illustration",menuRoot,new Vector2(.555f,.156f),new Vector2(.735f,.263f),RewardEmblem(1),true);
            CompletionText("Earned light",menuRoot,new Vector2(.735f,.163f),new Vector2(.915f,.251f),28,UiCopy.Raw(receipt.Lights>0?"+1":"0"));
            CompletionText("Completion reward note",menuRoot,new Vector2(.08f,.104f),new Vector2(.92f,.153f),14,
                receipt.Lights==0?"Your stars and score are saved for this mode.":WaitingParcel!=null?"A gift is waiting for you!":new UiCopy("New lights for a gift: {0} / 3",Rewards.OrderProgress));
            var next=MenuButton(WaitingParcel!=null?"Collect my gift":"Back to map",.012f,ContinueCompletion,true,.12f,.88f);
            next.name="Continuecompletion";
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
            parcelReturnsToMap=false;ShowJourney();if(!ConversationOpen)TryAutomaticAd();
        }
    }
}
