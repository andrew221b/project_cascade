import { Store,Failure,digest } from './store.mjs';
import { authorizeService,verifyPurchase,verifyServicePurchase } from './verify.mjs';
import { verifySSV } from './security.mjs';
import { publishers } from './publishers.mjs';
import {loadCredentials,importCredentials} from './credential-import.mjs';
import {unityGateway} from './unity-gateway.mjs';
import {migration} from './unity-migration.mjs';
const headers={'Content-Type':'application/json','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'};
const json=(value,status=200)=>new Response(JSON.stringify(value),{status,headers});
export async function cleanup(store,env,fetcher=fetch){
 if(!env.UGS_SERVICE_KEY_ID||!env.UGS_SERVICE_SECRET)throw new Failure(503,'DeletionCredentialsMissing');
 const environments=(env.UGS_DELETION_ENVIRONMENT_IDS??'').split(',').filter(Boolean);if(!environments.length||environments.some(id=>!/^[-a-f0-9]{36}$/i.test(id)))throw new Failure(503,'DeletionEnvironmentsMissing');
 const auth='Basic '+Buffer.from(env.UGS_SERVICE_KEY_ID+':'+env.UGS_SERVICE_SECRET).toString('base64');
 let failure;
 for(const job of await store.all('SELECT * FROM deletions WHERE done=0 LIMIT 10')){try{
  for(const environment of environments){
  const base='https://services.api.unity.com/cloud-save/v1/data/projects/'+encodeURIComponent(env.UGS_PROJECT_ID)+'/environments/'+encodeURIComponent(environment)+'/players/'+encodeURIComponent(job.player);
  for(const path of ['/items','/public/items','/protected/items','/private/items']){const r=await fetcher(base+path,{method:'DELETE',headers:{Authorization:auth,'UnityEnvironment':environment},signal:AbortSignal.timeout(8000)});if(![200,204,404].includes(r.status))throw new Failure(503,'CloudDataDeletionPending');}
  for(const id of JSON.parse(job.legacy_ids??'[]')){
   if(!/^iap_[a-f0-9]{46}$/.test(id))throw new Failure(503,'InvalidLegacyOwnership');
   const url='https://services.api.unity.com/cloud-save/v1/data/projects/'+encodeURIComponent(env.UGS_PROJECT_ID)+'/environments/'+encodeURIComponent(environment)+'/custom/'+id+'/private/items';
   const found=await fetcher(url+'?keys=owner',{headers:{Authorization:auth},signal:AbortSignal.timeout(8000)});if(found.status===404)continue;if(!found.ok)throw new Failure(503,'OwnershipCleanupPending');
   const item=(await found.json()).results?.find(i=>i.key==='owner');if(item?.value?.PlayerId!==job.player)continue;
   const removed=await fetcher(url+'/owner?writeLock='+encodeURIComponent(item.writeLock),{method:'DELETE',headers:{Authorization:auth},signal:AbortSignal.timeout(8000)});if(![200,204,404].includes(removed.status))throw new Failure(503,'OwnershipCleanupPending');
  }
  }
  const r=await fetcher('https://services.api.unity.com/player-identity/v1/projects/'+encodeURIComponent(env.UGS_PROJECT_ID)+'/users/'+encodeURIComponent(job.player),{method:'DELETE',headers:{Authorization:auth},signal:AbortSignal.timeout(8000)});if(![200,204,404].includes(r.status))throw new Failure(503,'IdentityDeletionPending');
  // Remove the actual player ID after the deletion completes; keep the keyed tombstone only.
  await store.run('DELETE FROM deletions WHERE owner=?',job.owner);
 }catch(error){failure??=error;}
 }
 if(failure)throw failure;
 return {Status:'Complete'};
}
export async function handle(request,env,dependencies={}){
 const path=new URL(request.url).pathname.replace(/^\/api/,'');
 try{
  if(path==='/admin/import-credentials')return json(await importCredentials(request,env));
  if(env.UNITY_BACKEND_PHASE==='unity'&&!path.startsWith('/admin/unity-migration/'))return json(await unityGateway(request,env,path,body,dependencies.fetch??fetch));
  env=await loadCredentials(env);
  if(path.startsWith('/admin/unity-migration/'))return json(await migration(request,env,path,body));
  if(env.UNITY_BACKEND_PHASE==='frozen'&&path!=='/health')throw new Failure(503,'UnityMigrationInProgress');
  if(path==='/health'&&request.method==='GET'){const storage=env.DB?!!(await env.DB.prepare('SELECT 1 AS ok FROM sqlite_master LIMIT 1').first()):false;return json({storage,service:'cascade-account-cloud',ready:!!(env.DB&&env.SERVICE_BEARER_TOKEN&&env.DATA_KEY&&env.UGS_ENVIRONMENT_ID),publishersConfigured:!!(env.BUNDLE_ID&&env.APPLE_PRIVATE_KEY&&env.GOOGLE_CREDENTIALS_JSON),deletionConfigured:!!(env.UGS_SERVICE_KEY_ID&&env.UGS_SERVICE_SECRET)});}
  const store=dependencies.store??new Store(env.DB,env.DATA_KEY);let publisher;const getPublisher=async()=>publisher??=(dependencies.publishers??await publishers(env,store));
  if(path==='/admob/ssv'&&request.method==='GET'){
   const event=await verifySSV(request.url,dependencies.fetch??fetch);
   // AdMob console verifies callbacks with a synthetic unit, not a served ad.
   // A verified console probe acknowledges reachability only; it never grants.
   if(event.ad_unit==='1234567890')return json({verified:true,test:true});
   await store.confirm(event,(env.ADMOB_REWARDED_UNITS??'').split(',').filter(Boolean));return json({accepted:true});
  }
  if(['/notifications/apple','/notifications/google'].includes(path)&&request.method==='POST'){const data=await body(request,131072);const bounded=new Request(request.url,{method:'POST',headers:request.headers,body:JSON.stringify(data)});return json(await (await getPublisher()).notification(path.endsWith('apple')?'apple':'google',bounded));}
  if(path==='/deletion/confirm'&&request.method==='POST'){
   if(request.headers.get('origin')!==new URL(request.url).origin)throw new Failure(403,'InvalidOrigin');
   const data=await body(request,1024);if(data.confirm!=='DELETE')throw new Failure(400,'ConfirmationRequired');const result=await store.redeemDelete(data.code,env.BUNDLE_ID);try{await cleanup(store,env,dependencies.fetch??fetch);}catch{/* Durable job remains; no false completion. */}return json(result,202);
  }
  if(path==='/purchase/local-verify'&&request.method==='POST'){
   const data=await body(request,2*1024*1024),allowed=['productId','transactionId','receipt','jws','bundleId'];
   if(!data||typeof data!=='object'||Array.isArray(data)||Object.keys(data).some(key=>!allowed.includes(key))
      ||typeof data.productId!=='string'||typeof data.transactionId!=='string'||typeof data.bundleId!=='string'
      ||data.receipt!=null&&typeof data.receipt!=='string'||data.jws!=null&&typeof data.jws!=='string')throw new Failure(400,'InvalidPurchaseFields');
   const hasJws=!!data.jws,hasReceipt=!!data.receipt;if(!hasJws&&!hasReceipt)throw new Failure(400,'MissingPurchaseEvidence');
   const proof={...data,store:hasJws?'AppleAppStore':'GooglePlay'},purchase=await verifyPurchase(proof,{...(await getPublisher()),bundleId:env.BUNDLE_ID});
   if(!purchase.valid)return json({Valid:false});
   return json({Valid:true,CatalogVerified:true,ProductId:purchase.productId,TransactionId:purchase.transactionId,Platform:proof.store,BundleId:env.BUNDLE_ID,
    ...(purchase.expiresUnix!==undefined?{SubscriptionExpiresUnix:purchase.expiresUnix}:{})});
  }
  if(request.method!=='POST')return json({error:'NotFound'},404);
  if(!authorizeService(request.headers.get('authorization'),env.SERVICE_BEARER_TOKEN))return json({error:'Unauthorized'},401);
  const data=await body(request,2*1024*1024);
  if(data.projectId!==env.UGS_PROJECT_ID||data.environmentId!==env.UGS_ENVIRONMENT_ID||typeof data.playerId!=='string'||!/^[A-Za-z0-9_-]{1,128}$/.test(data.playerId))throw new Failure(403,'ContextMismatch');
  const player=data.playerId;
  if(path==='/account/read')return json(await store.read(player));
  if(path==='/account/write')return json(await store.write(player,data.version,data.payload));
  if(path==='/purchase/claim'){if(!['AppleAppStore','GooglePlay'].includes(data.platform)||typeof data.transactionId!=='string'||data.transactionId.length>256)throw new Failure(400,'InvalidTransaction');return json({claimed:await store.claim(player,'transaction|'+digest(data.platform+'|'+data.transactionId),data.productId,data.bundleId)});}
  if(path==='/verify'){
   const pub=await getPublisher();const result=await verifyServicePurchase(data,request.headers.get('authorization'),{...pub,serviceToken:env.SERVICE_BEARER_TOKEN,expectedProjectId:env.UGS_PROJECT_ID,expectedEnvironmentId:env.UGS_ENVIRONMENT_ID});
   if(result.body.Valid){if(!await store.claim(player,'transaction|'+digest(data.store+'|'+data.transactionId),data.productId,data.bundleId))throw new Failure(409,'TransactionOwnership');await pub.remember(data);}return json(result.body,result.status);
  }
  if(path==='/reward/intent'){if(!(env.ADMOB_REWARDED_UNITS??'').split(',').includes(data.unit))throw new Failure(403,'AdUnitNotConfigured');return json(await store.intent(player,data.kind,data.subject??'',data.unit));}
  if(path==='/reward/status'){const reward=await store.one('SELECT confirmed,"transaction" FROM rewards WHERE id=? AND player=?',data.id,player);return json({ReceiptId:reward?.confirmed?'ssv_'+reward.transaction:null});}
  if(path==='/account/effects')return json(await store.effects(player));
  if(path==='/account/deletion-code')return json(await store.deleteCode(player));
  if(path==='/account/delete'){const result=await store.delete(player,env.BUNDLE_ID);try{await cleanup(store,env,dependencies.fetch??fetch);}catch{}return json(result,202);}
  if(path==='/maintenance'){let pending=false;
   // A failed publisher event must not prevent other events or account erasure.
   for(const event of await store.all('SELECT * FROM inbox WHERE done=0 ORDER BY created LIMIT 20')){try{await (await getPublisher()).reconcile(event.payload);await store.run('UPDATE inbox SET done=1 WHERE id=?',event.id);}catch{pending=true;}}
   for(const row of await store.all('SELECT chain FROM subscriptions WHERE observed<? LIMIT 20',store.now()-3600)){try{await (await getPublisher()).reconcile(row.chain);}catch{pending=true;}}
   await store.run('DELETE FROM rewards WHERE confirmed=0 AND expires<?',store.now()-3*86400);await store.run('DELETE FROM delete_codes WHERE expires<?',store.now());try{await cleanup(store,env,dependencies.fetch??fetch);}catch{pending=true;}if(pending)throw new Failure(503,'MaintenancePending');return json({Status:'Complete'});
  }
  return json({error:'NotFound'},404);
 }catch(e){return json({error:e instanceof Failure?e.message:'ServiceUnavailable'},e instanceof Failure?e.status:503);}
}
async function body(request,limit){if(!request.headers.get('content-type')?.startsWith('application/json'))throw new Failure(415,'ExpectedJSON');let size=0;const chunks=[];for await(const c of request.body??[]){size+=c.length;if(size>limit)throw new Failure(413,'RequestBudget');chunks.push(c);}try{return JSON.parse(Buffer.concat(chunks).toString('utf8'));}catch{throw new Failure(400,'InvalidJSON');}}
