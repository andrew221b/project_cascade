using UnityEngine;
using UnityEngine.UI;

namespace Cascade.Presentation
{
    // The home sky uses the same anatomy, continuous wing poses and rear atlas as world rigs.
    internal sealed class WildlifeImageRig
    {
        public readonly RectTransform Root;
        public readonly Image Body,RearBody;
        readonly Image tail;
        readonly RectTransform[] wings=new RectTransform[2];
        readonly Image[] wingArt=new Image[2];
        readonly float[] bind=new float[2];
        WildlifeSpecies species;bool configured;
        public WildlifeImageRig(Transform parent,string name)
        {
            Root=Rect(name,parent);
            tail=Picture("Tail or hindwing",Root);
            wings[1]=Rect("Far wing joint",Root);wingArt[1]=Picture("Far painted wing",wings[1]);
            Body=Picture("Side painted body",Root);RearBody=Picture("Rear painted body",Root);
            wings[0]=Rect("Near wing joint",Root);wingArt[0]=Picture("Near painted wing",wings[0]);
            Hide();
        }
        static RectTransform Rect(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=Vector2.zero;return r;
        }
        static Image Picture(string name,Transform parent){var image=Rect(name,parent).gameObject.AddComponent<Image>();image.raycastTarget=false;return image;}
        public void Select(WildlifeSpecies kind)
        {
            if(configured&&species==kind)return;species=kind;configured=true;
            var torso=RegionalWildlifeArt.CellFor(kind,0);Body.sprite=RegionalWildlifeArt.Part(kind);
            Size(Body.rectTransform,torso,1);RearBody.sprite=WildlifeRearArt.Get(kind);
            if(WildlifeFlightArt.Has(kind)){
                Body.sprite=WildlifeFlightArt.Get(kind);Body.rectTransform.pivot=Vector2.one*.5f;
                Body.rectTransform.sizeDelta=new Vector2(1,Body.sprite.rect.height/Body.sprite.rect.width);
            }
            var rear=WildlifeRearArt.Pixels(kind);RearBody.rectTransform.sizeDelta=new Vector2(rear.width/rear.height,1);
            var extra=RegionalWildlifeArt.CellFor(kind,3);tail.sprite=RegionalWildlifeArt.Part(kind,3);
            bool bird=WildlifeMotion.IsBird(kind),dragon=kind==WildlifeSpecies.Dragonfly||kind==WildlifeSpecies.Damselfly;
            Size(tail.rectTransform,extra,bird?(kind==WildlifeSpecies.Swallow||kind==WildlifeSpecies.Tern?1.08f:.65f):dragon?.78f:1.13f);
            for(int i=0;i<2;i++){
                var cell=RegionalWildlifeArt.CellFor(kind,i+1);wingArt[i].sprite=RegionalWildlifeArt.Part(kind,i+1);bind[i]=cell.BindAngle;
                float span=WildlifeMotion.IsBird(kind)?i==0?1.26f:1.09f:kind==WildlifeSpecies.Dragonfly||kind==WildlifeSpecies.Damselfly?.94f:1.13f;
                Size(wingArt[i].rectTransform,cell,span);
            }
        }
        static void Size(RectTransform rect,WildlifeRigAtlas.Cell cell,float width)
        {
            rect.pivot=new Vector2((cell.Pivot.x-cell.Pixels.x)/cell.Pixels.width,1-(cell.Pivot.y-cell.Pixels.y)/cell.Pixels.height);
            rect.sizeDelta=new Vector2(width,width*cell.Pixels.height/cell.Pixels.width);
        }
        public void Paint(Vector2 point,float bodySize,Vector3 heading,float bank,WildlifePose pose,Color tint,float rear)
        {
            Root.gameObject.SetActive(true);Root.anchoredPosition=point;Root.localScale=Vector3.one*bodySize;Root.localRotation=Quaternion.Euler(0,0,bank);
            float facing=heading.x>=0?1:-1,angle=Mathf.Atan2(heading.y,heading.x)*Mathf.Rad2Deg-bank;
            float profile=angle-(facing<0?180:0),wingHeading=Mathf.LerpAngle(profile,angle-90,rear);
            Body.rectTransform.localScale=new Vector3(facing,1,1);Body.rectTransform.localRotation=Quaternion.Euler(0,0,profile);
            RearBody.rectTransform.localRotation=Quaternion.Euler(0,0,wingHeading);
            var front=tint;front.a*=1-rear;var back=tint;back.a*=rear;Body.color=front;RearBody.color=back;
            Body.enabled=front.a>.001f;RearBody.enabled=back.a>.001f;
            bool bird=WildlifeMotion.IsBird(species),moth=WildlifeMotion.Style(species)==WildlifeFlightStyle.Moth;
            tail.enabled=front.a>.001f&&!WildlifeRearArt.IsRegional(species)&&!moth&&species!=WildlifeSpecies.Butterfly;
            tail.color=front;
            tail.rectTransform.anchoredPosition=(Vector2)(Quaternion.Euler(0,0,profile)*(bird?new Vector3(-.38f*facing,-.17f,0):new Vector3(-.055f*facing,-.025f,0)));
            tail.rectTransform.localScale=new Vector3(facing,1,1);
            tail.rectTransform.localRotation=Quaternion.Euler(0,0,profile+(bird?pose.Tail:pose.NearWing*.78f+6)*facing);
            for(int i=0;i<2;i++){
                float wing=i==0?pose.NearWing:pose.FarWing,dorsal=(i==0?1:-1)*(wing*.38f+8+bind[i]);
                wings[i].anchoredPosition=Vector2.Lerp(new Vector2(0,.035f),new Vector2(i==0?-.09f:.09f,.10f),rear);
                wings[i].localScale=new Vector3(Mathf.Lerp(facing,i==0?1:-1,rear),1,1);
                wings[i].localRotation=Quaternion.Euler(0,pose.WingFold*12,wingHeading+Mathf.LerpAngle(wing+bind[i],dorsal,rear));
                var color=tint;color.a*=i==0?1:.84f;wingArt[i].color=color;
                wingArt[i].enabled=species!=WildlifeSpecies.FieldCricket;
            }
        }
        public void Hide(){Body.enabled=RearBody.enabled=tail.enabled=false;foreach(var image in wingArt)image.enabled=false;}
    }
}
