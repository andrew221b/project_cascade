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
        readonly Vector3[] from = new Vector3[8], to = new Vector3[8], oldPositions = new Vector3[8];
        readonly SymbolId[] oldRow = new SymbolId[8];
        readonly bool[] used = new bool[8];
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
            Skip(); IsRunning = true; EventIndex = -1;
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
            game?.SettleOutcome();
            if (game != null && game.Interaction != null) { game.Interaction.FinishPresentation(); game.SyncPresentation(); }
        }
        IEnumerator Run(TransitionResult result)
        {
            var frame = game.VisibleFrame; var level = game.Interaction.Attempt.Level;
            try {
                for (int index = 0; index < result.Events.Count; index++) {
                    var e = result.Events[index]; EventIndex = index; CurrentEvent = e.Kind;
                    if (e.Kind == PuzzleEventKind.CommandAccepted) continue;
                    bool reduced = game.ReducedMotion;
                    if (e.Kind == PuzzleEventKind.RowPermuted || e.Kind == PuzzleEventKind.EntryPermuted) {
                        if(e.Kind==PuzzleEventKind.EntryPermuted&&game.IsWindFloor(e.Floor)) {
                            game.Machine.CueWind(level.Floors[e.Floor].EntryTransform==EntryTransform.ShiftRightOne);
                            game.SetPresentationFeedback(level.Floors[e.Floor].EntryTransform==EntryTransform.ShiftRightOne?"Wind slides the row right":"Wind slides the row left");
                        }
                        CaptureRow(frame); frame.Read(e, level); game.RenderFrame(frame); MapRow(frame);
                        feedback.Sound(ToyCue.Swap); yield return Travel(profile.swap, !reduced, false);
                    } else if (e.Kind == PuzzleEventKind.FloorExited) {
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
                                feedback.Burst(origin, new Color(1, .46f, .28f, .9f), 12, reduced); yield return Recoil(e.Floor, profile.bomb); break;
                            case PuzzleEventKind.BombDefused:
                                game.SetPresentationFeedback("Bomb defused"); feedback.Sound(ToyCue.Defuse);
                                feedback.Burst(origin, new Color(.4f, .88f, .73f, .8f), 4, reduced); yield return Hold(profile.contact); break;
                            case PuzzleEventKind.KeyOpened:
                                game.SetPresentationFeedback(new UiCopy("Edge {0} unlocked",e.Value+1)); feedback.Sound(ToyCue.Unlock); feedback.Haptic();
                                feedback.Burst(origin, new Color(1, .79f, .37f, .8f), 4, reduced); yield return Recoil(e.Floor, profile.contact); break;
                            case PuzzleEventKind.Repaired:
                                game.SetPresentationFeedback(new UiCopy("Repair +{0}",e.Value)); feedback.Sound(ToyCue.Repair);
                                feedback.Burst(origin, new Color(.4f, .88f, .73f, .8f), 5, reduced); yield return Recoil(e.Floor, profile.contact); break;
                            case PuzzleEventKind.RibbonCollected:
                            case PuzzleEventKind.HourglassCollected:
                                game.SetPresentationFeedback(e.Kind==PuzzleEventKind.RibbonCollected?"One far swap ready":"Bomb paused for two swaps");feedback.Sound(ToyCue.Stamp);feedback.Haptic();
                                feedback.Burst(origin,new Color(.5f,.8f,1,.85f),5,reduced);yield return Hold(profile.contact);break;
                            case PuzzleEventKind.BombFrozen:
                                game.SetPresentationFeedback("The bomb waits. The wood still wears down.");yield return Hold(.12f);break;
                            case PuzzleEventKind.StampCollected:
                                game.SetPresentationFeedback("Stamp collected"); feedback.Sound(ToyCue.Stamp);
                                feedback.Burst(origin, new Color(.74f, .61f, .86f, .8f), 4, reduced); yield return Hold(profile.contact); break;
                            case PuzzleEventKind.BridgeConsumed:
                                game.SetPresentationFeedback("One-use bridge closed"); feedback.Sound(ToyCue.Bridge); yield return Hold(profile.contact); break;
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
            for (int i = 0; i < frame.Row.Count; i++) { oldRow[i] = frame.Row[i]; oldPositions[i] = game.Machine.TokenVisualPosition(i); used[i] = false; }
        }
        void MapRow(PresentationFrame frame)
        {
            for (int destination = 0; destination < frame.Row.Count; destination++) {
                int source = 0; while (source < frame.Row.Count && (used[source] || oldRow[source] != frame.Row[destination])) source++;
                if (source == frame.Row.Count) source = destination; // Defensive view fallback; never invent a gameplay permutation.
                used[source] = true; from[destination] = oldPositions[source]; to[destination] = game.Machine.TokenPosition(destination)+game.Machine.TokenSupportOffset(destination);
                game.Machine.SetPiecePose(destination,PiecePose.Exchanging);
                game.OffsetPresentedToken(destination, game.ReducedMotion ? Vector3.zero : from[destination]-game.Machine.TokenPosition(destination), Vector2.one);
            }
        }
        IEnumerator Travel(float duration, bool arcs, bool falling)
        {
            if (game.ReducedMotion) { yield return Hold(profile.reducedStep); yield break; }
            float time = 0;int count=game.VisibleFrame.Row.Count;
            for(int i=0;i<count;i++)game.Machine.SetPiecePose(i,falling?PiecePose.Falling:PiecePose.Exchanging);
            while (time < duration) {
                float t = Mathf.Clamp01(time / duration), eased = 1-Mathf.Pow(1-t,3);
                for (int i = 0; i < game.VisibleFrame.Row.Count; i++) {
                    float local=falling?Mathf.Clamp01((t-(i%3)*.035f)/(1-(i%3)*.035f)):t;
                    Vector3 position = Vector3.Lerp(from[i], to[i], eased);
                    if(falling)position.y=Mathf.Lerp(from[i].y,to[i].y,local*local);
                    if (arcs && !game.ReducedMotion && Mathf.Abs(from[i].x-to[i].x) > .01f)
                        position.y += Mathf.Sin(t*Mathf.PI) * profile.swapArc * (from[i].x < to[i].x ? 1 : .35f);
                    game.OffsetPresentedToken(i, position-game.Machine.TokenPosition(i), Vector2.one);
                    game.Machine.SetPiecePose(i,falling?PiecePose.Falling:PiecePose.Exchanging,Mathf.Sin(local*Mathf.PI)*(i%2==0?1:-1)*(falling?13:9));
                }
                time += Time.unscaledDeltaTime; yield return null;
            }
            for (int i = 0; i < game.VisibleFrame.Row.Count; i++){game.OffsetPresentedToken(i, Vector3.zero, Vector2.one);game.Machine.SetPiecePose(i,falling?PiecePose.Settling:PiecePose.Resting);}
            if(arcs)game.Machine.LandWeight(game.VisibleFrame.ActiveFloor);
        }
        IEnumerator BreakWood(int floor)
        {
            if(game.ReducedMotion){feedback.WoodChips(game.Machine.FloorPosition(floor),2,true,game.Machine.FloorSpan(floor));yield return Hold(profile.reducedStep);yield break;}
            float time=0;const float duration=.34f;bool splintered=false;
            while(time<duration){
                float t=Mathf.Clamp01(time/duration);game.Machine.FractureFloor(floor,Mathf.SmoothStep(0,1,t));
                if(!splintered&&t>=.45f){splintered=true;feedback.WoodChips(game.Machine.FloorPosition(floor),14,false,game.Machine.FloorSpan(floor)*.76f);}
                time+=Time.unscaledDeltaTime;yield return null;
            }
        }
        IEnumerator Recoil(int floor, float duration)
        {
            if (game.ReducedMotion) { yield return Hold(profile.reducedStep); yield break; }
            float time = 0;
            while (time < duration) {
                game.Machine.RecoilFloor(floor, -Mathf.Sin(time/duration*Mathf.PI)*profile.platformRecoil);
                time += Time.unscaledDeltaTime; yield return null;
            }
            game.Machine.RecoilFloor(floor, 0);
        }
        IEnumerator Land(bool arrival)
        {
            game.Machine.LandWeight(game.VisibleFrame.ActiveFloor);
            float duration = game.ReducedMotion ? profile.reducedStep : Mathf.Max(.19f,profile.landing), time = 0;
            while (time < duration) {
                for (int i = 0; i < game.VisibleFrame.Row.Count; i++){
                    float t=Mathf.Clamp01(time/duration),pulse=game.ReducedMotion?0:Mathf.Cos(t*Mathf.PI*2)*Mathf.Exp(-t*7);
                    float squash=pulse*profile.landingSquash;
                    game.OffsetPresentedToken(i,Vector3.up*(game.ReducedMotion?0:Mathf.Sin(t*Mathf.PI)*Mathf.Exp(-t*5)*.09f),new Vector2(1+squash*.5f,1-squash));
                    game.Machine.SetPiecePose(i,PiecePose.Settling,pulse*(i%2==0?1:-1)*5);
                }
                time += Time.unscaledDeltaTime; yield return null;
            }
            game.RenderFrame(game.VisibleFrame, arrival);
        }
        IEnumerator Hold(float duration)
        {
            if (game.ReducedMotion) duration = profile.reducedStep;
            float time = 0; while (time < duration) { time += Time.unscaledDeltaTime; yield return null; }
        }
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
            for (int i = 0; i < game.SlotViews.Count; i++) {
                bool target = game.Interaction.SelectedSlot == i;
                if (target != selectionTarget[i]) {
                    selectionFrom[i] = selection[i]; selectionTarget[i] = target; selectionTime[i] = 0;
                    if (target) feedback.Sound(ToyCue.Select);
                }
                if (selectionTime[i] >= profile.selection) continue;
                selectionTime[i] = game.ReducedMotion ? profile.selection : Mathf.Min(profile.selection, selectionTime[i] + Time.unscaledDeltaTime);
                float t = selectionTime[i] / profile.selection, goal = target ? 1 : 0;
                selection[i] = Mathf.Lerp(selectionFrom[i], goal, 1-Mathf.Pow(1-t,3));
                float ratio = (1 + .04f * selection[i]) / (1 + .04f * goal);
                game.OffsetPresentedToken(i, Vector3.up * ((selection[i]-goal)*game.Machine.SelectionLiftWorld(i)), Vector2.one*ratio);
            }
        }
    }
}
