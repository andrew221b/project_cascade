import {assets} from './assets.mjs';
const HOST='flyingladders.vickand.com',API='/api/account-deletion',VERSION='20261029.1';
const privateHeaders={'Content-Type':'application/json','Cache-Control':'no-store','Referrer-Policy':'no-referrer','X-Content-Type-Options':'nosniff','X-FlyingLadders-Web':VERSION};
const json=(error,status)=>Response.json({error},{status,headers:privateHeaders});
async function boundedBody(stream,maximum){
  let size=0;const chunks=[];
  for await(const chunk of stream??[]){size+=chunk.byteLength;if(size>maximum)throw Error('BodyBudget');chunks.push(chunk);}
  const bytes=new Uint8Array(size);let offset=0;for(const chunk of chunks){bytes.set(chunk,offset);offset+=chunk.byteLength;}return bytes;
}
export function createHandler(originFetch=fetch){
  return async function handle(request,env){
    const url=new URL(request.url),path=url.pathname.replace(/\/$/,'')||'/';
    if(url.hostname!==HOST)return json('NotFound',404);
    if(path===API){
      if(request.method!=='POST')return new Response(null,{status:405,headers:{...privateHeaders,Allow:'POST'}});
      if(url.protocol!=='https:'||request.headers.get('origin')!=='https://'+HOST)return json('InvalidOrigin',403);
      if(!request.headers.get('content-type')?.startsWith('application/json'))return json('ExpectedJSON',415);
      if(!env.ACCOUNT_GATEWAY?.fetch)return json('ServiceUnavailable',503);
      const ip=request.headers.get('cf-connecting-ip');
      if(!ip)return json('EdgeIdentityRequired',503);
      try{
        const body=await boundedBody(request.body,1024);let data;
        try{data=JSON.parse(new TextDecoder().decode(body));}catch{return json('InvalidDeletionCode',400);}
        if(!data||Array.isArray(data)||Object.keys(data).some(k=>!['code','confirm'].includes(k))||!/^[A-Za-z0-9_-]{32}$/.test(data.code??'')||data.confirm!=='DELETE')return json('InvalidDeletionCode',400);
        const upstream=await env.ACCOUNT_GATEWAY.fetch('https://vickand.com/api/flying-ladders/deletion/confirm',{method:'POST',headers:{'Content-Type':'application/json',Origin:'https://'+HOST,'CF-Connecting-IP':ip},body:JSON.stringify({code:data.code,confirm:'DELETE'}),redirect:'manual',signal:AbortSignal.timeout(30000)});
        if(upstream.status>=300&&upstream.status<400)return json('ServiceUnavailable',503);
        const result=await boundedBody(upstream.body,65536);
        return new Response(result,{status:upstream.status,headers:privateHeaders});
      }catch(error){return json(error?.message==='BodyBudget'?'RequestBudget':'ServiceUnavailable',error?.message==='BodyBudget'?413:503);}
    }
    if(!['GET','HEAD'].includes(request.method))return new Response(null,{status:405,headers:{Allow:'GET, HEAD'}});
    if(url.protocol!=='https:')return Response.redirect('https://'+HOST+url.pathname,301);
    const asset=assets[url.pathname]??assets[path];
    if(asset){
      if(asset.redirect)return new Response(null,{status:308,headers:{Location:asset.redirect,'Cache-Control':'no-store'}});
      const headers={'Content-Type':asset.type,'Cache-Control':path==='/delete-account'?'no-store':'no-cache','X-Content-Type-Options':'nosniff','Referrer-Policy':path==='/delete-account'?'no-referrer':'strict-origin-when-cross-origin','Permissions-Policy':'camera=(), microphone=(), geolocation=()','X-Frame-Options':'DENY','X-FlyingLadders-Web':VERSION,ETag:'"'+asset.sha256+'"'};
      if(path==='/delete-account'){headers['X-Robots-Tag']='noindex, nofollow';headers['Content-Security-Policy']="form-action 'self'; frame-ancestors 'none'; base-uri 'self'; object-src 'none'";}
      return new Response(request.method==='HEAD'?null:asset.body,{status:200,headers});
    }
    // The retained Vercel origin serves the existing images, fonts and scripts.
    // The hostname is fixed above; client input can never select another origin.
    return originFetch(request);
  };
}
export default {fetch:createHandler()};
