using System.Linq;
using Cascade.Domain;
namespace Cascade.Content
{
    public static class StageJourney
    {
        // These are stable campaign sources; each actual stage is bound by the run.
        public static LevelContent[] Sequence(ContentCatalog catalog,PuzzleMode mode,string locale)
            =>catalog.PrimaryLevels.Select(c=>new LevelContent(c.Definition,mode==PuzzleMode.Shapes?"en":locale,c.Title,c.Lesson,c.IsPrimary,mode)).ToArray();
    }
}
