import {loadAppleSdk} from '../AccountCloud/backend/sdk.mjs';
import {HttpFailure,boundedJson,un64,sha} from './common.mjs';
const bundle='com.vickand.flyingladders';
export function createSignatures(fetcher=fetch){
 let jwks,ssvKeys,jwksAt=0,ssvAt=0;const appleVerifiers=new Map();
 async function getKeys(kind,force=false){
  const isGoogle=kind==='google',now=Date.now();let value=isGoogle?jwks:ssvKeys,at=isGoogle?jwksAt:ssvAt;
  if(value&&!force&&now-at<3600000)return value;
  // Unknown keys cannot trigger an outbound fetch on every malicious request.
  if(value&&now-at<60000)return value;
  const response=await fetcher(isGoogle?'https://www.googleapis.com/oauth2/v3/certs':'https://www.gstatic.com/admob/reward/verifier-keys.json',{redirect:'manual',signal:AbortSignal.timeout(6000)});
  if(!response.ok)throw new HttpFailure(503,'VerificationKeysUnavailable');
  value=await boundedJson(response,65536);if(!Array.isArray(value?.keys)||value.keys.length>32)throw new HttpFailure(503,'VerificationKeysInvalid');
  if(isGoogle){jwks=value;jwksAt=now;}else{ssvKeys=value;ssvAt=now;}return value;
 }
 async function google(request,env,data){
  const token=/^Bearer ([A-Za-z0-9_.-]+)$/.exec(request.headers.get('authorization')??'')?.[1];if(!token||token.length>16384)throw new HttpFailure(401,'PushAuthenticationRequired');
  let header,claims,parts=token.split('.');try{if(parts.length!==3)throw 0;header=JSON.parse(new TextDecoder().decode(un64(parts[0])));claims=JSON.parse(new TextDecoder().decode(un64(parts[1])));}catch{throw new HttpFailure(401,'PushAuthenticationInvalid');}
  const now=Math.floor(Date.now()/1000);
  if(header.alg!=='RS256'||typeof header.kid!=='string'||!['accounts.google.com','https://accounts.google.com'].includes(claims.iss)||claims.aud!==env.GOOGLE_PUSH_AUDIENCE||claims.email!==env.GOOGLE_PUSH_EMAIL||claims.email_verified!==true||!Number.isFinite(claims.exp)||claims.exp<now-30||claims.exp>now+7200||!Number.isFinite(claims.iat)||claims.iat>now+30||claims.iat<now-7200)throw new HttpFailure(403,'PushIdentity');
  let keys=await getKeys('google'),key=keys.keys.find(k=>k.kid===header.kid&&k.kty==='RSA');if(!key){keys=await getKeys('google',true);key=keys.keys.find(k=>k.kid===header.kid&&k.kty==='RSA');}if(!key)throw new HttpFailure(401,'PushSigningKey');
  let verified=false;try{const imported=await crypto.subtle.importKey('jwk',key,{name:'RSASSA-PKCS1-v1_5',hash:'SHA-256'},false,['verify']);verified=await crypto.subtle.verify('RSASSA-PKCS1-v1_5',imported,un64(parts[2]),new TextEncoder().encode(parts[0]+'.'+parts[1]));}catch{}
  if(!verified)throw new HttpFailure(403,'PushSignature');
  let event;try{if(typeof data?.message?.data!=='string'||data.message.data.length>100000)throw 0;event=JSON.parse(new TextDecoder().decode(un64(data.message.data)));}catch{throw new HttpFailure(400,'PushData');}
  if(event.packageName!==bundle)throw new HttpFailure(403,'NotificationScope');if(event.testNotification)return {test:true};
  const purchase=event.subscriptionNotification?.purchaseToken??event.voidedPurchaseNotification?.purchaseToken;
  if(purchase==null)return {ignored:true};if(typeof purchase!=='string'||purchase.length>16384)throw new HttpFailure(400,'PurchaseTokenBudget');
  return {chain:'chain|GooglePlay|'+await sha(purchase)};
 }
 async function apple(data,env){
  if(typeof data?.signedPayload!=='string'||data.signedPayload.length>131000)throw new HttpFailure(400,'SignedPayloadRequired');
  // Unverified environment only selects which verifier must authenticate it.
  let hint;try{hint=JSON.parse(new TextDecoder().decode(un64(data.signedPayload.split('.')[1])))?.data?.environment;}catch{throw new HttpFailure(403,'NotificationSignature');}
  if(!['Sandbox','Production'].includes(hint)||typeof env.APPLE_ROOT_CA_BASE64!=='string')throw new HttpFailure(403,'NotificationEnvironment');
  const key=hint+'|'+env.APPLE_ROOT_CA_BASE64;let verifier=appleVerifiers.get(key);
  if(!verifier){const {SignedDataVerifier,Environment}=loadAppleSdk();verifier=new SignedDataVerifier(env.APPLE_ROOT_CA_BASE64.split(',').map(v=>Buffer.from(v,'base64')),true,hint==='Production'?Environment.PRODUCTION:Environment.SANDBOX,bundle,hint==='Production'?6824290826:undefined);if(appleVerifiers.size>=4)appleVerifiers.clear();appleVerifiers.set(key,verifier);}
  let event,tx;try{event=await verifier.verifyAndDecodeNotification(data.signedPayload);if(event.notificationType!=='TEST')tx=await verifier.verifyAndDecodeTransaction(event.data?.signedTransactionInfo);}catch(error){
   // Certificate network failures remain retryable, never acknowledge an event.
   if(error?.status===2||/timeout|fetch|network|ECONN|OCSP/i.test(String(error?.message??'')))throw new HttpFailure(503,'CertificateCheckPending');
   throw new HttpFailure(403,'NotificationSignature');
  }
  if(event.data?.bundleId!==bundle||event.data?.environment!==hint)throw new HttpFailure(403,'NotificationScope');if(event.notificationType==='TEST')return {test:true};
  if(tx?.bundleId!==bundle||tx?.environment!==hint||typeof tx.originalTransactionId!=='string'||!tx.originalTransactionId||tx.originalTransactionId.length>128)throw new HttpFailure(403,'NotificationScope');
  return {chain:'chain|AppleAppStore|'+await sha(tx.originalTransactionId)};
 }
 async function ssv(url){
  const raw=url.search.slice(1),match=/^(.+)&signature=([A-Za-z0-9_%-]+={0,2})&key_id=(\d+)$/.exec(raw);if(!match)throw new HttpFailure(400,'SignedCallbackRequired');
  const fields=new Set();for(const[key]of url.searchParams){if(fields.has(key))throw new HttpFailure(400,'DuplicateRewardField');fields.add(key);}
  let keys=await getKeys('ssv'),key=keys.keys.find(k=>String(k.keyId)===match[3]);if(!key){keys=await getKeys('ssv',true);key=keys.keys.find(k=>String(k.keyId)===match[3]);}if(!key)throw new HttpFailure(403,'RewardSigningKey');
  try{
   const {createPublicKey,verify}=await import('node:crypto');const publicKey=createPublicKey(key.pem),signature=un64(decodeURIComponent(match[2]));
   if(![decodeURIComponent(match[1]),decodeURIComponent(match[1].replace(/\+/g,' '))].some(text=>verify('sha256',Buffer.from(text),publicKey,signature)))throw 0;
  }catch{throw new HttpFailure(403,'InvalidRewardCallback');}
 }
 return {google,apple,ssv};
}
