using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cascade.Editor.Release
{
    // Public configuration only. Provider/server credentials never belong in a player.
    public sealed class AuthenticationConfiguration
    {
        [JsonProperty(Required=Required.Always)] public string unityProjectId;
        [JsonProperty(Required=Required.Always)] public string unityPlayerAccountsClientId;
        [JsonProperty(Required=Required.Always)] public string unityEnvironmentName;
        [JsonProperty(Required=Required.Always)] public string cloudCodeModuleName;
        [JsonProperty(Required=Required.Always)] public bool cloudCodeDeployedAndVerified;
        [JsonProperty(Required=Required.Always)] public bool purchaseVerificationConfigured;
        public bool serverRewardsConfigured;
        public string accountDeletionUrl="";
        public string unityPlayerAccountsDisplayName="";
        public string privacyPolicyUrl="";
        public string termsOfUseUrl="";
        [JsonIgnore] public bool HasClient=>!string.IsNullOrEmpty(unityPlayerAccountsClientId);
        public static AuthenticationConfiguration Read(string path,string projectId)
        {
            if(new FileInfo(path).Length>16384)throw new InvalidDataException("Authentication configuration is too large.");
            var json=JObject.Parse(File.ReadAllText(path),new JsonLoadSettings{DuplicatePropertyNameHandling=DuplicatePropertyNameHandling.Error});
            string[] strings={nameof(unityProjectId),nameof(unityPlayerAccountsClientId),nameof(unityEnvironmentName),nameof(cloudCodeModuleName)};
            string[] flags={nameof(cloudCodeDeployedAndVerified),nameof(purchaseVerificationConfigured)};
            string[] optionalStrings={nameof(accountDeletionUrl),nameof(unityPlayerAccountsDisplayName),nameof(privacyPolicyUrl),nameof(termsOfUseUrl)};
            string[] optional=optionalStrings.Concat(new[]{nameof(serverRewardsConfigured)}).ToArray();
            if(json.Properties().Any(p=>!strings.Contains(p.Name)&&!flags.Contains(p.Name)&&!optional.Contains(p.Name))||strings.Any(key=>json[key]?.Type!=JTokenType.String)||flags.Any(key=>json[key]?.Type!=JTokenType.Boolean)||json[nameof(serverRewardsConfigured)]!=null&&json[nameof(serverRewardsConfigured)].Type!=JTokenType.Boolean||optionalStrings.Any(key=>json[key]!=null&&json[key].Type!=JTokenType.String))
                throw new InvalidDataException("Authentication configuration must contain the named public fields with string/boolean types. No secrets.");
            var config=json.ToObject<AuthenticationConfiguration>(JsonSerializer.Create(
                new JsonSerializerSettings{MissingMemberHandling=MissingMemberHandling.Error,MaxDepth=8}));
            if(config==null)throw new InvalidDataException("Authentication configuration is empty.");
            config.Validate(projectId);return config;
        }
        public void Validate(string projectId)
        {
            if(!Guid.TryParseExact(unityProjectId,"D",out _)||unityProjectId!=projectId)
                throw new InvalidDataException("authentication.json must identify this Unity project's Cloud Project ID.");
            if(!Identifier(unityEnvironmentName,64,false)||!Identifier(cloudCodeModuleName,64,false)
                ||unityPlayerAccountsClientId==null||HasClient&&!Identifier(unityPlayerAccountsClientId,128,true))
                throw new InvalidDataException("Check the public Player Accounts client ID, environment and module name in authentication.json.");
            if(cloudCodeDeployedAndVerified&&!HasClient)
                throw new InvalidDataException("Cloud activation requires a configured Player Accounts client ID.");
            if(serverRewardsConfigured&&!cloudCodeDeployedAndVerified)throw new InvalidDataException("Server rewards require verified cloud deployment.");
            if(!string.IsNullOrEmpty(accountDeletionUrl)&&(!Uri.TryCreate(accountDeletionUrl,UriKind.Absolute,out var deletion)||deletion.Scheme!="https"||!string.IsNullOrEmpty(deletion.UserInfo)||!string.IsNullOrEmpty(deletion.Query)||!string.IsNullOrEmpty(deletion.Fragment)))throw new InvalidDataException("Invalid public account deletion URL.");
            if(unityPlayerAccountsDisplayName==null||unityPlayerAccountsDisplayName.Length>128||unityPlayerAccountsDisplayName.Any(char.IsControl))throw new InvalidDataException("Invalid public Player Accounts display name.");
            foreach(var address in new[]{privacyPolicyUrl,termsOfUseUrl})
                if(!string.IsNullOrEmpty(address)&&(!Uri.TryCreate(address,UriKind.Absolute,out var link)||link.Scheme!="https"||!string.IsNullOrEmpty(link.UserInfo)||!string.IsNullOrEmpty(link.Query)||!string.IsNullOrEmpty(link.Fragment)))throw new InvalidDataException("Invalid public policy URL.");
            if(purchaseVerificationConfigured&&!cloudCodeDeployedAndVerified)
                throw new InvalidDataException("Verified purchases require a deployed and verified account backend.");
        }
        static bool Identifier(string value,int max,bool dot)=>!string.IsNullOrEmpty(value)&&value.Length<=max
            &&value.All(c=>c>='a'&&c<='z'||c>='A'&&c<='Z'||c>='0'&&c<='9'||c=='-'||c=='_'||dot&&c=='.');
    }
}
