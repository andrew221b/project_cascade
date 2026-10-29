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
        RectTransform conversationRoot,conversationSafe,conversationPlate,conversationName,conversationBust,conversationStage;
        ConversationReferenceLayout conversationLayout;bool storyMenuVisible;int conversationWorld;string conversationReadingLocale;
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
            conversation=scene;conversationLine=conversationPage=0;conversationPages.Clear();conversationFinal=GameButtonDesign.CanonicalCaption(final);conversationDone=done;conversationDismiss=dismiss??done;
            conversationWasPaused=Interaction.IsPaused;Interaction.SetPaused(true);EndTokenHold();HideInstruction();
            conversationRoot=Rect("Story conversation",safeRoot.parent,Vector2.zero,Vector2.one);
            conversationSafe=Rect("Story safe content",conversationRoot,Vector2.zero,Vector2.one);
            storyStatusVisible=status.gameObject.activeSelf;storyPaperVisible=statusPaper.gameObject.activeSelf;storyReceiverVisible=receiver.gameObject.activeSelf;
            if(CurrentScreen==GameScreen.JourneyMap&&journeyFooter!=null){
                storyMapFooter=journeyFooter;storyMapFooterVisible=journeyFooter.gameObject.activeSelf;
                journeyFooter.gameObject.SetActive(false);
            }
            if(CurrentScreen==GameScreen.Checkpoint){status.gameObject.SetActive(false);statusPaper.gameObject.SetActive(false);receiver.gameObject.SetActive(false);resultRoot.gameObject.SetActive(false);}
            BuildConversationReference();
            RefreshConversationLayout();PaintConversationLine();conversationRoot.SetAsLastSibling();
            storyMenuVisible=menuRoot.gameObject.activeSelf;if(storyMenuVisible)menuRoot.gameObject.SetActive(false);
            if(!ReducedMotion){
                var group=conversationRoot.gameObject.AddComponent<CanvasGroup>();group.alpha=.91f;
                conversationArrival=group.DOFade(1,.30f).SetUpdate(true).SetLink(conversationRoot.gameObject);
            }
        }
        void StyleConversationButton(Button button,Sprite surface,Color ink)
        {
            StandardAction(button);
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.font=art.titleFont;label.color=ink;label.textWrappingMode=TextWrappingModes.NoWrap;
            label.margin=new Vector4(16,7,16,7)*MenuUnit;
        }
        internal void RefreshConversationLayout()
        {
            if(conversationPlate==null||conversation==null)return;
            conversationSafe.anchorMin=safeRoot.anchorMin;conversationSafe.anchorMax=safeRoot.anchorMax;
            conversationSafe.offsetMin=safeRoot.offsetMin;conversationSafe.offsetMax=safeRoot.offsetMax;
            // Leave room for TMP glyph overhang at the wrapped paragraph edges.
            conversationBody.margin=new Vector4(4,4,4,4);
            const float textWidth=560;float textSize=Preferences.LargeText?36.72f:34;
            bool reflow=conversationPages.Count==0||conversationReadingWidth!=textWidth||conversationReadingFont!=textSize||conversationReadingLocale!=UiLanguage;
            if(reflow){
                int source=conversationPages.Count==0?0:conversationPages[conversationPage].Source;conversationPages.Clear();
                for(int i=0;i<conversation.Lines.Length;i++){
                    var line=conversation.Lines[i];string copy=(line.Goal==null?new UiCopy(line.Text):new UiCopy(line.Text,new UiCopy(line.Goal))).ToString();
                    conversationPages.Add((i,copy));
                }
                conversationPage=Mathf.Max(0,conversationPages.FindIndex(p=>p.Source==source));
                conversationReadingWidth=textWidth;conversationReadingFont=textSize;conversationReadingLocale=UiLanguage;
            }
            conversationLine=conversationPages[conversationPage].Source;
            conversationBody.fontSize=conversationBody.fontSizeMin=conversationBody.fontSizeMax=textSize;conversationBody.enableAutoSizing=false;
            if(reflow){
                float longest=0;foreach(var line in conversationPages)longest=Mathf.Max(longest,conversationBody.GetPreferredValues(line.Text,textWidth-conversationBody.margin.x-conversationBody.margin.z,Mathf.Infinity).y);
                // Include font ascenders and rounding even when the minimum paper height applies.
                conversationReadingRoom=Mathf.Max(214,Mathf.Ceil(longest))+16;
            }
            float shift=conversationLayout!=null?conversationLayout.BottomShift:0;
            float paperHeight=conversationReadingRoom+128,paperTop=1042-paperHeight+shift;
            ConversationReferenceLayout.PlaceActionPair(conversationNext,conversationSkip,1090+shift);
            ConversationReferenceLayout.Place(conversationPlate,28,paperTop,664,paperHeight);
            conversationPlate.GetComponent<Image>().pixelsPerUnitMultiplier=5;
            bool mika=conversation.Lines[conversationLine].Speaker=="Mika";
            ConversationReferenceLayout.Place(conversationBust,mika?10:130,paperTop-390,mika?700:460,420);
            float pictureWidth=mika&&conversationPortrait.sprite!=null?800*conversationPortrait.sprite.rect.width/conversationPortrait.sprite.rect.height:460;
            ConversationReferenceLayout.Place(conversationPortrait.rectTransform,mika?(700-pictureWidth)*.5f:0,0,pictureWidth,mika?800:420);
            ConversationReferenceLayout.Place(conversationName,36,paperTop-48,330,104);
            ConversationReferenceLayout.Place(conversationBody.rectTransform,52,64,textWidth,conversationReadingRoom);
            ConversationReferenceLayout.Place(conversationCount.rectTransform,548,20,80,44);
            string copyNow=conversationPages[conversationPage].Text;if(conversationBody.text!=copyNow)conversationBody.SetCopy(UiCopy.Raw(copyNow));
            CenteredContentRow.CenterWrappedText(conversationBody,conversationPlate);
            string pageCount=(conversationPage+1)+" / "+conversationPages.Count;if(conversationCount.text!=pageCount)conversationCount.SetCopy(UiCopy.Raw(pageCount));
            var nextLabel=conversationNext.GetComponentInChildren<TextMeshProUGUI>();UiCopy nextCopy=conversationPage==conversationPages.Count-1?conversationFinal:ReferenceCopy("Continue","Далее","Далі");
            if(nextLabel.text!=nextCopy.ToString())nextLabel.SetCopy(nextCopy);
            conversationNext.GetComponent<GameButtonDesign>()?.Refresh();
            conversationSpeaker.GetComponentInParent<CenteredContentRow>()?.Refresh();nextLabel.GetComponentInParent<CenteredContentRow>()?.Refresh();
            conversationSkip.GetComponentInChildren<TextMeshProUGUI>().GetComponentInParent<CenteredContentRow>()?.Refresh();
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
            var line=conversation.Lines[conversationLine];conversationPortrait.sprite=line.Speaker=="Pip"||line.Speaker=="Rue"||line.Speaker=="Mika"?Resources.Load<Sprite>("CascadeArt/Journey/"+line.Speaker):ConversationPortrait(line.Speaker);conversationSpeaker.SetCopy(line.Speaker);
            if(line.Speaker=="Mika")AttachMikaCosmetic(conversationPortrait,null,false);
            else conversationPortrait.GetComponent<MikaCosmeticView>()?.HideForOtherSpeaker();
            conversationBody.SetCopy(UiCopy.Raw(conversationPages[conversationPage].Text));
            CharacterPoseView.Attach(this,conversationPortrait).SetSpeechSource(conversationBody);
            if(conversationLocalization==null){
                conversationLocalization=conversationBody.GetComponent<LocalizeStringEvent>();
                if(conversationLocalization!=null)conversationLocalization.OnUpdateString.AddListener(RefreshConversationCopy);
            }
            RefreshConversationLayout();
            conversationCount.SetCopy(UiCopy.Raw((conversationPage+1)+" / "+conversationPages.Count));
            conversationNext.GetComponentInChildren<TextMeshProUGUI>().SetCopy(conversationPage==conversationPages.Count-1?conversationFinal:ReferenceCopy("Continue","Далее","Далі"));
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
            if(storyMenuVisible&&menuRoot!=null)menuRoot.gameObject.SetActive(true);storyMenuVisible=false;
            if(conversationRoot!=null){conversationRoot.gameObject.SetActive(false);Destroy(conversationRoot.gameObject);}conversationRoot=conversationSafe=conversationPlate=conversationStage=null;conversationLayout=null;
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
