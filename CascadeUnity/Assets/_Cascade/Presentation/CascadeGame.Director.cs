using System.Linq;
using Cascade.Application;
using Cascade.Domain;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        public DirectorMemory Director=>profile.Director;
        public bool IsWindFloor(int floor)=>Interaction?.Attempt.Direction?.WindFloor==floor;
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
            var attempt=Interaction.Attempt;string hash=attempt.Direction?.SourceHash??attempt.PinnedContentHash;
            return Catalog.PrimaryLevels.Concat(Catalog.LocalizedLevels).Concat(legacyContent)
                .FirstOrDefault(c=>c.Definition.PuzzleId==attempt.Level.PuzzleId&&c.Definition.ContentHash==hash)?.Definition;
        }
        public void LoadDirectedLevel(LevelDefinition source,string locale,DeliveryMood mood)
        {
            var recipe=DeliveryDirector.Plan(source,mood);
            MountAttempt(new AttemptService(recipe?.Apply(source)??source,locale,direction:recipe));PersistProgress();
        }
        bool FallBackFromUnprovenDirection()
        {
            if(!preparation.HasFailed||Interaction.Attempt.Direction==null||Interaction.Attempt.RouteLength!=0)return false;
            var source=DirectedSource();if(source==null)return false;
            bool travel=journeyActive;string locale=Interaction.Attempt.ActiveLocale;
            MountAttempt(new AttemptService(source,locale,Interaction.Attempt.Goal));journeyActive=travel;PersistProgress();return true;
        }
        bool CanOfferStrongerWood=>journeyActive&&Interaction.Attempt.Current.Status==PuzzleStatus.Lost
            &&DeliveryDirector.CanDirect(Interaction.Attempt.Level)&&profile.Director.Struggle>=4
            &&Interaction.Attempt.Direction?.Mood!=DeliveryMood.Recovery;
        void RestartWithStrongerWood()
        {
            if(!CanOfferStrongerWood)return;
            var source=DirectedSource();if(source==null)return;
            var recipe=DeliveryDirector.Plan(source,DeliveryMood.Recovery,Interaction.Attempt.Direction);
            var goal=Interaction.Attempt.Goal;string locale=Interaction.Attempt.ActiveLocale;
            MountAttempt(new AttemptService(recipe.Apply(source),locale,goal,recipe));journeyActive=true;PersistProgress();ShowGame();
        }
    }
}
