using UnityEngine;

namespace Cascade.Presentation
{
    // Presentation only. Fixed spring substeps keep inertia consistent on a
    // slow phone; scheduled gestures never run game commands or save progress.
    public sealed class CharacterPoseMotion
    {
        public readonly struct Binding
        {
            public readonly Vector2 Source;
            public readonly float Head,Body,Tail,Scarf,Lantern,Bag,Knees;
            public readonly float NearEar,FarEar,Accent,Brow,Cheek,Nose,Chest,TailTip,ClothTip,Flap;
            public Binding(Vector2 p,CharacterPoseProfile rig)
            {
                Source=p;Head=1-Smooth(rig.Neck.y-.025f,rig.Neck.y+.095f,p.y);
                if(rig.Kind==CharacterPoseKind.Victory)Head*=1-Smooth(.74f,.80f,p.x); // The raised paw is not part of the head.
                Body=1-Smooth(rig.Ground-.17f,rig.Ground,p.y);
                Bag=Box(p,rig.Bag);
                Tail=Area(p,rig.Tail)*(1-Smooth(.24f,.34f,p.x))*(1-Bag);
                Scarf=Area(p,rig.Scarf)*Mathf.Clamp01(Vector2.Distance(p,rig.ScarfRoot)/.12f);
                Lantern=Box(p,rig.Lantern)*Smooth(rig.LanternHook.y-.018f,rig.LanternHook.y+.045f,p.y);
                if(rig.Kind==CharacterPoseKind.Victory)Lantern=0; // Its long lantern is a separate rigid layer.
                if(rig.Kind==CharacterPoseKind.Apple)Body*=1-Area(p,new Vector4(.14f,.81f,.18f,.15f));
                if(rig.Kind==CharacterPoseKind.Sad)Body*=1-Area(p,new Vector4(.28f,.70f,.20f,.16f));
                NearEar=Area(p,rig.NearEar);FarEar=Area(p,rig.FarEar);
                Accent=Area(p,rig.Accent)*(1-Lantern);
                var near=rig.NearEye;var far=rig.FarEye;var mouth=rig.Mouth;
                Brow=rig.SoftBrows?Mathf.Max(Area(p,new Vector4(near.x,near.y-near.w*1.35f,near.z*1.6f,near.w*.8f)),Area(p,new Vector4(far.x,far.y-far.w*1.35f,far.z*1.5f,far.w*.8f))):0;
                Cheek=Mathf.Max(Area(p,new Vector4(mouth.x-mouth.z*1.05f,mouth.y,.045f,.035f)),Area(p,new Vector4(mouth.x+mouth.z*1.05f,mouth.y,.04f,.03f)));
                Nose=Area(p,new Vector4(mouth.x+.02f,mouth.y-.035f,.035f,.023f));
                Chest=Area(p,new Vector4(rig.Neck.x,rig.Neck.y+.14f,.17f,.13f))*(1-Bag)*(1-Tail)*(1-Lantern);
                TailTip=Tail*Smooth(.13f,.29f,Vector2.Distance(p,rig.TailRoot))*(1-Smooth(.18f,.30f,p.x))*Smooth(rig.Tail.y-.18f,rig.Tail.y-.08f,p.y);
                ClothTip=Scarf*Smooth(.065f,.21f,Vector2.Distance(p,rig.ScarfRoot));
                Flap=Bag*Area(p,new Vector4(rig.Bag.x,rig.Bag.y-rig.Bag.w*.42f,rig.Bag.z*.95f,rig.Bag.w*.38f));
                Knees=rig.Kind==CharacterPoseKind.Lantern||rig.Kind==CharacterPoseKind.Thinking?Smooth(.73f,.78f,p.y)*(1-Smooth(.85f,.90f,p.y)):0;
            }
        }
        readonly CharacterPoseProfile rig;readonly bool appleBite;
        readonly int seed;
        readonly float aspect,inverseAspect;
        float bodyHeave,kneeOffset,breath;
        float nextSniff=2.2f,sniffStart=-100,nextNearEar=.9f,nextFarEar=3.7f,nearEarStart=-100,farEarStart=-100;
        float nextFeeling=4,feelingStart=-100,feeling,expression,expressionVelocity,sniff;
        float nearEar,farEar,tip,tipVelocity,clothTip,clothTipVelocity,accent,accentVelocity,flap,flapVelocity;
        Vector2 nearEarTurn=Vector2.right,farEarTurn=Vector2.right,tipTurn=Vector2.right,clothTipTurn=Vector2.right,accentTurn=Vector2.right,flapTurn=Vector2.right;
        readonly Vector2 tailBend,clothBend,flapHinge;
        float simulationRemainder;
        float clock,nextBlink,blinkAt=-100,secondBlink=float.PositiveInfinity,nextLook,lookUntil;
        int blinkIndex,lookIndex;
        float wind,windVelocity,tail,tailVelocity,cloth,clothVelocity,lantern,lanternVelocity,head,headVelocity;
        float mouth,mouthVelocity,bodyAngle,bodyVelocity,bag,bagVelocity;Vector2 gaze,gazeVelocity,look;
        Vector2 headTurn=Vector2.right,bodyTurn=Vector2.right,tailTurn=Vector2.right,clothTurn=Vector2.right,lanternTurn=Vector2.right,bagTurn=Vector2.right;
        public float Age=>clock;
        public float NearEarAngle=>nearEar;
        public float FarEarAngle=>farEar;
        public float Sniff=>sniff;
        public float Expression=>expression;
        public float AccentAngle=>accent;
        public float TailTipAngle=>tip;
        public float ClothTipAngle=>clothTip;
        public float WhiskerFan=>rig.Whiskers?(sniff*.8f+breath*.12f):0;
        public float Blink{get;private set;}
        public Vector2 Gaze=>gaze;
        public float MouthOpen=>mouth;
        public float Hop{get;private set;}
        public float Sigh{get;private set;}
        public float TailAngle=>tail;
        public float LanternAngle=>lantern;
        public float HeadAngle=>head;
        public CharacterPoseMotion(CharacterPoseProfile profile,int salt=197,bool animateAppleBite=true){rig=profile;appleBite=animateAppleBite;seed=salt;aspect=rig.Cell.width/rig.Cell.height;inverseAspect=1/aspect;
            tailBend=Vector2.Lerp(profile.TailRoot,new Vector2(profile.Tail.x,profile.Tail.y),.55f);
            clothBend=Vector2.Lerp(profile.ScarfRoot,new Vector2(profile.Scarf.x,profile.Scarf.y),.55f);
            flapHinge=new Vector2(profile.Bag.x,profile.Bag.y-profile.Bag.w*.70f);Reset();}
        public void Reset()
        {
            simulationRemainder=0;clock=0;blinkIndex=lookIndex=0;nextBlink=1.7f+Noise(3)*1.4f;nextLook=1.1f;lookUntil=0;blinkAt=-100;secondBlink=float.PositiveInfinity;
            tail=tailVelocity=cloth=clothVelocity=lantern=lanternVelocity=head=headVelocity=wind=windVelocity=mouth=mouthVelocity=bodyAngle=bodyVelocity=bag=bagVelocity=0;
            gaze=gazeVelocity=look=Vector2.zero;Blink=Hop=Sigh=0;
            bodyHeave=kneeOffset=breath=expression=expressionVelocity=sniff=feeling=0;
            nextSniff=2.2f+Noise(251);sniffStart=nearEarStart=farEarStart=feelingStart=-100;
            nextNearEar=.9f+Noise(257)*1.5f;nextFarEar=3.7f+Noise(263)*2;nextFeeling=4+Noise(269)*3;
            nearEar=farEar=tip=tipVelocity=clothTip=clothTipVelocity=accent=accentVelocity=flap=flapVelocity=0;
            nearEarTurn=farEarTurn=tipTurn=clothTipTurn=accentTurn=flapTurn=Vector2.right;
            headTurn=bodyTurn=tailTurn=clothTurn=lanternTurn=bagTurn=Vector2.right;
        }
        float Noise(int value)=>WildlifeMotion.Hash(seed+value*173);
        public void Advance(float seconds,float gust,float speech=0)
        {
            if(float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)return;
            const float tick=1f/120;
            simulationRemainder+=Mathf.Min(seconds,.1f);
            while(simulationRemainder+1e-7f>=tick){Step(tick,gust,speech);simulationRemainder=Mathf.Max(0,simulationRemainder-tick);}
            headTurn=Rotation(head);bodyTurn=Rotation(bodyAngle);tailTurn=Rotation(tail);clothTurn=Rotation(cloth*rig.FabricGain);lanternTurn=Rotation(lantern);bagTurn=Rotation(bag);
            nearEarTurn=Rotation(nearEar);farEarTurn=Rotation(farEar);tipTurn=Rotation(tip);clothTipTurn=Rotation(clothTip*rig.FabricGain);accentTurn=Rotation(accent*rig.AccentGain);flapTurn=Rotation(flap);
            bodyHeave=(breath*.0011f+Sigh*.007f)*Ease(0,.8f,clock);kneeOffset=Mathf.Sin(clock*.57f)*.0045f*Ease(0,.8f,clock);

        }
        void Step(float dt,float gust,float speech)
        {
            clock+=dt;float wake=Ease(0,.8f,clock);
            if(clock>=nextBlink){blinkAt=clock;nextBlink=clock+3.1f+Noise(++blinkIndex+19)*3.5f;secondBlink=Noise(blinkIndex+81)<.15f?clock+.38f:float.PositiveInfinity;}
            if(clock>=secondBlink){blinkAt=clock;secondBlink=float.PositiveInfinity;}
            float b=clock-blinkAt;Blink=b<.08f?Ease(0,.08f,b):b<.12f?1:1-Ease(.12f,.27f,b);
            if(rig.Kind==CharacterPoseKind.Apple&&appleBite){
                Blink=Mathf.Max(Blink,Pulse(clock-3.00f,.08f,.12f,.27f));
                float chew=Ease(3.35f,3.65f,clock)*(1-Ease(6.7f,7.1f,clock));
                speech=Mathf.Max(speech,chew*(.25f+.20f*Mathf.Sin(clock*9.3f)));
            }
            if(clock>=nextLook){lookIndex++;lookUntil=clock+1.4f+Noise(lookIndex+43)*1.4f;nextLook=lookUntil+1.7f+Noise(lookIndex+61)*2;look=new Vector2((Noise(lookIndex+71)-.5f)*1.35f,(Noise(lookIndex+83)-.5f)*.42f);}
            var aim=clock<lookUntil?look:Vector2.zero;
            gaze.x=Mathf.SmoothDamp(gaze.x,aim.x,ref gazeVelocity.x,.20f,Mathf.Infinity,dt);gaze.y=Mathf.SmoothDamp(gaze.y,aim.y,ref gazeVelocity.y,.24f,Mathf.Infinity,dt);
            float sighAge=(clock+2)%12;
            Sigh=rig.Kind==CharacterPoseKind.Sad?Ease(3.4f,4.3f,sighAge)*(1-Ease(4.7f,6.3f,sighAge)):0;
            float joyTime=clock%13;
            Hop=rig.Kind==CharacterPoseKind.Victory?Jump(joyTime-.9f,.60f,.035f)+Jump(joyTime-1.87f,.48f,.021f):0;
            float joy=rig.Kind==CharacterPoseKind.Victory?Ease(.35f,.8f,joyTime)*(1-Ease(2.5f,3.7f,joyTime)):0;
            Blink=Mathf.Max(Blink,joy*.18f);
            mouth=Mathf.SmoothDamp(mouth,Mathf.Clamp01(Mathf.Max(speech,joy*.40f)),ref mouthVelocity,.10f,Mathf.Infinity,dt);
            wind=Mathf.SmoothDamp(wind,Mathf.Clamp(gust,-1,1),ref windVelocity,.25f,Mathf.Infinity,dt);
            float envelope=.5f+.5f*Mathf.Sin(clock*.16f+Noise(127)*6);
            Spring(ref tail,ref tailVelocity,(Mathf.Sin(clock*.86f+Mathf.Sin(clock*.23f)*.28f)*(4+4.2f*envelope*envelope)+wind*.6f)*wake,5.5f,.83f,dt);
            Spring(ref cloth,ref clothVelocity,(wind*5.3f+Mathf.Sin(clock*3.6f+Noise(163)*6)*(.65f+Mathf.Abs(wind)))*wake,9,.60f,dt);
            float balance=(Mathf.Sin(clock*.48f+Noise(167)*3)*.55f+Mathf.Sin(clock*.19f)*.20f)*wake;
            float previous=bodyAngle;bodyAngle=Mathf.SmoothDamp(bodyAngle,balance-Sigh*.70f,ref bodyVelocity,.32f,Mathf.Infinity,dt);
            // Lantern inertia responds to the hand/body acceleration and air,
            // with its own damped lag. The top of the handle stays in the paw.
            Spring(ref lantern,ref lanternVelocity,-(bodyAngle-previous)/dt*.8f+wind*1.9f+Mathf.Sin(clock*.62f+1.2f)*1.4f+joy*Mathf.Sin(joyTime*5)*2,6.2f,.42f,dt);
            Spring(ref bag,ref bagVelocity,-(bodyAngle-previous)/dt*.6f+wind*.6f+Mathf.Sin(clock*.77f)*.55f,5,.72f,dt);
            if(clock>=nextSniff){sniffStart=clock;nextSniff=clock+5.5f+Noise((int)clock+281)*5;}
            if(clock>=nextNearEar){nearEarStart=clock;nextNearEar=clock+6+Noise((int)clock+283)*9;}
            if(clock>=nextFarEar){farEarStart=clock;nextFarEar=clock+9+Noise((int)clock+293)*11;}
            if(clock>=nextFeeling){feelingStart=clock;nextFeeling=clock+11+Noise((int)clock+307)*12;}
            feeling=Pulse(clock-feelingStart,.6f,2.6f,3.5f);
            float sniffAge=clock-sniffStart;
            sniff=(Pulse(sniffAge,.11f,.18f,.34f)+.7f*Pulse(sniffAge-.43f,.12f,.19f,.38f))*rig.SniffGain;
            nearEar=Twitch(clock-nearEarStart)*3.1f*rig.EarGain;
            farEar=Twitch(clock-farEarStart)*-2.5f*rig.EarGain;
            breath=Mathf.Sin(clock*1.47f+Mathf.Sin(clock*.23f)*.18f)*wake;
            float emotion=rig.Kind==CharacterPoseKind.Sad?-.28f*Sigh:rig.Kind==CharacterPoseKind.Victory?joy*.75f:rig.Kind==CharacterPoseKind.Thinking?feeling*.35f:rig.Kind==CharacterPoseKind.Apple?feeling*.2f:feeling*.45f;
            expression=Mathf.SmoothDamp(expression,emotion,ref expressionVelocity,.20f,Mathf.Infinity,dt);
            Spring(ref tip,ref tipVelocity,-tailVelocity*.12f+wind*1.2f+Mathf.Sin(clock*1.27f)*1.2f*wake,5,.70f,dt);
            Spring(ref clothTip,ref clothTipVelocity,wind*.85f+clothVelocity*.035f+Mathf.Sin(clock*4.3f)*Mathf.Abs(wind)*.20f,10,.66f,dt);
            Spring(ref accent,ref accentVelocity,wind*2.6f+breath*.5f+clothVelocity*.08f,7,.62f,dt);
            Spring(ref flap,ref flapVelocity,-bagVelocity*.15f+breath*.25f,8,.72f,dt);
            float nod=feeling*Mathf.Sin((clock-feelingStart)*2.1f)*(rig.Kind==CharacterPoseKind.Thinking?.7f:.35f);
            float tilt=rig.Kind==CharacterPoseKind.Sad?gaze.x*2.4f+Sigh*2.4f:rig.Kind==CharacterPoseKind.Thinking?gaze.x*1.8f+Mathf.Sin(clock*.49f)*.45f:gaze.x*1.7f;
            head=Mathf.SmoothDamp(head,tilt+Mathf.Sin(clock*.73f)*.28f-Sigh*.7f+nod,ref headVelocity,.34f,Mathf.Infinity,dt);
        }
        static float Pulse(float t,float rise,float hold,float end)=>Ease(0,rise,t)*(1-Ease(hold,end,t));
        static float Twitch(float age){if(age<0||age>.78f)return 0;float t=age/.78f,envelope=Mathf.Sin(t*Mathf.PI);return Mathf.Sin(t*Mathf.PI*2)*envelope*envelope;}
        static void Spring(ref float p,ref float v,float goal,float frequency,float damping,float dt){v+=((goal-p)*frequency*frequency-2*damping*frequency*v)*dt;p+=v*dt;}
        static float Jump(float t,float duration,float height){if(t<-.22f||t>duration+.28f)return 0;if(t<0)return -.004f*Ease(-.22f,-.07f,t)*(1-Ease(-.07f,0,t));if(t<duration){float u=t/duration;return height*4*u*(1-u);}return -.003f*Ease(duration,duration+.055f,t)*(1-Ease(duration+.055f,duration+.28f,t));}
        public Vector2 Deform(Vector2 source)=>Deform(new Binding(source,rig));
        public Vector2 Deform(Binding b)
        {
            if(clock<=0)return b.Source;var p=b.Source;
            if(b.Tail>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.TailRoot,tailTurn),b.Tail);
            if(b.TailTip>.001f)p=Vector2.LerpUnclamped(p,Turn(p,tailBend,tipTurn),b.TailTip);
            if(b.ClothTip>.001f)p=Vector2.LerpUnclamped(p,Turn(p,clothBend,clothTipTurn),b.ClothTip);
            if(b.Scarf>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.ScarfRoot,clothTurn),b.Scarf);
            if(b.Lantern>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.LanternHook,lanternTurn),b.Lantern);
            if(b.Accent>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.AccentRoot,accentTurn),b.Accent);
            if(b.NearEar>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.NearEarRoot,nearEarTurn),b.NearEar);
            if(b.FarEar>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.FarEarRoot,farEarTurn),b.FarEar);
            if(b.Flap>.001f)p=Vector2.LerpUnclamped(p,Turn(p,flapHinge,flapTurn),b.Flap);
            if(b.Bag>.001f)p=Vector2.LerpUnclamped(p,Turn(p,rig.BagHook,bagTurn),b.Bag);
            p.x+=kneeOffset*b.Knees;
            p.y-=expression*.0035f*b.Cheek;
            p.y-=(expression*.0025f+gaze.y*.0012f)*b.Brow;
            p.y-=sniff*.0018f*b.Nose;
            p.x+=sniff*.0011f*b.Nose;
            p.y-=breath*.0010f*b.Chest;
            if(b.Head>.001f){p=Vector2.LerpUnclamped(p,Turn(p,rig.Neck,headTurn),b.Head);p.y+=Sigh*.004f*b.Head;}
            return Body(p,b.Body);
        }
        Vector2 Body(Vector2 p,float weight)
        {
            var pivot=new Vector2(.55f,rig.Ground);var moved=Turn(p,pivot,bodyTurn);
            moved.y+=bodyHeave;
            p=Vector2.LerpUnclamped(p,moved,weight);p.y-=Hop;return p;
        }
        public Vector2 DeformLantern(Vector2 p)=>Body(Turn(p,rig.LanternHook,lanternTurn),1);
        Vector2 Turn(Vector2 p,Vector2 pivot,Vector2 rotation){p-=pivot;p.x*=aspect;p=new Vector2(p.x*rotation.x-p.y*rotation.y,p.x*rotation.y+p.y*rotation.x);p.x*=inverseAspect;return pivot+p;}
        static Vector2 Rotation(float degrees){float a=degrees*Mathf.Deg2Rad;return new Vector2(Mathf.Cos(a),Mathf.Sin(a));}
        public static float Smooth(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
        public static float Ease(float a,float b,float v){float t=Mathf.InverseLerp(a,b,v);return t*t*t*(t*(t*6-15)+10);}
        static float Area(Vector2 p,Vector4 a){if(a.z<=0||a.w<=0)return 0;return 1-Smooth(.65f,1,Vector2.Scale(p-new Vector2(a.x,a.y),new Vector2(1/a.z,1/a.w)).magnitude);}
        static float Box(Vector2 p,Vector4 a){if(a.z<=0||a.w<=0)return 0;return 1-Smooth(.88f,1.12f,Mathf.Max(Mathf.Abs((p.x-a.x)/a.z),Mathf.Abs((p.y-a.y)/a.w)));}
    }
}
