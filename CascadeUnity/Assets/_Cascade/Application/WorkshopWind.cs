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
            float cycle=seconds/14f,phase=cycle-(float)Math.Floor(cycle);
            float envelope=(float)Math.Pow(Math.Sin(phase*Math.PI),6);
            float gust=Noise((float)Math.Floor(cycle)*.9f,7)*envelope*.47f;
            return Math.Max(-1,Math.Min(1,Noise(seconds*.12f,3)*.45f+Noise(seconds*.31f,19)*.16f+(float)Math.Sin(seconds*.67f)*.19f+gust));
        }
    }
}
