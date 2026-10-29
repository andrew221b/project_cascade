using TMPro;
using UnityEngine;
namespace Cascade.Presentation
{
    public enum GameButtonRole { PrimaryGreen,SecondaryYellow,TertiaryLabel,IconPrimary,IconSecondary }
    [CreateAssetMenu(menuName="Cascade/UI/Button preset")]
    public sealed class GameButtonPreset:ScriptableObject
    {
        public GameButtonRole Role;
        public Sprite Skin;
        public TMP_FontAsset Font;
        public Vector2 Size=new Vector2(480,104);
        public float FontSize=34,IconSize=48,Inset=2;
        public Color Ink=Color.white;
        public static GameButtonPreset Load(GameButtonRole role)=>Resources.Load<GameButtonPreset>("CascadeArt/DesignSystem/Buttons/"+role);
    }
}
