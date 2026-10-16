using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        string languagePreview;
#if UNITY_EDITOR
        public static SystemLanguage? EditorSystemLanguage { get; set; }
#endif
        public string UiLanguage=>LocalizationSettings.SelectedLocale.Identifier.Code;
        public string ProposedPuzzleLocale=>null;
        static UiCopy NativeLanguageName(string code)=>UiCopy.Raw(code=="ru"?"Русский":code=="uk"?"Українська":"English");
        static UiCopy LanguageName(string code)=>code=="ru"?"Russian":code=="uk"?"Ukrainian":"English";
        void InitializeUiLanguage()
        {
            // Preserve earlier RU/UK choices. Earlier English defaults adopt the
            // device language; every subsequent manual choice overrides detection.
            bool follow=Preferences.FollowSystemLanguage??(Preferences.UiLocale=="en");
            Preferences.FollowSystemLanguage=follow;
            if(follow){
                var system=UnityEngine.Application.systemLanguage;
#if UNITY_EDITOR
                system=EditorSystemLanguage??system;
#endif
                Preferences.UiLocale=Preferences.PuzzleLocale=SystemUiLocale(system);
            }
            // A synchronous Addressables wait can stall the iOS main thread during a cold
            // launch or when returning to the app. The locale assets are already listed in
            // AvailableLocales; their tables finish loading through the normal player loop.
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(Preferences.UiLocale);
        }
        static string SystemUiLocale(SystemLanguage language)=>language==SystemLanguage.Russian?"ru":language==SystemLanguage.Ukrainian?"uk":"en";
        public void SetUiLocale(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code);
            Preferences.UiLocale=code; // Persistence preference, never a second active UI locale.
            Preferences.FollowSystemLanguage=false;
            PersistProgress();
            if(fieldTrayOpen)RefreshToolTray();
            if(CurrentScreen==GameScreen.Settings)DrawMenu();
        }
        public void SetGameLanguage(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            Preferences.UiLocale=code;
            Preferences.PuzzleLocale=code;
            Preferences.FollowSystemLanguage=false;
            LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code);
            PersistProgress();
            if(fieldTrayOpen)RefreshToolTray();
            if(CurrentScreen==GameScreen.Settings)DrawMenu();
        }
        public void ProposePuzzleLocale(string code)
        {
            // Compatibility entry point: the game now has one language selection.
            SetGameLanguage(code);PersistProgress(true);
        }
        public void ConfirmPuzzleLocale()
        {
            // Older callers may still confirm; selection has already applied immediately.
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
