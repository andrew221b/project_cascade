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
            LocalizationSettings.InitializationOperation.WaitForCompletion();
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
            NavigationNote("Language confirmation",new UiCopy("New puzzles: {0}. This row stays the same.",NativeLanguageName(proposedPuzzleLocale)),.48f,.72f);
            NavigationButton("Usefornewpuzzles","Use for new puzzles",6,.31f,ConfirmPuzzleLocale,true);
            NavigationButton("Keepmychoice","Keep my choice",6,.19f,()=>{proposedPuzzleLocale=null;languagePreview=null;SetScreen(GameScreen.Settings);});
        }
        void DrawLanguages()
        {
            if(languagePreview==null)languagePreview=Preferences.PuzzleLocale;
            var menuHeading=Text("UI language help",menuRoot,new Vector2(.17f,.66f),new Vector2(.83f,.72f),22,"Menu language");menuHeading.font=art.titleFont;menuHeading.alignment=TextAlignmentOptions.Center;
            LanguageCarousel("UiLocale",UiLanguage,.65f,SetUiLocale);
            var wordHeading=Text("Word language help",menuRoot,new Vector2(.17f,.435f),new Vector2(.83f,.51f),22,"Word language · next puzzle");wordHeading.font=art.titleFont;wordHeading.alignment=TextAlignmentOptions.Center;
            LanguageCarousel("PuzzleLocale",languagePreview,.42f,code=>{languagePreview=code;DrawMenu();});
            var apply=MenuButton("Use for new puzzles",.20f,()=>ProposePuzzleLocale(languagePreview),true);apply.name="PuzzleLocale"+languagePreview;apply.interactable=languagePreview!=Preferences.PuzzleLocale;
        }
    }
}
