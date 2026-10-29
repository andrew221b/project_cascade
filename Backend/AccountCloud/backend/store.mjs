import { createHash, createHmac, randomBytes, createCipheriv, createDecipheriv } from 'node:crypto';
import { gunzipSync } from 'node:zlib';
export const digest=x=>createHash('sha256').update(x).digest('hex');
export class Failure extends Error{constructor(status,code){super(code);this.status=status;}}
export class Store {
 constructor(db,secret,clock=()=>Date.now()){if(!db||!secret||secret.length<32)throw new Failure(503,'StorageUnavailable');this.db=db;this.secret=secret;this.clock=clock;}
 now(){return Math.floor(this.clock()/1000);}
 owner(player){return createHmac('sha256',this.secret).update('owner|'+player).digest('hex');}
 async one(sql,...args){return this.db.prepare(sql).bind(...args).first();}
 async all(sql,...args){return (await this.db.prepare(sql).bind(...args).all()).results;}
 async run(sql,...args){return this.db.prepare(sql).bind(...args).run();}
 async alive(player){if(await this.one('SELECT owner FROM tombstones WHERE owner=?',this.owner(player)))throw new Failure(410,'AccountDeleted');}
 async read(player){await this.alive(player);return await this.one('SELECT version,payload FROM accounts WHERE player=?',player)??{version:0,payload:null};}
 async write(player,version,payload){
  if(!Number.isSafeInteger(version)||version<0||typeof payload!=='string'||Buffer.byteLength(payload)>1536*1024)throw new Failure(400,'AccountBudget');
  await this.alive(player);
  // Tombstone check is in the SAME SQL statement as the mutation; prior alive() alone is insufficient.
  let r= version===0 ? await this.run('INSERT INTO accounts(player,version,payload) SELECT ?,1,? WHERE NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?) ON CONFLICT(player) DO NOTHING',player,payload,this.owner(player))
   :await this.run('UPDATE accounts SET version=version+1,payload=? WHERE player=? AND version=? AND NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?)',payload,player,version,this.owner(player));
  await this.alive(player);return {written:r.meta.changes===1,version:version+1};
 }
 async claim(player,key,product,bundle){
  await this.alive(player);const owner=this.owner(player);
  await this.run('INSERT INTO owners(key,owner,product,bundle) SELECT ?,?,?,? WHERE NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?) ON CONFLICT(key) DO NOTHING',key,owner,product,bundle,owner);
  await this.alive(player);const r=await this.one('SELECT * FROM owners WHERE key=?',key);return r?.owner===owner&&r.product===product&&r.bundle===bundle;
 }
 seal(value){const iv=randomBytes(12),key=createHash('sha256').update(this.secret).digest(),c=createCipheriv('aes-256-gcm',key,iv);return Buffer.concat([iv,c.update(value,'utf8'),c.final(),c.getAuthTag()]).toString('base64url');}
 unseal(value){const b=Buffer.from(value,'base64url'),c=createDecipheriv('aes-256-gcm',createHash('sha256').update(this.secret).digest(),b.subarray(0,12));c.setAuthTag(b.subarray(-16));return Buffer.concat([c.update(b.subarray(12,-16)),c.final()]).toString('utf8');}
 async intent(player,kind,subject,unit){
  await this.alive(player);if(!['coins','hint','parcel_open','parcel_bonus'].includes(kind)||typeof unit!=='string'||!/^\d{5,30}$/.test(unit))throw new Failure(400,'InvalidOffer');
  if(!['coins','hint'].includes(kind)&&!/^parcel:([1-9]\d{0,2})$/.test(subject))throw new Failure(400,'InvalidParcel');
  const outstanding=await this.one('SELECT id FROM rewards WHERE player=? AND kind=? AND subject=? AND confirmed=0 AND expires>?',player,kind,subject,this.now());
  if(outstanding)return {Id:outstanding.id,UserId:player};
  const id=randomBytes(24).toString('base64url'),now=this.now();
  await this.run('INSERT INTO rewards(id,player,kind,subject,unit,created,expires) SELECT ?,?,?,?,?,?,? WHERE NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?)',id,player,kind,subject,unit,now,now+900,this.owner(player));
  await this.alive(player);return {Id:id,UserId:player};
 }
 async confirm(event,allowedUnits){
  const r=await this.one('SELECT * FROM rewards WHERE id=?',event.custom_data);
  if(!r)throw new Failure(404,'UnknownOffer');await this.alive(r.player);
  const stamp=Number(event.timestamp)/1000;
  if(!allowedUnits.includes(event.ad_unit)||r.unit!==event.ad_unit||r.player!==event.user_id||!Number.isFinite(stamp)||stamp<r.created-30||stamp>r.expires||this.now()>r.expires+3*86400||event.reward_amount!=='1'||!event.transaction_id||event.transaction_id.length>256)throw new Failure(403,'InvalidReward');
  const tx=digest('admob|'+event.transaction_id);
  // UNIQUE(transaction) prevents another ticket from consuming the same signed event.
  try {await this.run('UPDATE rewards SET confirmed=1,"transaction"=? WHERE id=? AND ("transaction" IS NULL OR "transaction"=?) AND NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?)',tx,r.id,tx,this.owner(r.player));} catch(error){if(['parcel_open','parcel_bonus'].includes(r.kind)&&await this.one('SELECT id FROM rewards WHERE player=? AND kind=? AND subject=? AND confirmed=1',r.player,r.kind,r.subject))return;throw error;}
  const confirmed=await this.one('SELECT confirmed,"transaction" FROM rewards WHERE id=?',r.id);if(!confirmed?.confirmed||confirmed.transaction!==tx)throw new Failure(409,'RewardReused');
 }
 async effects(player){await this.alive(player);const subscriptions=await this.all('SELECT product AS Product,MAX(expires) AS Expires FROM subscriptions WHERE player=? AND superseded_by IS NULL GROUP BY product',player);return {Rewards:await this.all('SELECT "transaction" AS Id,kind AS Kind,subject AS Subject FROM rewards WHERE player=? AND confirmed=1',player),Subscriptions:subscriptions.length?['cascade.no_ads_week','cascade.no_ads_month','cascade.no_ads_year'].map(Product=>({Product,Expires:subscriptions.find(s=>s.Product===Product)?.Expires??0})):[]};}
 async saveSubscription(player,chain,store,product,proof,expires,observed,expectedGeneration=null){
  await this.alive(player);const ownerKey='chain|'+store+'|'+digest(chain);
  // Product may change inside a subscription group; chain owner never changes.
  if(!await this.claim(player,ownerKey,'subscription-chain',store))throw new Failure(409,'SubscriptionOwnership');
  if(expectedGeneration===null){await this.run('INSERT INTO subscriptions(chain,player,store,product,proof,expires,observed) SELECT ?,?,?,?,?,?,? WHERE NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?) ON CONFLICT(chain) DO NOTHING',ownerKey,player,store,product,this.seal(proof),expires,observed,this.owner(player));}
  else await this.run('UPDATE subscriptions SET product=?,expires=?,observed=?,generation=generation+1 WHERE chain=? AND player=? AND generation=? AND NOT EXISTS(SELECT 1 FROM tombstones WHERE owner=?)',product,expires,observed,ownerKey,player,expectedGeneration,this.owner(player));
  await this.alive(player);return ownerKey;
 }
 async delete(player,bundle=''){
  const owner=this.owner(player),created=this.now(),row=await this.one('SELECT payload FROM accounts WHERE player=?',player);let grants=[];
  if(row){try{const payload=row.payload.startsWith('gz1:')?gunzipSync(Buffer.from(row.payload.slice(4),'base64'),{maxOutputLength:8*1024*1024}).toString('utf8'):row.payload;grants=JSON.parse(payload).Account?.Profile?.Monetization?.Grants??[];}catch{/* Corrupt game data must not prevent account deletion. */}}
  const legacy=grants.filter(g=>/^store_[a-f0-9]{64}$/.test(g.Id)),legacyIds=JSON.stringify(legacy.map(g=>'iap_'+g.Id.slice(6,52)));

  // D1 batch is a transaction. Authentication deletion is retryable and happens only AFTER this commit.
  await this.db.batch([
   this.db.prepare('INSERT INTO tombstones(owner,created) VALUES(?,?) ON CONFLICT(owner) DO NOTHING').bind(owner,created),
   this.db.prepare('INSERT INTO deletions(owner,player,created,legacy_ids) VALUES(?,?,?,?) ON CONFLICT(owner) DO NOTHING').bind(owner,player,created,legacyIds),
   ...legacy.map(g=>this.db.prepare('INSERT INTO owners(key,owner,product,bundle) VALUES(?,?,?,?) ON CONFLICT(key) DO NOTHING').bind('transaction|'+g.Id.slice(6),owner,g.Product,bundle)),
   ...['accounts','rewards','subscriptions','delete_codes'].map(t=>this.db.prepare(`DELETE FROM ${t} WHERE player=?`).bind(player))
  ]);return {Status:'DeletionPending',RequestId:owner};
 }
 async deleteCode(player){await this.alive(player);const code=randomBytes(24).toString('base64url');await this.run('INSERT INTO delete_codes(hash,player,expires) VALUES(?,?,?)',digest(code),player,this.now()+600);return {Code:code,ExpiresUnix:this.now()+600};}
 async redeemDelete(code,bundle=''){if(typeof code!=='string'||!/^[A-Za-z0-9_-]{32}$/.test(code))throw new Failure(400,'InvalidCode');const r=await this.one('SELECT player FROM delete_codes WHERE hash=? AND expires>?',digest(code),this.now());if(!r)throw new Failure(400,'InvalidCode');return this.delete(r.player,bundle);}
}
