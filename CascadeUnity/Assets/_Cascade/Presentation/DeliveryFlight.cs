using UnityEngine;

namespace Cascade.Presentation
{
    public static class DeliveryFlight
    {
        // Gravity first, then a C1-continuous deceleration into the moving seat.
        public static Vector3 Fall(Vector3 start,Vector3 seat,float progress,bool receiver)
        {
            float t=Mathf.Clamp01(progress),join=receiver?.65f:.88f;
            float vertical=t*t;
            if(t>join){float q=(t-join)/(1-join),q2=q*q,q3=q2*q;
                vertical=(2*q3-3*q2+1)*join*join+(q3-2*q2+q)*2*join*(1-join)+(-2*q3+3*q2);
            }
            var p=Vector3.Lerp(start,seat,t*t*(3-2*t));p.y=Mathf.Lerp(start.y,seat.y,vertical);return p;
        }
        public static Vector3 Reject(Vector3 start,Vector3 comparison,float progress,int slot,int count)
        {
            float t=Mathf.Clamp01(progress),s=t*t*(3-2*t),fan=(slot-(count-1)*.5f)/Mathf.Max(1,count*.5f);
            Vector3 a=start+new Vector3(fan*.60f,.20f,0),b=comparison+new Vector3(fan*.32f,.12f,0);
            float u=1-s;return u*u*u*start+3*u*u*s*a+3*u*s*s*b+s*s*s*comparison;
        }
    }
}
