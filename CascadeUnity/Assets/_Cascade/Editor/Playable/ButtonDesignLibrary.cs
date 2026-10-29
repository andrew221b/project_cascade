#if UNITY_EDITOR
using System;
using System.IO;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Cascade.Editor.Playable
{
    public static class ButtonDesignLibrary
    {
        public const string Folder="Assets/_Cascade/Content/Resources/CascadeArt/DesignSystem/Buttons";
        [MenuItem("Cascade/UI/Refresh button prefabs")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder);
            foreach(GameButtonRole role in Enum.GetValues(typeof(GameButtonRole))){
                var preset=GameButtonPreset.Load(role);if(preset==null||preset.Font==null)throw new InvalidOperationException("Missing button preset: "+role);
                var go=new GameObject(role.ToString(),typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button),typeof(GameButtonDesign));go.layer=5;
                var rect=(RectTransform)go.transform;rect.sizeDelta=preset.Size;
                var face=go.GetComponent<Image>();face.sprite=preset.Skin;face.type=Image.Type.Sliced;face.color=role==GameButtonRole.TertiaryLabel?Color.clear:Color.white;
                go.GetComponent<Button>().targetGraphic=face;go.GetComponent<GameButtonDesign>().Preset=preset;
                if(preset.Skin!=null)go.AddComponent<PrimaryButtonSkin>().Configure(preset.Skin);
                var body=new GameObject("Reference shop button body",typeof(RectTransform));body.transform.SetParent(go.transform,false);((RectTransform)body.transform).sizeDelta=preset.Size;
                bool icon=role==GameButtonRole.IconPrimary;
                if(icon){var part=new GameObject("Button icon",typeof(RectTransform),typeof(Image));part.transform.SetParent(body.transform,false);var image=part.GetComponent<Image>();image.sprite=Resources.Load<Sprite>("CascadeArt/ItemsReferenceOct14/Storefront");image.preserveAspect=true;image.raycastTarget=false;((RectTransform)part.transform).sizeDelta=Vector2.one*preset.IconSize;}
                else{var part=new GameObject("Button label",typeof(RectTransform),typeof(TextMeshProUGUI));part.transform.SetParent(body.transform,false);var label=part.GetComponent<TextMeshProUGUI>();label.font=preset.Font;label.fontSize=preset.FontSize;label.text=role==GameButtonRole.IconSecondary?"?":"Button";label.color=preset.Ink;label.raycastTarget=false;((RectTransform)part.transform).sizeDelta=preset.Size;
                    if(role==GameButtonRole.TertiaryLabel){
                        string path=Folder+"/TertiaryLabel.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                        if(material==null){material=new Material(preset.Font.material);AssetDatabase.CreateAsset(material,path);}
                        material.EnableKeyword("UNDERLAY_ON");material.SetColor(ShaderUtilities.ID_UnderlayColor,new Color(.035f,.018f,.01f,.95f));material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX,.15f);material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY,-.5f);material.SetFloat(ShaderUtilities.ID_UnderlayDilate,.1f);material.SetFloat(ShaderUtilities.ID_UnderlaySoftness,.15f);EditorUtility.SetDirty(material);label.fontSharedMaterial=material;label.UpdateMeshPadding();
                    }}
                PrefabUtility.SaveAsPrefabAsset(go,Folder+"/"+role+".prefab");UnityEngine.Object.DestroyImmediate(go);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
    }
}
#endif
