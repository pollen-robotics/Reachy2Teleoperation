using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace TeleopReachy
{
    public class ScenesManager : Singleton<ScenesManager>
    {
        public GameObject userTracker = null;

        public GameObject userInput = null;

        public GameObject ground = null;

        public GameObject XROrigin = null;

        void Start()
        {
            SceneManager.LoadScene("ConnectionScene", LoadSceneMode.Additive);

            EventManager.StartListening(EventNames.QuitApplication, QuitApplication);

            EventManager.StartListening(EventNames.EnterConnectionScene, LoadConnectionSceneEndUnloadMirrorScene);
            EventManager.StartListening(EventNames.QuitConnectionScene, UnloadConnectionSceneAndLoadMirrorScene);

            // EventManager.StartListening(EventNames.EnterMirrorScene, LoadMirrorScene);
            // EventManager.StartListening(EventNames.QuitMirrorScene, UnloadMirrorScene);

            EventManager.StartListening(EventNames.EnterTeleoperationScene, LoadTeleoperationSceneAndUnloadMirrorScene);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, UnloadTeleoperationSceneAndLoadMirrorScene);

            EventManager.StartListening(EventNames.ShowXRay, ShowXRay);
            EventManager.StartListening(EventNames.HideXRay, HideXRay);
        }

        void QuitApplication()
        {
            Debug.Log("Exiting app");
            Application.Quit();
        }

        private void UnloadRobotDataScene()
        {
            SceneManager.UnloadSceneAsync("RobotDataScene");
            Debug.Log("[ScenesManager] Function UnloadRobotDataScene done"); 
        }

        private void LoadConnectionSceneEndUnloadMirrorScene()
        {
            Debug.Log("[ScenesManager] LoadConnectionSceneEndUnloadMirrorScene");
            if(SceneManager.GetSceneByName("RobotDataScene").isLoaded)
            {
                Debug.Log("[ScenesManager] Unloading RobotDataScene");
                UnloadRobotDataScene();
                Debug.Log("[ScenesManager] Unloading MirrorScene");
                UnloadMirrorScene();
            }
            LoadConnectionScene();
            Debug.Log("[ScenesManager] Connection Scene loading");
        }

        private void LoadConnectionScene()
        {
            Debug.Log("Loading Connection Scene");
            ground.SetActive(true);
            userInput.SetActive(false);
            userTracker.SetActive(false);
            SceneManager.LoadScene("ConnectionScene", LoadSceneMode.Additive);
            Debug.Log("Connection Scene loaded");
        }

        private void UnloadConnectionSceneAndLoadMirrorScene()
        {
            SceneManager.UnloadSceneAsync("ConnectionScene");
            userTracker.SetActive(true);
            userInput.SetActive(true);
            StartCoroutine(LoadRobotDataSceneAndMirrorScene());
        }

        IEnumerator LoadRobotDataSceneAndMirrorScene()
        {
            SceneManager.LoadScene("RobotDataScene", LoadSceneMode.Additive);
            yield return null;
            EventManager.TriggerEvent(EventNames.RobotDataSceneLoaded);

            LoadMirrorScene();
        }

        private void LoadMirrorScene()
        {
            ground.SetActive(true);
            StartCoroutine(LoadTransitionRoom());
        }

        IEnumerator LoadTransitionRoom()
        {
            while(!SceneManager.GetSceneByName("RobotDataScene").isLoaded)
            {
                yield return null;
            }
            SceneManager.LoadScene("MirrorScene", LoadSceneMode.Additive);
            yield return null;
            EventManager.TriggerEvent(EventNames.MirrorSceneLoaded);
        }

        private void UnloadMirrorScene()
        {
            SceneManager.UnloadSceneAsync("MirrorScene");
            Debug.Log("[ScenesManager] Function UnloadMirrorScene done");
            ground.SetActive(false);
        }

        private void LoadTeleoperationSceneAndUnloadMirrorScene()
        {
            StartCoroutine(LoadTeleoperationRoom());
            UnloadMirrorScene();
        }

        IEnumerator LoadTeleoperationRoom()
        {
            SceneManager.LoadScene("TeleoperationScene", LoadSceneMode.Additive);
            yield return null;
            EventManager.TriggerEvent(EventNames.TeleoperationSceneLoaded);
        }

        private void UnloadTeleoperationSceneAndLoadMirrorScene()
        {
            SceneManager.UnloadSceneAsync("TeleoperationScene");
            LoadMirrorScene();
        }

        private void ShowXRay()
        {
            ToogleXRRayInteractors(true);
        }

        private void HideXRay()
        {
            ToogleXRRayInteractors(false);
        }

        private void ToogleXRRayInteractors(bool activated)
        {
            XRInteractorLineVisual[] xrlines = XROrigin.GetComponentsInChildren<XRInteractorLineVisual>();
            foreach (XRInteractorLineVisual xr in xrlines)
                xr.enabled = activated;
        }
    }
}
