import test from 'node:test';import assert from 'node:assert/strict';import {generateKeyPairSync,sign} from 'node:crypto';import {handle} from './legacy-handler.mjs';

const keys=generateKeyPairSync('ec',{namedCurve:'prime256v1'}),keyId=83920143;

test('AdMob console probe requires a valid signature and never confirms a reward',async()=>{
  const fetcher=async()=>Response.json({keys:[{keyId,pem:keys.publicKey.export({type:'spki',format:'pem'})}]});
 let confirmations=0;const store={confirm:async()=>{confirmations++;}};
 const query='ad_network=5450213213286189855&ad_unit=1234567890&custom_data=unused&reward_amount=1&reward_item=reward&timestamp=1800000000000&transaction_id=console-probe&user_id=qa';
 const signature=sign('sha256',Buffer.from(query),keys.privateKey).toString('base64url');
 const url='https://example.test/api/admob/ssv?'+query+'&signature='+signature+'&key_id='+keyId;
 const response=await handle(new Request(url),{}, {store,fetch:fetcher});
 assert.equal(response.status,200);assert.deepEqual(await response.json(),{verified:true,test:true});assert.equal(confirmations,0);
 const forged=await handle(new Request(url.replace('custom_data=unused','custom_data=altered')),{}, {store,fetch:fetcher});assert.equal(forged.status,403);assert.equal(confirmations,0);
 const actual=query.replace('ad_unit=1234567890','ad_unit=1665469215'),actualSignature=sign('sha256',Buffer.from(actual),keys.privateKey).toString('base64url');
 const normal=await handle(new Request('https://example.test/api/admob/ssv?'+actual+'&signature='+actualSignature+'&key_id='+keyId),{ADMOB_REWARDED_UNITS:'1665469215'},{store,fetch:fetcher});assert.equal(normal.status,200);assert.equal(confirmations,1);
});

// Google console encodes the configured reward label as form query data.
test('AdMob form-encoded labels verify as their exact decoded value, including a literal plus',async()=>{
 const fetcher=async()=>Response.json({keys:[{keyId,pem:keys.publicKey.export({type:'spki',format:'pem'})}]});
 for(const label of ['Open Lootbox','gold+coin','Золоті монети']){
  const query='ad_network=5450213213286189855&ad_unit=1234567890&reward_amount=1&reward_item='+new URLSearchParams({label}).toString().slice(6)+'&timestamp=1800000000000&transaction_id=console-probe';
  const signed=decodeURIComponent(query.replace(/\+/g,' '));
  const signature=sign('sha256',Buffer.from(signed),keys.privateKey).toString('base64url');
  const url='https://example.test/api/admob/ssv?'+query+'&signature='+signature+'&key_id='+keyId;
  let confirmations=0;const store={confirm:async()=>{confirmations++;}};
  const response=await handle(new Request(url),{}, {store,fetch:fetcher});
  assert.equal(response.status,200,label);assert.equal(confirmations,0);
  const forged=await handle(new Request(url.replace('reward_amount=1','reward_amount=40')),{}, {store,fetch:fetcher});
  assert.equal(forged.status,403);assert.equal(confirmations,0);
 }
});
