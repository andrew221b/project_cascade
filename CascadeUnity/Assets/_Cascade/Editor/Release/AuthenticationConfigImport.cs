using System;
using System.IO;
using Cascade.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Unity.Services.Authentication.PlayerAccounts;

namespace Cascade.Editor.Release
{
    public static class AuthenticationConfigImport
    {
        [MenuItem("CASCADE/Release/Check Apple and Google sign-in configuration")]
        public static void Check()
        {
            var config=Read();
            Debug.Log(config.HasClient?"CASCADE account config is valid. Apply it after verifying provider redirects and the backend.":
                "CASCADE account config is valid and inactive. Fill unityPlayerAccountsClientId after creating a Unity Player Accounts client.");
        }
        static AuthenticationConfiguration Read()
        {
            string root=Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
            return AuthenticationConfiguration.Read(Path.Combine(root,"Configs","authentication.json"),UnityEngine.Application.cloudProjectId);
        }
        [MenuItem("CASCADE/Release/Apply Apple and Google sign-in configuration")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Apply configuration outside Play Mode.");
            var config=Read();
            var cloud=AssetDatabase.LoadAssetAtPath<CloudPlayerSettings>("Assets/_Cascade/Content/Resources/CascadeArt/CloudPlayerSettings.asset");
            if(cloud==null)throw new InvalidOperationException("Cloud player settings asset is missing.");
            var type=typeof(PlayerAccountService).Assembly.GetType("Unity.Services.Authentication.PlayerAccounts.UnityPlayerAccountSettings",true);
            const string path="Assets/_Cascade/Content/Resources/UnityPlayerAccountSettings.asset";
            var account=Resources.Load("UnityPlayerAccountSettings") as ScriptableObject;
            if(account==null&&config.HasClient){account=ScriptableObject.CreateInstance(type);AssetDatabase.CreateAsset(account,path);}
            if(account!=null){
                var data=new SerializedObject(account);var id=data.FindProperty("clientId");
                if(id==null)throw new InvalidOperationException("Installed Player Accounts client ID field is unavailable.");
                Undo.RecordObject(account,"Configure CASCADE account sign-in");
                id.stringValue=config.unityPlayerAccountsClientId;
                // Use the SDK's project-bound redirects; avoid stale overrides from another app.
                data.FindProperty("useCustomDeepLinkUri").boolValue=false;
                data.FindProperty("scopeMask").intValue=1|4; // openid + offline_access; no email needed by the game.
                data.ApplyModifiedProperties();EditorUtility.SetDirty(account);
            }
            Undo.RecordObject(cloud,"Configure CASCADE cloud save");
            cloud.projectId=config.unityProjectId;cloud.environment=config.unityEnvironmentName;cloud.moduleName=config.cloudCodeModuleName;
            cloud.playerAccountsConfigured=config.HasClient;cloud.enabledForPlayers=config.cloudCodeDeployedAndVerified;
            cloud.purchaseVerificationConfigured=config.purchaseVerificationConfigured;
            EditorUtility.SetDirty(cloud);AssetDatabase.SaveAssets();
            Debug.Log("CASCADE: account configuration applied; cloud active="+cloud.enabledForPlayers+". No credentials logged.");
        }
    }
    public sealed class AuthenticationConfigBeforeBuild:IPreprocessBuildWithReport
    {
        public int callbackOrder=>-1000;
        public void OnPreprocessBuild(BuildReport report)
        {
            try{AuthenticationConfigImport.Apply();}
            catch(Exception e){throw new BuildFailedException("CASCADE account configuration: "+e.Message);}
        }
    }
}
