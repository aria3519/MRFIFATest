using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Jisu.Utils
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private LoadSceneMode sceneMode = LoadSceneMode.Additive;
        [SerializeField] private bool isStartLoadOnAwake;
        [SerializeField] private bool isLoadedSceneActive;
        [SerializeField] private bool isDestroyOnLoadScene;

        public bool IsCompleteLoadScene { get; private set; }

        private void Awake()
        {
            if (isStartLoadOnAwake == true)
                Async_LoadScene();
        }

        private async void Async_LoadScene()
        {
            AsyncOperation async;

            async = SceneManager.LoadSceneAsync(sceneName, sceneMode);

            while (!async.isDone)
            {
                await Task.Yield();
            }

            if (isLoadedSceneActive)
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            IsCompleteLoadScene = true;

            if(IsCompleteLoadScene && isDestroyOnLoadScene)
                Destroy(this.gameObject);
        }

        public void LoadSceneManual(in string loadedSceneName, in LoadSceneMode mode = LoadSceneMode.Additive)
        {
            sceneName = loadedSceneName;
            sceneMode = mode;

            Async_LoadScene();
        }
    }
}
