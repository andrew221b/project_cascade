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
        void LiveDeviceDemo(RectTransform root,LevelDefinition level,int kind,bool isolatedClock=false)
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
                row[i].SetShape(art.SymbolMesh(before.Row[i]),art.SymbolMaterial(before.Row[i]).GetColor("_BaseColor"),true);
                var ghost=Rect("Before piece "+i,root,new Vector2(x-.25f/row.Length,.84f),new Vector2(x+.25f/row.Length,1));ghosts[i]=ghost.gameObject.AddComponent<ShapeFaceGraphic>();
                var shade=art.SymbolMaterial(before.Row[i]).GetColor("_BaseColor");shade.a=.45f;ghosts[i].SetShape(art.SymbolMesh(before.Row[i]),shade);ghost.gameObject.SetActive(ReducedMotion);

            }
            if(kind!=1){
            var afterRim=Rect("Demo receiver",root,new Vector2(.02f,.01f),new Vector2(.98f,.15f));Background(afterRim,new Color(.94f,.73f,.32f),false).sprite=art.panel;
            for(int i=0;i<row.Length;i++) {
                float x=(i+.5f)/row.Length;var cell=Rect("Demo goal "+i,root,new Vector2(x-.30f/row.Length,.02f),new Vector2(x+.30f/row.Length,.14f));
                cell.gameObject.AddComponent<ShapeFaceGraphic>().SetShape(art.SymbolMesh(level.TargetSymbolIds[i]),art.SymbolMaterial(level.TargetSymbolIds[i]).GetColor("_BaseColor"),true);
            }
            }
            ContactDefinition contact=kind==2?floor.Bomb:kind==3?floor.Key:kind==4?floor.Repair:kind==8?floor.OptionalStamp:kind>=9?floor.Powerup:null;
            Image mount=null;HangingToolTag tag=null;
            Sprite armedBomb=null,safeBomb=kind==2?Resources.Load<Sprite>("CascadeArt/Devices/BombSafe"):null;
            var woodCount=Text("Wood moves",root,new Vector2(.01f,.20f),new Vector2(.48f,.35f),13,"");
            woodCount.gameObject.SetActive(kind==0);
            if(contact!=null) {
                float x=Mathf.Clamp((contact.Slot+.5f)/row.Length,.18f,.82f);
                var body=Rect("Demo device",root,new Vector2(x-.13f,.20f),new Vector2(x+.13f,.49f));
                mount=Picture("Tool",body,Vector2.zero,Vector2.one,ToolBody(kind),true);
                mount.material=ToolSilhouette;
                if(kind==2)BuildBombMark(mount.rectTransform,contact.RequiredSymbol);else BuildToolBodyMark(mount.rectTransform,kind,contact.RequiredSymbol);
                if(kind==2)armedBomb=mount.sprite;
                Line("Device strap",new Vector2(x,.45f),new Vector2(x,.51f),new Color(.4f,.27f,.15f),3);
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
            if(kind==5)board.GetComponent<WoodBoardGraphic>().SetDamage(2);
            RectTransform bridge=null;int gateEdge=kind==3?floor.Key.EdgeToOpen:floor.OneUseBridgeEdge??0;float gateX=(gateEdge+1f)/row.Length;
            var latch=Rect("Demo rope gate",root,new Vector2(gateX-.012f,.42f),new Vector2(gateX+.012f,.90f));Background(latch,new Color(.62f,.43f,.22f),false);latch.gameObject.SetActive(kind==3);
            if(kind==7){
                bridge=Rect("Demo folding bridge",root,new Vector2(gateX-.09f,.48f),new Vector2(gateX+.09f,.54f));bridge.gameObject.AddComponent<WoodBoardGraphic>();bridge.pivot=new Vector2(0,.5f);
                Line("Bridge hinge",new Vector2(gateX-.09f,.46f),new Vector2(gateX-.09f,.56f),Ink,4);
            }
            if(kind==1){
                var landing=Rect("Landing board",root,new Vector2(.02f,.08f),new Vector2(.98f,.19f)).gameObject.AddComponent<WoodBoardGraphic>();
                landing.SetSurface(level.Floors[1]);
            }
            bool beneath=CurrentScreen==GameScreen.PowerupChallenges||CurrentScreen==GameScreen.Devices&&deviceIndex>=0;
            var outcome=Text("Demo result",root,new Vector2(0,-.14f),new Vector2(1,-.015f),beneath?20:16,"");
            if(isolatedClock)outcome.color=MenuPaper;
            if(beneath)menuRows.Add(new MenuRow{rect=outcome.rectTransform,top=0,height=28,offset=4});
            int lastStage=-1;var motion=root.gameObject.AddComponent<DeviceDemonstration>();motion.Initialize(this,(time,bad)=>{
                tag?.Tick(Time.unscaledDeltaTime,Machine.WindAt(0),0,!Interaction.IsSuspended,ReducedMotion);
                float local=bad?time-3.5f:time;var result=bad&&wrong!=null?wrong:correct;
                bool after=local>=1.8f;float blend=ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((local-1.1f)/.7f));
                for(int i=0;i<row.Length;i++) {
                    var destinations=bad&&wrong!=null?wrongDest:exchangeDest;
                    float fromX=(i+.5f)/row.Length,toX=(destinations[i]+.5f)/row.Length;
                    float x=Mathf.Lerp(fromX,toX,blend),y=.70f;
                    if(kind==1){
                        float fall=ReducedMotion?1:Mathf.Clamp01((local-.9f)/.65f);
                        y=Mathf.Lerp(.70f,.38f,fall*fall);
                        float shift=ReducedMotion?1:Mathf.SmoothStep(0,1,Mathf.Clamp01((local-1.55f)/.65f));x=Mathf.Lerp(fromX,(finalDest[i]+.5f)/row.Length,shift);
                    }else if(!ReducedMotion)y+=Mathf.Sin(blend*Mathf.PI)*(toX>=fromX?.12f:-.06f);
                    var cell=row[i].rectTransform;float half=ReducedMotion?.12f:.18f;
                    cell.anchorMin=new Vector2(x-.38f/row.Length,y-half);cell.anchorMax=new Vector2(x+.38f/row.Length,y+half);
                    cell.localScale=Vector3.one*(1+(ReducedMotion?0:Mathf.Sin(blend*Mathf.PI)*.05f));
                    ghosts[i].gameObject.SetActive(ReducedMotion);

                }
                int hp=floor.BaseDurability;foreach(var e in result.Events)if(e.Kind==PuzzleEventKind.DurabilityResolved&&e.Floor==0&&after)hp=Mathf.Max(0,e.Value);
                woodCount.SetCopy(new UiCopy("Wood: {0} moves",hp));
                bool blast=after&&result.Events.Any(e=>e.Kind==PuzzleEventKind.BombExploded),defused=after&&result.Events.Any(e=>e.Kind==PuzzleEventKind.BombDefused);
                if(kind==2&&mount!=null)mount.sprite=defused&&safeBomb!=null?safeBomb:armedBomb;
                if(mount!=null)mount.color=after?new Color(.65f,.8f,.73f,.7f):Color.white;
                board.gameObject.SetActive(!blast&&(kind!=1||!after));latch.gameObject.SetActive(kind==3?!after:kind==7&&after);
                if(bridge!=null)bridge.localRotation=Quaternion.Euler(0,0,-78*(ReducedMotion?1:Mathf.Clamp01((local-1.7f)/.4f)));
                int stage=(after?1:0)+(bad?2:0);if(stage!=lastStage){lastStage=stage;outcome.SetCopy(after?(kind==1?"":blast?"Board breaks":defused?"Bomb defused":kind==3?"Unlocked":kind==4?"Repaired":kind==8?"Stamp collected":kind==9?"One far swap ready":kind==10?"Bomb paused for two swaps":"then down"):"");}
                if(hand!=null) {
                    hand.gameObject.SetActive(!ReducedMotion&&local<2.3f);
                    int start=command.Kind==CommandKind.Swap?command.Edge:0;float x=(start+.5f+blend)/row.Length;
                    hand.rectTransform.anchorMin=new Vector2(x-.09f,.54f);hand.rectTransform.anchorMax=new Vector2(x+.09f,.96f);
                }
            },isolatedClock);
        }
    }
}
