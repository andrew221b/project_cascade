import test from 'node:test';import assert from 'node:assert/strict';import workerFetch from '../scripts/worker-fetch.mjs';
test('Worker Apple transport preserves request/response and OCSP bytes',async()=>{
 const original=globalThis.fetch;let request;
 try{globalThis.fetch=async(input,options)=>{request={input,options};return new Response(Uint8Array.from([0,1,128,255]),{status:200,headers:{'Content-Type':'application/ocsp-response'}});};
  const body=Buffer.from([1,2,3]),response=await workerFetch('http://ocsp.apple.com',{method:'POST',headers:{'Content-Type':'application/ocsp-request'},body,timeout:1000});
  assert.equal(request.input,'http://ocsp.apple.com');assert.equal(request.options.method,'POST');assert.equal(request.options.body,body);assert.equal(request.options.timeout,undefined);assert.ok(request.options.signal instanceof AbortSignal);assert.equal(response.status,200);assert.equal(response.headers.get('content-type'),'application/ocsp-response');assert.deepEqual([...await response.buffer()],[0,1,128,255]);
 }finally{globalThis.fetch=original;}
});
test('Worker Apple transport keeps caller cancellation and non-success statuses',async()=>{
 const original=globalThis.fetch;const controller=new AbortController();controller.abort();
 try{globalThis.fetch=async(_input,options)=>{assert.equal(options.signal.aborted,true);return new Response('{"error":"busy"}',{status:503});};
  const response=await workerFetch('https://api.storekit.apple.com',{signal:controller.signal,timeout:1000});assert.equal(response.ok,false);assert.equal(response.status,503);assert.deepEqual(await response.json(),{error:'busy'});
 }finally{globalThis.fetch=original;}
});
