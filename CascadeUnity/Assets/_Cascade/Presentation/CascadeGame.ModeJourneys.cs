using System;
using System.IO;
using System.Linq;
using Cascade.Application;
using Cascade.Domain;
using Cascade.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        RectTransform journeyModeTabs;
        public bool CanSwitchJourneyMode=>!saveBlocked&&!nativeAdSuspended&&previewAd==null&&!IsDescending&&!IsPlanningDescent
            &&Interaction!=null&&!Interaction.IsPresenting&&(profile.Completion==null||profile.Completion.Acknowledged);
        void SwitchJourneyMode(PuzzleMode mode)
        {
            if(mode==PreferredMode){ShowJourney();return;}
            if(!CanSwitchJourneyMode)return;
            PersistProgress();if(HasSaveError)return;
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
            MountAttempt(restored,profile.Descent);journeyActive=profile.JourneyAttempt;storyRoute=false;
            chapterPage=0;selectedJourneyStop=NextJourneyStop();ShowJourney();
        }
        void DrawJourneyModeTabs()
        {
            var row=Rect("Map mode tabs",menuRoot,new Vector2(.055f,.897f),new Vector2(.945f,.897f));
            journeyModeTabs=row;row.pivot=new Vector2(.5f,1);row.sizeDelta=new Vector2(0,52*MenuUnit);
            var modes=new[]{PuzzleMode.Shapes,PuzzleMode.Words,PuzzleMode.Mixed};
            for(int i=0;i<modes.Length;i++){
                var mode=modes[i];bool active=mode==PreferredMode;
                var tab=Button(mode.ToString(),row,new Vector2(i/3f+.005f,0),new Vector2((i+1)/3f-.005f,1),()=>SelectMode(mode));tab.name="Mapmode"+mode;
                var face=tab.GetComponent<Image>();face.sprite=enamelControls[active?1:0];face.type=Image.Type.Sliced;face.color=Color.white;
                var label=tab.GetComponentInChildren<TextMeshProUGUI>();label.font=art.titleFont;label.margin=new Vector4(7,3,7,3);label.enableAutoSizing=true;
                label.fontSizeMin=15*MenuUnit;label.fontSizeMax=20*MenuUnit;label.textWrappingMode=TextWrappingModes.NoWrap;
                if(!active)navigationLightLabels.Add(label);tab.interactable=active||CanSwitchJourneyMode;
            }
            CompletionText("Selected mode progress",menuRoot,new Vector2(.25f,.766f),new Vector2(.55f,.806f),16,
                UiCopy.Raw(JourneyClearCount+" / "+JourneySequence().Count));
            Picture("Mode earned stars",menuRoot,new Vector2(.57f,.772f),new Vector2(.63f,.803f),CompletionArt(0),true);
            var offered=JourneySequence().Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            CompletionText("Mode stars total",menuRoot,new Vector2(.64f,.766f),new Vector2(.77f,.806f),16,UiCopy.Raw(profile.Clears.Where(c=>offered.Contains(c.CampaignNodeId)).Sum(c=>c.BestStars).ToString()));
        }
        void FitJourneyModeTabs()
        {
            if(journeyModeTabs==null)return;
            journeyModeTabs.sizeDelta=new Vector2(0,52*MenuUnit);
            foreach(var label in journeyModeTabs.GetComponentsInChildren<TextMeshProUGUI>()){label.fontSizeMin=15*MenuUnit;label.fontSizeMax=20*MenuUnit;}
        }
    }
}
