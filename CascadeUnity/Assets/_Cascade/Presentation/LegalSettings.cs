using System;
using UnityEngine;

namespace Cascade.Presentation
{
    [CreateAssetMenu(menuName="CASCADE/Legal and support settings")]
    public sealed class LegalSettings:ScriptableObject
    {
        public string privacyPolicyUrl="",termsOfUseUrl="",supportUrl="";
        public string publisherName="",publisherCountry="",supportEmail="",privacyEmail="";
        [Tooltip("Enable only after the publisher has approved the documents and verified public access without sign-in.")]
        public bool documentsApproved,publicUrlsVerified;
        public bool HasPublishedDocuments=>documentsApproved&&publicUrlsVerified&&!string.IsNullOrWhiteSpace(publisherName)
            &&!string.IsNullOrWhiteSpace(publisherCountry)&&!string.IsNullOrWhiteSpace(supportEmail)&&!string.IsNullOrWhiteSpace(privacyEmail)
            &&ValidUrl(privacyPolicyUrl)&&ValidUrl(termsOfUseUrl)&&ValidUrl(supportUrl);
        public static bool ValidUrl(string value)=>Uri.TryCreate(value,UriKind.Absolute,out var uri)&&uri.Scheme==Uri.UriSchemeHttps
            &&string.IsNullOrEmpty(uri.UserInfo)&&!string.IsNullOrWhiteSpace(uri.Host);
    }
}
