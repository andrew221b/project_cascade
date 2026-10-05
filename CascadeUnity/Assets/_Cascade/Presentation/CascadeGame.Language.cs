using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        string proposedPuzzleLocale;
        public string UiLanguage=>LocalizationSettings.SelectedLocale.Identifier.Code;
        public string ProposedPuzzleLocale=>proposedPuzzleLocale;
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
            Preferences.PuzzleLocale=proposedPuzzleLocale;proposedPuzzleLocale=null;SetScreen(GameScreen.Settings);
        }
        void DrawWordLanguageConfirmation()
        {
            MenuTitle("Change word language?",new UiCopy("New puzzles: {0}. This row stays the same.",LanguageName(proposedPuzzleLocale)));
            MenuButton("Use for new puzzles",.43f,ConfirmPuzzleLocale,true);
            MenuButton("Keep my choice",.30f,()=>{proposedPuzzleLocale=null;SetScreen(GameScreen.Settings);});
        }
        void DrawLanguages()
        {
            Text("UI language help",menuRoot,new Vector2(.10f,.66f),new Vector2(.90f,.72f),18,"Menu language");
            string[] codes={"en","ru","uk"};
            for(int i=0;i<codes.Length;i++) {
                string code=codes[i];
                var button=MenuButton(LanguageName(code),.56f-i*.10f,()=>SetUiLocale(code),UiLanguage==code);
                button.name="UiLocale"+code;
            }
            Text("Word language help",menuRoot,new Vector2(.10f,.26f),new Vector2(.90f,.33f),18,"Word language · next puzzle");
            for(int i=0;i<codes.Length;i++) {
                string code=codes[i];float left=.06f+i*.30f;
                var button=MenuButton(LanguageName(code),.175f,()=>ProposePuzzleLocale(code),Preferences.PuzzleLocale==code,left,left+.28f);
                button.name="PuzzleLocale"+code;button.GetComponentInChildren<TextMeshProUGUI>().fontSize=16;
            }
        }
    }
}
