using System.Collections;
using Cascade.Domain;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed class PuzzleEventPlayer : MonoBehaviour
    {
        CascadeGame game;
        MotionProfile profile;
        ToyFeedbackPool feedback;
        Coroutine playback;
        bool releaseCaptured,releasedExchange;
        readonly Vector3[] from = new Vector3[8], to = new Vector3[8], oldPositions = new Vector3[8];
        readonly Vector3[] oldScales = new Vector3[8];
        readonly Vector2[] fromStretch = new Vector2[8];
        readonly float[] oldAngles = new float[8], fromAngles = new float[8];
        readonly SymbolId[] oldRow = new SymbolId[8];
        readonly bool[] used = new bool[8];
        readonly int[] sourceSlots=new int[8];
        readonly float[] selection = new float[8], selectionFrom = new float[8], selectionTime = new float[8];
        readonly bool[] selectionTarget = new bool[8];
        public bool IsRunning { get; private set; }
        public int EventIndex { get; private set; } = -1;
        public PuzzleEventKind CurrentEvent { get; private set; }
        public int CompletedPresentations { get; private set; }
        public void Initialize(CascadeGame owner, MotionProfile motion, ToyFeedbackPool pool) { game = owner; profile = motion; feedback = pool; }
        public void Play(TransitionResult result)
        {
            if (!result.Accepted) return;
            Skip(); IsRunning = true; EventIndex = -1;releaseCaptured=false;
            for (int i = 0; i < selection.Length; i++) { selection[i] = selectionFrom[i] = 0; selectionTarget[i] = false; selectionTime[i] = profile.selection; }
            playback = StartCoroutine(Run(result));
        }
        public void Skip()
        {
            if (!IsRunning) return;
            if (playback != null) StopCoroutine(playback); playback = null; Finish();
        }
        void OnDisable() { Skip(); }
        void Finish()
        {
            if (!IsRunning) return; IsRunning = false; playback = null; CompletedPresentations++;
            feedback?.RowPhysics?.End();
            game?.CompleteTouchExchange();
            if(game!=null&&game.CanPresent){game.Machine.PrepareWind(true,0);game.ReactionBanner.Clear();game.SettleOutcome();}
            if (game != null && game.Interaction != null) { game.Interaction.FinishPresentation(); if(game.CanPresent)game.SyncPresentation(); }
        }
        IEnumerator Run(TransitionResult result)
        {
            var frame = game.VisibleFrame; var level = game.Interaction.Attempt.Level;
            try {
                for (int index = 0; index < result.Events.Count; index++) {
                    while(PresentationPaused)yield return null;
                    var e = result.Events[index]; EventIndex = index; CurrentEvent = e.Kind;
                    if (e.Kind == PuzzleEventKind.CommandAccepted) continue;
                    bool reduced = game.ReducedMotion;
                    if (e.Kind == PuzzleEventKind.RowPermuted || e.Kind == PuzzleEventKind.EntryPermuted) {
                        if(e.Kind==PuzzleEventKind.EntryPermuted&&game.IsWindFloor(e.Floor)) {
                            game.Machine.CueWind(level.Floors[e.Floor].EntryTransform==EntryTransform.ShiftRightOne);
                            game.CueDeviceWind(level.Floors[e.Floor].EntryTransform==EntryTransform.ShiftRightOne);
                            game.SetPresentationFeedback("The wind moved the row");
                            game.AnnounceReaction("The wind moved the row","");
                        }
                        CaptureRow(frame); frame.Read(e, level); game.RenderFrame(frame); MapRow(frame);
                        feedback.Sound(ToyCue.Swap); yield return Travel(profile.swap, !reduced, false);
                    } else if (e.Kind == PuzzleEventKind.FloorExited) {
                        int next=e.Floor+1;if(next<level.Floors.Count&&game.IsWindFloor(next))yield return WarnWind(level.Floors[next].EntryTransform==EntryTransform.ShiftRightOne);
                        if(frame.Durability<=0){
                            feedback.Sound(ToyCue.Fragile);yield return BreakWood(e.Floor);
                        }else yield return Recoil(e.Floor, profile.fracture);
                        frame.Read(e, level); game.RenderFrame(frame);
                    } else if (e.Kind == PuzzleEventKind.FloorEntered || e.Kind == PuzzleEventKind.ReceiverEvaluated) {
                        CaptureRow(frame); frame.Read(e, level); bool arrival = e.Kind == PuzzleEventKind.ReceiverEvaluated;
                        game.RenderFrame(frame, arrival); MapRow(frame);
                        yield return Travel(profile.fall, false, true); feedback.Sound(ToyCue.Land);
                        yield return Land(arrival);
                        if (arrival) {
                            feedback.Sound(e.Value == 1 ? ToyCue.Success : ToyCue.Reject);
                            yield return game.PlayOutcome(e.Value==1);
                            game.CompleteDeliveryPose();
                        }
                    } else {
                        Vector3 origin = EventOrigin(e, level); frame.Read(e, level); game.RenderFrame(frame);
                        game.CueDevice(e);
                        switch (e.Kind) {
                            case PuzzleEventKind.BombTicked:
                                game.SetPresentationFeedback(new UiCopy("Bomb: {0} moves left",e.Value)); yield return Hold(.07f); break;
                            case PuzzleEventKind.BombExploded:
                                game.SetPresentationFeedback(new UiCopy("Blast: −{0} durability",e.Value)); feedback.Sound(ToyCue.Bomb); feedback.Haptic();
                                game.AnnounceReaction("Watch the blast!","Bomb");
                                yield return Recoil(e.Floor, Mathf.Max(.30f,profile.bomb));yield return HoldReaction(); break;
                            case PuzzleEventKind.BombDefused:
                                game.SetPresentationFeedback("Bomb defused"); feedback.Sound(ToyCue.Defuse);
                                game.AnnounceReaction("Bomb defused","BombSafe");yield return HoldReaction();break;
                            case PuzzleEventKind.KeyOpened:
                                game.SetPresentationFeedback(new UiCopy("Edge {0} unlocked",e.Value+1)); feedback.Sound(ToyCue.Unlock); feedback.Haptic();
                                game.AnnounceReaction("Gate open!","BrassKey");yield return Recoil(e.Floor,profile.contact);yield return HoldReaction(); break;
                            case PuzzleEventKind.Repaired:
                                game.SetPresentationFeedback(new UiCopy("Repair +{0}",e.Value)); feedback.Sound(ToyCue.Repair);
                                game.AnnounceReaction("Board mended!","RepairKit");yield return Recoil(e.Floor,profile.contact);yield return HoldReaction(); break;
                            case PuzzleEventKind.RibbonCollected:
                            case PuzzleEventKind.HourglassCollected:
                                game.SetPresentationFeedback(e.Kind==PuzzleEventKind.RibbonCollected?"One far swap ready":"Bomb paused for two swaps");feedback.Sound(ToyCue.Stamp);feedback.Haptic();
                                bool ribbon=e.Kind==PuzzleEventKind.RibbonCollected;int tool=ribbon?9:10;
                                game.AnnounceReaction(ribbon?"Ribbon ready!":"Time paused!",ribbon?"RibbonSpool":"Hourglass");
                                yield return HoldReaction();break;
                            case PuzzleEventKind.BombFrozen:
                                game.SetPresentationFeedback("The bomb waits. The wood still wears down.");yield return Hold(.12f);break;
                            case PuzzleEventKind.StampCollected:
                                game.SetPresentationFeedback("Stamp collected"); feedback.Sound(ToyCue.Stamp);
                                game.AnnounceReaction("Stamp yours!","Stamp");yield return HoldReaction();break;
                            case PuzzleEventKind.BridgeConsumed:
                                game.SetPresentationFeedback("One-use bridge closed");game.AnnounceReaction("One-use bridge closed","");feedback.Sound(ToyCue.Bridge);yield return Hold(profile.contact);break;
                            case PuzzleEventKind.DurabilityResolved:
                                if (result.DurabilityCost > 1) { feedback.Sound(ToyCue.Fragile); yield return Recoil(e.Floor, profile.fracture); }
                                break;
                        }
                    }
                }
            }
            finally { Finish(); }
        }
        void CaptureRow(PresentationFrame frame)
        {
            releasedExchange=!releaseCaptured&&game.ReleasedDrag;
            for (int i = 0; i < frame.Row.Count; i++) { oldRow[i] = frame.Row[i]; oldPositions[i] = game.Machine.TokenVisualPosition(i); oldScales[i] = game.Machine.TokenVisualScale(i); oldAngles[i] = Mathf.DeltaAngle(0,game.Machine.TokenVisualAngle(i)); used[i] = false; }
            if(!releaseCaptured&&game.ReleasedDrag&&game.ReleasedPartnerSlot>=0)oldPositions[game.ReleasedPartnerSlot]=game.ReleasedPartnerPosition;
            releaseCaptured=true;
        }
        void MapRow(PresentationFrame frame)
        {
            // Reserve unchanged occurrences first: a held duplicate must not be replaced
            // by an identical body that stayed in another slot.
            for(int i=0;i<frame.Row.Count;i++)used[i]=oldRow[i]==frame.Row[i];
            for (int destination = 0; destination < frame.Row.Count; destination++) {
                int source = destination;
                if(oldRow[destination]!=frame.Row[destination]) {
                    source=0;while(source<frame.Row.Count && (used[source] || oldRow[source]!=frame.Row[destination]))source++;
                }
                if (source == frame.Row.Count) source = destination; // Defensive view fallback; never invent a gameplay permutation.
                sourceSlots[destination]=source;
                used[source] = true; from[destination] = oldPositions[source]; to[destination] = game.Machine.TokenPosition(destination)+game.Machine.TokenSupportOffset(destination);
                var basis=game.Machine.TokenBaseScale(destination);
                fromStretch[destination]=new Vector2(oldScales[source].x/basis.x,oldScales[source].y/basis.y);fromAngles[destination]=oldAngles[source];
                game.Machine.SetPiecePose(destination,PiecePose.Exchanging,game.ReducedMotion?0:fromAngles[destination]);
                game.OffsetPresentedToken(destination, game.ReducedMotion ? Vector3.zero : from[destination]-game.Machine.TokenPosition(destination), game.ReducedMotion?Vector2.one:fromStretch[destination]);
            }
        }
        IEnumerator Travel(float duration, bool arcs, bool falling)
        {
            if (game.ReducedMotion) { yield return Hold(profile.reducedStep); yield break; }
            if(falling){yield return PhysicalFall();yield break;}
            duration=Mathf.Max(duration,releasedExchange?.36f:.32f);
            float time = 0;int count=game.VisibleFrame.Row.Count;
            if(falling){float drop=0;for(int i=0;i<count;i++)drop=Mathf.Max(drop,from[i].y-to[i].y);duration=Mathf.Max(duration,Mathf.Sqrt(2*drop/9.81f)+.07f);}
            else if(arcs)for(int i=0;i<count;i++)if(Mathf.Abs(from[i].x-to[i].x)>.01f)game.Machine.ContactImpulse(game.VisibleFrame.ActiveFloor,i,-.16f,Mathf.Sign(to[i].x-from[i].x));
            for(int i=0;i<count;i++)game.Machine.SetPiecePose(i,falling?PiecePose.Falling:PiecePose.Exchanging);
            while (time < duration) {
                if(PresentationPaused){yield return null;continue;}
                float t = Mathf.Clamp01(time / duration), eased = 1-Mathf.Pow(1-t,3);
                for (int i = 0; i < game.VisibleFrame.Row.Count; i++) {
                    float local=falling?Mathf.Clamp01((t-(i%3)*.035f)/(1-(i%3)*.035f)):t;
                    Vector3 position = Vector3.Lerp(from[i], to[i], eased);
                    if(falling){
                        float flight=Mathf.Max(0,time-(i%3)*.025f);
                        position.y=Mathf.Max(to[i].y,from[i].y-4.905f*flight*flight);
                        position.x+=game.Machine.WindAt(Mathf.Min(game.VisibleFrame.ActiveFloor,game.Interaction.Attempt.Level.Floors.Count-1))*.055f*Mathf.Sin(local*Mathf.PI);
                    }
                    if (arcs && Mathf.Abs(from[i].x-to[i].x) > .01f){
                        bool placed=releasedExchange&&sourceSlots[i]==game.ReleasedTokenSlot;
                        float route=placed?t:Mathf.Clamp01((t-.08f)/.92f);
                        float smooth=route*route*(3-2*route);
                        position=Vector3.Lerp(from[i],to[i],smooth);
                        if(!placed)position.y+=Mathf.Sin(route*Mathf.PI)*Mathf.Max(profile.swapArc,.32f);
                        position.z=Mathf.Lerp(from[i].z,placed?-.52f:-.8f,Mathf.Sin(t*Mathf.PI));
                    }
                    float angle=Mathf.Sin(local*Mathf.PI)*(i%2==0?1:-1)*(falling?13:9);
                    if(falling)angle*=Mathf.SmoothStep(0,1,Mathf.Clamp01((position.y-to[i].y)/.3f));
                    game.OffsetPresentedToken(i, position-game.Machine.TokenPosition(i), Vector2.Lerp(fromStretch[i],Vector2.one,eased));
                    game.Machine.SetPiecePose(i,falling?PiecePose.Falling:PiecePose.Exchanging,Mathf.Lerp(fromAngles[i],0,eased)+angle);
                }
                time += Time.unscaledDeltaTime; yield return null;
            }
            while(PresentationPaused)yield return null;
            for (int i = 0; i < game.VisibleFrame.Row.Count; i++){game.OffsetPresentedToken(i, Vector3.zero, Vector2.one);game.Machine.SetPiecePose(i,falling?PiecePose.Settling:PiecePose.Resting);}
            for(int i=0;i<count;i++)if(falling||arcs&&Mathf.Abs(from[i].x-to[i].x)>.01f)
                game.Machine.ContactImpulse(game.VisibleFrame.ActiveFloor,i,falling?Mathf.Clamp(duration*.7f,.3f,.8f)/count:.42f,falling?0:Mathf.Sign(to[i].x-from[i].x)*.45f);
        }
        IEnumerator PhysicalFall()
        {
            int count=game.VisibleFrame.Row.Count;float drop=0;
            for(int i=0;i<count;i++)drop=Mathf.Max(drop,from[i].y-to[i].y);
            float flight=Mathf.Sqrt(2*Mathf.Max(0,drop)/9.81f);
            var bodies=feedback.RowPhysics;bodies.Begin(from,to,count);
            float time=0,settleAt=flight+.44f,duration=settleAt+.42f;
            try{
                while(time<duration){
                    if(PresentationPaused){yield return null;continue;}
                    if(game.ReducedMotion)break;
                    float seat=Mathf.SmoothStep(0,1,Mathf.InverseLerp(settleAt,duration,time));
                    for(int i=0;i<count;i++){
                        var position=Vector3.Lerp(bodies.Position(i),to[i],seat);
                        float angle=Mathf.LerpAngle(bodies.Angle(i),0,seat);
                        game.Machine.SetPiecePose(i,PiecePose.Falling,angle);
                        game.OffsetPresentedToken(i,position-game.Machine.TokenPosition(i),Vector2.one);
                    }
                    time=bodies.Elapsed;yield return null;
                }
            }finally{bodies.End();}
            for(int i=0;i<count;i++){
                game.Machine.SetPiecePose(i,PiecePose.Settling);
                game.OffsetPresentedToken(i,Vector3.zero,Vector2.one);
            }
        }
        IEnumerator BreakWood(int floor)
        {
            game.AnnounceReaction("Wood gives way!","");
            if(game.ReducedMotion){feedback.WoodSnap();feedback.WoodChips(game.Machine.FloorPosition(floor),2,true,game.Machine.FloorSpan(floor));yield return Hold(profile.reducedStep);yield break;}
            float time=0;const float duration=1.02f;bool splintered=false,tail=false;
            while(time<duration){
                if(PresentationPaused){yield return null;continue;}
                float t=Mathf.Clamp01(time/duration);
                float fracture=t<.32f?.24f*Mathf.SmoothStep(0,1,t/.32f):Mathf.Lerp(.24f,1,Mathf.SmoothStep(0,1,(t-.32f)/.68f));
                game.Machine.FractureFloor(floor,fracture);
                if(!splintered&&t>=.40f){splintered=true;feedback.WoodSnap();feedback.Haptic();game.Machine.ContactImpulse(floor,game.Machine.TokenCount/2,.65f,0);feedback.WoodChips(game.Machine.FloorPosition(floor),14,false,game.Machine.FloorSpan(floor)*.96f);}
                if(!tail&&t>=.64f){tail=true;feedback.WoodChips(game.Machine.FloorPosition(floor),8,false,game.Machine.FloorSpan(floor)*.92f);}
                time+=Time.unscaledDeltaTime;yield return null;
            }
            while(PresentationPaused)yield return null;
        }
        IEnumerator Recoil(int floor, float duration)
        {
            if (game.ReducedMotion) { yield return Hold(profile.reducedStep); yield break; }
            float time = 0;
            while (time < duration) {
                if(PresentationPaused){yield return null;continue;}
                game.Machine.RecoilFloor(floor, -Mathf.Sin(time/duration*Mathf.PI)*profile.platformRecoil);
                time += Time.unscaledDeltaTime; yield return null;
            }
            while(PresentationPaused)yield return null;
            game.Machine.RecoilFloor(floor, 0);
        }
        IEnumerator Land(bool arrival)
        {
            float duration = game.ReducedMotion ? profile.reducedStep : Mathf.Max(.19f,profile.landing), time = 0;
            while (time < duration) {
                if(PresentationPaused){yield return null;continue;}
                for (int i = 0; i < game.VisibleFrame.Row.Count; i++){
                    float t=Mathf.Clamp01(time/duration),pulse=game.ReducedMotion?0:Mathf.Cos(t*Mathf.PI*2)*Mathf.Exp(-t*7);
                    float squash=pulse*profile.landingSquash;
                    game.OffsetPresentedToken(i,Vector3.up*(game.ReducedMotion?0:Mathf.Sin(t*Mathf.PI)*Mathf.Exp(-t*5)*.09f),new Vector2(1+squash*.5f,1-squash));
                    game.Machine.SetPiecePose(i,PiecePose.Settling,pulse*(i%2==0?1:-1)*5);
                }
                time += Time.unscaledDeltaTime; yield return null;
            }
            while(PresentationPaused)yield return null;
            game.RenderFrame(game.VisibleFrame, arrival);
        }
        IEnumerator WarnWind(bool right)
        {
            game.ReactionBanner.ShowWind(right);
            while(game.ReactionBanner.IsShowing){
                if(!PresentationPaused)game.Machine.PrepareWind(right,Mathf.SmoothStep(0,1,game.ReactionBanner.Elapsed/.85f));
                yield return null;
            }
            while(PresentationPaused)yield return null;
        }
        IEnumerator HoldReaction()
        {
            while(game.ReactionBanner!=null&&game.ReactionBanner.IsShowing)yield return null;
        }
        IEnumerator Hold(float duration)
        {
            if (game.ReducedMotion) duration = profile.reducedStep;
            float time = 0; while (time < duration) { if(!PresentationPaused)time += Time.unscaledDeltaTime; yield return null; }
            while(PresentationPaused)yield return null;
        }
        bool PresentationPaused => game.Interaction==null||game.Interaction.IsPaused||game.Interaction.IsSuspended;
        Vector3 EventOrigin(PuzzleEvent e, LevelDefinition level)
        {
            if (e.Floor < 0 || e.Floor >= level.Floors.Count) return game.Machine.TokenPosition(0);
            var floor = level.Floors[e.Floor]; ContactDefinition contact = null;
            if (e.Kind == PuzzleEventKind.BombDefused || e.Kind == PuzzleEventKind.BombExploded || e.Kind == PuzzleEventKind.BombTicked) contact = floor.Bomb;
            else if (e.Kind == PuzzleEventKind.KeyOpened) contact = floor.Key;
            else if (e.Kind == PuzzleEventKind.Repaired) contact = floor.Repair;
            else if (e.Kind == PuzzleEventKind.StampCollected) contact = floor.OptionalStamp;
            else if(e.Kind==PuzzleEventKind.RibbonCollected||e.Kind==PuzzleEventKind.HourglassCollected)contact=floor.Powerup;
            else if(e.Kind==PuzzleEventKind.BombFrozen)contact=floor.Bomb;
            return contact == null ? game.Machine.FloorPosition(e.Floor) : game.Machine.ContactPosition(e.Floor, contact.Slot);
        }
        void LateUpdate()
        {
            if (IsRunning || game == null || game.Interaction == null || !game.enabled || game.Interaction.IsSuspended || game.Interaction.IsPaused) return;
            game.AdvanceTokenTouch(Time.unscaledDeltaTime);
            for (int i = 0; i < game.SlotViews.Count; i++) {
                if(game.OwnsTokenPose(i)) { selection[i]=selectionFrom[i]=game.Interaction.SelectedSlot==i?1:0;selectionTarget[i]=game.Interaction.SelectedSlot==i;selectionTime[i]=profile.selection;continue; }
                bool target = game.Interaction.SelectedSlot == i;
                if (target != selectionTarget[i]) {
                    selectionFrom[i] = selection[i]; selectionTarget[i] = target; selectionTime[i] = 0;
                    if (target) feedback.Sound(ToyCue.Select);
                }
                selectionTime[i] = game.ReducedMotion ? profile.selection : Mathf.Min(profile.selection, selectionTime[i] + Time.unscaledDeltaTime);
                float t = selectionTime[i] / profile.selection, goal = target ? 1 : 0;
                selection[i] = Mathf.Lerp(selectionFrom[i], goal, 1-Mathf.Pow(1-t,3));
                float ratio = 1 + .04f * selection[i];
                game.OffsetPresentedToken(i, Vector3.up * (selection[i]*game.Machine.SelectionLiftWorld(i)), Vector2.one*ratio);
            }
        }
    }
}
