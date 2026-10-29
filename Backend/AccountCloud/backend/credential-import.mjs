import {createPrivateKey,createPublicKey,verify} from 'node:crypto';
import {Store,Failure,digest} from './store.mjs';

const id='publishers-v1';
const names=['APPLE_PRIVATE_KEY','GOOGLE_CREDENTIALS_JSON','SERVICE_BEARER_TOKEN'];
const scope=(env,value)=>value.projectId===env.UGS_PROJECT_ID&&value.environmentId===env.UGS_ENVIRONMENT_ID&&value.bundleId===env.BUNDLE_ID;
const fail=(status,code)=>{throw new Failure(status,code);};

function values(env,value){
 if(!value||typeof value!=='object'||Array.isArray(value)||Object.keys(value).length!==names.length||names.some(name=>typeof value[name]!=='string')||Object.keys(value).some(name=>!names.includes(name)))fail(400,'InvalidCredentialFields');
 if(!/^[A-Za-z0-9_-]{43,128}$/.test(value.SERVICE_BEARER_TOKEN))fail(400,'InvalidServiceCredential');
 try{
  const apple=createPrivateKey(value.APPLE_PRIVATE_KEY);
  if(apple.asymmetricKeyType!=='ec'||apple.asymmetricKeyDetails?.namedCurve!=='prime256v1')throw new Error();
  const google=JSON.parse(value.GOOGLE_CREDENTIALS_JSON);
  if(google.type!=='service_account'||google.client_email!==env.GOOGLE_PUSH_EMAIL||google.token_uri!=='https://oauth2.googleapis.com/token'||typeof google.private_key_id!=='string'||createPrivateKey(google.private_key).asymmetricKeyType!=='rsa')throw new Error();
 }catch{fail(400,'InvalidPublisherCredentials');}
 return Object.fromEntries(names.map(name=>[name,value[name]]));
}

export async function loadCredentials(env){
 if(!env.DB||!env.DATA_KEY)return env;
 const store=new Store(env.DB,env.DATA_KEY);
 const row=await store.one('SELECT sealed FROM server_credentials WHERE id=?',id);
 if(!row)return env;
 try{
  const saved=JSON.parse(store.unseal(row.sealed));
  if(saved.purpose!==id||!scope(env,saved))throw new Error();
  // Read only the known fields; no record can alter a project, environment or DATA_KEY.
  return {...env,...values(env,saved.values)};
 }catch{fail(503,'StoredCredentialsUnavailable');}
}

export async function importCredentials(request,env,clock=()=>Date.now()){
 const now=clock(),deadline=Number(env.CREDENTIAL_IMPORT_DEADLINE)*1000;
 if(!env.CREDENTIAL_IMPORT_PUBLIC_KEY)fail(404,'NotFound');
 if(!Number.isSafeInteger(deadline)||deadline<=now||deadline>now+3600000)fail(410,'CredentialImportExpired');
 if(request.method!=='POST')fail(405,'MethodNotAllowed');
 if(request.headers.has('origin'))fail(403,'CredentialImportOrigin');
 if(!request.headers.get('content-type')?.startsWith('application/json'))fail(415,'ExpectedJSON');
 const signature=request.headers.get('x-cascade-import-signature');
 if(!signature||!/^[A-Za-z0-9_-]{80,110}$/.test(signature))fail(401,'CredentialImportAuthentication');
 const chunks=[];let size=0;
 for await(const chunk of request.body??[]){size+=chunk.length;if(size>32768)fail(413,'CredentialImportBudget');chunks.push(chunk);}
 const raw=Buffer.concat(chunks),origin=new URL(request.url).origin;
 const signed=Buffer.concat([Buffer.from('cascade-credential-import-v1\n'+origin+'\n/api/admin/import-credentials\n'),raw]);
 try{if(!verify('sha256',signed,createPublicKey(env.CREDENTIAL_IMPORT_PUBLIC_KEY),Buffer.from(signature,'base64url')))throw new Error();}catch{fail(401,'CredentialImportAuthentication');}
 let data;try{data=JSON.parse(raw.toString('utf8'));}catch{fail(400,'InvalidJSON');}
 if(!data||data.purpose!==id||!scope(env,data))fail(403,'CredentialImportScope');
 if(!Number.isSafeInteger(data.issuedAt)||data.issuedAt<now-300000||data.issuedAt>now+30000)fail(403,'CredentialImportTimestamp');
 const secrets=values(env,data.values),store=new Store(env.DB,env.DATA_KEY);
 const sealed=store.seal(JSON.stringify({purpose:id,projectId:env.UGS_PROJECT_ID,environmentId:env.UGS_ENVIRONMENT_ID,bundleId:env.BUNDLE_ID,values:secrets}));
 // Exactly one initial installation, even if signed requests race or are replayed.
 const result=await store.run('INSERT INTO server_credentials(id,sealed,signer,created) VALUES(?,?,?,?) ON CONFLICT(id) DO NOTHING',id,sealed,digest(env.CREDENTIAL_IMPORT_PUBLIC_KEY),Math.floor(now/1000));
 if(result.meta.changes!==1)fail(409,'CredentialsAlreadyInstalled');
 return {installed:true,configuredKeys:names};
}
