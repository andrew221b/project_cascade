using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class ToyMachineView
    {
        readonly float[] contactLoads=new float[8],contactLoadVelocity=new float[8],contactTargets=new float[8];
        readonly float[] contactSlide=new float[8],contactSlideVelocity=new float[8],contactLean=new float[8],contactLeanVelocity=new float[8];
        int contactFloor=-1;
        float centreLoad,centreFullLoad,loadCentre=.5f;
        bool PieceLifted(int index)=>heldWeight==index||!interaction.IsPresenting&&interaction.SelectedSlot==index;
        Vector3 ContactVector(int index)=>Vector3.Scale(tokens[index].mesh.bounds.center-Vector3.up*tokens[index].mesh.bounds.extents.y,restScales[index]);
        float ContactX(int index,int floor)
            =>Mathf.Clamp01((restPositions[index].x+ContactVector(index).x+contactSlide[index]-floors[floor].board.basePosition.x)/Mathf.Max(.01f,floors[floor].board.baseScale.x)+.5f);
        // Simply supported elastic beam response to a point load. Both rope ends remain pinned.
        static float PointLoadBend(float x,float at)
        {
            return x<=at?8*(1-at)*x*(1-(1-at)*(1-at)-x*x)
                :8*at*(1-x)*(1-at*at-(1-x)*(1-x));
        }
        float LoadResponse(float x,int floor,bool full=false)
        {
            float sum=0;
            for(int i=0;i<tokens.Count;i++)sum+=PieceMass(i)*(full?1:contactLoads[i])*PointLoadBend(x,ContactX(i,floor));
            return sum;
        }
        void ResetContactLoads()
        {
            contactFloor=-1;
            for(int i=0;i<8;i++)contactLoads[i]=contactLoadVelocity[i]=contactTargets[i]=contactSlide[i]=contactSlideVelocity[i]=contactLean[i]=contactLeanVelocity[i]=0;
        }
        void TickContactLoads(float seconds)
        {
            int floor=Frame.Status==PuzzleStatus.Playing?Frame.ActiveFloor:-1;
            bool entered=floor!=contactFloor;contactFloor=floor;
            float mass=0,moment=0;
            for(int i=0;i<tokens.Count;i++){
                float target=floor<0||PieceLifted(i)||piecePoses[i]==PiecePose.Falling||piecePoses[i]==PiecePose.Exchanging?0:
                    piecePoses[i]==PiecePose.Settling?Mathf.Clamp01(1-Mathf.Max(0,visualOffsets[i].y)/.065f):1;
                if(!entered&&seconds>0&&floor>=0&&Mathf.Abs(target-contactTargets[i])>.001f)
                    ContactImpulse(floor,i,(target-contactTargets[i])*.48f,0);
                contactTargets[i]=target;
                if(entered||reducedMotion){contactLoads[i]=target;contactLoadVelocity[i]=0;}
                else Spring(ref contactLoads[i],ref contactLoadVelocity[i],target,150,18,seconds);
                contactLoads[i]=Mathf.Clamp01(contactLoads[i]);
                if(floor>=0){float weight=PieceMass(i)*contactLoads[i];mass+=weight;moment+=weight*ContactX(i,floor);}
            }
            loadCentre=mass>.001f?moment/mass:.5f;
            centreLoad=floor>=0?LoadResponse(.5f,floor):0;
            centreFullLoad=floor>=0?LoadResponse(.5f,floor,true):1;
        }
        float BendAt(int floor,float x)
        {
            float sample=Mathf.Clamp01(x)*16;int left=Mathf.Min(15,(int)sample);
            return Mathf.Lerp(floors[floor].deflection[left],floors[floor].deflection[left+1],sample-left);
        }
        float SurfaceAngle(int floor,float x)
        {
            float left=Mathf.Max(0,x-.035f),right=Mathf.Min(1,x+.035f);
            return floors[floor].roll-Mathf.Atan2(BendAt(floor,right)-BendAt(floor,left),(right-left)*floors[floor].board.baseScale.x)*Mathf.Rad2Deg;
        }
        void TickSeat(int index,float seconds)
        {
            bool seated=Frame.Status==PuzzleStatus.Playing&&!PieceLifted(index)&&piecePoses[index]!=PiecePose.Falling&&piecePoses[index]!=PiecePose.Exchanging;
            float lean=0,slide=0;
            if(seated){
                int floor=Frame.ActiveFloor;float angle=SurfaceAngle(floor,ContactX(index,floor));
                lean=Mathf.Clamp(angle-floors[floor].roll,-10,10);
                float width=TokenBodySize(index).x;
                slide=Mathf.Clamp(-Mathf.Sin(angle*Mathf.Deg2Rad)*width*.38f,-width*.08f,width*.08f);
            }
            if(reducedMotion){contactLean[index]=lean;contactSlide[index]=contactLeanVelocity[index]=contactSlideVelocity[index]=0;}
            else {
                Spring(ref contactLean[index],ref contactLeanVelocity[index],lean,85,13,seconds);
                Spring(ref contactSlide[index],ref contactSlideVelocity[index],slide,42,9,seconds);
            }
        }
        Vector3 SupportedTokenOffset(int index)
        {
            if(Frame.Status!=PuzzleStatus.Playing)return ReceiverPointOffset(restPositions[index]);
            if(PieceLifted(index))return Vector3.zero;
            int floor=Frame.ActiveFloor;var foot=ContactVector(index);
            var point=restPositions[index]+foot+Vector3.right*contactSlide[index];
            var rotation=Quaternion.Euler(0,0,floors[floor].roll+contactLean[index]);
            return point+FloorPointOffset(floor,point,true)-rotation*foot-restPositions[index];
        }
    }
}
