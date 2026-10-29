using System;
using System.Linq;
using Cascade.Content;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        int completedWorld=-1;
        readonly Sprite[] worldCompletionIcons=new Sprite[9];
        public int CompletedWorld=>completedWorld;
        Sprite WorldCompletionDestinationIcon(int world)
        {
            if(worldCompletionIcons[world]!=null)return worldCompletionIcons[world];
            // Tight sprite regions reuse the original nine island textures. Their
            // transparent export gutters must not become extra icon/label gaps.
            var ink=new[]{new Rect(15,8,479,500),new Rect(15,15,420,448),new Rect(15,15,424,440),new Rect(14,8,466,500),new Rect(15,15,416,450),new Rect(15,15,399,429),new Rect(15,8,436,521),new Rect(15,15,434,489),new Rect(15,15,435,446)};
            var size=new[]{new Vector2(509,523),new Vector2(450,478),new Vector2(453,469),new Vector2(495,523),new Vector2(446,479),new Vector2(429,459),new Vector2(466,544),new Vector2(464,519),new Vector2(465,476)};
            return worldCompletionIcons[world]=AuthoredIconRegion(WorldIslandArt.Texture(world,4),ink[world],size[world].x,size[world].y);
        }
        int WorldCompletionReceipt(int world)=>9600+(int)PreferredMode*16+world;
        bool TryWorldCompletion()
        {
            if(PersonalMissionActive||profile.Completion==null||!profile.Completion.Acknowledged)return false;
            int stop=Catalog.PrimaryLevels.ToList().FindIndex(c=>c.Definition.CampaignNodeId==profile.Completion.NodeId);
            if(stop<0)return false;int world=JourneyStory.Zone(stop);
            if(RemembersStory(WorldCompletionReceipt(world)))return false;
            var ids=Catalog.PrimaryLevels.Skip(world*60).Take(60).Select(c=>c.Definition.CampaignNodeId).ToHashSet();
            var offered=JourneySequence().Where(c=>ids.Contains(c.Definition.CampaignNodeId)).ToArray();
            if(offered.Length==0||offered.Any(c=>!sessionClears.Contains(c.Definition.CampaignNodeId)))return false;
            if(world<JourneyStory.WorldCount-1){
                var nextIds=Catalog.PrimaryLevels.Skip((world+1)*60).Take(60).Select(c=>c.Definition.CampaignNodeId).ToHashSet();
                if(!JourneySequence().Any(c=>nextIds.Contains(c.Definition.CampaignNodeId)&&IsContentOpen(c)))return false;
            }
            CancelConversation();completedWorld=world;islandBrowseIndex=-1;SetScreen(GameScreen.WorldComplete);feedback.Sound(ToyCue.Unlock);return true;
        }
        bool AcknowledgeWorldCompletion()
        {
            if(CurrentScreen!=GameScreen.WorldComplete||completedWorld<0||saveBlocked||HasSaveError)return false;
            int receipt=WorldCompletionReceipt(completedWorld);bool added=heardStory.Add(receipt);PersistProgress(true);
            if(!HasSaveError)return true;
            if(added)heardStory.Remove(receipt);profile.HeardStory=heardStory.OrderBy(p=>p).ToList();return false;
        }
        void OpenCompletedWorldDestination()
        {
            if(!AcknowledgeWorldCompletion())return;
            int next=Mathf.Min(completedWorld+1,JourneyStory.WorldCount-1);mapPage=next;islandBrowseIndex=-1;
            selectedJourneyStop=Enumerable.Range(next*60,60).Where(IsJourneyStopOpen)
                .OrderBy(n=>sessionClears.Contains(Catalog.PrimaryLevels[n].Definition.CampaignNodeId)?1:0).First();
            ShowSelectedJourney();
        }
        void WorldCompletionHome(){if(AcknowledgeWorldCompletion())ShowHome();}
        void DrawWorldCompletion()
        {
            LoadNavigationArt();machine.SetZone(completedWorld);
            var frame=Rect("Reference shop environment",menuRoot,Vector2.one*.5f,Vector2.one*.5f);
            var background=frame.gameObject.AddComponent<Image>();background.sprite=ReferenceShopArt("Backdrop");background.raycastTarget=false;
            var stage=Rect("World completion composition",menuRoot,Vector2.one*.5f,Vector2.one*.5f);
            stage.sizeDelta=new Vector2(720,1280);var layout=stage.gameObject.AddComponent<ReferenceShopLayout>();layout.Initialize(menuRoot,frame);
            var sky=Rect("Living home sky",stage,Vector2.zero,Vector2.one);
            ReferencePicture("Drifting home island 0",sky,WorldIslandArt.Get(completedWorld,0),-9,300,211,320);
            ReferencePicture("Drifting home island 1",sky,WorldIslandArt.Get(completedWorld,1),505,185,235,340);
            var heroRect=ReferenceRect("World completion Mika",stage,92,140,536,536);
            var hero=heroRect.gameObject.AddComponent<RawImage>();hero.raycastTarget=false;
            var board=ReferencePicture("World completion illustrated board",stage,TutorialReferenceArt.Panel,35,598,650,614,false);
            board.type=Image.Type.Sliced;board.pixelsPerUnitMultiplier=2.5f;
            var paw=ReferenceRect("World completion resting paw",stage,92,140,536,536).gameObject.AddComponent<RawImage>();paw.raycastTarget=false;
            stage.gameObject.AddComponent<WorldCompletionMika>().Initialize(this,hero,paw,board,completedWorld);
            var headingArea=ReferenceRect("World completion heading inset",stage,84,667,552,71);
            var heading=ReferenceText("world completion heading",headingArea,WorldCompletionTitle(completedWorld),0,0,552,71,49);heading.fontSizeMin=30;
            var headingFit=CentreContent(headingArea,heading.rectTransform);headingFit.Padding=8;headingFit.Refresh();
            ReferenceRect("World completion gold divider",stage,129,747,462,18).gameObject.AddComponent<WorldCompletionDivider>().raycastTarget=false;
            var body=ReferenceText("world completion story",stage,WorldCompletionBody(completedWorld),84,778,552,134,27,false,true);
            body.font=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??art.titleFont;body.fontStyle=FontStyles.Bold;body.fontSizeMin=23;body.alignment=TextAlignmentOptions.Center;
            var next=ReferenceButton("Open next world",stage,75,930,570,120,OpenCompletedWorldDestination,"GreenAction");
            var nextBody=(RectTransform)ReferenceButtonBody(next);
            var emblem=ReferencePicture("World completion destination island",nextBody,WorldCompletionDestinationIcon(Mathf.Min(completedWorld+1,8)),18,15,94,90).rectTransform;
            var caption=ReferenceText("world completion next",nextBody,completedWorld<8?ReferenceCopy("Open the next world","Открыть новый мир","Відкрити новий світ"):ReferenceCopy("Our journey","Наше путешествие","Наша подорож"),118,23,416,74,36,true);
            var nextFit=CentreContent(nextBody,emblem,caption.rectTransform);nextFit.Padding=18;nextFit.Refresh();layout.BindTouch((RectTransform)next.transform,new Vector2(570,120));
            var home=ReferenceButton("World completion home",stage,140,1070,440,92,WorldCompletionHome,"AmberAction");
            var homeBody=(RectTransform)ReferenceButtonBody(home);var icon=ReferenceRect("World completion home icon",homeBody,29,21,47,48).gameObject.AddComponent<PauseGlyphGraphic>();icon.Configure(0);
            var homeCaption=ReferenceText("world completion home",homeBody,ReferenceCopy("Home","На главную","На головну"),91,15,309,62,35);
            var homeFit=CentreContent(homeBody,icon.rectTransform,homeCaption.rectTransform);homeFit.Padding=18;homeFit.Refresh();layout.BindTouch((RectTransform)home.transform,new Vector2(440,92));
            next.interactable=home.interactable=!saveBlocked&&!HasSaveError;
        }
        UiCopy WorldCompletionTitle(int world)
        {
            string[] en={
                "The first lamps are lit!",
                "Water Reaches the Houses",
                "The empty crate route",
                "An Order with Ada's Seal",
                "The northbound route",
                "Ada Has the Receipt",
                "A path through snow",
                "Ready for its return",
                "Rin's first watch"};
            string[] ru={
                "Первые огни зажжены!",
                "Вода дошла до домов",
                "Маршрут пустого ящика",
                "Заказ с печатью Ады",
                "Путь на север",
                "Расписка у Ады",
                "Путь через снег",
                "Маяк ждёт Фонарь",
                "Первая вахта Рин"};
            string[] uk={
                "Перші вогні запалені!",
                "Вода дійшла до будинків",
                "Маршрут порожнього ящика",
                "Замовлення з печаткою Ади",
                "Шлях на північ",
                "Розписка в Ади",
                "Шлях крізь сніг",
                "Маяк чекає на Ліхтар",
                "Перша варта Рін"};
            return ReferenceCopy(en[world],ru[world],uk[world]);
        }
        UiCopy WorldCompletionBody(int world)
        {
            string[] en={
                "The first lamps are lit.\nThe relay failed before the rain.\nBen saw a crate pass his mill.\nWe'll ask what happened there.",
                "The mill and water pipe work.\nThe crate passed before the storm.\nRose's seal is on its packing.\nThe next stop is her orchard.",
                "The seedling beds are warm.\nRose supplied an empty crate.\nNora collected the load at Ada's.\nWe'll check the delivery order.",
                "The post route is open.\nAda's seal is on the order.\nIts instructions look unsafe.\nClara saw the northbound flight.",
                "The boats can pass again.\nThe crate was glowing inside.\nClara was never told of a trial.\nThe archive holds the work records.",
                "The archive records are safe.\nAda approved a north lens trial.\nFelix signed for the service key.\nAda can trace the permit number.",
                "The pass is marked on Oscar's map.\nThe shelter is warm and stocked.\nAda is answering our signal.\nWe'll ask her about the permit.",
                "The beacon is ready for its Lantern.\nAda denies authorising its removal.\nThe signed conditions went with the load.\nWe'll compare them at the observatory.",
                "The High Lantern is home.\nThe north route stays lit.\nFelix begins the repairs with Walter.\nAt dawn, Rin has lamps to check."};
            string[] ru={
                "Первые фонари горят.\nРеле отказало до дождя.\nБен видел ящик у мельницы.\nУзнаем, что там произошло.",
                "Мельница и водопровод работают.\nЯщик пролетел до бури.\nНа упаковке печать Розы.\nСледующая остановка — её сад.",
                "Рассада в тепле.\nРоза дала пустой ящик.\nНора забрала груз у Ады.\nСверим заказ на перевозку.",
                "Почтовый путь открыт.\nНа заказе печать Ады.\nУказания в нём опасны.\nКлара видела рейс на север.",
                "Лодки снова проходят.\nВнутри ящика горел свет.\nКларе не сообщили об испытании.\nЗаписи о работах хранятся в архиве.",
                "Записи архива спасены.\nАда разрешила испытать линзу.\nФеликс взял рабочий ключ.\nПо номеру найдём условия работ.",
                "Перевал отмечен на карте Оскара.\nВ приюте тепло и есть припасы.\nАда отвечает на наш сигнал.\nСпросим её о разрешении.",
                "Маяк готов принять Фонарь.\nАда не разрешала его забирать.\nПодписанные условия ушли с грузом.\nСверим их в обсерватории.",
                "Высший Фонарь вернулся домой.\nСеверный путь остаётся освещённым.\nФеликс начинает ремонт с Вальтером.\nНа рассвете Рин выходит на обход."};
            string[] uk={
                "Перші ліхтарі горять.\nРеле відмовило до дощу.\nБен бачив ящик біля млина.\nДізнаємося, що там сталося.",
                "Млин і водогін працюють.\nЯщик пролетів до бурі.\nНа пакуванні печатка Рози.\nНаступна зупинка — її сад.",
                "Розсада в теплі.\nРоза дала порожній ящик.\nНора забрала вантаж в Ади.\nЗвіримо замовлення на перевезення.",
                "Поштовий шлях відкрито.\nНа замовленні печатка Ади.\nВказівки в ньому небезпечні.\nКлара бачила рейс на північ.",
                "Човни знову проходять.\nУсередині ящика горіло світло.\nКларі не повідомили про випробування.\nЗаписи про роботи зберігаються в архіві.",
                "Записи архіву врятовані.\nАда дозволила випробувати лінзу.\nФелікс узяв робочий ключ.\nЗа номером знайдемо умови робіт.",
                "Перевал позначено на карті Оскара.\nУ прихистку тепло та є припаси.\nАда відповідає на наш сигнал.\nСпитаємо її про дозвіл.",
                "Маяк готовий прийняти Ліхтар.\nАда не дозволяла його забирати.\nПідписані умови пішли з вантажем.\nЗвіримо їх в обсерваторії.",
                "Вищий Ліхтар повернувся додому.\nПівнічний шлях лишається освітленим.\nФелікс починає ремонт із Вальтером.\nНа світанку Рін виходить на обхід."};
            return ReferenceCopy(en[world],ru[world],uk[world]);
        }
    }
}
