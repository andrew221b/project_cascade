import test from 'node:test';import assert from 'node:assert/strict';
import{DatabaseSync}from'node:sqlite';import{readFileSync,readdirSync}from'node:fs';import{generateKeyPairSync,sign}from'node:crypto';
import{Store}from'./store.mjs';import{handle}from'./legacy-handler.mjs';import{importCredentials,loadCredentials}from'./credential-import.mjs';
class DB{
 constructor(){this.db=new DatabaseSync(':memory:');for(const n of readdirSync(new URL('../drizzle',import.meta.url)).filter(n=>n.endsWith('.sql')))this.db.exec(readFileSync(new URL('../drizzle/'+n,import.meta.url),'utf8'));}
 prepare(sql){const db=this.db;return{first:async()=>db.prepare(sql).get()??null,bind(...args){return{
  first:async()=>db.prepare(sql).get(...args)??null,
  all:async()=>({results:db.prepare(sql).all(...args)}),
  run:async()=>({meta:{changes:Number(db.prepare(sql).run(...args).changes)}})
 };}};}
}
const origin='https://backend.example.test',path='/api/admin/import-credentials';
function fixture(){const signer=generateKeyPairSync('ec',{namedCurve:'prime256v1'}),apple=generateKeyPairSync('ec',{namedCurve:'prime256v1'}),google=generateKeyPairSync('rsa',{modulusLength:2048});const now=Date.now(),env={DB:new DB(),DATA_KEY:'test-data-key-'.repeat(5),SERVICE_BEARER_TOKEN:'old-test-'.repeat(8),UGS_PROJECT_ID:'project',UGS_ENVIRONMENT_ID:'production',BUNDLE_ID:'com.test.cascade',GOOGLE_PUSH_EMAIL:'purchases@example.iam.gserviceaccount.com',CREDENTIAL_IMPORT_PUBLIC_KEY:signer.publicKey.export({type:'spki',format:'pem'}),CREDENTIAL_IMPORT_DEADLINE:String(Math.floor(now/1000)+600)};
 const values={APPLE_PRIVATE_KEY:apple.privateKey.export({type:'pkcs8',format:'pem'}),GOOGLE_CREDENTIALS_JSON:JSON.stringify({type:'service_account',client_email:env.GOOGLE_PUSH_EMAIL,private_key_id:'test-key-id',private_key:google.privateKey.export({type:'pkcs8',format:'pem'}),token_uri:'https://oauth2.googleapis.com/token'}),SERVICE_BEARER_TOKEN:'new_test_credential_'.repeat(4)};
 const payload={purpose:'publishers-v1',issuedAt:now,projectId:env.UGS_PROJECT_ID,environmentId:env.UGS_ENVIRONMENT_ID,bundleId:env.BUNDLE_ID,values};
 function request(data=payload,options={}){const raw=JSON.stringify(data),signed=Buffer.from('cascade-credential-import-v1\n'+(options.signedOrigin??origin)+'\n'+path+'\n'+raw),signature=sign('sha256',signed,options.signer??signer.privateKey).toString('base64url');return new Request(origin+path,{method:'POST',headers:{'Content-Type':'application/json','X-Cascade-Import-Signature':signature,...options.headers},body:raw});}
 return{env,values,payload,request,now,store:new Store(env.DB,env.DATA_KEY)};
}
test('signed import stores only authenticated ciphertext, returns no secrets, and connects the prepared service token',async()=>{
 const f=fixture();const installed=await handle(f.request(),f.env);assert.equal(installed.status,200);const answer=await installed.text();assert.equal(JSON.parse(answer).installed,true);for(const value of Object.values(f.values))assert.ok(!answer.includes(value));
 const rows=await f.store.all('SELECT * FROM server_credentials');assert.equal(rows.length,1);assert.ok(!rows[0].sealed.includes('PRIVATE KEY'));assert.ok(!rows[0].sealed.includes(f.values.SERVICE_BEARER_TOKEN));
 const loaded=await loadCredentials(f.env);assert.equal(loaded.SERVICE_BEARER_TOKEN,f.values.SERVICE_BEARER_TOKEN);assert.equal(loaded.DATA_KEY,f.env.DATA_KEY);assert.equal(loaded.UGS_PROJECT_ID,f.env.UGS_PROJECT_ID);
 const request=token=>new Request(origin+'/api/account/read',{method:'POST',headers:{'Content-Type':'application/json',Authorization:'Bearer '+token},body:JSON.stringify({projectId:'project',environmentId:'production',playerId:'alice'})});
 assert.equal((await handle(request(f.env.SERVICE_BEARER_TOKEN),f.env)).status,401);assert.equal((await handle(request(f.values.SERVICE_BEARER_TOKEN),f.env)).status,200);assert.equal((await (await handle(new Request(origin+'/api/health'),f.env)).json()).publishersConfigured,true);
});
test('disabled, expired, unsigned and foreign-key imports cannot write',async()=>{
 const f=fixture();await assert.rejects(()=>importCredentials(f.request(),{...f.env,CREDENTIAL_IMPORT_PUBLIC_KEY:undefined}),e=>e.status===404);await assert.rejects(()=>importCredentials(f.request(),{...f.env,CREDENTIAL_IMPORT_DEADLINE:'1'}),e=>e.status===410);
 await assert.rejects(()=>importCredentials(new Request(origin+path,{method:'POST',headers:{'Content-Type':'application/json'},body:'{}'}),f.env),e=>e.status===401);
 const foreign=generateKeyPairSync('ec',{namedCurve:'prime256v1'});await assert.rejects(()=>importCredentials(f.request(f.payload,{signer:foreign.privateKey}),f.env),e=>e.status===401);assert.equal((await f.store.all('SELECT * FROM server_credentials')).length,0);
});
test('signature binds origin and bytes; scope, browser origin and freshness are mandatory',async()=>{
 const f=fixture();for(const patch of [{projectId:'foreign'},{environmentId:'foreign'},{bundleId:'foreign'},{issuedAt:f.now-301000},{issuedAt:f.now+31000}])await assert.rejects(()=>importCredentials(f.request({...f.payload,...patch}),f.env,()=>f.now),e=>e.status===403);
 await assert.rejects(()=>importCredentials(f.request(f.payload,{signedOrigin:'https://foreign.example'}),f.env),e=>e.status===401);await assert.rejects(()=>importCredentials(f.request(f.payload,{headers:{Origin:origin}}),f.env),e=>e.status===403);
 const valid=f.request();const tampered=new Request(valid.url,{method:'POST',headers:valid.headers,body:JSON.stringify({...f.payload,issuedAt:f.now+1})});await assert.rejects(()=>importCredentials(tampered,f.env),e=>e.status===401);assert.equal((await f.store.all('SELECT * FROM server_credentials')).length,0);
});
test('only the three exact credential fields and expected Google identity can be installed',async()=>{
 const f=fixture(),wrongGoogle=JSON.stringify({...JSON.parse(f.values.GOOGLE_CREDENTIALS_JSON),client_email:'foreign@example.com'});
 for(const values of [{...f.values,DATA_KEY:'replace-encryption'},{...f.values,APPLE_PRIVATE_KEY:'invalid'},{...f.values,GOOGLE_CREDENTIALS_JSON:wrongGoogle},{...f.values,SERVICE_BEARER_TOKEN:'short'}])await assert.rejects(()=>importCredentials(f.request({...f.payload,values}),f.env),e=>e.status===400);
 await assert.rejects(()=>importCredentials(f.request({...f.payload,padding:'x'.repeat(32768)}),f.env),e=>e.status===413);assert.equal((await f.store.all('SELECT * FROM server_credentials')).length,0);
});
test('concurrent signed attempts have one winner and cannot overwrite the installed credentials',async()=>{
 const f=fixture(),other={...f.payload,values:{...f.values,SERVICE_BEARER_TOKEN:'another_test_credential_'.repeat(3)}};
 const results=await Promise.allSettled([importCredentials(f.request(),f.env),importCredentials(f.request(other),f.env)]);assert.equal(results.filter(r=>r.status==='fulfilled').length,1);assert.equal(results.find(r=>r.status==='rejected').reason.status,409);
 assert.equal((await f.store.all('SELECT * FROM server_credentials')).length,1);await assert.rejects(()=>importCredentials(f.request(),f.env),e=>e.status===409);
 const loaded=await loadCredentials({...f.env,CREDENTIAL_IMPORT_PUBLIC_KEY:undefined,CREDENTIAL_IMPORT_DEADLINE:undefined});assert.equal(loaded.SERVICE_BEARER_TOKEN,f.values.SERVICE_BEARER_TOKEN);await assert.rejects(()=>importCredentials(f.request(),{...f.env,CREDENTIAL_IMPORT_PUBLIC_KEY:undefined}),e=>e.status===404);
});
test('tampered or cross-environment encrypted records fail closed instead of falling back to the old service token',async()=>{
 const f=fixture();await importCredentials(f.request(),f.env);await assert.rejects(()=>loadCredentials({...f.env,UGS_ENVIRONMENT_ID:'foreign'}),e=>e.status===503);await f.store.run('UPDATE server_credentials SET sealed=?','tampered');assert.equal((await handle(new Request(origin+'/api/health'),f.env)).status,503);
});
