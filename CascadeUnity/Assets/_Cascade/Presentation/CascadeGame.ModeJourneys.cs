using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Content;
using Cascade.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform journeyModeTabs;
        bool changingJourneyMode;
        StoryConversation parallelArrival;
        bool RemembersStory(int receipt)=>heardStory.Contains(receipt)||profile.RemembersStory(receipt);
        bool TryParallelArrival()
        {
            if(parallelArrival==null)return false;
            var scene=parallelArrival;parallelArrival=null;
            SetScreen(GameScreen.JourneyMap);OpenConversation(scene,"To the map",ShowSelectedJourney);return true;
        }
        internal Vector2 MapAmbientOffset;
        public bool CanSwitchJourneyMode=>!ConversationOpen&&!saveBlocked&&!nativeAdSuspended&&previewAd==null&&!IsDescending&&!IsPlanningDescent
            &&Interaction!=null&&!Interaction.IsPresenting&&(profile.Completion==null||profile.Completion.Acknowledged);
        void SwitchJourneyMode(PuzzleMode mode)
        {
            if(!CanSwitchJourneyMode)return;
            if(mode==PreferredMode){ShowJourney();return;}
            PersistProgress(true);if(HasSaveError)return;
            bool knowsOtherSky=profile.HasStoryMemory;
            bool returning=profile.ModeJourneys.Any(j=>j.Mode==mode.ToString());
            var oldMode=PreferredMode;var oldJourney=ModeJourney.Capture(profile);var oldSlots=profile.ModeJourneys.ToList();
            AttemptService restored;
            try {
                profile.SelectJourney(mode.ToString());PreferredMode=mode;
                EnsureSavedJourneyContent();
                restored=profile.RestoreAttempt(Catalog,legacyContent);
                if(restored==null){var first=PreferredLevels().First();restored=new AttemptService(first.Definition,first.Locale);profile.Attempt=AttemptRecord.Capture(restored);profile.JourneyAttempt=true;}
                // Persist the entire switch before changing the displayed puzzle. A write failure keeps the old tab.
                profileStore.Save(profile);HasSaveError=false;
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is InvalidOperationException||e is ArgumentException){
                oldJourney.Apply(profile);profile.ModeJourneys=oldSlots;profile.PreferredMode=oldMode.ToString();PreferredMode=oldMode;HasSaveError=true;
                if(status!=null)status.SetCopy("Could not save. Keep the game open and try again.");
                return;
            }
            sessionClears.Clear();sessionClears.UnionWith(profile.Clears.Select(c=>c.CampaignNodeId));
            heardStory.Clear();heardStory.UnionWith(profile.HeardStory);firstGestureShown=profile.FirstGestureShown;
            changingJourneyMode=true;
            try {
                MountAttempt(restored,profile.Descent);journeyActive=profile.JourneyAttempt;storyRoute=false;
                chapterPage=0;selectedJourneyStop=NextJourneyStop();
                var arrival=StoryConversations.ParallelSky(mode,JourneyStory.Zone(selectedJourneyStop),returning);
                if(knowsOtherSky&&!RemembersStory(arrival.Receipt))parallelArrival=arrival;
                ShowJourney();
            } finally {changingJourneyMode=false;}
        }
        void DrawJourneyModeTabs()
        {
            var row=Rect("Map mode tabs",menuRoot,new Vector2(.11f,.837f),new Vector2(.89f,.837f));
            journeyModeTabs=row;row.pivot=new Vector2(.5f,1);row.sizeDelta=new Vector2(0,40*MenuUnit);
            var modes=new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed};
            for(int i=0;i<modes.Length;i++){
                var mode=modes[i];bool active=mode==PreferredMode;
                var tab=Button(mode.ToString(),row,new Vector2(i/3f+.015f,0),new Vector2((i+1)/3f-.015f,1),()=>SelectMode(mode));tab.name="Mapmode"+mode;
                var face=tab.GetComponent<Image>();face.sprite=enamelControls[active?1:0];face.type=Image.Type.Sliced;face.color=Color.white;
                var label=tab.GetComponentInChildren<TextMeshProUGUI>();label.font=art.titleFont;label.margin=new Vector4(7,3,7,3);label.enableAutoSizing=true;
                label.fontSizeMin=14*MenuUnit;label.fontSizeMax=16*MenuUnit;label.textWrappingMode=TextWrappingModes.NoWrap;
                if(!active)navigationLightLabels.Add(label);tab.interactable=active||CanSwitchJourneyMode;
            }
            var stars=Rect("Map star balance",menuRoot,new Vector2(.5f,.9625f),new Vector2(.5f,.9625f));stars.sizeDelta=new Vector2(30,30)*MenuUnit;
            Picture("Mode earned stars",stars,new Vector2(.02f,0),new Vector2(.29f,1),CompletionArt(0),true);
            var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            var total=CompletionText("Mode stars total",stars,new Vector2(.32f,0),new Vector2(.98f,1),18,UiCopy.Raw(LevelCompletion.FormatStars(profile.Clears.Where(c=>offered.Contains(c.CampaignNodeId)).Sum(c=>c.BestStarRating))),CelebrationType.SkyNumber);
            total.alignment=TextAlignmentOptions.MidlineLeft;total.margin=Vector4.zero;total.textWrappingMode=TextWrappingModes.NoWrap;
            FitMapStarBalance(stars,total);
        }
        void FitMapStarBalance(RectTransform stars,TextMeshProUGUI total)
        {
            // Emblem and value share the central axis; asymmetric digits no longer pull it sideways.
            total.enableAutoSizing=false;total.fontSize=18*MenuUnit;
            float width=total.GetPreferredValues(total.text,1000,40*MenuUnit).x;
            stars.pivot=new Vector2(.5f,.5f);stars.sizeDelta=Vector2.one*30*MenuUnit;
            var icon=(RectTransform)stars.Find("Mode earned stars");icon.anchorMin=icon.anchorMax=new Vector2(0,.5f);
            icon.pivot=new Vector2(0,.5f);icon.anchoredPosition=Vector2.zero;icon.sizeDelta=Vector2.one*30*MenuUnit;
            var digits=total.rectTransform;digits.anchorMin=digits.anchorMax=new Vector2(0,.5f);
            digits.pivot=new Vector2(.5f,.5f);digits.anchoredPosition=new Vector2(15,-25)*MenuUnit;digits.sizeDelta=new Vector2(width,22*MenuUnit);
        }
        void FitJourneyModeTabs()
        {
            if(journeyModeTabs==null)return;
            journeyModeTabs.sizeDelta=new Vector2(0,40*MenuUnit);
            foreach(var label in journeyModeTabs.GetComponentsInChildren<TextMeshProUGUI>()){label.fontSizeMin=14*MenuUnit;label.fontSizeMax=16*MenuUnit;}
            var stars=journeyModeTabs.parent.Find("Map star balance") as RectTransform;
            if(stars!=null)FitMapStarBalance(stars,stars.GetComponentInChildren<TextMeshProUGUI>());
        }
    }
}
