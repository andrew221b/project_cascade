using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cascade.Persistence
{
    public sealed class PurchaseIdentity
    {
        public string AppleToken,GoogleAccountId;
        // Metadata from AuthenticationService's already authenticated session only.
        // This does not validate JWTs or store receipts. Unknown SDK token formats
        // fall back to the existing Cloud Code endpoint; grants remain server-only.
        public static PurchaseIdentity FromAuthenticatedSession(string project,string player,string accessToken)
        {
            if(string.IsNullOrWhiteSpace(project)||string.IsNullOrWhiteSpace(player)||string.IsNullOrEmpty(accessToken)||accessToken.Length>16384)return null;
            try{
                var parts=accessToken.Split('.');if(parts.Length!=3||parts[1].Length==0)return null;
                string payload=parts[1].Replace('-','+').Replace('_','/');payload=payload.PadRight(payload.Length+(4-payload.Length%4)%4,'=');
                var claims=JObject.Parse(new UTF8Encoding(false,true).GetString(Convert.FromBase64String(payload)));
                if(claims["project_id"]?.Type!=JTokenType.String||(string)claims["project_id"]!=project
                    ||claims["sub"]?.Type!=JTokenType.String||(string)claims["sub"]!=player
                    ||claims["exp"]?.Type!=JTokenType.Integer||(long)claims["exp"]<=DateTimeOffset.UtcNow.ToUnixTimeSeconds()+30)return null;
                var audience=claims["aud"] as JArray;if(audience==null||audience.Count>64)return null;
                string environment=null;
                foreach(var item in audience){
                    if(item.Type!=JTokenType.String)return null;string value=(string)item;
                    if(!value.StartsWith("envId:",StringComparison.Ordinal))continue;
                    if(environment!=null)return null;environment=value.Substring(6);
                }
                if(!Guid.TryParseExact(environment,"D",out var ignored))return null;
                return For(project,environment,player);
            }catch(Exception e)when(e is JsonException||e is FormatException||e is ArgumentException||e is OverflowException){return null;}
        }
        public static PurchaseIdentity For(string project,string environment,string player)
        {
            if(string.IsNullOrWhiteSpace(project)||string.IsNullOrWhiteSpace(environment)||string.IsNullOrWhiteSpace(player))throw new ArgumentException("Authenticated purchase context is required.");
            string hex;
            using(var hash=SHA256.Create())hex=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes("cascade-purchase-v1|"+project+"|"+environment+"|"+player))).Replace("-","").ToLowerInvariant();
            return new PurchaseIdentity{GoogleAccountId=hex,AppleToken=hex.Substring(0,8)+"-"+hex.Substring(8,4)+"-"+hex.Substring(12,4)+"-"+hex.Substring(16,4)+"-"+hex.Substring(20,12)};
        }
    }
}
