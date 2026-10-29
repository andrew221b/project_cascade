using System;
using System.Text;
using System.Threading.Tasks;
using Cascade.Application;
using UnityEngine;
using UnityEngine.Networking;

namespace Cascade.Presentation
{
    public sealed class PublicReceiptVerifier
    {
        const int MaxRequestBytes=2*1024*1024,MaxResponseBytes=4096;
        readonly string endpoint;

        [Serializable] sealed class RequestBody {public string productId,transactionId,receipt,jws,bundleId;}
        [Serializable] sealed class ResponseBody
        {
            public bool Valid,CatalogVerified;
            public string ProductId,TransactionId,Platform,BundleId;
            public long SubscriptionExpiresUnix;
        }

        public PublicReceiptVerifier(string url){endpoint=ValidEndpoint(url)?url:null;}
        public bool Configured=>endpoint!=null;
        public static bool ValidEndpoint(string value)
        {
            if(!Uri.TryCreate(value,UriKind.Absolute,out var uri)||uri.Scheme!=Uri.UriSchemeHttps||string.IsNullOrEmpty(uri.Host)
                ||!string.IsNullOrEmpty(uri.UserInfo)||!string.IsNullOrEmpty(uri.Query)||!string.IsNullOrEmpty(uri.Fragment))return false;
            return uri.AbsolutePath=="/api/purchase/local-verify"
                ||uri.Host=="vickand.com"&&uri.AbsolutePath=="/api/flying-ladders/purchase/local-verify";
        }

        public async Task<NativePurchaseStore.Verification> VerifyAsync(NativePurchaseStore.Proof proof)
        {
            if(!Configured||proof==null||!PurchaseCatalog.IsKnown(proof.productId)||string.IsNullOrWhiteSpace(proof.transactionId)||proof.transactionId.Length>256
                ||proof.bundleId!=UnityEngine.Application.identifier||proof.store!="AppleAppStore"&&proof.store!="GooglePlay"
                ||proof.store=="AppleAppStore"&&string.IsNullOrWhiteSpace(proof.jws)||proof.store=="GooglePlay"&&string.IsNullOrWhiteSpace(proof.receipt))return null;
            var body=new RequestBody{productId=proof.productId,transactionId=proof.transactionId,receipt=proof.receipt,jws=proof.jws,bundleId=proof.bundleId};
            var bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(body));if(bytes.Length>MaxRequestBytes)return null;
            using(var request=new UnityWebRequest(endpoint,UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler=new UploadHandlerRaw(bytes);request.downloadHandler=new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Accept","application/json");
                request.timeout=35;request.redirectLimit=0;
                var operation=request.SendWebRequest();while(!operation.isDone)await Task.Yield();
                if(request.result!=UnityWebRequest.Result.Success||request.responseCode!=200)return null;
                var text=request.downloadHandler.text;if(string.IsNullOrWhiteSpace(text)||Encoding.UTF8.GetByteCount(text)>MaxResponseBytes)return null;
                ResponseBody result;try{result=JsonUtility.FromJson<ResponseBody>(text);}catch(Exception){return null;}
                if(result==null||!result.Valid||!result.CatalogVerified||result.ProductId!=proof.productId||result.TransactionId!=proof.transactionId
                    ||result.Platform!=proof.store||result.BundleId!=proof.bundleId||result.SubscriptionExpiresUnix<0||result.SubscriptionExpiresUnix>253402300799L)return null;
                return new NativePurchaseStore.Verification{Valid=true,SubscriptionExpiresUnix=result.SubscriptionExpiresUnix};
            }
        }
    }
}
