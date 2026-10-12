using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CascadeAccountModule;

public sealed class ModuleSetup : ICloudCodeSetup
{
    public void Setup(ICloudCodeConfig config)
    {
        config.Dependencies.AddSingleton<IGameApiClient>(GameApiClient.Create());
        config.Dependencies.AddSingleton(new PurchaseVerifier());
    }
}
