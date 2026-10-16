using UnityEngine;

namespace Cascade.Presentation
{
    public readonly struct SkyFlightPose
    {
        public readonly Vector2 Position;
        public readonly float Scale,Roll,Alpha,Facing,Proximity;
        public SkyFlightPose(Vector2 position,float scale,float roll,float alpha,float facing,float proximity=0)
        {Position=position;Scale=scale;Roll=roll;Alpha=alpha;Facing=facing;Proximity=proximity;}
    }
    // All clocks are decorative. A wrap happens outside the opening at zero opacity.
    public static class SkyFlightMotion
    {
        public static bool Visible(int index,int world,int capacity)=>(index-world%6+6)%6<capacity;
        public static float SourceFacing(int index)=>index==1||index==3?-1:1;
        static float Duration(int index)=>index==0?100:index==1?86:index==2?62:index==3?90:index==4?54:112;
        public static float Width(int index,SkyFlightPose pose)=>(index==0||index==5?.075f:index==1?.12f:.10f)*pose.Scale;
        // Two staggered arrivals share one schedule. A craft occupies either a
        // nearer lower lane or a smaller distant upper lane, never a six-ship swarm.
        public static bool TryFleet(float age,int index,int world,int capacity,out SkyFlightPose pose,int leadKindOffset=0)
        {
            for(int lane=0;lane<Mathf.Clamp(capacity,0,2);lane++){
                float clock=age+world*11.7f+lane*69;int visit=Mathf.FloorToInt(clock/138);
                int kind=((world+visit+lane*3+leadKindOffset)%6+6)%6;if(kind!=index)continue;
                float seed=WildlifeMotion.Hash(world*137+visit*311+lane*719+43),t=Mathf.Repeat(clock,138);
                float proximity=lane==0?Mathf.Lerp(.56f,.88f,seed):Mathf.Lerp(.08f,.27f,seed);
                float duration=Mathf.Min(128,Duration(kind)*Mathf.Lerp(.91f,1.08f,seed)*Mathf.Lerp(1.2f,.85f,proximity));
                if(t>=duration)continue;
                pose=Route(t/duration,t,kind,seed,(visit+world+lane)%2==0?1:-1,lane,proximity);return pose.Alpha>.001f;
            }
            pose=default;return false;
        }
        public static SkyFlightPose Sample(float age,int index,int world,float offset=0)
        {
            float duration=Duration(index);
            float clock=age/duration+Mathf.Repeat(index*.381966f+world*.071f+.23f+offset,1);
            int visit=Mathf.FloorToInt(clock);float seed=WildlifeMotion.Hash(index*137+world*719+visit*311+43);
            return Route(Mathf.Repeat(clock,1),age,index,seed,(index+visit)%2==0?1:-1,index%2,Mathf.Lerp(.1f,.88f,seed));
        }
        static SkyFlightPose Route(float p,float age,int index,float seed,float direction,int lane,float proximity)
        {
            bool balloon=index==0||index==5,airship=index==1||index==3;
            float travel=p+Mathf.Sin(p*Mathf.PI*2)*.026f,u=direction>0?travel:1-travel;
            float height=lane==0?.24f+seed*.12f:.74f+seed*.07f;
            float arc=balloon?.018f:airship?.028f:.064f,s=.5f+seed*.5f;
            float y=height+Mathf.Sin(p*Mathf.PI)*arc+Mathf.Sin(p*Mathf.PI*2)*arc*.40f*s;
            float frequency=balloon?.32f:.24f,bob=(balloon?.006f:.0025f)*Mathf.Sin(age*frequency+seed*8);
            float slope=(Mathf.Cos(p*Mathf.PI)*arc*Mathf.PI+Mathf.Cos(p*Mathf.PI*2)*arc*.8f*Mathf.PI*s)/1.44f;
            float roll=balloon?Mathf.Sin(age*frequency+seed*8-.65f)*1.2f:direction*Mathf.Atan(slope)*Mathf.Rad2Deg+Mathf.Sin(age*.17f+seed*8)*.5f;
            float alpha=Mathf.SmoothStep(0,1,p/.11f)*Mathf.SmoothStep(0,1,(1-p)/.11f);
            return new SkyFlightPose(new Vector2(-.22f+u*1.44f,y+bob),Mathf.Lerp(.45f,1.10f,proximity)*(1+Mathf.Sin(p*Mathf.PI)*.035f),roll,alpha,direction,proximity);
        }
        public static Vector3 Hover(float age,float seed)
            =>new Vector3(Mathf.Sin(age*.19f+seed)*.75f+Mathf.Sin(age*.073f+seed*2)*.25f,
                Mathf.Sin(age*.43f+seed)*.8f+Mathf.Sin(age*.17f+seed)*.2f,
                Mathf.Sin(age*.43f+seed-.65f)*.85f);
    }
}
