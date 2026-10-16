using UnityEngine;
namespace Cascade.Presentation
{
    // Decorative UI motion reads the scene clock; it cannot call puzzle commands.
    public sealed class WorldMotion:MonoBehaviour
    {
        CascadeGame game;RectTransform rect;Vector2 rest,travel;float seed,angle;bool aircraft;
        public void Initialize(CascadeGame owner,Vector2 displacement,float phase=0,float rotation=0,bool aircraft=false)
        {game=owner;rect=(RectTransform)transform;rest=rect.anchoredPosition;travel=displacement;seed=phase;angle=rotation;this.aircraft=aircraft;}
        void LateUpdate()
        {
            if(game==null||game.Machine==null||rect==null)return;
            float t=game.ReducedMotion?0:game.Machine.AmbientPhase;
            if(aircraft){var pose=SkyFlightMotion.Hover(t,seed);rect.anchoredPosition=rest+Vector2.Scale(new Vector2(pose.x,pose.y),travel);rect.localRotation=Quaternion.Euler(0,0,pose.z*angle);return;}
            rect.anchoredPosition=rest+new Vector2(Mathf.Sin(t*.22f+seed)*travel.x,Mathf.Sin(t*.7f+seed)*travel.y);
            rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*.65f+seed)*angle);
        }
    }
}
