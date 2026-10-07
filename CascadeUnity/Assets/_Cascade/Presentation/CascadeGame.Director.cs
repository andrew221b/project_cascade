using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cascade.Application;
using Cascade.Domain;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        Task<DirectionRecipe> directionTask;
        CancellationTokenSource directionCancellation;
        LevelDefinition directionSource;
        string directionLocale;
        DeliveryMood directionMood;
        DirectionRecipe directionPrevious;
        PuzzleGoal directionGoal;
        public bool IsPlanningDirection=>directionSource!=null;
        public DirectorMemory Director=>profile.Director;
        public bool IsWindFloor(int floor)=>descent!=null?(floor==0&&(descent.Stages[descent.StageIndex].Arrival==EntryTransform.ShiftLeftOne||descent.Stages[descent.StageIndex].Arrival==EntryTransform.ShiftRightOne)
            ||descent.Version>=4&&descent.StageIndex==1&&floor==1&&Interaction.Attempt.Level.Floors[1].EntryTransform!=EntryTransform.None
            ||descent.IsFinal&&descent.Direction?.WindFloor==floor):Interaction?.Attempt.Direction?.WindFloor==floor;
        bool CurrentWindRight=>Interaction.Attempt.Level.Floors[Enumerable.Range(0,Interaction.Attempt.Level.Floors.Count).First(IsWindFloor)].EntryTransform==EntryTransform.ShiftRightOne;
        static LevelDefinition WindGuide(bool right)
        {
            var row=Enumerable.Range(0,4).Select(i=>SymbolId.Parse("s:"+(char)('A'+i))).ToArray();
            var target=right?row.Skip(3).Concat(row.Take(3)):row.Skip(1).Concat(row.Take(1));
            var open=new[]{true,true,true};
            return new LevelDefinition("wind-guide","wind-guide","1","2.0","wind-guide",row,target,
                new[]{new TargetSegment(SegmentKind.Shapes,0,4)},new[]{new FloorDefinition(1,open),
                new FloorDefinition(1,open,entryTransform:right?EntryTransform.ShiftRightOne:EntryTransform.ShiftLeftOne)});
        }
        LevelDefinition DirectedSource()
        {
            if(descent!=null)return DescentPublishedSource();
            var attempt=Interaction.Attempt;string hash=attempt.Direction?.SourceHash??attempt.PinnedContentHash;
            return Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(legacyContent)
                .FirstOrDefault(c=>c.Definition.PuzzleId==attempt.Level.PuzzleId&&c.Definition.ContentHash==hash)?.Definition;
        }
        public void LoadDirectedLevel(LevelDefinition source,string locale,DeliveryMood mood)
        {
            BeginDirectionRequest(source,locale,mood);
        }
        void BeginDirectionRequest(LevelDefinition source,string locale,DeliveryMood mood,PuzzleGoal goal=null,DirectionRecipe previous=null)
        {
            CancelDirectionWork();
            MountAttempt(new AttemptService(source,locale,goal));
            directionSource=source;directionLocale=locale;directionMood=mood;
            directionGoal=goal??PuzzleGoal.Delivery;directionPrevious=previous?.Copy();
            Interaction.SetPreparing(true);status.SetCopy("Checking your puzzle…");
            ResumeDirectionWork();
        }
        void CancelDirectionWork(bool preserveRequest=false)
        {
            directionCancellation?.Cancel();directionCancellation?.Dispose();directionCancellation=null;
            if(directionTask!=null)directionTask.ContinueWith(t=>{var ignored=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
            directionTask=null;
            if(!preserveRequest){directionSource=null;directionLocale=null;directionPrevious=null;directionGoal=null;}
        }
        void ResumeDirectionWork()
        {
            if(directionSource==null||directionTask!=null)return;
            var source=directionSource;var mood=directionMood;
            var goal=directionGoal;var previous=directionPrevious?.Copy();
            directionCancellation=new CancellationTokenSource();var token=directionCancellation.Token;
            directionTask=Task.Run(()=>DeliveryDirector.PlanVerified(source,mood,previous,goal,token),token);
            Interaction?.SetPreparing(true);
        }
        void UpdateDirection()
        {
            if(directionTask==null||!directionTask.IsCompleted)return;
            var task=directionTask;directionTask=null;directionCancellation?.Dispose();directionCancellation=null;
            if(task.IsCanceled){ResumeDirectionWork();return;}
            DirectionRecipe recipe=null;
            if(task.IsFaulted){var ignored=task.Exception;}else recipe=task.Result;
            var source=directionSource;string locale=directionLocale;var goal=directionGoal;
            var screen=CurrentScreen;bool travel=journeyActive;
            CancelDirectionWork();
            MountAttempt(new AttemptService(recipe?.Apply(source)??source,locale,goal,recipe));
            journeyActive=travel;SetScreen(screen);PersistProgress();
        }
        bool FallBackFromUnprovenDirection()
        {
            if(!preparation.HasFailed||Interaction.Attempt.Direction==null||Interaction.Attempt.RouteLength!=0)return false;
            var source=DirectedSource();if(source==null)return false;
            bool travel=journeyActive;string locale=Interaction.Attempt.ActiveLocale;var screen=CurrentScreen;
            MountAttempt(new AttemptService(source,locale,Interaction.Attempt.Goal));journeyActive=travel;SetScreen(screen);PersistProgress();return true;
        }
        bool CanOfferStrongerWood=>descent==null&&journeyActive&&Interaction.Attempt.Current.Status==PuzzleStatus.Lost
            &&DeliveryDirector.CanDirect(Interaction.Attempt.Level)&&profile.Director.Struggle>=4
            &&Interaction.Attempt.Direction?.Mood!=DeliveryMood.Recovery;
        void RestartWithStrongerWood()
        {
            if(!CanOfferStrongerWood)return;
            var source=DirectedSource();if(source==null)return;
            var goal=Interaction.Attempt.Goal;string locale=Interaction.Attempt.ActiveLocale;
            var previous=Interaction.Attempt.Direction;
            BeginDirectionRequest(source,locale,DeliveryMood.Recovery,goal,previous);journeyActive=true;ShowGame();
        }
    }
}
