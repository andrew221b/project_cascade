using UnityEngine;

namespace Cascade.Presentation
{
    // Small, source-pixel-space secondary simulation for the welcome illustration.
    // XPBD distance/bend constraints retain length; moving anchors transmit inertia.
    public sealed class WelcomeSecondaryMotion
    {
        public readonly struct Binding
        {
            public readonly int Segment;public readonly float Blend;
            public Binding(int segment,float blend){Segment=segment;Blend=blend;}
        }
        static readonly Vector2[] ScarfRest={new Vector2(416,575),new Vector2(370,566),new Vector2(325,556),new Vector2(280,545),new Vector2(235,532),new Vector2(190,521),new Vector2(143,509)};
        static readonly Vector2[] TailRest={new Vector2(337,1035),new Vector2(250,1080),new Vector2(166,1010),new Vector2(113,916),new Vector2(95,819),new Vector2(146,739),new Vector2(238,710)};
        public static readonly Vector2 BodyPivot=new Vector2(590,1100),BagHinge=new Vector2(540,745),BagCentre=new Vector2(360,942);
        // The shoulder supports the cloth in calm. The source illustration is
        // the lifted pose, not the zero-force equilibrium.
        const float ScarfContactAngle=-78*Mathf.Deg2Rad;
        static readonly Vector2 ScarfContactTurn=new Vector2(Mathf.Cos(ScarfContactAngle),Mathf.Sin(ScarfContactAngle));
        const float Gravity=2400;
        sealed class Chain
        {
            readonly Vector2[] rest,points,previous,velocity,bendLambda,bendRest,local,turn,restDirection,skinOffset,bendTarget;
            readonly float[] inverseMass,lengths,lengthInverseMass,bendInverseMass;
            readonly float compliance,drag,damping;readonly bool cloth;
            public Chain(Vector2[] shape,bool cloth){
                rest=shape;this.cloth=cloth;int count=shape.Length;points=new Vector2[count];previous=new Vector2[count];velocity=new Vector2[count];bendLambda=new Vector2[count];bendRest=new Vector2[count];local=new Vector2[count];turn=new Vector2[count];restDirection=new Vector2[count];skinOffset=new Vector2[count];bendTarget=new Vector2[count];
                inverseMass=new float[count];lengths=new float[count];lengthInverseMass=new float[count];bendInverseMass=new float[count];compliance=cloth?.002f:.00003f;drag=cloth?.12f:.015f;damping=cloth?.7f:1.2f;
                for(int i=0;i<count;i++){
                    inverseMass[i]=i<2?0:1/(cloth?Mathf.Lerp(.024f,.014f,i/(float)(count-1)):.12f);
                    if(i>0)lengths[i]=(rest[i]-rest[i-1]).magnitude;
                    if(i>0&&i<count-1)bendRest[i]=rest[i-1]-rest[i]*2+rest[i+1];
                    restDirection[i]=(rest[Mathf.Min(count-1,i+1)]-rest[Mathf.Max(0,i-1)]).normalized;
                }
                for(int i=2;i<count;i++)lengthInverseMass[i]=1/(inverseMass[i-1]+inverseMass[i]);
                Reset(Vector2.right);
            }
            public void Reset(Vector2 rotation){for(int i=0;i<points.Length;i++){points[i]=previous[i]=local[i]=rest[0]+Turn(rest[i]-rest[0],rotation);velocity[i]=Vector2.zero;turn[i]=rotation;skinOffset[i]=local[i]-Turn(rest[i],rotation);}}
            public void Step(float dt,Vector2 translation,Vector2 bodyTurn,float wind,float age,Vector2 muscleTurn){
                float ramp=Ramp(age/.8f),alpha=compliance/(dt*dt);
                var root=BodyPoint(rest[0],translation,bodyTurn);var chainTurn=Turn(bodyTurn,muscleTurn);
                for(int i=0;i<points.Length;i++){
                    previous[i]=points[i];bendLambda[i]=Vector2.zero;
                    if(i>0&&i<points.Length-1){bendTarget[i]=Turn(bendRest[i],chainTurn);bendInverseMass[i]=1/(inverseMass[i-1]+4*inverseMass[i]+inverseMass[i+1]+alpha);}
                    if(inverseMass[i]==0){points[i]=root+Turn(rest[i]-rest[0],chainTurn);continue;}
                    float turbulence=cloth?Mathf.Abs(wind)*(Mathf.Sin(age*4.1f-i*.6f)+Mathf.Sin(age*7.7f+i*.31f)*.3f)*40:0;
                    var air=new Vector2(wind*85,-Mathf.Abs(wind)*(cloth?800:320)+turbulence);
                    float dragRate=drag*inverseMass[i];
                    velocity[i]=(velocity[i]+(new Vector2(0,Gravity)+air*dragRate)*ramp*dt)/(1+(dragRate+damping)*dt);
                    points[i]+=velocity[i]*dt;
                }
                for(int pass=0;pass<8;pass++){
                    for(int i=1;i<points.Length-1;i++){
                        float a=inverseMass[i-1],b=inverseMass[i],c=inverseMass[i+1];
                        var error=points[i-1]-points[i]*2+points[i+1]-bendTarget[i];
                        var delta=(-error-bendLambda[i]*alpha)*bendInverseMass[i];bendLambda[i]+=delta;
                        points[i-1]+=delta*a;points[i]-=delta*(2*b);points[i+1]+=delta*c;
                    }
                    // Alternate ordering to avoid a directional stretch bias.
                    if((pass&1)==0)for(int i=1;i<points.Length;i++)Length(i);
                    else for(int i=points.Length-1;i>0;i--)Length(i);
                }
                for(int i=0;i<points.Length;i++)velocity[i]=(points[i]-previous[i])/dt;
            }
            void Length(int i){
                float a=inverseMass[i-1],b=inverseMass[i],mass=lengthInverseMass[i];if(mass<=0)return;
                var delta=points[i]-points[i-1];float length=delta.magnitude;if(length<.00001f)return;
                float impulse=-(length-lengths[i])*mass;
                var correction=delta*(impulse/length);points[i-1]-=correction*a;points[i]+=correction*b;
            }
            public void PrepareSkin(Vector2 translation,Vector2 bodyTurn,Vector2 muscleTurn){
                var inverse=new Vector2(bodyTurn.x,-bodyTurn.y);
                for(int i=0;i<points.Length;i++)local[i]=BodyPivot+Turn(points[i]-BodyPivot-translation,inverse);
                for(int i=0;i<points.Length;i++){
                    if(i<2)turn[i]=muscleTurn;
                    else{var direction=(local[Mathf.Min(points.Length-1,i+1)]-local[i-1]).normalized;turn[i]=new Vector2(Vector2.Dot(restDirection[i],direction),Cross(restDirection[i],direction));}
                    skinOffset[i]=local[i]-Turn(rest[i],turn[i]);
                }
            }
            public Vector2 Skin(Vector2 source,Binding bind){
                int a=bind.Segment,b=a+1;var rotation=Vector2.LerpUnclamped(turn[a],turn[b],bind.Blend);var offset=Vector2.LerpUnclamped(skinOffset[a],skinOffset[b],bind.Blend);
                return new Vector2(source.x*rotation.x-source.y*rotation.y+offset.x,source.x*rotation.y+source.y*rotation.x+offset.y);
            }
            public float Stretch {get{float error=0;for(int i=1;i<points.Length;i++)error=Mathf.Max(error,Mathf.Abs((points[i]-points[i-1]).magnitude/lengths[i]-1));return error;}}
            public float Energy {get{float sum=0;for(int i=2;i<points.Length;i++)sum+=velocity[i].sqrMagnitude/inverseMass[i]*.5f;return sum;}}
            public float RootError(Vector2 translation,Vector2 bodyTurn)=>Vector2.Distance(points[0],BodyPoint(rest[0],translation,bodyTurn));
            public float Tip=>Vector2.Distance(local[local.Length-1],rest[rest.Length-1]);
#if UNITY_EDITOR
            public void Kick(Vector2 impulse){for(int i=2;i<points.Length;i++)velocity[i]+=impulse*(i/(float)(points.Length-1));}
#endif
        }
        readonly Chain scarf=new Chain(ScarfRest,true),tail=new Chain(TailRest,false);
        Vector2 translation,bodyTurn=Vector2.right,tailTurn=Vector2.right,scarfTurn=Vector2.right,lastBagRoot=BagHinge,lastBagVelocity,bagTurn=Vector2.right,bagBreathOffset,bagSkinOffset;
        float bagAngle,bagVelocity,lastBodyAngle,lastBodyAngularVelocity,scarfAngle,scarfVelocity;
        static readonly Vector2 BagArm=(BagCentre-BagHinge).normalized;
        static readonly float BagLength=(BagCentre-BagHinge).magnitude;
        public float BagAngle=>bagAngle*Mathf.Rad2Deg;
        public float MaxRelativeStretch=>Mathf.Max(scarf.Stretch,tail.Stretch);
        public float MaxAnchorError=>Mathf.Max(scarf.RootError(translation,bodyTurn),tail.RootError(translation,bodyTurn));
        public float KineticEnergy=>scarf.Energy+tail.Energy+.5f*1.8f*BagLength*BagLength*bagVelocity*bagVelocity+.5f*scarfVelocity*scarfVelocity;
        public float ScarfTipDisplacement=>scarf.Tip;
        public float TailTipDisplacement=>tail.Tip;
        public WelcomeSecondaryMotion(){Reset();}
        public void Reset(){translation=lastBagVelocity=bagBreathOffset=bagSkinOffset=Vector2.zero;bodyTurn=bagTurn=tailTurn=Vector2.right;scarfTurn=ScarfContactTurn;lastBagRoot=BagHinge;bagAngle=bagVelocity=lastBodyAngle=lastBodyAngularVelocity=scarfVelocity=0;scarfAngle=ScarfContactAngle;scarf.Reset(scarfTurn);tail.Reset(Vector2.right);}
        public void Step(float dt,Vector2 bodyOffset,float bodyDegrees,float wind,float age,float breath=0,float tailDegrees=0){
            translation=bodyOffset;float angle=bodyDegrees*Mathf.Deg2Rad;bodyTurn=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            float tailAngle=tailDegrees*Mathf.Deg2Rad;tailTurn=new Vector2(Mathf.Cos(tailAngle),Mathf.Sin(tailAngle));
            // Air pressure lifts the loose end away from shoulder contact. When
            // pressure falls, restoring torque and damping bring it back down.
            scarfVelocity=(scarfVelocity+(wind*wind*20.5f-(scarfAngle-ScarfContactAngle)*5.5f)*dt)/(1+4.7f*dt);
            scarfVelocity=Mathf.Clamp(scarfVelocity,-.65f,.65f);
            scarfAngle+=scarfVelocity*dt;float scarfLimit=Mathf.Clamp(scarfAngle,ScarfContactAngle,.10f);if(scarfLimit!=scarfAngle){scarfAngle=scarfLimit;scarfVelocity=0;}
            scarfTurn=new Vector2(Mathf.Cos(scarfAngle),Mathf.Sin(scarfAngle));
            scarf.Step(dt,translation,bodyTurn,wind,age,scarfTurn);tail.Step(dt,translation,bodyTurn,wind,age,tailTurn);
            bagBreathOffset=new Vector2((BagHinge.x-590)*breath*.0025f,-breath*.95f);
            var root=BodyPoint(BagHinge+bagBreathOffset,translation,bodyTurn);var velocity=(root-lastBagRoot)/dt;
            var acceleration=Vector2.ClampMagnitude((velocity-lastBagVelocity)/dt,1500);lastBagRoot=root;lastBagVelocity=velocity;
            float angularVelocity=(angle-lastBodyAngle)/dt,angularAcceleration=(angularVelocity-lastBodyAngularVelocity)/dt;lastBodyAngle=angle;lastBodyAngularVelocity=angularVelocity;
            var inverse=new Vector2(bodyTurn.x,-bodyTurn.y);var effective=Turn(new Vector2(wind*18/1.8f,Gravity)-acceleration,inverse);
            var direction=Turn(BagArm,new Vector2(Mathf.Cos(bagAngle),Mathf.Sin(bagAngle)));
            // Static strap/body contact supports the authored hanging angle.
            // Only acceleration and changed forces displace it from that rest.
            float torque=(Cross(direction,effective)-Cross(BagArm,new Vector2(0,Gravity)))/BagLength-angularAcceleration-bagAngle*4;
            if(bagAngle<-.012f)torque-=420*(bagAngle+.012f);
            bagVelocity=(bagVelocity+torque*dt)/(1+3.4f*dt);bagAngle+=bagVelocity*dt;
            float limit=Mathf.Clamp(bagAngle,-.035f,.06f);if(limit!=bagAngle){bagAngle=limit;bagVelocity=0;}
        }
        public void PrepareSkin(){scarf.PrepareSkin(translation,bodyTurn,scarfTurn);tail.PrepareSkin(translation,bodyTurn,tailTurn);bagTurn=new Vector2(Mathf.Cos(bagAngle),Mathf.Sin(bagAngle));bagSkinOffset=BagHinge+bagBreathOffset-Turn(BagHinge,bagTurn);}
        public Vector2 ScarfPoint(Vector2 source,Binding binding)=>scarf.Skin(source,binding);
        public static Vector2 RestingScarfPoint(Vector2 source)=>ScarfRest[0]+Turn(source-ScarfRest[0],ScarfContactTurn);
        public Vector2 TailPoint(Vector2 source,Binding binding)=>tail.Skin(source,binding);
        public Vector2 BagPoint(Vector2 source)=>new Vector2(source.x*bagTurn.x-source.y*bagTurn.y+bagSkinOffset.x,source.x*bagTurn.y+source.y*bagTurn.x+bagSkinOffset.y);
        public static Binding BindScarf(Vector2 source)=>Bind(source,ScarfRest);
        public static Binding BindTail(Vector2 source)=>Bind(source,TailRest);
        static Binding Bind(Vector2 source,Vector2[] rest){
            int closest=0;float blend=0,distance=float.PositiveInfinity;
            for(int i=0;i<rest.Length-1;i++){
                var edge=rest[i+1]-rest[i];float t=Mathf.Clamp01(Vector2.Dot(source-rest[i],edge)/edge.sqrMagnitude);float d=(source-rest[i]-edge*t).sqrMagnitude;
                if(d<distance){closest=i;blend=t;distance=d;}
            }
            return new Binding(closest,blend);
        }
        static float Ramp(float value){float t=Mathf.Clamp01(value);return t*t*t*(t*(t*6-15)+10);}
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static Vector2 Turn(Vector2 vector,Vector2 turn)=>new Vector2(vector.x*turn.x-vector.y*turn.y,vector.x*turn.y+vector.y*turn.x);
        static Vector2 BodyPoint(Vector2 source,Vector2 translation,Vector2 turn)=>BodyPivot+Turn(source-BodyPivot,turn)+translation;
#if UNITY_EDITOR
        public void KickForReview(Vector2 scarfImpulse,Vector2 tailImpulse,float bagAngularImpulse){scarf.Kick(scarfImpulse);tail.Kick(tailImpulse);bagVelocity+=bagAngularImpulse;}
#endif
    }
}
