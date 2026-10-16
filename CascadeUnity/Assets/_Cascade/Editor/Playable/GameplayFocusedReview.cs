using System;
using System.Collections;
using System.IO;
using Cascade.Presentation;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
namespace Cascade.Editor.Playable
{
    public static class GameplayFocusedReview
    {
        public static void Start(){var host=new GameObject("Isolated gameplay reference review");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<ReferenceShopLayout>().StartCoroutine(Run(host));}
        static IEnumerator Run(GameObject host){string dir=Path.Combine(Path.GetTempPath(),"CascadeGameplayFocusedReview",Guid.NewGuid().ToString("N"));CascadeGame.EditorProfileDirectory=dir;UnityEngine.Application.runInBackground=true;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");var game=UnityEngine.Object.FindFirstObjectByType<CascadeGame>();yield return GameplayReferenceReview.Extra(game,"GameplayReferenceOct14");CascadeGame.EditorProfileDirectory=null;yield return SceneManager.LoadSceneAsync("Assets/_Cascade/Scenes/CascadePlayable.unity");UnityEngine.Object.Destroy(host);if(Directory.Exists(dir))Directory.Delete(dir,true);EditorApplication.isPlaying=false;}
    }
}
