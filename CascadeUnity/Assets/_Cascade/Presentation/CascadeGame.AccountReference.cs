using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        bool accountGateFromHome,accountErrorAcknowledged,accountDetailsOpen;
        string accountProvider;
        UiCopy accountNotice;
        Button homeAccountShortcut;
        void DrawReferenceAccountGate()
        {
            if(AudienceAgeNeedsChoice){DrawReferenceAgeGate();return;}
            var stage=HomeReferenceSurface(true,out var layout);
            var sign=HomeBounds("Welcome hanging sign",stage,108,146,504,144);
            var face=sign.gameObject.AddComponent<Image>();face.sprite=AccountReferenceArt.Get("Header");face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=270/144f;face.raycastTarget=false;
            var title=ReferenceText("account heading",sign,ReferenceCopy("Welcome!","Добро пожаловать!","Ласкаво просимо!"),44,22,416,100,68);
            title.fontSizeMin=30;title.color=Color.white;
            title.enableVertexGradient=true;
            title.colorGradient=new VertexGradient(new Color(1,.76f,.16f),new Color(1,.76f,.16f),new Color(.76f,.25f,.015f),new Color(.76f,.25f,.015f));
            // Give this heading gold lettering without changing the shared font material.
            var lettering=new Material(title.fontSharedMaterial){name="Welcome gold lettering"};
            lettering.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.22f,.065f,.008f));lettering.SetFloat(ShaderUtilities.ID_OutlineWidth,.22f);lettering.SetFloat(ShaderUtilities.ID_FaceDilate,.06f);
            lettering.EnableKeyword("OUTLINE_ON");lettering.EnableKeyword("UNDERLAY_ON");
            lettering.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.23f,.07f,.01f,.65f));lettering.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,-.65f);
            title.fontSharedMaterial=lettering;title.UpdateMeshPadding();layout.Own(lettering);
            CentreHeading(face,title);var headingFit=sign.GetComponent<CenteredContentRow>();headingFit.Padding=32;headingFit.Refresh();
            var logo=HomeBounds("Welcome Flying Ladders logo",stage,150,280,420,236).gameObject.AddComponent<Image>();logo.sprite=HomeArt("Logo");logo.preserveAspect=true;logo.raycastTarget=false;
            var hero=HomeBounds("Mika waves hello",stage,206,520,284,405).gameObject.AddComponent<Image>();hero.sprite=AccountReferenceArt.Get("WelcomeMika");hero.preserveAspect=true;hero.raycastTarget=false;
            AttachMikaCosmetic(hero,null);
            hero.gameObject.AddComponent<WelcomeMikaView>().Initialize(this,hero);
            var ropes=new RectTransform[2];
            for(int i=0;i<2;i++)ropes[i]=HomeBounds("Welcome title rope "+i,stage,i==0?202:514,40,15,156);
            for(int i=0;i<2;i++){var rope=ropes[i].gameObject.AddComponent<Image>();rope.sprite=SettingsReferenceArt.Get("Cord");rope.type=Image.Type.Tiled;rope.pixelsPerUnitMultiplier=27/15f;rope.raycastTarget=false;ropes[i].SetSiblingIndex(sign.GetSiblingIndex());}
            layout.BindWelcome(sign,logo.rectTransform,ropes);
            if(AudienceCanUseLinkedAccount&&cloudSave?.Linked==true){
                DrawAccountCloudStatus(stage);
                AccountReferenceButton(stage,layout,"Continue signed-in game",ReferenceCopy("Continue","Продолжить","Продовжити"),"Apple",64,1000,592,148,()=>{accountErrorAcknowledged=true;accountNotice=default;SkipAccountGate();});
            }else if(AudienceCanUseLinkedAccount){
                string native=CloudPlatformAccounts.DisplayNameFor(UnityEngine.Application.platform,cloudSave?.PlatformAccountName);
                if(native=="Game Center")
                    AccountReferenceButton(stage,layout,"Sign in with Game Center",ReferenceCopy("Continue with Game Center","Войти через Game Center","Увійти через Game Center"),"Apple",64,956,592,148,()=>BeginReferenceAccountSignIn(native));
                else if(native=="Google Play Games")
                    AccountReferenceButton(stage,layout,"Sign in with Google Play Games",ReferenceCopy("Continue with Play Games","Войти через Play Games","Увійти через Play Games"),"Google",64,956,592,148,()=>BeginReferenceAccountSignIn(native));
                else {
                    AccountReferenceButton(stage,layout,"Sign in with Apple",ReferenceCopy("Continue with Apple","Продолжить с Apple","Продовжити з Apple"),"Apple",96,890,528,104,()=>BeginReferenceAccountSignIn("Apple"));
                    AccountReferenceButton(stage,layout,"Sign in with Google",ReferenceCopy("Continue with Google","Продолжить с Google","Продовжити з Google"),"Google",96,1006,528,104,()=>BeginReferenceAccountSignIn("Google"));
                }
                AccountReferenceButton(stage,layout,"Continue without an account",ReferenceCopy("Play as guest","Зайти как гость","Увійти як гість"),"Guest",180,1130,360,96,SkipAccountGate,true);
            }else{
                AccountReferenceButton(stage,layout,"Continue without an account",ReferenceCopy("Play as guest","Зайти как гость","Увійти як гість"),"Guest",200,1028,320,80,SkipAccountGate,true);
                var privacy=ReferenceText("minor account privacy note",stage,ReferenceCopy(
                    "Cloud sign-in is available from age 16.",
                    "Облачный вход доступен с 16 лет.",
                    "Хмарний вхід доступний з 16 років."),150,1125,420,48,21,false,true);
                privacy.alignment=TextAlignmentOptions.Center;
            }
            if(accountGateFromHome){var back=ReferenceButton("Back",sign,-47,-100,80,80,()=>{if(cloudSave?.CanCancelSignIn==true)cloudSave.CancelSignIn();accountNotice=default;ShowHome();},"BackButton");var backRect=(RectTransform)back.transform;backRect.anchorMin=backRect.anchorMax=backRect.pivot=new Vector2(0,1);backRect.anchoredPosition=new Vector2(-47,100);backRect.sizeDelta=new Vector2(80,80);layout.BindTouch(backRect,new Vector2(80,80));}
            DrawAccountReferenceDialog(stage,layout);layout.Fit();
        }
        void DrawAccountCloudStatus(RectTransform stage)
        {
            var paper=HomeBounds("Account cloud status paper",stage,108,828,504,72);
            var paperFace=paper.gameObject.AddComponent<Image>();paperFace.sprite=SettingsReferenceArt.Get("Row");paperFace.type=Image.Type.Sliced;paperFace.pixelsPerUnitMultiplier=1.7f;paperFace.raycastTarget=false;
            paper.anchorMin=paper.anchorMax=new Vector2(.5f,0);paper.pivot=new Vector2(.5f,0);paper.anchoredPosition=new Vector2(0,380);
            var saved=ReferenceText("account cloud state",paper,CloudState==CloudSyncState.Saved?ReferenceCopy("Your progress is saved in the cloud.","Прогресс сохранён в облаке.","Прогрес збережено в хмарі."):ReferenceCopy("Syncing your progress…","Синхронизация прогресса…","Синхронізація прогресу…"),24,12,456,48,26,false,true);
            saved.fontSizeMin=24;saved.color=new Color(.23f,.14f,.07f);saved.alignment=TextAlignmentOptions.Center;
        }
        void AccountReferenceButton(RectTransform stage,HomeReferenceLayout layout,string name,UiCopy copy,string icon,float x,float y,float width,float height,Action click,bool gold=false)
        {
            var button=ReferenceButton(name,stage,x,y,width,height,click);
            SetHomeButtonBounds(button,x,y,width,height);
            var target=(RectTransform)button.transform;target.pivot=new Vector2(.5f,.5f);target.anchoredPosition+=new Vector2(0,height*.5f);
            var body=ReferenceButtonBody(button);
            // The green atlas includes empty space and leaf ornaments above/below
            // its face. Grow the artwork too, so the primary action looks larger
            // than the compact status paper, rather than only enlarging its hit box.
            float artHeight=height+(gold?12:height>=140?42:20);
            var face=ReferencePicture("Account button face",body,AccountReferenceArt.Get(gold?"Gold":"Green"),0,(height-artHeight)*.5f,width,artHeight,false);
            face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=(gold?182:227)/artHeight;
            var provider=ReferencePicture("Account provider icon "+icon,body,AccountReferenceArt.Get(icon),gold?37:47,20,gold?37:48,height-40);
            var text=ReferenceText("account action",body,copy,gold?86:111,20,width-(gold?104:125),height-40,gold?27:30,!gold);
            text.fontSizeMin=gold?23:26;text.characterSpacing=-1;text.textWrappingMode=TextWrappingModes.NoWrap;
            RegisterButtonDesign(button,gold?GameButtonRole.SecondaryYellow:GameButtonRole.PrimaryGreen);
            button.gameObject.AddComponent<AccountActionLayout>().Initialize((RectTransform)body,provider.rectTransform,text,gold?37:48,height-40);
            layout.BindTouch((RectTransform)button.transform,new Vector2(width,height));
            if(cloudSave?.CanCancelSignIn==true||cloudSave?.CompletingSignIn==true||cloudSave?.NeedsAccountChoice==true)button.interactable=false;
        }
        void BeginReferenceAccountSignIn(string provider)
        {
            if(!AudienceCanUseLinkedAccount){accountNotice=ReferenceCopy("Cloud sign-in is available from age 16.","Облачный вход доступен с 16 лет.","Хмарний вхід доступний з 16 років.");DrawMenu();return;}
            if(purchaseStore?.State==PurchaseState.Busy){accountNotice=ReferenceCopy("Your purchase is being confirmed. Please wait.","Подтверждаем покупку. Подождите немного.","Підтверджуємо купівлю. Зачекайте трохи.");DrawMenu();return;}
            accountProvider=provider;
            accountErrorAcknowledged=false;
            if(cloudSave?.CanLink!=true){
                accountNotice=ReferenceCopy("Sign-in is not available yet. You can play as guest.","Вход пока недоступен. Можно играть как гость.","Вхід поки недоступний. Можна грати як гість.");DrawMenu();return;
            }
            PersistProgress(true);if(HasSaveError)return;
            accountNotice=default;cloudSave.LinkAccount();
        }
        void HomeAccountAction()
        {
            if(cloudSave?.Linked!=true){ShowAccountGate();return;}
            accountDetailsOpen=true;accountNotice=default;DrawMenu();
        }
        void SignOutCloudAccount()
        {
            bool purchaseActive=purchaseStore!=null&&(purchaseStore.State==PurchaseState.Busy);
            if(purchaseActive||cloudSave?.CanSignOut!=true)return;
            PersistProgress(true);if(HasSaveError)return;
            if(!cloudSave.SignOut()){
                accountNotice=ReferenceCopy("Could not sign out. Your progress is safe. Try again.","Не удалось выйти. Прогресс сохранён. Попробуйте ещё раз.","Не вдалося вийти. Прогрес збережено. Спробуйте ще раз.");DrawMenu();return;
            }
            accountDetailsOpen=false;accountNotice=default;DrawMenu();
        }
        void RefreshHomeAccountShortcut()
        {
            if(homeAccountShortcut==null)return;
            bool linked=cloudSave?.Linked==true;
            ApplyHomeAccountShortcutState(linked,true);
        }
        void ApplyHomeAccountShortcutState(bool linked,bool available)
        {
            homeAccountShortcut.name=linked?"Cloud account":"Sign in";
            homeAccountShortcut.GetComponent<Image>().sprite=AccountReferenceArt.Get("SignIn");
            // Opening account status never changes identity; the actions inside own their locks.
            homeAccountShortcut.interactable=available;
        }
        void DrawAccountReferenceDialog(RectTransform stage,HomeReferenceLayout layout)
        {
            bool details=accountDetailsOpen&&cloudSave?.Linked==true;
            bool waiting=cloudSave?.CanCancelSignIn==true,completing=cloudSave?.CompletingSignIn==true,choice=cloudSave?.NeedsAccountChoice==true;
            bool failed=!accountErrorAcknowledged&&CurrentScreen==GameScreen.AccountGate&&(cloudSave?.LastFailureCode=="SignInFailed"||cloudSave?.LastFailureCode=="SignInTimeout"||cloudSave?.LastFailureCode=="GameCenterSignInRequired"||cloudSave?.LastFailureCode=="GameCenterUnavailable");
            if(!details&&!waiting&&!completing&&!choice&&!failed&&string.IsNullOrEmpty(accountNotice.Key))return;
            foreach(var b in stage.GetComponentsInChildren<Button>())b.interactable=false;
            var veil=Rect("Account dialog input veil",stage,Vector2.zero,Vector2.one);veil.gameObject.AddComponent<Image>().color=new Color(0,0,0,.55f);
            var panel=ReferencePicture("Account dialog panel",stage,SettingsReferenceArt.Get("Panel"),0,0,540,choice||details?520:400,false);panel.type=Image.Type.Sliced;panel.pixelsPerUnitMultiplier=2;
            var dialog=panel.rectTransform;dialog.anchorMin=dialog.anchorMax=dialog.pivot=new Vector2(.5f,.5f);dialog.anchoredPosition=Vector2.zero;
            UiCopy heading=details?ReferenceCopy("Cloud save","Облачное сохранение","Хмарне збереження"):choice?ReferenceCopy("Load saved progress?","Загрузить сохранение?","Завантажити прогрес?"):
                waiting||completing?ReferenceCopy("Signing in","Вход в аккаунт","Вхід в акаунт"):ReferenceCopy("Account","Аккаунт","Акаунт");
            ReferenceText("account dialog heading",dialog,heading,28,28,484,65,35);
            UiCopy detail=details?(CloudState==CloudSyncState.Saved?ReferenceCopy("Your progress is saved in the cloud. You can restore it with this game account.","Прогресс сохранён в облаке. Его можно восстановить через этот игровой аккаунт.","Прогрес збережено в хмарі. Його можна відновити через цей ігровий акаунт."):ReferenceCopy("Your account is connected. Syncing your progress…","Аккаунт подключён. Синхронизация прогресса…","Акаунт підключено. Синхронізація прогресу…")):choice?ReferenceCopy("Choose which account’s progress to keep.","Выберите, прогресс какого аккаунта оставить.","Виберіть, прогрес якого акаунта залишити."):
                completing?ReferenceCopy("Connecting your cloud save…","Подключение облачного сохранения…","Підключення хмарного збереження…"):
                waiting?(cloudSave?.NativeSignInConfirmed==true?ReferenceCopy("Connecting your cloud save…","Подключение облачного сохранения…","Підключення хмарного збереження…"):
                    cloudSave?.PlatformAccountName!=null?ReferenceCopy("Signing in with "+accountProvider+"…","Вход через "+accountProvider+"…","Вхід через "+accountProvider+"…"):
                    ReferenceCopy("Choose "+accountProvider+" in the browser, then return here.","Выберите "+accountProvider+" в браузере, затем вернитесь в игру.","Виберіть "+accountProvider+" у браузері та поверніться до гри.")):
                cloudSave?.LastFailureCode=="GameCenterSignInRequired"?ReferenceCopy("Sign in to Game Center in iPhone Settings, then return here.","Войдите в Game Center в настройках iPhone и вернитесь в игру.","Увійдіть у Game Center в параметрах iPhone та поверніться до гри."):
                cloudSave?.LastFailureCode=="GameCenterUnavailable"?ReferenceCopy("Game Center is unavailable. Check Game Center in iPhone Settings.","Game Center недоступен. Проверьте его в настройках iPhone.","Game Center недоступний. Перевірте його в параметрах iPhone."):
                failed?ReferenceCopy("Sign-in did not finish. Your progress is safe. You can try again.","Вход не завершён. Прогресс сохранён. Можно попробовать ещё раз.","Вхід не завершено. Прогрес збережено. Можна спробувати ще раз."):accountNotice;
            ReferenceText("account dialog detail",dialog,detail,40,107,460,142,29,false,true);
            if(details){
                AccountDialogAction(dialog,layout,"Continue cloud account",ReferenceCopy("Continue","Продолжить","Продовжити"),264,()=>{accountDetailsOpen=false;accountNotice=default;DrawMenu();});
                AccountDialogAction(dialog,layout,"Sign out cloud account",ReferenceCopy("Sign out","Выйти из аккаунта","Вийти з акаунта"),384,SignOutCloudAccount);
                var signOut=stage.GetComponentsInChildren<Button>().First(b=>b.name=="Sign out cloud account");signOut.interactable=cloudSave.CanSignOut&&purchaseStore?.State!=PurchaseState.Busy;
            }else if(choice){
                AccountDialogAction(dialog,layout,"Load account",ReferenceCopy("Load account","Загрузить аккаунт","Завантажити акаунт"),264,()=>cloudSave.LoadExistingAccount());
                AccountDialogAction(dialog,layout,"Keep this phone's save",ReferenceCopy("Keep this phone's save","Оставить сохранение телефона","Залишити збереження телефона"),384,()=>{cloudSave.KeepLocalAccount();accountNotice=default;DrawMenu();});
                foreach(var b in stage.GetComponentsInChildren<Button>())if(b.name=="Load account"||b.name=="Keep this phone's save")b.interactable=cloudSave.CanLink;
            }else if(!completing)AccountDialogAction(dialog,layout,waiting?"Cancel sign-in":"Close account notice",waiting?ReferenceCopy("Cancel","Отмена","Скасувати"):ReferenceCopy("OK","Понятно","Зрозуміло"),284,()=>{if(waiting)cloudSave.CancelSignIn();accountErrorAcknowledged=true;accountNotice=default;DrawMenu();});
        }
        void AccountDialogAction(RectTransform stage,HomeReferenceLayout layout,string name,UiCopy copy,float y,Action click)
        {
            var button=ReferenceButton(name,stage,30,y,480,104,click,"GreenAction");var label=ReferenceText("account dialog action",ReferenceButtonBody(button),copy,16,8,430,60,26,true);CentreContent((RectTransform)ReferenceButtonBody(button),label.rectTransform);
            layout.BindTouch((RectTransform)button.transform,new Vector2(480,104));
        }
    }
}
