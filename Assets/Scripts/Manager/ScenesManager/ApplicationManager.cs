using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace TeleopReachy
{
    public class ApplicationManager : MonoBehaviour
    {
        public GameObject userTracker = null;

        public GameObject canvasOnlineMenu = null;

        public Transform menuParent = null;

        public GameObject userInput = null;

        public GameObject ground = null;

        public GameObject XROrigin = null;

        void Start()
        {
            SceneManager.LoadScene("ConnectionScene", LoadSceneMode.Additive);

            EventManager.StartListening(EventNames.QuitApplication, QuitApplication);

            EventManager.StartListening(EventNames.EnterConnectionScene, LoadConnectionScene);
            EventManager.StartListening(EventNames.QuitConnectionScene, UnloadConnectionScene);

            EventManager.StartListening(EventNames.EnterMirrorScene, LoadMirrorScene);
            EventManager.StartListening(EventNames.QuitMirrorScene, UnloadMirrorScene);

            EventManager.StartListening(EventNames.EnterTeleoperationScene, LoadTeleoperationScene);
            EventManager.StartListening(EventNames.QuitTeleoperationScene, UnloadTeleoperationScene);

            EventManager.StartListening(EventNames.ShowXRay, ShowXRay);
            EventManager.StartListening(EventNames.HideXRay, HideXRay);
        }

        void QuitApplication()
        {
            Debug.Log("Exiting app");
            Application.Quit();
        }

        private void LoadConnectionScene()
        {
            Debug.Log("Loading Connection Scene");
            ground.SetActive(true);
            userInput.SetActive(false);
            userTracker.SetActive(false);
            SceneManager.LoadScene("ConnectionScene", LoadSceneMode.Additive);
        }

        private void UnloadConnectionScene()
        {
            SceneManager.UnloadSceneAsync("ConnectionScene");
            StartCoroutine(LoadRobotDataScene());
        }

        IEnumerator LoadRobotDataScene()
        {
            userTracker.SetActive(true);

            SceneManager.LoadScene("RobotDataScene", LoadSceneMode.Additive);
            yield return null;
            EventManager.TriggerEvent(EventNames.RobotDataSceneLoaded);

            userInput.SetActive(true);
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
            ground.SetActive(false);
        }

        private void LoadTeleoperationScene()
        {
            StartCoroutine(LoadTeleoperationRoom());
        }

        IEnumerator LoadTeleoperationRoom()
        {
            SceneManager.LoadScene("TeleoperationScene", LoadSceneMode.Additive);
            yield return null;
            EventManager.TriggerEvent(EventNames.TeleoperationSceneLoaded);
        }

        private void UnloadTeleoperationScene()
        {
            SceneManager.UnloadSceneAsync("TeleoperationScene");
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
