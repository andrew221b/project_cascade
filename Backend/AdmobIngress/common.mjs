export class HttpFailure extends Error{constructor(status,code){super(code);this.status=status;}}
export async function boundedJson(response,limit){
 if(Number(response.headers.get('content-length'))>limit)throw new HttpFailure(413,'ResponseBudget');
 const reader=response.body?.getReader();if(!reader)throw new HttpFailure(400,'JSONRequired');let total=0;const chunks=[];
 try{while(true){const{done,value}=await reader.read();if(done)break;total+=value.byteLength;if(total>limit)throw new HttpFailure(413,'RequestBudget');chunks.push(value);}}catch(error){await reader.cancel().catch(()=>{});throw error;}
 const bytes=new Uint8Array(total);let offset=0;for(const chunk of chunks){bytes.set(chunk,offset);offset+=chunk.length;}
 try{return JSON.parse(new TextDecoder().decode(bytes));}catch{throw new HttpFailure(400,'InvalidJSON');}
}
export const un64=value=>Uint8Array.from(atob(value.replace(/-/g,'+').replace(/_/g,'/').padEnd(Math.ceil(value.length/4)*4,'=')),c=>c.charCodeAt(0));
export const sha=async value=>Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256',new TextEncoder().encode(value))),b=>b.toString(16).padStart(2,'0')).join('');
