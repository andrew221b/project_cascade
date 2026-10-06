using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cascade.Presentation
{
    // Invisible pooled collision proxies drive the original rendered pieces. This scene has no puzzle logic.
    public sealed class ToyRowPhysics
    {
        readonly CascadeGame game;
        readonly Rigidbody[] bodies=new Rigidbody[8];
        readonly BoxCollider[] boxes=new BoxCollider[8];
        readonly Vector3[] centres=new Vector3[8],seats=new Vector3[8];
        readonly Vector3[] shakeStarts=new Vector3[8],shakeTargets=new Vector3[8];
        readonly bool[] landed=new bool[8];
        readonly PhysicsMaterial material;
        readonly BoxCollider receiver;
        int count;float age;
        bool shaking;
        public bool Active=>count>0;
        public bool Shaking=>shaking;
        public float Elapsed=>age;
        public int LandedCount {get;private set;}
        public ToyRowPhysics(CascadeGame owner,Scene scene,Transform root)
        {
            game=owner;
            material=new PhysicsMaterial("Soft enamel contact"){dynamicFriction=.42f,staticFriction=.5f,bounciness=.36f,
                frictionCombine=PhysicsMaterialCombine.Average,bounceCombine=PhysicsMaterialCombine.Maximum};
            for(int i=0;i<bodies.Length;i++){
                var go=new GameObject("Cosmetic row body "+i,typeof(Rigidbody),typeof(BoxCollider));
                SceneManager.MoveGameObjectToScene(go,scene);go.transform.SetParent(root,false);
                var body=go.GetComponent<Rigidbody>();bodies[i]=body;boxes[i]=go.GetComponent<BoxCollider>();boxes[i].sharedMaterial=material;
                body.isKinematic=true;body.constraints=RigidbodyConstraints.FreezePositionZ|RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationY;
                body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;body.solverIterations=8;body.solverVelocityIterations=4;
                go.SetActive(false);
            }
            var rail=new GameObject("Cosmetic receiver contact",typeof(BoxCollider));SceneManager.MoveGameObjectToScene(rail,scene);rail.transform.SetParent(root,false);
            receiver=rail.GetComponent<BoxCollider>();receiver.sharedMaterial=material;receiver.enabled=false;
        }
        public void Begin(Vector3[] starts,Vector3[] targets,int size)
        {
            End();count=size;age=0;LandedCount=0;
            for(int i=0;i<count;i++){
                centres[i]=game.Machine.TokenBodyCentre(i);seats[i]=targets[i];landed[i]=false;
                boxes[i].size=game.Machine.TokenBodySize(i)*.91f;boxes[i].size=new Vector3(boxes[i].size.x,boxes[i].size.y,.5f);
                var b=bodies[i];b.gameObject.SetActive(true);b.isKinematic=false;b.useGravity=true;b.mass=game.VisibleFrame.Row[i].Kind==Cascade.Domain.SymbolKind.Glyph?1.2f:1;
                b.linearDamping=.12f;b.angularDamping=.6f;b.position=starts[i]+centres[i];b.rotation=Quaternion.identity;
                // Small opposed release impulses let neighbours collide rather than slide as a rigid row.
                b.linearVelocity=new Vector3((i%2==0?1:-1)*(.24f+i%3*.065f),-.04f*(i%3),0);
                b.angularVelocity=Vector3.forward*((i%2==0?1:-1)*(1.6f+i%3*.3f));
            }
            receiver.enabled=game.VisibleFrame.Status!=Cascade.Domain.PuzzleStatus.Playing;
            if(receiver.enabled){float low=targets[0].y+centres[0].y-boxes[0].size.y*.5f;
                receiver.transform.position=new Vector3((targets[0].x+targets[count-1].x)*.5f,low-.065f,starts[0].z);
                receiver.size=new Vector3(Mathf.Abs(targets[count-1].x-targets[0].x)+1,.13f,1.5f);}
        }
        public void Step(float dt)
        {
            if(!Active)return;age+=dt;
            for(int i=0;i<count;i++){
                var b=bodies[i];float height=Position(i).y-seats[i].y;
                if(!landed[i]&&b.linearVelocity.y<=0&&height<.10f){landed[i]=true;LandedCount++;
                    game.Machine.ContactImpulse(game.VisibleFrame.ActiveFloor,i,.7f,b.linearVelocity.x*.5f);
                    game.Feedback.Sound(ToyCue.Land);}
                float wind=game.Machine.WindAt(Mathf.Min(game.VisibleFrame.ActiveFloor,game.Interaction.Attempt.Level.Floors.Count-1));
                b.AddForce(Vector3.right*wind*.20f,ForceMode.Acceleration);
                if(landed[i]){
                    b.linearDamping=1.8f;b.angularDamping=2;
                    b.AddForce(Vector3.right*Mathf.Clamp((seats[i].x-Position(i).x)*13-b.linearVelocity.x*3,-8,8),ForceMode.Acceleration);
                    float angle=Mathf.DeltaAngle(0,Angle(i))*Mathf.Deg2Rad;
                    b.AddTorque(Vector3.forward*(-angle*9-b.angularVelocity.z*2),ForceMode.Acceleration);
                }
            }
        }
        public void BeginShake(Vector2 impulse)
        {
            if(Active||!game.Interaction.CanPlay||game.IsHoldingToken)return;
            int size=game.Machine.TokenCount;
            for(int i=0;i<size;i++){shakeStarts[i]=game.Machine.TokenVisualPosition(i);shakeTargets[i]=game.Machine.TokenPosition(i)+game.Machine.TokenSupportOffset(i);}
            Begin(shakeStarts,shakeTargets,size);shaking=true;
            for(int i=0;i<count;i++){
                bodies[i].linearVelocity=new Vector3(impulse.x*(.8f+i%3*.08f),.95f+Mathf.Abs(impulse.y)*.9f,0);
                bodies[i].angularVelocity=Vector3.forward*(impulse.x*(3.8f+i%2));
            }
        }
        public void PresentShake()
        {
            if(!shaking)return;
            if(game.ReducedMotion||game.IsHoldingToken||game.Interaction.IsPresenting||game.CurrentScreen!=GameScreen.Playing){FinishShake();return;}
            float seat=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.40f,.82f,age));
            for(int i=0;i<count;i++){
                var target=game.Machine.TokenPosition(i)+game.Machine.TokenSupportOffset(i);
                var position=Vector3.Lerp(Position(i),target,seat);
                game.Machine.SetPiecePose(i,PiecePose.Falling,Mathf.LerpAngle(Angle(i),0,seat));
                game.OffsetPresentedToken(i,position-game.Machine.TokenPosition(i)-game.Machine.TokenSupportOffset(i),Vector2.one);
            }
            if(age>=.82f)FinishShake();
        }
        void FinishShake()
        {
            if(shaking)for(int i=0;i<count&&i<game.Machine.TokenCount;i++){
                game.Machine.SetPiecePose(i,PiecePose.Resting);
                game.OffsetPresentedToken(i,Vector3.zero,Vector2.one);
            }
            End();
        }
        public void CancelShake(bool settle){if(!shaking)return;if(settle)FinishShake();else End();}
        public Vector3 Position(int index)=>bodies[index].position-bodies[index].rotation*centres[index];
        public float Angle(int index)=>Mathf.DeltaAngle(0,bodies[index].rotation.eulerAngles.z);
        public void End()
        {
            count=0;shaking=false;if(receiver!=null)receiver.enabled=false;
            foreach(var body in bodies)if(body!=null){body.isKinematic=true;body.gameObject.SetActive(false);}
        }
        public void Dispose(){End();if(material!=null)Object.Destroy(material);}
    }
}
