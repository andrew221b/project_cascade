using System;
using System.IO;
using Cascade.Editor.Playable;
using Cascade.Presentation;
using UnityEditor;
using UnityEngine;
using Unity.Services.Authentication.PlayerAccounts;

namespace Cascade.Editor.Release
{
    public static class AccountActivationValidation
    {
        // Native Editor validation only; does not authenticate, contact cloud endpoints or build a player.
        public static void Run()
        {
            AuthenticationConfigImport.Apply();
            var cloud=Resources.Load<CloudPlayerSettings>("CascadeArt/CloudPlayerSettings");
            string root=Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            var config=AuthenticationConfiguration.Read(Path.Combine(root,"Configs/authentication.json"),UnityEngine.Application.cloudProjectId);
            if(cloud==null||cloud.enabledForPlayers!=config.cloudCodeDeployedAndVerified||cloud.playerAccountsConfigured!=config.HasClient
                ||cloud.purchaseVerificationConfigured!=config.purchaseVerificationConfigured||cloud.environment!=config.unityEnvironmentName)
                throw new InvalidDataException("Applied account settings differ from public configuration.");
            var type=typeof(PlayerAccountService).Assembly.GetType("Unity.Services.Authentication.PlayerAccounts.UnityPlayerAccountSettings",true);
            var scratch=ScriptableObject.CreateInstance(type);
            try{
                var data=new SerializedObject(scratch);
                if(data.FindProperty("clientId")==null||data.FindProperty("scopeMask")==null||data.FindProperty("useCustomDeepLinkUri")==null)
                    throw new InvalidDataException("Installed Player Accounts SDK configuration contract has changed.");
            }finally{UnityEngine.Object.DestroyImmediate(scratch);}
            LocalizationAssets.Build();
            Directory.CreateDirectory(Path.Combine(root,"Reports/AccountActivation-Oct10"));
            File.WriteAllText(Path.Combine(root,"Reports/AccountActivation-Oct10/native-validation.txt"),
                "PASS: native Editor compiled account runtime/editor sources; JSON applied via SerializedObject; SDK fields available; EN/RU/UK tables updated.\n"+
                "No live login, backend deployment, player build or device verification. Cloud active="+cloud.enabledForPlayers+".\n");
            Debug.Log("CASCADE native account configuration validation PASS; no player build or cloud connection.");
        }
    }
}
