using UnityEngine;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        public void ShowHowToPlay()
        {
            if(CurrentScreen!=GameScreen.Devices)returnScreen=CurrentScreen;
            deviceIndex=-1;SetScreen(GameScreen.Devices);
        }
        void DrawBasics()
        {
            MenuTitle("How to play","One row. Bring it home in order.");
            var fixture=Catalog.GetPrimary("C18").Definition;
            LiveDeviceDemo(Rect("From piece to hanging tool",menuRoot,new Vector2(.10f,.63f),new Vector2(.90f,.81f)),fixture,4);
            MenuReadingSurface("How to play cloth",new Vector2(.10f,.195f),new Vector2(.90f,.585f));
            CompletionText("Guide gesture",menuRoot,new Vector2(.12f,.525f),new Vector2(.88f,.612f),17,"Swap neighbours. The number on the wood counts swaps left. At zero, the row falls.");
            CompletionText("Guide tag",menuRoot,new Vector2(.12f,.428f),new Vector2(.88f,.515f),16,"The picture on a tool shows what it needs. Move that piece above it.");
            CompletionText("Guide stages",menuRoot,new Vector2(.12f,.329f),new Vector2(.88f,.418f),16,"Make the row below, then descend. Finish all three stages to open the next level.");
            CompletionText("Guide stars",menuRoot,new Vector2(.12f,.235f),new Vector2(.88f,.319f),16,"Fewer moves, more stars. Replay a level to improve your score.");
            MenuButton("Tools & effects",.11f,()=>{deviceIndex=DeviceForCurrentFloor();DrawMenu();},true);
            MenuButton("Back",.015f,CloseMenu);
        }
    }
}
