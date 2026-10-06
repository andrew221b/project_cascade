using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        Material touchGlowMaterial;
        MaterialPropertyBlock touchGlowProperties;
        int dragDestination=-1;
        bool blockedDestination;
        Material TouchGlowMaterial => touchGlowMaterial!=null?touchGlowMaterial:
            touchGlowMaterial=new Material(Resources.Load<Shader>("CascadeArt/ContourGlow")){name="Soft ivory piece contour"};
        public void SetDragDestination(int index,bool blocked=false)
        {
            dragDestination=index;blockedDestination=blocked;
            for(int i=0;i<tokens.Count;i++)UpdateTouchContour(i);
        }
        void UpdateTouchContour(int index)
        {
            if(index>=selections.Count)return;
            var halo=selections[index];var body=tokens[index].transform;
            bool source=interaction.SelectedSlot==index||heldWeight==index;
            bool destination=dragDestination==index&&!blockedDestination&&!source;
            bool visible=interaction.CanPlay&&(source||destination)
                &&tokens[index].transform.gameObject.activeInHierarchy;
            halo.transform.gameObject.SetActive(visible);
            if(!visible)return;
            if(touchGlowProperties==null)touchGlowProperties=new MaterialPropertyBlock();
            halo.transform.SetPositionAndRotation(body.position+Vector3.forward*.10f,body.rotation);
            float contourScale=destination?1.025f:1.075f;
            halo.transform.localScale=Vector3.Scale(body.localScale,new Vector3(contourScale,contourScale,.96f));
            touchGlowProperties.SetColor("_Color",new Color(1,.95f,.65f,destination?.10f:.46f));
            halo.renderer.SetPropertyBlock(touchGlowProperties);
        }
        public void ShakeSuspension(Vector2 impulse)
        {
            if(reducedMotion||!gameplayView||Frame==null||interaction.IsPaused||interaction.IsSuspended)return;
            impulse=Vector2.ClampMagnitude(impulse,1);
            for(int f=0;f<floors.Count;f++){
                var rig=floors[f];float response=1-f*.09f;
                rig.swayVelocity+=impulse.x*1.3f*response;rig.sagVelocity+=Mathf.Abs(impulse.y)*.18f*response;
                rig.rollVelocity-=impulse.x*(7+f*1.2f);
                for(int side=0;side<2;side++)ropeSpans[side,f]?.Impulse(impulse*(side==0?.7f:.56f)*response);
            }
            for(int i=0;i<tokens.Count;i++)rockVelocity[i]+=impulse.x*(12+i%3*2)/PieceMass(i);
        }
    }
}
