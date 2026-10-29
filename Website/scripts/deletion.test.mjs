import test from 'node:test';import assert from 'node:assert/strict';import fs from 'node:fs';import vm from 'node:vm';
const script=fs.readFileSync(new URL('../deletion.js',import.meta.url),'utf8'),html=fs.readFileSync(new URL('../content/deletion-page.html',import.meta.url),'utf8');
function fixture(locale='en',fetcher=async()=>Response.json({Status:'DeletionPending'})){
 const ids={};for(const id of ['account-deletion-form','deletion-code','deletion-confirm','deletion-submit','deletion-status','deletion-fields','deletion-language'])ids[id]={value:'',checked:false,disabled:false,textContent:'',dataset:{},handlers:{},addEventListener(type,callback){this.handlers[type]=callback;},focus(){this.focused=true;}};
 const labels=[...html.matchAll(/data-deletion-copy="(\w+)"/g)].map(x=>({dataset:{deletionCopy:x[1]},textContent:''}));labels.push(ids['deletion-submit']);ids['deletion-submit'].dataset.deletionCopy='submit';const main={};
 const context={document:{querySelector:s=>s==='main'?main:ids[s.slice(1)],querySelectorAll:()=>labels},navigator:{language:locale},fetch:fetcher,AbortController,JSON,setTimeout,clearTimeout};
 vm.runInNewContext(script,context,{timeout:1000});return {ids,labels,submit:()=>ids['account-deletion-form'].handlers.submit({preventDefault(){}}),change:()=>ids['deletion-code'].handlers.input()};
}
for(const locale of ['en-US','ru-RU','uk-UA'])test('explicit consent and completed code are required: '+locale,async()=>{
 let calls=0;const f=fixture(locale,async()=>{calls++;return Response.json({Status:'DeletionPending'});}),i=f.ids;
 assert.equal(i['deletion-language'].value,locale.slice(0,2));assert.equal(i['deletion-submit'].disabled,true);
 i['deletion-code'].value='a'.repeat(32);f.change();assert.equal(i['deletion-submit'].disabled,true);await f.submit();assert.equal(calls,0);
 i['deletion-confirm'].checked=true;f.change();assert.equal(i['deletion-submit'].disabled,false);await f.submit();assert.equal(calls,1);assert.equal(i['deletion-code'].value,'');assert.equal(i['deletion-fields'].disabled,true);assert.equal(i['deletion-status'].focused,true);assert.ok(i['deletion-status'].textContent.length>20);
});
test('double submission and completed confirmation cannot duplicate a request',async()=>{
 let resolve,calls=0;const pending=new Promise(r=>resolve=r),f=fixture('en',async()=>{calls++;return pending;}),i=f.ids;
 i['deletion-code'].value='b'.repeat(32);i['deletion-confirm'].checked=true;
 const first=f.submit();await f.submit();assert.equal(calls,1);resolve(Response.json({Status:'DeletionPending'}));await first;await f.submit();assert.equal(calls,1);
});
test('error responses and unknown successful bodies never claim account deletion',async()=>{
 for(const response of [Response.json({error:'RetryLater'},{status:429}),Response.json({error:'unavailable'},{status:503}),Response.json({ok:true})]){
  const f=fixture('en',async()=>response),i=f.ids;i['deletion-code'].value='c'.repeat(32);i['deletion-confirm'].checked=true;await f.submit();assert.equal(i['deletion-fields'].disabled,false);assert.notEqual(i['deletion-code'].value,'');assert.ok(!i['deletion-status'].textContent.startsWith('Request accepted.'));
 }
});
test('the capability is sent in JSON to the same origin with no cookies/referrer',async()=>{
 let seen;const f=fixture('en',async(url,options)=>{seen={url,options};return Response.json({Status:'DeletionPending'});}),i=f.ids;
 i['deletion-code'].value='d'.repeat(32);i['deletion-confirm'].checked=true;await f.submit();assert.equal(seen.url,'/api/account-deletion');assert.equal(seen.options.credentials,'omit');assert.equal(seen.options.referrerPolicy,'no-referrer');assert.equal(seen.options.redirect,'error');assert.deepEqual(JSON.parse(seen.options.body),{code:'d'.repeat(32),confirm:'DELETE'});
});

test('HTML pattern compiles with the modern browser unicode-sets flag',()=>{const pattern=html.match(/pattern="([^"]+)"/)[1];const expression=new RegExp('^(?:'+pattern+')$','v');assert.equal(expression.test('a'.repeat(32)),true);assert.equal(expression.test('A-_'+ 'a'.repeat(29)),true);assert.equal(expression.test('!'.repeat(32)),false);});
