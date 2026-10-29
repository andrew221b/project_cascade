import test from 'node:test';
import assert from 'node:assert/strict';
import {createHandler} from './worker.mjs';
const root='https://flyingladders.vickand.com';
function request(data,origin=root,headers={}){return new Request(root+'/api/account-deletion',{method:'POST',headers:{Origin:origin,'Content-Type':'application/json','CF-Connecting-IP':'198.51.100.4',...headers},body:JSON.stringify(data)});}
test('form and documents stay on the product domain with private deletion headers',async()=>{
  const handle=createHandler(()=>{throw Error('Unexpected origin');});
  const response=await handle(new Request(root+'/delete-account/'),{});
  assert.equal(response.status,200);assert.equal(response.headers.get('cache-control'),'no-store');assert.equal(response.headers.get('referrer-policy'),'no-referrer');
  const body=await response.text();assert.match(body,/account-deletion-form/);assert.ok(!body.includes('chatgpt.site'));
  for(const path of ['/privacy/','/terms/','/data-request/']){const page=await(await handle(new Request(root+path),{})).text();assert.ok(!page.includes('Private review draft')&&!page.includes('Private review')&&!page.includes('Cloudflare D1')&&!page.includes('chatgpt.site'));}
});
test('deletion uses a fixed service and never forwards client credentials or relay headers',async()=>{
  let seen;const handle=createHandler();const env={ACCOUNT_GATEWAY:{fetch:async(url,options)=>{seen={url,options};return Response.json({Status:'DeletionPending'},{status:202});}}};
  const response=await handle(request({code:'a'.repeat(32),confirm:'DELETE'},root,{Authorization:'Bearer hostile','X-Flying-Relay':'hostile','X-Flying-Client-IP':'203.0.113.9'}),env);
  assert.equal(response.status,202);assert.equal(seen.url,'https://vickand.com/api/flying-ladders/deletion/confirm');assert.equal(seen.options.headers.Origin,root);assert.equal(seen.options.headers['CF-Connecting-IP'],'198.51.100.4');assert.equal(seen.options.headers.Authorization,undefined);assert.equal(seen.options.headers['X-Flying-Relay'],undefined);assert.equal(seen.options.redirect,'manual');
});
test('foreign origins, missing confirmation and malformed/oversized codes cannot reach Unity',async()=>{
  let calls=0;const handle=createHandler(),env={ACCOUNT_GATEWAY:{fetch:async()=>{calls++;throw Error('Must not call');}}};
  for(const origin of ['null','https://evil.test','https://flyingladders.vickand.com.evil.test'])assert.equal((await handle(request({code:'a'.repeat(32),confirm:'DELETE'},origin),env)).status,403);
  for(const data of [{code:'short',confirm:'DELETE'},{code:'a'.repeat(32),confirm:'KEEP'},{code:'a'.repeat(32),confirm:'DELETE',player:'other'}])assert.equal((await handle(request(data),env)).status,400);
  assert.equal((await handle(request({code:'a'.repeat(2048),confirm:'DELETE'}),env)).status,413);assert.equal(calls,0);
});
test('unknown hosts cannot become an open proxy and ordinary assets retain Vercel origin',async()=>{
  let calls=0;const handle=createHandler(async req=>{calls++;assert.equal(new URL(req.url).hostname,'flyingladders.vickand.com');return new Response('retained');});
  assert.equal((await handle(new Request('https://evil.test/asset'),{})).status,404);assert.equal(calls,0);
  assert.equal(await(await handle(new Request(root+'/assets/icon.webp'),{})).text(),'retained');assert.equal(calls,1);
});
test('upstream failure and redirect are never presented as a successful deletion',async()=>{
  const handle=createHandler();for(const [reply,expected] of [[()=>new Response(null,{status:302,headers:{Location:'https://evil.test'}}),503],[()=>Response.json({error:'RetryLater'},{status:429}),429],[()=>{throw Error('Private credentials');},503]]){
    const response=await handle(request({code:'a'.repeat(32),confirm:'DELETE'}),{ACCOUNT_GATEWAY:{fetch:reply}});assert.equal(response.status,expected);assert.ok(!(await response.text()).includes('Private credentials'));
  }
});
