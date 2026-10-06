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
            MenuTitle("Change word language?","Word language · next puzzle");
            menuPage.anchorMin=new Vector2(.045f,.265f);menuPage.anchorMax=new Vector2(.955f,.865f);
            var binding=menuBinding;binding.anchorMin=new Vector2(.045f,.852f);binding.anchorMax=new Vector2(.955f,.867f);
            var title=menuHeading.rectTransform;title.anchorMin=new Vector2(.10f,.74f);title.anchorMax=new Vector2(.90f,.838f);menuHeading.fontSize=28*(Preferences.LargeText?1.12f:1);
            var description=menuDescription.rectTransform;description.anchorMin=new Vector2(.10f,.66f);description.anchorMax=new Vector2(.90f,.735f);
            Text("Language confirmation",menuRoot,new Vector2(.10f,.51f),new Vector2(.90f,.65f),22,new UiCopy("New puzzles: {0}. This row stays the same.",NativeLanguageName(proposedPuzzleLocale)));
            MenuButton("Use for new puzzles",.425f,ConfirmPuzzleLocale,true);
            MenuButton("Keep my choice",.31f,()=>{proposedPuzzleLocale=null;languagePreview=null;SetScreen(GameScreen.Settings);});
        }
        void DrawLanguages()
        {
            if(languagePreview==null)languagePreview=Preferences.PuzzleLocale;
            var menuHeading=Text("UI language help",menuRoot,new Vector2(.10f,.66f),new Vector2(.90f,.72f),22,"Menu language");menuHeading.font=art.titleFont;menuHeading.alignment=TextAlignmentOptions.MidlineLeft;
            LanguageCarousel("UiLocale",UiLanguage,.65f,SetUiLocale);
            var wordHeading=Text("Word language help",menuRoot,new Vector2(.10f,.435f),new Vector2(.90f,.51f),22,"Word language · next puzzle");wordHeading.font=art.titleFont;wordHeading.alignment=TextAlignmentOptions.MidlineLeft;
            LanguageCarousel("PuzzleLocale",languagePreview,.42f,code=>{languagePreview=code;DrawMenu();});
            var apply=MenuButton("Use for new puzzles",.20f,()=>ProposePuzzleLocale(languagePreview),true);apply.name="PuzzleLocale"+languagePreview;apply.interactable=languagePreview!=Preferences.PuzzleLocale;
        }
    }
}
