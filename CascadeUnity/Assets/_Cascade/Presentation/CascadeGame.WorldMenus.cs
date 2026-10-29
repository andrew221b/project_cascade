using System;
using System.Linq;
using Cascade.Content;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        public int EnvironmentWorld {
            get {
                if(CurrentScreen==GameScreen.WorldComplete&&completedWorld>=0)return completedWorld;
                if((CurrentScreen==GameScreen.Pause||CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Fail)&&machine?.LivingWorld!=null)return machine.LivingWorld.Zone;
                if(CurrentScreen==GameScreen.Prologue&&worldComic>=0)return worldComic;
                if(CurrentScreen==GameScreen.JourneyMap||CurrentScreen==GameScreen.Dialogue)return Mathf.Clamp(mapPage,0,8);
                if(Catalog==null||profile==null||Catalog.PrimaryLevels.Count==0)return 0;
                int index=PrimaryLevelIndex(node);
                bool outcome=journeyActive&&(CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Parcel||CurrentScreen==GameScreen.Pause);
                return JourneyStory.Zone((HasResumableJourneyAttempt||outcome)&&index>=0?index:NextJourneyStop());
            }
        }
        void BindMenuWorldEnvironment()
        {
            if(CurrentScreen==GameScreen.Playing||CurrentScreen==GameScreen.Fail||CurrentScreen==GameScreen.Checkpoint||CurrentScreen==GameScreen.NoAds||menuRoot==null||ReferenceIslandScreen)return;
            int world=EnvironmentWorld;machine.SetZone(world);
            var owner=Rect("Themed world menu owner",menuRoot,Vector2.zero,Vector2.one);
            owner.gameObject.AddComponent<MenuWorldEnvironment>().Initialize(this,menuRoot,world);
        }
    }
    public static class MenuWorldArt
    {
        public const string Path="CascadeArt/WorldMenusOct14/";
        static readonly string[] names={"WindGardenBase","RainMillBase","CopperOrchardBase","CloudPostBase","GlassLagoonBase","MossArchiveBase","SnowSpireBase","GreatBeaconBase","StarObservatoryBase"};
        public static Texture2D Sky(int world){world=Mathf.Clamp(world,0,8);var revised=(world==0?GameplayReferenceArt.Texture("ReferenceDaySky"):null)??Resources.Load<Texture2D>(Path+names[world]);return revised??Resources.Load<Texture2D>("CascadeArt/"+(world<3?"Journey/":"Woodland/")+names[world]);}
        public static Color Light(int world,float age,bool moving)
        {
            var p=WorldLighting.Evaluate(world,false,age,moving);var r=WorldLighting.Evaluate(0,false,0,false);
            var c=p.Key*p.Intensity*p.Transmission*.4f+p.Sky*.7f+p.Horizon*.4f;
            var b=r.Key*r.Intensity*.4f+r.Sky*.7f+r.Horizon*.4f;
            float exposure=Mathf.Clamp(Mathf.Sqrt(p.Sky.grayscale/r.Sky.grayscale),.66f,1);
            return new Color(exposure*Mathf.Pow(Mathf.Clamp(c.r/b.r,.35f,1.25f),.4545f),exposure*Mathf.Pow(Mathf.Clamp(c.g/b.g,.35f,1.25f),.4545f),exposure*Mathf.Pow(Mathf.Clamp(c.b/b.b,.35f,1.25f),.4545f),1);
        }
    }
}
