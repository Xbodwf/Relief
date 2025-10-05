using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Relief.Modules.BuiltInModules
{
    public class ReliefUnityEvents : MonoBehaviour
    {
        public EventSystem EventSystem { get; set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.quitting += OnApplicationQuitting;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Application.quitting -= OnApplicationQuitting;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystem?.TriggerEvent("onSceneLoaded", scene.name, mode.ToString());
        }

        private void OnApplicationQuitting()
        {
            EventSystem?.TriggerEvent("onGameClosing");
        }
    }
}