using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Cascade.Presentation
{
    // English source keys are editorial text. Symbols use Raw and never enter this table.
    public readonly struct UiCopy : IFormattable
    {
        public const string Table = "CascadeUI";
        public readonly string Key;
        public readonly object[] Arguments;
        public readonly bool IsRaw;
        public UiCopy(string key,params object[] arguments) { Key=key??"";Arguments=arguments;IsRaw=string.IsNullOrEmpty(key)||key.All(char.IsDigit); }
        UiCopy(string raw,bool literal) { Key=raw??"";Arguments=Array.Empty<object>();IsRaw=literal; }
        public static UiCopy Raw(string value)=>new UiCopy(value,true);
        public static implicit operator UiCopy(string value)=>new UiCopy(value);
        public string English=>Arguments==null||Arguments.Length==0?Key:string.Format(CultureInfo.InvariantCulture,Key,Arguments.Select(a=>a is UiCopy copy?(object)copy.English:a).ToArray());
        public override string ToString()=>IsRaw||string.IsNullOrEmpty(Key)?Key:LocalizationSettings.StringDatabase.GetLocalizedString(Table,Key,Arguments);
        public string ToString(string format,IFormatProvider provider)=>ToString();
        public static UiCopy Join(string separator,System.Collections.Generic.IEnumerable<UiCopy> parts)
        { var values=parts.ToArray();return values.Length==0?Raw(""):new UiCopy("{0}",new Joined(separator,values)); }
        sealed class Joined : IFormattable
        {
            readonly string separator;readonly UiCopy[] parts;
            public Joined(string s,UiCopy[] p){separator=s;parts=p;}
            public override string ToString()=>string.Join(separator,parts.Select(p=>p.ToString()));
            public string ToString(string f,IFormatProvider p)=>ToString();
        }
    }
    public static class UiText
    {
        public static void SetCopy(this TMP_Text text,UiCopy copy)
        {
            var binding=text.GetComponent<LocalizedCaption>();
            if(binding==null)binding=text.gameObject.AddComponent<LocalizedCaption>();
            binding.Set(copy);
        }
    }
    [DisallowMultipleComponent]
    public sealed class LocalizedCaption : MonoBehaviour
    {
        TMP_Text text;
        LocalizeStringEvent localize;
        string previous;
        public string SourceIdentity=>previous??"";
        public void Set(UiCopy copy)
        {
            if(text==null)text=GetComponent<TMP_Text>();
            string identity=copy.English;
            if(previous==identity&&localize!=null)return;
            previous=identity;
            if(copy.IsRaw||string.IsNullOrEmpty(copy.Key)) {
                if(localize!=null)localize.enabled=false;
                text.text=copy.Key;return;
            }
            if(localize==null) {
                localize=gameObject.AddComponent<LocalizeStringEvent>();
                localize.OnUpdateString.AddListener(Apply);
            }
            localize.enabled=false;
            localize.StringReference=new LocalizedString(UiCopy.Table,copy.Key){Arguments=copy.Arguments};
            localize.enabled=true;localize.RefreshString();
        }
        void Apply(string value){if(text!=null)text.text=value;}
    }
}
