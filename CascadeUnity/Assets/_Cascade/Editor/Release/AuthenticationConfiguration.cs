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
        public bool HasClient=>!string.IsNullOrEmpty(unityPlayerAccountsClientId);
        public static AuthenticationConfiguration Read(string path,string projectId)
        {
            if(new FileInfo(path).Length>16384)throw new InvalidDataException("Authentication configuration is too large.");
            var json=JObject.Parse(File.ReadAllText(path),new JsonLoadSettings{DuplicatePropertyNameHandling=DuplicatePropertyNameHandling.Error});
            string[] strings={nameof(unityProjectId),nameof(unityPlayerAccountsClientId),nameof(unityEnvironmentName),nameof(cloudCodeModuleName)};
            string[] flags={nameof(cloudCodeDeployedAndVerified),nameof(purchaseVerificationConfigured)};
            if(json.Count!=strings.Length+flags.Length||strings.Any(key=>json[key]?.Type!=JTokenType.String)||flags.Any(key=>json[key]?.Type!=JTokenType.Boolean))
                throw new InvalidDataException("Authentication configuration must contain the six named fields with string/boolean types. No secrets.");
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
            if(purchaseVerificationConfigured&&!cloudCodeDeployedAndVerified)
                throw new InvalidDataException("Verified purchases require a deployed and verified account backend.");
        }
        static bool Identifier(string value,int max,bool dot)=>!string.IsNullOrEmpty(value)&&value.Length<=max
            &&value.All(c=>c>='a'&&c<='z'||c>='A'&&c<='Z'||c>='0'&&c<='9'||c=='-'||c=='_'||dot&&c=='.');
    }
}
