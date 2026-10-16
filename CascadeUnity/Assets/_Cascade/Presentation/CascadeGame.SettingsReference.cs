using System;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void DrawReferenceSettings()
        {
            var environment=Rect("Settings reference environment",menuRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            var sky=Rect("Settings landscape",environment,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            var skyImage=sky.gameObject.AddComponent<UnityEngine.UI.Image>();skyImage.sprite=SettingsReferenceArt.Get("Scene");skyImage.raycastTarget=false;
            var leaves=Rect("Settings leaves behind controls",environment,Vector2.zero,Vector2.one);
            leaves.gameObject.AddComponent<MenuLeafDrift>().Initialize(this,ReferenceShopArt("MenuGreenLeaf"));
            var frame=Rect("Settings outer wooden frame",environment,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            var frameImage=frame.gameObject.AddComponent<UnityEngine.UI.Image>();frameImage.sprite=SettingsReferenceArt.Get("Frame");frameImage.type=UnityEngine.UI.Image.Type.Sliced;frameImage.raycastTarget=false;
            var stage=Rect("Settings reference composition",menuRoot,new Vector2(.5f,1),new Vector2(.5f,1));
            var layout=stage.gameObject.AddComponent<SettingsReferenceLayout>();layout.Initialize(menuRoot,environment,sky,frameImage);
            for(int i=0;i<2;i++)layout.BindCord(i,ReferencePicture("Settings suspension extension "+i,stage,SettingsReferenceArt.Get("Cord"),i==0?178:525,0,16,1,false).rectTransform);
            ReferencePicture("Settings hanging title sign",stage,SettingsReferenceArt.Get("Header"),104,-32,519,305);
            var title=ReferenceText("settings heading",stage,SettingsReferenceTitle(),188,127,351,76,settingsPage<0||settingsPage==2?44:36);
            title.fontSizeMin=settingsPage<0?38:28;title.color=new Color(.17f,.055f,.018f);
            ReferenceButton("Back",stage,33,129,98,98,BackFromReferenceSettings,"BackButton");
            int rows=settingsPage==1?2:3;
            var panel=ReferencePicture("Settings wooden panel",stage,SettingsReferenceArt.Get("Panel"),88,240,545,settingsPage<0?871:rows==2?262:370,false);
            panel.type=UnityEngine.UI.Image.Type.Sliced;panel.pixelsPerUnitMultiplier=2;
            if(settingsPage<0)DrawReferenceSettingsRoot(stage);
            else if(settingsPage==0){
                SettingsReferenceToggle(stage,"Music",8,276,()=>Preferences.Music,()=>Preferences.Music=!Preferences.Music);
                SettingsReferenceToggle(stage,"Soundeffects",0,380,()=>Preferences.Sound,()=>Preferences.Sound=!Preferences.Sound);
                SettingsReferenceToggle(stage,"Haptics",9,484,()=>Preferences.Haptics,()=>Preferences.Haptics=!Preferences.Haptics);
            }else if(settingsPage==1){
                SettingsReferenceToggle(stage,"Largertext",10,276,()=>Preferences.LargeText,()=>Preferences.LargeText=!Preferences.LargeText);
                SettingsReferenceToggle(stage,"Tutorialhelp",11,380,()=>Preferences.Tutorials,()=>Preferences.Tutorials=!Preferences.Tutorials);
            }else{
                var codes=new[]{"en","ru","uk"};
                for(int i=0;i<codes.Length;i++){
                    string code=codes[i];bool selected=Preferences.UiLocale==code;float y=276+i*104;
                    var b=SettingsReferenceRow(stage,"GameLanguage"+code,NativeLanguageName(code),y,98,()=>{SetGameLanguage(code);PersistProgress(true);},selected,true);
                    SettingsReferenceCaption(b,NativeLanguageName(code),48,0,345,98,29);
                    ReferencePicture("Settings language radio "+code,ReferenceButtonBody(b),SettingsReferenceArt.Icon(selected?15:14),419,21,55,55);
                }
            }
            layout.Fit();
        }
        UiCopy SettingsReferenceTitle()=>settingsPage<0?ReferenceCopy("Settings","Настройки","Налаштування"):
            settingsPage==0?ReferenceCopy("Sound & motion","Звук и движение","Звук і рух"):
            settingsPage==1?ReferenceCopy("Comfort & help","Удобство и помощь","Зручність і допомога"):
            ReferenceCopy("Languages","Языки","Мови");
        void BackFromReferenceSettings()
        {
            if(settingsPage>=0){settingsPage=-1;languagePreview=null;DrawMenu();}else CloseMenu();
        }
        void DrawReferenceSettingsRoot(RectTransform stage)
        {
            SettingsReferenceLink(stage,"Soundeffects",ReferenceCopy("Sound & motion","Звуки и движение","Звук і рух"),0,274,()=>{settingsPage=0;DrawMenu();});
            SettingsReferenceLink(stage,"Comfort",ReferenceCopy("Comfort & help","Удобство и помощь","Зручність і допомога"),1,371,()=>{settingsPage=1;DrawMenu();});
            SettingsReferenceLink(stage,"Languages",ReferenceCopy("Languages","Языки","Мови"),2,470,()=>{settingsPage=2;DrawMenu();});
            bool active=Preferences.NotificationsEnabled&&localReminders?.PermissionGranted==true;
            var notifications=SettingsReferenceRow(stage,"Notifications",ReferenceCopy("Notifications","Уведомления","Сповіщення"),569,95,()=>{SetNotificationsEnabled(!active);PersistProgress(true);DrawMenu();});
            SettingsReferenceIcon(notifications,3);SettingsReferenceCaption(notifications,ReferenceCopy("Notifications","Уведомления","Сповіщення"));
            ReferencePicture("Settings notification switch",ReferenceButtonBody(notifications),SettingsReferenceArt.Icon(active?12:13),382,16,102,58);
            notifications.interactable=LocalRemindersAvailable&&!reminderPermissionPending;
            SettingsReferenceLink(stage,"Remove ads",ReferenceCopy("Remove ads","Отключить рекламу","Вимкнути рекламу"),4,671,ShowNoAds);
            SettingsReferenceLink(stage,"Privacy Policy",ReferenceCopy("Privacy Policy","Политика\nконфиденциальности","Політика\nконфіденційності"),5,770,()=>OpenLegalDocument("privacy"),100);
            SettingsReferenceLink(stage,"Terms of Use",ReferenceCopy("Terms of Use","Условия использования","Умови використання"),6,879,()=>OpenLegalDocument("terms"),94);
            var restore=ReferenceButton("Restore purchases",stage,138,988,448,89,RestorePurchasesFromSettings);
            var face=ReferencePicture("Settings restore gold face",ReferenceButtonBody(restore),SettingsReferenceArt.Get("Restore"),0,0,448,89,false);
            face.type=UnityEngine.UI.Image.Type.Sliced;face.pixelsPerUnitMultiplier=2;
            ReferencePicture("Settings restore arrows",ReferenceButtonBody(restore),SettingsReferenceArt.Icon(7),44,18,54,52);
            SettingsReferenceCaption(restore,ReferenceCopy("Restore purchases","Восстановить покупки","Відновити покупки"),123,0,307,89,27);
            restore.interactable=purchaseStore?.CanRestore==true;
            stage.GetComponent<SettingsReferenceLayout>().BindTouch((RectTransform)restore.transform,new Vector2(448,89));
        }
        UnityEngine.UI.Button SettingsReferenceRow(RectTransform stage,string name,UiCopy caption,float y,float height,Action click,bool selected=false,bool language=false)
        {
            var button=ReferenceButton(name,stage,109,y,507,height,click);
            var face=ReferencePicture("Settings row face",ReferenceButtonBody(button),SettingsReferenceArt.Get(selected?"Selected":"Row"),0,0,507,height,false);
            face.type=UnityEngine.UI.Image.Type.Sliced;face.pixelsPerUnitMultiplier=(selected?172:171)/height;
            stage.GetComponent<SettingsReferenceLayout>().BindTouch((RectTransform)button.transform,new Vector2(507,height));return button;
        }
        void SettingsReferenceCaption(UnityEngine.UI.Button button,UiCopy caption,float x=140,float y=0,float width=311,float height=90,float size=24)
        {
            var text=ReferenceText("settings caption "+button.name,ReferenceButtonBody(button),caption,x,y,width,height,size,false,false);
            text.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??art.titleFont;
            text.fontStyle=FontStyles.Bold;
            text.fontSizeMax=size*(Preferences.LargeText?1.10f:1);text.fontSizeMin=size*.9f;
            text.characterSpacing=-1;
            text.alignment=TextAlignmentOptions.MidlineLeft;text.color=new Color(.15f,.055f,.02f);
        }
        void SettingsReferenceIcon(UnityEngine.UI.Button button,int icon)=>ReferencePicture("Settings icon "+icon,ReferenceButtonBody(button),SettingsReferenceArt.Icon(icon),32,12,73,68);
        void SettingsReferenceLink(RectTransform stage,string name,UiCopy caption,int icon,float y,Action click,float height=90)
        {
            var button=SettingsReferenceRow(stage,name,caption,y,height,click);SettingsReferenceIcon(button,icon);
            SettingsReferenceCaption(button,caption,140,0,310,height);
            var arrow=ReferenceRect("Settings open section",ReferenceButtonBody(button),458,(height-31)*.5f,20,31).gameObject.AddComponent<SettingsChevronGraphic>();
            arrow.color=new Color(.27f,.105f,.035f);arrow.raycastTarget=false;
        }
        void SettingsReferenceToggle(RectTransform stage,string name,int icon,float y,Func<bool> value,Action toggle)
        {
            UiCopy caption=name=="Music"?ReferenceCopy("Music","Музыка","Музика"):
                name=="Soundeffects"?ReferenceCopy("Game sound","Звук игры","Звук гри"):
                name=="Haptics"?ReferenceCopy("Vibration","Вибрация","Вібрація"):
                name=="Largertext"?ReferenceCopy("Larger text","Крупный текст","Великий текст"):
                ReferenceCopy("In-game help","Помощь в игре","Допомога в грі");
            var button=SettingsReferenceRow(stage,name,caption,y,98,()=>{toggle();ApplyPreferences();PersistProgress(true);DrawMenu();});
            SettingsReferenceIcon(button,icon);SettingsReferenceCaption(button,caption,140,0,242,98);
            ReferencePicture("Settings switch "+name,ReferenceButtonBody(button),SettingsReferenceArt.Icon(value()?12:13),378,19,106,60);
        }
    }
}
