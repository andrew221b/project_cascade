using System.Collections.Generic;
using System.Linq;
using Cascade.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        sealed class DeviceDock
        {
            public int floor, kind;
            public RectTransform root;
            public RectTransform socket;
            public Image icon;
            public TextMeshProUGUI caption;
            public ContactDefinition contact;
            public float slotX,dockX,angle,angleVelocity;
            public Vector2 anchorPosition,anchorVelocity;
            public bool anchorReady;
            public float responseTime;
            public float successTime;
            public bool settledContact;
            public MaskableGraphic outcomeRibbon;
            public Sprite normalSprite,safeSprite;
            public DemoLine leftStrap,rightStrap;
        }
        readonly List<RectTransform> deviceOverlays=new List<RectTransform>();
        readonly List<DeviceDock> deviceDocks = new List<DeviceDock>();
        readonly Dictionary<int,Sprite> toolBodySprites=new Dictionary<int,Sprite>();
        Sprite ToolBody(int kind)
        {
            if(kind==2)return Resources.Load<Sprite>("CascadeArt/Devices/Bomb");
            if(toolBodySprites.TryGetValue(kind,out var sprite))return sprite;
            int index=kind==3?0:kind==4?1:kind==8?2:kind==9?3:4;
            sprite=WorkshopSpriteSet.Tool(Resources.Load<Texture2D>("CascadeArt/Reactions/ToolBodies"),index);toolBodySprites.Add(kind,sprite);return sprite;
        }
        static string DeviceAsset(int kind) => kind==2?"Bomb":kind==3?"BrassKey":kind==4?"RepairKit":kind==8?"Stamp":kind==9?"RibbonSpool":"Hourglass";
        void OpenDevice(int kind) { returnScreen=CurrentScreen;deviceIndex=kind;SetScreen(GameScreen.Devices); }
        void BuildDeviceDocks(RectTransform card,FloorDefinition floor,int count,int floorIndex)
        {
            card=Rect("Device attachments",card,Vector2.zero,Vector2.one);deviceOverlays.Add(card);
            var contacts=new List<(ContactDefinition contact,int kind)>();
            if(floor.Bomb!=null)contacts.Add((floor.Bomb,2));
            if(floor.Key!=null)contacts.Add((floor.Key,3));
            if(floor.Repair!=null)contacts.Add((floor.Repair,4));
            if(floor.OptionalStamp!=null)contacts.Add((floor.OptionalStamp,8));
            if(floor.Powerup!=null)contacts.Add((floor.Powerup,floor.Powerup.Kind==PowerupKind.Ribbon?9:10));
            contacts.Sort((a,b)=>a.contact.Slot.CompareTo(b.contact.Slot));
            var woodLabel=floorLabels[floorIndex].rectTransform;
            woodLabel.SetParent(card,false);
            float labelX=contacts.Count==0?.5f:contacts.Count==1?(contacts[0].contact.Slot<count*.5f?.76f:.24f):.5f;
            woodLabel.anchorMin=new Vector2(labelX-.23f,.09f);woodLabel.anchorMax=new Vector2(labelX+.23f,.27f);
            float previous=-1;int dockIndex=0;
            foreach(var item in contacts) {
                float slotX=.045f+.91f*(item.contact.Slot+.5f)/count;
                float x=contacts.Count==2?(dockIndex++==0?.18f:.82f):Mathf.Clamp(slotX,.15f,.85f);if(x-previous<.28f)x=Mathf.Min(.86f,previous+.28f);previous=x;
                // A wooden socket and its suspended tool share a real attachment point.
                var socket=Rect("Device socket",card,new Vector2(slotX-.034f,.295f),new Vector2(slotX+.034f,.39f));
                Background(socket,new Color(.31f,.22f,.15f),false).sprite=art.panel;
                DemoLine leftStrap=null,rightStrap=null;
                for(int side=-1;side<=1;side+=2) {
                    var strap=Rect("Device leather strap",card,Vector2.zero,Vector2.zero);
                    var leather=Background(strap,new Color(.55f,.36f,.18f),false);leather.sprite=art.panel;leather.type=Image.Type.Sliced;
                    var line=strap.gameObject.AddComponent<DemoLine>();line.Initialize(card,new Vector2(slotX+side*.018f,.32f),new Vector2(x+side*.055f,.10f),2.2f);
                    if(side<0)leftStrap=line;else rightStrap=line;
                }
                var button=Button(UiCopy.Raw("Device "+DeviceAsset(item.kind)),card,new Vector2(x,-.075f),new Vector2(x,-.075f),()=>OpenDevice(item.kind));
                var root=(RectTransform)button.transform;root.sizeDelta=new Vector2(88,55);
                button.GetComponent<Image>().color=Color.clear;button.GetComponentInChildren<TextMeshProUGUI>().SetCopy("");
                var icon=Picture("Recognizable device",root,new Vector2(.05f,-.10f),new Vector2(.88f,1.14f),ToolBody(item.kind),true);
                if(item.kind==2)BuildBombMark(icon.rectTransform,item.contact.RequiredSymbol);else BuildToolBodyMark(icon.rectTransform,item.kind,item.contact.RequiredSymbol);
                var ribbon=Rect("Contact outcome ribbon",root,new Vector2(-.04f,-.59f),new Vector2(1.66f,-.06f)).gameObject.AddComponent<ContactRibbonGraphic>();ribbon.color=new Color(.10f,.40f,.34f);ribbon.raycastTarget=false;ribbon.gameObject.SetActive(false);
                var caption=Text("Device state",root,new Vector2(-.08f,-.50f),new Vector2(1.08f,-.10f),12,"");caption.font=art.titleFont;
                deviceDocks.Add(new DeviceDock{floor=floorIndex,kind=item.kind,root=root,socket=socket,icon=icon,normalSprite=icon.sprite,safeSprite=item.kind==2?Resources.Load<Sprite>("CascadeArt/Devices/BombSafe"):null,outcomeRibbon=ribbon,caption=caption,contact=item.contact,slotX=slotX,dockX=x,leftStrap=leftStrap,rightStrap=rightStrap});
            }
            UiCopy landing=floor.EntryTransform==EntryTransform.MirrorNext?"Reverse on landing":floor.EntryTransform==EntryTransform.ShiftLeftOne?"Slide left on landing":"Slide right on landing";
            if(IsWindFloor(floorIndex))landing=floor.EntryTransform==EntryTransform.ShiftLeftOne?"Wind on landing: row slides left":"Wind on landing: row slides right";
            if(descent!=null&&floorIndex==0&&IsWindFloor(0))landing=floor.EntryTransform==EntryTransform.ShiftLeftOne?"Arrival wind: row moved left":"Arrival wind: row moved right";
            if(floor.EntryTransform!=EntryTransform.None||floor.LinkedPair.HasValue) {
                UiCopy description=floor.LinkedPair.HasValue?(floor.EntryTransform==EntryTransform.None?new UiCopy("Paired swaps"):UiCopy.Join(" · ",new[]{landing,new UiCopy("Paired swaps")})):landing;
                bool belowTools=descent!=null&&contacts.Count>0;
                var label=Text("Landing movement",card,new Vector2(.02f,belowTools?-.44f:-.16f),new Vector2(.98f,belowTools?-.27f:.01f),12,description);
                var b=label.gameObject.AddComponent<Button>();b.onClick.AddListener(()=>OpenDevice(floor.LinkedPair.HasValue?6:1));label.raycastTarget=true;
            }
            if(floor.LinkedPair.HasValue) {
                var pair=floor.LinkedPair.Value;float a=.045f+.91f*(pair.FirstEdge+1f)/count,b=.045f+.91f*(pair.SecondEdge+1f)/count;
                void Cord(Vector2 from,Vector2 to,float width) {var r=Rect("Linked pair cord",card,Vector2.zero,Vector2.zero);Background(r,new Color(.48f,.28f,.62f),false);r.gameObject.AddComponent<DemoLine>().Initialize(card,from,to,width);}
                Cord(new Vector2(a,.34f),new Vector2(a,.28f),3);Cord(new Vector2(a,.28f),new Vector2(b,.28f),3);Cord(new Vector2(b,.28f),new Vector2(b,.34f),3);

            }
        }
        void BuildBombMark(RectTransform icon,SymbolId required)
        {
            // Paint the contact on the metal body; it travels with the bomb, without a second prop.
            var mark=Rect("Bomb painted mark",icon,new Vector2(.26f,.11f),new Vector2(.70f,.58f));
            var paint=new Color(1,.95f,.76f);
            if(required.Kind==SymbolKind.Glyph){
                var letter=Text("Bomb painted letter",mark,new Vector2(0,-.15f),new Vector2(1,1.15f),22,UiCopy.Raw(FaceText(required)));
                letter.font=art.titleFont;letter.fontStyle=FontStyles.Bold;letter.color=paint;letter.margin=Vector4.zero;
                letter.alignment=TextAlignmentOptions.Center;letter.raycastTarget=false;letter.gameObject.AddComponent<SymbolGlyphCenter>();
            }else mark.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.SymbolMesh(required),paint);
        }
        void BuildToolBodyMark(RectTransform icon,int kind,SymbolId required)
        {
            var bounds=kind==3?new Rect(.33f,.51f,.34f,.32f):kind==4?new Rect(.35f,.165f,.30f,.30f):kind==8?new Rect(.33f,.22f,.36f,.36f):kind==9?new Rect(.21f,.35f,.32f,.34f):new Rect(.36f,.18f,.28f,.28f);
            var mark=Rect("Body painted symbol",icon,bounds.min,bounds.max);
            if(required.Kind==SymbolKind.Glyph){
                var letter=Text("Body painted letter",mark,Vector2.zero,Vector2.one,24,UiCopy.Raw(FaceText(required)));
                letter.font=art.titleFont;letter.fontStyle=FontStyles.Bold;letter.margin=Vector4.zero;letter.color=Ink;letter.raycastTarget=false;
                letter.enableAutoSizing=true;letter.fontSizeMin=12;letter.fontSizeMax=28;letter.gameObject.AddComponent<SymbolGlyphCenter>();
            }else {var face=mark.gameObject.AddComponent<ShapeFaceGraphic>();face.raycastTarget=false;face.SetShape(art.SymbolMesh(required),art.SymbolMaterial(required).GetColor("_BaseColor"),true);}
        }
        public void CueDevice(PuzzleEvent e)
        {
            foreach(var d in deviceDocks)if(d.floor==e.Floor){
                bool reacts=d.kind==2&&(e.Kind==PuzzleEventKind.BombTicked||e.Kind==PuzzleEventKind.BombExploded||e.Kind==PuzzleEventKind.BombDefused)
                    ||d.kind==3&&e.Kind==PuzzleEventKind.KeyOpened||d.kind==4&&e.Kind==PuzzleEventKind.Repaired
                    ||d.kind==8&&e.Kind==PuzzleEventKind.StampCollected||d.kind==9&&e.Kind==PuzzleEventKind.RibbonCollected||d.kind==10&&e.Kind==PuzzleEventKind.HourglassCollected;
                if(reacts){d.responseTime=.32f;if(e.Kind!=PuzzleEventKind.BombTicked&&e.Kind!=PuzzleEventKind.BombExploded)d.successTime=1.2f;if(!ReducedMotion)d.angleVelocity+=(e.Kind==PuzzleEventKind.BombExploded?-32:16);}
            }
        }
        public Vector3 DeviceVisualOrigin(int floor,int kind)
        {
            var dock=deviceDocks.FirstOrDefault(d=>d.floor==floor&&d.kind==kind);
            if(dock==null)return machine.FloorPosition(floor);
            // UI props live on the camera canvas. Emit in the feedback plane at the same screen point.
            return UIFeedbackPoint(dock.icon.rectTransform);
        }
        public RectTransform DeviceFeedbackAnchor(int floor,int kind)=>deviceDocks.FirstOrDefault(d=>d.floor==floor&&d.kind==kind)?.icon.rectTransform;
        public Vector3 UIFeedbackPoint(RectTransform rect)
        {
            var body=rect.TransformPoint(rect.rect.min+Vector2.Scale(rect.rect.size,new Vector2(.48f,.35f)));
            var screen=uiCamera.WorldToScreenPoint(body);
            var ray=uiCamera.ScreenPointToRay(screen);
            var plane=new Plane(Vector3.forward,new Vector3(0,0,-.7f));
            return plane.Raycast(ray,out var distance)?ray.GetPoint(distance):body;
        }
        bool deviceForecast;
        void ClearDeviceForecast()
        {
            if(!deviceForecast||VisibleFrame==null)return;deviceForecast=false;
            int f=VisibleFrame.ActiveFloor;if(f<Interaction.Attempt.Level.Floors.Count)PaintDeviceDocks(VisibleFrame,f,false,true);
        }
        void PreviewDeviceDocks(TransitionResult prediction)
        {
            deviceForecast=true;int floor=Interaction.Attempt.Current.ActiveFloor;
            foreach(var d in deviceDocks)if(d.floor==floor) {
                foreach(var e in prediction.Events)if(e.Floor==floor) {
                    bool applies=d.kind==2&&(e.Kind==PuzzleEventKind.BombDefused||e.Kind==PuzzleEventKind.BombExploded||e.Kind==PuzzleEventKind.BombFrozen)
                        ||d.kind==3&&e.Kind==PuzzleEventKind.KeyOpened||d.kind==4&&e.Kind==PuzzleEventKind.Repaired
                        ||d.kind==8&&e.Kind==PuzzleEventKind.StampCollected||d.kind==9&&e.Kind==PuzzleEventKind.RibbonCollected||d.kind==10&&e.Kind==PuzzleEventKind.HourglassCollected;
                    if(!applies)continue;
                    bool blast=e.Kind==PuzzleEventKind.BombExploded;
                    d.icon.color=blast?new Color(1,.52f,.38f):new Color(.67f,1,.83f);
                    d.caption.SetCopy(blast?"Burst":d.kind==2?(e.Kind==PuzzleEventKind.BombFrozen?"Pause bomb":"Safe"):d.kind==3?"Unlocked":d.kind==4?"Repaired":d.kind==9?"Far swap":d.kind==10?"Pause bomb":"Collected");
                }
            }
        }
        void PaintDeviceDocks(PresentationFrame frame,int floorIndex,bool past,bool active)
        {
            var floor=Interaction.Attempt.Level.Floors[floorIndex];
            foreach(var d in deviceDocks)if(d.floor==floorIndex) {
                d.root.gameObject.SetActive(!past);
                bool used=active&&(d.kind==2?frame.BombPhase!=BombPhase.Armed:d.kind==3?frame.KeyUsed:d.kind==4?frame.RepairUsed:d.kind==8?(frame.StampBits&(1<<floorIndex))!=0:frame.PowerupUsed);
                d.settledContact=used;d.outcomeRibbon.gameObject.SetActive(used);
                d.outcomeRibbon.color=d.kind==2&&frame.BombPhase==BombPhase.Exploded?new Color(.62f,.20f,.14f):d.kind==3?new Color(.42f,.29f,.12f):d.kind==8?new Color(.34f,.23f,.49f):d.kind>=9?new Color(.10f,.33f,.49f):new Color(.10f,.40f,.34f);
                d.caption.rectTransform.anchorMin=used?new Vector2(.02f,-.50f):new Vector2(-.08f,-.50f);d.caption.rectTransform.anchorMax=used?new Vector2(1.60f,-.10f):new Vector2(1.08f,-.10f);
                d.caption.color=used?MenuPaper:Ink;
                if(d.kind==2)d.icon.sprite=used&&frame.BombPhase==BombPhase.Defused&&d.safeSprite!=null?d.safeSprite:d.normalSprite;
                d.icon.color=used&&d.kind!=2?new Color(.85f,.94f,.88f):Color.white;
                UiCopy copy=used?new UiCopy(d.kind==2?(frame.BombPhase==BombPhase.Defused?"Safe":"Burst"):d.kind==3?"Unlocked":d.kind==4?"Repaired":d.kind==9?"Far swap":d.kind==10?"Pause bomb":"Collected"):d.kind==2?new UiCopy("{0} moves",active?frame.BombTicks:floor.Bomb.Ticks):d.kind==9?new UiCopy("Far swap"):d.kind==10?new UiCopy("Pause bomb"):UiCopy.Raw("");
                if(d.kind==2&&active&&frame.BombPhase==BombPhase.Armed&&frame.FrozenTicks>0){copy=new UiCopy("Paused: {0}",frame.FrozenTicks);d.icon.color=new Color(.65f,.86f,1);}
                d.caption.SetCopy(copy);
            }
        }
    }
}
