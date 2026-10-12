using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        enum CelebrationType { Information, Number, SkyNumber, PaperTitle, EnamelLabel, DisplayTitle, VictoryHeadline, BodyCopy, SkyCopy, EnamelCopy }
        readonly Dictionary<CelebrationType,Material> celebrationTypeMaterials=new Dictionary<CelebrationType,Material>();
        readonly List<(TextMeshProUGUI Label,CelebrationType Role)> celebrationTypeLabels=new List<(TextMeshProUGUI,CelebrationType)>();
        readonly HashSet<TextMeshProUGUI> themedLabels=new HashSet<TextMeshProUGUI>();

        // Shared lettering stays unframed; dialogue and selected trophy names own explicit paper plates.
        void StyleCelebrationType(TextMeshProUGUI label,CelebrationType role)
        {
            label.font=role==CelebrationType.DisplayTitle||role==CelebrationType.PaperTitle?art.titleFont:
                role==CelebrationType.BodyCopy||role==CelebrationType.EnamelCopy||role==CelebrationType.VictoryHeadline?art.bodyFont:art.titleFont;
            themedLabels.Add(label);
            label.extraPadding=false;
            if(!celebrationTypeMaterials.TryGetValue(role,out var material)){
                material=new Material(label.fontSharedMaterial){name="CASCADE celebration "+role,hideFlags=HideFlags.DontSave};
                material.SetColor(ShaderUtilities.ID_FaceColor,Color.white);
                material.SetFloat(ShaderUtilities.ID_FaceDilate,role==CelebrationType.Number||role==CelebrationType.SkyNumber?.015f:0);
                material.SetFloat(ShaderUtilities.ID_OutlineWidth,0);
                material.DisableKeyword("OUTLINE_ON");material.DisableKeyword("UNDERLAY_ON");
                if(role==CelebrationType.VictoryHeadline){
                    material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.20f,.085f,.025f,.9f));
                    material.SetFloat(ShaderUtilities.ID_OutlineWidth,.025f);
                    material.EnableKeyword("OUTLINE_ON");
                }
                if(role==CelebrationType.SkyCopy){
                    material.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.012f,.035f,.08f,1f));
                    material.SetFloat(ShaderUtilities.ID_UnderlayDilate,.32f);
                    material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,0);
                    material.SetFloat(ShaderUtilities.ID_UnderlaySoftness,.10f);
                }
                celebrationTypeMaterials.Add(role,material);
            }
            label.fontSharedMaterial=material;
            label.UpdateMeshPadding();
            celebrationTypeLabels.Add((label,role));
            PaintCelebrationType(label,role);
        }
        void PaintCelebrationType(TextMeshProUGUI label,CelebrationType role)
        {
            bool enamel=role==CelebrationType.EnamelLabel||role==CelebrationType.EnamelCopy;
            bool night=machine!=null&&machine.LivingWorld.Zone==8
                &&(CurrentScreen==GameScreen.JourneyMap&&islandBrowseIndex<0&&mapPage==8
                    ||CurrentScreen==GameScreen.Playing||CurrentScreen==GameScreen.Win||CurrentScreen==GameScreen.Fail||CurrentScreen==GameScreen.Checkpoint||CurrentScreen==GameScreen.JourneyEnd);
            bool bareNightCopy=night&&(role==CelebrationType.BodyCopy||role==CelebrationType.Information
                ||role==CelebrationType.DisplayTitle||role==CelebrationType.SkyNumber||role==CelebrationType.SkyCopy);
            label.color=bareNightCopy&&!IsOnStoryPaper(label)?MenuPaper:role==CelebrationType.Number?new Color(1,.87f,.43f):role==CelebrationType.VictoryHeadline?new Color(.22f,.10f,.035f):enamel?MenuPaper:new Color(.12f,.15f,.13f);
            if(role==CelebrationType.VictoryHeadline){
                label.enableVertexGradient=false;
            }

        }
        void PaintGameplayTextContrast()
        {
            if(machine==null)return;
            bool night=machine.LivingWorld.Zone==8;
            var ink=night?MenuPaper:Preferences.HighContrast?new Color(.06f,.17f,.18f):Ink;
            if(receiverLabel!=null)receiverLabel.color=ink;
            if(celebrationTypeMaterials.TryGetValue(CelebrationType.SkyCopy,out var material)
                &&material.IsKeywordEnabled("UNDERLAY_ON")!=night){
                if(night)material.EnableKeyword("UNDERLAY_ON");else material.DisableKeyword("UNDERLAY_ON");
                receiverLabel?.UpdateMeshPadding();
            }
        }
        void FinishThemedTypeStyles()
        {
            themedLabels.RemoveWhere(label=>label==null);
            if(menuRoot!=null)foreach(var label in menuRoot.GetComponentsInChildren<TextMeshProUGUI>()){
                if(label.name=="Illustrated numeral copy")continue;
                if(themedLabels.Contains(label)||IsSymbolFace(label.name)||label.name=="Mini glyph"||label.GetComponentInParent<UnityEngine.UI.Button>()!=null)continue;
                if(navigationLightLabels.Contains(label)){
                    StyleCelebrationType(label,label.font==art.titleFont?CelebrationType.EnamelLabel:CelebrationType.EnamelCopy);continue;
                }
                bool heading=label.name=="Page heading"||label.name=="Selected object title"||label.name=="Gift name"||label.name=="Menu title"||label.name=="CASCADE"||label.name=="Region";
                StyleCelebrationType(label,heading?CelebrationType.DisplayTitle:CelebrationType.BodyCopy);
            }
            for(int i=celebrationTypeLabels.Count-1;i>=0;i--){
                var item=celebrationTypeLabels[i];
                if(item.Label==null){celebrationTypeLabels.RemoveAt(i);continue;}
                PaintCelebrationType(item.Label,item.Role);
            }
        }
        void DisposeCelebrationTypeStyles()
        {
            foreach(var material in celebrationTypeMaterials.Values)if(material!=null)Destroy(material);
            celebrationTypeMaterials.Clear();celebrationTypeLabels.Clear();themedLabels.Clear();
        }
    }
}
