using UnityEditor;

namespace Cascade.Editor.Validation
{
    public static class StoryValidation
    {
        [MenuItem("CASCADE/QA/Run story content tests")]
        public static void Content()=>CascadeTestRunner.RunSelectedEditMode("StoryComic-Oct07",new[]{"Cascade.Tests.WorldPrologueTests"});
        [MenuItem("CASCADE/QA/Run comic journey tests")]
        public static void Flow()=>PlayModeValidation.RunSelected("StoryComic-Oct07",new[]{"Cascade.Tests.WorldComicFlowTests"});
    }
}
