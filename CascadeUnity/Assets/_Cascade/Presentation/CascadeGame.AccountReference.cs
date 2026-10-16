using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        bool accountGateFromHome,accountErrorAcknowledged;
        string accountProvider;
        UiCopy accountNotice;
        Button homeAccountShortcut;
        void DrawReferenceAccountGate()
        {
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
            var hero=HomeBounds("Mika waves hello",stage,206,510,284,405).gameObject.AddComponent<Image>();hero.sprite=AccountReferenceArt.Get("WelcomeMika");hero.preserveAspect=true;hero.raycastTarget=false;
            AttachMikaCosmetic(hero,null);
            var ropes=new RectTransform[2];
            for(int i=0;i<2;i++)ropes[i]=HomeBounds("Welcome title rope "+i,stage,i==0?202:514,40,15,156);
            for(int i=0;i<2;i++){var rope=ropes[i].gameObject.AddComponent<Image>();rope.sprite=SettingsReferenceArt.Get("Cord");rope.type=Image.Type.Tiled;rope.pixelsPerUnitMultiplier=27/15f;rope.raycastTarget=false;ropes[i].SetSiblingIndex(sign.GetSiblingIndex());}
            layout.BindWelcome(sign,logo.rectTransform,ropes);
            AccountReferenceButton(stage,layout,"Sign in with Apple",ReferenceCopy("Continue with Apple","Продолжить с Apple","Продовжити з Apple"),"Apple",128,916,464,88,()=>BeginReferenceAccountSignIn("Apple"));
            AccountReferenceButton(stage,layout,"Sign in with Google",ReferenceCopy("Continue with Google","Продолжить с Google","Продовжити з Google"),"Google",128,1018,464,88,()=>BeginReferenceAccountSignIn("Google"));
            AccountReferenceButton(stage,layout,"Continue without an account",ReferenceCopy("Play as guest","Зайти как гость","Увійти як гість"),"Guest",200,1124,320,80,SkipAccountGate,true);
            if(accountGateFromHome){var back=ReferenceButton("Back",stage,61,132,80,80,()=>{if(cloudSave?.CanCancelSignIn==true)cloudSave.CancelSignIn();accountNotice=default;ShowHome();},"BackButton");layout.BindTouch((RectTransform)back.transform,new Vector2(80,80));}
            DrawAccountReferenceDialog(stage,layout);layout.Fit();
        }
        void AccountReferenceButton(RectTransform stage,HomeReferenceLayout layout,string name,UiCopy copy,string icon,float x,float y,float width,float height,Action click,bool gold=false)
        {
            var button=ReferenceButton(name,stage,x,y,width,height,click);
            SetHomeButtonBounds(button,x,y,width,height);
            var target=(RectTransform)button.transform;target.pivot=new Vector2(.5f,.5f);target.anchoredPosition+=new Vector2(0,height*.5f);
            var body=ReferenceButtonBody(button);
            float artHeight=height+(gold?12:20);
            var face=ReferencePicture("Account button face",body,AccountReferenceArt.Get(gold?"Gold":"Green"),0,(height-artHeight)*.5f,width,artHeight,false);
            face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=(gold?182:227)/artHeight;
            var provider=ReferencePicture("Account provider icon "+icon,body,AccountReferenceArt.Get(icon),gold?37:47,16,gold?37:52,height-32);
            var text=ReferenceText("account action",body,copy,gold?86:111,8,width-(gold?104:125),height-16,gold?27:29,!gold);
            text.fontSizeMin=gold?23:24;text.characterSpacing=-1;text.textWrappingMode=TextWrappingModes.NoWrap;
            button.gameObject.AddComponent<AccountActionLayout>().Initialize((RectTransform)body,provider.rectTransform,text,gold?37:52,height-32);
            layout.BindTouch((RectTransform)button.transform,new Vector2(width,height));
            if(cloudSave?.CanCancelSignIn==true||cloudSave?.NeedsAccountChoice==true)button.interactable=false;
        }
        void BeginReferenceAccountSignIn(string provider)
        {
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
            bool purchaseActive=purchaseStore!=null&&(purchaseStore.State==PurchaseState.Busy||purchaseStore.State==PurchaseState.Connecting);
            if(purchaseActive||!cloudSave.CanSignOut)return;
            PersistProgress(true);if(HasSaveError)return;
            if(!cloudSave.SignOut()){
                accountNotice=ReferenceCopy("Could not sign out. Your progress is safe. Try again.","Не удалось выйти. Прогресс сохранён. Попробуйте ещё раз.","Не вдалося вийти. Прогрес збережено. Спробуйте ще раз.");DrawMenu();return;
            }
            RefreshHomeAccountShortcut();
        }
        void RefreshHomeAccountShortcut()
        {
            if(homeAccountShortcut==null)return;
            bool linked=cloudSave?.Linked==true;
            ApplyHomeAccountShortcutState(linked,!linked||cloudSave.CanSignOut);
        }
        void ApplyHomeAccountShortcutState(bool linked,bool available)
        {
            homeAccountShortcut.name=linked?"Sign out":"Sign in";
            homeAccountShortcut.GetComponent<Image>().sprite=AccountReferenceArt.Get(linked?"SignOut":"SignIn");
            bool purchaseActive=purchaseStore!=null&&(purchaseStore.State==PurchaseState.Busy||purchaseStore.State==PurchaseState.Connecting);
            homeAccountShortcut.interactable=available&&(!linked||!purchaseActive);
        }
        void DrawAccountReferenceDialog(RectTransform stage,HomeReferenceLayout layout)
        {
            bool waiting=cloudSave?.CanCancelSignIn==true,choice=cloudSave?.NeedsAccountChoice==true;
            bool failed=!accountErrorAcknowledged&&CurrentScreen==GameScreen.AccountGate&&(cloudSave?.LastFailureCode=="SignInFailed"||cloudSave?.LastFailureCode=="SignInTimeout");
            if(!waiting&&!choice&&!failed&&string.IsNullOrEmpty(accountNotice.Key))return;
            foreach(var b in stage.GetComponentsInChildren<Button>())b.interactable=false;
            var veil=ReferenceRect("Account dialog input veil",stage,0,0,720,Mathf.Max(1280,stage.rect.height));veil.gameObject.AddComponent<Image>().color=new Color(0,0,0,.55f);
            var panel=ReferencePicture("Account dialog panel",stage,SettingsReferenceArt.Get("Panel"),90,411,540,450,false);panel.type=Image.Type.Sliced;panel.pixelsPerUnitMultiplier=2;
            UiCopy heading=choice?ReferenceCopy("Load saved progress?","Загрузить сохранение?","Завантажити прогрес?"):
                waiting?ReferenceCopy("Signing in","Вход в аккаунт","Вхід в акаунт"):ReferenceCopy("Account","Аккаунт","Акаунт");
            ReferenceText("account dialog heading",stage,heading,123,445,474,65,35);
            UiCopy detail=choice?ReferenceCopy("This account already has progress. Choose which save to use.","У аккаунта уже есть прогресс. Выберите сохранение.","В акаунта вже є прогрес. Виберіть збереження."):
                waiting?ReferenceCopy("Choose "+accountProvider+" in the browser, then return here.","Выберите "+accountProvider+" в браузере, затем вернитесь в игру.","Виберіть "+accountProvider+" у браузері та поверніться до гри."):
                failed?ReferenceCopy("Sign-in did not finish. Your progress is safe. You can try again.","Вход не завершён. Прогресс сохранён. Можно попробовать ещё раз.","Вхід не завершено. Прогрес збережено. Можна спробувати ще раз."):accountNotice;
            ReferenceText("account dialog detail",stage,detail,130,520,460,132,29,false,true);
            if(choice){
                AccountDialogAction(stage,layout,"Load account",ReferenceCopy("Load account","Загрузить аккаунт","Завантажити акаунт"),676,()=>cloudSave.LoadExistingAccount());
                AccountDialogAction(stage,layout,"Keep this phone's save",ReferenceCopy("Keep this phone's save","Оставить сохранение телефона","Залишити збереження телефона"),757,()=>{cloudSave.KeepLocalAccount();accountNotice=default;DrawMenu();});
                foreach(var b in stage.GetComponentsInChildren<Button>())if(b.name=="Load account"||b.name=="Keep this phone's save")b.interactable=cloudSave.CanLink;
            }else AccountDialogAction(stage,layout,waiting?"Cancel sign-in":"Close account notice",waiting?ReferenceCopy("Cancel","Отмена","Скасувати"):ReferenceCopy("OK","Понятно","Зрозуміло"),724,()=>{if(waiting)cloudSave.CancelSignIn();accountErrorAcknowledged=true;accountNotice=default;DrawMenu();});
        }
        void AccountDialogAction(RectTransform stage,HomeReferenceLayout layout,string name,UiCopy copy,float y,Action click)
        {
            var button=ReferenceButton(name,stage,129,y,462,70,click,"GreenAction");ReferenceText("account dialog action",ReferenceButtonBody(button),copy,16,5,430,60,26,true);
            layout.BindTouch((RectTransform)button.transform,new Vector2(462,70));
        }
    }
}
