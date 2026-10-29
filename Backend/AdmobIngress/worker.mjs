import {HttpFailure,boundedJson,sha} from './common.mjs';
import {createSignatures} from './signatures.mjs';
const PROJECT='a1b8a760-5a0c-4c2d-8ae7-54c1b411259a',ENVIRONMENT='2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7',PREFIX='/api/flying-ladders',BUNDLE='com.vickand.flyingladders';
const headers={'Content-Type':'application/json','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'};
const reply=(data,status=200)=>new Response(JSON.stringify(data),{status,headers:status===429?{...headers,'Retry-After':'60'}:headers});
export function createHandler(fetcher=fetch,checks=createSignatures(fetcher)){
 let authorizationCache,healthCache;
 async function upstream(address,options,timeout,phase){try{return await fetcher(address,{...options,redirect:'manual',signal:AbortSignal.timeout(timeout)});}catch(error){if(['TimeoutError','AbortError'].includes(error?.name))throw new HttpFailure(503,phase+'Timeout');throw error;}}
 async function authorize(env){
  const key=env.UNITY_SERVICE_KEY_ID,secret=env.UNITY_SERVICE_SECRET;if(typeof key!=='string'||typeof secret!=='string'||!key||!secret)throw new HttpFailure(503,'GatewayNotConfigured');
  // Rotation of either half invalidates the cache. Only completed tokens live
  // across requests; failed/in-flight I/O and player state are never shared.
  const scope=await sha(key+':'+secret);
  if(authorizationCache?.scope===scope&&authorizationCache.until>Date.now())return authorizationCache.token;
  const response=await upstream('https://services.api.unity.com/auth/v1/token-exchange?projectId='+PROJECT+'&environmentId='+ENVIRONMENT,{method:'POST',headers:{Authorization:'Basic '+btoa(key+':'+secret),'Content-Type':'application/json'},body:'{}'},6000,'UnityAuthentication');
  if(!response.ok)throw new HttpFailure(503,'UnityAuthenticationPending');let result;try{result=await boundedJson(response,32768);}catch{throw new HttpFailure(503,'UnityAuthenticationPending');}
  if(typeof result.accessToken!=='string'||!result.accessToken||result.accessToken.length>16384)throw new HttpFailure(503,'UnityAuthenticationPending');
  authorizationCache={scope,token:result.accessToken,until:Date.now()+120000};return result.accessToken;
 }
 async function unity(env,endpoint,params){
  for(let attempt=0;;attempt++){
   const token=await authorize(env),response=await upstream('https://cloud-code.services.api.unity.com/v1/projects/'+PROJECT+'/modules/CascadeAccount/'+endpoint,{method:'POST',headers:{Authorization:'Bearer '+token,'Content-Type':'application/json',UnityEnvironment:ENVIRONMENT},body:JSON.stringify({params})},20000,'UnityOperation');
   if(response.status===401&&attempt===0){await response.body?.cancel();if(authorizationCache?.token===token)authorizationCache=undefined;continue;}
   let data;try{data=await boundedJson(response,65536);}catch{throw new HttpFailure(503,'UnityResponseInvalid');}
   if(!response.ok){
    if(response.status===422&&/Invalid reward signature|Unknown reward signing key|Invalid AdMob callback|Duplicate reward field|Invalid reward notification|Reward notification scope mismatch|Reward notification reused|Reward intent reused/.test(JSON.stringify(data)))throw new HttpFailure(403,'InvalidRewardCallback');
    throw new HttpFailure(503,'UnityOperationPending');
   }
   const output=data.output??data.result??data;if(output===null||typeof output!=='object'||Array.isArray(output))throw new HttpFailure(503,'UnityResponseInvalid');return output;
  }
 }
 async function limit(binding,key){if(!binding?.limit)throw new HttpFailure(503,'RateProtectionNotConfigured');if(!(await binding.limit({key})).success)throw new HttpFailure(429,'RetryLater');}
 async function json(request,size){if(!request.headers.get('content-type')?.startsWith('application/json'))throw new HttpFailure(415,'ExpectedJSON');return boundedJson(request,size);}
 const handle=async function handle(request,env){
  const started=Date.now();let route='unknown';
  try{
   const url=new URL(request.url),path=url.pathname.replace(/\/$/,'');
   if(url.hostname!=='vickand.com'||url.protocol!=='https:'||!path.startsWith(PREFIX+'/'))return reply({error:'NotFound'},404);route=path.slice(PREFIX.length);
   if(!['/admob/ssv','/health','/notifications/apple','/notifications/google','/purchase/local-verify','/deletion/confirm'].includes(route))return reply({error:'NotFound'},404);
   const method=['/admob/ssv','/health'].includes(route)?'GET':'POST';if(request.method!==method)return new Response(JSON.stringify({error:'MethodNotAllowed'}),{status:405,headers:{...headers,Allow:method}});
   if(url.search.length>8192)return reply({error:'RequestBudget'},414);
   if(route==='/admob/ssv'){
    if(!url.searchParams.get('signature')||!/^\d+$/.test(url.searchParams.get('key_id')??''))return reply({error:'SignedCallbackRequired'},400);
    await checks.ssv(url);const result=await unity(env,'UnityAdmobNotification',{callbackUrl:request.url});
    if(!(result.Accepted===true||result.accepted===true||result.Verified===true&&result.Test===true||result.verified===true&&result.test===true))throw new HttpFailure(503,'UnityAcknowledgementPending');return reply(result);
   }
   if(route==='/notifications/apple'||route==='/notifications/google'){
    const data=await json(request,131072),event=await checks[route.endsWith('apple')?'apple':'google'](...(route.endsWith('apple')?[data,env]:[request,env,data]));
    if(event.test||event.ignored)return reply(event);const result=await unity(env,'UnitySubscriptionNotification',{chain:event.chain});
    if(!(result.Accepted===true||result.accepted===true||result.Ignored===true||result.ignored===true))throw new HttpFailure(503,'UnityAcknowledgementPending');return reply(result);
   }
   if(route==='/health'){
    // Only operational health is briefly cached; no wallet/purchase result is.
    if(healthCache?.until>Date.now())return reply(healthCache.value,healthCache.status);
    const result=await unity(env,'BackendStatus',{}),read=name=>result[name]??result[name[0].toLowerCase()+name.slice(1)];
    const value={service:'flying-ladders-ingress',backend:'UnityCloudCode',storage:'UnityCloudSave',version:read('Version'),ready:read('Ready')===true,healthy:read('Healthy')===true,maintenanceFresh:read('MaintenanceFresh')===true};
    const status=value.ready&&value.healthy?200:503;healthCache={value,status,until:Date.now()+(status===200?30000:5000)};return reply(value,status);
   }
   // Only unauthenticated expensive interactive routes use IP abuse controls.
   // Trusted signed publisher callbacks are never throttled by a shared IP.
   let ip=request.headers.get('cf-connecting-ip');
   const relay=request.headers.get('x-flying-relay');if(relay){if(typeof env.LEGACY_RELAY_TOKEN!=='string'||await sha(relay)!==await sha(env.LEGACY_RELAY_TOKEN))throw new HttpFailure(403,'RelayAuthentication');const forwarded=request.headers.get('x-flying-client-ip');if(!forwarded||forwarded.length>64||!/^[0-9a-fA-F:.]+$/.test(forwarded))throw new HttpFailure(403,'RelayIdentity');ip=forwarded;}
   if(!ip)throw new HttpFailure(503,'EdgeIdentityRequired');
   await limit(env.PUBLIC_BUDGET,await sha(route+'|'+ip));
   if(route==='/purchase/local-verify'){
    const data=await json(request,1536*1024),allowed=['productId','transactionId','receipt','jws','bundleId'];
    if(!data||Array.isArray(data)||Object.keys(data).some(key=>!allowed.includes(key))||typeof data.productId!=='string'||data.productId.length>128||typeof data.transactionId!=='string'||data.transactionId.length>512||data.bundleId!==BUNDLE||!data.receipt&&!data.jws||['receipt','jws'].some(k=>data[k]!=null&&typeof data[k]!=='string'))throw new HttpFailure(400,'InvalidPurchaseFields');
    await limit(env.PROOF_BUDGET,await sha(data.productId+'|'+data.transactionId));
    return reply(await unity(env,'UnityVerifyLocalPurchase',Object.fromEntries(allowed.map(key=>[key,data[key]??'']))));
   }
   if(![url.origin,'https://flyingladders.vickand.com','https://cascade-sky-gardens.active-seed-4233.chatgpt.site'].includes(request.headers.get('origin')))throw new HttpFailure(403,'InvalidOrigin');
   const data=await json(request,1024);if(!/^[A-Za-z0-9_-]{32}$/.test(data?.code??'')||data?.confirm!=='DELETE')throw new HttpFailure(400,'InvalidDeletionCode');
   return reply(await unity(env,'UnityRedeemDeletion',{code:data.code,confirmation:data.confirm}),202);
  }catch(error){
   const status=error instanceof HttpFailure?error.status:503;
   // Fixed codes and timings only: no raw URLs, payloads, player IDs or secrets.
   if(status>=500)console.warn(JSON.stringify({event:'backend_ingress_pending',route,status,type:String(error?.name??'Error').slice(0,40),code:error instanceof HttpFailure?error.message:'ServiceUnavailable',elapsed_ms:Date.now()-started}));
   return reply({error:error instanceof HttpFailure?error.message:'ServiceUnavailable'},status);
  }
 };
 return handle;
}
const service=createHandler();
export default {fetch:service};

