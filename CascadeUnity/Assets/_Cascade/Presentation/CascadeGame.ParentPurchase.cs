using Cascade.Application;

namespace Cascade.Presentation
{
    public sealed partial class CascadeGame
    {
        void RequestStorePurchase(string product)
        {
            if(purchaseStore==null||!PurchaseCatalog.IsKnown(product))return;
            if(purchaseStore.State==PurchaseState.Retry){purchaseStore.RetryConnection();return;}
            purchaseStore.Buy(product);
        }
    }
}
