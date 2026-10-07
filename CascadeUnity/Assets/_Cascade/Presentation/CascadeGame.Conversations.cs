using System;
using System.Linq;
using Cascade.Content;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        StoryConversation conversation;
        RectTransform conversationRoot,conversationSafe,conversationPlate,conversationName,conversationBust;
        TextMeshProUGUI conversationBody,conversationSpeaker,conversationCount;
        Button conversationNext,conversationSkip;
        Image conversationPortrait;
        Tween conversationArrival,portraitArrival;
        Sprite[] residents,incumbentBusts;
        Action conversationDone,conversationDismiss;
        UiCopy conversationFinal;
        int conversationLine;
        float conversationAge;
        bool conversationWasPaused;
        bool storyStatusVisible,storyPaperVisible,storyReceiverVisible;
        public bool ConversationOpen=>conversation!=null;
        public int ConversationReceipt=>conversation?.Receipt??-1;
        public int ConversationLine=>conversationLine;

        void OpenConversation(StoryConversation scene,UiCopy final,Action done,Action dismiss=null)
        {
            if(ConversationOpen||scene==null)return;
            conversation=scene;conversationLine=0;conversationFinal=final;conversationDone=done;conversationDismiss=dismiss??done;
            conversationWasPaused=Interaction.IsPaused;Interaction.SetPaused(true);EndTokenHold();HideInstruction();
            conversationRoot=Rect("Story conversation",safeRoot.parent,Vector2.zero,Vector2.one);
            conversationSafe=Rect("Story safe content",conversationRoot,Vector2.zero,Vector2.one);
            storyStatusVisible=status.gameObject.activeSelf;storyPaperVisible=statusPaper.gameObject.activeSelf;storyReceiverVisible=receiver.gameObject.activeSelf;
            if(CurrentScreen==GameScreen.Checkpoint){status.gameObject.SetActive(false);statusPaper.gameObject.SetActive(false);receiver.gameObject.SetActive(false);resultRoot.gameObject.SetActive(false);}
            Background(conversationRoot,new Color(.025f,.09f,.12f,.28f),true);
            conversationPlate=Rect("Speech ribbon",conversationSafe,new Vector2(.035f,.025f),new Vector2(.965f,.025f));
            conversationPlate.pivot=new Vector2(.5f,0);
            var lining=Rect("Opaque enamel lining",conversationPlate,new Vector2(.028f,.045f),new Vector2(.972f,.955f));Background(lining,new Color(.04f,.235f,.245f),true);
            var rim=Rect("Enamel ribbon rim",conversationPlate,Vector2.zero,Vector2.one);var plate=Background(rim,Color.white,false);plate.sprite=enamelControls[0];plate.type=Image.Type.Sliced;
            conversationBust=Rect("Speaker portrait",conversationPlate,new Vector2(.035f,.94f),new Vector2(.31f,1.53f));
            conversationPortrait=conversationBust.gameObject.AddComponent<Image>();conversationPortrait.preserveAspect=true;conversationPortrait.raycastTarget=false;
            conversationName=Rect("Speaker name ribbon",conversationPlate,new Vector2(.29f,.96f),new Vector2(.89f,1.30f));
            var cloth=Background(conversationName,Color.white,false);cloth.sprite=CompletionArt(1);cloth.preserveAspect=true;
            conversationSpeaker=Text("Speaker name",conversationName,new Vector2(.14f,.25f),new Vector2(.86f,.74f),20,"");conversationSpeaker.font=art.titleFont;conversationSpeaker.color=MenuInk;
            conversationBody=Text("Conversation line",conversationPlate,new Vector2(.075f,.32f),new Vector2(.925f,.87f),20,"");
            conversationBody.alignment=TextAlignmentOptions.MidlineLeft;conversationBody.color=new Color(1,.97f,.86f);conversationBody.textWrappingMode=TextWrappingModes.Normal;conversationBody.overflowMode=TextOverflowModes.Overflow;
            conversationCount=Text("Conversation progress",conversationPlate,new Vector2(.76f,.89f),new Vector2(.925f,.965f),14,"");conversationCount.color=new Color(.81f,.92f,.87f);
            conversationNext=Button("Next line",conversationPlate,new Vector2(.51f,.055f),new Vector2(.925f,.265f),AdvanceConversation);
            StyleConversationButton(conversationNext,enamelControls[1],MenuInk);
            conversationSkip=Button("Not now",conversationPlate,new Vector2(.075f,.055f),new Vector2(.43f,.265f),SkipConversation);
            StyleConversationButton(conversationSkip,enamelControls[0],new Color(1,.97f,.86f));
            RefreshConversationLayout();PaintConversationLine();conversationRoot.SetAsLastSibling();
            if(!ReducedMotion){
                var group=conversationRoot.gameObject.AddComponent<CanvasGroup>();group.alpha=.91f;
                conversationPlate.anchoredPosition=new Vector2(0,-24*MenuUnit);
                conversationArrival=DOTween.Sequence().Append(conversationPlate.DOAnchorPos(Vector2.zero,.34f).SetEase(Ease.OutQuint)).Join(group.DOFade(1,.30f)).SetUpdate(true);
            }
        }
        void StyleConversationButton(Button button,Sprite surface,Color ink)
        {
            var image=button.GetComponent<Image>();image.sprite=surface;image.type=Image.Type.Sliced;image.color=Color.white;
            var label=button.GetComponentInChildren<TextMeshProUGUI>();label.font=art.titleFont;label.color=ink;label.textWrappingMode=TextWrappingModes.NoWrap;
        }
        void RefreshConversationLayout()
        {
            if(conversationPlate==null)return;
            conversationSafe.anchorMin=safeRoot.anchorMin;conversationSafe.anchorMax=safeRoot.anchorMax;
            conversationSafe.offsetMin=safeRoot.offsetMin;conversationSafe.offsetMax=safeRoot.offsetMax;
            float width=Mathf.Min(safeRoot.rect.width*.93f,760*MenuUnit);
            conversationPlate.anchorMin=new Vector2(.5f,.025f);conversationPlate.anchorMax=conversationPlate.anchorMin;
            conversationPlate.sizeDelta=new Vector2(width,(Preferences.LargeText?264:244)*MenuUnit);
            conversationBody.fontSize=(Preferences.LargeText?22:20)*MenuUnit;
            conversationSpeaker.fontSize=20*MenuUnit;conversationCount.fontSize=14*MenuUnit;
            foreach(var button in new[]{conversationNext,conversationSkip}){
                var label=button.GetComponentInChildren<TextMeshProUGUI>();label.enableAutoSizing=true;label.fontSizeMin=15*MenuUnit;label.fontSizeMax=18*MenuUnit;
                var rect=(RectTransform)button.transform;rect.sizeDelta=new Vector2(0,Mathf.Max(0,48*MenuUnit-conversationPlate.sizeDelta.y*.21f));
            }
        }
        Sprite ConversationPortrait(string name)
        {
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
            var line=conversation.Lines[conversationLine];conversationPortrait.sprite=ConversationPortrait(line.Speaker);conversationSpeaker.SetCopy(line.Speaker);
            conversationBody.SetCopy(line.Goal==null?new UiCopy(line.Text):new UiCopy(line.Text,new UiCopy(line.Goal)));
            conversationCount.SetCopy(UiCopy.Raw((conversationLine+1)+" / "+conversation.Lines.Length));
            conversationNext.GetComponentInChildren<TextMeshProUGUI>().SetCopy(conversationLine==conversation.Lines.Length-1?conversationFinal:new UiCopy("Next line"));
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
        public void AdvanceConversation()
        {
            if(!ConversationOpen)return;conversationBody.ForceMeshUpdate();
            if(conversationBody.maxVisibleCharacters<conversationBody.textInfo.characterCount){conversationAge=1000;conversationBody.maxVisibleCharacters=int.MaxValue;return;}
            if(conversationLine+1<conversation.Lines.Length){conversationLine++;PaintConversationLine();return;}
            CompleteConversation(false);
        }
        public void SkipConversation(){if(ConversationOpen)CompleteConversation(true);}
        void CompleteConversation(bool dismiss)
        {
            if(conversation.Receipt>=0){heardStory.Add(conversation.Receipt);PersistProgress();if(HasSaveError)return;}
            var next=dismiss?conversationDismiss:conversationDone;CancelConversation();next?.Invoke();
        }
        void CancelConversation()
        {
            if(!ConversationOpen)return;conversationArrival?.Kill();portraitArrival?.Kill();conversationArrival=portraitArrival=null;conversation=null;
            conversationDone=conversationDismiss=null;Interaction?.SetPaused(conversationWasPaused);
            if(CurrentScreen==GameScreen.Checkpoint){status.gameObject.SetActive(storyStatusVisible);statusPaper.gameObject.SetActive(storyPaperVisible);receiver.gameObject.SetActive(storyReceiverVisible);resultRoot.gameObject.SetActive(true);}
            if(conversationRoot!=null){conversationRoot.gameObject.SetActive(false);Destroy(conversationRoot.gameObject);}conversationRoot=conversationSafe=conversationPlate=null;
        }
#if UNITY_EDITOR
        public void ReviewConversation(StoryConversation scene)
        {
            if(!ProfilePath.Contains("CascadeNavigationReview"))throw new InvalidOperationException("An isolated review profile is required.");
            CancelConversation();OpenConversation(scene,"Back to the map",()=>{});
        }
#endif
        void DisposeConversationArt(){foreach(var sprites in new[]{residents,incumbentBusts})if(sprites!=null)foreach(var sprite in sprites)if(sprite!=null)Destroy(sprite);}
        bool TryCheckpointConversation()
        {
            int stop=Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==node);
            if(!journeyActive||!StoryConversations.HasCheckpoint(stop,descent.StageIndex))return false;
            var scene=StoryConversations.Checkpoint(stop);if(heardStory.Contains(scene.Receipt))return false;
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
            var scene=StoryConversations.Island(world,island);if(heardStory.Contains(scene.Receipt))return;
            OpenConversation(scene,"Back to the map",()=>TryAutomaticAd());
        }
        bool CanResumeSelectedJourney(int index)=>index>=0&&index<Catalog.PrimaryLevels.Count&&HasResumableJourneyAttempt
            &&(Interaction.Attempt.RouteLength>0||descent!=null||IsPlanningDescent||IsPlanningDirection||!JourneySequence().Any(c=>Cascade.Application.DeliveryDirector.MatchesSource(Interaction.Attempt,c.Definition)))
            &&Catalog.PrimaryLevels[index].Definition.CampaignNodeId==node;
        void TravelSelectedJourney()
        {
            if(!IsJourneyStopOpen(selectedJourneyStop)){ShowJourney();return;}
            if(CanResumeSelectedJourney(selectedJourneyStop))ResumeJourneyPuzzle();else BeginJourney(selectedJourneyStop);
        }
    }
}
