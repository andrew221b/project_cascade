import { createServer } from 'node:http';
import { readFileSync } from 'node:fs';
import { SignedDataVerifier, AppStoreServerAPIClient, Environment } from '@apple/app-store-server-library';
import { GoogleAuth } from 'google-auth-library';
import { authorizeService, validateServiceConfig, verifyServicePurchase } from './verify.mjs';

const required = name => { const value = process.env[name]; if (!value) throw new Error(`Missing ${name}`); return value; };
const bundleId = required('BUNDLE_ID');
const service = { serviceToken: required('SERVICE_BEARER_TOKEN'), expectedProjectId: required('UGS_PROJECT_ID'), expectedEnvironmentId: required('UGS_ENVIRONMENT_ID') };
validateServiceConfig(service);
const mode = required('APPLE_ENVIRONMENT');
if (!['Sandbox', 'Production'].includes(mode)) throw new Error('APPLE_ENVIRONMENT must be Sandbox or Production');
const environment = mode === 'Production' ? Environment.PRODUCTION : Environment.SANDBOX;
const rootCAs = required('APPLE_ROOT_CA_FILES').split(',').map(path => readFileSync(path.trim()));
const appAppleId = mode === 'Production' ? Number(required('APPLE_APP_ID')) : undefined;
if (mode === 'Production' && (!Number.isSafeInteger(appAppleId) || appAppleId <= 0)) throw new Error('Invalid APPLE_APP_ID');
const verifier = new SignedDataVerifier(rootCAs, true, environment, bundleId, appAppleId);
const appleAPI = new AppStoreServerAPIClient(readFileSync(required('APPLE_PRIVATE_KEY_FILE'), 'utf8'), required('APPLE_KEY_ID'), required('APPLE_ISSUER_ID'), bundleId, environment);
const googleAuth = new GoogleAuth({ scopes: ['https://www.googleapis.com/auth/androidpublisher'] });
const google = await googleAuth.getClient();
const adapters = {
  bundleId, ...service,
  apple: async proof => {
    const submitted = await verifier.verifyAndDecodeTransaction(proof.jws);
    if (submitted.transactionId !== proof.transactionId || submitted.productId !== proof.productId) return submitted;
    // Refresh from Apple so an older signed purchase cannot hide its subsequent revocation.
    const latest = await appleAPI.getTransactionInfo(proof.transactionId);
    return verifier.verifyAndDecodeTransaction(latest.signedTransactionInfo);
  },
  google: async (productId, token) => {
    const response = await google.request({ timeout: 10000, url: `https://androidpublisher.googleapis.com/androidpublisher/v3/applications/${encodeURIComponent(bundleId)}/purchases/products/${encodeURIComponent(productId)}/tokens/${encodeURIComponent(token)}` });
    return response.data;
  }
};
const server = createServer(async (request, response) => {
  response.setHeader('Content-Type', 'application/json'); response.setHeader('Cache-Control', 'no-store');
  if (request.method !== 'POST' || request.url !== '/verify') { response.writeHead(404).end('{"Valid":false}'); return; }
  if (!authorizeService(request.headers.authorization, service.serviceToken)) { response.writeHead(401).end('{"Valid":false}'); return; }
  if (!(request.headers['content-type'] ?? '').startsWith('application/json')) { response.writeHead(415).end('{"Valid":false}'); return; }
  let bytes = 0; const chunks = [];
  try {
    for await (const chunk of request) {
      bytes += chunk.length; if (bytes > 131072) { response.writeHead(413).end('{"Valid":false}'); return; } chunks.push(chunk);
    }
    const proof = JSON.parse(Buffer.concat(chunks).toString('utf8'));
    let timeout;
    try {
      const result = await Promise.race([verifyServicePurchase(proof, request.headers.authorization, adapters), new Promise((_, reject) => { timeout = setTimeout(() => reject(new Error('Store timeout')), 15000); })]);
      response.writeHead(result.status).end(JSON.stringify(result.body));
    } finally { clearTimeout(timeout); }
  } catch { response.writeHead(503).end('{"Valid":false}'); } // Never log receipts, tokens or private keys.
});
server.requestTimeout = 20000; server.headersTimeout = 10000;
server.listen(Number(process.env.PORT ?? 8080), '0.0.0.0');
