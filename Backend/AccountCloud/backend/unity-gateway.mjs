import {Failure} from './store.mjs';
import {loadAppleSdk,loadGoogleSdk} from './sdk.mjs';
let credentialToken;
const required=(env,...keys)=>{if(keys.some(key=>!env[key]))throw new Failure(503,'UnityGatewayNotConfigured');};
async function token(env,fetcher){
 required(env,'UGS_PROJECT_ID','UGS_ENVIRONMENT_ID','UGS_SERVICE_KEY_ID','UGS_SERVICE_SECRET');
 const key=env.UGS_SERVICE_KEY_ID+'|'+env.UGS_ENVIRONMENT_ID;
 if(credentialToken?.key===key&&credentialToken.until>Date.now())return credentialToken.value;
 const auth='Basic '+Buffer.from(env.UGS_SERVICE_KEY_ID+':'+env.UGS_SERVICE_SECRET).toString('base64');
 const response=await fetcher('https://services.api.unity.com/auth/v1/token-exchange?projectId='+encodeURIComponent(env.UGS_PROJECT_ID)+'&environmentId='+encodeURIComponent(env.UGS_ENVIRONMENT_ID),{method:'POST',headers:{Authorization:auth,'Content-Type':'application/json'},body:'{}',signal:AbortSignal.timeout(6000)});
 if(!response.ok)throw new Failure(503,'UnityGatewayAuthentication');const data=await response.json();if(typeof data.accessToken!=='string')throw new Failure(503,'UnityGatewayAuthentication');
 credentialToken={key,value:data.accessToken,until:Date.now()+120000};return data.accessToken;
}
export async function unityCall(env,name,params,fetcher=fetch){
 const bearer=await token(env,fetcher);
 const response=await fetcher('https://cloud-code.services.api.unity.com/v1/projects/'+encodeURIComponent(env.UGS_PROJECT_ID)+'/modules/CascadeAccount/'+name,{method:'POST',headers:{Authorization:'Bearer '+bearer,'Content-Type':'application/json',UnityEnvironment:env.UGS_ENVIRONMENT_ID},body:JSON.stringify({params}),signal:AbortSignal.timeout(14000)});
 if(!response.ok)throw new Failure(response.status===401?503:response.status===422?400:503,'UnityOperationPending');
 const result=await response.json();const output=result.output??result.result??result;return Object.fromEntries(Object.entries(output).map(([key,value])=>[key[0].toUpperCase()+key.slice(1),value]));
}
export async function unityNotification(env,kind,request,fetcher=fetch){
 let chain;
 if(kind==='apple'){
  required(env,'APPLE_ROOT_CA_BASE64','APPLE_ENVIRONMENT','BUNDLE_ID');const {SignedDataVerifier,Environment}=loadAppleSdk();
  const apple=new SignedDataVerifier(env.APPLE_ROOT_CA_BASE64.split(',').map(s=>Buffer.from(s,'base64')),true,env.APPLE_ENVIRONMENT==='Production'?Environment.PRODUCTION:Environment.SANDBOX,env.BUNDLE_ID,env.APPLE_ENVIRONMENT==='Production'?Number(env.APPLE_APP_ID):undefined);
  const data=await request.json(),event=await apple.verifyAndDecodeNotification(data.signedPayload);
  if(event.data?.bundleId!==env.BUNDLE_ID||event.data?.environment!==env.APPLE_ENVIRONMENT)throw new Failure(403,'NotificationScope');if(event.notificationType==='TEST')return {test:true};
  const tx=await apple.verifyAndDecodeTransaction(event.data?.signedTransactionInfo);if(tx.bundleId!==env.BUNDLE_ID||!tx.originalTransactionId)throw new Failure(403,'NotificationScope');
  const {createHash}=await import('node:crypto');chain='chain|AppleAppStore|'+createHash('sha256').update(tx.originalTransactionId).digest('hex');
 }else{
  required(env,'GOOGLE_PUSH_AUDIENCE','GOOGLE_PUSH_EMAIL','BUNDLE_ID');const bearer=/^Bearer (.+)$/.exec(request.headers.get('authorization')??'')?.[1];if(!bearer)throw new Failure(401,'PushAuthenticationRequired');
  const {OAuth2Client}=loadGoogleSdk();const claims=(await new OAuth2Client({transporterOptions:{fetchImplementation:fetcher,timeout:8000}}).verifyIdToken({idToken:bearer,audience:env.GOOGLE_PUSH_AUDIENCE})).getPayload();
  if(!['accounts.google.com','https://accounts.google.com'].includes(claims?.iss)||claims.email!==env.GOOGLE_PUSH_EMAIL||claims.email_verified!==true)throw new Failure(403,'PushIdentity');
  const push=await request.json(),event=JSON.parse(Buffer.from(push.message?.data??'','base64').toString('utf8'));if(event.packageName!==env.BUNDLE_ID)throw new Failure(403,'NotificationScope');if(event.testNotification)return {test:true};
  const purchase=event.subscriptionNotification?.purchaseToken??event.voidedPurchaseNotification?.purchaseToken;if(!purchase)return {ignored:true};
  const {createHash}=await import('node:crypto');chain='chain|GooglePlay|'+createHash('sha256').update(purchase).digest('hex');
 }
 // Return 2xx only after Unity persisted reconciliation. Publishers retry failures.
 return unityCall(env,'UnitySubscriptionNotification',{chain},fetcher);
}
export async function unityGateway(request,env,path,readBody,fetcher=fetch){
 if(path==='/health'&&request.method==='GET'){
  const status=await unityCall(env,'UnityMigrationStatus',{},fetcher);return {service:'flying-ladders-unity-gateway',backend:'UnityCloudCode',storage:'UnityCloudSave',ready:status.Mode==='Unity',migration:status.Manifest?.Status};
 }
 if(path==='/admob/ssv'&&request.method==='GET')return unityCall(env,'UnityAdmobNotification',{callbackUrl:request.url},fetcher);
 if(['/notifications/apple','/notifications/google'].includes(path)&&request.method==='POST'){
  const data=await readBody(request,131072);return unityNotification(env,path.endsWith('apple')?'apple':'google',new Request(request.url,{method:'POST',headers:request.headers,body:JSON.stringify(data)}),fetcher);
 }
 if(path==='/purchase/local-verify'&&request.method==='POST'){
  const data=await readBody(request,1536*1024),allowed=['productId','transactionId','receipt','jws','bundleId'];
  if(!data||Array.isArray(data)||Object.keys(data).some(key=>!allowed.includes(key))||typeof data.productId!=='string'||typeof data.transactionId!=='string'||data.bundleId!==env.BUNDLE_ID||!data.receipt&&!data.jws)throw new Failure(400,'InvalidPurchaseFields');
  return unityCall(env,'UnityVerifyLocalPurchase',Object.fromEntries(allowed.map(key=>[key,data[key]??''])),fetcher);
 }
 if(path==='/deletion/confirm'&&request.method==='POST'){
  if(request.headers.get('origin')!==new URL(request.url).origin)throw new Failure(403,'InvalidOrigin');const data=await readBody(request,1024);
  return unityCall(env,'UnityRedeemDeletion',{code:data.code,confirmation:data.confirm},fetcher);
 }
 throw new Failure(410,'GameStateMovedToUnity');
}
