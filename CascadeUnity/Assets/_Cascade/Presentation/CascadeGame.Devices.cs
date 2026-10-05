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
            public DemoLine leftStrap,rightStrap;
        }
        readonly List<RectTransform> deviceOverlays=new List<RectTransform>();
        readonly List<DeviceDock> deviceDocks = new List<DeviceDock>();
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
                var icon=Picture("Recognizable device",root,new Vector2(0,.16f),new Vector2(.57f,1),Resources.Load<Sprite>("CascadeArt/Devices/"+DeviceAsset(item.kind)),true);
                var pocket=Rect("Match this piece",root,new Vector2(.59f,.34f),new Vector2(.93f,.87f));
                Background(pocket,new Color(.99f,.91f,.70f,.96f),false).sprite=art.panel;
                var face=Rect("Required symbol",pocket,new Vector2(.13f,.13f),new Vector2(.87f,.87f)).gameObject.AddComponent<ShapeFaceGraphic>();
                face.SetShape(art.SymbolMesh(item.contact.RequiredSymbol),art.SymbolMaterial(item.contact.RequiredSymbol).GetColor("_BaseColor"));face.raycastTarget=false;
                if(item.contact.RequiredSymbol.Kind==SymbolKind.Glyph)Text("Required letter",pocket,Vector2.zero,Vector2.one,18,UiCopy.Raw(FaceText(item.contact.RequiredSymbol)));
                var caption=Text("Device state",root,new Vector2(-.08f,-.03f),new Vector2(1.08f,.24f),12,"");caption.font=art.titleFont;
                deviceDocks.Add(new DeviceDock{floor=floorIndex,kind=item.kind,root=root,socket=socket,icon=icon,caption=caption,contact=item.contact,slotX=slotX,dockX=x,leftStrap=leftStrap,rightStrap=rightStrap});
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
                d.icon.color=used?new Color(.62f,.72f,.66f,.70f):Color.white;
                UiCopy copy=used?new UiCopy(d.kind==2?(frame.BombPhase==BombPhase.Defused?"Safe":"Burst"):"Collected"):d.kind==2?new UiCopy("{0} moves",active?frame.BombTicks:floor.Bomb.Ticks):d.kind==9?new UiCopy("Far swap"):d.kind==10?new UiCopy("Pause bomb"):UiCopy.Raw("");
                if(d.kind==2&&active&&frame.BombPhase==BombPhase.Armed&&frame.FrozenTicks>0){copy=new UiCopy("Paused: {0}",frame.FrozenTicks);d.icon.color=new Color(.65f,.86f,1);}
                d.caption.SetCopy(copy);
            }
        }
    }
}
