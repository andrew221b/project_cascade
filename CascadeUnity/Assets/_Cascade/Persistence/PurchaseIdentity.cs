using System;
using System.Security.Cryptography;
using System.Text;

namespace Cascade.Persistence
{
    public sealed class PurchaseIdentity
    {
        public string AppleToken,GoogleAccountId;
        public static PurchaseIdentity For(string project,string environment,string player)
        {
            if(string.IsNullOrWhiteSpace(project)||string.IsNullOrWhiteSpace(environment)||string.IsNullOrWhiteSpace(player))throw new ArgumentException("Authenticated purchase context is required.");
            string hex;
            using(var hash=SHA256.Create())hex=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes("cascade-purchase-v1|"+project+"|"+environment+"|"+player))).Replace("-","").ToLowerInvariant();
            return new PurchaseIdentity{GoogleAccountId=hex,AppleToken=hex.Substring(0,8)+"-"+hex.Substring(8,4)+"-"+hex.Substring(12,4)+"-"+hex.Substring(16,4)+"-"+hex.Substring(20,12)};
        }
    }
}
