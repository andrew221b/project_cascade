using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        readonly System.Collections.Generic.List<GameButtonDesign> buttonDesigns=new System.Collections.Generic.List<GameButtonDesign>();
        GameButtonDesign RegisterButtonDesign(Button button,GameButtonRole? role=null)
        {
            var design=button.GetComponent<GameButtonDesign>()??button.gameObject.AddComponent<GameButtonDesign>();
            design.Initialize(safeRoot,role);return design;
        }
    }
}
