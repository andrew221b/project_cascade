import {createCipheriv,createHash,randomBytes,timingSafeEqual} from 'node:crypto';
import {Store,Failure} from './store.mjs';
import {unityCall} from './unity-gateway.mjs';
const tables=['accounts','owners','rewards','subscriptions','tombstones','deletions','delete_codes','inbox'];
export async function migration(request,env,path,readBody){
 const expected='Bearer '+env.SERVICE_BEARER_TOKEN,actual=request.headers.get('authorization')??'';
 if(!env.SERVICE_BEARER_TOKEN||actual.length!==expected.length||!timingSafeEqual(Buffer.from(actual),Buffer.from(expected)))throw new Failure(401,'Unauthorized');
 if(request.method!=='POST'||request.headers.has('origin'))throw new Failure(403,'MigrationServiceRequired');
 const body=await readBody(request,4096);if(body.projectId!==env.UGS_PROJECT_ID||body.environmentId!==env.UGS_ENVIRONMENT_ID)throw new Failure(403,'MigrationScope');
 const store=new Store(env.DB,env.DATA_KEY);
 if(path==='/admin/unity-migration/export'){
  if(env.UNITY_BACKEND_PHASE!=='frozen')throw new Failure(409,'FreezeBeforeMigration');
  const source=Object.fromEntries(await Promise.all(tables.map(async table=>[table,await store.all('SELECT * FROM '+table)])));
  if(source.accounts.length>128)throw new Failure(413,'MigrationBatchRequired');
  const Publisher={BundleId:env.BUNDLE_ID,ApplePrivateKey:env.APPLE_PRIVATE_KEY,AppleKeyId:env.APPLE_KEY_ID,AppleIssuerId:env.APPLE_ISSUER_ID,AppleEnvironment:env.APPLE_ENVIRONMENT,AppleAppId:env.APPLE_APP_ID,GoogleCredentialsJson:env.GOOGLE_CREDENTIALS_JSON,ServiceAccountKeyId:env.UGS_SERVICE_KEY_ID,ServiceAccountSecret:env.UGS_SERVICE_SECRET,LegacyOwnerKey:env.DATA_KEY};
  if(Object.values(Publisher).some(value=>typeof value!=='string'||!value))throw new Failure(503,'MigrationCredentialsIncomplete');
  const data={Schema:1,ProjectId:env.UGS_PROJECT_ID,EnvironmentId:env.UGS_ENVIRONMENT_ID,Publisher,Accounts:source.accounts,Owners:source.owners,Rewards:source.rewards,Subscriptions:source.subscriptions,Tombstones:source.tombstones,Deletions:source.deletions,DeleteCodes:source.delete_codes,Inbox:source.inbox};
  const plain=Buffer.from(JSON.stringify(data));if(plain.length>1400*1024)throw new Failure(413,'MigrationBatchRequired');
  const key=createHash('sha256').update('cascade-unity-vault-v1|'+env.SERVICE_BEARER_TOKEN).digest(),iv=randomBytes(12),aes=createCipheriv('aes-256-gcm',key,iv);aes.setAAD(Buffer.from('cascade-unity-vault-v1|'+env.UGS_PROJECT_ID+'|'+env.UGS_ENVIRONMENT_ID));
  const ciphertext=Buffer.concat([aes.update(plain),aes.final()]);
  return {Envelope:JSON.stringify({Schema:1,Iv:iv.toString('base64'),Ciphertext:ciphertext.toString('base64'),Tag:aes.getAuthTag().toString('base64')}),Counts:Object.fromEntries(tables.map(table=>[table,source[table].length]))};
 }
 if(path==='/admin/unity-migration/purge'){
  if(env.UNITY_BACKEND_PHASE!=='unity')throw new Failure(409,'ActivateUnityBeforePurge');
  const status=await unityCall(env,'UnityMigrationStatus',{});if(status.Mode!=='Unity'||status.Manifest?.Status!=='Verified'||status.Manifest?.ManifestHash!==body.manifestHash)throw new Failure(409,'UnityMigrationNotVerified');
  // The owner explicitly requested retirement, without keeping an old DB backup.
  for(const table of [...tables,'server_credentials'])await store.run('DELETE FROM '+table);
  const remaining=Object.fromEntries(await Promise.all([...tables,'server_credentials'].map(async table=>[table,(await store.one('SELECT COUNT(*) AS count FROM '+table)).count])));
  if(Object.values(remaining).some(count=>count!==0))throw new Failure(503,'LegacyPurgePending');return {Purged:true,Remaining:remaining,ManifestHash:body.manifestHash};
 }
 throw new Failure(404,'NotFound');
}
