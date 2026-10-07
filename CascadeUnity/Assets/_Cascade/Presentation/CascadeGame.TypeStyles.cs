using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        enum CelebrationType { Information, Number, SkyNumber, PaperTitle, EnamelLabel }
        readonly Dictionary<CelebrationType,Material> celebrationTypeMaterials=new Dictionary<CelebrationType,Material>();
        readonly List<(TextMeshProUGUI Label,CelebrationType Role)> celebrationTypeLabels=new List<(TextMeshProUGUI,CelebrationType)>();

        // Live lettering keeps localized copy and animated counters crisp. One material per
        // role provides a fine carved edge and a soft downward shadow, rather than duplicate text.
        void StyleCelebrationType(TextMeshProUGUI label,CelebrationType role)
        {
            label.font=art.titleFont;
            label.extraPadding=true;
            if(!celebrationTypeMaterials.TryGetValue(role,out var material)){
                material=new Material(label.fontSharedMaterial){name="CASCADE celebration "+role,hideFlags=HideFlags.DontSave};
                material.SetColor(ShaderUtilities.ID_FaceColor,Color.white);
                material.SetFloat(ShaderUtilities.ID_FaceDilate,role==CelebrationType.Number||role==CelebrationType.SkyNumber?.015f:0);
                material.SetColor(ShaderUtilities.ID_OutlineColor,role==CelebrationType.Number||role==CelebrationType.EnamelLabel?
                    new Color(.035f,.20f,.21f,.95f):role==CelebrationType.SkyNumber?new Color(1,.80f,.37f,.95f):new Color(1,.98f,.87f,.92f));
                material.SetFloat(ShaderUtilities.ID_OutlineWidth,role==CelebrationType.PaperTitle?.035f:role==CelebrationType.Number||role==CelebrationType.SkyNumber?.11f:.085f);
                material.EnableKeyword("OUTLINE_ON");
                material.EnableKeyword("UNDERLAY_ON");
                material.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.025f,.13f,.14f,.30f));
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX,.03f);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,-.20f);
                material.SetFloat(ShaderUtilities.ID_UnderlayDilate,.02f);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness,.24f);
                celebrationTypeMaterials.Add(role,material);
            }
            label.fontSharedMaterial=material;
            label.UpdateMeshPadding();
            celebrationTypeLabels.Add((label,role));
            PaintCelebrationType(label,role);
        }
        void PaintCelebrationType(TextMeshProUGUI label,CelebrationType role)
        {
            bool night=machine!=null&&machine.LivingWorld.Zone==8;
            label.color=role==CelebrationType.Number?new Color(1,.87f,.43f):role==CelebrationType.EnamelLabel?MenuPaper:
                (role==CelebrationType.Information||role==CelebrationType.SkyNumber)&&night?MenuPaper:new Color(.065f,.25f,.26f);
        }
        void FinishThemedTypeStyles()
        {
            for(int i=celebrationTypeLabels.Count-1;i>=0;i--){
                var item=celebrationTypeLabels[i];
                if(item.Label==null){celebrationTypeLabels.RemoveAt(i);continue;}
                PaintCelebrationType(item.Label,item.Role);
            }
        }
        void DisposeCelebrationTypeStyles()
        {
            foreach(var material in celebrationTypeMaterials.Values)if(material!=null)Destroy(material);
            celebrationTypeMaterials.Clear();celebrationTypeLabels.Clear();
        }
    }
}
