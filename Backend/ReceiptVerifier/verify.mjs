import { timingSafeEqual, createHash } from 'node:crypto';

// The caller never supplies a trusted verdict. Store adapters verify publisher evidence.
export const products = new Map([
  ['cascade.no_automatic_ads', 'Non-Consumable'],
  ['cascade.no_ads_week', 'Subscription'], ['cascade.no_ads_month', 'Subscription'], ['cascade.no_ads_year', 'Subscription'],
  ['cascade.coins_240', 'Consumable'], ['cascade.coins_480', 'Consumable'],
  ['cascade.coins_720', 'Consumable'], ['cascade.coins_960', 'Consumable'],
  ['cascade.coins_600', 'Consumable'], ['cascade.coins_1080', 'Consumable']
]);
export async function verifyPurchase(proof, { bundleId, apple, google, appleSubscription, googleSubscription, purchaseIdentity }) {
  if (!proof || proof.bundleId !== bundleId || !products.has(proof.productId) ||
      typeof proof.transactionId !== 'string' || !proof.transactionId.trim() || proof.transactionId.length > 256) return { valid: false };
  const subscription = products.get(proof.productId) === 'Subscription';
  let purchase, expiresUnix;
  if (proof.store === 'AppleAppStore') {
    if (typeof proof.jws !== 'string' || !proof.jws) return { valid: false };
    purchase = await apple(proof);
    if (purchaseIdentity && purchase.appAccountToken?.toLowerCase() !== purchaseIdentity.appleToken) return { valid: false };
    if (purchase.bundleId !== bundleId || purchase.type !== (subscription ? 'Auto-Renewable Subscription' : products.get(proof.productId)) ||
        purchase.inAppOwnershipType !== 'PURCHASED' || !subscription && purchase.revocationDate != null) return { valid: false };
    if (subscription) {
      if (typeof appleSubscription !== 'function') return { valid: false };
      expiresUnix = await appleSubscription(proof, purchase);
    }
  } else if (proof.store === 'GooglePlay') {
    const envelope = JSON.parse(proof.receipt);
    if (envelope.Store !== 'GooglePlay') return { valid: false };
    const payload = JSON.parse(envelope.Payload);
    const receipt = JSON.parse(payload.json);
    if (typeof receipt.purchaseToken !== 'string' || !receipt.purchaseToken || receipt.packageName !== bundleId ||
        receipt.productId !== proof.productId) return { valid: false };
    const data = subscription ? await googleSubscription?.(proof.productId, receipt.purchaseToken) : await google(proof.productId, receipt.purchaseToken);
    if (!data) return { valid: false };
    if (subscription) {
      if (data.packageName !== bundleId || data.latestOrderId !== proof.transactionId ||
          data.externalAccountIdentifiers?.obfuscatedExternalAccountId !== purchaseIdentity?.googleAccountId ||
          !data.lineItems?.some(item => item.productId === proof.productId)) return { valid: false };
      const allowed = ['SUBSCRIPTION_STATE_ACTIVE', 'SUBSCRIPTION_STATE_IN_GRACE_PERIOD', 'SUBSCRIPTION_STATE_CANCELED'];
      const latest = Math.max(0, ...data.lineItems.filter(item => item.productId === proof.productId).map(item => Date.parse(item.expiryTime) / 1000 || 0));
      expiresUnix = allowed.includes(data.subscriptionState) && latest > Date.now() / 1000 ? Math.floor(latest) : 0;
      purchase = { productId: proof.productId, transactionId: proof.transactionId, quantity: 1 };
    } else {
    // The authenticated publisher API is authoritative; client receipt purchaseState is ignored.
    if (data.purchaseState !== 0 || (data.quantity ?? 1) !== 1 || data.orderId !== proof.transactionId) return { valid: false };
    if (purchaseIdentity && data.obfuscatedExternalAccountId !== purchaseIdentity.googleAccountId) return { valid: false };
    purchase = { productId: proof.productId, transactionId: data.orderId, quantity: data.quantity ?? 1 };
    }
  } else return { valid: false };
  return purchase.productId === proof.productId && purchase.transactionId === proof.transactionId && purchase.quantity === 1
    ? { valid: true, productId: proof.productId, transactionId: proof.transactionId, ...(subscription ? { expiresUnix: Number.isSafeInteger(expiresUnix) && expiresUnix >= 0 ? expiresUnix : 0 } : {}) } : { valid: false };
}

const contextId = value => typeof value === 'string' && /^[A-Za-z0-9][A-Za-z0-9_-]{0,127}$/.test(value);
export function validateServiceConfig({ serviceToken, expectedProjectId, expectedEnvironmentId }) {
  if (typeof serviceToken !== 'string' || !/^[A-Za-z0-9._~+/=-]{32,256}$/.test(serviceToken) ||
      !contextId(expectedProjectId) || !contextId(expectedEnvironmentId)) throw new Error('Missing or invalid service authorization configuration');
}
export function authorizeService(authorization, serviceToken) {
  if (typeof authorization !== 'string' || authorization.length > 270 || typeof serviceToken !== 'string' || !serviceToken) return false;
  const match = /^Bearer ([A-Za-z0-9._~+/=-]+)$/i.exec(authorization);
  if (!match) return false;
  const submitted = Buffer.from(match[1], 'utf8'), expected = Buffer.from(serviceToken, 'utf8');
  return submitted.length === expected.length && timingSafeEqual(submitted, expected);
}

// This endpoint trusts identity only from an authenticated server-to-server UGS call.
// Cloud Code, rather than a client-selected player ID, supplies its invocation context.
export async function verifyServicePurchase(proof, authorization, options) {
  validateServiceConfig(options);
  const deny = status => ({ status, body: { Valid: false } });
  if (!authorizeService(authorization, options.serviceToken)) return deny(401);
  if (!proof || typeof proof !== 'object' || Array.isArray(proof) || !contextId(proof.playerId)) return deny(400);
  if (proof.projectId !== options.expectedProjectId || proof.environmentId !== options.expectedEnvironmentId) return deny(403);
  const purchase = await verifyPurchase(proof, { ...options, purchaseIdentity: accountBinding(proof.projectId, proof.environmentId, proof.playerId) });
  if (!purchase.valid) return deny(200);
  return { status: 200, body: {
    Valid: true, PlayerBindingVerified: true, CatalogVerified: true,
    PlayerId: proof.playerId, ProjectId: options.expectedProjectId, EnvironmentId: options.expectedEnvironmentId,
    ProductId: purchase.productId, TransactionId: purchase.transactionId, Platform: proof.store, BundleId: options.bundleId,
    ...(purchase.expiresUnix !== undefined ? { SubscriptionExpiresUnix: purchase.expiresUnix } : {})
  } };
}

export function accountBinding(project, environment, player) {
  const hash = createHash('sha256').update(`cascade-purchase-v1|${project}|${environment}|${player}`, 'utf8').digest('hex');
  return { googleAccountId: hash, appleToken: `${hash.slice(0, 8)}-${hash.slice(8, 12)}-${hash.slice(12, 16)}-${hash.slice(16, 20)}-${hash.slice(20, 32)}` };
}
