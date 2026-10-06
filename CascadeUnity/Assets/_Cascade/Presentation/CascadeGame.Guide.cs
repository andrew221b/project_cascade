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
            LiveDeviceDemo(Rect("From piece to hanging tool",menuRoot,new Vector2(.10f,.61f),new Vector2(.90f,.81f)),fixture,4);
            MenuReadingSurface("How to play cloth",new Vector2(.10f,.195f),new Vector2(.90f,.585f));
            CompletionText("Guide gesture",menuRoot,new Vector2(.12f,.485f),new Vector2(.88f,.577f),17,"Swap neighbours. The number on the wood counts swaps left. At zero, the row falls.");
            CompletionText("Guide tag",menuRoot,new Vector2(.12f,.386f),new Vector2(.88f,.478f),16,"The picture on a tool shows what it needs. Move that piece above it.");
            CompletionText("Guide stages",menuRoot,new Vector2(.12f,.293f),new Vector2(.88f,.386f),16,"Make the row below, then descend. Finish all three stages to open the next level.");
            CompletionText("Guide stars",menuRoot,new Vector2(.12f,.205f),new Vector2(.88f,.293f),16,"Fewer moves earn more stars. Replay an open level to beat your score. Three new lights earn a gift.");
            MenuButton("Tools & effects",.11f,()=>{deviceIndex=DeviceForCurrentFloor();DrawMenu();},true);
            MenuButton("Back",.015f,CloseMenu);
        }
    }
}
