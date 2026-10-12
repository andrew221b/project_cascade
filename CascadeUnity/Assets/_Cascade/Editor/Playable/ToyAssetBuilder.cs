using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cascade.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Cascade.Editor.Playable
{
    public static class ToyAssetBuilder
    {
        const string Root = "Assets/_Cascade/Content/Resources/CascadeArt";
        [MenuItem("CASCADE/Build Toy Art")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Build artwork outside Play Mode.");
            Directory.CreateDirectory(Root + "/Meshes"); Directory.CreateDirectory(Root + "/Materials"); Directory.CreateDirectory(Root + "/Fonts");
            AssetDatabase.Refresh();
            var art = AssetDatabase.LoadAssetAtPath<ToyArt>(Root + "/ToyArt.asset");
            if (art == null) { art = ScriptableObject.CreateInstance<ToyArt>(); AssetDatabase.CreateAsset(art, Root + "/ToyArt.asset"); }
            Undo.RecordObject(art, "Build original CASCADE artwork");
            var contours = new[] {
                Round(new[] { new Vector2(0,.58f),new Vector2(-.54f,-.4f),new Vector2(.54f,-.4f) }, .15f),
                Radial(48, _ => .5f),
                RoundedRect(1,1,.18f),
                Round(new[] {new Vector2(0,.58f),new Vector2(-.46f,0),new Vector2(0,-.58f),new Vector2(.46f,0)},.12f),
                Round(Radial(10, i => i%2==0?.56f:.29f, Mathf.PI*.5f),.13f),
                Round(Radial(6,_=>.54f,Mathf.PI/6),.12f)
            };
            string[] names={"Triangle","Circle","Square","Diamond","Star","Hexagon"};
            art.shapes = contours.Select((p,i)=>SaveMesh(names[i],Extrude(p,.26f))).ToArray();
            art.tile=SaveMesh("LetterTile",Extrude(RoundedRect(1,1,.18f),.22f));
            art.beam=SaveMesh("PlatformBeam",Extrude(RoundedRect(6,1,.3f),.36f));
            art.disk=SaveMesh("RoundHardware",Extrude(Radial(32,_=>.5f),.22f));
            art.quad=SaveMesh("Quad",new Mesh {vertices=new[]{new Vector3(-.5f,-.5f),new Vector3(.5f,-.5f),new Vector3(.5f,.5f),new Vector3(-.5f,.5f)},
                triangles=new[]{0,2,1,0,3,2},uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up}});
            string[] shades={"FFC856","59CAB1","EB8B7E","B496D9","64B9E1","F3A45D"};
            art.colors=shades.Select((c,i)=>Material(names[i]+"Plastic",c,.28f,.02f)).ToArray();
            art.cream=Material("IvoryTile","FFF0CC",.23f,0);
            art.graphite=Material("WarmGraphite","354C50",.25f,.1f);
            art.gold=Material("GoldenHardware","EFB953",.34f,.16f);
            art.paleGold=Material("ReceiverIvory","FBE2A3",.22f,.03f);
            art.teal=Material("MintEnamel","53AE9E",.28f,.06f);
            art.coral=Material("CoralDetail","E38370",.26f,.03f);
            art.shadow=PlainMaterial("SoftContact",Shader.Find("CASCADE/SoftContact"));
            art.shadow.SetColor("_BaseColor",new Color(.11f,.23f,.25f,.28f));
            art.backdrop=PlainMaterial("SkyWorkshop",Shader.Find("Universal Render Pipeline/Unlit"));
            var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/SkyWorkshop.png");
            importer.textureType=TextureImporterType.Default; importer.mipmapEnabled=false; importer.wrapMode=TextureWrapMode.Clamp;
            importer.maxTextureSize=2048; importer.textureCompression=TextureImporterCompression.Compressed; importer.SaveAndReimport();
            art.backdrop.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/SkyWorkshop.png"));
            art.backdrop.SetColor("_BaseColor",Color.white);
            art.bodyFont=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceBody")??FontAsset("BalsamiqSans-Regular","Body");
            art.titleFont=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/ShapeManiaInterface")??Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/InterfaceTitle")??FontAsset("BalsamiqSans-Bold","Title");
            art.displayFont=Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/ShapeManiaInterface")??Resources.Load<TMP_FontAsset>("CascadeArt/Fonts/StorybookDisplay");
            art.panel=PanelSprite();
            EditorUtility.SetDirty(art); EditorUtility.SetDirty(art.shadow); EditorUtility.SetDirty(art.backdrop);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("CASCADE original toy art built: six silhouettes, tile, platform, shared materials, fonts and background.");
        }
        static Material Material(string name,string hex,float smoothness,float metallic)
        {
            var material=PlainMaterial(name,Shader.Find("Universal Render Pipeline/Lit"));
            ColorUtility.TryParseHtmlString("#"+hex,out var color); material.SetColor("_BaseColor",color);
            material.SetFloat("_Smoothness",smoothness); material.SetFloat("_Metallic",metallic); EditorUtility.SetDirty(material); return material;
        }
        static Material PlainMaterial(string name,Shader shader)
        {
            if(shader==null)throw new InvalidOperationException("CASCADE shader missing: "+name);
            string path=Root+"/Materials/"+name+".mat"; var result=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(result==null){result=new Material(shader){name=name};AssetDatabase.CreateAsset(result,path);}
            return result;
        }
        static Mesh SaveMesh(string name,Mesh mesh)
        {
            mesh.name=name; mesh.RecalculateNormals(); mesh.RecalculateBounds(); string path=Root+"/Meshes/"+name+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing!=null){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);EditorUtility.SetDirty(existing);return existing;}
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static TMP_FontAsset FontAsset(string source,string name)
        {
            string path=Root+"/Fonts/"+name+".asset"; var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path); if(existing!=null)return existing;
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Cascade/Art/Fonts/"+source+".ttf");
            var asset=TMP_FontAsset.CreateFontAsset(font,80,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            asset.name=name; AssetDatabase.CreateAsset(asset,path);
            string characters=new string(Enumerable.Range(32,95).Select(c=>(char)c).Concat(Enumerable.Range(0x400,0x100).Select(c=>(char)c)).ToArray())+"−→←‹›·…";
            asset.TryAddCharacters(characters,out string missing); asset.atlasPopulationMode=AtlasPopulationMode.Static;
            AssetDatabase.AddObjectToAsset(asset.material,asset);
            foreach(var texture in asset.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,asset);
            EditorUtility.SetDirty(asset); Debug.Log(name+" font built; unsupported extended characters (full glyph QA is P15): "+missing);
            return asset;
        }
        static Sprite PanelSprite()
        {
            string path=Root+"/Panel.png";
            if(!File.Exists(path)){
                const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var pixels=new Color32[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                    var p=new Vector2(Mathf.Abs(x-31.5f)-18,Mathf.Abs(y-31.5f)-18);
                    float distance=new Vector2(Mathf.Max(0,p.x),Mathf.Max(0,p.y)).magnitude-13;
                    pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(.5f-distance));
                }
                texture.SetPixels32(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(22,22,22,22);
            importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static Vector2[] Radial(int count,Func<int,float> radius,float offset=0)=>Enumerable.Range(0,count).Select(i=>{
            float a=offset+i*Mathf.PI*2/count;return new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius(i);}).ToArray();
        static Vector2[] RoundedRect(float width,float height,float radius)
        {
            var points=new List<Vector2>();
            for(int corner=0;corner<4;corner++){
                float start=corner*Mathf.PI*.5f; var center=new Vector2(corner==0||corner==3?width/2-radius:-width/2+radius,corner<2?height/2-radius:-height/2+radius);
                for(int i=0;i<=8;i++){float a=start+i*Mathf.PI/16;points.Add(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
            }
            return points.ToArray();
        }
        static Vector2[] Round(Vector2[] points,float fraction)
        {
            var result=new List<Vector2>();
            for(int i=0;i<points.Length;i++){
                var vertex=points[i];var a=Vector2.Lerp(vertex,points[(i+points.Length-1)%points.Length],fraction);
                var b=Vector2.Lerp(vertex,points[(i+1)%points.Length],fraction);
                for(int n=0;n<=6;n++){float t=n/6f;result.Add((1-t)*(1-t)*a+2*(1-t)*t*vertex+t*t*b);}
            }
            return result.ToArray();
        }
        static Mesh Extrude(Vector2[] outline,float depth)
        {
            int n=outline.Length;var vertices=new List<Vector3>();var triangles=new List<int>();
            float[] scales={.88f,.985f,1,.97f,.89f};float[] zs={-depth*.5f,-depth*.32f,depth*.02f,depth*.36f,depth*.5f};
            for(int ring=0;ring<scales.Length;ring++)foreach(var p in outline)vertices.Add(new Vector3(p.x*scales[ring],p.y*scales[ring],zs[ring]));
            for(int r=0;r<scales.Length-1;r++)for(int i=0;i<n;i++){
                int a=r*n+i,b=r*n+(i+1)%n,c=(r+1)*n+i,d=(r+1)*n+(i+1)%n;
                triangles.AddRange(new[]{a,b,c,c,b,d});
            }
            int front=vertices.Count;vertices.Add(new Vector3(0,0,zs[0]));int back=vertices.Count;vertices.Add(new Vector3(0,0,zs[4]));
            for(int i=0;i<n;i++){triangles.AddRange(new[]{front,(i+1)%n,i,back,4*n+i,4*n+(i+1)%n});}
            return new Mesh{vertices=vertices.ToArray(),triangles=triangles.ToArray(),uv=vertices.Select(v=>new Vector2(v.x+.5f,v.y+.5f)).ToArray()};
        }
    }
}
