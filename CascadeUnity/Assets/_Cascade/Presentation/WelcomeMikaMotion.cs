using UnityEngine;

namespace Cascade.Presentation
{
    // Decorative skin in the original illustration's 1024 x 1536 pixel space.
    // Boots remain planted; the head is rigid, with softer neck/cloth transitions.
    public sealed class WelcomeMikaMotion
    {
        public readonly struct SkinPoint
        {
            public readonly Vector2 Source;
            public readonly float Bag,Tail,Cloth,Arm,Hand,Head,NearEar,FarEar,Whiskers,Planted,Chest,LeftKnee,RightKnee,Nose,Cheek,Brow;
            public readonly WelcomeSecondaryMotion.Binding TailSkin;
            public SkinPoint(Vector2 p){
                Source=p;Bag=Mathf.Max(Ellipse(p,new Vector2(360,942),new Vector2(150,172)),BagStrap(p));
                // A broad falloff joins the fur to the fixed body without a
                // compressed seam when the tail makes a larger arc.
                Tail=Ellipse(p,new Vector2(177,922),new Vector2(260,380))*(1-Smooth(270,420,p.x))*Smooth(650,730,p.y)*(1-Bag);
                float clothTop=448+60*Smooth(150,415,p.x);
                float clothFeather=Mathf.Lerp(100,30,Smooth(170,340,p.x));
                Cloth=Smooth(clothTop-clothFeather,clothTop+16,p.y)*(1-Smooth(580,670,p.y))*(1-Smooth(363,428,p.x))*Smooth(90,122,p.x);
                // The raised hand occupies the right-side silhouette. Keep its
                // influence away from the tall ear and the face above it.
                Arm=Smooth(790,900,p.x)*Smooth(240,345,p.y)*(1-Smooth(637,702,p.y));Hand=Arm*(1-Smooth(452,511,p.y));
                Head=Smooth(561,508,p.y)*Smooth(153,205,p.x)*(1-Smooth(798,842,p.x))*(1-Cloth)*(1-Bag);
                NearEar=Ellipse(p,new Vector2(278,279),new Vector2(129,155));FarEar=Ellipse(p,new Vector2(736,152),new Vector2(97,135));
                Whiskers=((1-Smooth(367,434,p.x))*Smooth(364,390,p.y)*(1-Smooth(449,470,p.y))+Smooth(744,795,p.x)*Smooth(319,347,p.y)*(1-Smooth(408,431,p.y)))*(1-Cloth);
                Planted=1-Smooth(1170,1320,p.y);Chest=Ellipse(p,new Vector2(590,778),new Vector2(230,300))*(1-Bag)*(1-Tail)*(1-Cloth);
                float trouser=Smooth(1105,1140,p.y)*(1-Smooth(1260,1310,p.y));
                LeftKnee=Ellipse(p,new Vector2(500,1140),new Vector2(140,205))*trouser;
                RightKnee=Ellipse(p,new Vector2(710,1180),new Vector2(110,170))*trouser;
                Nose=Ellipse(p,new Vector2(625,386),new Vector2(43,30));
                Cheek=CheekWeight(p);
                Brow=Mathf.Max(Ellipse(p,new Vector2(486,266),new Vector2(73,27)),Ellipse(p,new Vector2(680,221),new Vector2(49,24)));
                TailSkin=Tail>.001f?WelcomeSecondaryMotion.BindTail(p):default;
            }
        }
        readonly WelcomeSecondaryMotion secondary=new WelcomeSecondaryMotion();
        float simulationRemainder;
        float clock,nextBlink,blinkStart=-100,nextWave=1.1f,waveStart=-100,nextLook,lookUntil,headVelocity,lookXVelocity,lookYVelocity,bodyLeanVelocity;
        float doubleBlinkAt=float.PositiveInfinity,blinkSpeed=1,waveTempo=1,waveStrength=1,armVelocity,handVelocity,filteredWind,windVelocity;
        float nextNearEar,nextFarEar,nearEarStart=-100,farEarStart=-100;
        float tailAngle,tailVelocity,nextSniff=2.1f,sniffStart=-100;
        Vector2 nearWhiskerRotation=Vector2.right,farWhiskerRotation=Vector2.right;
        int seed,blinkIndex,waveIndex,lookIndex;
        Vector2 lookTarget,bodyVelocity,bodySkinOffset;
        Vector2 leftLegOffset,rightLegOffset;
        Vector2 bodyRotation=Vector2.right,armRotation=Vector2.right,handRotation=Vector2.right,headRotation=Vector2.right,nearEarRotation=Vector2.right,farEarRotation=Vector2.right;
        float whiskerLift,smile,sniff;
        public float Smile=>smile;
        public float SniffMotion=>sniff;
        public float Age=>clock;
        public float Blink {get;private set;}
        public float Wave {get;private set;}
        public float HeadAngle {get;private set;}
        public float BagAngle=>secondary.BagAngle;
        public float BodyAngle {get;private set;}
        public float ArmAngle {get;private set;}
        public float WristAngle {get;private set;}
        public Vector2 Gaze {get;private set;}
        public Vector2 BodyOffset {get;private set;}
        public float Breath {get;private set;}
        public float ScarfTipDisplacement=>secondary.ScarfTipDisplacement;
        public float TailTipDisplacement=>secondary.TailTipDisplacement;
        public float MaxSecondaryStretch=>secondary.MaxRelativeStretch;
        public float MaxSecondaryAnchorError=>secondary.MaxAnchorError;
        public WelcomeMikaMotion(int sessionSeed){Reset(sessionSeed);}
        public void Reset(int sessionSeed)
        {
            seed=sessionSeed;simulationRemainder=0;clock=0;nextBlink=2.4f+Noise(3)*.8f;blinkStart=waveStart=-100;nextWave=1.1f;nextLook=2.2f;lookUntil=0;
            blinkIndex=waveIndex=lookIndex=0;headVelocity=lookXVelocity=lookYVelocity=bodyLeanVelocity=0;bodyVelocity=Vector2.zero;
            doubleBlinkAt=float.PositiveInfinity;blinkSpeed=waveTempo=waveStrength=1;armVelocity=handVelocity=filteredWind=windVelocity=0;
            nextNearEar=.9f+Noise(207)*1.2f;nextFarEar=5+Noise(211)*1.8f;nearEarStart=farEarStart=-100;
            tailAngle=tailVelocity=0;sniffStart=-100;nextSniff=2.1f;nearWhiskerRotation=farWhiskerRotation=Vector2.right;
            leftLegOffset=rightLegOffset=Vector2.zero;
            smile=sniff=0;
            Blink=Wave=HeadAngle=BodyAngle=ArmAngle=WristAngle=Breath=0;Gaze=BodyOffset=lookTarget=Vector2.zero;secondary.Reset();
            bodyRotation=armRotation=handRotation=headRotation=nearEarRotation=farEarRotation=Vector2.right;bodySkinOffset=Vector2.zero;whiskerLift=0;
        }
        float Noise(int index)=>WildlifeMotion.Hash(seed+index*173);
        static float Smooth(float a,float b,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,value));
        // Zero velocity and acceleration at either end; useful for gestures that
        // must blend into a held illustration without a mechanical stop.
        static float Ease(float a,float b,float value){float t=Mathf.InverseLerp(a,b,value);return t*t*t*(t*(t*6-15)+10);}
        static float Ellipse(Vector2 p,Vector2 centre,Vector2 radius)=>1-Smooth(.72f,1,Vector2.Scale(p-centre,new Vector2(1/radius.x,1/radius.y)).magnitude);
        static float BagStrap(Vector2 source){
            var edge=new Vector2(470,900)-WelcomeSecondaryMotion.BagHinge;float t=Mathf.Clamp01(Vector2.Dot(source-WelcomeSecondaryMotion.BagHinge,edge)/edge.sqrMagnitude);
            float distance=(source-WelcomeSecondaryMotion.BagHinge-edge*t).magnitude;return (1-Smooth(14,24,distance))*Smooth(.12f,.30f,t);
        }
        public void Advance(float seconds,float wind,Vector2 visitor,bool hasVisitor)
        {
            if(float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)return;
            const float tick=1f/120;
            simulationRemainder+=Mathf.Min(seconds,.10f);
            while(simulationRemainder+1e-7f>=tick){Step(tick,Mathf.Clamp(wind,-1,1),visitor,hasVisitor);simulationRemainder=Mathf.Max(0,simulationRemainder-tick);}
            // Bone matrices are needed by the render pose, not the spring substeps.
            secondary.PrepareSkin();bodyRotation=Rotation(BodyAngle);armRotation=Rotation(ArmAngle);handRotation=Rotation(WristAngle);headRotation=Rotation(HeadAngle);
            var pivot=WelcomeSecondaryMotion.BodyPivot;bodySkinOffset=pivot+BodyOffset-new Vector2(pivot.x*bodyRotation.x-pivot.y*bodyRotation.y,pivot.x*bodyRotation.y+pivot.y*bodyRotation.x);
            nearEarRotation=Rotation(EarTwitch(clock-nearEarStart)*2.2f);farEarRotation=Rotation(EarTwitch(clock-farEarStart)*-1.8f);
            whiskerLift=Breath*.55f+Mathf.Sin(clock*2.4f)*.22f;
            float sniffAge=clock-sniffStart;
            sniff=Sniff(sniffAge);smile=(Wave*.65f+Ease(0,.7f,(clock+7)%19-8)*(1-Ease(2.2f,3.5f,(clock+7)%19-8))*.22f)*Ease(0,.9f,clock);
            nearWhiskerRotation=Rotation(Breath*1.6f+sniff*7);
            farWhiskerRotation=Rotation(-Breath*1.2f-Sniff(sniffAge-.06f)*5.5f);
        }
        void Step(float dt,float wind,Vector2 visitor,bool hasVisitor)
        {
            clock+=dt;
            if(clock>=nextBlink){
                blinkStart=clock;nextBlink=clock+3.5f+Noise(++blinkIndex+17)*3.8f;blinkSpeed=.93f+Noise(blinkIndex+191)*.15f;
                doubleBlinkAt=Noise(blinkIndex+193)<.14f?clock+.34f:float.PositiveInfinity;
            }
            if(clock>=doubleBlinkAt){blinkStart=clock;doubleBlinkAt=float.PositiveInfinity;}
            float blinkAge=(clock-blinkStart)*blinkSpeed;
            Blink=blinkAge<.085f?Ease(0,.085f,blinkAge):blinkAge<.125f?1:1-Ease(.125f,.28f,blinkAge);
            if(clock>=nextWave){
                waveStart=clock;nextWave=clock+22+Noise(++waveIndex+47)*13;
                waveTempo=.92f+Noise(waveIndex+227)*.16f;waveStrength=.91f+Noise(waveIndex+229)*.15f;
            }
            float waving=(clock-waveStart)*waveTempo;
            float prepare=Ease(0,.24f,waving)*(1-Ease(.30f,.62f,waving));
            Wave=Ease(.28f,.82f,waving)*(1-Ease(2.65f,3.6f,waving));
            float phase=(waving-.6f)*5+Mathf.Sin(waving*2.2f)*.1f;
            float sweep=Mathf.Sin(phase);
            float armTarget=-1.6f*prepare+Wave*waveStrength*(sweep*4.5f-.6f);
            float handTarget=Wave*waveStrength*(Mathf.Sin(phase-.55f)*8.8f+Mathf.Sin(phase*2-.3f)*.65f);
            ArmAngle=Mathf.SmoothDamp(ArmAngle,armTarget,ref armVelocity,.095f,Mathf.Infinity,dt);
            WristAngle=Mathf.SmoothDamp(WristAngle,handTarget,ref handVelocity,.075f,Mathf.Infinity,dt);
            if(clock>=nextLook){
                lookIndex++;lookUntil=clock+1.2f+Noise(lookIndex+151)*1.3f;nextLook=lookUntil+2+Noise(lookIndex+91)*2.7f;
                lookTarget=hasVisitor?Vector2.ClampMagnitude(visitor,.85f):new Vector2((Noise(lookIndex+73)-.5f)*1.05f,(Noise(lookIndex+101)-.5f)*.4f);
            }
            if(hasVisitor&&clock<lookUntil)lookTarget=Vector2.Lerp(lookTarget,Vector2.ClampMagnitude(visitor,.85f),1-Mathf.Exp(-dt*5));
            var aim=clock<lookUntil?lookTarget:Vector2.zero;
            Gaze=new Vector2(Mathf.SmoothDamp(Gaze.x,aim.x,ref lookXVelocity,.16f,Mathf.Infinity,dt),Mathf.SmoothDamp(Gaze.y,aim.y,ref lookYVelocity,.19f,Mathf.Infinity,dt));
            float wake=Ease(0,.9f,clock);
            Breath=(Mathf.Sin(clock*1.58f+Mathf.Sin(clock*.21f)*.17f)+Mathf.Sin(clock*.57f)*.14f)*wake;
            float shift=Mathf.Sin(clock*.48f)*8+Mathf.Sin(clock*.19f)*3-Wave*1.8f+prepare*.8f;
            BodyOffset=new Vector2(Mathf.SmoothDamp(BodyOffset.x,shift*wake,ref bodyVelocity.x,.28f,Mathf.Infinity,dt),Mathf.SmoothDamp(BodyOffset.y,-Breath*1.4f+Mathf.Sin(clock*.96f)*.35f*wake,ref bodyVelocity.y,.32f,Mathf.Infinity,dt));
            // Slow knee flexion transfers weight while both boot soles stay fixed.
            float transfer=Mathf.Sin(clock*.58f+Mathf.Sin(clock*.19f)*.21f)*wake;
            leftLegOffset=new Vector2(6*transfer,-2*Mathf.Max(0,transfer));
            rightLegOffset=new Vector2(4.5f*transfer,-1.8f*Mathf.Max(0,-transfer));
            BodyAngle=Mathf.SmoothDamp(BodyAngle,(-BodyOffset.x*.045f-Wave*.6f+prepare*.16f)*wake,ref bodyLeanVelocity,.3f,Mathf.Infinity,dt);
            HeadAngle=Mathf.SmoothDamp(HeadAngle,Gaze.x*.85f+Mathf.Sin(clock*.69f)*.25f+Wave*Mathf.Sin(waving*2.1f)*.35f-prepare*.18f,ref headVelocity,.38f,Mathf.Infinity,dt);
            filteredWind=Mathf.SmoothDamp(filteredWind,wind,ref windVelocity,.26f,Mathf.Infinity,dt);
            // A living tail never waits for the next hand-wave timer. Muscle
            // effort swells over a much slower cycle than the side-to-side sweep.
            float envelope=.5f+.5f*Mathf.Sin(clock*.17f+Noise(251)*6);
            float tailTarget=(Mathf.Sin(clock*.88f+Mathf.Sin(clock*.23f)*.26f+Noise(253)*6)*(3.8f+4.4f*envelope*envelope)+Mathf.Sin(clock*1.47f+1.1f)*.30f)*wake;
            tailAngle=Mathf.SmoothDamp(tailAngle,tailTarget,ref tailVelocity,.22f,Mathf.Infinity,dt);
            secondary.Step(dt,BodyOffset,BodyAngle,filteredWind,clock,Breath,tailAngle);
            if(clock>=nextSniff){sniffStart=clock;nextSniff=clock+4.5f+Noise((int)clock+257)*3;}
            if(clock>=nextNearEar){nearEarStart=clock;nextNearEar=clock+8+Noise((int)clock+233)*8;}
            if(clock>=nextFarEar){farEarStart=clock;nextFarEar=clock+11+Noise((int)clock+239)*9;}
        }
        public Vector2 Deform(Vector2 source)=>Deform(new SkinPoint(source));
        public Vector2 Deform(SkinPoint bind)
        {
            var source=bind.Source;
            if(clock<=0||bind.Planted<=0)return source;
            var p=source;
            if(bind.Tail>.001f)p=Vector2.LerpUnclamped(p,secondary.TailPoint(source,bind.TailSkin),bind.Tail);
            // The loose scarf has its own draw layer so a full drape/lift does
            // not stretch the shoulder, cheek or transparent surrounding mesh.
            if(bind.Bag>.001f)p=Vector2.LerpUnclamped(p,secondary.BagPoint(source),bind.Bag);
            if(bind.Arm>.001f)p=Vector2.LerpUnclamped(p,Rotate(p,new Vector2(784,650),armRotation),bind.Arm);
            if(bind.Hand>.001f)p=Vector2.LerpUnclamped(p,Rotate(p,new Vector2(883,475),handRotation),bind.Hand);
            if(bind.NearEar>.001f)p=Vector2.LerpUnclamped(p,Rotate(p,new Vector2(366,318),nearEarRotation),bind.NearEar);
            if(bind.FarEar>.001f)p=Vector2.LerpUnclamped(p,Rotate(p,new Vector2(677,233),farEarRotation),bind.FarEar);
            p=MicroFace(p,bind.Nose,bind.Cheek,bind.Brow);
            p.y+=whiskerLift*bind.Whiskers;
            if(bind.Head>.001f)p=Vector2.LerpUnclamped(p,Rotate(p,new Vector2(562,535),headRotation),bind.Head);
            p.x+=(source.x-590)*Breath*.0025f*bind.Chest;
            p.y-=Breath*.95f*bind.Chest;
            p+=leftLegOffset*bind.LeftKnee+rightLegOffset*bind.RightKnee;
            var bodyPoint=new Vector2(p.x*bodyRotation.x-p.y*bodyRotation.y+bodySkinOffset.x,p.x*bodyRotation.y+p.y*bodyRotation.x+bodySkinOffset.y);
            p=bind.Planted>=1?bodyPoint:Vector2.LerpUnclamped(p,bodyPoint,bind.Planted);
            return p;
        }
        public Vector2 WhiskerPoint(Vector2 source,bool near)
        {
            var p=Rotate(source,near?new Vector2(549,413):new Vector2(691,375),near?nearWhiskerRotation:farWhiskerRotation);
            p=MicroFace(p,Ellipse(source,new Vector2(625,386),new Vector2(43,30)),CheekWeight(source),0);
            p=Rotate(p,new Vector2(562,535),headRotation);
            return new Vector2(p.x*bodyRotation.x-p.y*bodyRotation.y+bodySkinOffset.x,p.x*bodyRotation.y+p.y*bodyRotation.x+bodySkinOffset.y);
        }
        public Vector2 ScarfPoint(Vector2 source,WelcomeSecondaryMotion.Binding binding)
        {
            var p=secondary.ScarfPoint(source,binding);
            return new Vector2(p.x*bodyRotation.x-p.y*bodyRotation.y+bodySkinOffset.x,p.x*bodyRotation.y+p.y*bodyRotation.x+bodySkinOffset.y);
        }
        static float CheekWeight(Vector2 p)=>Mathf.Max(Ellipse(p,new Vector2(562,451),new Vector2(64,46)),Ellipse(p,new Vector2(701,419),new Vector2(48,43)))*(1-Ellipse(p,new Vector2(620,445),new Vector2(75,48)));
        Vector2 MicroFace(Vector2 p,float nose,float cheek,float brow){p.y-=sniff*1.7f*nose+smile*3.8f*cheek+smile*2.0f*brow;p.x+=sniff*.8f*nose;return p;}
        static float Sniff(float age)=>Ease(0,.12f,age)*(1-Ease(.17f,.32f,age))+.75f*Ease(.39f,.52f,age)*(1-Ease(.57f,.77f,age));
        static float EarTwitch(float age){if(age<0||age>=.72f)return 0;float t=age/.72f,s=Mathf.Sin(t*Mathf.PI);return Mathf.Sin(t*Mathf.PI*2)*s*s;}
        static Vector2 Rotation(float degrees){float a=degrees*Mathf.Deg2Rad;return new Vector2(Mathf.Cos(a),Mathf.Sin(a));}
        static Vector2 Rotate(Vector2 point,Vector2 pivot,Vector2 turn){var p=point-pivot;return pivot+new Vector2(p.x*turn.x-p.y*turn.y,p.x*turn.y+p.y*turn.x);}
    }
}
