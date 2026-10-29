using System;
using UnityEngine;

namespace Cascade.Presentation
{
    // Expressions are edits of each exact source face, including its jaw/chin.
    // No borrowed mouth stamps. Original eyes, clothing and pose stay shared.
    public static class CharacterSpeechFaces
    {
        [Serializable] public sealed class Entry { public string name,source,sheet,resource;public int index,columns,rows;public float aspect;public Vector4 rect,muzzle,opening; }
        [Serializable] sealed class Book { public Entry[] faces; }
        public sealed class Pose { public Entry Entry;public Texture2D Open,Half;public float Top; }
        static Book book;
        const string Root="CascadeArt/CharacterMotionOct16/";
        static readonly int Half=Shader.PropertyToID("_SpeechHalfTex"),Valid=Shader.PropertyToID("_SpeechValid"),Rect=Shader.PropertyToID("_SpeechRect"),Muzzle=Shader.PropertyToID("_SpeechMuzzle"),Top=Shader.PropertyToID("_SpeechTop"),Uv=Shader.PropertyToID("_SpeechUv"),Open=Shader.PropertyToID("_SpeechOpenTex");
        public static Pose For(CharacterPoseProfile profile,Sprite source)
        {
            if(book==null)book=JsonUtility.FromJson<Book>(Resources.Load<TextAsset>(Root+"SpeechFaceLandmarks").text);
            Entry found=null;float error=.005f;
            foreach(var entry in book.faces)if(entry.name==profile.Name){float delta=Mathf.Abs(entry.aspect-profile.Cell.width/profile.Cell.height);if(delta<error&&Resources.Load<Texture2D>(entry.resource)==source.texture){found=entry;error=delta;}}
            if(found==null)return null;
            float eyeY=Mathf.Max(profile.NearEye.y,profile.FarEye.y),eyeHeight=Mathf.Max(profile.NearEye.w,profile.FarEye.w);
            return new Pose{Entry=found,Open=Resources.Load<Texture2D>(Root+found.sheet+"Open"),Half=Resources.Load<Texture2D>(Root+found.sheet+"Half"),Top=eyeY+eyeHeight*(profile.SoftBrows?.40f:1.1f)};
        }
        public static void Apply(Material material,Pose pose)
        {
            material.SetFloat(Valid,pose==null?0:1);if(pose==null)return;
            var e=pose.Entry;material.SetTexture(Open,pose.Open);material.SetVector(Rect,e.rect);material.SetVector(Muzzle,e.muzzle);material.SetFloat(Top,pose.Top);material.SetTexture(Half,pose.Half);
            material.SetVector(Uv,new Vector4(e.index%e.columns/(float)e.columns,1-(e.index/e.columns+1f)/e.rows,1f/e.columns,1f/e.rows));
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Clear(){book=null;}
    }
}
