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
        Sprite[] completionArt,rewardEmblems,celebrationPaper,victoryStars,victoryDigits,victorySymbols;
        Sprite victoryMika;
        bool parcelReturnsToMap;
        public bool CelebrationMotionAllowed=>applicationFocused&&!applicationPaused&&!nativeAdSuspended&&isActiveAndEnabled;
        public LevelCompletion LastCompletion=>profile.Completion;
        bool FirstCompletionReturnsToMap=>!PersonalMissionActive&&profile.Completion!=null
            &&JourneySequence().FirstOrDefault()?.Definition.CampaignNodeId==profile.Completion.NodeId
            &&profile.AllClears.Select(c=>c.CampaignNodeId).Distinct().Take(2).Count()==1;
        bool IsPinnedJourneyStop(int index)=>index>=0&&index<Catalog.PrimaryLevels.Count&&HasResumableJourneyAttempt
            &&Catalog.PrimaryLevels[index].Definition.CampaignNodeId==node;
        bool IsJourneyStopOpen(int index)=>IsPinnedJourneyStop(index)||index>=0&&index<Catalog.PrimaryLevels.Count
            &&IsContentOpen(Catalog.PrimaryLevels[index]);
        public bool IsContentOpen(LevelContent content)
        {
            if(content==null)return false;
            if(!DescentRun.CanDescend(content.Definition))return false;
            var sequence=JourneySequence();
            return CampaignProgress.IsOpen(sequence.Select(c=>c.Definition.CampaignNodeId).ToArray(),sessionClears,content.Definition.CampaignNodeId,
                sequence.Where(c=>c.Definition.TeachingNode>0).Select(c=>c.Definition.CampaignNodeId));
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
        Sprite VictoryStar(int index)
        {
            if(victoryStars==null){
                var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/VictoryStarsOct11");if(texture==null)return CompletionArt(0);
                // The generated sheet is full, half and empty, left to right. Crop its transparent gutter once for crisp large UI stars.
                victoryStars=Enumerable.Range(0,3).Select(i=>Sprite.Create(texture,new Rect(i*texture.width/3f,texture.height*.32f,texture.width/3f,texture.height*.36f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect)).ToArray();
            }
            return victoryStars[Mathf.Clamp(index,0,victoryStars.Length-1)];
        }
        Image RatedStar(string name,Transform parent,Vector2 min,Vector2 max,double rating,int slot)
        {
            var star=Picture(name,parent,min,max,VictoryStar(2),true);
            float amount=Mathf.Clamp01((float)rating-slot);
            if(amount>0){
                var fill=Picture("Rating fill "+slot,star.transform,Vector2.zero,Vector2.one,VictoryStar(0),true);
                fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.fillAmount=amount;fill.raycastTarget=false;
            }
            return star;
        }
        Sprite VictoryMika()
        {
            if(victoryMika!=null)return victoryMika;
            // Oct 11 portrait included an opaque studio backdrop, which read as a
            // foggy rectangle over the victory sky. This cutout is the same pose
            // with alpha around Mika and the lantern.
            var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/VictoryMikaLanternCutoutOct12");
            return texture==null?Resources.Load<Sprite>("CascadeArt/Journey/Mika"):(victoryMika=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,0),100,0,SpriteMeshType.FullRect));
        }
        Sprite[] VictoryDigits()
        {
            if(victoryDigits!=null)return victoryDigits;
            var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/VictoryDigitsOct11");if(texture==null)return null;
            // Tight alpha bounds from the generated 5x2 sheet, with a small equal gutter.
            // The previous loose crops included stray atlas pixels and clipped the 0 edge.
            var ink=new[]{new Rect(29,298,225,299),new Rect(299,296,181,299),new Rect(520,297,209,298),new Rect(767,296,210,302),new Rect(997,297,231,301),
                new Rect(27,688,209,285),new Rect(268,684,226,289),new Rect(523,688,212,285),new Rect(758,684,228,290),new Rect(1011,684,217,288)};
            const float gutter=5;float sx=texture.width/1254f,sy=texture.height/1254f;
            victoryDigits=ink.Select(r=>Sprite.Create(texture,new Rect((r.x-gutter)*sx,texture.height-(r.y+r.height+gutter)*sy,
                (r.width+gutter*2)*sx,(r.height+gutter*2)*sy),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect)).ToArray();
            return victoryDigits;
        }
        Sprite[] VictorySymbols()
        {
            if(victorySymbols!=null)return victorySymbols;
            var texture=Resources.Load<Texture2D>("CascadeArt/Workshop/VictorySymbolsOct11");if(texture==null)return null;
            float sx=texture.width/1774f,sy=texture.height/887f;
            victorySymbols=new[]{
                Sprite.Create(texture,new Rect(300*sx,158*sy,570*sx,549*sy),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect),
                Sprite.Create(texture,new Rect(1115*sx,146*sy,298*sx,563*sy),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect),
                Sprite.Create(texture,new Rect(1120*sx,154*sy,288*sx,272*sy),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect)
            };
            return victorySymbols;
        }
        void DisposeCompletionArt(){foreach(var sprites in new[]{completionArt,rewardEmblems,celebrationPaper,resultMedals,victoryStars,victoryDigits,victorySymbols})if(sprites!=null)foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);if(victoryMika!=null)Destroy(victoryMika);DisposeCelebrationTypeStyles();}
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
        TextMeshProUGUI CompletionText(string name,Transform parent,Vector2 min,Vector2 max,float size,UiCopy copy,CelebrationType role=CelebrationType.Information)
        {
            var label=Text(name,parent,min,max,Mathf.RoundToInt(size),copy);label.font=art.titleFont;label.enableAutoSizing=true;
            if(name=="Gift name"){role=CelebrationType.DisplayTitle;size=28;}
            // Typography uses physical UI units, including nested ribbon/medal labels.
            float unit=MenuUnit;
            label.fontSizeMax=size*(Preferences.LargeText?1.1f:1)*unit;
            label.fontSizeMin=size*(role==CelebrationType.VictoryHeadline?.64f:.78f)*unit;label.color=MenuInk;
            label.margin=new Vector4(6,4,6,4);StyleCelebrationType(label,role);return label;
        }
        void CompletionNumber(string name,Transform parent,Vector2 min,Vector2 max,float height,string value)
        {
            var area=Rect(name,parent,min,max);
            var digits=area.gameObject.AddComponent<VictoryScoreDigits>();
            if(digits.Initialize(VictoryDigits(),art.titleFont,area.rect.height*height,VictorySymbols()))digits.SetFormatted(value);
            else {
                Destroy(digits);
                var fallback=CompletionText(name+" fallback",parent,min,max,30,UiCopy.Raw(value),CelebrationType.SkyNumber);
                fallback.alignment=TextAlignmentOptions.Midline;fallback.textWrappingMode=TextWrappingModes.NoWrap;
            }
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
            Background(Rect("Victory sun wash",menuRoot,Vector2.zero,Vector2.one),new Color(1f,.87f,.48f,.12f),true);
            string rank=receipt.StarRating==3?"Brilliant!":receipt.StarRating>=2?"Beautifully done!":"Made it!";
            var ribbon=Picture("Completion ribbon",menuRoot,new Vector2(.045f,.875f),new Vector2(.955f,.995f),CompletionArt(1),false);
            ribbon.rectTransform.anchoredPosition=Vector2.down*(8*MenuUnit);
            var rankLabel=CompletionText("Completion title",ribbon.transform,new Vector2(.10f,.14f),new Vector2(.90f,.86f),27,rank,CelebrationType.VictoryHeadline);
            rankLabel.textWrappingMode=TextWrappingModes.NoWrap;rankLabel.alignment=TextAlignmentOptions.Midline;rankLabel.margin=Vector4.zero;
            rankLabel.rectTransform.anchoredPosition=Vector2.up*(6*MenuUnit);
            rankLabel.fontStyle=FontStyles.Normal;
            rankLabel.gameObject.AddComponent<VictoryHeadlineShimmer>().Initialize(this,rankLabel);
            var stars=new Image[3];for(int i=0;i<3;i++){
                float x=.0475f+i*.305f;stars[i]=Picture("Earned star "+(i+1),menuRoot,new Vector2(x,.6903f),new Vector2(x+.295f,.8547f),VictoryStar(2),true);
            }
            var scoreMedal=Picture("Score medal",menuRoot,new Vector2(.11f,.5285f),new Vector2(.89f,.6985f),ResultMedal(0),false);
            var scoreLabel=CompletionText("Score label",scoreMedal.transform,new Vector2(.08f,.56f),new Vector2(.92f,.76f),20,receipt.NewBest?"New best!":"Score",CelebrationType.EnamelLabel);scoreLabel.margin=Vector4.zero;scoreLabel.textWrappingMode=TextWrappingModes.NoWrap;navigationLightLabels.Add(scoreLabel);
            var score=CompletionText("Earned score",scoreMedal.transform,new Vector2(.12f,.02f),new Vector2(.88f,.55f),56,UiCopy.Raw(receipt.Score.ToString("0")),CelebrationType.Number);score.margin=new Vector4(2,0,2,0);score.gameObject.SetActive(false);
            var scoreGlyphs=Rect("Earned score glyphs",scoreMedal.transform,new Vector2(.12f,.20f),new Vector2(.88f,.52f));
            var digitDisplay=scoreGlyphs.gameObject.AddComponent<VictoryScoreDigits>();digitDisplay.Initialize(VictoryDigits(),art.titleFont,scoreGlyphs.rect.height*.80f,VictorySymbols());
            string[] names={"Time","Moves","Undos"};string[] values={receipt.ActiveSeconds.HasValue?((int)receipt.ActiveSeconds.Value/60).ToString()+":"+((int)receipt.ActiveSeconds.Value%60).ToString("00"):"—",receipt.Moves.ToString(),receipt.Undos.ToString()};int[] medals={3,1,2};
            for(int i=0;i<3;i++){
                float left=.055f+i*.315f;
                var metric=Rect("Award metric "+i,menuRoot,new Vector2(left,.269f),new Vector2(left+.26f,.419f));
                metric.gameObject.AddComponent<CanvasGroup>();
                var medal=Picture(names[i]+" medal",metric,new Vector2(.03f,.30f),new Vector2(.97f,.95f),ResultMedal(medals[i]),true);
                medal.rectTransform.localScale=Vector3.one*.80f;
                CompletionNumber(names[i]+" statistic",metric,new Vector2(.05f,-.01f),new Vector2(.95f,.31f),.62f,values[i]);
                var heading=CompletionText(names[i]+" statistic heading",metric,new Vector2(0,-.19f),new Vector2(1,-.01f),12,names[i]);
                heading.rectTransform.anchoredPosition=Vector2.up*(4*MenuUnit);
                heading.alignment=TextAlignmentOptions.Midline;heading.margin=Vector4.zero;
            }
            if(PersonalMissionActive)CompletionText("Mission thanks",menuRoot,new Vector2(.10f,.166f),new Vector2(.90f,.26f),20,"Thank you! The delivery helped.",CelebrationType.SkyNumber);
            else {
            // Interleave two rewards above the gaps between the three statistic
            // columns. Each icon and its amount move as one visual group.
            Picture("Earned gold illustration",menuRoot,new Vector2(.105f,.397f),new Vector2(.355f,.533f),RewardEmblem(0),true);
            CompletionNumber("Earned coins",menuRoot,new Vector2(.315f,.414f),new Vector2(.56f,.517f),.39f,"×"+receipt.Coins);
            Picture("Earned lights illustration",menuRoot,new Vector2(.555f,.397f),new Vector2(.775f,.533f),RewardEmblem(1),true);
            CompletionNumber("Earned light",menuRoot,new Vector2(.735f,.414f),new Vector2(.935f,.517f),.39f,"×"+receipt.Lights);
            }
            var hero=Picture("Mika victory lantern",menuRoot,new Vector2(.31f,.045f),new Vector2(.69f,.315f),VictoryMika(),true);
            hero.color=Color.white;
            // Keep the authored proportions. Raise the compact cutout as one
            // rigid image; changing only its position cannot stretch Mika.
            hero.rectTransform.localScale=Vector3.one*.72f;
            hero.rectTransform.anchoredPosition=Vector2.up*(10*MenuUnit);
            // Render above the warm screen wash, while reward and statistic UI
            // stays in front of her.
            hero.transform.SetSiblingIndex(scoreMedal.transform.GetSiblingIndex());
            hero.gameObject.AddComponent<WorldMotion>().Initialize(this,new Vector2(0,.45f),0,.26f);
            bool hasParcel=WaitingParcel!=null,hasNext=!FirstCompletionReturnsToMap&&NextCompletionContent()!=null;
            UiCopy primaryLabel=hasParcel?(UiCopy)"Claim trophy":hasNext?(UiCopy)"Next level":(UiCopy)"Back to map";
            Action primaryAction=hasParcel?ContinueCompletion:hasNext?ContinueToNextLevel:ContinueToMap;
            var primary=MenuButton(primaryLabel,.018f,primaryAction,true,hasParcel?.14f:.21f,hasParcel?.86f:.79f);primary.name=hasParcel?"Claim trophy":"Continue completion";
            SetCompletionButtonBottom((RectTransform)primary.transform,24f,62f);
            if(!hasParcel){
                CompletionIconButton("Retry completed level",RetryCompletedLevel,.10f,true);
                if(hasNext)CompletionIconButton("Completion map",ContinueToMap,.90f,false);
            }
            var celebration=menuRoot.gameObject.AddComponent<RewardCelebration>();celebration.Initialize(this,stars,(float)receipt.StarRating,score,receipt.Score,CompletionArt(3),false,ribbon.rectTransform,new[]{VictoryStar(0),VictoryStar(1),VictoryStar(2)},digitDisplay);
        }
        Button CompletionIconButton(string name,Action action,float centerX,bool retryIcon)
        {
            var button=MenuButton(UiCopy.Raw(string.Empty),.018f,action,false,centerX-.085f,centerX+.085f);button.name=name;
            var rect=(RectTransform)button.transform;rect.sizeDelta=new Vector2(0,62*MenuUnit);
            SetCompletionButtonBottom(rect,24f,62f);
            var label=button.GetComponentInChildren<TMP_Text>();if(label!=null)label.gameObject.SetActive(false);
            var face=button.GetComponent<Image>();face.sprite=enamelControls[0];face.type=Image.Type.Sliced;face.color=new Color(1,1,1,0);
            var faceObject=new GameObject("Icon button enamel",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));faceObject.transform.SetParent(button.transform,false);
            var faceRect=(RectTransform)faceObject.transform;faceRect.anchorMin=new Vector2(.04f,.04f);faceRect.anchorMax=new Vector2(.96f,.96f);faceRect.offsetMin=faceRect.offsetMax=Vector2.zero;
            var faceImage=faceObject.GetComponent<Image>();faceImage.sprite=enamelControls[0];faceImage.type=Image.Type.Sliced;faceImage.color=Color.white;faceImage.raycastTarget=false;
            faceObject.AddComponent<PrimaryButtonSkin>().Configure(ParcelArt("AmberAction"));button.targetGraphic=faceImage;button.transition=Selectable.Transition.ColorTint;
            if(retryIcon){
                var glyph=new GameObject("Retry arrow icon",typeof(RectTransform),typeof(CanvasRenderer),typeof(RetryArrowGraphic));glyph.transform.SetParent(button.transform,false);
                var iconRect=(RectTransform)glyph.transform;iconRect.anchorMin=iconRect.anchorMax=new Vector2(.5f,.5f);iconRect.sizeDelta=Vector2.one*36*MenuUnit;
                glyph.GetComponent<RetryArrowGraphic>().color=new Color(.24f,.075f,.012f,1);
            }else Picture("Map pictogram",button.transform,new Vector2(.16f,.16f),new Vector2(.84f,.84f),navigationIcons[4],true);
            return button;
        }
        void SetCompletionButtonBottom(RectTransform rect,float bottom,float height=48)
        {
            for(int i=0;i<menuBottoms.Count;i++)if(menuBottoms[i].rect==rect){menuBottoms[i].bottom=bottom;menuBottoms[i].height=height;return;}
        }
        public void ContinueCompletion()
        {
            if(CurrentScreen!=GameScreen.Win||profile.Completion==null)return;
            if(PersonalMissionActive){RestorePersonalCampaign(true);return;}
            if(FirstCompletionReturnsToMap)profile.Completion.NextNode=null;
            profile.Completion.StatsSeen=true;PersistProgress(true);if(HasSaveError)return;
            if(WaitingParcel!=null)OpenParcel(true);
            else FinishCompletion();
        }
        public void RetryCompletedLevel()
        {
            if(CurrentScreen!=GameScreen.Win||profile.Completion==null)return;
            string completedNode=profile.Completion.NodeId;
            if(PersonalMissionActive){RestorePersonalCampaign(false);if(HasSaveError)return;}
            var content=JourneySequence().FirstOrDefault(c=>c.Definition.CampaignNodeId==completedNode);
            if(content==null)return;
            StartCampaignLevel(content.Definition,content.Locale);if(HasSaveError)return;ShowGame();
        }
        LevelContent NextCompletionContent()
        {
            if(PersonalMissionActive)return null;
            if(profile.Completion==null)return null;
            var sequence=JourneySequence().ToList();int at=sequence.FindIndex(c=>c.Definition.CampaignNodeId==profile.Completion.NodeId);
            return at<0?null:sequence.Skip(at+1).FirstOrDefault(IsContentOpen);
        }
        public void ContinueToNextLevel()
        {
            if(CurrentScreen!=GameScreen.Win||profile.Completion==null)return;
            if(FirstCompletionReturnsToMap){ContinueToMap();return;}
            var next=NextCompletionContent();if(next==null)return;
            profile.Completion.NextNode=next.Definition.CampaignNodeId;ContinueCompletion();
        }
        public void ContinueToMap()
        {
            if(CurrentScreen!=GameScreen.Win||profile.Completion==null)return;
            if(PersonalMissionActive){ContinueCompletion();return;}
            profile.Completion.NextNode=null;ContinueCompletion();
        }
        void FinishCompletion()
        {
            string next=FirstCompletionReturnsToMap?null:profile.Completion?.NextNode;
            if(profile.Completion!=null)profile.Completion.Acknowledged=true;
            PersistProgress(true);if(HasSaveError)return;
            parcelReturnsToMap=false;
            var content=next==null?null:JourneySequence().FirstOrDefault(c=>c.Definition.CampaignNodeId==next&&IsContentOpen(c));
            if(content!=null){
                int index=Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==next);
                if(index>=0){
                    selectedJourneyStop=index;mapPage=JourneyStory.Zone(index);islandBrowseIndex=-1;SetScreen(GameScreen.JourneyMap);
                    if(!TryWorldComic(mapPage,()=>BeginJourney(index)))BeginJourney(index);
                    return;
                }
            }
            ShowJourney();if(!ConversationOpen)TryAutomaticAd();
        }
    }
}
