using UnityEngine;

namespace Cascade.Presentation
{
    // All samples are functions of the ambient clock. Pausing/redrawing never advances a pose.
    public enum WildlifeFlightStyle { SmallBird, SoaringBird, Butterfly, Dragonfly, Moth, Beetle, Bee, Owl, Cricket }

    public readonly struct WildlifePose
    {
        public readonly float NearWing, FarWing, WingFold, Tail, Lift, NearFlex, FarFlex;
        public readonly int Phase;
        public readonly bool Gliding;
        public WildlifePose(float near, float far, float fold, float tail, float lift,float nearFlex,float farFlex,int phase, bool gliding)
        { NearWing=near; FarWing=far; WingFold=fold; Tail=tail; Lift=lift;NearFlex=nearFlex;FarFlex=farFlex; Phase=phase; Gliding=gliding; }
    }

    public static class WildlifeMotion
    {
        static readonly float[] wingAngles={-64,-46,-12,29,58,39,5,-36};
        public static bool IsBird(WildlifeSpecies species)=>(int)species<8||species==WildlifeSpecies.Bullfinch||species==WildlifeSpecies.BarnOwl;

        // When a view becomes visible mid-cycle, wait for the next scheduled
        // arrival so the first creature never materializes halfway across the sky.
        public struct VisitGate
        {
            int cycle;
            bool admitted;
            bool initialized;
            public bool Admit(int currentCycle,float age)
            {
                if(!initialized||cycle!=currentCycle){initialized=true;cycle=currentCycle;admitted=age<=.32f;}
                return admitted;
            }
            public void Reset(){cycle=int.MinValue;admitted=false;initialized=false;}
        }

        public static WildlifeFlightStyle Style(WildlifeSpecies species)
        {
            if(species==WildlifeSpecies.BarnOwl)return WildlifeFlightStyle.Owl;
            if(species==WildlifeSpecies.Bumblebee)return WildlifeFlightStyle.Bee;
            if(species==WildlifeSpecies.FieldCricket)return WildlifeFlightStyle.Cricket;
            if(species==WildlifeSpecies.Gull||species==WildlifeSpecies.Tern)return WildlifeFlightStyle.SoaringBird;
            if(IsBird(species))return WildlifeFlightStyle.SmallBird;
            if(species==WildlifeSpecies.Dragonfly||species==WildlifeSpecies.Damselfly)return WildlifeFlightStyle.Dragonfly;
            if(species==WildlifeSpecies.Butterfly)return WildlifeFlightStyle.Butterfly;
            if(species==WildlifeSpecies.LunaMoth||species==WildlifeSpecies.BookMoth||species==WildlifeSpecies.MoonMoth)return WildlifeFlightStyle.Moth;
            return WildlifeFlightStyle.Beetle;
        }

        public static WildlifePose Pose(WildlifeFlightStyle style,float age,float wingRate,float seed,float turn=0,float previousTurn=0,float effort=0)
        {
            bool bird=style==WildlifeFlightStyle.SmallBird||style==WildlifeFlightStyle.SoaringBird||style==WildlifeFlightStyle.Owl;
            // Integrate a slowly varying cadence; multiplying time by a varying rate
            // would jump the phase whenever a bird changed effort.
            float cadence=.92f+Hash((int)(seed*100003)+419)*.16f;
            float clock=age*wingRate*cadence+seed*3.17f
                +.10f*Mathf.Sin(age*.83f+seed*6)+.035f*Mathf.Sin(age*1.71f+seed*4);
            if(style==WildlifeFlightStyle.Butterfly||style==WildlifeFlightStyle.Moth)
                clock+=.13f*Mathf.Sin(age*1.37f+seed*9)+.055f*Mathf.Sin(age*3.61f+seed*4);
            float phase=UnitCycle(clock),amount=1;
            bool gliding=false;
            if(bird) {
                bool soaring=style==WildlifeFlightStyle.SoaringBird||style==WildlifeFlightStyle.Owl;
                float burstPeriod=(soaring?6.4f:4.5f)+seed*.7f;
                float burstLength=(soaring?1.55f:2.05f)+seed*.35f;
                float burst=Mathf.Repeat(age+seed*burstPeriod,burstPeriod);
                amount=Mathf.SmoothStep(0,1,Mathf.Min(burst/.18f,(burstLength-burst)/.24f));
                amount=Mathf.Lerp(amount,1,Mathf.Clamp01(effort));
                gliding=burst>=burstLength&&effort<=0;
            }
            float near=Angle(phase)*amount,far=Angle(Mathf.Repeat(phase-.055f,1))*amount;
            float fold=Mathf.Sin(phase*Mathf.PI*2)*(bird?amount:1);
            float lift=bird?Mathf.Sin(phase*Mathf.PI*2-.6f)*amount*.013f:0;
            switch(style) {
                case WildlifeFlightStyle.SoaringBird: near*=.86f;far*=.86f;break;
                case WildlifeFlightStyle.Butterfly: near=near*.75f-8;far=far*.69f-11;break;
                case WildlifeFlightStyle.Moth: near=near*.59f-6;far=far*.56f-9;break;
                case WildlifeFlightStyle.Dragonfly: near=near*.16f;far=far*.14f;fold*=.45f;break;
                case WildlifeFlightStyle.Beetle: near=near*.52f;far=far*.46f;break;
                case WildlifeFlightStyle.Bee: near*=.31f;far*=.29f;fold*=.28f;break;
                case WildlifeFlightStyle.Owl: near*=.72f;far*=.68f;lift*=.65f;break;
                case WildlifeFlightStyle.Cricket: near=far=fold=lift=0;break;
            }
            if(bird) {
                float rest=Mathf.Sin(age*1.1f+seed*6)*2.0f;
                near+=rest*(1-amount);far+=(rest-2)*(1-amount);
            }
            // A delayed response lets feathers follow the body rather than turn as one flat card.
            float tail=Mathf.Clamp((previousTurn-turn)*.7f,-9,9)+Mathf.Sin(age*2.1f+seed*5)*1.5f;
            float flex=(Angle(phase)-Angle(Mathf.Repeat(phase-wingRate*.042f,1)))*.00125f*(bird?amount:.72f);
            float farFlex=(Angle(Mathf.Repeat(phase-.055f,1))-Angle(Mathf.Repeat(phase-.055f-wingRate*.054f,1)))*.0011f*(bird?amount:.65f);
            return new WildlifePose(near,far,fold,tail,lift,flex,farFlex,Mathf.FloorToInt(phase*8),gliding);
        }

        static float Angle(float phase)
        {
            // Mathf.Repeat(-epsilon,1) can round to exactly 1 on float32.
            // Wrap once more before indexing the eight authored phases.
            phase=UnitCycle(phase);
            float slot=phase*8;int a=Mathf.FloorToInt(slot);float t=slot-a;
            // Periodic cubic tangents carry velocity through the intermediate poses.
            // Easing each adjacent pair independently stopped the wing eight times per beat.
            float p0=wingAngles[(a+7)%8],p1=wingAngles[a],p2=wingAngles[(a+1)%8],p3=wingAngles[(a+2)%8];
            return .5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);
        }
        static float UnitCycle(float value){float p=value-Mathf.Floor(value);return p>=1?0:Mathf.Max(0,p);}

        public static float Travel(WildlifeFlightStyle style,float progress,float seed)
        {
            progress=Mathf.Clamp01(progress);
            if(style==WildlifeFlightStyle.Dragonfly||style==WildlifeFlightStyle.Bee) {
                if(progress<.18f)return Segment(progress,0,.18f,0,.20f);
                if(progress<.32f)return .20f;
                if(progress<.46f)return Segment(progress,.32f,.46f,.20f,.52f);
                if(progress<.67f)return .52f;
                if(progress<.84f)return Segment(progress,.67f,.84f,.52f,.88f);
                return Segment(progress,.84f,1,.88f,1);
            }
            if(style==WildlifeFlightStyle.Butterfly||style==WildlifeFlightStyle.Moth) {
                float route=Mathf.SmoothStep(0,1,progress);
                float envelope=Mathf.Sin(progress*Mathf.PI);
                return Mathf.Clamp01(route+Mathf.Sin(progress*Mathf.PI*4+seed*5)*.026f*envelope);
            }
            // A monotonic speed pulse: acceleration out of cover, a relaxed glide,
            // then a second burst. Endpoints and the frozen-clock contract stay exact.
            float eased=progress*progress*(3-2*progress);
            return eased+Mathf.Sin(progress*Mathf.PI*2)*Mathf.Sin(progress*Mathf.PI)*.045f;
        }

        public static float Depth(float progress,float seed)
        {
            float t=Mathf.Clamp01(progress),near=seed<.5f?t:1-t;
            return Mathf.Lerp(.52f,1.30f,Mathf.SmoothStep(0,1,near))
                +Mathf.Sin(t*Mathf.PI)*(.12f+seed*.14f);
        }

        public static void Route(float side,float seed,bool crossing,bool map,out Vector3 a,out Vector3 b,out Vector3 c,out Vector3 d)
        {
            float start=Mathf.Lerp(-.26f,.29f,Hash((int)(seed*100000)+31));
            float finish=Mathf.Lerp(-.25f,.30f,Hash((int)(seed*100000)+67));
            float bend=(seed<.5f?1:-1)*Mathf.Lerp(.13f,.25f,Hash((int)(seed*100000)+101));
            if(crossing){
                // Every arrival starts beyond the viewport, including the occasional
                // high flight. The broad two-control turn prevents a straight drop.
                bool top=seed>.88f;
                float edge=Mathf.Lerp(.72f,.82f,Hash((int)(seed*100000)+131));
                if(top){
                    a=new Vector3(Mathf.Lerp(-.24f,.24f,Hash((int)(seed*100000)+157)),edge);
                    d=new Vector3(-side*edge,finish);
                    b=Vector3.Lerp(a,d,.26f)+new Vector3(side*.20f,bend*.70f);
                    c=Vector3.Lerp(a,d,.70f)+new Vector3(-side*.17f,-bend*.52f);
                }else{
                    a=new Vector3(side*edge,start);
                    d=new Vector3(-side*edge,finish);
                    b=Vector3.Lerp(a,d,.32f)+Vector3.up*bend+Vector3.right*(side*.07f);
                    c=Vector3.Lerp(a,d,.68f)-Vector3.up*(bend*.8f)-Vector3.right*(side*.06f);
                }
            }else{
                // A guest sweeps in from outside, makes a loose sideways curve in
                // the edge of the sky, then clears the frame before fading away.
                float edge=Mathf.Lerp(.74f,.84f,Hash((int)(seed*100000)+131));
                float lane=map?Mathf.Lerp(.35f,.47f,Hash((int)(seed*100000)+157)):
                    Mathf.Lerp(.16f,.27f,Hash((int)(seed*100000)+157));
                float turn=map?Mathf.Lerp(.43f,.55f,Hash((int)(seed*100000)+181)):
                    Mathf.Lerp(.55f,.78f,Hash((int)(seed*100000)+181));
                float entry=Mathf.Lerp(-.52f,.52f,Hash((int)(seed*100000)+197));
                float exit=Mathf.Lerp(-.52f,.52f,Hash((int)(seed*100000)+211));
                if(Mathf.Abs(exit-entry)<.30f)exit=Mathf.Clamp(exit+(exit>=entry?.42f:-.42f),-.68f,.68f);
                float arc=(Hash((int)(seed*100000)+223)<.5f?-1:1)*Mathf.Lerp(.17f,.32f,Hash((int)(seed*100000)+239));
                a=new Vector3(side*edge,entry);
                b=new Vector3(side*lane,Mathf.Clamp(Mathf.Lerp(entry,exit,.24f)+arc,-.84f,.84f));
                c=new Vector3(side*turn,Mathf.Clamp(Mathf.Lerp(entry,exit,.72f)-arc*.70f,-.84f,.84f));
                d=new Vector3(side*edge,exit);
            }
        }

        static float Segment(float p,float from,float to,float a,float b)
            => Mathf.Lerp(a,b,Mathf.SmoothStep(0,1,(p-from)/(to-from)));

        public static Vector3 Flutter(WildlifeFlightStyle style,float age,float progress,float seed,float strength)
        {
            float fade=Mathf.Sin(Mathf.Clamp01(progress)*Mathf.PI),x=0,y=0;
            switch(style) {
                case WildlifeFlightStyle.Butterfly:
                    x=Mathf.Sin(age*.73f+seed*7)*strength*.18f;
                    y=Mathf.Sin(age*.91f+seed*6)*strength*.30f;
                    break;
                case WildlifeFlightStyle.Dragonfly:
                    x=Mathf.Sin(age*.8f+seed*9)*strength*.08f;
                    y=Mathf.Sin(age*.9f+seed*4)*strength*.09f;
                    break;
                case WildlifeFlightStyle.Bee:
                    x=Mathf.Sin(age*1.3f+seed*9)*strength*.13f;
                    y=Mathf.Sin(age*1.1f+seed*4)*strength*.16f;
                    break;
                case WildlifeFlightStyle.Moth:
                    x=Mathf.Sin(age*.65f+seed*8)*strength*.22f;
                    y=Mathf.Sin(age*.82f+seed*3)*strength*.24f;
                    break;
                default:y=Mathf.Sin(age*.65f+seed*6)*strength*.20f;break;
            }
            return new Vector3(x*fade,y*fade,0);
        }

        public static Vector3 Path(Vector3 a,Vector3 b,Vector3 c,Vector3 d,WildlifeFlightStyle style,
            float age,float duration,float seed,float strength,int zone,float side)
        {
            float progress=Mathf.Clamp01(age/duration),route=Travel(style,progress,seed);
            var point=CruisePoint(a,b,c,d,style,route,seed);
            point+=Flutter(style,age,progress,seed,strength);
            if(style==WildlifeFlightStyle.Moth&&(zone==8||zone==5||zone==7)) {
                float loop=Mathf.SmoothStep(0,1,progress/.21f)*Mathf.SmoothStep(0,1,(1-progress)/.23f);
                float angle=age*(.90f+seed*.22f)+seed*8;
                var light=new Vector3(side*.463f,zone==8?-.285f:-.10f,0);
                var orbit=new Vector3(Mathf.Sin(angle)*.009f,Mathf.Cos(angle*.94f)*.027f,0);
                point=Vector3.Lerp(point,light+orbit,loop);
            }
            return point;
        }
        static Vector3 CruisePoint(Vector3 a,Vector3 b,Vector3 c,Vector3 d,WildlifeFlightStyle style,float route,float seed)
        {
            var point=Bezier(a,b,c,d,route);
            // A broad excursion is part of the route, not high-frequency jitter. The
            // smooth envelope returns with the same tangent at both ends. In side
            // gutters the ellipse stays narrow; open sky allows an actual return arc.
            if(seed>.16f) {
                bool crossing=Mathf.Abs(d.x-a.x)>.7f;
                float q=Mathf.Clamp01((route-.12f)/.76f);
                float envelope=Mathf.Sin(q*Mathf.PI);envelope*=envelope;
                float angle=q*Mathf.PI*2+(seed>.68f?Mathf.Sin(q*Mathf.PI*2)*.35f:0);
                bool mapGutter=Mathf.Abs(d.x-a.x)<.12f&&Mathf.Abs(b.x)>.32f;
                float radius=crossing?Mathf.Lerp(.29f,.37f,seed):mapGutter?.013f:Mathf.Lerp(.11f,.19f,seed);
                float rise=style==WildlifeFlightStyle.SoaringBird?.12f:.085f;
                float direction=d.x<a.x?-1:1;
                point+=new Vector3(direction*radius*Mathf.Sin(angle)*envelope,
                    (seed<.55f?1:-1)*rise*(1-Mathf.Cos(angle))*envelope,0);
            }
            return point;
        }

        public static Vector3 Bezier(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
        { float u=1-t;return u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d; }
        public static Vector3 Tangent(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
        { float u=1-t;return 3*u*u*(b-a)+6*u*t*(c-b)+3*t*t*(d-c); }
        public static float Bank(Vector3 tangent,float facing)
            => Mathf.Clamp(Mathf.Atan2(tangent.y,Mathf.Abs(tangent.x))*Mathf.Rad2Deg*.23f*Mathf.Clamp(tangent.x*4,-1,1),-20,20);

        // Looking along a wide section of the route ignores tiny wing/body flutter.
        // A hover retains the route's direction rather than flipping on numerical noise.
        public static Vector3 Heading(Vector3 a,Vector3 b,Vector3 c,Vector3 d,WildlifeFlightStyle style,
            float age,float duration,float seed,int zone,float side)
        {
            var delta=Path(a,b,c,d,style,Mathf.Clamp(age+.32f,0,duration),duration,seed,0,zone,side)
                -Path(a,b,c,d,style,Mathf.Clamp(age-.32f,0,duration),duration,seed,0,zone,side);
            if(delta.sqrMagnitude<.00000005f) {
                float route=Travel(style,age/duration,seed);
                delta=CruisePoint(a,b,c,d,style,Mathf.Clamp01(route+.002f),seed)
                    -CruisePoint(a,b,c,d,style,Mathf.Clamp01(route-.002f),seed);
            }
            return delta.sqrMagnitude>.00000001f?delta.normalized:new Vector3(-side,0,0);
        }
        public static float RearView(Vector3 heading)
            =>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.72f,Mathf.Abs(heading.x)));

        public static float Hash(int value)
        {
            uint hash=unchecked((uint)value*747796405u+2891336453u);
            hash=((hash>>(int)((hash>>28)+4))^hash)*277803737u;
            return ((hash>>22)^hash)/(float)uint.MaxValue;
        }
    }
}
