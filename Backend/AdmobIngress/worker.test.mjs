import test from'node:test';import assert from'node:assert/strict';import{createHandler}from'./worker.mjs';
const trusted={ssv:async()=>{}};const createVerified=(f)=>createHandler(f,trusted);
const env={UNITY_SERVICE_KEY_ID:'fake-test-id',UNITY_SERVICE_SECRET:'fake-test-secret'},url='https://vickand.com/api/flying-ladders/admob/ssv?ad_unit=1234567890&reward_amount=1&reward_item=Open+Lootbox%2B&timestamp=123&transaction_id=abc&signature=abc&key_id=1';
function fake(output={verified:true,test:true},status=200){const calls=[];return{calls,handler:createVerified(async(url,options)=>{calls.push({url,options});return url.includes('token-exchange')?Response.json({accessToken:'fake-service-jwt'}):Response.json({output},{status});})};}
test('exact signed URL and fixed production scope reach Unity without caller credentials',async()=>{const{handler,calls}=fake();const r=await handler(new Request(url,{headers:{Authorization:'Bearer attacker'}}),env);assert.equal(r.status,200);assert.deepEqual(await r.json(),{verified:true,test:true});assert.equal(JSON.parse(calls[1].options.body).params.callbackUrl,url);assert.ok(calls[1].url.endsWith('/UnityAdmobNotification'));assert.equal(calls[1].options.headers.UnityEnvironment,'2cf87fca-ee3b-46cc-acc5-b5cbf2ad12d7');assert.equal(calls[1].options.headers.Authorization,'Bearer fake-service-jwt');assert.equal(r.headers.get('cache-control'),'no-store');});
test('unsigned, unknown-path, wrong-host and wrong-method requests never invoke Unity',async()=>{const{handler,calls}=fake();for(const[address,method,status]of[['https://vickand.com/api/flying-ladders/admob/ssv','GET',400],[url,'POST',405],['https://vickand.com/api/flying-ladders/other','GET',404],[url.replace('vickand.com','other.example'),'GET',404]])assert.equal((await handler(new Request(address,{method}),env)).status,status);assert.equal(calls.length,0);});
test('Unity failure never acknowledges a reward',async()=>{const{handler}=fake({error:'busy'},503);assert.equal((await handler(new Request(url),env)).status,503);});
test('runtime conflict remains retryable; invalid signature is rejected',async()=>{for(const[detail,status]of[['Account is busy. Retry safely.',503],['Invalid reward signature.',403]]){const handler=createVerified(async(address)=>address.includes('token-exchange')?Response.json({accessToken:'test'}):Response.json({detail},{status:422}));assert.equal((await handler(new Request(url),env)).status,status);}});
test('health exposes readiness without account manifest or credentials',async()=>{const{handler}=fake({Version:'20261028.2',Ready:true,Healthy:true,MaintenanceFresh:true,private:'private-test-marker'});const r=await handler(new Request('https://vickand.com/api/flying-ladders/health'),env),body=await r.text();assert.equal(r.status,200);assert.ok(!body.includes('private-test-marker'));assert.ok(!body.includes(env.UNITY_SERVICE_SECRET));});
test('service-token cache does not conflate different callbacks',async()=>{const{handler,calls}=fake();await handler(new Request(url),env);await handler(new Request(url.replace('transaction_id=abc','transaction_id=second')),env);assert.equal(calls.filter(c=>c.url.includes('token-exchange')).length,1);assert.notEqual(JSON.parse(calls[1].options.body).params.callbackUrl,JSON.parse(calls[2].options.body).params.callbackUrl);});
test('oversized query and vendor response cannot acknowledge a reward',async()=>{const{handler,calls}=fake();assert.equal((await handler(new Request(url+'&large='+'a'.repeat(9000)),env)).status,414);assert.equal(calls.length,0);const huge=createVerified(async()=>new Response('a'.repeat(70000)));assert.equal((await huge(new Request(url),env)).status,503);});
test('success without explicit Unity acknowledgement remains retryable',async()=>{const{handler}=fake({ignored:true});assert.equal((await handler(new Request(url),env)).status,503);});

test('edge-runtime fetch keeps redirects manual and never leaks credentials to a redirect target',async()=>{
 const calls=[];const handler=createVerified(async(address,options)=>{
  assert.equal(options.redirect,'manual');calls.push(address);
  return new Response(null,{status:302,headers:{Location:'https://untrusted.example/steal'}});
 });
 assert.equal((await handler(new Request(url),env)).status,503);
 assert.equal(calls.length,1);
 assert.ok(calls[0].startsWith('https://services.api.unity.com/'));
});
test('runtime exceptions are sanitized in logs and remain private in the response',async()=>{
 const warnings=[],original=console.warn;console.warn=(message)=>warnings.push(message);
 try{
  const handler=createVerified(async()=>{throw new TypeError('upstream '+env.UNITY_SERVICE_SECRET+' https://example.invalid/?secret=private');});
  const response=await handler(new Request(url),env);
  assert.equal(response.status,503);assert.deepEqual(await response.json(),{error:'ServiceUnavailable'});
  assert.equal(warnings.length,1);assert.ok(warnings[0].includes('TypeError'));
  assert.ok(!warnings[0].includes(env.UNITY_SERVICE_SECRET));assert.ok(!warnings[0].includes('?secret='));
 }finally{console.warn=original;}
});

test('Unity cold-start timeouts remain retryable and identify the failing stage',async()=>{
 for(const[failAt,expected]of[[1,'UnityAuthenticationTimeout'],[2,'UnityOperationTimeout']]){
  let calls=0;const handler=createVerified(async()=>{
   if(++calls===failAt)throw new DOMException('upstream timeout','TimeoutError');
   return Response.json({accessToken:'fake-service-jwt'});
  });
  const response=await handler(new Request(url),env);
  assert.equal(response.status,503);assert.deepEqual(await response.json(),{error:expected});
 }
});
