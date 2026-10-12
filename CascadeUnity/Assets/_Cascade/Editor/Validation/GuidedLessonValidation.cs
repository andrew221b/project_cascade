using UnityEditor;

namespace Cascade.Editor.Validation
{
    public static class GuidedLessonValidation
    {
        [MenuItem("CASCADE/QA/Run guided input tests")]
        public static void Input()=>CascadeTestRunner.RunSelectedEditMode("GuidedLessons-Oct08",new[]{"Cascade.Tests.InteractionTests","Cascade.Tests.GuidedPracticeTests","Cascade.Tests.LessonRouteTests","Cascade.Tests.MechanicLessonTests"});
        [MenuItem("CASCADE/QA/Run guided lesson flow tests")]
        public static void Flow()=>PlayModeValidation.RunSelected("GuidedLessons-Oct08",new[]{"Cascade.Tests.GuidedLessonFlowTests"});
    }
}
