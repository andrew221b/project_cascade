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
            LiveDeviceDemo(Rect("From piece to hanging tool",menuRoot,new Vector2(.16f,.51f),new Vector2(.84f,.72f)),fixture,4);
            Text("Guide gesture",menuRoot,new Vector2(.16f,.42f),new Vector2(.84f,.51f),17,"Drag a piece onto its neighbour. They change places.");
            Text("Guide tag",menuRoot,new Vector2(.16f,.29f),new Vector2(.84f,.42f),17,"The little tag shows which piece the tool needs. Move that piece directly above the tool.");
            Text("Guide goal",menuRoot,new Vector2(.16f,.20f),new Vector2(.84f,.29f),17,"Make the row below. Then bring your row down.");
            MenuButton("Tools & effects",.11f,()=>{deviceIndex=DeviceForCurrentFloor();DrawMenu();},true);
            MenuButton("Back",.015f,CloseMenu);
        }
    }
}
