using System;
using System.IO;
using Cascade.Presentation;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Release
{
    public static class WebsiteLegalConfigImport
    {
        sealed class Config
        {
            public string siteOrigin,customDomain,privacyPolicyUrl,termsOfUseUrl,supportUrl,publisherName,publisherCountry,supportEmail,privacyEmail;
            public bool documentsApproved,publicLegalUrlsVerified;
        }
        [MenuItem("CASCADE/Release/Apply website and legal configuration")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Apply website settings outside Play Mode.");
            string root=Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            var c=JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(root,"Configs","website.json")));
            if(c==null)throw new InvalidDataException("Missing Configs/website.json.");
            string origin=(string.IsNullOrWhiteSpace(c.customDomain)?c.siteOrigin:c.customDomain)?.TrimEnd('/');
            if(!LegalSettings.ValidUrl(origin))throw new InvalidDataException("Website origin must use HTTPS.");
            string privacy=string.IsNullOrWhiteSpace(c.privacyPolicyUrl)?origin+"/privacy/":c.privacyPolicyUrl;
            string terms=string.IsNullOrWhiteSpace(c.termsOfUseUrl)?origin+"/terms/":c.termsOfUseUrl;
            string support=string.IsNullOrWhiteSpace(c.supportUrl)?origin+"/support/":c.supportUrl;
            if(!LegalSettings.ValidUrl(privacy)||!LegalSettings.ValidUrl(terms)||!LegalSettings.ValidUrl(support))throw new InvalidDataException("Legal and support URLs must use HTTPS.");
            if(c.documentsApproved&&(string.IsNullOrWhiteSpace(c.publisherName)||string.IsNullOrWhiteSpace(c.publisherCountry)||!ValidEmail(c.supportEmail)||!ValidEmail(c.privacyEmail)))
                throw new InvalidDataException("Approved documents require the publisher identity and public support/privacy email.");
            const string path="Assets/_Cascade/Content/Resources/CascadeArt/LegalSettings.asset";
            var s=AssetDatabase.LoadAssetAtPath<LegalSettings>(path);
            if(s==null){s=ScriptableObject.CreateInstance<LegalSettings>();AssetDatabase.CreateAsset(s,path);}
            Undo.RecordObject(s,"Configure CASCADE legal documents");
            s.privacyPolicyUrl=privacy;s.termsOfUseUrl=terms;s.supportUrl=support;s.publisherName=c.publisherName;s.publisherCountry=c.publisherCountry;s.supportEmail=c.supportEmail;s.privacyEmail=c.privacyEmail;s.documentsApproved=c.documentsApproved;
            s.publicUrlsVerified=c.documentsApproved&&c.publicLegalUrlsVerified;
            foreach(string document in new[]{"privacy","terms"})if(Resources.Load<TextAsset>("CascadeLegal/"+document)==null)throw new InvalidDataException("Build Website first to export offline legal documents.");
            EditorUtility.SetDirty(s);AssetDatabase.SaveAssets();
            Debug.Log("CASCADE legal configuration applied. Public links remain hidden until approved documents and unauthenticated URL access are verified; offline copies remain available.");
        }
        static bool ValidEmail(string value)=>!string.IsNullOrWhiteSpace(value)&&value.Contains("@")&&!value.Contains(" ");
    }
}
