using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        string proposedPuzzleLocale;
        string languagePreview;
        public string UiLanguage=>LocalizationSettings.SelectedLocale.Identifier.Code;
        public string ProposedPuzzleLocale=>proposedPuzzleLocale;
        static UiCopy NativeLanguageName(string code)=>UiCopy.Raw(code=="ru"?"Русский":code=="uk"?"Українська":"English");
        static UiCopy LanguageName(string code)=>code=="ru"?"Russian":code=="uk"?"Ukrainian":"English";
        void InitializeUiLanguage()
        {
            // A synchronous Addressables wait can stall the iOS main thread during a cold
            // launch or when returning to the app. The locale assets are already listed in
            // AvailableLocales; their tables finish loading through the normal player loop.
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(Preferences.UiLocale);
        }
        public void SetUiLocale(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code);
            Preferences.UiLocale=code; // Persistence preference, never a second active UI locale.
            PersistProgress();
            if(CurrentScreen==GameScreen.Settings)DrawMenu();
        }
        public void SetGameLanguage(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            Preferences.UiLocale=code;
            Preferences.PuzzleLocale=code;
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code);
            PersistProgress();
            if(CurrentScreen==GameScreen.Settings)DrawMenu();
        }
        public void ProposePuzzleLocale(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            proposedPuzzleLocale=code;SetScreen(GameScreen.WordLanguage);
        }
        public void ConfirmPuzzleLocale()
        {
            if(proposedPuzzleLocale==null)return;
            Preferences.PuzzleLocale=proposedPuzzleLocale;PersistProgress();proposedPuzzleLocale=null;languagePreview=null;SetScreen(GameScreen.Settings);
        }
        void DrawWordLanguageConfirmation()
        {
            NavigationHeader("Change word language?",()=>{proposedPuzzleLocale=null;languagePreview=null;SetScreen(GameScreen.Settings);});
            NavigationNote("Language confirmation",new UiCopy("New puzzles: {0}",NativeLanguageName(proposedPuzzleLocale)),.48f,.72f);
            NavigationButton("Usefornewpuzzles","Use for new puzzles",6,.31f,ConfirmPuzzleLocale,true);
            NavigationButton("Keepmychoice","Keep my choice",6,.19f,()=>{proposedPuzzleLocale=null;languagePreview=null;SetScreen(GameScreen.Settings);});
        }
        void DrawLanguages()
        {
            foreach(var code in new[]{"en","ru","uk"}){
                string chosen=code;
                var button=SettingsButton(NativeLanguageName(code),()=>SetGameLanguage(chosen));
                button.name="GameLanguage"+code;
                if(code==Preferences.UiLocale)MarkSelection(button,true);
            }
        }
    }
}
