import { products, accountBinding } from './verify.mjs';
import { Failure,digest } from './store.mjs';
import { loadAppleSdk,loadGoogleSdk } from './sdk.mjs';
export async function publishers(env,store,dependencies={}){
 // Apple jsrsasign initializes randomness. Worker modules must initialize it inside a request.
 const {SignedDataVerifier,AppStoreServerAPIClient,Environment}=loadAppleSdk();
 const {GoogleAuth,OAuth2Client}=loadGoogleSdk();
 const bundleId=env.BUNDLE_ID;
 const need=(...names)=>{if(names.some(n=>!env[n]))throw new Failure(503,'PublisherNotConfigured');};
 let apple=dependencies.appleVerifier,api=dependencies.appleAPI,google;
 function appleClients(requireApi=true){
  if(!apple){need('BUNDLE_ID','APPLE_ROOT_CA_BASE64','APPLE_ENVIRONMENT');if(!['Sandbox','Production'].includes(env.APPLE_ENVIRONMENT))throw new Failure(503,'PublisherNotConfigured');const production=env.APPLE_ENVIRONMENT==='Production';if(production&&(!Number.isSafeInteger(Number(env.APPLE_APP_ID))||Number(env.APPLE_APP_ID)<=0))throw new Failure(503,'PublisherNotConfigured');
   apple=new SignedDataVerifier(env.APPLE_ROOT_CA_BASE64.split(',').map(s=>Buffer.from(s,'base64')),true,production?Environment.PRODUCTION:Environment.SANDBOX,bundleId,production?Number(env.APPLE_APP_ID):undefined);
  }
  // Incoming signed notifications need Apple's public CA; only publisher API calls need our private signing key.
  if(requireApi&&!api){need('BUNDLE_ID','APPLE_PRIVATE_KEY','APPLE_KEY_ID','APPLE_ISSUER_ID','APPLE_ENVIRONMENT');api=new AppStoreServerAPIClient(env.APPLE_PRIVATE_KEY,env.APPLE_KEY_ID,env.APPLE_ISSUER_ID,bundleId,env.APPLE_ENVIRONMENT==='Production'?Environment.PRODUCTION:Environment.SANDBOX);}
 }
 async function googleRequest(path){if(dependencies.googleRequest)return dependencies.googleRequest(path);need('BUNDLE_ID','GOOGLE_CREDENTIALS_JSON');google??=new GoogleAuth({credentials:JSON.parse(env.GOOGLE_CREDENTIALS_JSON),scopes:['https://www.googleapis.com/auth/androidpublisher'],clientOptions:{transporterOptions:{fetchImplementation:fetch,timeout:10000}}});return (await (await google.getClient()).request({timeout:10000,url:'https://androidpublisher.googleapis.com/androidpublisher/v3/applications/'+encodeURIComponent(bundleId)+'/purchases/'+path})).data;}
 async function appleState(id,ownerToken){appleClients();const state=await api.getAllSubscriptionStatuses(id);if(state.bundleId!==bundleId)throw new Failure(403,'InvalidPublisher');const lines=[];
  for(const group of state.data??[])for(const item of group.lastTransactions??[]){const tx=await apple.verifyAndDecodeTransaction(item.signedTransactionInfo);const renewal=item.signedRenewalInfo?await apple.verifyAndDecodeRenewalInfo(item.signedRenewalInfo):null;
   if(tx.originalTransactionId!==id||tx.bundleId!==bundleId||tx.appAccountToken?.toLowerCase()!==ownerToken||products.get(tx.productId)!=='Subscription')continue;
   let end=0;if([1,4].includes(item.status)&&tx.revocationDate==null){end=tx.expiresDate??0;if(item.status===4)end=Math.max(end,renewal?.gracePeriodExpiresDate??0);}lines.push({product:tx.productId,expires:Math.floor(end/1000)});
  }return lines;
 }
 async function refresh(row){
  if(row.superseded_by){const result=await store.run('UPDATE subscriptions SET expires=0,observed=?,generation=generation+1 WHERE chain=? AND generation=?',store.now(),row.chain,row.generation);return result.meta.changes===1;}
  const proof=JSON.parse(store.unseal(row.proof)),binding=accountBinding(env.UGS_PROJECT_ID,env.UGS_ENVIRONMENT_ID,row.player);let product=row.product,expiry=0;
  if(row.store==='AppleAppStore'){const lines=await appleState(proof.originalTransactionId,binding.appleToken);const best=lines.sort((a,b)=>b.expires-a.expires)[0];if(best){product=best.product;expiry=best.expires;}}
  else{const data=await googleRequest('subscriptionsv2/tokens/'+encodeURIComponent(proof.token));if(data.externalAccountIdentifiers?.obfuscatedExternalAccountId!==binding.googleAccountId)throw new Failure(403,'SubscriptionBinding');
   if(data.linkedPurchaseToken&&!await store.claim(row.player,'chain|GooglePlay|'+digest(data.linkedPurchaseToken),'subscription-chain','GooglePlay'))throw new Failure(409,'SubscriptionOwnership');
   if(['SUBSCRIPTION_STATE_ACTIVE','SUBSCRIPTION_STATE_IN_GRACE_PERIOD','SUBSCRIPTION_STATE_CANCELED'].includes(data.subscriptionState)){const best=(data.lineItems??[]).filter(l=>products.get(l.productId)==='Subscription').sort((a,b)=>Date.parse(b.expiryTime)-Date.parse(a.expiryTime))[0];if(best){product=best.productId;expiry=Math.floor(Date.parse(best.expiryTime)/1000)||0;}}
  }
  await store.alive(row.player);const result=await store.run('UPDATE subscriptions SET product=?,expires=?,observed=?,generation=generation+1 WHERE chain=? AND generation=? AND NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?)',product,Math.max(0,expiry),store.now(),row.chain,row.generation,store.owner(row.player));return result.meta.changes===1;
 }
 return {
  bundleId,
  apple:async proof=>{appleClients();const tx=await apple.verifyAndDecodeTransaction(proof.jws);if(tx.transactionId!==proof.transactionId||tx.productId!==proof.productId)return tx;return apple.verifyAndDecodeTransaction((await api.getTransactionInfo(proof.transactionId)).signedTransactionInfo);},
  appleSubscription:async(proof,tx)=>{const lines=await appleState(tx.originalTransactionId,tx.appAccountToken?.toLowerCase());return Math.max(0,...lines.filter(l=>l.product===proof.productId).map(l=>l.expires));},
  google:(product,token)=>googleRequest('products/'+encodeURIComponent(product)+'/tokens/'+encodeURIComponent(token)),
  googleSubscription:async(product,token)=>{const d=await googleRequest('subscriptionsv2/tokens/'+encodeURIComponent(token));return {...d,packageName:bundleId,latestOrderId:d.latestOrderId??d.lineItems?.find(l=>l.productId===product)?.latestSuccessfulOrderId};},
  async remember(proof){if(products.get(proof.productId)!=='Subscription')return;
   let chain,material;if(proof.store==='AppleAppStore'){appleClients();const tx=await apple.verifyAndDecodeTransaction(proof.jws);chain=tx.originalTransactionId;material={originalTransactionId:chain};}else{const r=JSON.parse(JSON.parse(JSON.parse(proof.receipt).Payload).json);chain=r.purchaseToken;material={token:chain};const d=await googleRequest('subscriptionsv2/tokens/'+encodeURIComponent(chain));if(d.linkedPurchaseToken&&!await store.claim(proof.playerId,'chain|GooglePlay|'+digest(d.linkedPurchaseToken),'subscription-chain','GooglePlay'))throw new Failure(409,'SubscriptionOwnership');}
   const key=await store.saveSubscription(proof.playerId,chain,proof.store,proof.productId,JSON.stringify(material),0,0);
   if(proof.store==='GooglePlay'){const data=await googleRequest('subscriptionsv2/tokens/'+encodeURIComponent(chain));if(data.linkedPurchaseToken)await store.run('UPDATE subscriptions SET superseded_by=?,expires=0,generation=generation+1 WHERE chain=? AND player=?',key,'chain|GooglePlay|'+digest(data.linkedPurchaseToken),proof.playerId);}
   await this.reconcile(key);
  },
  async reconcile(key){for(let retry=0;retry<4;retry++){const row=await store.one('SELECT * FROM subscriptions WHERE chain=?',key);if(!row)return;if(await refresh(row))return;}throw new Failure(503,'EntitlementConflict');},
  async notification(kind,request){let eventId,chain;
   if(kind==='apple'){appleClients(false);const payload=await request.json();const event=await apple.verifyAndDecodeNotification(payload.signedPayload);if(event.data?.bundleId!==bundleId||event.data?.environment!==env.APPLE_ENVIRONMENT)throw new Failure(403,'NotificationScope');
    eventId=event.notificationUUID;if(event.notificationType==='TEST')return {test:true};if(!event.data?.signedTransactionInfo)throw new Failure(400,'MissingTransaction');const tx=await apple.verifyAndDecodeTransaction(event.data.signedTransactionInfo);if(event.data.signedRenewalInfo)await apple.verifyAndDecodeRenewalInfo(event.data.signedRenewalInfo);if(tx.bundleId!==bundleId)throw new Failure(403,'NotificationScope');chain='chain|AppleAppStore|'+digest(tx.originalTransactionId);
   }else{need('GOOGLE_PUSH_AUDIENCE','GOOGLE_PUSH_EMAIL');const bearer=/^Bearer (.+)$/.exec(request.headers.get('authorization')??'')?.[1];if(!bearer)throw new Failure(401,'PushAuthenticationRequired');const claims=(await new OAuth2Client({transporterOptions:{fetchImplementation:fetch,timeout:10000}}).verifyIdToken({idToken:bearer,audience:env.GOOGLE_PUSH_AUDIENCE})).getPayload();if(!['accounts.google.com','https://accounts.google.com'].includes(claims?.iss)||claims.email!==env.GOOGLE_PUSH_EMAIL||claims.email_verified!==true)throw new Failure(403,'PushIdentity');const push=await request.json();const event=JSON.parse(Buffer.from(push.message?.data??'','base64').toString());if(event.packageName!==bundleId)throw new Failure(403,'NotificationScope');if(event.testNotification)return {test:true};eventId=push.message?.messageId;const token=event.subscriptionNotification?.purchaseToken??event.voidedPurchaseNotification?.purchaseToken;if(!token)return {ignored:true};chain='chain|GooglePlay|'+digest(token);
   }
   if(typeof eventId!=='string'||eventId.length>256)throw new Failure(400,'InvalidEvent');const key=kind+'|'+eventId;
   // Store only a lookup hash, never the original signed notification or purchase token.
   if(!await store.one('SELECT chain FROM subscriptions WHERE chain=?',chain))return {ignored:true};
   await store.run('INSERT INTO inbox(id,store,payload,created) VALUES(?,?,?,?) ON CONFLICT(id) DO NOTHING',key,kind,chain,store.now());const receipt=await store.one('SELECT done FROM inbox WHERE id=?',key);if(!receipt.done){await this.reconcile(chain);await store.run('UPDATE inbox SET done=1 WHERE id=?',key);}return {accepted:true};
  }
 };
}
