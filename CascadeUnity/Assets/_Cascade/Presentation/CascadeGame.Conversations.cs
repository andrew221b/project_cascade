using System;
using System.Linq;
using Cascade.Content;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        StoryConversation conversation;
        RectTransform conversationRoot,conversationSafe,conversationPlate,conversationName,conversationBust;
        TextMeshProUGUI conversationBody,conversationSpeaker,conversationCount;
        LocalizeStringEvent conversationLocalization;
        Button conversationNext,conversationSkip;
        Image conversationPortrait;
        Tween conversationArrival,portraitArrival;
        Sprite storyPaper;
        Sprite[] residents,incumbentBusts;
        Action conversationDone,conversationDismiss;
        UiCopy conversationFinal;
        int conversationLine,conversationPage;
        readonly System.Collections.Generic.List<(int Source,string Text)> conversationPages=new System.Collections.Generic.List<(int,string)>();
        float conversationReadingWidth,conversationReadingRoom,conversationReadingFont;
        float conversationAge;
        bool conversationWasPaused;
        bool storyStatusVisible,storyPaperVisible,storyReceiverVisible;
        RectTransform storyMapFooter;
        bool storyMapFooterVisible;
        public bool ConversationOpen=>conversation!=null;
        public int ConversationReceipt=>conversation?.Receipt??-1;
        public int ConversationLine=>conversationLine;

        void OpenConversation(StoryConversation scene,UiCopy final,Action done,Action dismiss=null)
        {
            if(ConversationOpen||scene==null)return;
            conversation=scene;conversationLine=conversationPage=0;conversationPages.Clear();conversationFinal=final;conversationDone=done;conversationDismiss=dismiss??done;
            conversationWasPaused=Interaction.IsPaused;Interaction.SetPaused(true);EndTokenHold();HideInstruction();
            conversationRoot=Rect("Story conversation",safeRoot.parent,Vector2.zero,Vector2.one);
            conversationSafe=Rect("Story safe content",conversationRoot,Vector2.zero,Vector2.one);
            storyStatusVisible=status.gameObject.activeSelf;storyPaperVisible=statusPaper.gameObject.activeSelf;storyReceiverVisible=receiver.gameObject.activeSelf;
            if(CurrentScreen==GameScreen.JourneyMap&&journeyFooter!=null){
                storyMapFooter=journeyFooter;storyMapFooterVisible=journeyFooter.gameObject.activeSelf;
                journeyFooter.gameObject.SetActive(false);
            }
            if(CurrentScreen==GameScreen.Checkpoint){status.gameObject.SetActive(false);statusPaper.gameObject.SetActive(false);receiver.gameObject.SetActive(false);resultRoot.gameObject.SetActive(false);}
            Background(conversationRoot,new Color(.025f,.09f,.12f,.12f),true);
            conversationPlate=Rect("Speech ribbon",conversationSafe,new Vector2(.015f,.018f),new Vector2(.985f,.018f));
            conversationPlate.pivot=new Vector2(.5f,0);
            PaintStoryPaper(conversationPlate,true);
            conversationBust=Rect("Speaker portrait",conversationPlate,new Vector2(.025f,.88f),new Vector2(.27f,1.75f));
            conversationPortrait=conversationBust.gameObject.AddComponent<Image>();conversationPortrait.preserveAspect=true;conversationPortrait.raycastTarget=false;
            conversationName=Rect("Speaker name",conversationPlate,new Vector2(.035f,1),new Vector2(.035f,1));
            conversationName.pivot=new Vector2(0,.5f);conversationName.sizeDelta=new Vector2(120,32)*MenuUnit;
            conversationName.anchoredPosition=new Vector2(2,2)*MenuUnit;
            var nameFrame=Background(conversationName,Color.white,false);nameFrame.sprite=enamelControls[2];nameFrame.type=Image.Type.Sliced;
            conversationSpeaker=Text("Speaker",conversationName,Vector2.zero,Vector2.one,17,"");
            conversationSpeaker.font=art.titleFont;conversationSpeaker.color=MenuInk;conversationSpeaker.alignment=TextAlignmentOptions.Center;
            conversationSpeaker.margin=new Vector4(9,2,9,2)*MenuUnit;conversationSpeaker.textWrappingMode=TextWrappingModes.NoWrap;
            conversationSpeaker.enableAutoSizing=true;conversationSpeaker.fontSizeMin=14*MenuUnit;conversationSpeaker.fontSizeMax=17*MenuUnit;
            conversationBody=Text("Conversation line",conversationPlate,Vector2.zero,Vector2.one,17,"");
            conversationBody.alignment=TextAlignmentOptions.TopLeft;conversationBody.color=MenuInk;conversationBody.textWrappingMode=TextWrappingModes.Normal;conversationBody.overflowMode=TextOverflowModes.Overflow;conversationBody.margin=Vector4.zero;
            conversationCount=Text("Conversation progress",conversationPlate,new Vector2(.78f,.80f),new Vector2(.94f,.96f),12,"");conversationCount.color=new Color(.39f,.35f,.27f);
            conversationNext=Button("Next line",conversationPlate,new Vector2(.75f,0),new Vector2(.95f,0),AdvanceConversation);
            StyleConversationButton(conversationNext,enamelControls[0],MenuPaper);
            conversationSkip=Button("Not now",conversationPlate,new Vector2(.05f,0),new Vector2(.32f,0),SkipConversation);
            StyleConversationButton(conversationSkip,enamelControls[0],MenuPaper);StandardAction(conversationSkip,true);
            RefreshConversationLayout();PaintConversationLine();conversationRoot.SetAsLastSibling();
            if(!ReducedMotion){
                var group=conversationRoot.gameObject.AddComponent<CanvasGroup>();group.alpha=.91f;
                conversationPlate.anchoredPosition=new Vector2(0,-24*MenuUnit);
                conversationArrival=DOTween.Sequence().Append(conversationPlate.DOAnchorPos(Vector2.zero,.34f).SetEase(Ease.OutQuint)).Join(group.DOFade(1,.30f)).SetUpdate(true);
            }
        }
        void StyleConversationButton(Button button,Sprite surface,Color ink)
        {
            StandardAction(button);
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.font=art.titleFont;label.color=ink;label.textWrappingMode=TextWrappingModes.NoWrap;
            label.margin=new Vector4(16,7,16,7)*MenuUnit;
        }
        void RefreshConversationLayout()
        {
            if(conversationPlate==null||conversation==null)return;
            conversationSafe.anchorMin=safeRoot.anchorMin;conversationSafe.anchorMax=safeRoot.anchorMax;
            conversationSafe.offsetMin=safeRoot.offsetMin;conversationSafe.offsetMax=safeRoot.offsetMax;
            float width=Mathf.Min(safeRoot.rect.width*.97f,700*MenuUnit);
            conversationPlate.GetComponent<Image>().pixelsPerUnitMultiplier=8/MenuUnit;
            conversationPlate.anchorMin=new Vector2(.5f,.018f);conversationPlate.anchorMax=conversationPlate.anchorMin;
            float textSize=(Preferences.LargeText?18:17)*MenuUnit;
            conversationBody.fontSize=textSize;conversationBody.enableAutoSizing=false;
            conversationBody.fontSizeMin=textSize;conversationBody.fontSizeMax=textSize;
            float textWidth=width-44*MenuUnit;
            float maximum=Mathf.Max(122*MenuUnit,Mathf.Min((Preferences.LargeText?196:180)*MenuUnit,conversationSafe.rect.height*.31f));
            float room=Mathf.Max(48*MenuUnit,maximum-68*MenuUnit);
            if(conversationPages.Count==0||conversationReadingWidth!=textWidth||conversationReadingRoom!=room||conversationReadingFont!=textSize){
                // Reflow after the actual Canvas changes size, not just at menu construction.
                int source=conversationPages.Count==0?0:conversationPages[conversationPage].Source;
                conversationPages.Clear();
                for(int i=0;i<conversation.Lines.Length;i++){
                    var line=conversation.Lines[i];string copy=(line.Goal==null?new UiCopy(line.Text):new UiCopy(line.Text,new UiCopy(line.Goal))).ToString();
                    conversationPages.Add((i,copy));
                }
                conversationPage=Mathf.Max(0,conversationPages.FindIndex(p=>p.Source==source));
                conversationReadingWidth=textWidth;conversationReadingRoom=room;conversationReadingFont=textSize;
            }
            conversationLine=conversationPages[conversationPage].Source;
            string visibleCopy=conversationPages[conversationPage].Text;
            if(conversationBody.text!=visibleCopy)conversationBody.SetCopy(UiCopy.Raw(visibleCopy));
            string pageCount=(conversationPage+1)+" / "+conversationPages.Count;
            if(conversationCount.text!=pageCount)conversationCount.SetCopy(UiCopy.Raw(pageCount));
            var nextLabel=conversationNext.GetComponentInChildren<TextMeshProUGUI>();
            UiCopy nextCopy=conversationPage==conversationPages.Count-1?conversationFinal:new UiCopy("Next line");
            if(nextLabel.text!=nextCopy.ToString())nextLabel.SetCopy(nextCopy);
            float preferred=conversationBody.GetPreferredValues(visibleCopy,textWidth,Mathf.Infinity).y;
            // A single long authored sentence may grow the strip; it must never be clipped mid-thought.
            conversationPlate.sizeDelta=new Vector2(width,Mathf.Max(138*MenuUnit,preferred+86*MenuUnit));
            conversationBody.rectTransform.anchorMin=Vector2.zero;conversationBody.rectTransform.anchorMax=Vector2.one;
            conversationBody.rectTransform.offsetMin=new Vector2(24*MenuUnit,54*MenuUnit);conversationBody.rectTransform.offsetMax=new Vector2(-20*MenuUnit,-32*MenuUnit);
            conversationBody.lineSpacing=0;conversationBody.paragraphSpacing=0;
            var map=menuRoot.Find("Parallax island world/Map view") as RectTransform;
            if(CurrentScreen==GameScreen.JourneyMap&&map!=null){
                float lower=(conversationPlate.sizeDelta.y+24*MenuUnit)/Mathf.Max(1,menuRoot.rect.height)+.025f;
                map.anchorMin=new Vector2(map.anchorMin.x,lower);
            }
            string localizedName=new UiCopy(conversation.Lines[conversationLine].Speaker).ToString();
            float nameFont=(Preferences.LargeText?17:16)*MenuUnit;
            float nameWidth=conversationSpeaker.GetPreferredValues(localizedName,Mathf.Infinity,Mathf.Infinity).x+26*MenuUnit;
            conversationName.sizeDelta=new Vector2(Mathf.Clamp(nameWidth,120*MenuUnit,width-88*MenuUnit),36*MenuUnit);
            conversationName.anchoredPosition=new Vector2(2,2)*MenuUnit;
            conversationSpeaker.fontSize=nameFont;conversationSpeaker.fontSizeMin=14*MenuUnit;conversationSpeaker.fontSizeMax=nameFont;
            conversationSpeaker.overflowMode=TextOverflowModes.Truncate;conversationCount.fontSize=12*MenuUnit;
            foreach(var button in new[]{conversationNext,conversationSkip}){
                var label=button.GetComponentInChildren<TextMeshProUGUI>();
                label.textWrappingMode=TextWrappingModes.NoWrap;label.alignment=TextAlignmentOptions.Midline;
                label.margin=new Vector4(16,7,16,7)*MenuUnit;label.enableAutoSizing=true;
                label.fontSizeMin=13*MenuUnit;label.fontSizeMax=15*MenuUnit;label.fontSize=15*MenuUnit;
                float desired=label.GetPreferredValues(label.text,Mathf.Infinity,Mathf.Infinity).x+32*MenuUnit;
                float actionWidth=Mathf.Clamp(desired,100*MenuUnit,(width*.90f-12*MenuUnit)*.5f);
                bool advance=button==conversationNext;var rect=(RectTransform)button.transform;
                rect.anchorMin=rect.anchorMax=new Vector2(advance?.95f:.05f,0);
                rect.pivot=new Vector2(advance?1:0,0);rect.sizeDelta=new Vector2(actionWidth,44*MenuUnit);rect.anchoredPosition=new Vector2(0,5*MenuUnit);button.GetComponent<CenteredContentRow>()?.Refresh();
            }
        }
        Sprite StoryPaper()
        {
            if(storyPaper!=null)return storyPaper;
            var strip=Resources.Load<Texture2D>("CascadeArt/Navigation/StoryPaperStrip");
            if(strip!=null){
                // Tight source bounds exclude transparent canvas. Nine-slice keeps the paper curls intact.
                var bounds=new Rect(40f/2172f*strip.width,148f/724f*strip.height,2094f/2172f*strip.width,425f/724f*strip.height);
                float density=strip.width/2172f;
                return storyPaper=Sprite.Create(strip,bounds,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,
                    new Vector4(190,165,190,45)*density);
            }
            var atlas=Resources.Load<Texture2D>("CascadeArt/Workshop/CompletionAtlas");
            if(atlas==null)return art.panel;
            // The quiet woven-paper centre of the existing illustrated ribbon.
            var area=new Rect(850f/1254f*atlas.width,900f/1254f*atlas.height,215f/1254f*atlas.width,100f/1254f*atlas.height);
            return storyPaper=Sprite.Create(atlas,area,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        }
        Image PaintStoryPaper(RectTransform root,bool interactive)
        {
            var paper=Background(root,Color.white,interactive);paper.sprite=StoryPaper();paper.type=Image.Type.Sliced;
            paper.pixelsPerUnitMultiplier=8/ MenuUnit;return paper;
        }
        Sprite ConversationPortrait(string name)
        {
            if(name=="Mika")return MikaBasePortrait();
            if(name=="Aster")return Resources.Load<Sprite>("CascadeArt/Journey/Aster");
            if(residents==null)residents=NavigationSlices("Journey/Residents",3,2);
            int index=Array.IndexOf(new[]{"Bram","Sora","Neri","Toma","Lumi","Gran"},name);if(index>=0)return residents[index];
            if(incumbentBusts==null){
                incumbentBusts=new Sprite[3];string[] names={"Mika","Pip","Rue"};
                var crops=new[]{new Vector4(.17f,.56f,.68f,.44f),new Vector4(.22f,.39f,.60f,.57f),new Vector4(.19f,.39f,.67f,.60f)};
                for(int i=0;i<3;i++){
                    var source=Resources.Load<Sprite>("CascadeArt/Journey/"+names[i]);var texture=source.texture;var crop=crops[i];
                    incumbentBusts[i]=Sprite.Create(texture,new Rect(texture.width*crop.x,texture.height*crop.y,texture.width*crop.z,texture.height*crop.w),new Vector2(.5f,.5f),100);
                }
            }
            return incumbentBusts[name=="Pip"?1:name=="Rue"?2:0];
        }
        void PaintConversationLine()
        {
            RefreshConversationLayout();
            conversationLine=conversationPages[conversationPage].Source;
            var line=conversation.Lines[conversationLine];conversationPortrait.sprite=ConversationPortrait(line.Speaker);conversationSpeaker.SetCopy(line.Speaker);
            if(line.Speaker=="Mika")AttachMikaCosmetic(conversationPortrait,null,true);
            else conversationPortrait.GetComponent<MikaCosmeticView>()?.HideForOtherSpeaker();
            conversationBody.SetCopy(UiCopy.Raw(conversationPages[conversationPage].Text));
            if(conversationLocalization==null){
                conversationLocalization=conversationBody.GetComponent<LocalizeStringEvent>();
                if(conversationLocalization!=null)conversationLocalization.OnUpdateString.AddListener(RefreshConversationCopy);
            }
            RefreshConversationLayout();
            conversationCount.SetCopy(UiCopy.Raw((conversationPage+1)+" / "+conversationPages.Count));
            conversationNext.GetComponentInChildren<TextMeshProUGUI>().SetCopy(conversationPage==conversationPages.Count-1?conversationFinal:new UiCopy("Next line"));
            conversationAge=0;conversationBody.maxVisibleCharacters=ReducedMotion?int.MaxValue:0;
            portraitArrival?.Kill();conversationBust.localScale=ReducedMotion?Vector3.one:Vector3.one*.97f;
            if(!ReducedMotion)portraitArrival=conversationBust.DOScale(1,.28f).SetEase(Ease.OutQuint).SetUpdate(true);
        }
        void TickConversation()
        {
            if(!ConversationOpen||!applicationFocused||applicationPaused)return;
            conversationAge+=Mathf.Min(Time.unscaledDeltaTime,.1f);
            int revealed=Mathf.FloorToInt(conversationAge*42),length=conversationBody.textInfo.characterCount;
            conversationBody.maxVisibleCharacters=ReducedMotion?int.MaxValue:length>0?Mathf.Min(revealed,length):revealed;
        }
        void RefreshConversationCopy(string value)=>RefreshConversationLayout();
        public void AdvanceConversation()
        {
            if(!ConversationOpen)return;conversationBody.ForceMeshUpdate();
            if(conversationBody.maxVisibleCharacters<conversationBody.textInfo.characterCount){conversationAge=1000;conversationBody.maxVisibleCharacters=int.MaxValue;return;}
            if(conversationPage+1<conversationPages.Count){conversationPage++;PaintConversationLine();return;}
            CompleteConversation(false);
        }
        public void SkipConversation(){if(ConversationOpen)CompleteConversation(true);}
        void CompleteConversation(bool dismiss)
        {
            if(conversation.Receipt>=0){heardStory.Add(conversation.Receipt);
                // A completed island's last two pages introduce the next island once.
                int completed=conversation.Receipt-2000;
                if(!dismiss&&conversationLine>=2&&completed>=0&&completed<=84&&completed%10<5&&completed!=84){int nextIsland=completed/10*5+completed%10+1;heardStory.Add(8000+nextIsland/5*10+nextIsland%5);}
                PersistProgress(true);if(HasSaveError)return;}
            var next=dismiss?conversationDismiss:conversationDone;CancelConversation();next?.Invoke();
        }
        void CancelConversation()
        {
            if(!ConversationOpen)return;conversationArrival?.Kill();portraitArrival?.Kill();conversationArrival=portraitArrival=null;conversation=null;
            if(conversationLocalization!=null)conversationLocalization.OnUpdateString.RemoveListener(RefreshConversationCopy);
            conversationLocalization=null;
            conversationDone=conversationDismiss=null;Interaction?.SetPaused(conversationWasPaused);
            if(CurrentScreen==GameScreen.Checkpoint){status.gameObject.SetActive(storyStatusVisible);statusPaper.gameObject.SetActive(storyPaperVisible);receiver.gameObject.SetActive(storyReceiverVisible);resultRoot.gameObject.SetActive(true);}
            if(storyMapFooter!=null)storyMapFooter.gameObject.SetActive(storyMapFooterVisible);storyMapFooter=null;
            var map=menuRoot.Find("Parallax island world/Map view") as RectTransform;if(map!=null)map.anchorMin=new Vector2(map.anchorMin.x,.18f);
            if(conversationRoot!=null){conversationRoot.gameObject.SetActive(false);Destroy(conversationRoot.gameObject);}conversationRoot=conversationSafe=conversationPlate=null;
        }
#if UNITY_EDITOR
        public void ReviewConversation(StoryConversation scene)
        {
            if(!ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("An isolated review profile is required.");
            CancelConversation();OpenConversation(scene,"Back to the map",()=>{});
        }
#endif
        void DisposeConversationArt(){foreach(var sprites in new[]{residents,incumbentBusts})if(sprites!=null)foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);if(storyPaper!=null&&storyPaper!=art.panel)Destroy(storyPaper);}
        bool TryCheckpointConversation()
        {
            int stop=Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node);
            if(!journeyActive||!StoryConversations.HasCheckpoint(stop,descent.StageIndex))return false;
            var scene=StoryConversations.Checkpoint(stop);if(RemembersStory(scene.Receipt))return false;
            OpenConversation(scene,"Descend",GoLower);return true;
        }
        void TryIslandConversation()
        {
            if(ConversationOpen||profile.Completion==null||!profile.Completion.Acknowledged)return;
            var completion=profile.Completion;var firstClear=profile.Clears.FirstOrDefault(c=>c.CampaignNodeId==completion.NodeId);
            if(firstClear==null||firstClear.AttemptId!=completion.AttemptId)return;
            int stop=Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==completion.NodeId);if(stop<0)return;
            int world=JourneyStory.Zone(stop),island=JourneyStory.IslandFor(stop);
            var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            var required=Catalog.PrimaryLevels.Skip(JourneyStory.IslandFirst(world,island)).Take(JourneyStory.IslandLevels(world,island)).Where(c=>offered.Contains(c.Definition.CampaignNodeId)).ToArray();
            if(required.Length==0||required.Any(c=>!sessionClears.Contains(c.Definition.CampaignNodeId)))return;
            var scene=StoryConversations.Island(world,island);if(RemembersStory(scene.Receipt))return;
            OpenConversation(scene,"Back to the map",()=>TryAutomaticAd());
        }
        bool CanResumeSelectedJourney(int index)=>index>=0&&index<Catalog.PrimaryLevels.Count&&HasResumableJourneyAttempt
            &&(Interaction.Attempt.RouteLength>0||descent!=null||IsPlanningDescent||IsPlanningDirection||!JourneySequence().Any(c=>Cascade.Application.DeliveryDirector.MatchesSource(Interaction.Attempt,c.Definition)))
            &&Catalog.PrimaryLevels[index].Definition.CampaignNodeId==node;
        void TravelSelectedJourney()
        {
            if(PersonalMissionActive){ShowGame();return;}
            if(!IsJourneyStopOpen(selectedJourneyStop)){ShowJourney();return;}
            if(CanResumeSelectedJourney(selectedJourneyStop))ResumeJourneyPuzzle();else BeginJourney(selectedJourneyStop);
        }
    }
}
