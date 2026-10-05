using System.Linq;
using Cascade.Domain;
using Cascade.Solver;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void LiveDeviceDemo(RectTransform root,LevelDefinition level,int kind)
        {
            var resolver=new PuzzleResolver(level);var before=resolver.CreateInitialState();
            var goal=kind==8?new PuzzleGoal(1):PuzzleGoal.Delivery;
            var solution=PuzzleSolver.FindShortest(resolver,goal:goal);var command=solution.Path[0];var correct=resolver.Resolve(before,command);
            TransitionResult wrong=null;
            if(kind==2)for(int edge=0;edge<before.Row.Count-1;edge++) {
                var candidate=resolver.Resolve(before,PuzzleCommand.Swap(edge));
                if(candidate.Accepted&&candidate.Events.Any(e=>e.Kind==PuzzleEventKind.BombExploded)){wrong=candidate;break;}
            }
            var floor=level.Floors[0];
            var exchange=correct.Events.FirstOrDefault(e=>e.Kind==PuzzleEventKind.RowPermuted);
            var exchanged=exchange==null?before.Row:exchange.Row;
            int[] Destinations(System.Collections.Generic.IReadOnlyList<SymbolId> target){
                var destinations=new int[before.Row.Count];var used=new bool[target.Count];
                for(int source=0;source<destinations.Length;source++)for(int destination=0;destination<target.Count;destination++)
                    if(!used[destination]&&before.Row[source]==target[destination]){destinations[source]=destination;used[destination]=true;break;}
                return destinations;
            }
            var exchangeDest=Destinations(exchanged);var finalDest=Destinations(correct.State.Row);
            var wrongDest=wrong==null?exchangeDest:Destinations(wrong.State.Row);
            void Line(string name,Vector2 start,Vector2 end,Color shade,float thickness=3){
                var line=Rect(name,root,(start+end)*.5f,(start+end)*.5f);line.sizeDelta=new Vector2(0,thickness);
                var graphic=Background(line,shade,false);graphic.sprite=art.panel;
                // Size is resolved by the demo painter after the native layout settles.
                line.gameObject.AddComponent<DemoLine>().Initialize(root,start,end,thickness);
            }

            var board=Rect("Demo wood board",root,new Vector2(.02f,.40f),new Vector2(.98f,.50f));board.gameObject.AddComponent<WoodBoardGraphic>();
            var row=new ShapeFaceGraphic[before.Row.Count];var faces=new TextMeshProUGUI[row.Length];var ghosts=new ShapeFaceGraphic[row.Length];
            for(int i=0;i<row.Length;i++) {
                float x=(i+.5f)/row.Length;var cell=Rect("Demo piece "+i,root,new Vector2(x-.38f/row.Length,.51f),new Vector2(x+.38f/row.Length,.89f));
                row[i]=cell.gameObject.AddComponent<ShapeFaceGraphic>();row[i].raycastTarget=false;
                faces[i]=Text("Demo glyph",cell,Vector2.zero,Vector2.one,18,FaceText(before.Row[i]));faces[i].raycastTarget=false;
                row[i].SetShape(art.SymbolMesh(before.Row[i]),art.SymbolMaterial(before.Row[i]).GetColor("_BaseColor"));
                var ghost=Rect("Before piece "+i,root,new Vector2(x-.25f/row.Length,.84f),new Vector2(x+.25f/row.Length,1));ghosts[i]=ghost.gameObject.AddComponent<ShapeFaceGraphic>();
                var shade=art.SymbolMaterial(before.Row[i]).GetColor("_BaseColor");shade.a=.45f;ghosts[i].SetShape(art.SymbolMesh(before.Row[i]),shade);ghost.gameObject.SetActive(ReducedMotion);

            }
            var afterRim=Rect("Demo receiver",root,new Vector2(.02f,.01f),new Vector2(.98f,.15f));Background(afterRim,new Color(.94f,.73f,.32f),false).sprite=art.panel;
            for(int i=0;i<row.Length;i++) {
                float x=(i+.5f)/row.Length;var cell=Rect("Demo goal "+i,root,new Vector2(x-.30f/row.Length,.02f),new Vector2(x+.30f/row.Length,.14f));
                cell.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.SymbolMesh(level.TargetSymbolIds[i]),art.SymbolMaterial(level.TargetSymbolIds[i]).GetColor("_BaseColor"));
            }
            ContactDefinition contact=kind==2?floor.Bomb:kind==3?floor.Key:kind==4?floor.Repair:kind==8?floor.OptionalStamp:null;
            Image mount=null;ShapeFaceGraphic contactFace=null;Image[] pips=new Image[12],fuse=new Image[6];
            for(int i=0;i<pips.Length;i++)pips[i]=Background(Rect("Demo strength "+i,root,new Vector2(.03f+i*.042f,.30f),new Vector2(.057f+i*.042f,.34f)),new Color(.93f,.66f,.25f),false);
            if(contact!=null) {
                float x=(contact.Slot+.5f)/row.Length;
                var body=Rect("Demo device",root,new Vector2(x-.075f,.32f),new Vector2(x+.075f,.54f));mount=Background(body,kind==2?new Color(.9f,.35f,.22f):Selected,false);mount.sprite=art.panel;
                contactFace=Rect("Required piece",body,new Vector2(.2f,.2f),new Vector2(.8f,.8f)).gameObject.AddComponent<ShapeFaceGraphic>();
                contactFace.SetShape(art.SymbolMesh(contact.RequiredSymbol),art.SymbolMaterial(contact.RequiredSymbol).GetColor("_BaseColor"));contactFace.raycastTarget=false;
                if(kind==2)for(int i=0;i<fuse.Length;i++) {
                    float fx=x+(i-(floor.Bomb.Ticks-1)*.5f)*.04f;
                    fuse[i]=Background(Rect("Demo fuse "+i,root,new Vector2(fx-.012f,.22f),new Vector2(fx+.012f,.26f)),new Color(1,.70f,.20f),false);fuse[i].sprite=art.panel;
                }
            }
            var handSprite=Resources.Load<Sprite>("CascadeArt/TutorialHand");var hand=handSprite==null?null:Picture("Demo hand",root,new Vector2(.02f,.54f),new Vector2(.18f,.96f),handSprite,true);
            if(kind==6&&floor.LinkedPair.HasValue){
                foreach(int edge in new[]{floor.LinkedPair.Value.FirstEdge,floor.LinkedPair.Value.SecondEdge}){
                    float left=(edge+.5f)/row.Length,right=(edge+1.5f)/row.Length;
                    Line("Tied pair span",new Vector2(left,.48f),new Vector2(right,.48f),new Color(.55f,.36f,.69f),4);
                    Line("Left tied loop",new Vector2(left,.48f),new Vector2(left,.58f),new Color(.55f,.36f,.69f),4);
                    Line("Right tied loop",new Vector2(right,.48f),new Vector2(right,.58f),new Color(.55f,.36f,.69f),4);
                }
                Line("Both pairs connected",new Vector2(.375f,.48f),new Vector2(.625f,.48f),new Color(.55f,.36f,.69f),2);
            }
            if(kind==5)foreach(int edge in floor.FragileEdges){
                float x=(edge+1f)/row.Length;
                for(int cut=0;cut<4;cut++)Line("Cracked crossing",new Vector2(x+(cut%2==0?-.018f:.018f),.39f+cut*.035f),new Vector2(x+(cut%2==0?.018f:-.018f),.425f+cut*.035f),new Color(.76f,.27f,.16f),3);
            }
            RectTransform bridge=null;int gateEdge=kind==3?floor.Key.EdgeToOpen:floor.OneUseBridgeEdge??0;float gateX=(gateEdge+1f)/row.Length;
            var latch=Rect("Demo rope gate",root,new Vector2(gateX-.012f,.42f),new Vector2(gateX+.012f,.90f));Background(latch,new Color(.62f,.43f,.22f),false);latch.gameObject.SetActive(kind==3);
            if(kind==7){
                bridge=Rect("Demo folding bridge",root,new Vector2(gateX-.09f,.48f),new Vector2(gateX+.09f,.54f));bridge.gameObject.AddComponent<WoodBoardGraphic>();bridge.pivot=new Vector2(0,.5f);
                Line("Bridge hinge",new Vector2(gateX-.09f,.46f),new Vector2(gateX-.09f,.56f),Ink,4);
            }
            if(kind==1){
                Rect("Landing board",root,new Vector2(.02f,.11f),new Vector2(.98f,.18f)).gameObject.AddComponent<WoodBoardGraphic>();
                foreach(float x in new[]{.035f,.965f}){
                    var roller=Rect("Entry roller",root,new Vector2(x-.035f,.20f),new Vector2(x+.035f,.30f));Background(roller,Ink,false).sprite=art.panel;
                    var arrow=Rect("Roll left arrow",root,new Vector2(x-.022f,.21f),new Vector2(x+.022f,.29f));arrow.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.shapes[0],new Color(.98f,.75f,.31f));arrow.localRotation=Quaternion.Euler(0,0,90);
                }
            }
            var outcome=Text("Demo result",root,new Vector2(.51f,.20f),new Vector2(.99f,.32f),13,"");
            int lastStage=-1;var motion=root.gameObject.AddComponent<DeviceDemonstration>();motion.Initialize(this,(time,bad)=>{
                float local=bad?time-3.5f:time;var result=bad&&wrong!=null?wrong:correct;
                bool after=local>=1.8f;float blend=ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((local-1.1f)/.7f));
                for(int i=0;i<row.Length;i++) {
                    var destinations=bad&&wrong!=null?wrongDest:exchangeDest;
                    float fromX=(i+.5f)/row.Length,toX=(destinations[i]+.5f)/row.Length;
                    float x=Mathf.Lerp(fromX,toX,blend),y=.70f;
                    if(kind==1){
                        float fall=ReducedMotion?1:Mathf.Clamp01((local-.9f)/.65f);
                        y=Mathf.Lerp(.70f,.28f,fall*fall);
                        float shift=ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((local-1.55f)/.65f));x=Mathf.Lerp(fromX,(finalDest[i]+.5f)/row.Length,shift);
                    }else if(!ReducedMotion)y+=Mathf.Sin(blend*Mathf.PI)*(toX>=fromX?.12f:-.06f);
                    var cell=row[i].rectTransform;float half=ReducedMotion?.12f:.18f;
                    cell.anchorMin=new Vector2(x-.38f/row.Length,y-half);cell.anchorMax=new Vector2(x+.38f/row.Length,y+half);
                    cell.localScale=Vector3.one*(1+(ReducedMotion?0:Mathf.Sin(blend*Mathf.PI)*.05f));
                    ghosts[i].gameObject.SetActive(ReducedMotion);

                }
                int hp=floor.BaseDurability;foreach(var e in result.Events)if(e.Kind==PuzzleEventKind.DurabilityResolved&&e.Floor==0&&after)hp=Mathf.Max(0,e.Value);
                for(int i=0;i<pips.Length;i++){
                    pips[i].gameObject.SetActive(i<Mathf.Max(floor.BaseDurability,hp));
                    pips[i].color=after&&i>=hp?new Color(.83f,.31f,.20f,.45f):new Color(.93f,.66f,.25f);
                }
                bool blast=after&&result.Events.Any(e=>e.Kind==PuzzleEventKind.BombExploded),defused=after&&result.Events.Any(e=>e.Kind==PuzzleEventKind.BombDefused);
                if(mount!=null)mount.color=defused?new Color(.30f,.70f,.62f):blast?Ink:kind==2?new Color(.90f,.35f,.22f):new Color(.94f,.73f,.32f);
                if(kind==2)for(int i=0;i<fuse.Length;i++)fuse[i].gameObject.SetActive(!after&&i<floor.Bomb.Ticks);
                board.gameObject.SetActive(!blast&&(kind!=1||!after));latch.gameObject.SetActive(kind==3?!after:kind==7&&after);
                if(bridge!=null)bridge.localRotation=Quaternion.Euler(0,0,-78*(ReducedMotion?1:Mathf.Clamp01((local-1.7f)/.4f)));
                int stage=(after?1:0)+(bad?2:0);if(stage!=lastStage){lastStage=stage;outcome.SetCopy(after?(blast?"Board breaks":defused?"Bomb defused":kind==3?"Unlocked":kind==4?"Repaired":kind==8?"Stamp collected":"then down"):"");}
                if(hand!=null) {
                    hand.gameObject.SetActive(!ReducedMotion&&local<2.3f);
                    int start=command.Kind==CommandKind.Swap?command.Edge:0;float x=(start+.5f+blend)/row.Length;
                    hand.rectTransform.anchorMin=new Vector2(x-.09f,.54f);hand.rectTransform.anchorMax=new Vector2(x+.09f,.96f);
                }
            });
        }
    }
}
