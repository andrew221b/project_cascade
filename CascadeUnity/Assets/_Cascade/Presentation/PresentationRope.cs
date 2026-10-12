using UnityEngine;

namespace Cascade.Presentation
{
    // Cosmetic point-mass chain. Never reads symbols, commands, rewards or puzzle state.
    public sealed class PresentationRope
    {
        const float Step=1f/90f;
        readonly Vector2[] points,previous;
        readonly int seed;
        Vector2 lastTop,lastBottom;
        float remainder,lastLength;
        bool ready;
        public PresentationRope(int nodes,int variation){points=new Vector2[nodes];previous=new Vector2[nodes];seed=variation;}
        public void Advance(Vector2 top,Vector2 bottom,float seconds,float time,float wind,float force,float gravity,float limit,bool still)
        {
            float length=Vector2.Distance(top,bottom);
            bool discontinuity=!ready||Mathf.Abs(length-lastLength)>Mathf.Max(.01f,lastLength*.15f)||
                Vector2.Distance(top,lastTop)>Mathf.Max(.03f,length*.30f)||Vector2.Distance(bottom,lastBottom)>Mathf.Max(.03f,length*.30f);
            if(discontinuity||still){
                for(int i=0;i<points.Length;i++){
                    float t=i/(float)(points.Length-1);
                    var bow=still?Vector2.zero:Vector2.right*(Mathf.Min(limit,length*.018f)*wind*4*t*(1-t));
                    points[i]=previous[i]=Vector2.Lerp(top,bottom,t)+bow;
                }
                remainder=0;ready=true;
            }
            lastTop=top;lastBottom=bottom;lastLength=length;Pin(top,bottom);
            if(still||seconds<=0||length<.001f)return;
            remainder+=Mathf.Clamp(seconds,0,.05f);float link=length/(points.Length-1)*1.006f;
            while(remainder>=Step){
                remainder-=Step;
                for(int i=1;i<points.Length-1;i++){
                    float t=i/(float)(points.Length-1);
                    float eddy=Mathf.PerlinNoise(time*.43f+t*.74f,seed*.137f+t*1.61f)*2-1;
                    float ripple=Mathf.PerlinNoise(time*.81f-t*.44f,seed*.273f+t*3.4f)*2-1;
                    var acceleration=new Vector2(force*(wind*(.85f+.24f*eddy)+eddy*.18f+ripple*.07f),-gravity);
                    var current=points[i];var velocity=(current-previous[i])*Mathf.Exp(-1.8f*Step);
                    points[i]=current+velocity+acceleration*(Step*Step);previous[i]=current;
                }
                for(int pass=0;pass<6;pass++){
                    for(int i=0;i<points.Length-1;i++)Constrain(i,i+1,link,1);
                    for(int i=points.Length-2;i>=0;i--)Constrain(i,i+1,link,1);
                    for(int i=0;i<points.Length-2;i++)Constrain(i,i+2,link*1.995f,.06f);
                    // Suppress tiny accordion folds while preserving broad inertial bends.
                    for(int i=1;i<points.Length-1;i++)points[i]=Vector2.Lerp(points[i],(points[i-1]+points[i+1])*.5f,.018f);
                    Pin(top,bottom);
                }
                // A visual guard for extreme viewport/focus changes, not a prescribed wave shape.
                for(int i=1;i<points.Length-1;i++){
                    float t=i/(float)(points.Length-1);var center=Vector2.Lerp(top,bottom,t);
                    points[i]=center+Vector2.ClampMagnitude(points[i]-center,Mathf.Min(limit,length*.13f)*Mathf.Sin(t*Mathf.PI));
                }
                Pin(top,bottom);
            }
        }
        void Pin(Vector2 top,Vector2 bottom){points[0]=previous[0]=top;points[points.Length-1]=previous[points.Length-1]=bottom;}
        void Constrain(int a,int b,float length,float strength)
        {
            var delta=points[b]-points[a];float distance=delta.magnitude;if(distance<.00001f)return;
            var correction=delta*((distance-length)/distance)*strength;
            if(a==0)points[b]-=correction;else if(b==points.Length-1)points[a]+=correction;
            else{points[a]+=correction*.5f;points[b]-=correction*.5f;}
        }
        public void Impulse(float sideways)
            =>Impulse(Vector2.right*sideways);
        public void Impulse(Vector2 velocity)
        {
            if(!ready)return;
            for(int i=1;i<points.Length-1;i++)previous[i]-=velocity*(Step*Mathf.Sin(i/(float)(points.Length-1)*Mathf.PI));
        }
        public Vector2 Sample(float t)
        {
            float u=Mathf.Clamp01(t)*(points.Length-1);int i=Mathf.Min(points.Length-2,(int)u);float f=u-i;
            var b=points[i];var c=points[i+1];var a=i==0?b+(b-c):points[i-1];var d=i+2>=points.Length?c+(c-b):points[i+2];
            return .5f*((2*b)+(-a+c)*f+(2*a-5*b+4*c-d)*f*f+(-a+3*b-3*c+d)*f*f*f);
        }
    }
}
