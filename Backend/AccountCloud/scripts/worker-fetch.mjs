// Adapter for the official Apple SDK's node-fetch transport in Workers.
// Cryptographic verification and OCSP response checks stay in the pinned SDK.
export const Headers=globalThis.Headers;
export const Request=globalThis.Request;
export const Response=globalThis.Response;
export default async function workerFetch(input,options={}){
 const {timeout,...init}=options;
 if(typeof timeout==='number'&&timeout>0){const deadline=AbortSignal.timeout(timeout);init.signal=init.signal?AbortSignal.any([init.signal,deadline]):deadline;}
 const response=await globalThis.fetch(input,init);
 if(!response.ok&&new URL(input).hostname==='ocsp.apple.com')console.warn('AppleOCSPTransportFailure',{status:response.status});
 // node-fetch 2's buffer() is used by Apple's OCSP validator.
 Object.defineProperty(response,'buffer',{value:async()=>Buffer.from(await response.arrayBuffer())});
 return response;
}
