using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        string languagePreview;
        string queuedUiLanguage;
        bool applyingUiLanguage;
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
            QueueUiLanguage(Preferences.UiLocale);
        }
        static string SystemUiLocale(SystemLanguage language)=>language==SystemLanguage.Russian?"ru":language==SystemLanguage.Ukrainian?"uk":"en";
        public void SetUiLocale(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            Preferences.UiLocale=code; // Persistence preference, never a second active UI locale.
            Preferences.FollowSystemLanguage=false;
            QueueUiLanguage(code);PersistProgress();
        }
        public void SetGameLanguage(string code)
        {
            if(code!="en"&&code!="ru"&&code!="uk")throw new ArgumentOutOfRangeException(nameof(code));
            Preferences.UiLocale=code;
            Preferences.PuzzleLocale=code;
            Preferences.FollowSystemLanguage=false;
            QueueUiLanguage(code);PersistProgress();
        }
        void QueueUiLanguage(string code)
        {
            queuedUiLanguage=code;
            if(!applyingUiLanguage)StartCoroutine(ApplyQueuedUiLanguage());
        }
        IEnumerator ApplyQueuedUiLanguage()
        {
            applyingUiLanguage=true;
            try{
                while(queuedUiLanguage!=null){
                    // Changing SelectedLocale discards the database's current preload handle.
                    // Let initialization finish before changing it; never block the iOS frame.
                    var initialization=LocalizationSettings.InitializationOperation;
                    if(!initialization.IsDone)yield return initialization;
                    if(initialization.Status!=UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)yield break;
                    string code=queuedUiLanguage;queuedUiLanguage=null;
                    LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code);
                    var selected=LocalizationSettings.InitializationOperation;
                    if(!selected.IsDone)yield return selected;
                    if(selected.Status!=UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)yield break;
                    // Requests received while loading replace the pending choice. Rebuild raw
                    // reference captions only for the final locale, alongside localized bindings.
                    if(queuedUiLanguage==null){
                        if(fieldTrayOpen)RefreshToolTray();
                        if(flowReady&&menuRoot!=null&&menuRoot.gameObject.activeInHierarchy)DrawMenu();
                    }
                }
            }finally{applyingUiLanguage=false;}
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
