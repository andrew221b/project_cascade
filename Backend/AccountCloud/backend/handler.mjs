const canonical='https://vickand.com/api/flying-ladders';
const paths=new Set(['/health','/admob/ssv','/notifications/apple','/notifications/google','/purchase/local-verify','/deletion/confirm']);
const headers={'Content-Type':'application/json','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'};
const json=(body,status)=>new Response(JSON.stringify(body),{status,headers});
// Compatibility for already installed clients and the existing deletion page.
// No Unity credentials, game records or publisher SDKs are used by this relay.
export async function handle(request,env,dependencies={}){
 const url=new URL(request.url),path=url.pathname.replace(/^\/api/,'');
 if(!paths.has(path))return json({error:'GameStateMovedToUnity'},410);
 const method=['/health','/admob/ssv'].includes(path)?'GET':'POST';if(request.method!==method)return json({error:'MethodNotAllowed'},405);
 if(url.search.length>8192)return json({error:'RequestBudget'},414);
 const interactive=['/purchase/local-verify','/deletion/confirm'].includes(path);
 if(interactive&&(!env.LEGACY_RELAY_TOKEN||!request.headers.get('cf-connecting-ip')))return json({error:'RelayNotConfigured'},503);
 if(path==='/deletion/confirm'&&request.headers.get('origin')!==url.origin)return json({error:'InvalidOrigin'},403);
 try{
  let body;
  if(method==='POST'){
   const limit=path==='/purchase/local-verify'?1536*1024:path==='/deletion/confirm'?1024:131072;
   if(!request.headers.get('content-type')?.startsWith('application/json'))return json({error:'ExpectedJSON'},415);
   let size=0;const chunks=[];for await(const chunk of request.body??[]){size+=chunk.length;if(size>limit)return json({error:'RequestBudget'},413);chunks.push(chunk);}
   body=Buffer.concat(chunks);
  }
  const forwarded={'Content-Type':'application/json'};
  if(request.headers.get('authorization')&&path==='/notifications/google')forwarded.Authorization=request.headers.get('authorization');
  if(request.headers.get('origin'))forwarded.Origin=request.headers.get('origin');
  if(interactive){forwarded['X-Flying-Relay']=env.LEGACY_RELAY_TOKEN;forwarded['X-Flying-Client-IP']=request.headers.get('cf-connecting-ip');}
  const response=await(dependencies.fetch??fetch)(canonical+path+url.search,{method,redirect:'manual',headers:forwarded,...(body?{body}:{}),signal:AbortSignal.timeout(28000)});
  if(response.status>=300&&response.status<400)return json({error:'RelayPending'},503);
  // Unity responses are small; never relay unbounded third-party output.
  if(Number(response.headers.get('content-length'))>65536)return json({error:'ResponseBudget'},503);
  const text=await response.text();if(text.length>65536)return json({error:'ResponseBudget'},503);
  return new Response(text,{status:response.status,headers});
 }catch{return json({error:'RelayPending'},503);}
}
