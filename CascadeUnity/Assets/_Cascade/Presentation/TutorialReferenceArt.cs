using UnityEngine;
namespace Cascade.Presentation
{
    // Existing panel pixels are reused with a local nine-slice; no duplicated frame texture.
    public static class TutorialReferenceArt
    {
        static Sprite panel;static readonly Sprite[] pieces=new Sprite[4];
        public static Sprite Panel {
            get {
                if(panel!=null)return panel;var source=GameplayReferenceArt.Sprite("FieldHelpPanel");if(source==null)return null;
                var rect=source.rect;panel=Sprite.Create(source.texture,rect,Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(300*rect.width/1202,300*rect.height/1308,250*rect.width/1202,300*rect.height/1308));return panel;
            }
        }
        public static Sprite Shape(Cascade.Domain.SymbolId symbol)
        {
            int index=symbol.Value=="circle"?0:symbol.Value=="triangle"?1:symbol.Value=="square"?2:symbol.Value=="diamond"?3:-1;
            if(index<0)return null;if(pieces[index]!=null)return pieces[index];var texture=Resources.Load<Texture2D>("CascadeArt/TutorialReferenceOct16/Pieces");if(texture==null)return null;
            float width=texture.width*.5f,height=texture.height*.5f;pieces[index]=Sprite.Create(texture,new Rect(index%2*width,(1-index/2)*height,width,height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);return pieces[index];
        }
        public static Sprite UndoRibbon=>Resources.Load<Sprite>("CascadeArt/TutorialReferenceOct16/UndoRibbon");
    }
}
