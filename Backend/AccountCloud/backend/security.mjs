import { createPublicKey, verify } from 'node:crypto';
import { Failure } from './store.mjs';
const keys=new Map();
async function cachedKeys(url,ttl,fetcher){const now=Date.now();let c=keys.get(url);if(c&&now-c.at<ttl)return c.value;const response=await fetcher(url,{signal:AbortSignal.timeout(8000)});if(!response.ok)throw new Failure(503,'VerificationKeysUnavailable');const value=await response.json();keys.set(url,{at:now,value});return value;}
export async function verifySSV(url,fetcher=fetch){
 const raw=new URL(url).search.slice(1);if(raw.length>8192)throw new Failure(400,'InvalidSignature');
 const decoded=decodeURIComponent(raw),match=/^(.+)&signature=([A-Za-z0-9_-]+={0,2})&key_id=(\d+)$/.exec(decoded);
 if(!match)throw new Failure(403,'InvalidSignature');
 const p=new URLSearchParams(raw);for(const name of p.keys())if(p.getAll(name).length!==1)throw new Failure(400,'DuplicateParameter');
 const endpoint='https://www.gstatic.com/admob/reward/verifier-keys.json';let data=await cachedKeys(endpoint,12*3600000,fetcher),key=data.keys?.find(k=>String(k.keyId)===match[3]);
 if(!key){const c=keys.get(endpoint);if(c&&Date.now()-c.at>60000){keys.delete(endpoint);data=await cachedKeys(endpoint,12*3600000,fetcher);key=data.keys?.find(k=>String(k.keyId)===match[3]);}}
 if(!key||![match[1],match[1].replaceAll('+',' ')].some(text=>verify('sha256',Buffer.from(text,'utf8'),createPublicKey(key.pem),Buffer.from(match[2],'base64url'))))throw new Failure(403,'InvalidSignature');
 return Object.fromEntries(p);
}
export async function verifyUnityToken(token,project,fetcher=fetch){
 if(typeof token!=='string'||token.length>8192)throw new Failure(401,'AuthenticationRequired');const parts=token.split('.');if(parts.length!==3)throw new Failure(401,'InvalidToken');
 let h,p;try{h=JSON.parse(Buffer.from(parts[0],'base64url'));p=JSON.parse(Buffer.from(parts[1],'base64url'));}catch{throw new Failure(401,'InvalidToken');}
 if(h.alg!=='RS256'||typeof h.kid!=='string')throw new Failure(401,'InvalidToken');
 const endpoint='https://player-auth.services.api.unity.com/.well-known/jwks.json';let data=await cachedKeys(endpoint,8*3600000,fetcher),jwk=data.keys?.find(k=>k.kid===h.kid);
 if(!jwk){const c=keys.get(endpoint);if(c&&Date.now()-c.at>60000){keys.delete(endpoint);data=await cachedKeys(endpoint,8*3600000,fetcher);jwk=data.keys?.find(k=>k.kid===h.kid);}}
 const now=Date.now()/1000;
 if(!jwk||!verify('RSA-SHA256',Buffer.from(parts[0]+'.'+parts[1]),createPublicKey({key:jwk,format:'jwk'}),Buffer.from(parts[2],'base64url'))||p.iss!=='https://player-auth.services.api.unity.com'||p.project_id!==project||!Number.isFinite(p.exp)||p.exp<=now||p.nbf>now||typeof p.sub!=='string'||!/^[A-Za-z0-9_-]{1,128}$/.test(p.sub))throw new Failure(401,'InvalidToken');return p.sub;
}
