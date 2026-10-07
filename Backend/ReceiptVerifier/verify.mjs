// The caller never supplies a trusted verdict. Store adapters verify publisher evidence.
export const products = new Map([
  ['cascade.no_automatic_ads', 'Non-Consumable'],
  ['cascade.coins_240', 'Consumable'], ['cascade.coins_480', 'Consumable']
]);
export async function verifyPurchase(proof, { bundleId, apple, google }) {
  if (!proof || proof.bundleId !== bundleId || !products.has(proof.productId) ||
      typeof proof.transactionId !== 'string' || !proof.transactionId.trim() || proof.transactionId.length > 256) return { valid: false };
  let purchase;
  if (proof.store === 'AppleAppStore') {
    if (typeof proof.jws !== 'string' || !proof.jws) return { valid: false };
    purchase = await apple(proof);
    if (purchase.bundleId !== bundleId || purchase.type !== products.get(proof.productId) ||
        purchase.revocationDate != null || purchase.inAppOwnershipType !== 'PURCHASED') return { valid: false };
  } else if (proof.store === 'GooglePlay') {
    const envelope = JSON.parse(proof.receipt);
    if (envelope.Store !== 'GooglePlay') return { valid: false };
    const payload = JSON.parse(envelope.Payload);
    const receipt = JSON.parse(payload.json);
    if (typeof receipt.purchaseToken !== 'string' || !receipt.purchaseToken || receipt.packageName !== bundleId ||
        receipt.productId !== proof.productId) return { valid: false };
    const data = await google(proof.productId, receipt.purchaseToken);
    // The authenticated publisher API is authoritative; client receipt purchaseState is ignored.
    if (data.purchaseState !== 0 || (data.quantity ?? 1) !== 1 || data.orderId !== proof.transactionId) return { valid: false };
    purchase = { productId: proof.productId, transactionId: data.orderId, quantity: data.quantity ?? 1 };
  } else return { valid: false };
  return purchase.productId === proof.productId && purchase.transactionId === proof.transactionId && purchase.quantity === 1
    ? { valid: true, productId: proof.productId, transactionId: proof.transactionId } : { valid: false };
}
