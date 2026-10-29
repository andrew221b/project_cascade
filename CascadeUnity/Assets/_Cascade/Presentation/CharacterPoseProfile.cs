using UnityEngine;

namespace Cascade.Presentation
{
    public enum CharacterPoseKind { Lantern, Thinking, Apple, Sad, Victory, Speaker }

    // Landmarks are measured in a complete art cell, top-left origin. Cropped
    // dialogue busts and full-body skins therefore use the very same rig.
    public sealed class CharacterPoseProfile
    {
        [System.Serializable] sealed class CostumeLandmark { public string asset;public int pose;public Vector4 near,far,mouth,lantern;public Vector2 hook; }
        [System.Serializable] sealed class LandmarkBook { public CostumeLandmark[] poses; }
        static LandmarkBook costumeLandmarks;
        public string Name;
        public CharacterPoseKind Kind;
        public Rect Cell;
        public Vector4 NearEye,FarEye,Mouth;
        public Vector2 Neck,TailRoot,ScarfRoot,LanternHook,BagHook;
        public Vector4 Tail,Scarf,Lantern,Bag;
        public bool OpenMouth,Beak,Contour;
        public float Ground=.92f,FabricGain=1;
        public Vector4 NearEar,FarEar,Accent;
        public Vector2 NearEarRoot,FarEarRoot,AccentRoot;
        public float EarGain=1,AccentGain=1,SniffGain=1;
        public bool Whiskers=true,SoftBrows=true;
        public Vector2 Point(Sprite sprite,Vector2 uv)=>new Vector2((uv.x*sprite.texture.width-Cell.x)/Cell.width,((1-uv.y)*sprite.texture.height-Cell.y)/Cell.height);
        public Vector2 Uv(Sprite sprite,Vector2 p)=>new Vector2((Cell.x+p.x*Cell.width)/sprite.texture.width,1-(Cell.y+p.y*Cell.height)/sprite.texture.height);

        static Vector4 Area(float x,float y,float rx,float ry)=>new Vector4(x,y,rx,ry);
        static CharacterPoseProfile Mouse(string name,Rect cell,CharacterPoseKind kind,Vector4 near,Vector4 far,Vector4 mouth,Vector2 neck)
        {
            return new CharacterPoseProfile{Name=name,Cell=cell,Kind=kind,NearEye=near,FarEye=far,Mouth=mouth,Neck=neck,
                TailRoot=new Vector2(.35f,.65f),Tail=Area(.18f,.59f,.23f,.24f),ScarfRoot=new Vector2(.49f,.34f),Scarf=Area(.24f,.31f,.25f,.085f),
                LanternHook=new Vector2(.865f,.605f),Lantern=Area(.865f,.705f,.105f,.13f),BagHook=new Vector2(.46f,.43f),Bag=Area(.34f,.62f,.17f,.14f)};
        }
        public static CharacterPoseProfile For(Sprite sprite)
        {
            var profile=Create(sprite);if(profile!=null)profile.DetailLandmarks();return profile;
        }
        static CharacterPoseProfile Create(Sprite sprite)
        {
            if(sprite==null)return null;
            string name=sprite.texture.name;var cell=new Rect(0,0,sprite.texture.width,sprite.texture.height);
            if(name=="WelcomeMika")return null; // Its authored trial rig remains intact.
            if(name=="HomeMika"){
                return Mouse(name,cell,CharacterPoseKind.Lantern,Area(.476f,.190f,.045f,.033f),Area(.659f,.169f,.036f,.030f),Area(.589f,.263f,.047f,.025f),new Vector2(.57f,.315f)).WithOpenMouth();
            }
            if(name=="InventoryMika"){
                var p=Mouse(name,cell,CharacterPoseKind.Lantern,Area(.536f,.186f,.050f,.033f),Area(.711f,.160f,.023f,.029f),Area(.674f,.258f,.033f,.012f),new Vector2(.615f,.32f));
                p.LanternHook=new Vector2(.905f,.643f);p.Lantern=Area(.90f,.742f,.10f,.12f);return p;
            }
            if(name=="RestingMika"){
                var p=Mouse(name,cell,CharacterPoseKind.Apple,Area(.483f,.307f,.044f,.047f),Area(.651f,.258f,.031f,.041f),Area(.604f,.392f,.039f,.014f),new Vector2(.565f,.435f));
                p.TailRoot=new Vector2(.35f,.69f);p.Tail=Area(.27f,.63f,.15f,.18f);p.ScarfRoot=new Vector2(.47f,.455f);p.Scarf=Area(.28f,.435f,.19f,.09f);p.Lantern=Vector4.zero;p.Bag=Vector4.zero;return p;
            }
            if(name=="VictoryMikaLanternCutoutOct12"){
                var p=Mouse(name,cell,CharacterPoseKind.Victory,Area(.388f,.356f,.053f,.051f),Area(.615f,.277f,.041f,.049f),Area(.538f,.442f,.067f,.033f),new Vector2(.52f,.50f)).WithOpenMouth();
                p.LanternHook=new Vector2(.866f,.26f);p.Lantern=Area(.866f,.457f,.13f,.235f);p.Tail=Vector4.zero;p.ScarfRoot=new Vector2(.48f,.57f);p.Scarf=Area(.24f,.59f,.24f,.13f);p.BagHook=new Vector2(.43f,.67f);p.Bag=Area(.24f,.90f,.17f,.17f);return p;
            }
            if(name=="FailureReferenceSource"){
                // This sprite is a contour cut from the supplied reference photo.
                cell=new Rect(sprite.rect.x,sprite.texture.height-sprite.rect.yMax,sprite.rect.width,sprite.rect.height);
                var p=Mouse("Seated sad Mika",cell,CharacterPoseKind.Sad,Area(.642f,.359f,.059f,.044f),Area(.789f,.388f,.044f,.039f),Area(.740f,.477f,.035f,.014f),new Vector2(.72f,.53f));
                p.TailRoot=new Vector2(.39f,.63f);p.Tail=Area(.19f,.58f,.22f,.25f);p.Scarf=Vector4.zero;p.Lantern=Vector4.zero;p.Bag=Vector4.zero;p.Contour=true;p.Ground=.97f;return p;
            }
            if(name=="Mika"||name=="MikaThinking"){
                bool thinking=name=="MikaThinking";
                var p=Mouse(name,cell,thinking?CharacterPoseKind.Thinking:CharacterPoseKind.Lantern,
                    thinking?Area(.577f,.251f,.044f,.030f):Area(.544f,.207f,.042f,.030f),
                    thinking?Area(.736f,.209f,.027f,.030f):Area(.702f,.186f,.023f,.028f),
                    thinking?Area(.699f,.305f,.027f,.012f):Area(.675f,.264f,.028f,.011f),thinking?new Vector2(.64f,.35f):new Vector2(.61f,.32f));
                if(thinking){p.Tail=Area(.22f,.54f,.22f,.22f);p.TailRoot=new Vector2(.4f,.62f);p.Scarf=Area(.30f,.32f,.19f,.08f);p.LanternHook=new Vector2(.878f,.673f);p.Lantern=Area(.88f,.76f,.10f,.12f);}
                if((float)sprite.texture.width/sprite.texture.height>.69f){p.NearEye=Area(.538f,.183f,.050f,.033f);p.FarEye=Area(.718f,.158f,.023f,.028f);p.Mouth=Area(.658f,.253f,.034f,.016f);}
                return p;
            }
            if(name=="Pip"||name=="Rue"||name=="Aster"){
                var p=new CharacterPoseProfile{Name=name,Cell=cell,Kind=CharacterPoseKind.Speaker,Ground=1};
                if(name=="Pip"){p.NearEye=Area(.503f,.18f,.046f,.045f);p.FarEye=Area(.746f,.236f,.028f,.037f);p.Mouth=Area(.607f,.318f,.165f,.062f);p.Neck=new Vector2(.55f,.42f);}
                if(name=="Rue"){p.NearEye=Area(.525f,.345f,.037f,.026f);p.FarEye=Area(.696f,.316f,.023f,.025f);p.Mouth=Area(.647f,.468f,.092f,.055f);p.Neck=new Vector2(.58f,.55f);}
                if(name=="Aster"){p.NearEye=Area(.518f,.263f,.045f,.052f);p.FarEye=Area(.73f,.232f,.033f,.049f);p.Mouth=Area(.669f,.347f,.05f,.06f);p.Neck=new Vector2(.6f,.46f);p.Beak=true;}
                return p;
            }
            if(name=="Residents"){
                int column=Mathf.Clamp(Mathf.RoundToInt(sprite.rect.x/(sprite.texture.width/3f)),0,2),row=sprite.rect.y>sprite.texture.height*.25f?0:1;
                int index=row*3+column;cell=new Rect(column*sprite.texture.width/3f,row*sprite.texture.height/2f,sprite.texture.width/3f,sprite.texture.height/2f);
                var near=new[]{Area(.585f,.385f,.047f,.041f),Area(.585f,.315f,.047f,.044f),Area(.464f,.274f,.047f,.043f),Area(.477f,.258f,.045f,.041f),Area(.546f,.315f,.049f,.044f),Area(.561f,.324f,.045f,.039f)};
                var far=new[]{Area(.778f,.325f,.023f,.036f),Area(.796f,.256f,.012f,.034f),Area(.713f,.226f,.020f,.034f),Area(.683f,.207f,.022f,.035f),Area(.746f,.258f,.021f,.038f),Area(.751f,.279f,.025f,.037f)};
                var mouths=new[]{Area(.703f,.476f,.105f,.058f),Area(.740f,.366f,.079f,.05f),Area(.625f,.400f,.115f,.065f),Area(.615f,.375f,.09f,.055f),Area(.676f,.433f,.11f,.06f),Area(.679f,.425f,.095f,.055f)};
                return new CharacterPoseProfile{Name=new[]{"Bram","Sora","Neri","Toma","Lumi","Gran"}[index],Cell=cell,Kind=CharacterPoseKind.Speaker,NearEye=near[index],FarEye=far[index],Mouth=mouths[index],Neck=new Vector2(.60f,.55f),Ground=1,Beak=index==1};
            }
            return Costume(sprite,name);
        }
        void DetailLandmarks()
        {
            // These compact regions are local to each authored pose. Metal,
            // books, spectacles and planted feet are outside the soft details.
            NearEar=Area(NearEye.x-.14f,NearEye.y-.085f,.09f,.10f);
            FarEar=Area(FarEye.x+.065f,FarEye.y-.10f,.065f,.095f);
            if(Name=="RestingMika"){NearEar=Area(.31f,.24f,.095f,.09f);FarEar=Area(.68f,.14f,.065f,.10f);}
            if(Kind==CharacterPoseKind.Sad){NearEar=Area(.44f,.19f,.12f,.14f);FarEar=Area(.88f,.28f,.07f,.10f);EarGain=.7f;}
            if(Kind==CharacterPoseKind.Victory){NearEar=Area(.20f,.29f,.14f,.13f);FarEar=Area(.67f,.15f,.08f,.12f);}
            if(Kind==CharacterPoseKind.Speaker){
                SoftBrows=Name!="Aster"&&Name!="Toma"&&Name!="Gran";
                Whiskers=Name=="Rue"||Name=="Neri"||Name=="Toma"||Name=="Lumi"||Name=="Gran";
                NearEar=FarEar=Vector4.zero;SniffGain=Beak?.10f:.65f;
                if(Name=="Pip"){Accent=Area(.32f,.22f,.23f,.075f);AccentRoot=new Vector2(.50f,.22f);AccentGain=.25f;}
                if(Name=="Rue"){NearEar=Area(.29f,.20f,.10f,.17f);FarEar=Area(.73f,.13f,.08f,.17f);Accent=Area(.42f,.72f,.21f,.10f);AccentRoot=new Vector2(.56f,.57f);AccentGain=.35f;}
                if(Name=="Aster"){Accent=Area(.43f,.16f,.22f,.10f);AccentRoot=new Vector2(.59f,.29f);AccentGain=.6f;}
                if(Name=="Bram"){NearEar=Area(.34f,.34f,.055f,.065f);Accent=Area(.30f,.23f,.22f,.12f);AccentRoot=new Vector2(.52f,.38f);AccentGain=.35f;}
                if(Name=="Sora"){ScarfRoot=new Vector2(.55f,.56f);Scarf=Area(.25f,.63f,.25f,.14f);FabricGain=.5f;Accent=Area(.23f,.29f,.14f,.12f);AccentRoot=new Vector2(.40f,.35f);AccentGain=.55f;}
                if(Name=="Neri"){NearEar=Area(.24f,.34f,.05f,.06f);ScarfRoot=new Vector2(.59f,.62f);Scarf=Area(.32f,.68f,.24f,.13f);FabricGain=.6f;}
                if(Name=="Toma"){Accent=Area(.59f,.60f,.18f,.07f);AccentRoot=new Vector2(.58f,.55f);AccentGain=.2f;}
                if(Name=="Lumi"){NearEar=Area(.24f,.27f,.15f,.29f);FarEar=Area(.61f,.10f,.055f,.16f);EarGain=.45f;ScarfRoot=new Vector2(.57f,.57f);Scarf=Area(.71f,.75f,.12f,.21f);FabricGain=.55f;}
                if(Name=="Gran"){NearEar=Area(.27f,.27f,.12f,.16f);FarEar=Area(.75f,.18f,.065f,.09f);ScarfRoot=new Vector2(.55f,.65f);Scarf=Area(.28f,.80f,.22f,.17f);FabricGain=.35f;}
            }else{
                // A second visible garment detail moves independently from the
                // existing primary cloth/antenna/cape region of each skin.
                Accent=Area(Neck.x+.02f,Neck.y+.075f,.13f,.055f);AccentRoot=Neck;AccentGain=.35f;
                if(Name.StartsWith("BeeSuit",System.StringComparison.Ordinal)){Accent=Area(.69f,.39f,.09f,.075f);AccentRoot=new Vector2(.58f,.39f);AccentGain=.9f;}
                if(Name.StartsWith("JesterSuit",System.StringComparison.Ordinal)){Accent=Area(.26f,.17f,.10f,.07f);AccentRoot=new Vector2(.39f,.19f);AccentGain=.7f;}
                if(Name.StartsWith("SkyCourier",System.StringComparison.Ordinal)){Accent=Area(.25f,.35f,.17f,.08f);AccentRoot=new Vector2(.46f,.32f);AccentGain=.7f;}
                if(Name.StartsWith("WinterHood",System.StringComparison.Ordinal)){Accent=Area(.70f,.49f,.095f,.10f);AccentRoot=new Vector2(.57f,.39f);AccentGain=.6f;}
                if(Name.StartsWith("StarKnight",System.StringComparison.Ordinal)||Name.StartsWith("ConstellationMage",System.StringComparison.Ordinal)){Accent=Area(.25f,.51f,.12f,.20f);AccentRoot=new Vector2(.42f,.33f);AccentGain=.45f;}
                if(Name.StartsWith("MothMask",System.StringComparison.Ordinal)||Name.StartsWith("FrogSuit",System.StringComparison.Ordinal)||Name.StartsWith("FoxMask",System.StringComparison.Ordinal)){NearEar=FarEar=Vector4.zero;Whiskers=false;SoftBrows=false;}
                if(Kind==CharacterPoseKind.Victory)Accent=Vector4.zero;
            }
            NearEarRoot=new Vector2(NearEar.x+NearEar.z*.55f,NearEar.y+NearEar.w*.75f);
            FarEarRoot=new Vector2(FarEar.x-FarEar.z*.45f,FarEar.y+FarEar.w*.70f);
        }
        CharacterPoseProfile WithOpenMouth(){OpenMouth=true;return this;}
        static CharacterPoseProfile Costume(Sprite sprite,string name)
        {
            var names=new[]{"SkyCourier","MoonObserver","RainExplorer","SkyPilot","MothMask","GardenFestival","CloudGardener","ExplorerHat","FrogSuit","StarKnight","FoxMask","MushroomCap","BeeSuit","WinterHood","PirateHat","JesterSuit","ConstellationMage"};
            int i=System.Array.IndexOf(names,name);if(i<0)return null;
            bool thinking=sprite.rect.center.x>sprite.texture.width*.5f;
            var cell=new Rect(thinking?sprite.texture.width*.5f:0,0,sprite.texture.width*.5f,sprite.texture.height);
            if(costumeLandmarks==null)costumeLandmarks=JsonUtility.FromJson<LandmarkBook>(Resources.Load<TextAsset>("CascadeArt/CharacterMotionOct16/CostumeLandmarks").text);
            CostumeLandmark measured=null;foreach(var entry in costumeLandmarks.poses)if(entry.asset==name&&entry.pose==(thinking?1:0)){measured=entry;break;}
            if(measured==null)throw new System.InvalidOperationException("Missing costume animation landmarks: "+name);
            var p=Mouse(name+(thinking?" thinking":" hopeful"),cell,thinking?CharacterPoseKind.Thinking:CharacterPoseKind.Lantern,measured.near,measured.far,measured.mouth,new Vector2(.59f,Mathf.Max(measured.near.y,measured.far.y)+.115f));
            p.TailRoot=new Vector2(.32f,.62f);p.Tail=Area(.18f,.52f,.20f,.22f);p.Scarf=Area(.30f,.35f,.18f,.09f);p.LanternHook=new Vector2(.775f,.638f);p.Lantern=Area(.775f,.714f,.095f,.11f);
            p.LanternHook=measured.hook;p.Lantern=measured.lantern;
            // Use the visible fabric/accessory of this outfit, not an imaginary
            // yellow scarf crossing its mask or its face.
            if(i==1){p.ScarfRoot=new Vector2(.46f,.32f);p.Scarf=Area(.28f,.36f,.19f,.10f);p.FabricGain=.55f;}
            if(i==2){p.ScarfRoot=new Vector2(.53f,.56f);p.Scarf=Area(.63f,.66f,.09f,.09f);}
            if(i==3){p.ScarfRoot=new Vector2(.50f,.30f);p.Scarf=Area(.34f,.39f,.11f,.11f);}
            if(i==4){p.ScarfRoot=new Vector2(.22f,.24f);p.Scarf=Area(.18f,.34f,.075f,.10f);}
            if(i==5){p.ScarfRoot=new Vector2(.55f,.33f);p.Scarf=Area(.62f,.40f,.065f,.065f);}
            if(i==6||i==7){p.ScarfRoot=new Vector2(.30f,.19f);p.Scarf=Area(.20f,.25f,.10f,.07f);}
            if(i==8){p.ScarfRoot=new Vector2(.57f,.36f);p.Scarf=Area(.60f,.44f,.055f,.08f);}
            if(i==9){p.ScarfRoot=new Vector2(.37f,.35f);p.Scarf=Area(.14f,.53f,.11f,.17f);p.FabricGain=.35f;}
            if(i==10){p.ScarfRoot=new Vector2(.24f,.20f);p.Scarf=Area(.22f,.29f,.055f,.08f);}
            if(i==11){p.ScarfRoot=new Vector2(.63f,.39f);p.Scarf=Area(.75f,.42f,.10f,.055f);}
            if(i==12){p.ScarfRoot=new Vector2(.34f,.20f);p.Scarf=Area(.31f,.13f,.055f,.08f);}
            if(i==13){p.ScarfRoot=new Vector2(.54f,.40f);p.Scarf=Area(.60f,.51f,.07f,.11f);p.OpenMouth=!thinking;}
            if(i==14){p.ScarfRoot=new Vector2(.30f,.16f);p.Scarf=Area(.28f,.08f,.08f,.09f);}
            if(i==15){p.ScarfRoot=new Vector2(.66f,.14f);p.Scarf=Area(.74f,.12f,.08f,.075f);}
            if(i==16){p.ScarfRoot=new Vector2(.36f,.36f);p.Scarf=Area(.16f,.59f,.10f,.19f);p.FabricGain=.35f;}
            return p;
        }
    }
}
