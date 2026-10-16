using System;
namespace Cascade.Application
{
    // View-only smooth weather. No random calls or gameplay transitions.
    public static class WorkshopWind
    {
        static float Noise(float t,int salt)
        {
            int cell=(int)Math.Floor(t);float x=t-cell;x=x*x*x*(x*(x*6-15)+10);
            float Value(int n){unchecked{uint h=(uint)(n*374761393+salt*668265263);h=(h^(h>>13))*1274126177;return (h&65535)/32767.5f-1;}}
            float a=Value(cell);return a+(Value(cell+1)-a)*x;
        }
        public static float Sample(float seconds)
        {
            float cycle=(float)Math.Floor(seconds/28f),age=seconds-cycle*28f;
            float start=2+(Noise(cycle,37)+1)*1.7f;
            float length=8+(Noise(cycle,53)+1)*2;
            float strength=.46f+(Noise(cycle,71)+1)*.2f;
            float envelope=Pulse(age-start,length);
            // Some gusts have a weaker second breath; others leave a long calm.
            if(Noise(cycle,97)>.05f)envelope+=Pulse(age-19,5+(Noise(cycle,107)+1))*.48f;
            float direction=Noise(cycle,113)<0?-1:1;
            float turbulence=.86f+Noise(seconds*.43f,3)*.10f+Noise(seconds*.91f,19)*.04f;
            return direction*strength*envelope*turbulence;
        }
        static float Pulse(float age,float length)
        {
            if(age<=0||age>=length)return 0;
            // Continuous value and velocity at either end, with a gentle lull inside.
            float swell=(float)Math.Pow(Math.Sin(age/length*Math.PI),2);
            return swell*(.88f+.12f*(float)Math.Cos(age*1.3f));
        }
    }
}
